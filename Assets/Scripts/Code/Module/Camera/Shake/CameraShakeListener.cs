using TaoTie;
using UnityEngine;

namespace Cinemachine
{
    public class CameraShakeListener : CinemachineExtension
    {
        /// <summary>
        /// Gain to apply to the Impulse signal.
        /// </summary>
        [Tooltip("Gain to apply to the Impulse signal.  1 is normal strength.  Setting this to 0 completely mutes the signal.")]
        public float m_Gain = 1;

        public bool ScreenSpace = true;

        private Matrix4x4 shakeMatrix = new Matrix4x4();

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage == CinemachineCore.Stage.Aim)
            {
                Vector3 shakePos = Vector3.zero;
                Quaternion shakeRot = Quaternion.identity;
                if (CameraShakeManager.Instance.GetShake(out shakePos, out shakeRot))
                {
                    if (ScreenSpace)
                    {
                        shakeMatrix.SetTRS(Vector3.zero, state.FinalOrientation, Vector3.one);
                        shakePos = shakeMatrix.MultiplyPoint(shakePos * m_Gain);
                    }
                    state.PositionCorrection += shakePos;
                    shakeRot = Quaternion.SlerpUnclamped(Quaternion.identity, shakeRot, -m_Gain);
                    state.OrientationCorrection *= shakeRot;
                }
            }
        }
    }
}
