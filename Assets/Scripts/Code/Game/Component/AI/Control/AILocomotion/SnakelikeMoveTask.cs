using UnityEngine;

namespace TaoTie
{
    public class SnakelikeMoveTask: GoToTask
    {
        private float elapsed;
        private const float SNAKE_FREQUENCY = 3f;
        private const float SNAKE_AMPLITUDE = 1.5f;

        public void Init(AIKnowledge knowledge, AILocomotionHandler.ParamGoTo param)
        {
            base.Init(knowledge, param);
            elapsed = 0f;
        }

        protected override void OnMovingToDestination(AILocomotionHandler handler, Vector3 currentPos, Vector3 target)
        {
            elapsed += GameTimerManager.Instance.GetDeltaTime() / 1000f;
            Vector3 moveDir = (target - currentPos).normalized;
            Vector3 perpendicular = new Vector3(-moveDir.z, 0, moveDir.x);
            float offset = Mathf.Sin(elapsed * SNAKE_FREQUENCY) * SNAKE_AMPLITUDE;
            destination = target + perpendicular * offset;
            handler.ForceLookAt();
            handler.UpdateMotionFlag(speedLevel);
            if (turnSpeed != 0)
            {
                handler.UpdateTurnSpeed(turnSpeed);
            }
        }
    }
}
