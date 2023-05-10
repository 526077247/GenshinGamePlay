#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Slate;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace TaoTie
{
    [Attachable(typeof(SpineAnimatorTrack))]
    public class PlaySpineAnimatorClip: ActionClip
    {
        [SerializeField]
        [HideInInspector]
        private float _length = 1.0f / 60.0f;

        public override float length
        {
            get { return _length; }
            set { _length = value; }
        }
        
        public SkeletonDataAsset Data;
        
        [ValueDropdown(nameof(GetShowClipName))]
        public string ClipName;

        private IEnumerable GetShowClipName()
        {
            if(Data==null) return null;
            ValueDropdownList<string> list = new ValueDropdownList<string>();
            var data = Data.GetSkeletonData(false);
            if (data.Animations.Count > 0)
            {
                foreach (var clip in data.Animations)
                {
                    list.Add(clip.Name, clip.Name);
                }
               
            }
            return list;
        }
    }
}
#endif