using LyticsSitecore.Connector.Models.Interfaces;
using System.Collections.Generic;

namespace LyticsSitecore.Connector.Service.Interface
{
    public interface ILyticsService
    {
        IEnumerable<ILyticsSegment> GetAllSegments();
        HashSet<string> GetCurrentUserSegmentIds();
        void IntegrateLyticsRules();
    }
}
