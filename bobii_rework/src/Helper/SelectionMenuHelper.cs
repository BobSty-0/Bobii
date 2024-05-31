using Discord;

namespace bobii_rework.Helper
{
    public class SelectionMenuHelper
    {
        #region Methods
        public static SelectMenuBuilder GetSelectionMenuBuilder(string customId, List<SelectMenuOptionBuilder> options, string placeHolder = "", int maxValue = 1, int minValue = 1)
        {
            var selectMenu = new SelectMenuBuilder()
                .WithPlaceholder(placeHolder)
                .WithMinValues(maxValue)
                .WithMaxValues(minValue)
                .WithCustomId(customId)
                .WithType(ComponentType.SelectMenu)
                .WithOptions(options);

            return selectMenu;
        }
        #endregion
    }
}
