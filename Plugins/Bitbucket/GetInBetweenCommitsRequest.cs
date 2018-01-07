using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    class GetInBetweenCommitsRequest : BitbucketRequestBase<List<BBCommit>>
    {
        private readonly BBRepository _sourceRepo;
        private readonly BBRepository _targetRepo;
        private readonly BBCommit _sourceBbCommit;
        private readonly BBCommit _targetBbCommit;

        public GetInBetweenCommitsRequest(BBRepository sourceRepo, BBRepository targetRepo,
            BBCommit sourceBbCommit, BBCommit targetBbCommit,Settings settings)
            : base(settings)
        {
            _sourceRepo = sourceRepo;
            _targetRepo = targetRepo;
            _sourceBbCommit = sourceBbCommit;
            _targetBbCommit = targetBbCommit;
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
                return string.Format(
                    "/projects/{0}/repos/{1}/commits?until={2}&since={3}&secondaryRepositoryId={4}&start=0&limit=10",
                    _sourceRepo.ProjectKey, _sourceRepo.RepoName, 
                    _sourceBbCommit.Hash, _targetBbCommit.Hash, _targetRepo.Id);
            }
        }

        protected override List<BBCommit> ParseResponse(JObject json)
        {
            var result = new List<BBCommit>();
            foreach(JObject commit in json["values"])
            {
                result.Add(BBCommit.Parse(commit));
            }
            return result;
        }
    }
}
