using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum ConcernType
    {
        [LabelText("AllExcludeGWGO, 场景单位")]
        AllExcludeGWGO = 0,
        [LabelText("CombatExcludeGWGO, 有战斗组件的场景单位")]
        CombatExcludeGWGO = 1,
        [LabelText("AllAvatars, 所有玩家")]
        AllAvatars = 2,
        [LabelText("LocalAvatar, 玩家")]
        LocalAvatar = 3,
        //[LabelText("LocalTeam, 玩家队伍")]
        //LocalTeam = 4
    }
}