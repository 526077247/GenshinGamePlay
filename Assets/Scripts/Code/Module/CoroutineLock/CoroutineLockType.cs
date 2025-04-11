namespace TaoTie
{
    public class CoroutineLockType
    {
        public const int None = 0;
        public const int Resources = 1;
        public const int UIManager = 2;
        public const int UIMsgBox = 3;
        public const int EnableObjView = 4;
        public const int PathQuery = 5;
        public const int Max = 100; // 这个必须最大
    }
}