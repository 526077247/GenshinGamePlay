namespace TaoTie
{
    public interface IOnWidthPaddingChange
    {
        
    }
    
    public interface IOnTopWidthPaddingChange:IOnWidthPaddingChange
    {
        
    }
    public interface IOnMiniGameWidthPaddingChange:IOnWidthPaddingChange
    {
#if MINIGAME_SUBPLATFORM_DOUYIN
        public const int ButtonSpace = 80;
#else
        public const int ButtonSpace = 0;
#endif
    }
}