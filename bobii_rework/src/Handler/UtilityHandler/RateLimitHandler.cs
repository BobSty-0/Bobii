using bobii_rework.Entities.Interactions;
using bobii_rework.Extensions;
using bobii_rework.GlobalConstants.Sprachcodes;
using bobii_rework.Helper;
using bobii_rework.src.GlobalConstants.Sprachcodes;
using Discord;

namespace bobii_rework.Handler.UtilityHandler
{
    public class RateLimitHandler
    {
        #region Declarations
        private BobiiInteractionContext _bobiiInteractionContext;
        #endregion

        #region Constructor
        public RateLimitHandler(BobiiInteractionContext context)
        {
            _bobiiInteractionContext = context;
        }
        #endregion

        #region Tasks
        public async Task MyRatelimitCallback(IRateLimitInfo arg)
        {
            if (!arg.RetryAfter.HasValue)
            {
                return;
            }

            var unixTimeStamp = DateTimeOffset.UtcNow.AddSeconds(arg.RetryAfter.Value + 1).ToUnixTimeSeconds();
            await _bobiiInteractionContext.RespondOrModifyOriginalResponse(
                Captions.Error, 
                Contents.RatelimitAusgeloest, 
                new object[] { unixTimeStamp });

            _bobiiInteractionContext.HasResponded = true;
        }
        #endregion
    }
}
