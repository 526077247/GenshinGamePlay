#if ENABLE_HOOK_TEST_CASE
#if UNITY_EDITOR
/*
 * �����޸�SceneView����������ƶ�������ʵ�֣�Ĭ���� Mathf.Pow(1.8f, deltaTime)��
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.AnimatedValues;
using System.Runtime.CompilerServices;

namespace MonoHook.Test
{
    //[InitializeOnLoad] // ȡ������ע�ͼ���Ч������ʹ��unity2020, ��Ϊ�м�������������unity2019��һ�£��˴���δ�����ݣ�
    public static class SceneViewMoveFunc_HookTest
    {
        #region ���䶨��ԭ���ֶ�
        private static Vector3 s_Motion
        {
            get => (Vector3)_fi_s_Motion.GetValue(null);
            set => _fi_s_Motion.SetValue(null, value);
        }

        private static bool s_Moving
        {
            get => (bool)_fi_s_Moving.GetValue(null);
            set => _fi_s_Moving.SetValue(null, value);
        }

        private static float s_FlySpeedTarget
        {
            get => (float)_fi_s_FlySpeedTarget.GetValue(null);
            set => _fi_s_FlySpeedTarget.SetValue(null, value);
        }

        /// <summary>
        /// ������ÿֻ֡�ܵ���һ�Σ���Ϊ�ڲ��ǵ��õ�Timeer.Update��
        /// </summary>
        private static float s_deltaTime
        {
            get => (float)_mi_deltaTime.Invoke(null, null);
        }

        private static SceneView s_CurrentSceneView
        {
            // �˱����뵱ǰcontext�йأ����ÿ��ʹ�ö����뼴ʱ��ȡ
            get => _fi_s_CurrentSceneView.GetValue(null) as SceneView;
        }

        private static Type _sceneViewMotionType;
        // �⼸���ֶ���ֵ���ͣ���Ϊ����ֱ�ӻ�ȡ�������ã�ÿ�ζ���Ҫʹ�÷����ȡ��������
        private static FieldInfo _fi_s_Motion;
        private static FieldInfo _fi_s_Moving;
        private static FieldInfo _fi_s_FlySpeedTarget;
        private static FieldInfo _fi_s_CurrentSceneView;
        private static MethodInfo _mi_deltaTime;

        private static AnimVector3 s_FlySpeed;

        private const float k_FlySpeed = 9f;
        private const float k_FlySpeedAcceleration = 1.8f;
        #endregion

        private static MethodHook _hook;

        /// <summary>
        /// �Զ���SceneView�����ƶ�����
        /// </summary>
        /// <returns></returns>
        private static float CustomAccMoveFunction(float deltaTime)
        {
            float FlySpeedTarget = s_FlySpeedTarget;
            float speed = (FlySpeedTarget < Mathf.Epsilon) ? k_FlySpeed : (FlySpeedTarget * Mathf.Pow(k_FlySpeedAcceleration, deltaTime));

            return speed;
        }

        static SceneViewMoveFunc_HookTest()
        {
            if (_hook == null)
            {
                _sceneViewMotionType = typeof(BuildPipeline).Assembly.GetType("UnityEditor.SceneViewMotion");

                // ����ԭ���ֶκ�����
                {
                    _fi_s_Motion = _sceneViewMotionType.GetField("s_Motion", BindingFlags.Static | BindingFlags.NonPublic);
                    _fi_s_Moving = _sceneViewMotionType.GetField("s_Moving", BindingFlags.Static | BindingFlags.NonPublic);
                    _fi_s_FlySpeedTarget = _sceneViewMotionType.GetField("s_FlySpeedTarget", BindingFlags.Static | BindingFlags.NonPublic);
                    _fi_s_CurrentSceneView = _sceneViewMotionType.GetField("s_CurrentSceneView", BindingFlags.Static | BindingFlags.NonPublic);

                    s_FlySpeed = _sceneViewMotionType.GetField("s_FlySpeed", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as AnimVector3;

                    _mi_deltaTime = typeof(BuildPipeline).Assembly.GetType("UnityEditor.CameraFlyModeContext")
                        .GetProperty("deltaTime", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
                }


                MethodInfo miTarget = _sceneViewMotionType.GetMethod("GetMovementDirection", BindingFlags.Static | BindingFlags.NonPublic);

                MethodInfo miReplacement = new Func<Vector3>(GetMovementDirectionNew).Method;
                MethodInfo miProxy = new Func<Vector3>(GetMovementDirectionProxy).Method;

                _hook = new MethodHook(miTarget, miReplacement, miProxy);
                _hook.Install();

                Debug.Log("���ض���SceneView������ƶ��ٶ�");
            }
        }

        /// <summary>
        /// ��дԭ�е��������ƶ��߼�(�˷���Ҳ���Ա���ȫ�޸�)
        /// </summary>
        /// <returns></returns>
        private static Vector3 GetMovementDirectionNew()
        {
            //return GetMovementDirectionProxy();

            s_Moving = s_Motion.sqrMagnitude > 0f;
            var _CurrentSceneView = s_CurrentSceneView; // ��������Ա����η������
            float speed = _CurrentSceneView.cameraSettings.speed;
            float deltaTime = s_deltaTime;              // s_deltaTime ���ɱ���η���
            if (Event.current.shift)
            {
                speed *= 5f;
            }
            if (s_Moving)
            {
                if (_CurrentSceneView.cameraSettings.accelerationEnabled)
                {
                    s_FlySpeedTarget = CustomAccMoveFunction(deltaTime); // �Զ�������ƶ�����
                }
                else
                {
                    s_FlySpeedTarget = k_FlySpeed;
                }
            }
            else
            {
                s_FlySpeedTarget = 0f;
            }
            if (_CurrentSceneView.cameraSettings.easingEnabled)
            {
                s_FlySpeed.speed = 1f / _CurrentSceneView.cameraSettings.easingDuration;
                s_FlySpeed.target = (s_Motion.normalized * s_FlySpeedTarget) * speed;
            }
            else
            {
                s_FlySpeed.value = (s_Motion.normalized * s_FlySpeedTarget) * speed;
            }
            return s_FlySpeed.value * deltaTime;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static Vector3 GetMovementDirectionProxy()
        {
            // dummy
            return Vector3.zero;
        }


    }
}
#endif
#endif