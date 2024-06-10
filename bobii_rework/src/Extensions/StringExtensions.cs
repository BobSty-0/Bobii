using bobii_rework.Enums;

namespace bobii_rework.Extensions
{
    public static class StringExtensions
    {
        public static Language ToLanguage(this string str)
        {
            switch (str)
            {
                case "en":
                    return Language.EN;
                case "de":
                    return Language.DE;
                case "ru":
                    return Language.RU;
                default:
                    throw new NotSupportedException($"{str} ist keine unterstütze Sprache");
            }
        }

        public static byte ToByte(this string str)
        {
            return byte.Parse(str);
        }
    }
}
