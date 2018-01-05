using System;
using System.Diagnostics;

namespace GitCommands.Git
{
    public interface IRevisionDiffProvider
    {
        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="revA">The first revision</param>
        /// <param name="revB">The second "current" revision</param>
        /// <returns></returns>
        string Get(string revA, string revB);

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="revA">The first revision</param>
        /// <param name="revB">The second "current" revision</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        /// <returns></returns>
        string Get(string revA, string revB, string fileName, string oldFileName, bool isTracked);
    }

    /// <summary>
    /// Translate GitRevision including artificial commits to diff options
    /// Closely related to GitRevision.cs 
    /// </summary>
    public sealed class RevisionDiffProvider : IRevisionDiffProvider
    {
        // This is an instance class to not have static dependencies in GitModule
        private static readonly string StagedOpt = "--cached";

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="revA">The first revision</param>
        /// <param name="revB">The second "current" revision</param>
        /// <returns></returns>
        public string Get(string revA, string revB)
        {
            return GetInternal(revA, revB);
        }

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="revA">The first revision</param>
        /// <param name="revB">The second "current" revision</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        /// <returns></returns>
        public string Get(string revA, string revB, string fileName, string oldFileName, bool isTracked)
        {
            return GetInternal(revA, revB, fileName, oldFileName, isTracked);
        }

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="revA">The first revision</param>
        /// <param name="revB">The second "current" revision</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        /// <returns></returns>
        private string GetInternal(string revA, string revB, string fileName = null, string oldFileName = null, bool isTracked = true)
        {
            string extra = string.Empty;
            revA = ArtificialToDiffOptions(revA);
            revB = ArtificialToDiffOptions(revB);

            //Note: As artificial are options, diff unstage..unstage and 
            // stage..stage will show output, different from e.g. HEAD..HEAD
            //Diff-to-itself is not always disabled or is transient why this is not handled as error in release builds
            Debug.Assert(!(revA == revB && (revA.IsNullOrEmpty() || revA == StagedOpt)),
                "Unexpectedly two identical artificial revisions to diff: " + revA +
                ". This will be displayed as diff to HEAD, not an identical diff.");

            //As empty (unstaged) and --cached (staged) are options (not revisions),
            // order must be preserved with -R
            if (revA != revB && (revA.IsNullOrEmpty() ||
                               revA == StagedOpt && !revB.IsNullOrEmpty()))
            {
                extra = "-R";
            }

            //Special case: Remove options comparing unstaged-staged
            if (revA.IsNullOrEmpty() && revB == StagedOpt ||
                revA == StagedOpt && revB.IsNullOrEmpty())
            {
                revA = revB = string.Empty;
            }

            //Reorder options - not strictly required
            if (revB == StagedOpt)
            {
                extra += " " + StagedOpt;
                revB = String.Empty;
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                extra = string.Join(" ", extra, revA, revB);
            }
            else
            {
                //Untracked files can only be compared to /dev/null
                //The UI should normall only allow this for unstaged to staged, but it can be included in multi selections
                if (!isTracked)
                {
                    extra += " --no-index";
                    oldFileName = fileName;
                    fileName = "/dev/null";
                }
                else
                {
                    extra += " " + revA + " " + revB;
                }

                extra += " -- " + fileName.QuoteNE() + " " + oldFileName.QuoteNE();
            }

            return extra.Trim();
        }

        /// <summary>
        /// Translate the revision string to an option string
        /// Artificial "commits" are options, handle aliases too
        /// (order and handling of empty arguments is not handled here)
        /// </summary>
        /// <param name="rev"></param>
        /// <returns></returns>
        private string ArtificialToDiffOptions(string rev)
        {
            if (rev.IsNullOrEmpty() || rev == GitRevision.UnstagedGuid)
            {
                rev = string.Empty;
            }
            else if (rev == "^" || rev == GitRevision.UnstagedGuid + "^" || rev == GitRevision.IndexGuid)
            {
                rev = StagedOpt;
            }
            else
            {
                //Normal commit
                if (rev == "^^" || rev == GitRevision.UnstagedGuid + "^^" || rev == GitRevision.IndexGuid + "^")
                {
                    rev = "HEAD";
                }
                else if (rev == "^^^" || rev == GitRevision.UnstagedGuid + "^^^" || rev == GitRevision.IndexGuid + "^^")
                {
                    rev = "HEAD^";
                }

                rev = rev.QuoteNE();
            }

            return rev;
        }
    }
}
