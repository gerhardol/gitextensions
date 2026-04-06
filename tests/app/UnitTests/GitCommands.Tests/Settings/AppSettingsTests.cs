using System.CodeDom.Compiler;
using System.Reflection;
using FluentAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitCommandsTests.Settings;

[TestFixture]
internal sealed class AppSettingsTests
{
    private const string SettingsFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?><dictionary />";

    [TestCase(null, "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("\t", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("33.33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("33.33.33.33", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("a", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("5", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("v4.5", "https://git-extensions-documentation.readthedocs.org/en/main/")]
    [TestCase("4.5", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("4.5.", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("4.5.0", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("4.5.2", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("4.5.2.1", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("4.5.2x", "https://git-extensions-documentation.readthedocs.org/en/release-4.5/")]
    [TestCase("40.501.123", "https://git-extensions-documentation.readthedocs.org/en/release-40.501/")]
    public void SetDocumentationBaseUrl_should_currectly_append_version(string version, string expected)
    {
        AppSettings.GetTestAccessor().ResetDocumentationBaseUrl();

        AppSettings.SetDocumentationBaseUrl(version);
        AppSettings.DocumentationBaseUrl.Should().Be(expected);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
#pragma warning disable IDE0060 // Remove unused parameter
    public void Should_return_default_value(PropertyInfo property, object value, object defaultValue, bool isISetting)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        // Arrange
        object root = null;

        if (isISetting)
        {
            root = property.GetValue(null);

            property = property.PropertyType
                .GetProperty(nameof(ISetting<>.Value));
        }

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");
        tempFiles.AddFile(filePath + ".backup", keepFile: false);

        File.WriteAllText(filePath, SettingsFileContent);

        using GitExtSettingsCache cache = GitExtSettingsCache.Create(filePath);
        DistributedSettings container = new(lowerPriority: null, cache, SettingLevel.Unknown);
        object storedValue = null;

        // Act
        AppSettings.UsingContainer(container, () =>
        {
            storedValue = property.GetValue(root);
        });

        // Assert
        ClassicAssert.That(storedValue, Is.EqualTo(defaultValue));
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Should_save_value(PropertyInfo property, object value, object defaultValue, bool isISetting)
    {
        // Arrange
        object root = null;

        if (isISetting)
        {
            root = property.GetValue(null);

            property = property.PropertyType
                .GetProperty(nameof(ISetting<>.Value));
        }

        using TempFileCollection tempFiles = new();
        string filePath = tempFiles.AddExtension(".settings");
        tempFiles.AddFile(filePath + ".backup", keepFile: false);

        File.WriteAllText(filePath, SettingsFileContent);

        using GitExtSettingsCache cache = GitExtSettingsCache.Create(filePath);
        DistributedSettings container = new(lowerPriority: null, cache, SettingLevel.Unknown);
        object storedValue = null;

        // Act
        AppSettings.UsingContainer(container, () =>
        {
            property.SetValue(root, value);

            storedValue = property.GetValue(root);
        });

        // Assert
        if (Type.GetTypeCode(property.PropertyType) == TypeCode.String)
        {
            if (isISetting)
            {
                ClassicAssert.That(storedValue, Is.EqualTo(value ?? string.Empty));
            }
            else
            {
                ClassicAssert.That(storedValue, Is.EqualTo(value ?? defaultValue));
            }
        }
        else if (Type.GetTypeCode(property.PropertyType) == TypeCode.DateTime)
        {
            // We keep only the date
            ClassicAssert.That(storedValue, Is.EqualTo(((DateTime)value).Date));
        }
        else
        {
            ClassicAssert.That(storedValue, Is.EqualTo(value));
        }
    }

    #region Test Cases

    private static IEnumerable<object[]> TestCases()
    {
        foreach ((PropertyInfo property, object defaultValue, bool isNullable, bool isISetting) in PropertyInfos())
        {
            if (isNullable)
            {
                yield return new object[] { property, null, defaultValue, isISetting };
            }

            Type? propertyType = property.PropertyType;
            if (isISetting)
            {
                propertyType = propertyType.GetProperty(nameof(ISetting<>.Value))?.PropertyType;
            }

            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            Validates.NotNull(propertyType);

            foreach (object value in Values())
            {
                Type valueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
                if (valueType == propertyType)
                {
                    yield return new object[] { property, value, defaultValue, isISetting };
                }
            }
        }

        static IEnumerable<(PropertyInfo property, object defaultValue, bool isNullable, bool isISetting)> PropertyInfos()
        {
            Dictionary<string, PropertyInfo> properties = typeof(AppSettings).GetProperties()
                .ToDictionary(x => x.Name, x => x);

            const bool isISetting = true;
            const bool isNoISetting = false;
            const bool isNotNullable = false;

            yield return (properties[nameof(AppSettings.TelemetryEnabled)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.AutoNormaliseBranchName)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FileStatusFindInFilesGitGrepTypeIndex)], 1, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FileStatusMergeSingleItemWithFolder)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FileStatusShowGroupNodesInFlatList)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberAmendCommitState)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.StashKeepIndex)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmStashDrop)], false, isNotNullable, isNoISetting);
            yield return (properties[nameof(AppSettings.ApplyPatchIgnoreWhitespace)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ApplyPatchSignOff)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseHistogramDiffAlgorithm)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseGitColoring)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ReverseGitColoring)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowErrorsWhenStagingFiles)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.EnsureCommitMessageSecondLineEmpty)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.LastCommitMessage)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.CommitDialogNumberOfPreviousMessages)], 6, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitDialogSelectStagedOnEnterMessage)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitDialogShowOnlyMyMessages)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowCommitAndPush)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowResetWorkTreeChanges)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowResetAllChanges)], true, isNotNullable, isISetting);

            yield return (properties[nameof(AppSettings.ShowConEmuTab)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ConEmuStyle)], "Default", true, isISetting);
            yield return (properties[nameof(AppSettings.ConEmuTerminal)], "bash", true, isISetting);
            yield return (properties[nameof(AppSettings.OutputHistoryDepth)], 20, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.OutputHistoryPanelVisible)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowOutputHistoryAsTab)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseBrowseForFileHistory)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseDiffViewerForBlame)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowGpgInformation)], true, isNotNullable, isISetting);

            yield return (properties[nameof(AppSettings.MessageEditorWordWrap)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSplitViewLayout)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ProvideAutocompletion)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.TruncatePathMethod)], TruncatePathMethod.None, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowGitStatusInBrowseToolbar)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowGitStatusForArtificialCommits)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RevisionSortOrder)], RevisionSortOrder.GitDefault, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesLocal)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CheckForUncommittedChangesInCheckoutBranch)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AlwaysShowCheckoutBranchDlg)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitAndPushForcedWhenAmend)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesRemote)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitInfoShowContainedInTags)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitInfoShowTagThisCommitDerivesFrom)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AvatarImageCacheDays)], 13, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAuthorAvatarInCommitInfo)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AvatarProvider)], AvatarProvider.None, isNotNullable, isNoISetting);
            yield return (properties[nameof(AppSettings.Translation)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.UserProfileHomeDir)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CustomHomeDir)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.EnableAutoScale)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CloseCommitDialogAfterCommit)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CloseCommitDialogAfterLastCommit)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RefreshArtificialCommitOnApplicationActivated)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.StageInSuperprojectAfterCommit)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FollowRenamesInFileHistory)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FollowRenamesInFileHistoryExactOnly)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FullHistoryInFileHistory)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.SimplifyMergesInFileHistory)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.LoadFileHistoryOnShow)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.LoadBlameOnShow)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DetectCopyInFileOnBlame)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DetectCopyInAllOnBlame)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.IgnoreWhitespaceOnBlame)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.OpenSubmoduleDiffInSeparateWindow)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RevisionGraphShowArtificialCommits)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RevisionGraphDrawAlternateBackColor)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RevisionGraphDrawNonRelativesGray)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RevisionGraphDrawNonRelativesTextGray)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DefaultPullAction)], GitPullAction.Merge, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FormPullAction)], GitPullAction.Merge, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AutoStash)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RebaseAutoStash)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CheckoutBranchAction)], LocalChangesAction.DontChange, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CheckoutOtherBranchAfterReset)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseDefaultCheckoutBranchAction)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontShowHelpImages)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AlwaysShowAdvOpt)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmAmend)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmCommitIfNoBranch)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ConfirmBranchCheckout)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AutoPopStashAfterPull)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.AutoPopStashAfterCheckoutBranch)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.AutoPullOnPushRejectedAction)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmPushNewBranch)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmAddTrackingRef)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmCommitAfterConflictsResolved)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmSecondAbortConfirmation)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmRebase)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmResolveConflicts)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmUndoLastCommit)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmFetchAndPruneAll)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmSwitchWorktree)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.IncludeUntrackedFilesInAutoStash)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.IncludeUntrackedFilesInManualStash)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowRemoteBranches)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowReflogReferences)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSuperprojectTags)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSuperprojectBranches)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSuperprojectRemoteBranches)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UpdateSubmodulesOnCheckout)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.DontConfirmUpdateSubmodulesOnCheckout)], null, true, isISetting);
            yield return (properties[nameof(AppSettings.ShowGitCommandLine)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowStashCount)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAheadBehindData)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSubmoduleStatus)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RelativeDate)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowGitNotes)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAnnotatedTagsMessages)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.HideMergeCommits)], false, isNotNullable, isNoISetting);
            yield return (properties[nameof(AppSettings.ShowTags)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowRevisionGridGraphColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowRevisionGridTooltips)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAuthorAvatarColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAuthorNameColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowDateColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowObjectIdColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowBuildStatusIconColumn)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowBuildStatusTextColumn)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAuthorDate)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CloseProcessDialog)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowCurrentBranchOnly)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSimplifyByDecoration)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BranchFilterEnabled)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowOnlyFirstParent)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitDialogSelectionFilter)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DefaultCloneDestinationPath)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.RevisionGridQuickSearchTimeout)], 4000, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.MaxRevisionGraphCommits)], 100000, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowDiffForAllParents)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowFindInCommitFilesGitGrep)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowAvailableDiffTools)], true, isNotNullable, isNoISetting);
            yield return (properties[nameof(AppSettings.DiffVerticalRulerPosition)], 0, isNotNullable, isNoISetting);
            yield return (properties[nameof(AppSettings.GitGrepUserArguments)], "", isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.GitGrepIgnoreCase)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.GitGrepMatchWholeWord)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RecentWorkingDir)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.StartWithRecentWorkingDir)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AutoStartPageant)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.MarkIllFormedLinesInCommitMsg)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseSystemVisualStyle)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.MulticolorBranches)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.HighlightAuthoredRevisions)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.FillRefLabels)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.MergeGraphLanesHavingCommonParent)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RenderGraphWithDiagonals)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.StraightenGraphDiagonals)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.StraightenGraphSegmentsLimit)], 80, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.LastFormatPatchDir)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.IgnoreWhitespaceKind)], IgnoreWhitespaceKind.None, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberIgnoreWhiteSpacePreference)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowNonPrintingChars)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberShowNonPrintingCharsPreference)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowEntireFile)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberShowEntireFilePreference)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DiffDisplayAppearance)], DiffDisplayAppearance.Patch, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberDiffDisplayAppearance)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberNumberOfContextLines)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowSyntaxHighlightingInDiff)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RememberShowSyntaxHighlightingInDiff)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowRepoCurrentBranch)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.OwnScripts)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.RecursiveSubmodules)], 1, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShorteningRecentRepoPathStrategy)], ShorteningRecentRepoPathStrategy.None, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.MaxTopRepositories)], 0, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RecentRepositoriesHistorySize)], 30, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.HideTopRepositoriesFromRecentList)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RecentReposComboMinWidth)], 0, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.SerializedHotkeys)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.SortTopRepos)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.SortRecentRepos)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DontCommitMerge)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationMaxCntCharsFirstLine)], 0, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationMaxCntCharsPerLine)], 0, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationSecondLineMustBeEmpty)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationIndentAfterFirstLine)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationAutoWrap)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitValidationRegEx)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.CommitTemplates)], string.Empty, true, isISetting);
            yield return (properties[nameof(AppSettings.CreateLocalBranchForRemote)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseFormCommitMessage)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CommitAutomaticallyAfterCherryPick)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AddCommitReferenceToCherryPick)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.LastUpdateCheck)], default(DateTime), false, isISetting);
            yield return (properties[nameof(AppSettings.CheckForUpdates)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.CheckForReleaseCandidates)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.OmitUninterestingDiff)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UseConsoleEmulatorForCommands)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RefsSortBy)], GitRefsSortBy.Default, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RefsSortOrder)], GitRefsSortOrder.Descending, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.DiffListSorting)], DiffListSortType.FilePath, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowBranches)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowRemotes)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowWorktrees)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowTags)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowStashes)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeShowSubmodules)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeBranchesIndex)], 0, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeRemotesIndex)], 1, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeWorktreesIndex)], 2, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeTagsIndex)], 3, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeSubmodulesIndex)], 4, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.RepoObjectsTreeStashesIndex)], 5, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameDisplayAuthorFirst)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowAuthor)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowAuthorDate)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowAuthorTime)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowLineNumbers)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowOriginalFilePath)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.BlameShowAuthorAvatar)], true, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AutomaticContinuousScroll)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.AutomaticContinuousScrollDelay)], 600, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.IsEditorSettingsMigrated)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.ShowProcessDialogPasswordInput)], false, isNotNullable, isISetting);
            yield return (properties[nameof(AppSettings.UninformativeRepoNameRegex)], "app|(repo(sitory)?)", isNotNullable, isISetting);
        }

        static IEnumerable<object> Values()
        {
            yield return string.Empty;
            yield return " ";
            yield return "0";

            yield return false;
            yield return true;

            yield return char.MinValue;
            yield return char.MaxValue;
            yield return ' ';
            yield return '0';

            yield return byte.MinValue;
            yield return byte.MaxValue;

            yield return int.MinValue;
            yield return int.MaxValue;
            yield return 0;
            yield return 1;
            yield return -1;

            yield return float.MinValue;
            yield return float.MaxValue;
            yield return float.Epsilon;
            yield return float.PositiveInfinity;
            yield return float.NegativeInfinity;
            yield return float.NaN;
            yield return 0f;
            yield return 1f;
            yield return -1f;

            yield return double.MinValue;
            yield return double.MaxValue;
            yield return double.Epsilon;
            yield return double.PositiveInfinity;
            yield return double.NegativeInfinity;
            yield return double.NaN;
            yield return 0d;
            yield return 1d;
            yield return -1d;

            yield return decimal.MinValue;
            yield return decimal.MaxValue;
            yield return decimal.Zero;
            yield return decimal.One;
            yield return decimal.MinusOne;

            yield return DateTime.MinValue;
            yield return DateTime.MaxValue;
            yield return DateTime.Today;

            Type[] enumTypes =
            [
                typeof(TruncatePathMethod),
                typeof(AvatarProvider),
                typeof(GitPullAction),
                typeof(LocalChangesAction),
                typeof(IgnoreWhitespaceKind),
                typeof(ShorteningRecentRepoPathStrategy),
                typeof(GitRefsSortBy),
                typeof(GitRefsSortOrder),
                typeof(DiffListSortType),
                typeof(RevisionSortOrder),
            ];

            foreach (Type enumType in enumTypes)
            {
                foreach (object enumValue in Enum.GetValues(enumType))
                {
                    yield return enumValue;
                }
            }
        }
    }

    #endregion Test Cases
}
