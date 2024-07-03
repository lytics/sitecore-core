using Sitecore.Diagnostics;
using System;

namespace LyticsSitecore.Connector.Agent
{
    public class RulesRefreshAgent
    {
        /// <summary>
        /// This method will be invoked at regular intervals as specified in the lytycs.config file.
        /// </summary>
        public void Run()
        {
            try
            {
                Log.Info($"Lytics sitecore connector rules refresh agent invoked at {DateTime.Now}.", this);
                LyticsContext.Service.IntegrateLyticsRules();
            }
            catch (Exception e)
            {
                Log.Error($"Error when refreshing lytics segments {e}.", this);
            }
        }
    }
}
