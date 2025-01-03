using LyticsSitecore.Connector.Service;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Engines;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Install.Files;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;
using Sitecore.Install.Utils;
using Sitecore.Pipelines;
using Sitecore.SecurityModel;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace LyticsSitecore.Connector.Pipeline.Initialize
{
    public class InitializeLytics
    {
        public InitializeLytics(string accessKey, string rootAddress)
        {
            AccessKey = accessKey;
            RootAddress = rootAddress;
        }
        public string AccessKey { get; set; }
        public string RootAddress { get; set; }
        public int MaxTimeout { get; set; }
        public void Process(PipelineArgs args)
        {
            try
            {
                Log.Info($"Lytics sitecore connector initialized at {DateTime.Now}.", this);
                LyticsContext.MaxTimeout = MaxTimeout;
                LyticsContext.Service = new LyticsService();
                LyticsContext.AccessKey = AccessKey;
                LyticsContext.RootAddress = RootAddress;
                LyticsContext.Service.IntegrateLyticsRules();
                AssessSitecoreState();
                ActivateLyticsRules();
            }
            catch (Exception e)
            {
                Log.Error($"Error when initializing lytics connector {e}.", this);
            }
        }

        private void ActivateLyticsRules()
        {
            Database db = Factory.GetDatabase("master", false);
            if (db == null) return;
            Item conditional = db.GetItem(Constants.SitecoreIds.ConditionalRenderingsCustomTagId);
            if (conditional != null && conditional[Constants.Common.Tags].Split('|').All(x => x != Constants.SitecoreIds.LyticsRuleTagId))
            {
                using (new SecurityDisabler())
                {
                    using (new EditContext(conditional))
                    {
                        var tags = conditional[Constants.Common.Tags].Split('|').ToList();
                        tags.Add(Constants.SitecoreIds.LyticsRuleTagId);
                        conditional[Constants.Common.Tags] = string.Join("|", tags);
                    }
                }
            }
        }

        public void AssessSitecoreState()
        {
            if (RequiredSitecoreItemsMissing())
            {
                var filepath = "";
                if (System.Text.RegularExpressions.Regex.IsMatch(Settings.DataFolder, @"^(([a-zA-Z]:\\)|(//)).*"))
                    filepath = Settings.DataFolder +
                               @"\packages\Lytics-Sitecore-Connector.zip";
                else
                    filepath = HttpRuntime.AppDomainAppPath + Settings.DataFolder.Substring(1) +
                               @"\packages\Lytics-Sitecore-Connector.zip";
                try
                {
                    if (File.Exists(filepath))
                        File.Delete(filepath);
                    var manifestResourceStream = GetType().Assembly
                        .GetManifestResourceStream("LyticsSitecoreConnector.Resources.Lytics.zip");
                    if (manifestResourceStream != null)
                    {
                        byte[] file = new byte[manifestResourceStream.Length];
                        for (int copied = 0; copied < manifestResourceStream.Length;)
                        {
                            copied += manifestResourceStream.Read(file, copied, (int)manifestResourceStream.Length - copied);
                        }
                        File.WriteAllBytes(filepath, file);
                        while (true)
                        {
                            if (!IsFileLocked(new FileInfo(filepath)))
                            {

                                using (new SecurityDisabler())
                                {
                                    //using (new ProxyDisabler())
                                    //{
                                    using (new SyncOperationContext())
                                    {
                                        IProcessingContext context = new SimpleProcessingContext();
                                        IItemInstallerEvents events =
                                            new DefaultItemInstallerEvents(
                                                new BehaviourOptions(InstallMode.Overwrite, MergeMode.Undefined));
                                        context.AddAspect(events);
                                        IFileInstallerEvents events1 = new DefaultFileInstallerEvents(true);
                                        context.AddAspect(events1);

                                        Sitecore.Install.Installer installer = new Sitecore.Install.Installer();
                                        installer.InstallPackage(MainUtil.MapPath(filepath), context);
                                        break;
                                    }
                                    //}
                                }
                            }
                            else
                                Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Lytics Connector was unable to initialize", e, this);
                }
            }
        }

        /// <summary>
        /// Detects if a required sitecore item is missing.
        /// </summary>
        /// <returns></returns>
        private static bool RequiredSitecoreItemsMissing()
        {
            Database db = Factory.GetDatabase("master", false);
            if (db != null)
            {
                return typeof(Constants.SitecoreIds)
                    .GetFields()
                    .Any(f => db.GetItem(f.GetValue(null).ToString()) == null);
            }
            return false;
        }

        /// <summary>
        /// checks to see if the file is done being written to the filesystem
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }
    }
}
