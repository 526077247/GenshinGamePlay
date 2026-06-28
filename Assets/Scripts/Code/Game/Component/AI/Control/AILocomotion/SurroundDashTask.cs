using UnityEngine;

namespace TaoTie
{
    public class SurroundDashTask: LocoBaseTask
    {
        private Unit anchor;
        private Vector3? anchorFixedPoint;
        private bool clockwise;
        private bool reverseMoveDir;
        private float radius;
        private float currentAngle;

        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamSurroundDash param)
        {
            base.Init(knowledge);
            anchor = param.Anchor;
            anchorFixedPoint = param.AnchorFixedPoint;
            speedLevel = param.SpeedLevel;
            clockwise = param.Clockwise;
            reverseMoveDir = param.ReverseMoveDir;
            radius = param.Radius;
            destination = GetAnchorPosition();
            currentAngle = 0f;
        }

        private Vector3 GetAnchorPosition()
        {
            if (anchorFixedPoint.HasValue)
                return anchorFixedPoint.Value;
            if (anchor != null)
                return anchor.Position;
            return destination;
        }

        public override void UpdateLoco(AILocomotionHandler handler, AITransform currentTransform, ref LocoTaskState state)
        {
            Vector3 anchorPos = GetAnchorPosition();
            if (anchorPos == destination && anchor == null)
            {
                Stopped = true;
                state = LocoTaskState.Finished;
                handler.UpdateMotionFlag(MotionFlag.Idle);
                return;
            }

            Vector3 toSelf = currentTransform.Position - anchorPos;
            toSelf.y = 0;
            if (toSelf.sqrMagnitude > 0.0001f)
            {
                currentAngle = Mathf.Atan2(toSelf.z, toSelf.x);
            }

            float angleStep = clockwise ? -0.5f : 0.5f;
            if (reverseMoveDir) angleStep = -angleStep;
            currentAngle += angleStep * GameTimerManager.Instance.GetDeltaTime() / 1000f * 2f;

            destination = anchorPos + new Vector3(
                Mathf.Cos(currentAngle) * radius,
                0,
                Mathf.Sin(currentAngle) * radius
            );

            handler.ForceLookAt();
            handler.UpdateMotionFlag(speedLevel);

            if (Stopped)
            {
                state = LocoTaskState.Finished;
                handler.UpdateMotionFlag(MotionFlag.Idle);
            }
        }

        public override void OnCloseTask(AILocomotionHandler handler)
        {
            base.OnCloseTask(handler);
            anchor = null;
            anchorFixedPoint = null;
            handler.UpdateMotionFlag(MotionFlag.Idle);
        }
    }
}
