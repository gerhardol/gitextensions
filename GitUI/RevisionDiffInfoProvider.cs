using System.Collections.Generic;
using GitCommands;

namespace GitUI
{
    public sealed class RevisionDiffInfoProvider
    {
        /// <summary>
        /// One row selected:
        /// B - Selected row
        /// A - B's parent
        ///
        /// Two rows selected:
        /// A - first selected row
        /// B - second selected row
        /// </summary>
        public static string Get(IList<GitRevision> revisions, RevisionDiffKind diffKind,
            out string extraDiffArgs, out string revA, out string revB)
        {
            //Note: Order in revisions is that first clicked is last in array
            string error = "";
            //Detect rename and copy
            extraDiffArgs = "-M -C";

            if (revisions == null)
            {
                error = "Unexpected null revision argument to difftool";
                revA = null;
                revB = null;
            }
            else if (revisions.Count == 0 || revisions.Count > 2)
            {
                error = "Unexpected number of arguments to difftool: " + revisions.Count;
                revA = null;
                revB = null;
            }
            else if (revisions[0] == null || revisions.Count > 1 && revisions[1] == null)
            {
                error = "Unexpected single null argument to difftool";
                revA = null;
                revB = null;
            }
            else if (diffKind == RevisionDiffKind.DiffAB)
            {
                if (revisions.Count == 1)
                {
                    revA = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                }
                else
                {
                    revA = revisions[1].Guid;
                }
                revB = revisions[0].Guid;
            }
            else
            {
                //Second revision is always local 
                revB = null;

                if (diffKind == RevisionDiffKind.DiffBLocal)
                {
                    revA = revisions[0].Guid;
                }
                else if (diffKind == RevisionDiffKind.DiffBParentLocal)
                {
                    revA = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                }
                else
                {
                    revA = revisions[0].Guid;
                    if (revisions.Count == 1)
                    {
                        if (diffKind == RevisionDiffKind.DiffALocal)
                        {
                            revA = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                        }
                        else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                        {
                            revA = (revisions[0].FirstParentGuid ?? revisions[0].Guid + '^') + "^";
                        }
                        else
                        {
                            error = "Unexpected arg to difftool with one revision: " + diffKind;
                        }
                    }
                    else
                    {
                        if (diffKind == RevisionDiffKind.DiffALocal)
                        {
                            revA = revisions[1].Guid;
                        }
                        else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                        {
                            revA = revisions[1].FirstParentGuid ?? revisions[1].Guid + '^';
                        }
                        else
                        {
                            error = "Unexpected arg to difftool with two revisions: " + diffKind;
                        }
                    }
                }
            }
            return error;
        }
    }
}