using System.Collections;
using Sirenix.OdinInspector;

namespace TaoTie
{
    public static class GameKeyCode
    {
#if UNITY_EDITOR
        public static IEnumerable GetGameKeyCodeList()
        {
            var fields = typeof(GameKeyCode).GetFields();
            ValueDropdownList<int> list = new ValueDropdownList<int>();
            if (fields.Length > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (!fields[i].IsStatic || fields[i].Name == "Max")
                    {
                        continue;
                    }
                    var val = (int) fields[i].GetValue(null);
                    var attr = fields[i].GetCustomAttributes(typeof(LabelTextAttribute), true);
                    if (attr.Length > 0 && attr[0] is LabelTextAttribute label)
                    {
                        list.Add($"{label.Text}({val})", val);
                    }
                    else
                    {
                        list.Add($"{fields[i].Name}({val})", val);
                    }
                }
               
            }
            return list;
        }
#endif
        public const int MoveForward = 0;
        public const int MoveBack = 1;
        public const int MoveLeft = 2;
        public const int MoveRight = 3;
        public const int Jump = 4;
        [LabelText("普攻")]
        public const int NormalAttack = 5;
        [LabelText("1号交互键位")]
        public const int Opera1 = 6;
        [LabelText("鼠标解锁")]
        public const int CursorUnlock = 7;
        public const int Skill1 = 8;
        public const int Skill2 = 9;
        public const int Back = 10;
        public const int Max = 11;
    }
}