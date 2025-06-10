namespace TaoTie
{
    public static class CDNConfigHelper
    {
        public static string GetChannel(this CDNConfig self)
        {
            if (self == null) return string.Empty;
#if !UNITY_WEBGL
            var rename = "common";
            for (int i = 0; i < Define.RenameList.Length; i++)
            {
                if (Define.RenameList[i] == self.Channel)
                {
                    rename = self.Channel;
                    break;
                }
            }
            return rename;
#else
            return self.Channel;
#endif
        }
    }
}