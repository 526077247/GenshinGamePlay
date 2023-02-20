namespace TaoTie
{
    public enum MoveDecision
    {
        NoMoveDecision = 0,
        StandStill = 1,
        ReturnToBorn = 2,
        Wander = 3,
        FollowScriptedPath = 4,
        Investigate = 5,
        ReactActionPoint = 6,
        PatrolFollow = 7,
        FollowServerRoute = 8,
        SkillPrepare = 9,
        MeleeCharge = 10,
        CombatFollowMove = 11,
        FacingMove = 12,
        Surround = 13,
        FindBack = 14,
        CombatFixedMove = 15,
        CrabMove = 16,
        BezierMove = 17,
        SpacialChase = 18,
        SpacialProbe = 19,
        SpacialAdjust = 20,
        ScriptedMoveTo = 21,
        Landing = 22,
        Extraction = 23,
        Flee = 24,
        BirdCircling = 25,
        AutoPlayerSkillPrepare = 26,
        AutoPlayerFollowTarget = 27,
        BrownianMove = 28,
        MoveDecisionCount = 29,
        
        Max,
    }
}