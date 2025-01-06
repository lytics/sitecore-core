using LyticsSitecore.Connector.Models;
using LyticsSitecore.Connector.Models.Interfaces;
using LyticsSitecore.Connector.Service.Interface;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;


namespace LyticsSitecore.Connector.Service
{
    public class LyticsService : ILyticsService
    {
        private Dictionary<string, HashSet<string>> _segmentDef = new Dictionary<string, HashSet<string>>();
        public IEnumerable<ILyticsSegment> GetAllSegments()
        {
            try
            {
                List<ILyticsSegment> ret = new List<ILyticsSegment>();
                WebClient wc = new WebClient();
                dynamic data =
                    JsonConvert.DeserializeObject<ExpandoObject>(
                        wc.DownloadString(string.Format("{0}/api/segment?access_token={1}", LyticsContext.RootAddress, LyticsContext.AccessKey)));
                if (data != null && data.data != null)
                {
                    foreach (dynamic segment in data.data)
                    {
                        try
                        {
                            if (segment.kind == Constants.Common.Segment)
                            {
                                ret.Add(new LyticsSegment()
                                {
                                    Id = segment.id,
                                    Name = segment.name,
                                    SlugName = segment.slug_name
                                });
                            }
                        }
                        catch (RuntimeBinderException ex)
                        {
                            //means it's not a segment and we don't want it.
                        }
                    }
                }
                return ret;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public HashSet<string> GetCurrentUserSegmentIds()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[Constants.Cookie.LyticsSegments];
            HashSet<string> ret = new HashSet<string>();
            if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
            {
                ret.UnionWith(JsonConvert.DeserializeObject<Dictionary<string, string>>(HttpUtility.UrlDecode(cookie.Value)).Keys);
            }
            return ret;
        }

        public void IntegrateLyticsRules()
        {
            Database db = Factory.GetDatabase("master", false);
            if (db == null)
            {
                Log.Error("Failed to get the master database.", this);
                return;
            }

            Dictionary<string, ID> populatedSegments = new Dictionary<string, ID>();
            Item lyticsSegmentFolder = db.GetItem(Constants.SitecoreIds.SegmentRuleFolder);

            if (lyticsSegmentFolder == null)
            {
                Log.Error($"Lytics Segment Folder not found at {Constants.SitecoreIds.SegmentRuleFolder}.", this);
                return;
            }

            using (new SecurityDisabler())
            {
                // Populate existing Sitecore segments into the dictionary
                foreach (Item scSegment in lyticsSegmentFolder.Children)
                {
                    string segmentId = scSegment[Constants.LyticsSegments.SegmentId];
                    if (!string.IsNullOrWhiteSpace(segmentId) && !populatedSegments.ContainsKey(segmentId))
                    {
                        populatedSegments.Add(segmentId, scSegment.ID);
                    }
                }

                // Fetch all Lytics segments
                var segments = GetAllSegments()?.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
                if (segments == null || !segments.Any())
                {
                    Log.Warn("No Lytics segments found to integrate.", this);
                    return;
                }

                // Process each Lytics segment
                foreach (ILyticsSegment segment in segments)
                {

                    if (populatedSegments.TryGetValue(segment.Id, out ID existingItemId))
                    {
                        //item = db.GetItem(existingItemId);
                        populatedSegments.Remove(segment.Id); // Mark as processed
                    }
                    else
                    {
                        string name = ItemUtil.ProposeValidItemName(segment.Name, Constants.LyticsSegments.UnknownSegment);
                        var item = lyticsSegmentFolder.Add(name, new TemplateID(new ID(Constants.SitecoreIds.SegmentTemplateId)));
                        if (item != null)
                        {
                            using (new EditContext(item))
                            {
                                item[Constants.LyticsSegments.SegmentId] = segment.Id;
                                item[Constants.LyticsSegments.SegmentName] = segment.SlugName;
                                item.Appearance.ReadOnly = true;
                            }
                        }
                        else
                        {
                            Log.Warn($"Failed to create or update segment: {segment.Name} (ID: {segment.Id}).", this);
                        }
                    }


                }

                // Remove any remaining Sitecore segments that were not in Lytics
                if (populatedSegments != null && populatedSegments.Any())
                {
                    foreach (var segmentId in populatedSegments.Keys)
                    {
                        var item = db.GetItem(populatedSegments[segmentId]);
                        if (item != null)
                        {
                            item.Delete();
                            Log.Warn($"Removed Sitecore segment with ID: {item.ID} and Name: {item.Name} as it no longer exists in Lytics.", this);
                        }
                    }
                }
            }
        }

    }
}
