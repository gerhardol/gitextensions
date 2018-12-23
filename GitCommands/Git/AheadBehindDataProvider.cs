using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IAheadBehindDataProvider
    {
        IDictionary<string, AheadBehindData> GetData(string branchName = "*");
    }

    public class AheadBehindDataProvider : IAheadBehindDataProvider
    {
        private readonly Func<IExecutable> _getGitExecutable;

        private readonly Regex _aheadBehindRegEx =
            new Regex(@"^\[(ahead (?<ahead>\d+))?(, )?(behind (?<behind>\d+))?\] (?<branch>.*)$",
                RegexOptions.Compiled | RegexOptions.Multiline);

        public AheadBehindDataProvider(Func<IExecutable> getGitExecutable)
        {
            _getGitExecutable = getGitExecutable;
        }

        public IDictionary<string, AheadBehindData> GetData(string branchName = "*")
        {
            return GetData(null, branchName);
        }

        // This method is required to facilitate unit tests
        private IDictionary<string, AheadBehindData> GetData(Encoding encoding, string branchName = "*")
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                throw new ArgumentException(nameof(branchName));
            }

            if (branchName == "(no branch)")
            {
                return null;
            }

            var aheadBehindGitCommand = new GitArgumentBuilder("for-each-ref")
            {
                "--format=\"%(push:track) %(refname:short)\"",
                "refs/heads/" + branchName
            };

            var result = GetGitExecutable().GetOutput(aheadBehindGitCommand, outputEncoding: encoding);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            var matches = _aheadBehindRegEx.Matches(result);
            if (matches.Count < 1)
            {
                return null;
            }

            var aheadBehindForBranchesData = new Dictionary<string, AheadBehindData>();
            foreach (Match match in matches)
            {
                aheadBehindForBranchesData.Add(match.Groups["branch"].Value,
                    new AheadBehindData
                    {
                        Branch = match.Groups["branch"].Value,
                        AheadCount = match.Groups["ahead"].Value,
                        BehindCount = match.Groups["behind"].Value
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

        public readonly struct TestAccessor
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
