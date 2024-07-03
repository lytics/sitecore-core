using LyticsSitecore.Connector.Models;
using LyticsSitecore.Connector.Models.Interfaces;
using LyticsSitecore.Connector.Service.Interface;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;


namespace LyticsSitecore.Connector.Service
{
    public class LyticsService:ILyticsService
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
            if (db != null)
            {
                Dictionary<string, ID> populatedSegments = new Dictionary<string, ID>();
                Item lyticsSegmentFolder = db.GetItem(Constants.SitecoreIds.SegmentRuleFolder);
                if (lyticsSegmentFolder != null)
                {
                    using (new SecurityDisabler())
                    {
                        foreach (Item scSegment in lyticsSegmentFolder.Children)
                        {
                            if (!populatedSegments.ContainsKey(scSegment[Constants.LyticsSegments.SegmentId]))
                            {
                                populatedSegments.Add(scSegment[Constants.LyticsSegments.SegmentId], scSegment.ID);
                            }
                        }
                        var segments = GetAllSegments();
                        if (segments != null && segments.Any())
                        {
                            foreach (ILyticsSegment segment in GetAllSegments().Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                            {
                                Item item;
                                if (populatedSegments.ContainsKey(segment.Id))
                                {
                                    item = db.GetItem(populatedSegments[segment.Id]);
                                    populatedSegments.Remove(segment.Id);
                                }
                                else
                                {
                                    string name = ItemUtil.ProposeValidItemName(segment.Name, Constants.LyticsSegments.UnknownSegment);
                                    item = lyticsSegmentFolder.Add(name, new TemplateID(new ID(Constants.SitecoreIds.SegmentTemplateId)));
                                }
                                if (item != null)
                                {
                                    using (new EditContext(item))
                                    {
                                        item[Constants.LyticsSegments.SegmentId] = segment.Id;
                                        item[Constants.LyticsSegments.SegmentName] = segment.SlugName;
                                        item.Appearance.ReadOnly = true;
                                    }
                                }
                            } 
                        }
                        if (populatedSegments != null && populatedSegments.Any())
                        {
                            foreach (string key in populatedSegments.Keys)
                            {
                                db.GetItem(populatedSegments[key]).Delete();
                            } 
                        }
                           
                    }
                }
            }
        }
    }
}
