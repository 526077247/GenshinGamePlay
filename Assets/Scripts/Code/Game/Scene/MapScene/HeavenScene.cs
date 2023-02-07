namespace TaoTie
{
    /// <summary>
    /// 大闹天宫场景
    /// </summary>
    public class HeavenScene:BaseMapScene
    {
        public override string Name => "Heaven";
        public override string GetScenePath()
        {
            return "Scenes/MapScene/Heaven.unity";
        }
    }
}