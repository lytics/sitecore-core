using LyticsSitecore.Connector.Models.Interfaces;

namespace LyticsSitecore.Connector.Models
{
    public class LyticsSegment : ILyticsSegment
    {
        public string Name { get; set; }
        public string SlugName { get; set; }
        public string Id { get; set; }
    }
}
