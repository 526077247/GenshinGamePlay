#region Header
/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using TaoTie.LitJson.Extensions;

namespace TaoTie.LitJson
{
    internal struct PropertyMetadata
    {
        public MemberInfo Info;
        public bool IsField;
        public Type Type;
        public string Name;
    }


    internal struct ArrayMetadata
    {
        private Type element_type;
        private bool is_array;
        private bool is_list;
        private bool is_hashSet;

        public Type ElementType
        {
            get
            {
                if (element_type == null)
                    return typeof(JsonData);

                return element_type;
            }

            set { element_type = value; }
        }

        public bool IsArray
        {
            get { return is_array; }
            set { is_array = value; }
        }

        public bool IsList
        {
            get { return is_list; }
            set { is_list = value; }
        }
        
        public bool IsHashSet
        {
            get { return is_hashSet; }
            set { is_hashSet = value; }
        }
    }


    internal struct ObjectMetadata
    {
        private Type element_type;
        private bool is_dictionary;

        private IDictionary<string, PropertyMetadata> properties;


        public Type ElementType
        {
            get
            {
                if (element_type == null)
                    return typeof(JsonData);

                return element_type;
            }

            set { element_type = value; }
        }

        public bool IsDictionary
        {
            get { return is_dictionary; }
            set { is_dictionary = value; }
        }

        public IDictionary<string, PropertyMetadata> Properties
        {
            get { return properties; }
            set { properties = value; }
        }
    }


    internal delegate void ExporterFunc(object obj, JsonWriter writer);
    public delegate void ExporterFunc<T>(T obj, JsonWriter writer);

    internal delegate object ImporterFunc(object input);
    public delegate TValue ImporterFunc<TJson, TValue>(TJson input);

    public delegate IJsonWrapper WrapperFactory();


    public class JsonMapper
    {
        #region readonly
        private static Type floatType = typeof(float);
        private static Type doubleType = typeof(double);
        private static Type decimalType = typeof(decimal);
        private static Type DateTimeType = typeof(DateTime);
        private static Type charType = typeof(char);
        private static Type sbyteType = typeof(sbyte);
        private static Type byteType = typeof(byte);
        private static Type ushortType = typeof(ushort);
        private static Type shortType = typeof(short);
        private static Type uintType = typeof(uint);
        private static Type intType = typeof(int);
        private static Type ulongType = typeof(ulong);
        private static Type longType = typeof(long);
        private static Type stringType = typeof(string);
        #if UNITY_EDITOR
        private static Type UnityEngineObjectType = typeof(UnityEngine.Object);
        #endif
        #endregion
        #region Fields
        private static int max_nesting_depth;

        private static IFormatProvider datetime_format;

        private static IDictionary<Type, ExporterFunc> base_exporters_table;
        private static IDictionary<Type, ExporterFunc> custom_exporters_table;

        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> base_importers_table;
        private static IDictionary<Type,
                IDictionary<Type, ImporterFunc>> custom_importers_table;

        private static IDictionary<Type, ArrayMetadata> array_metadata;
        private static readonly object array_metadata_lock = new Object();

        private static IDictionary<Type,
                IDictionary<Type, MethodInfo>> conv_ops;
        private static readonly object conv_ops_lock = new Object();

        private static IDictionary<Type, ObjectMetadata> object_metadata;
        private static readonly object object_metadata_lock = new Object();

        private static IDictionary<Type,
                IList<PropertyMetadata>> type_properties;
        private static readonly object type_properties_lock = new Object();

        private static JsonWriter static_writer;
        private static readonly object static_writer_lock = new Object();
        #endregion


