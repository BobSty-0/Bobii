using bobii_rework.Extensions;

namespace bobii_rework.Handler
{
    public abstract class EventHandlerBase
    {
        #region Tasks
        public Task Execute()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (await CheckData())
                    {
                        return;
                    }

                    await ExecuteEvent();
                }
                catch (Exception ex)
                {
                    this.WriteLineToConsole($"{ex.Message} | {ex.StackTrace}");
                }
            });

            return Task.CompletedTask;
        }
        #endregion

        #region Overridables
        public virtual Task<bool> CheckData()
        {
            return Task.FromResult(false);
        }

        public abstract Task ExecuteEvent();
        #endregion
    }
}
