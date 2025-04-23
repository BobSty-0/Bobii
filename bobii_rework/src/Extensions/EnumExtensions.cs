using Discord.Interactions;

namespace bobii_rework.Extensions
{
    public static class EnumExtensions
    {
        public static string GetChoiceDisplay(this Enum value)
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            if (memberInfo.Length <= 0)
            {
                return value.ToString();
            }

            var attributes = memberInfo[0].GetCustomAttributes(typeof(ChoiceDisplayAttribute), false);
            return attributes.Length > 0 ? ((ChoiceDisplayAttribute)attributes[0]).Name : value.ToString();
        }
    }
}
