namespace TaoTie
{
    public partial class UnitConfig: II18NConfig
    {
        public string GetI18NText(LangType lang)
        {
            switch (lang)
            {
                case LangType.Chinese:
                    return Chinese;
                case LangType.English:
                    return English;
            }
            return English;
        }
    }
}