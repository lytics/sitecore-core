using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using System;

namespace LyticsSitecore.Connector.Rules
{
    public class LyticsSegmentMatch<T> : StringOperatorCondition<T>
     where T : RuleContext
    {
        public ID Segment { get; set; }

        protected override bool Execute(T ruleContext)
        {
            try
            {
                Log.Info($"Lytics sitecore connector initialized at {DateTime.Now}.", this);
                Item obj = ruleContext.Item.Database.GetItem(Segment);
                if (obj != null)
                {
                    var currentUserSegmentIds = LyticsContext.Service.GetCurrentUserSegmentIds();
                    return currentUserSegmentIds.Contains(obj[Constants.LyticsSegments.SegmentName]);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"Error when matching lytics segments {e}.", this);
                return false;
            }
        }
    }
}
