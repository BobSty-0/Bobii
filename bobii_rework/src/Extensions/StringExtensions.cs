using bobii_rework.Enums;

namespace bobii_rework.Extensions
{
    public static class StringExtensions
    {
        public static Language ToLanguage(this string str)
        {
            var enumValues = Enum.GetValues(typeof(Language)).Cast<Language>();
            return enumValues.FirstOrDefault(l => l.ToString() == str);
        }

        public static byte ToByte(this string str)
        {
            return byte.Parse(str);
        }
    }
}
