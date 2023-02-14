using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

namespace Slate.ActionClips
{

    [Category("Paths")]
    [Description("For this clip to work you only need to have a baked NavMesh. The actor does NOT need, or use a NavMeshAgent Component. The length of the clip is determined by the path's length and the speed parameter set, while the Blend In parameter is used only for the initial look ahead blending")]
    public class PathfindFromTo : ActorActionClip
    {

        [SerializeField]
        [HideInInspector]
        private float _blendIn = 0.5f;

        [Min(0.01f)]
        public float speed = 3f;
        public TransformRefPosition sourcePosition;
        public TransformRefPosition targetPosition;

        private Vector3[] pathPoints;

        private Vector3 wasPosition;
        private Quaternion wasRotation;

        private Vector3 lastFrom;
        private Vector3 lastTo;

        public override string info {
            get { return string.Format("Pathfind To\n{0}", targetPosition.ToString()); }
        }

        public override float length {
            get
            {
                if ( isValid ) {
                    TryCalculatePath();
                    return Path.GetLength(pathPoints) / speed;
                }
                return 0;
            }
        }

        public override float blendIn {
            get { return length > 0 ? _blendIn : 0; }
            set { _blendIn = value; }
        }

        protected override void OnEnter() {
            pathPoints = null; //recalc
            TryCalculatePath();
            wasPosition = actor.transform.position;
            wasRotation = actor.transform.rotation;
            if ( pathPoints == null || pathPoints.Length == 0 ) {
                Debug.LogWarning(string.Format("Actor '{0}' can't pathfind to '{1}'", actor.name, targetPosition.value), actor);
            }
        }

        protected override void OnUpdate(float time) {
            if ( pathPoints != null && pathPoints.Length > 1 ) {

                if ( length == 0 ) {
                    actor.transform.position = Path.GetPosition(0, pathPoints);
                    return;
                }

                //POSITION
                var pos = actor.transform.position;
                pos = Path.GetPosition(time / length, pathPoints);
                NavMeshHit hit;
                if ( NavMesh.SamplePosition(pos, out hit, 10f, -1) ) {
                    pos.y = hit.position.y;
                }
                actor.transform.position = pos;


                //ROTATION
                var lookPos = Path.GetPosition(( time / length ) + 0.01f, pathPoints); //FIXME:...!
                if ( blendIn > 0 && time <= blendIn ) {
                    var lookRot = Quaternion.LookRotation(lookPos - actor.transform.position);
                    actor.transform.rotation = Easing.Ease(EaseType.QuadraticInOut, wasRotation, lookRot, time / blendIn);
                } else {
                    actor.transform.LookAt(lookPos);
                }
                var angles = actor.transform.GetLocalEulerAngles();
                angles.x = 0;
                angles.z = 0;
                actor.transform.SetLocalEulerAngles(angles);
            }
        }

        protected override void OnReverse() {
            actor.transform.position = wasPosition;
            actor.transform.rotation = wasRotation;
            pathPoints = null;
        }

        void TryCalculatePath() {
            var targetPos = TransformPosition(targetPosition.value, targetPosition.space);
            var sourcePos = TransformPosition(sourcePosition.value, sourcePosition.space);
            if ( pathPoints == null || lastFrom != sourcePos || lastTo != targetPos ) {
                var navPath = new NavMeshPath();
                NavMesh.CalculatePath(sourcePos, targetPos, -1, navPath);
                pathPoints = navPath.corners;
            }
            lastFrom = sourcePos;
            lastTo = targetPos;
        }


        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        protected override void OnDrawGizmosSelected() {

            var targetPos = TransformPosition(targetPosition.value, targetPosition.space);
            var sourcePos = TransformPosition(sourcePosition.value, sourcePosition.space);
            Gizmos.DrawSphere(targetPos, 0.2f);
            Gizmos.DrawSphere(sourcePos, 0.2f);

            if ( pathPoints != null && pathPoints.Length > 1 ) {
                Gizmos.color = Color.red;
                for ( int i = 0; i < pathPoints.Length; i++ ) {
                    var a = pathPoints[i];
                    var b = ( i == pathPoints.Length - 1 ) ? pathPoints[i] : pathPoints[i + 1];
                    Gizmos.DrawLine(a, b);
                }
                Gizmos.color = Color.white;
            }
        }

        protected override void OnSceneGUI() {
            var targetPos = targetPosition.value;
            var sourcePos = sourcePosition.value;
            DoVectorPositionHandle(targetPosition.space, ref targetPos);
            DoVectorPositionHandle(sourcePosition.space, ref sourcePos);
            targetPosition.value = targetPos;
            sourcePosition.value = sourcePos;
        }

#endif

    }
}