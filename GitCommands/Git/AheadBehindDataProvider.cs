using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitExtUtils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IAheadBehindDataProvider
    {
        IDictionary<string, AheadBehindData> GetData(string branchName = "");
    }

    public class AheadBehindDataProvider : IAheadBehindDataProvider
    {
        private readonly Func<IExecutable> _getGitExecutable;

        // Parse info about remote branches, see below for explanation
        private readonly Regex _aheadBehindRegEx =
            new Regex(
                @"^((?<gone_p>gone)|((ahead\s(?<ahead_p>\d+))?(,\s)?(behind\s(?<behind_p>\d+))?)|.*?)::
                   ((?<gone_u>gone)|((ahead\s(?<ahead_u>\d+))?(,\s)?(behind\s(?<behind_u>\d+))?)|.*?)::
                   (?<remote_p>.*?)::(?<remote_u>.*?)::(?<branch>.*)$",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
        private readonly string _refFormat = @"%(push:track,nobracket)::%(upstream:track,nobracket)::%(push)::%(upstream)::%(refname:short)";

        public AheadBehindDataProvider(Func<IExecutable> getGitExecutable)
        {
            _getGitExecutable = getGitExecutable;
        }

        [CanBeNull]
        public IDictionary<string, AheadBehindData> GetData(string branchName = "")
        {
            if (!AppSettings.ShowAheadBehindData)
            {
                return null;
            }

            return GetData(null, branchName);
        }

        // This method is required to facilitate unit tests
        private IDictionary<string, AheadBehindData> GetData(Encoding encoding, string branchName = "")
        {
            if (branchName == null)
            {
                throw new ArgumentException(nameof(branchName));
            }

            if (branchName == DetachedHeadParser.DetachedBranch)
            {
                return null;
            }

            var aheadBehindGitCommand = new GitArgumentBuilder("for-each-ref")
            {
                $"--color=never",
                $"--format=\"{_refFormat}\"",
                "refs/heads/" + branchName
            };

            var result = GetGitExecutable().GetOutput(aheadBehindGitCommand, outputEncoding: encoding);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            var matches = _aheadBehindRegEx.Matches(result);
            var aheadBehindForBranchesData = new Dictionary<string, AheadBehindData>();
            foreach (Match match in matches)
            {
                var branch = match.Groups["branch"].Value;
                var remoteRef = (match.Groups["remote_p"].Success && !string.IsNullOrEmpty(match.Groups["remote_p"].Value))
                            ? match.Groups["remote_p"].Value
                            : match.Groups["remote_u"].Value;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(remoteRef))
                {
                    continue;
                }

                aheadBehindForBranchesData.Add(match.Groups["branch"].Value,
                    new AheadBehindData
                    {
                        // The information is displayed in the push button, so the push info is preferred (may differ from upstream)
                        Branch = branch,
                        AheadCount =

                            // Prefer push to upstream for the count
                            match.Groups["ahead_p"].Success
                            ? match.Groups["ahead_p"].Value
                            : match.Groups["ahead_u"].Success
                            ? match.Groups["ahead_u"].Value

                            // No information about the remote branch, it is gone
                            : match.Groups["gone_p"].Success || match.Groups["gone_u"].Success
                            ? AheadBehindData.Gone

                            // A remote exists, but "track" does not display the count if ahead/behind match
                            : "0",

                        // Behind do not track '0' or 'gone', only in Ahead
                        BehindCount = match.Groups["behind_p"].Success ? match.Groups["behind_p"].Value : match.Groups["behind_u"].Value
                    });
            }

            return aheadBehindForBranchesData;
        }

        [NotNull]
        private IExecutable GetGitExecutable()
        {
            var executable = _getGitExecutable();

            if (executable == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IExecutable)}");
            }

            return executable;
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly AheadBehindDataProvider _provider;

            public TestAccessor(AheadBehindDataProvider provider)
            {
                _provider = provider;
            }

            public IDictionary<string, AheadBehindData> GetData(Encoding encoding, string branchName) => _provider.GetData(encoding, branchName);
        }
    }
}
