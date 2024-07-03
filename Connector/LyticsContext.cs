using LyticsSitecore.Connector.Service.Interface;

namespace LyticsSitecore.Connector
{
    public static class LyticsContext
    {
        public static ILyticsService Service { get; internal set; }

        public static string AccessKey { get; internal set; }

        public static string RootAddress { get; internal set; }

        public static int MaxTimeout { get; internal set; }
    }
}
