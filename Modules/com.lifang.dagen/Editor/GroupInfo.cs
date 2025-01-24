using System;
using System.Collections.Generic;
using System.Reflection;

namespace DaGenGraph.Editor
{
    public interface ISort
    {
        public float MinSort { get; }
    }
    public class GroupItem:ISort
    {
        public float MinSort { get; set; }
        public string GroupId;
        public List<MemberItem> Members = new ();
    }
    
    public class MemberItem:ISort
    {
        public float MinSort{ get; set; }
        public MemberInfo Member;
    }
    
}