namespace TaoTie
{
    public interface II18NConfig
    {
        public string GetI18NText(LangType lang);
    }
    
    public interface II18NSwitchConfig: II18NConfig
    {
        public string GetI18NText(LangType lang, int type = 0);
    }
}