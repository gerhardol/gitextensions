using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Newtonsoft.Json.Linq;

namespace JenkinsIntegration
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JenkinsIntegrationMetadata : BuildServerAdapterMetadataAttribute
    {
        public JenkinsIntegrationMetadata(string buildServerType)
            : base(buildServerType) { }

        public override string CanBeLoaded
        {
            get
            {
                if (EnvUtils.IsNet4FullOrHigher())
                    return null;
                return ".Net 4 full framework required";
            }
        }
    }

    [Export(typeof(IBuildServerAdapter))]
    [JenkinsIntegrationMetadata("Jenkins")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class JenkinsAdapter : IBuildServerAdapter
    {
        private static readonly IBuildDurationFormatter _buildDurationFormatter = new BuildDurationFormatter();
        private IBuildServerWatcher _buildServerWatcher;

        private HttpClient _httpClient;

        private static readonly Dictionary<string, JenkinsCacheInfo> _buildCache = new Dictionary<string, JenkinsCacheInfo>();
        private readonly IList<string> _projectsUrls = new List<string>();
        private IList<Task<JenkinsInfo>> _getBuildUrls;
        public void Initialize(IBuildServerWatcher buildServerWatcher, ISettingsSource config, Func<string, bool> isCommitInRevisionGrid)
        {
            if (_buildServerWatcher != null)
                throw new InvalidOperationException("Already initialized");

            _buildServerWatcher = buildServerWatcher;

            var projectName = config.GetString("ProjectName", null);
            var hostName = config.GetString("BuildServerUrl", null);

            if (!string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(projectName))
            {
                var baseAdress = hostName.Contains("://")
                                     ? new Uri(hostName, UriKind.Absolute)
                                     : new Uri(string.Format("{0}://{1}:8080", Uri.UriSchemeHttp, hostName), UriKind.Absolute);

                _httpClient = new HttpClient(new HttpClientHandler(){ UseDefaultCredentials = true});
                _httpClient.Timeout = TimeSpan.FromMinutes(2);
                _httpClient.BaseAddress = baseAdress;

                var buildServerCredentials = buildServerWatcher.GetBuildServerCredentials(this, true);

                UpdateHttpClientOptions(buildServerCredentials);

                string[] projectUrls = _buildServerWatcher.ReplaceVariables(projectName)
                    .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var projectUrl in projectUrls.Select(s => baseAdress + "job/" + s.Trim() + "/"))
                {
                    AddGetBuildUrl(projectUrl);
                }
            }
        }

        private void AddGetBuildUrl(string projectUrl)
        {
            if (!_projectsUrls.Contains(projectUrl))
            {
                _projectsUrls.Add(projectUrl);
                if (_buildCache.ContainsKey(projectUrl))
                {
                    //Make sure the cache is recalculated but not invalidated
                    _buildCache[projectUrl].Timestamp = -1;
                }
            }
        }

        public class JenkinsInfo
        {
            public string Url { get; set; }
            public long Timestamp { get; set; }
            public IEnumerable<JToken> JobDesc { get; set; }
        }

        public class JenkinsCacheInfo
        {
            public long Timestamp = -1;
            public IList<BuildInfo> BuildInfo = new List<BuildInfo>();
        }

        private IList<Task<JenkinsInfo>> GetBuildInfo(bool fullInfo)
        {
            return _projectsUrls.Select(
                        projectUrl => GetBuildInfoTask(projectUrl, fullInfo))
                        .ToList();
        }

        private Task<JenkinsInfo> GetBuildInfoTask(string projectUrl, bool fullInfo)
        {
            return GetResponseAsync(FormatToGetJson(projectUrl, fullInfo), CancellationToken.None)
                .ContinueWith(
                    task =>
                    {
                        long timestamp = 0;
                        IEnumerable<JToken> s = Enumerable.Empty<JToken>();
                        string t = task.Result;
                        if (t.IsNotNullOrWhitespace())
                        {
                            JObject jobDescription = JObject.Parse(t);
                            if (jobDescription["builds"] != null)
                            {
                                //Freestyle jobs
                                s = jobDescription["builds"];
                            }
                            else if (jobDescription["jobs"] != null)
                            {
                                //Multibranch pipeline
                                s = jobDescription["jobs"]
                                    .SelectMany(j => j["builds"]);
                                foreach (var j in jobDescription["jobs"])
                                {
                                    long ts = j["lastBuild"]["timestamp"].ToObject<long>();
                                    timestamp = Math.Max(timestamp, ts);
                                }
                            }
                            //else: The server is overloaded or a multibranch pipeline is not configured

                            if (jobDescription["lastBuild"] != null)
                            {
                                timestamp = jobDescription["lastBuild"]["timestamp"].ToObject<long>();
                            }
                        }

                        return new JenkinsInfo
                        {
                            Url = projectUrl,
                            Timestamp = timestamp,
                            JobDesc = s
                        };
                    },
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent);
        }

        private IList<Task<JenkinsInfo>> GetBuildUrls(bool forceUpdate)
        {
            if (_getBuildUrls == null || forceUpdate)
            {
                _getBuildUrls = GetBuildInfo(true);
            }
            return _getBuildUrls;
        }

        private IList<Task<JenkinsInfo>> GetLastBuildInfo()
        {
            return GetBuildInfo(false);
        }

        /// <summary>
        /// Gets a unique key which identifies this build server.
        /// </summary>
        public string UniqueKey
        {
            get { return _httpClient.BaseAddress.Host; }
        }

        public IObservable<BuildInfo> GetFinishedBuildsSince(IScheduler scheduler, DateTime? sinceDate = null)
        {
            //Similar as for AppVeyor
            //GetBuilds() will return the same builds as for GetRunningBuilds().
            //Multiple calls will fetch same info multiple times and make debugging very confusing
            return Observable.Empty<BuildInfo>();
        }

        public IObservable<BuildInfo> GetRunningBuilds(IScheduler scheduler)
        {
            return GetBuilds(scheduler, null, true);
        }

        private IObservable<BuildInfo> GetBuilds(IScheduler scheduler, DateTime? sinceDate = null, bool? running = null)
        {
            return Observable.Create<BuildInfo>((observer, cancellationToken) =>
                Task<IDisposable>.Factory.StartNew(
                    () => scheduler.Schedule(() => ObserveBuilds(sinceDate, running, observer, cancellationToken))));
        }

        private void ObserveBuilds(DateTime? sinceDate, bool? running, IObserver<BuildInfo> observer, CancellationToken cancellationToken)
        {
            try
            {
                //Fetch information about all builds for a project only if the cha
                //Wait for new builds and for inprogress builds to complete
                //There is no need to let this job complete until there has been an updated build,
                //complete and let the Observe.Retry() requery all builds (every 10th second)

                IList<Task<JenkinsInfo>> buildUrls = new List<Task<JenkinsInfo>>();
                IList<Task<JenkinsInfo>> cacheUrls = new List<Task<JenkinsInfo>>();
                foreach (var projectUrl in _projectsUrls)
                {
                    bool added = false;
                    if (_buildCache.ContainsKey(projectUrl))
                    {
                        //Update build info from the cache, also if refreshing
                        foreach (var buildInfo in _buildCache[projectUrl].BuildInfo)
                        {
                            observer.OnNext(buildInfo);
                            //If any job is InProgress, it has to be requeried
                            //This could be done per job too, probably not adding anything
                            if (buildInfo.Status == BuildInfo.BuildStatus.InProgress && !added)
                            {
                                added = true;
                                buildUrls.Add(GetBuildInfoTask(projectUrl, true));
                            }
                        }
                    }

                    //Note that the cache is not invalidated when 'running' is set,
                    //but the cache is force updated when switching repos
                    if (!added)
                    {
                        if (!_buildCache.ContainsKey(projectUrl)
                            || _buildCache[projectUrl].Timestamp < 0)
                        {
                            _buildCache[projectUrl] = new JenkinsCacheInfo();
                            buildUrls.Add(GetBuildInfoTask(projectUrl, true));
                        }
                        else
                        {
                            cacheUrls.Add(GetBuildInfoTask(projectUrl, false));
                        }
                    }
                }

                foreach (var url in cacheUrls)
                {
                    if (!url.IsFaulted)
                    {
                        if (url.Result.Timestamp > _buildCache[url.Result.Url].Timestamp)
                        {
                            buildUrls.Add(GetBuildInfoTask(url.Result.Url, true));
                        }
                    }
                }

                if (buildUrls.All(t => t.IsCanceled))
                {
                    observer.OnCompleted();
                    return;
                }


                foreach (var currentGetBuildUrls in buildUrls)
                {
                    //Update from cache if newer
                    if (currentGetBuildUrls.IsFaulted)
                    {
                        Debug.Assert(currentGetBuildUrls.Exception != null);

                        observer.OnError(currentGetBuildUrls.Exception);
                        continue;
                    }

                    _buildCache[currentGetBuildUrls.Result.Url].Timestamp = currentGetBuildUrls.Result.Timestamp;
                    _buildCache[currentGetBuildUrls.Result.Url].BuildInfo.Clear();

                    foreach (var buildDetails in currentGetBuildUrls.Result.JobDesc)
                    {
                        var buildInfo = CreateBuildInfo((JObject) buildDetails);
                        _buildCache[currentGetBuildUrls.Result.Url].BuildInfo.Add(buildInfo);
                        observer.OnNext(buildInfo);
                    }
                }

/*                //Wait for new builds and for inprogress builds to complete
                //There is no need to let this job complete until there has been an updated build,
                //complete and let the Observe.Retry() requery all builds (every 10th second)
                bool doContinue = true;
                while (doContinue)
                {
                    try
                    {
                        Thread.Sleep(10000);

                        foreach (var task in GetLastBuildInfo())
                        {
                            doContinue = task.Result.Timestamp <= timestamp;
                            if (!doContinue)
                            {
                                break;
                            }
                        }

                        foreach (var task in inProgress
                            .Where(b => b.Status == BuildInfo.BuildStatus.InProgress)
                         .Select(b => GetResponseAsync(FormatToGetJson(b.Url, true), cancellationToken).Result)
                            .Where(s => !string.IsNullOrEmpty(s)).ToArray())
                        {
                            JObject build = JObject.Parse(task.Result);
                            if (build["building"] == null || !build["building"].ToObject<bool>())                        }
                    }
                    catch
                    {
                        //This step will give exception for instance when switching repo, not critical
                        doContinue = false;
                    }
                }
                */

                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the observer is already stopped
            }
            catch (Exception ex)
            {
                //Cancelling a subtask is similar to cancelling this task
                if (ex.InnerException == null || !(ex.InnerException is OperationCanceledException))
                {
                    observer.OnError(ex);
                }
            }
        }

        private readonly string JenkinsTreeBuildInfo = "number,result,timestamp,url,actions[lastBuiltRevision[SHA1],totalCount,failCount,skipCount],building,duration";
        private static BuildInfo CreateBuildInfo(JObject buildDescription)
        {
            var idValue = buildDescription["number"].ToObject<string>();
            var statusValue = buildDescription["result"].ToObject<string>();
            var startDateTicks = buildDescription["timestamp"].ToObject<long>();
            //var displayName = buildDescription["fullDisplayName"].ToObject<string>();
            var webUrl = buildDescription["url"].ToObject<string>();

            var action = buildDescription["actions"];
            var commitHashList = new List<string>();
            string testResults = string.Empty;
            foreach (var element in action)
            {
                if (element["lastBuiltRevision"] != null)
                    commitHashList.Add(element["lastBuiltRevision"]["SHA1"].ToObject<string>());
                if (element["totalCount"] != null)
                {
                    int nbTests = element["totalCount"].ToObject<int>();
                    if (nbTests != 0)
                    {
                        int nbFailedTests = element["failCount"].ToObject<int>();
                        int nbSkippedTests = element["skipCount"].ToObject<int>();
                        testResults = $"{nbTests} tests ({nbFailedTests} failed, {nbSkippedTests} skipped)";
                    }
                }
            }

            var isRunning = buildDescription["building"].ToObject<bool>();
            long? buildDuration;
            if (isRunning)
            {
                buildDuration = null;
            }
            else
            {
                buildDuration = buildDescription["duration"].ToObject<long>();
            }

            var status = isRunning ? BuildInfo.BuildStatus.InProgress : ParseBuildStatus(statusValue);
            var statusText = status.ToString("G");
            var buildInfo = new BuildInfo
            {
                Id = idValue,
                StartDate = TimestampToDateTime(startDateTicks),
                Duration = buildDuration,
                Status = status,
                CommitHashList = commitHashList.ToArray(),
                Url = webUrl
            };
            var durationText = _buildDurationFormatter.Format(buildInfo.Duration);
            buildInfo.Description = $"#{idValue} {durationText} {testResults} {statusText}";
            return buildInfo;
        }

        public static DateTime TimestampToDateTime(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTime.Now.Kind).AddMilliseconds(timestamp);
        }

        private static AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        private static BuildInfo.BuildStatus ParseBuildStatus(string statusValue)
        {
            switch (statusValue)
            {
                case "SUCCESS":
                    return BuildInfo.BuildStatus.Success;
                case "FAILURE":
                    return BuildInfo.BuildStatus.Failure;
                case "UNSTABLE":
                    return BuildInfo.BuildStatus.Unstable;
                case "ABORTED":
                    return BuildInfo.BuildStatus.Stopped;
                default:
                    return BuildInfo.BuildStatus.Unknown;
            }
        }

        private Task<Stream> GetStreamAsync(string restServicePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _httpClient.GetAsync(restServicePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ContinueWith(
                    task => GetStreamFromHttpResponseAsync(task, restServicePath, cancellationToken),
                    cancellationToken,
                    TaskContinuationOptions.AttachedToParent,
                    TaskScheduler.Current)
                .Unwrap();
        }

        private Task<Stream> GetStreamFromHttpResponseAsync(Task<HttpResponseMessage> task, string restServicePath, CancellationToken cancellationToken)
        {
#if !__MonoCS__
            bool unauthorized = task.Status == TaskStatus.RanToCompletion &&
                                task.Result.StatusCode == HttpStatusCode.Unauthorized;

            if (task.IsFaulted || task.IsCanceled)
            {
                //No results for this task
                return null;
            }

            if (task.Result.IsSuccessStatusCode)
            {
                var httpContent = task.Result.Content;

                if (httpContent.Headers.ContentType.MediaType == "text/html")
                {
                    // Jenkins responds with an HTML login page when guest access is denied.
                    unauthorized = true;
                }
                else
                {
                    return httpContent.ReadAsStreamAsync();
                }
            }
            else if (task.Result.StatusCode == HttpStatusCode.NotFound)
            {
                //The url does not exist, no jobs to retrieve
                return null;
            }
            else if (task.Result.StatusCode == HttpStatusCode.Forbidden)
            {
                unauthorized = true;
            }

            if (unauthorized)
            {
                var buildServerCredentials = _buildServerWatcher.GetBuildServerCredentials(this, false);

                if (buildServerCredentials != null)
                {
                    UpdateHttpClientOptions(buildServerCredentials);

                    return GetStreamAsync(restServicePath, cancellationToken);
                }

                throw new OperationCanceledException(task.Result.ReasonPhrase);
            }

            throw new HttpRequestException(task.Result.ReasonPhrase);
#else
            return null;
#endif
        }

        private void UpdateHttpClientOptions(IBuildServerCredentials buildServerCredentials)
        {
            var useGuestAccess = buildServerCredentials == null || buildServerCredentials.UseGuestAccess;

            _httpClient.DefaultRequestHeaders.Authorization = useGuestAccess
                ? null : CreateBasicHeader(buildServerCredentials.Username, buildServerCredentials.Password);
        }

        private Task<string> GetResponseAsync(string relativePath, CancellationToken cancellationToken)
        {
            var getStreamTask = GetStreamAsync(relativePath, cancellationToken);

            return getStreamTask.ContinueWith(
                task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                        return string.Empty;
                    using (var responseStream = task.Result)
                    {
                        return new StreamReader(responseStream).ReadToEnd();
                    }
                },
                cancellationToken,
                TaskContinuationOptions.AttachedToParent,
                TaskScheduler.Current);
        }

        private string FormatToGetJson(string restServicePath, bool buildsInfo = false)
        {
            string buildTree = "lastBuild[timestamp]";
            int depth = 1;
            int postIndex = restServicePath.IndexOf('?');
            if (postIndex >= 0)
            {
                int endLen = restServicePath.Length - postIndex;
                if (restServicePath.EndsWith("/"))
                {
                    endLen--;
                }

                string post = restServicePath.Substring(postIndex, endLen);
                if (post == "?m")
                {
                    //Multi pipeline project
                    buildTree = "jobs[" + buildTree;
                    if (buildsInfo)
                    {
                        depth = 2;
                        buildTree += ",builds[" + JenkinsTreeBuildInfo + "]";
                    }
                    buildTree += "]";
                }
                else
                {
                    //user defined format (will likely require changes in the code)
                    buildTree = post;
                }

                restServicePath = restServicePath.Substring(0, postIndex);
            }
            else
            {
                //Freestyle project
                if (buildsInfo)
                {
                    buildTree += ",builds[" + JenkinsTreeBuildInfo + "]";
                }
            }

            if (!restServicePath.EndsWith("/"))
                restServicePath += "/";
            restServicePath += "api/json?depth=" + depth + "&tree=" + buildTree;
            return restServicePath;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
