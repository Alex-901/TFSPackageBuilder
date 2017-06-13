using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using PackageBuilderBusiness.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PackageBuilderBusiness
{
    public class Package : iPackage
    {
        int _releaseId;
        string _tfsURL;
        string _outputPath;
        string _folderName = "\\Package";
        WorkItemStore _workItemStore;
        List<string> _includedFileTypes = new List<string>();

        public string ErrorMessage { get; set; }
        public List<string> Exceptions { get; set; } = new List<string>();

        public string OutputPath { get; set; }

        public Package(int ReleaseId, List<string> fileTypes)
        {
            _releaseId = ReleaseId;
            _includedFileTypes = fileTypes;
        }

        public bool BuildPackage()
        {
            _folderName += "_" + _releaseId;
            GetConfigSettings();
            OutputPath = _outputPath + _folderName;

            return CreatePackage();
        }
        
        public void GetConfigSettings()
        {
            string sIncludedFileTypes = string.Empty;

            if (!GetConfigValue("TFSURL", ref _tfsURL)) { Console.WriteLine("Please set config key TFSURL"); }
            if (!GetConfigValue("OutputPath", ref _outputPath)) { Console.WriteLine("Please set config key OutputPath"); }

            _outputPath = string.Format(_outputPath, Environment.UserName);
        }

        public bool CreatePackage()
        {
            var files = GetLatestFiles();

            if (!files.Any()) { ErrorMessage = "No changesets found"; return false; }

            using (var tfs = new TfsTeamProjectCollection(new Uri(_tfsURL), CredentialCache.DefaultCredentials))
            {
                tfs.EnsureAuthenticated();
                _workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
                var versionControl = tfs.GetService<VersionControlServer>();

                foreach (var x in files)
                {
                    Item item;

                    try
                    {
                        item = versionControl.GetItem(x);
                    }
                    catch (Exception)
                    {
                        Exceptions.Add("[No permissions/File doesn't exist] - " + x);
                        continue;
                    }

                    WriteToFile(item.DownloadFile(), _outputPath + _folderName + "\\" + item.ServerItem.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(), FileMode.Create);
                }
            }

            if (Exceptions.Any())
            {
                File.WriteAllText(_outputPath + _folderName + "\\PackageExceptions.txt", string.Join(Environment.NewLine, Exceptions.Select(x => x)));
            }
            
            return true;
        }

        public void WriteToFile(Stream stream, string fullFilePath, FileMode mode)
        {
            if (!Directory.Exists(_outputPath + _folderName))
            {
                Directory.CreateDirectory(_outputPath + _folderName);
            }

            using (Stream s = stream)
            {
                byte[] content;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    s.CopyTo(memoryStream);
                    content = memoryStream.ToArray();
                }

                FileStream F = new FileStream(fullFilePath, mode, FileAccess.ReadWrite);

                for (int i = 0; i <= content.Length - 1; i++)
                {
                    F.WriteByte(content[i]);
                }

                F.Close();
                F.Dispose();
            }
        }

        public List<string> GetLatestFiles()
        {
            var associatedChangesets = new List<Changeset>();

            using (var tfs = new TfsTeamProjectCollection(new Uri(_tfsURL), CredentialCache.DefaultCredentials))
            {
                tfs.EnsureAuthenticated();
                _workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
                var versionControl = tfs.GetService<VersionControlServer>();

                foreach (WorkItemLinkInfo linkItems in GetLinkedWorkItems())
                {
                    if (linkItems.TargetId != _releaseId)
                    {
                        WorkItem wi = _workItemStore.GetWorkItem(linkItems.TargetId);

                        if (wi != null)
                        {
                            foreach (var link in wi.Links)
                            {
                                if ((link == null) || !(link is ExternalLink)) { continue; }

                                string externalLink = ((ExternalLink)link).LinkedArtifactUri;

                                if (LinkingUtilities.DecodeUri(externalLink).ArtifactType == "Changeset")
                                    associatedChangesets.Add(versionControl.ArtifactProvider.GetChangeset(new Uri(externalLink)));
                            }
                        }
                    }
                }
            }

            return associatedChangesets.SelectMany(x => x.Changes.Select(y => y.Item.ServerItem)
                                                                    .Where(z => _includedFileTypes.Any(a => z.ToUpper().Contains(a)))).Distinct().ToList();
        }

        public WorkItemLinkInfo[] GetLinkedWorkItems()
        {
            Query wiQuery = new Query(_workItemStore, "SELECT [System.Id] FROM WorkItemLinks WHERE [Source].[System.Id] = " + _releaseId);

            return wiQuery.RunLinkQuery();
        }

        public bool GetConfigValue(string KeyName, ref string Value)
        {
            try
            {
                Value = ConfigurationManager.AppSettings[KeyName].ToString();
                return true;
            }
            catch
            {
                Value = string.Empty;
                return false;
            }
        }
    }
}
