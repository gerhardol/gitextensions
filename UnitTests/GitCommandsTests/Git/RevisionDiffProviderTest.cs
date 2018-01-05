using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class RevisionDiffProviderTest
    {
        //See RevisionDiffProvider.ArtificialToDiffOptions() for possible "aliases" for artificial commits
        //All variants are not tested in all situations

        private RevisionDiffProvider _revisionDiffProvider;

        [SetUp]
        public void Setup()
        {
            _revisionDiffProvider = new RevisionDiffProvider();
        }

#if !DEBUG
        //Testcases that should assert in debug; should not occur but undefined behavior that should be blocked in GUI
        //Cannot compare unstaged to unstaged or staged to staged but give predictive output in release builds

        //Two empty parameters will compare working dir to index
        [TestCase(null)]
        [TestCase("")]
        [TestCase(GitRevision.UnstagedGuid)]
        public void RevisionDiffProvider_should_return_empty_if_To_is_UnstagedGuid(string revA)
        {
            _revisionDiffProvider.Get(revA, GitRevision.UnstagedGuid).Should().BeEmpty();
        }

        //Two staged revisions gives duplicated options, no reason to clean
        [TestCase("^")]
        [TestCase(GitRevision.IndexGuid)]
        public void RevisionDiffProvider_should_return_cached_if_both_IndexGuid(string revA)
        {
            _revisionDiffProvider.Get(revA, GitRevision.IndexGuid).Should().Be("--cached --cached");
        }
#endif

        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid)]
        [TestCase("^", "")]
        [TestCase(GitRevision.IndexGuid, null)]
        public void RevisionDiffProvider_staged_to_unstaged(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().BeEmpty();
        }

        [TestCase(GitRevision.UnstagedGuid, GitRevision.IndexGuid)]
        [TestCase("", "^")]
        public void RevisionDiffProvider_unstaged_to_staged(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().Be("-R");
        }

        [TestCase(GitRevision.UnstagedGuid + "^^")]
        [TestCase(GitRevision.IndexGuid + "^")]
        [TestCase("HEAD")]
        public void RevisionDiffProvider_head_to_unstaged(string revA)
        {
            _revisionDiffProvider.Get(revA, GitRevision.UnstagedGuid).Should().Be("\"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid + "^", "^")]
        [TestCase("HEAD", GitRevision.IndexGuid)]
        public void RevisionDiffProvider_head_to_staged(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().Be("--cached \"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid, "HEAD")]
        public void RevisionDiffProvider_staged_to_head(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().Be("-R --cached \"HEAD\"");
        }

        [TestCase("HEAD", "123456789")]
        public void RevisionDiffProvider_normal1(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().Be("\"HEAD\" \"123456789\"");
        }

        [TestCase("123456789", "HEAD")]
        public void RevisionDiffProvider_normal2(string revA, string revB)
        {
            _revisionDiffProvider.Get(revA, revB).Should().Be("\"123456789\" \"HEAD\"");
        }

        //Standard usage when filename is included
        [TestCase("123456789", GitRevision.UnstagedGuid, "a.txt", null, true)]
        public void RevisionDiffProvider_fileName_tracked1(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().Be("\"123456789\"  -- \"a.txt\"");
        }

        //If fileName is null, ignore oldFileName and tracked
        [TestCase("123456789", "HEAD", null, "b.txt", true)]
        public void RevisionDiffProvider_fileName_nul_oldname(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().Be("\"123456789\" \"HEAD\"");
        }

        //Include old filename if is included
        [TestCase("123456789", "234567890", "a.txt", "b.txt", true)]
        public void RevisionDiffProvider_fileName_oldfilename(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().Be("\"123456789\" \"234567890\" -- \"a.txt\" \"b.txt\"");
        }

        //normal testcase when untracked is set
        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid, "a.txt", null, false)]
        public void RevisionDiffProvider_fileName_untracked1(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }

        //If fileName is null, ignore oldFileName and tracked
        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid, null, "b.txt", false)]
        public void RevisionDiffProvider_fileNameUntracked2(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().BeEmpty();
        }

        //Ignore revisions for untracked
        [TestCase("123456789", "234567890", "a.txt", "b.txt", false)]
        public void RevisionDiffProvider_fileNameUntracked3(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(revA, revB, fileName, oldFileName, isTracked).Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }
    }
}