        #region Constructors
        static JsonMapper()
        {
            max_nesting_depth = 100;

            array_metadata = new Dictionary<Type, ArrayMetadata>();
            conv_ops = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
            object_metadata = new Dictionary<Type, ObjectMetadata>();
            type_properties = new Dictionary<Type,
                            IList<PropertyMetadata>>();

            static_writer = new JsonWriter();

            datetime_format = DateTimeFormatInfo.InvariantInfo;

            base_exporters_table = new Dictionary<Type, ExporterFunc>();
            custom_exporters_table = new Dictionary<Type, ExporterFunc>();

            base_importers_table = new Dictionary<Type,
                                 IDictionary<Type, ImporterFunc>>();
            custom_importers_table = new Dictionary<Type,
                                   IDictionary<Type, ImporterFunc>>();

            RegisterBaseExporters();
            RegisterBaseImporters();
        }
        #endregion


        #region Private Methods
        private static void AddArrayMetadata(Type type)
        {
            if (array_metadata.ContainsKey(type))
                return;

            ArrayMetadata data = new ArrayMetadata();

            data.IsArray = type.IsArray;

            if (type.GetInterface("System.Collections.IList") != null)
                data.IsList = true;

            if (type.GetInterface("System.Collections.Generic.ISet`1") != null)
            {
                data.IsHashSet = true;
                data.ElementType = type.GetGenericArguments()[0];
            }

            foreach (PropertyInfo p_info in type.GetProperties()) {
                if (p_info.Name != "Item")
                    continue;

                ParameterInfo[] parameters = p_info.GetIndexParameters();

                if (parameters.Length != 1)
                    continue;

                if (parameters[0].ParameterType == intType)
                    data.ElementType = p_info.PropertyType;
            }
            
            lock (array_metadata_lock)
            {
                try
                {
                    array_metadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddObjectMetadata(Type type)
        {
            if (object_metadata.ContainsKey(type))
                return;

            ObjectMetadata data = new ObjectMetadata();

            if (type.GetInterface("System.Collections.IDictionary") != null)
                data.IsDictionary = true;

            data.Properties = new Dictionary<string, PropertyMetadata>();
            foreach (PropertyInfo p_info in type.GetProperties())
            {
                if (p_info.Name == "Item")
                {
                    ParameterInfo[] parameters = p_info.GetIndexParameters();

                    if (parameters.Length != 1)
                        continue;

                    if (parameters[0].ParameterType == stringType)
                        data.ElementType = p_info.PropertyType;

                    continue;
                }

                PropertyMetadata p_data = new PropertyMetadata();
                p_data.Info = p_info;
                p_data.Type = p_info.PropertyType;
                var attr = p_info.GetCustomAttributes(JsonNode.Type,false);
                if (attr.Length > 0)
                    p_data.Name = (attr[0] as JsonNode)?.Name;
                else
                    p_data.Name = p_info.Name;
                data.Properties.Add(p_data.Name, p_data);
            }

            foreach (FieldInfo f_info in type.GetFields())
            {
                PropertyMetadata p_data = new PropertyMetadata();
                p_data.Info = f_info;
                p_data.IsField = true;
                p_data.Type = f_info.FieldType;
                var attr = f_info.GetCustomAttributes(JsonNode.Type,false);
                if (attr.Length > 0)
                    p_data.Name = (attr[0] as JsonNode)?.Name;
                else
                    p_data.Name = f_info.Name;
                data.Properties.Add(p_data.Name, p_data);
            }

            lock (object_metadata_lock)
            {
                try
                {
                    object_metadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddTypeProperties(Type type)
        {
            if (type_properties.ContainsKey(type))
                return;

            IList<PropertyMetadata> props = new List<PropertyMetadata>();

            foreach (PropertyInfo p_info in type.GetProperties())
            {
                if (p_info.Name == "Item")
                    continue;
                var attr = p_info.GetCustomAttributes(JsonIgnore.Type,false);
                if(attr.Length>0)continue;
                attr = p_info.GetCustomAttributes(typeof(NonSerializedAttribute),false);
                if(attr.Length>0)continue;
                PropertyMetadata p_data = new PropertyMetadata();
                p_data.Info = p_info;
                p_data.IsField = false;
                attr = p_info.GetCustomAttributes(JsonNode.Type,false);
                if (attr.Length > 0)
                    p_data.Name = (attr[0] as JsonNode)?.Name;
                else
                    p_data.Name = p_info.Name;
                props.Add(p_data);
            }

            foreach (FieldInfo f_info in type.GetFields())
            {
                var attr = f_info.GetCustomAttributes(JsonIgnore.Type,false);
                if(attr.Length>0)continue;
                attr = f_info.GetCustomAttributes(typeof(NonSerializedAttribute),false);
                if(attr.Length>0)continue;
                PropertyMetadata p_data = new PropertyMetadata();
                p_data.Info = f_info;
                p_data.IsField = true;
                attr = f_info.GetCustomAttributes(JsonNode.Type,false);
                if (attr.Length > 0)
                    p_data.Name = (attr[0] as JsonNode)?.Name;
                else
                    p_data.Name = f_info.Name;
                props.Add(p_data);
            }

            lock (type_properties_lock)
            {
                try
                {
                    type_properties.Add(type, props);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static MethodInfo GetConvOp(Type t1, Type t2)
        {
            lock (conv_ops_lock)
            {
                if (!conv_ops.ContainsKey(t1))
                    conv_ops.Add(t1, new Dictionary<Type, MethodInfo>());
            }

            if (conv_ops[t1].ContainsKey(t2))
                return conv_ops[t1][t2];

            MethodInfo op = t1.GetMethod(
                "op_Implicit", new Type[] { t2 });

            lock (conv_ops_lock)
            {
                try
                {
                    conv_ops[t1].Add(t2, op);
                }
                catch (ArgumentException)
                {
                    return conv_ops[t1][t2];
                }
            }

            return op;
        }

        private static object ReadValue(Type inst_type, JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd)
                return null;

            //ILRuntime doesn't support nullable valuetype
            Type underlying_type = inst_type;//Nullable.GetUnderlyingType(inst_type);
            Type value_type = inst_type;

            if (reader.Token == JsonToken.Null)
            {
                if (inst_type.IsClass || underlying_type != null)
                {
                    return null;
                }

                throw new JsonException(String.Format(
                            "Can't assign null to an instance of type {0}",
                            inst_type));
            }

            if (reader.Token == JsonToken.Double ||
                reader.Token == JsonToken.Int ||
                reader.Token == JsonToken.Long ||
                reader.Token == JsonToken.String ||
                reader.Token == JsonToken.Boolean)
            {

                Type json_type = reader.Value.GetType();
                
                if (value_type.IsAssignableFrom(json_type))
                    return reader.Value;
                // If there's a custom importer that fits, use it
                if (custom_importers_table.ContainsKey(json_type) &&
                    custom_importers_table[json_type].ContainsKey(
                        value_type))
                {

                    ImporterFunc importer =
                        custom_importers_table[json_type][value_type];

                    return importer(reader.Value);
                }

                // Maybe there's a base importer that works
                if (base_importers_table.ContainsKey(json_type) &&
                    base_importers_table[json_type].ContainsKey(
                        value_type))
                {

                    ImporterFunc importer =
                        base_importers_table[json_type][value_type];

                    return importer(reader.Value);
                }

                // Maybe it's an enum
                if (value_type.IsEnum)
                    return Enum.ToObject(value_type, reader.Value);

                // Try using an implicit conversion operator
                MethodInfo conv_op = GetConvOp(value_type, json_type);

                if (conv_op != null)
                    return conv_op.Invoke(null,
                                           new object[] { reader.Value });

                // No luck
                throw new JsonException(String.Format(
                        "Can't assign value '{0}' (type {1}) to type {2}",
                        reader.Value, json_type, inst_type));
            }

            object instance = null;

            if (reader.Token == JsonToken.ArrayStart)
            {

                AddArrayMetadata(inst_type);
                ArrayMetadata t_data = array_metadata[inst_type];

                if (!t_data.IsArray && !t_data.IsList && !t_data.IsHashSet)
                    throw new JsonException(String.Format(
                            "Type {0} can't act as an array",
                            inst_type));

                if (!t_data.IsHashSet)
                {
                    IList list;
                    Type elem_type;

                    if (!t_data.IsArray)
                    {
                        list = (IList)Activator.CreateInstance(inst_type);
                        elem_type = t_data.ElementType;
                    }
                    else
                    {
                        list = new ArrayList();
                        elem_type = inst_type.GetElementType();
                    }

                    while (true)
                    {
                        object item = ReadValue(elem_type, reader);
                        if (item == null && reader.Token == JsonToken.ArrayEnd)
                            break;
                    
                        list.Add(item);
                    }

                    if (t_data.IsArray)
                    {
                        int n = list.Count;
                        instance = Array.CreateInstance(elem_type, n);

                        for (int i = 0; i < n; i++)
                            ((Array)instance).SetValue(list[i], i);
                    }
                    else
                        instance = list;
                }
                else 
                {
                    instance = Activator.CreateInstance(inst_type);
                    var elem_type = t_data.ElementType;
                    var addMethod = inst_type.GetMethod("Add");
                    object[] param = new object[1];
                    while (true)
                    {
                        object item = ReadValue(elem_type, reader);
                        if (item == null && reader.Token == JsonToken.ArrayEnd)
                            break;
                        param[0] = item;
                        addMethod.Invoke(instance, param);
                    }
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                if (value_type.IsArray)
                {
                    var elem_type = value_type.GetElementType();
                    int[] length = null;
                    IList list = new ArrayList();
                    while (true)
                    {
                        reader.Read();
                        if (reader.Token == JsonToken.ObjectEnd)
                            break;
                        if ((string)reader.Value == "_Length")
                        {
                            length = (int[])ReadValue(typeof(int[]), reader);
                        }
                        else if ((string)reader.Value == "_Array")
                        {
                            reader.Read();//JsonToken.ArrayStart
                            if (reader.Token == JsonToken.ArrayStart)
                            {
                                while (true)
                                {
                                    object item = ReadValue(elem_type, reader);
                                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                                        break;
                    
                                    list.Add(item);
                                }
                            }
                        }
                        else
                        {
                            ReadSkip(reader);
                        }
                    }
                    if (length != null)
                    {
                        instance = Array.CreateInstance(elem_type, length);
                        int[] index = new int[length.Length];
                        for (int i = 0; i < list.Count; i++)
                        {
                            ((Array) instance).SetValue(list[i], index);
                            bool inRange = false;
                            for (int k = index.Length - 1; k >= 0; k--)
                            {
                                index[k]++;
                                if (index[k] == length[k])
                                {
                                    index[k] = 0;
                                }
                                else
                                {
                                    inRange = true;
                                    break;
                                }
                            }

                            if (!inRange)
                            {
                                break;
                            }
                        }
                    }
                    return instance;
                }
                ObjectMetadata t_data = default;
                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    string property = (string)reader.Value;

                    if (instance == null)
                    {
                        bool hasType = false;
                        if (property == "_t")
                        {
                            hasType = true;
                            string typeName = (string)ReadValue(stringType, reader);
#if UNITY_EDITOR
                            if (typeName == "_UnityEngineObject" &&
                                UnityEngineObjectType.IsAssignableFrom(value_type))
                            {
                                if (UnityEditor.EditorApplication.isPlaying)
                                {
                                    while (true)
                                    {
                                        reader.Read();
                                        if (reader.Token == JsonToken.ObjectEnd)
                                            break;
                                    }
                                    return null;
                                }
                                string guid = null;
                                string uType = null;
                                while (true)
                                {
                                    reader.Read();
                                    if (reader.Token == JsonToken.ObjectEnd)
                                        break;
                                    if ((string) reader.Value == "_ut")
                                    {
                                        uType = (string) ReadValue(stringType, reader);
                                    }
                                    else if ((string) reader.Value == "_GUID")
                                    {
                                        guid = (string) ReadValue(stringType, reader);
                                    }
                                    else
                                    {
                                        ReadSkip(reader);
                                    }
                                }

                                if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(uType))
                                {
                                    var ut = FindType(uType, value_type);
                                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath(path, ut);
                                }

                                return instance;
                            }
                           
#else
                            if (typeName == "_UnityEngineObject")
                            {
                                while (true)
                                {
                                    reader.Read();
                                    if (reader.Token == JsonToken.ObjectEnd)
                                        break;
                                }
                                return null;
                            }
#endif
                            else if (typeName != value_type.FullName)
                            {
                                var type = FindType(typeName, value_type);
                                if (type != null)
                                {
                                    if (!value_type.IsAssignableFrom(type))
                                    {
                                        throw new Exception($"类型不匹配！jsonType = {type} valueType = {value_type}");
                                    }
                                    value_type = type;
                                }
                                else
                                {
                                    throw new Exception($"丢失类型！{typeName}");
                                }
                            }
                        }
                        AddObjectMetadata(value_type);
                        t_data = object_metadata[value_type];
                
                        instance = Activator.CreateInstance(value_type);
                        if (hasType)
                        {
                            continue;
                        }
                    }
                   
                    
                    if (t_data.Properties.ContainsKey(property))
                    {
                        PropertyMetadata prop_data =
                            t_data.Properties[property];

                        if (prop_data.IsField)
                        {
                            ((FieldInfo)prop_data.Info).SetValue(
                                instance, ReadValue(prop_data.Type, reader));
                        }
                        else
                        {
                            PropertyInfo p_info =
                                (PropertyInfo)prop_data.Info;

                            if (p_info.CanWrite)
                                p_info.SetValue(
                                    instance,
                                    ReadValue(prop_data.Type, reader),
                                    null);
                            else
                                ReadValue(prop_data.Type, reader);
                        }

                    }
                    else
                    {
                        if (!t_data.IsDictionary)
                        {

                            if (!reader.SkipNonMembers)
                            {
                                throw new JsonException(String.Format(
                                        "The type {0} doesn't have the " +
                                        "property '{1}'",
                                        inst_type, property));
                            }
                            else
                            {
                                ReadSkip(reader);
                                continue;
                            }
                        }
                        //这里是为了 扩展可以让他支持 字典的key为int类型
                        if (t_data.IsDictionary)
                        {
                            var dicTypes = instance.GetType().GetGenericArguments();
                            var converter = System.ComponentModel.TypeDescriptor.GetConverter(dicTypes[0]);
                            if (converter != null)
                            {
                                var temp = converter.ConvertFromString(property);
                                t_data.ElementType = dicTypes[1];
                                ((IDictionary)instance).Add(temp, ReadValue(t_data.ElementType, reader));
                                continue;
                            }
                        }
                        
                        ((IDictionary)instance).Add(
                            property, ReadValue(
                                t_data.ElementType, reader));
                    }

                }

            }

            return instance;
        }
        
        private static IJsonWrapper ReadValue(WrapperFactory factory,
                                               JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
                return null;

            IJsonWrapper instance = factory();

            if (reader.Token == JsonToken.String)
            {
                instance.SetString((string)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double)
            {
                instance.SetDouble((double)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int)
            {
                instance.SetInt((int)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long)
            {
                instance.SetLong((long)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean)
            {
                instance.SetBoolean((bool)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart)
            {
                instance.SetJsonType(JsonType.Array);

                while (true)
                {
                    IJsonWrapper item = ReadValue(factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    ((IList)instance).Add(item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                instance.SetJsonType(JsonType.Object);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    string property = (string)reader.Value;

                    ((IDictionary)instance)[property] = ReadValue(
                        factory, reader);
                }

            }

            return instance;
        }

        private static void ReadSkip(JsonReader reader)
        {
            ToWrapper(
                delegate { return new JsonMockWrapper(); }, reader);
        }

        private static void RegisterBaseExporters()
        {
            base_exporters_table[byteType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((byte)obj));
                };

            base_exporters_table[charType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToString((char)obj));
                };

            base_exporters_table[DateTimeType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToString((DateTime)obj,
                                                    datetime_format));
                };

            base_exporters_table[decimalType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write((decimal)obj);
                };

            base_exporters_table[sbyteType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((sbyte)obj));
                };

            base_exporters_table[shortType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((short)obj));
                };

            base_exporters_table[ushortType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((ushort)obj));
                };

            base_exporters_table[uintType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToUInt64((uint)obj));
                };

            base_exporters_table[ulongType] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write((ulong)obj);
                };
        }

        private static void RegisterBaseImporters()
        {
            ImporterFunc importer;

            importer = delegate (object input)
            {
                return Convert.ToByte((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                byteType, importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt64((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                ulongType, importer);

            importer = delegate (object input)
            {
                return Convert.ToSByte((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                sbyteType, importer);

            importer = delegate (object input)
            {
                return Convert.ToInt16((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                shortType, importer);

            importer = delegate (object input)
            {
                return Convert.ToInt64((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                longType, importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt16((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                ushortType, importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt32((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                uintType, importer);

            importer = delegate (object input)
            {
                return Convert.ToSingle((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                floatType, importer);

            importer = delegate (object input)
            {
                return Convert.ToDouble((int)input);
            };
            RegisterImporter(base_importers_table, intType,
                doubleType, importer);

            importer = delegate (object input)
            {
                return Convert.ToDecimal((double)input);
            };
            RegisterImporter(base_importers_table, doubleType,
                decimalType, importer);


            importer = delegate (object input)
            {
                return Convert.ToUInt32((long)input);
            };
            RegisterImporter(base_importers_table, longType,
                uintType, importer);

            importer = delegate (object input)
            {
                return Convert.ToChar((string)input);
            };
            RegisterImporter(base_importers_table, stringType,
                charType, importer);

            importer = delegate (object input)
            {
                return Convert.ToDateTime((string)input, datetime_format);
            };
            RegisterImporter(base_importers_table, stringType,
                DateTimeType, importer);

            RegisterImporter<double, float>(input => Convert.ToSingle(input));
        }

        private static void RegisterImporter(
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type json_type, Type value_type, ImporterFunc importer)
        {
            if (!table.ContainsKey(json_type))
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());

            table[json_type][value_type] = importer;
        }

        private static void WriteValue(object obj, JsonWriter writer,
                                        bool writer_is_private,
                                        int depth)
        {
            if (depth > max_nesting_depth)
                throw new JsonException(
                    String.Format("Max allowed object depth reached while " +
                                   "trying to export from type {0}",
                                   obj.GetType()));

            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj is IJsonWrapper)
            {
                if (writer_is_private)
                    writer.TextWriter.Write(((IJsonWrapper)obj).ToJson());
                else
                    ((IJsonWrapper)obj).ToJson(writer);

                return;
            }

            if (obj is String)
            {
                writer.Write((string)obj);
                return;
            }

            if (obj is Single)
            {
                writer.Write((float)obj);
                return;
            }

            if (obj is Double)
            {
                writer.Write((double)obj);
                return;
            }

            if (obj is Int32)
            {
                writer.Write((int)obj);
                return;
            }

            if (obj is Boolean)
            {
                writer.Write((bool)obj);
                return;
            }

            if (obj is Int64)
            {
                writer.Write((long)obj);
                return;
            }

            if (obj is Array array)
            {
                if (array.Rank > 1)
                {
                    writer.WriteObjectStart();
                    writer.WritePropertyName("_Length");
                    writer.WriteArrayStart();
                    for (int i = 0; i < array.Rank; i++)
                    {
                        WriteValue(array.GetLength(i), writer, writer_is_private, depth + 1);
                    }
                    writer.WriteArrayEnd();
                    writer.WritePropertyName("_Array");
                    writer.WriteArrayStart();
                    foreach (object elem in array)
                        WriteValue(elem, writer, writer_is_private, depth + 1);
                    writer.WriteArrayEnd();
                    writer.WriteObjectEnd();
                }
                else
                {
                    writer.WriteArrayStart();
                    foreach (object elem in array)
                        WriteValue(elem, writer, writer_is_private, depth + 1);

                    writer.WriteArrayEnd();
                }
                return;
            }

            if (obj is IList)
            {
                writer.WriteArrayStart();
                foreach (object elem in (IList)obj)
                    WriteValue(elem, writer, writer_is_private, depth + 1);
                writer.WriteArrayEnd();

                return;
            }

            if (obj is IDictionary)
            {
                writer.WriteObjectStart();
                foreach (DictionaryEntry entry in (IDictionary)obj)
                {
                    writer.WritePropertyName(Convert.ToString(entry.Key));//(string) entry.Key);
                    WriteValue(entry.Value, writer, writer_is_private,
                                depth + 1);
                }
                writer.WriteObjectEnd();

                return;
            }

            Type obj_type= obj.GetType();
            
            if (obj_type.IsGenericType && obj_type.GetInterface("System.Collections.Generic.ISet`1") != null)
            {
                writer.WriteArrayStart();
                foreach (object elem in (IEnumerable)obj)
                    WriteValue(elem, writer, writer_is_private, depth + 1);
                writer.WriteArrayEnd();

                return;
            }
#if UNITY_EDITOR
            if (UnityEngineObjectType.IsAssignableFrom(obj_type) && 
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj as UnityEngine.Object, out string guid,out long localId))
            {
                writer.WriteObjectStart();
                writer.WritePropertyName("_t");
                writer.Write("_UnityEngineObject");
                writer.WritePropertyName("_ut");
                writer.Write(obj_type.FullName);
                writer.WritePropertyName("_GUID");
                writer.Write(guid);
                writer.WriteObjectEnd();
                return;
            }
#endif

            // See if there's a custom exporter for the object
            if (custom_exporters_table.ContainsKey(obj_type))
            {
                ExporterFunc exporter = custom_exporters_table[obj_type];
                exporter(obj, writer);

                return;
            }

            // If not, maybe there's a base exporter
            if (base_exporters_table.ContainsKey(obj_type))
            {
                ExporterFunc exporter = base_exporters_table[obj_type];
                exporter(obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum)
            {
                Type e_type = Enum.GetUnderlyingType(obj_type);

                if (e_type == longType)
                    writer.Write((long)obj);
                else if(e_type == ulongType)
                    writer.Write((ulong)obj);
                else if (e_type == byteType)
                    writer.Write((byte)obj);
                else if(e_type == sbyteType)
                    writer.Write((sbyte)obj);
                else if (e_type == shortType)
                    writer.Write((short)obj);
                else if(e_type == ushortType)
                    writer.Write((ushort)obj);
                else if(e_type == uintType)
                    writer.Write((uint)obj);
                else
                    writer.Write((int)obj);

                return;
            }

            // Okay, so it looks like the input should be exported as an
            // object
            AddTypeProperties(obj_type);
            IList<PropertyMetadata> props = type_properties[obj_type];

            writer.WriteObjectStart();
            writer.WritePropertyName("_t");
            writer.Write(obj_type.FullName);
            foreach (PropertyMetadata p_data in props)
            {
                if (p_data.IsField)
                {
                    var value = ((FieldInfo) p_data.Info).GetValue(obj);
                    if(value == default) continue;
                    writer.WritePropertyName(p_data.Name);
                    WriteValue(value, writer, writer_is_private, depth + 1);
                }
                else
                {
                    PropertyInfo p_info = (PropertyInfo)p_data.Info;

                    if (p_info.CanRead)
                    {
                        var value = p_info.GetValue(obj, null);
                        if(value==default) continue;
                        writer.WritePropertyName(p_data.Name);
                        WriteValue(value, writer, writer_is_private, depth + 1);
                    }
                }
            }
            writer.WriteObjectEnd();
        }
        #endregion


        public static string ToJson<T>(T obj)
        {
            if (obj == null) return "null";
            lock (static_writer_lock)
            {
                static_writer.Reset();

                WriteValue(obj, static_writer, true, 0);

                return static_writer.ToString();
            }
        }

        public static void ToJson(object obj, JsonWriter writer)
        {
            WriteValue(obj, writer, false, 0);
        }

        public static JsonData ToObject(JsonReader reader)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, reader);
        }

        public static JsonData ToObject(TextReader reader)
        {
            JsonReader json_reader = new JsonReader(reader);

            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json_reader);
        }

        public static JsonData ToObject(string json)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json);
        }

        public static T ToObject<T>(JsonReader reader)
        {
            return (T)ReadValue(typeof(T), reader);
        }

        public static T ToObject<T>(TextReader reader)
        {
            JsonReader json_reader = new JsonReader(reader);

            return (T)ReadValue(typeof(T), json_reader);
        }

        public static T ToObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length == 4 && json.ToLower() == "null") return default;
            JsonReader reader = new JsonReader(json);

            return (T)ReadValue(typeof(T), reader);
        }

        public static object ToObject(Type type, string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length == 4 && json.ToLower() == "null") return null;
            JsonReader reader = new JsonReader(json);
            return ReadValue(type, reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
                                              JsonReader reader)
        {
            return ReadValue(factory, reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
                                              string json)
        {
            JsonReader reader = new JsonReader(json);

            return ReadValue(factory, reader);
        }

        public static void RegisterExporter<T>(ExporterFunc<T> exporter)
        {
            ExporterFunc exporter_wrapper =
                delegate (object obj, JsonWriter writer)
                {
                    exporter((T)obj, writer);
                };

            custom_exporters_table[typeof(T)] = exporter_wrapper;
        }

        public static void RegisterImporter<TJson, TValue>(
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importer_wrapper =
                delegate (object input)
                {
                    return importer((TJson)input);
                };

            RegisterImporter(custom_importers_table, typeof(TJson),
                              typeof(TValue), importer_wrapper);
        }

        public static void UnregisterExporters()
        {
            custom_exporters_table.Clear();
        }

        public static void UnregisterImporters()
        {
            custom_importers_table.Clear();
        }
        
        #region 多态支持
        private static readonly Dictionary<string, Type> _temp = new Dictionary<string, Type>();
        public static Type FindType(string fullName,Type baseType)
        {
            if (_temp.TryGetValue(fullName, out var type))
            {
                return type;
            }

            Assembly assembly = null;
            if (baseType != null)
            {
                assembly = baseType.Assembly;
                type = assembly.GetType(fullName);
                if (type != null)
                {
                    _temp[fullName] = type;
                    return type;
                }
            }

            var ass = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < ass.Length; i++)
            {
                if (ass[i] != assembly)
                {
                    type = ass[i].GetType(fullName);
                    if (type != null)
                    {
                        _temp[fullName] = type;
                        return type;
                    }
                }
            }
            return null;
        }

        public static bool RedirectType(string fullName,Type type,bool overwrite = true)
        {
            if (!overwrite&&_temp.ContainsKey(fullName))
            {
                return false;
            }
            _temp[fullName] = type;
            return true;
        }

        public static Dictionary<string, Type> GetAll()
        {
            return _temp;
        }
        #endregion
    }
}
