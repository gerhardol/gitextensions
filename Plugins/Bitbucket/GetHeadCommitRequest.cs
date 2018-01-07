using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    class BBCommit
    {
        public static BBCommit Parse(JObject json)
        {
            return new BBCommit
            {
                Hash = json["id"].ToString(),
                Message = json["message"].ToString(),
                AuthorName = json["author"]["name"].ToString(),
                IsMerge = ((JArray)json["parents"]).Count > 1
            };
        }
        public string Hash { get; set; }
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public bool IsMerge { get; set; }
    }
    class GetHeadCommitRequest : BitbucketRequestBase<BBCommit>
    {
        private readonly BBRepository _repo;
        private readonly string _branch;

        public GetHeadCommitRequest(BBRepository bbRepository, string branchName, Settings settings)
            : base(settings)
        {
            _repo = bbRepository;
            _branch = branchName;
        }

        protected override object RequestBody
        {
            get { return null; }
        }

        protected override Method RequestMethod
        {
            get { return Method.GET; }
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/projects/{0}/repos/{1}/commits/refs/heads/{2}",
                                     _repo.ProjectKey, _repo.RepoName, _branch);
            }
        }

        protected override BBCommit ParseResponse(JObject json)
        {
            return BBCommit.Parse(json);
        }
    }
}
