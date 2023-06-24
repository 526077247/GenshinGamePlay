using System.Collections.Generic;
namespace TaoTie
{
    public class NumericType
    {
        public static int GetKey(string key)
        {
            if (Map.TryGetValue(key, out var res))
            {
                return res;
            }
            Log.Error($"{key}属性不存在");
            return -1;
        }
        private static Dictionary<string, int> map;
        public static Dictionary<string, int> Map
        {
            get
            {
                if (map == null)
                {
                    map = new Dictionary<string, int>();
                    map.Add("AOI",AOI);
                    map.Add("AOIBase",AOIBase);
                    map.Add("Lv",Lv);
                    map.Add("LvBase",LvBase);
                    map.Add("Exp",Exp);
                    map.Add("ExpBase",ExpBase);
                    map.Add("Hp",Hp);
                    map.Add("HpBase",HpBase);
                    map.Add("MaxHp",MaxHp);
                    map.Add("MaxHpBase",MaxHpBase);
                    map.Add("MaxHpAdd",MaxHpAdd);
                    map.Add("MaxHpPct",MaxHpPct);
                    map.Add("MaxHpFinalAdd",MaxHpFinalAdd);
                    map.Add("MaxHpFinalPct",MaxHpFinalPct);
                    map.Add("HpReUp",HpReUp);
                    map.Add("HpReUpBase",HpReUpBase);
                    map.Add("HpReUpAdd",HpReUpAdd);
                    map.Add("HpReUpPct",HpReUpPct);
                    map.Add("HpReUpFinalAdd",HpReUpFinalAdd);
                    map.Add("HpReUpFinalPct",HpReUpFinalPct);
                    map.Add("Mp",Mp);
                    map.Add("MpBase",MpBase);
                    map.Add("MaxMp",MaxMp);
                    map.Add("MaxMpBase",MaxMpBase);
                    map.Add("MaxMpAdd",MaxMpAdd);
                    map.Add("MaxMpPct",MaxMpPct);
                    map.Add("MaxMpFinalAdd",MaxMpFinalAdd);
                    map.Add("MaxMpFinalPct",MaxMpFinalPct);
                    map.Add("MpReUp",MpReUp);
                    map.Add("MpReUpBase",MpReUpBase);
                    map.Add("MpReUpAdd",MpReUpAdd);
                    map.Add("MpReUpPct",MpReUpPct);
                    map.Add("MpReUpFinalAdd",MpReUpFinalAdd);
                    map.Add("MpReUpFinalPct",MpReUpFinalPct);
                    map.Add("Speed",Speed);
                    map.Add("SpeedBase",SpeedBase);
                    map.Add("SpeedAdd",SpeedAdd);
                    map.Add("SpeedPct",SpeedPct);
                    map.Add("SpeedFinalAdd",SpeedFinalAdd);
                    map.Add("SpeedFinalPct",SpeedFinalPct);
                    map.Add("ATK",ATK);
                    map.Add("ATKBase",ATKBase);
                    map.Add("ATKAdd",ATKAdd);
                    map.Add("ATKPct",ATKPct);
                    map.Add("ATKFinalAdd",ATKFinalAdd);
                    map.Add("ATKFinalPct",ATKFinalPct);
                    map.Add("DEF",DEF);
                    map.Add("DEFBase",DEFBase);
                    map.Add("DEFAdd",DEFAdd);
                    map.Add("DEFPct",DEFPct);
                    map.Add("DEFFinalAdd",DEFFinalAdd);
                    map.Add("DEFFinalPct",DEFFinalPct);
                    map.Add("EN",EN);
                    map.Add("ENBase",ENBase);
                }
                return map;
            }
        }
		public const int Max = 10000;

		/// <summary> AOI </summary>
		public const int AOI = 1000;
		/// <summary> AOIBase </summary>
		public const int AOIBase = 1000 * 10 + 1;

		/// <summary> 等级 </summary>
		public const int Lv = 1001;
		/// <summary> 等级Base </summary>
		public const int LvBase = 1001 * 10 + 1;

		/// <summary> Exp </summary>
		public const int Exp = 1002;
		/// <summary> ExpBase </summary>
		public const int ExpBase = 1002 * 10 + 1;

		/// <summary> 当前血量 </summary>
		public const int Hp = 1003;
		/// <summary> 当前血量Base </summary>
		public const int HpBase = 1003 * 10 + 1;

