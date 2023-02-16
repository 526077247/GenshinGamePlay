using UnityEngine;

namespace TaoTie
{
    public class LocoBaseTask
    {
        public bool delayStopping;
        protected Vector3 _destination;
        protected AIMoveSpeedLevel _speedLevel;
        protected float _startTick;
        protected ObstacleHandling _obstacleHandling;
        protected AIKnowledge _aiKnowledge;
        protected const float CHECKFAIL_PRE_TIME = 1.5f;
        protected static readonly FailDetectionSystem[] CHECKFAIL;
        protected Vector3? _prevPos;
        protected AITimer _moveFailTimer;
        protected float _distanceMoved;
        protected Vector3? _moveFailStartPos;
        protected bool _stopped;
        protected DirectionLock _directionLock;


        public bool stopped { get; }


        public enum ObstacleHandling
        {
            KeepMoving = 0,
            Stop = 1,
            StopOnlyByPathEnd = 2,
            Teleport = 3
        }

        protected struct FailDetectionSystem
        {

            public float CHECKFAIL_SPEED_RUNSTOP;
            public float CHECKFAIL_SPEED_WALKSTOP;
            public float CHECKFAIL_TIME; 

            public FailDetectionSystem(float time, float walk, float run) {
                CHECKFAIL_SPEED_RUNSTOP = default;
                CHECKFAIL_SPEED_WALKSTOP = default;
                CHECKFAIL_TIME = default;
            }
        }

        public struct DirectionLock
        {

            public bool lockX;
            public bool lockY;
            public bool lockZ;


            public Vector3 Apply(Vector3 origin) => default;
        }

    }
}