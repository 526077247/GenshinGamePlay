﻿using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace Slate
{

    ///<summary>Defines a parameter (property/field) wrapper that can be animated</summary>
    [System.Serializable]
    public class AnimatedParameter : IAnimatableData, ISerializationCallbackReceiver
    {

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            serializedData = JsonUtility.ToJson(data);
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _data = data;
            _parameterModel = parameterModel;
        }

        ///<summary>Used for storing the meta data</summary>
        [System.Serializable]
        class SerializationMetaData
        {
            public string parameterName;
            public string declaringTypeName;
            public string transformHierarchyPath;
            public ParameterType parameterType;

            public Type declaringType { get; private set; }
            public PropertyInfo property { get; private set; }
            public FieldInfo field { get; private set; }
            public Type animatedType { get; private set; }
            public void Deserialize() {
                declaringType = ReflectionTools.GetType(declaringTypeName);
                if ( declaringType != null ) {
                    property = ReflectionTools.GetRelativeMember(declaringType, parameterName) as PropertyInfo;
                    field = ReflectionTools.GetRelativeMember(declaringType, parameterName) as FieldInfo;
                    animatedType = property != null ? property.PropertyType : field != null ? field.FieldType : null;
                }
            }
        }

        ///<summary>The type of the parameter.</summary>
        public enum ParameterType
        {
            NotSet,
            Property,
            Field
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>This event is raised when a parameter is changed with argument being the IAnimatableData parameter.</summary>
        public static event System.Action<IAnimatableData> onParameterChanged;

        ///----------------------------------------------------------------------------------------------

        [SerializeField] private string _serializedData;
        [SerializeField] private bool _isDisabled;
        [SerializeField] private AnimationCurve[] _curves;
        [SerializeField] private string _scriptExpression;

        ///----------------------------------------------------------------------------------------------

        ///<summary>Is the parameter enabled and will be used?</summary>
        public bool enabled { get { return !_isDisabled; } }
        ///<summary>Set the parameter enabled/disabled</summary>
        public void SetEnabled(bool value, float time) {
            if ( enabled != value ) {
                RecordUndo();
                _isDisabled = !value;
                if ( time > 0 && snapshot != null ) {
                    if ( value == false ) { SetCurrentValue(snapshot); }
                    if ( value == true ) { Evaluate(time, 0); }
                }
                NotifyChange();
            }
        }

        //Serialized json data
        private string serializedData {
            get { return _serializedData; }
            set { _serializedData = value; }
        }

        //Serialization structure
        [System.NonSerialized]
        private SerializationMetaData _data;
        private SerializationMetaData data {
            get
            {
                if ( _data == null ) {
                    _data = JsonUtility.FromJson<SerializationMetaData>(serializedData);
                    _data.Deserialize();
                }
                return _data;
            }
        }

        //The current model used based on animated type
        [System.NonSerialized]
        private IAnimatedParameterModel _parameterModel;
        public IAnimatedParameterModel parameterModel {
            get
            {
                if ( _parameterModel == null ) {
                    Type modelType = null;
                    if ( parameterModelsMap.TryGetValue(animatedType, out modelType) ) {
                        _parameterModel = Activator.CreateInstance(modelType) as IAnimatedParameterModel;
                    }
                }
                return _parameterModel;
            }
        }


        ///<summary>The animation curves of the parameter</summary>
        public AnimationCurve[] curves {
            get { return _curves; }
            private set { _curves = value; }
        }

        ///<summary>The script expression used if any</summary>
        public string scriptExpression {
            get { return _scriptExpression; }
            set
            {
                if ( _scriptExpression != value ) {
                    RecordUndo();
                    _scriptExpression = value;
#if SLATE_USE_EXPRESSIONS
                    {
                        if ( !string.IsNullOrEmpty(value) ) {
                            CompileExpression();
                            return;
                        }
                        compiledExpression = null;
                        compileException = null;
                    }
#endif
                }
            }
        }

        ///<summary>Has active expression?</summary>
        public bool hasActiveExpression {
            get { return !string.IsNullOrEmpty(scriptExpression); }
        }


        ///<summary>Get the meta info from the deserialized data. These are basicaly shortcuts</summary>
        public string parameterName { get { return data.parameterName; } }
        public Type animatedType { get { return data.animatedType; } }
        public ParameterType parameterType { get { return data.parameterType; } }
        public string transformHierarchyPath { get { return data.transformHierarchyPath; } }
        public Type declaringType { get { return data.declaringType; } }
        public PropertyInfo property { get { return data.property; } }
        public FieldInfo field { get { return data.field; } }
        public bool isProperty { get { return parameterType == ParameterType.Property; } }
        //

        ///<summary>The IKeyable reference this parameters belongs to</summary>
        public IKeyable keyable { get; private set; }
        ///<summary>The snapshot value before evaluation</summary>
        private float[] snapshot { get; set; }
        ///<summary>The last evaluated value. Mostly used to check value changes</summary>
        private float[] lastEval { get; set; }
        ///<summary>Used to virtualy parent transform based parameters if not null</summary>
        private Transform virtualTransformParent { get; set; }

#if SLATE_USE_EXPRESSIONS
        ///<summary>The compiled expression if any</summary>
        public StagPoint.Eval.Expression compiledExpression { get; private set; }
        ///<summary>The compile exception if any</summary>
        public System.Exception compileException { get; private set; }
#endif

        ///<summary>The animation target root object (fetched from IKeyable reference)</summary>
        public object targetObject {
            get { return keyable != null ? keyable.animatedParametersTarget : null; }
        }

        ///<summary>AnimatedType to ModelType mapping</summary>
        private static Dictionary<Type, Type> parameterModelsMap;
        ///<summary>The types supported for animation</summary>
        public readonly static Type[] supportedTypes;

        [System.NonSerialized]
        private object _animatableAttribute; //Cache of possible [AnimatableParameter] attribute used for creating this parameter
        public AnimatableParameterAttribute animatableAttribute {
            get
            {
                if ( _animatableAttribute == null ) {
                    var m = GetMemberInfo();
                    if ( m == null ) { return null; }
                    var att = m.RTGetAttribute<AnimatableParameterAttribute>(true);
                    _animatableAttribute = att != null ? att : new object();
                }
                return _animatableAttribute as AnimatableParameterAttribute;
            }
        }

        ///<summary>External means that the parameter was not created by the use of [AnimatableParameter] attribute, but rather added manually.</summary>
        public bool isExternal {
            get { return animatableAttribute == null; }
        }

        ///<summary>Is all good to go?</summary>
        public bool isValid {
            get
            {
                if ( string.IsNullOrEmpty(serializedData) || data == null ) {
                    return false;
                }
                return isProperty ? property != null : field != null;
            }
        }

        ///<summary>Static init</summary>
        static AnimatedParameter() {
            parameterModelsMap = new Dictionary<Type, Type>();
            foreach ( var type in ReflectionTools.GetImplementationsOf(typeof(IAnimatedParameterModel)) ) {
                var decAtt = type.RTGetAttribute<DecoratorAttribute>(false);
                if ( decAtt != null ) { parameterModelsMap[decAtt.targetType] = type; } else { Debug.LogError(string.Format("{0} is missing the Decorator attribute", type.Name)); }
            }
            supportedTypes = parameterModelsMap.Keys.ToArray();
        }

        ///<summary>Creates a new animated parameter out of a member info that optionaly exists on a component in child transform of root transform.</summary>
        public AnimatedParameter(IKeyable keyable, Type type, string memberPath, string transformPath) {
            this.keyable = keyable;
            var member = ReflectionTools.GetRelativeMember(type, memberPath);
            if ( member is PropertyInfo ) {
                ConstructWithProperty((PropertyInfo)member, type, memberPath, transformPath);
                return;
            }
            if ( member is FieldInfo ) {
                ConstructWithField((FieldInfo)member, type, memberPath, transformPath);
                return;
            }
            Debug.LogError("MemberInfo provided is neither Property, nor Field, or can't be found.");
        }

        //construct with FieldInfo
        void ConstructWithField(FieldInfo targetField, Type type, string memberPath, string transformPath) {

            if ( !supportedTypes.Contains(targetField.FieldType) ) {
                Debug.LogError(string.Format("Type '{0}' is not supported for animation", targetField.FieldType));
                return;
            }

            if ( targetField.IsStatic ) {
                Debug.LogError("Static Fields are not supported");
                return;
            }

            var newData = new SerializationMetaData();
            newData.parameterType = ParameterType.Field;
            newData.parameterName = memberPath;
            newData.declaringTypeName = type.FullName;
            newData.transformHierarchyPath = transformPath;

            serializedData = JsonUtility.ToJson(newData);
            InitializeCurves();
        }

        //construct with PropertyInfo
        void ConstructWithProperty(PropertyInfo targetProperty, Type type, string memberPath, string transformPath) {

            if ( !supportedTypes.Contains(targetProperty.PropertyType) ) {
                Debug.LogError(string.Format("Type '{0}' is not supported for animation", targetProperty.PropertyType));
                return;
            }

            if ( !targetProperty.CanRead || !targetProperty.CanWrite ) {
                Debug.LogError("Animated Property must be able to both read and write");
                return;
            }

            if ( targetProperty.RTGetGetMethod().IsStatic ) {
                Debug.LogError("Static Properties are not supported");
                return;
            }

            var newData = new SerializationMetaData();
            newData.parameterType = ParameterType.Property;
            newData.parameterName = memberPath;
            newData.declaringTypeName = type.FullName;
            newData.transformHierarchyPath = transformPath;

            serializedData = JsonUtility.ToJson(newData);
            InitializeCurves();
        }

        ///<summary>The PropertyInfo or FieldInfo used</summary>
        public MemberInfo GetMemberInfo() {
            if ( isValid ) {
                return isProperty ? (MemberInfo)property : (MemberInfo)field;
            }
            return null;
        }

        ///<summary>The curves used</summary>
        public AnimationCurve[] GetCurves() {
            return curves;
        }

        ///<summary>Returns true if this animated parameter points to the same property/field as the provided one does.</summary>
        public bool CompareTo(AnimatedParameter other) {
            return parameterName == other.parameterName && declaringType == other.declaringType && transformHierarchyPath == other.transformHierarchyPath;
        }

        //Initialize the curves
        void InitializeCurves() {
            curves = new AnimationCurve[parameterModel.RequiredCurvesCount()];
            for ( var i = 0; i < curves.Length; i++ ) {
                curves[i] = new AnimationCurve();
            }
        }

        ///<summary>Validate the parameter within the context of provided keyable reference</summary>
        public void Validate(IKeyable keyable) {
            this.keyable = keyable;
#if SLATE_USE_EXPRESSIONS
            {
                CompileExpression();
            }
#endif
        }

        ///<summary>Set the virtual parent for transform based parameters</summary>
        public void SetVirtualTransformParent(Transform virtualTransformParent) {
            this.virtualTransformParent = virtualTransformParent;
        }

        ///<summary>Store a snapshot for restoring later.</summary>
        public void SetSnapshot() {

            if ( !isValid ) {
                return;
            }

#if UNITY_EDITOR
            {
                if ( !Application.isPlaying && Prefs.autoKey && Prefs.autoFirstKey && !HasAnyKey() ) {
                    TryKeyIdentity(0);
                }
            }
#endif

            //always save snapshot even if disabled for in case it get's enabled
            snapshot = GetCurrentValueAsFloats();
            lastEval = snapshot.ToArray(); //a copy since lastEval is modified within evaluate
        }

        ///<summary>Try to add new key if the value has changed</summary>
        public bool TryAutoKey(float time) {

            if ( hasActiveExpression ) {
                return false;
            }

            if ( !isValid || !enabled ) {
                return false;
            }

            if ( !HasAnyKey() && !isExternal ) {
                return false;
            }

            if ( HasChanged() ) {
                SetKeyCurrent(time);
                return true;
            }

            return false;
        }

        ///<summary>Evaluate animation and sets the target property/field value</summary>
        public void Evaluate(float time, float previousTime, float weight = 1f) {

            if ( !enabled || targetObject == null || targetObject.Equals(null) ) {
                return;
            }

            if ( !Evaluate_2_Expression(time, previousTime, weight) ) {
                Evaluate_1_Curves(time, previousTime, weight);
            }
        }

        ///<summary>Evaluate the curve keyframes if any</summary>
        void Evaluate_1_Curves(float time, float previousTime, float weight = 1f) {

            if ( !HasAnyKey() ) {
                return;
            }

#if UNITY_EDITOR
            {
                if ( !Application.isPlaying ) {
                    if ( time == previousTime && HasChanged() ) {
                        if ( !Prefs.autoKey ) { //in case of no Auto-Key, store changed params
                            var _value = GetCurrentValueAsObject();
                            Action restore = () => { SetCurrentValue(_value); };
                            Action commit = () => { TryAutoKey(time); };
                            var paramCallbacks = new CutsceneUtility.ChangedParameterCallbacks(restore, commit);
                            CutsceneUtility.changedParameterCallbacks[this] = paramCallbacks;
                        }
                        return; //auto-key or not, return if the parameter changed
                    }
                    if ( !Prefs.autoKey ) {
                        CutsceneUtility.changedParameterCallbacks.Remove(this);
                    }
                }
            }
#endif


            //set lastEval by lerp snapshot to curves
            for ( var i = 0; i < curves.Length; i++ ) {
                lastEval[i] = Mathf.LerpUnclamped(snapshot[i], curves[i].Evaluate(time), weight);
            }

            //set object value
            SetCurrentValue(lastEval);
        }

        //Evaluate the expression if any
        bool Evaluate_2_Expression(float time, float previousTime, float weight = 1f) {
#if SLATE_USE_EXPRESSIONS
            {
                if ( compiledExpression == null ) {
                    return false;
                }

                // SetCurrentValue(snapshot);
                var expValue = compiledExpression.Execute();
                if ( expValue is object[] ) {
                    lastEval = ( expValue as object[] ).Cast<float>().ToArray();
                } else {
                    lastEval = parameterModel.ConvertToFloats(expValue);
                }
                SetCurrentValue(lastEval);
                return true;
            }
#else
            return false;
#endif
        }

        ///<summary>Restore the snapshot on the target</summary>
        public void RestoreSnapshot() {

            if ( !isValid || !enabled ) {
                return;
            }

            if ( snapshot != null && isExternal ) {
                SetCurrentValue(snapshot);
            }

            lastEval = null;
            snapshot = null;
        }

#if SLATE_USE_EXPRESSIONS
        private StagPoint.Eval.Environment env;
        public StagPoint.Eval.Environment GetExpressionEnvironment() {
            if ( env != null ) { return env; }
            env = keyable.GetExpressionEnvironment().Push();
            Slate.Expressions.ExpressionsUtility.Wrap(this, env);
            return env;
        }

        ///<summary>Compile the expression if any</summary>
        void CompileExpression() {
            env = null;
            Exception exception = null;
            StagPoint.Eval.Expression expression = null;
            if ( hasActiveExpression ) {
                Slate.Expressions.ExpressionsUtility.CompileExpression(scriptExpression, GetExpressionEnvironment(), this, out exception, out expression);
                compileException = exception;
                compiledExpression = expression;
            }
        }
#endif


        ///<summary>Returns the curves evaluated value at time</summary>
        public object GetEvalValue(float time) {
            var floats = new float[curves.Length];
            for ( var i = 0; i < curves.Length; i++ ) {
                floats[i] = curves[i].Evaluate(time);
            }
            return parameterModel.ConvertToObject(floats);
        }

        ///<summary>Resolves the final object within which the property/field is declared and thus animated.</summary>
        [System.NonSerialized]
        private object _resolvedMemberObject = null;
        public object ResolvedMemberObject() {

            if ( targetObject == null || targetObject.Equals(null) ) {
                return null;
            }

            //if snapshot not null, means at least one time this has been evaluated
            if ( _resolvedMemberObject != null && !_resolvedMemberObject.Equals(null) && snapshot != null ) {
                return _resolvedMemberObject;
            }

            var result = targetObject;
            //if is gameobject, resolve path and final component type
            if ( targetObject is GameObject ) {
                var leafGameObject = (GameObject)targetObject;
                if ( !string.IsNullOrEmpty(transformHierarchyPath) ) {
                    var leafTransform = UnityObjectUtility.ResolveTransformPath(leafGameObject.transform, transformHierarchyPath);
                    leafGameObject = leafTransform != null ? leafTransform.gameObject : null;
                }
                result = leafGameObject != null ? leafGameObject.GetComponent(declaringType) : null;
            }

            result = ReflectionTools.GetRelativeMemberParent(result, parameterName);
            return _resolvedMemberObject = result;
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Gets the current raw property value from target boxed to object</summary>
        public object GetCurrentValueAsObject() { return parameterModel.ConvertToObject(GetCurrentValueAsFloats()); }

        ///<summary>Gets the current raw property value from target as floats[]</summary>
        float[] GetCurrentValueAsFloats() {

            if ( !isValid ) {
                return null;
            }

            var obj = ResolvedMemberObject();
            if ( obj == null || obj.Equals(null) ) {
                return null;
            }

            if ( obj is Transform && animatedType == typeof(Vector3) ) { //special treat
                var transform = obj as Transform;
                var vector = default(Vector3);
                if ( parameterName == "localPosition" ) {
                    vector = transform.localPosition;
                    if ( virtualTransformParent != null && transform.parent == null ) {
                        vector = virtualTransformParent.InverseTransformPoint(vector);
                    }
                }

                if ( parameterName == "localEulerAngles" ) {
                    vector = transform.GetLocalEulerAngles();
                    if ( virtualTransformParent != null && transform.parent == null ) {
                        vector -= virtualTransformParent.GetLocalEulerAngles();
                        // vector = ( Quaternion.Inverse(virtualTransformParent.rotation) * Quaternion.Euler(vector) ).eulerAngles;
                    }
                }

                if ( parameterName == "localScale" ) {
                    vector = transform.localScale;
                }

                return new float[3] { vector.x, vector.y, vector.z };
            }

            return parameterModel.GetDirect(obj, GetMemberInfo());
        }

        ///<summary>Sets the current value to the object, given a boxed value</summary>
        public void SetCurrentValue(object value) { SetCurrentValue(parameterModel.ConvertToFloats(value)); }

        ///<summary>Sets the current value to the object, given a float[] that represents that value. This avoids boxing and thus allocations.</summary>
        void SetCurrentValue(float[] floats) {

            if ( !isValid ) {
                return;
            }

            var obj = ResolvedMemberObject();
            if ( obj == null || obj.Equals(null) ) {
                return;
            }

            if ( obj is Transform && animatedType == typeof(Vector3) ) { //special treat
                var transform = obj as Transform;
                var vector = new Vector3(floats[0], floats[1], floats[2]);
                if ( parameterName == "localPosition" ) {
                    if ( virtualTransformParent != null && transform.parent == null ) {
                        vector = virtualTransformParent.TransformPoint(vector);
                    }
                    transform.localPosition = vector;
                    return;
                }

                if ( parameterName == "localEulerAngles" ) {
                    if ( virtualTransformParent != null && transform.parent == null ) {
                        vector += virtualTransformParent.GetLocalEulerAngles();
                        // vector = (virtualTransformParent.rotation * Quaternion.Euler(vector)).eulerAngles;
                    }
                    transform.SetLocalEulerAngles(vector);
                    return;
                }

                if ( parameterName == "localScale" ) {
                    transform.localScale = vector;
                    return;
                }
            }

            parameterModel.SetDirect(obj, GetMemberInfo(), floats);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Has the property on target changed since the last evaluation?</summary>
        public bool HasChanged() {
            float[] a = lastEval;
            if ( a == null ) { return false; }
            float[] b = GetCurrentValueAsFloats();
            if ( b == null ) { return false; }
            if ( a.Length == b.Length ) {
                for ( var i = 0; i < a.Length; i++ ) {
                    if ( Mathf.Abs(a[i] - b[i]) > 0.001f ) {
                        return true;
                    }
                }
            }
            return false;
        }

        ///<summary>Are there any keys at all?</summary>
        public bool HasAnyKey() {
            return CurveUtility.HasAnyKey(curves);
        }

        ///<summary>Has any key at time?</summary>
        public bool HasKey(float time) {
            return CurveUtility.HasKey(time, curves);
        }

        ///<summary>Returns the key time after time, or first key if time is last key time.</summary>
        public float GetKeyNext(float time) {
            return CurveUtility.GetKeyNext(time, curves);
        }

        ///<summary>Returns the key time before time, or last key if time is first key time.</summary>
        public float GetKeyPrevious(float time) {
            return CurveUtility.GetKeyPrevious(time, curves);
        }

        ///----------------------------------------------------------------------------------------------

        ///<summary>Try add key at time, with identity value either from existing curves or in case of no curves, from current property value.</summary>
        public bool TryKeyIdentity(float time) {
#if UNITY_EDITOR
            if ( !HasAnyKey() ) {
                SetKeyCurrent(time);
                return true;
            }

            RecordUndo();
            var keyAdded = false;
            var mode = parameterModel.ForceStepMode() ? TangentMode.Constant : Prefs.defaultTangentMode;
            for ( var i = 0; i < curves.Length; i++ ) {
                if ( CurveUtility.AddKey(curves[i], time, curves[i].Evaluate(time), mode) ) {
                    keyAdded = true;
                }
            }
            NotifyChange();
            return keyAdded;
#else
			return false;
#endif
        }

        ///<summary>Sets a key on target at time with it's current property value</summary>
        public void SetKeyCurrent(float time) {
#if UNITY_EDITOR
            RecordUndo();
            var floats = GetCurrentValueAsFloats();
            var mode = parameterModel.ForceStepMode() ? TangentMode.Constant : Prefs.defaultTangentMode;
            if ( enabled ) {
                for ( var i = 0; i < curves.Length; i++ ) {
                    CurveUtility.AddKey(curves[i], time, floats[i], mode);
                }
            }
            lastEval = floats;
            NotifyChange();
#endif
        }

        ///<summary>Remove keys at time</summary>
        public void RemoveKey(float time) {
#if UNITY_EDITOR
            RecordUndo();
            CurveUtility.RemoveKeys(time, curves);
            NotifyChange();
#endif
        }

        ///<summary>Set curves PreWrap</summary>
        public void SetPreWrapMode(WrapMode mode) {
            RecordUndo();
            foreach ( var curve in curves ) {
                curve.preWrapMode = mode;
            }
            NotifyChange();
        }

        ///<summary>Set curves PostWrap</summary>
        public void SetPostWrapMode(WrapMode mode) {
            RecordUndo();
            foreach ( var curve in curves ) {
                curve.postWrapMode = mode;
            }
            NotifyChange();
        }

        ///<summary>Offset all curves by value.</summary>
        public void OffsetValue(object deltaValue) {
            RecordUndo();
            var decomposed = parameterModel.ConvertToFloats(deltaValue);
            for ( var i = 0; i < curves.Length; i++ ) {
                curves[i].OffsetCurveValue(decomposed[i]);
            }
            NotifyChange();
        }

        ///<summary>Reset curves</summary>
        public void Reset() {
            RecordUndo();
            scriptExpression = null;
            foreach ( var curve in curves ) {
                curve.keys = new Keyframe[0];
                curve.preWrapMode = WrapMode.ClampForever;
                curve.postWrapMode = WrapMode.ClampForever;
            }
            NotifyChange();
        }

        ///<summary>Changes the type of the parameter (field/property) while keeping the data</summary>
        public void ChangeMemberType(ParameterType newType) {
            if ( newType != this.parameterType ) {
                data.parameterType = newType;
                serializedData = JsonUtility.ToJson(data);
            }
        }

        ///<summary>A friendly label including all info</summary>
        public override string ToString() { return FriendlyName(); }
        ///<summary>A friendly label including all info</summary>
        public string FriendlyName() {
            if ( string.IsNullOrEmpty(serializedData) ) { return "NOT SET!"; }

            var name = parameterName;

            if ( animatableAttribute != null && !string.IsNullOrEmpty(animatableAttribute.customName) ) {
                name = animatableAttribute.customName;
            }

            if ( !isValid ) { return string.Format("*{0}*", name); }
            if ( !isExternal ) { name = name.Substring(name.LastIndexOf('.') + 1); }
            if ( name == "localPosition" ) name = "Position";
            if ( name == "localEulerAngles" ) name = "Rotation";
            if ( name == "localScale" ) name = "Scale";
            if ( isExternal ) { name = string.Format("{0} <i>({1})</i>", name, declaringType.Name); }
            if ( !enabled ) { name += " <i>(Disabled)</i>"; }
            name = name.SplitCamelCase();
            return string.IsNullOrEmpty(transformHierarchyPath) ? name : transformHierarchyPath + "/" + name;
        }

        ///<summary>Returns formated text of value at time</summary>
        public string GetKeyLabel(float time) {
            return parameterModel.GetKeyLabel(curves.Select(c => c.Evaluate(time)).ToArray());
        }

        //Helper function
        void RecordUndo() {
#if UNITY_EDITOR
            {
                UnityEngine.Object obj = null;
                obj = keyable as UnityEngine.Object;
                if ( obj != null ) { UnityEditor.Undo.RecordObject(obj, "Parameter Change"); }
                obj = ResolvedMemberObject() as UnityEngine.Object;
                if ( obj != null ) { UnityEditor.Undo.RecordObject(obj, "Parameter Change"); }
            }
#endif
        }

        ///<summary>Raise the change event</summary>
        void NotifyChange() {
            if ( onParameterChanged != null ) {
                onParameterChanged(this);
            }
        }
    }
}