		/// <summary> 最大血量上限 </summary>
		public const int MaxHp = 1004;
		/// <summary> 最大血量上限Base </summary>
		public const int MaxHpBase = 1004 * 10 + 1;
		/// <summary> 最大血量上限Add </summary>
		public const int MaxHpAdd = 1004 * 10 + 2;
		/// <summary> 最大血量上限Pct </summary>
		public const int MaxHpPct = 1004 * 10 + 3;
		/// <summary> 最大血量上限FinalAdd </summary>
		public const int MaxHpFinalAdd = 1004 * 10 + 4;
		/// <summary> 最大血量上限FinalPct </summary>
		public const int MaxHpFinalPct = 1004 * 10 + 5;

		/// <summary> 血量回复 </summary>
		public const int HpReUp = 1005;
		/// <summary> 血量回复Base </summary>
		public const int HpReUpBase = 1005 * 10 + 1;
		/// <summary> 血量回复Add </summary>
		public const int HpReUpAdd = 1005 * 10 + 2;
		/// <summary> 血量回复Pct </summary>
		public const int HpReUpPct = 1005 * 10 + 3;
		/// <summary> 血量回复FinalAdd </summary>
		public const int HpReUpFinalAdd = 1005 * 10 + 4;
		/// <summary> 血量回复FinalPct </summary>
		public const int HpReUpFinalPct = 1005 * 10 + 5;

		/// <summary> 当前法力 </summary>
		public const int Mp = 1006;
		/// <summary> 当前法力Base </summary>
		public const int MpBase = 1006 * 10 + 1;

		/// <summary> 最大法力上限 </summary>
		public const int MaxMp = 1007;
		/// <summary> 最大法力上限Base </summary>
		public const int MaxMpBase = 1007 * 10 + 1;
		/// <summary> 最大法力上限Add </summary>
		public const int MaxMpAdd = 1007 * 10 + 2;
		/// <summary> 最大法力上限Pct </summary>
		public const int MaxMpPct = 1007 * 10 + 3;
		/// <summary> 最大法力上限FinalAdd </summary>
		public const int MaxMpFinalAdd = 1007 * 10 + 4;
		/// <summary> 最大法力上限FinalPct </summary>
		public const int MaxMpFinalPct = 1007 * 10 + 5;

		/// <summary> 法力回复 </summary>
		public const int MpReUp = 1008;
		/// <summary> 法力回复Base </summary>
		public const int MpReUpBase = 1008 * 10 + 1;
		/// <summary> 法力回复Add </summary>
		public const int MpReUpAdd = 1008 * 10 + 2;
		/// <summary> 法力回复Pct </summary>
		public const int MpReUpPct = 1008 * 10 + 3;
		/// <summary> 法力回复FinalAdd </summary>
		public const int MpReUpFinalAdd = 1008 * 10 + 4;
		/// <summary> 法力回复FinalPct </summary>
		public const int MpReUpFinalPct = 1008 * 10 + 5;

		/// <summary> 移动速度 </summary>
		public const int Speed = 1009;
		/// <summary> 移动速度Base </summary>
		public const int SpeedBase = 1009 * 10 + 1;
		/// <summary> 移动速度Add </summary>
		public const int SpeedAdd = 1009 * 10 + 2;
		/// <summary> 移动速度Pct </summary>
		public const int SpeedPct = 1009 * 10 + 3;
		/// <summary> 移动速度FinalAdd </summary>
		public const int SpeedFinalAdd = 1009 * 10 + 4;
		/// <summary> 移动速度FinalPct </summary>
		public const int SpeedFinalPct = 1009 * 10 + 5;

		/// <summary> 攻击力 </summary>
		public const int ATK = 1010;
		/// <summary> 攻击力Base </summary>
		public const int ATKBase = 1010 * 10 + 1;
		/// <summary> 攻击力Add </summary>
		public const int ATKAdd = 1010 * 10 + 2;
		/// <summary> 攻击力Pct </summary>
		public const int ATKPct = 1010 * 10 + 3;
		/// <summary> 攻击力FinalAdd </summary>
		public const int ATKFinalAdd = 1010 * 10 + 4;
		/// <summary> 攻击力FinalPct </summary>
		public const int ATKFinalPct = 1010 * 10 + 5;

		/// <summary> 防御力 </summary>
		public const int DEF = 1011;
		/// <summary> 防御力Base </summary>
		public const int DEFBase = 1011 * 10 + 1;
		/// <summary> 防御力Add </summary>
		public const int DEFAdd = 1011 * 10 + 2;
		/// <summary> 防御力Pct </summary>
		public const int DEFPct = 1011 * 10 + 3;
		/// <summary> 防御力FinalAdd </summary>
		public const int DEFFinalAdd = 1011 * 10 + 4;
		/// <summary> 防御力FinalPct </summary>
		public const int DEFFinalPct = 1011 * 10 + 5;

		/// <summary> 霸体值 </summary>
		public const int EN = 1012;
		/// <summary> 霸体值Base </summary>
		public const int ENBase = 1012 * 10 + 1;
    }
}
