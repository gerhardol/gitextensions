namespace GitUI.UserControls.RevisionGrid
{
    partial class FormRevisionFilter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Ok = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_CommitsLimit = new System.Windows.Forms.NumericUpDown();
            this.Message = new System.Windows.Forms.TextBox();
            this.DiffContent = new System.Windows.Forms.TextBox();
            this.RawLog = new System.Windows.Forms.TextBox();
            this.Author = new System.Windows.Forms.TextBox();
            this.Since = new System.Windows.Forms.DateTimePicker();
            this._NO_TRANSLATE_lblSince = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblUntil = new System.Windows.Forms.Label();
            this.Until = new System.Windows.Forms.DateTimePicker();
            this._NO_TRANSLATE_lblAuthor = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblCommitter = new System.Windows.Forms.Label();
            this.Committer = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_lblMessage = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblDiffContent = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblRawLog = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblLimit = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_lblIgnoreCase = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.SinceCheck = new System.Windows.Forms.CheckBox();
            this.CheckUntil = new System.Windows.Forms.CheckBox();
            this.AuthorCheck = new System.Windows.Forms.CheckBox();
            this.CommitterCheck = new System.Windows.Forms.CheckBox();
            this.MessageCheck = new System.Windows.Forms.CheckBox();
            this.DiffContentCheck = new System.Windows.Forms.CheckBox();
            this.RawLogCheck = new System.Windows.Forms.CheckBox();
            this.IgnoreCase = new System.Windows.Forms.CheckBox();
            this.CommitsLimitCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblPathFilter = new System.Windows.Forms.Label();
            this.PathFilterCheck = new System.Windows.Forms.CheckBox();
            this.PathFilter = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_lblBranches = new System.Windows.Forms.Label();
            this.BranchFilterCheck = new System.Windows.Forms.CheckBox();
            this.BranchFilter = new System.Windows.Forms.TextBox();
            this.ReflogCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblReflogCheck = new System.Windows.Forms.Label();
            this.CurrentBranchOnlyCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck = new System.Windows.Forms.Label();
            this.ShowOnlyFirstParentCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblShowOnlyFirstParent = new System.Windows.Forms.Label();
            this.ShowMergeCommitsCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblShowMergeCommits = new System.Windows.Forms.Label();
            this.SimplifyByDecorationCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblSimplifyByDecoration = new System.Windows.Forms.Label();
            this.FullHistoryCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblFullHistory = new System.Windows.Forms.Label();
            this.SimplifyMergesCheck = new System.Windows.Forms.CheckBox();
            this._NO_TRANSLATE_lblSimplifyMerges = new System.Windows.Forms.Label();
            this.MainPanel.SuspendLayout();
            this.ControlsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitsLimit)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.tableLayoutPanel1);
            this.MainPanel.Size = new System.Drawing.Size(408, 336);
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.Ok);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 336);
            this.ControlsPanel.Size = new System.Drawing.Size(408, 39);
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(320, 8);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // _NO_TRANSLATE_CommitsLimit
            // 
            this._NO_TRANSLATE_CommitsLimit.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_CommitsLimit.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitsLimit.Location = new System.Drawing.Point(96, 168);
            this._NO_TRANSLATE_CommitsLimit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitsLimit.Name = "_NO_TRANSLATE_CommitsLimit";
            this._NO_TRANSLATE_CommitsLimit.Size = new System.Drawing.Size(116, 23);
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Location = new System.Drawing.Point(96, 119);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(285, 23);
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Location = new System.Drawing.Point(96, 119);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(285, 23);
            // 
            // DiffContent
            // 
            this.DiffContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffContent.Location = new System.Drawing.Point(96, 119);
            this.DiffContent.Name = "DiffContent";
            this.DiffContent.Size = new System.Drawing.Size(285, 23);
            // 
            // RawLog
            // 
            this.RawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RawLog.Location = new System.Drawing.Point(96, 61);
            this.RawLog.Name = "RawLog";
            this.RawLog.Size = new System.Drawing.Size(285, 23);
            // 
            // Since
            // 
            this.Since.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Since.Location = new System.Drawing.Point(96, 3);
            this.Since.Name = "Since";
            this.Since.Size = new System.Drawing.Size(200, 23);
            // 
            // _NO_TRANSLATE_lblSince
            // 
            this._NO_TRANSLATE_lblSince.AutoSize = true;
            this._NO_TRANSLATE_lblSince.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblSince.Location = new System.Drawing.Point(3, 0);
            this._NO_TRANSLATE_lblSince.Name = "_NO_TRANSLATE_lblSince";
            this._NO_TRANSLATE_lblSince.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblSince.Text = "Since";
            this._NO_TRANSLATE_lblSince.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblUntil
            // 
            this._NO_TRANSLATE_lblUntil.AutoSize = true;
            this._NO_TRANSLATE_lblUntil.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblUntil.Location = new System.Drawing.Point(3, 29);
            this._NO_TRANSLATE_lblUntil.Name = "_NO_TRANSLATE_lblUntil";
            this._NO_TRANSLATE_lblUntil.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblUntil.Text = "Until";
            this._NO_TRANSLATE_lblUntil.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Until
            // 
            this.Until.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Until.Location = new System.Drawing.Point(96, 32);
            this.Until.Name = "Until";
            this.Until.Size = new System.Drawing.Size(200, 23);
            // 
            // _NO_TRANSLATE_lblAuthor
            // 
            this._NO_TRANSLATE_lblAuthor.AutoSize = true;
            this._NO_TRANSLATE_lblAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblAuthor.Location = new System.Drawing.Point(3, 58);
            this._NO_TRANSLATE_lblAuthor.Name = "_NO_TRANSLATE_lblAuthor";
            this._NO_TRANSLATE_lblAuthor.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblAuthor.Text = "Author";
            this._NO_TRANSLATE_lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblCommitter
            // 
            this._NO_TRANSLATE_lblCommitter.AutoSize = true;
            this._NO_TRANSLATE_lblCommitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblCommitter.Location = new System.Drawing.Point(3, 87);
            this._NO_TRANSLATE_lblCommitter.Name = "_NO_TRANSLATE_lblCommitter";
            this._NO_TRANSLATE_lblCommitter.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblCommitter.Text = "Committer";
            this._NO_TRANSLATE_lblCommitter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Committer
            // 
            this.Committer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Committer.Location = new System.Drawing.Point(96, 90);
            this.Committer.Name = "Committer";
            this.Committer.Size = new System.Drawing.Size(285, 23);
            // 
            // _NO_TRANSLATE_lblMessage
            // 
            this._NO_TRANSLATE_lblMessage.AutoSize = true;
            this._NO_TRANSLATE_lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblMessage.Location = new System.Drawing.Point(3, 116);
            this._NO_TRANSLATE_lblMessage.Name = "_NO_TRANSLATE_lblMessage";
            this._NO_TRANSLATE_lblMessage.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblMessage.Text = "Message";
            this._NO_TRANSLATE_lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblDiffContent
            // 
            this._NO_TRANSLATE_lblDiffContent.AutoSize = true;
            this._NO_TRANSLATE_lblDiffContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblDiffContent.Location = new System.Drawing.Point(3, 116);
            this._NO_TRANSLATE_lblDiffContent.Name = "_NO_TRANSLATE_lblDiffContent";
            this._NO_TRANSLATE_lblDiffContent.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblDiffContent.Text = "Message";
            this._NO_TRANSLATE_lblDiffContent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblRawLog
            // 
            this._NO_TRANSLATE_lblRawLog.AutoSize = true;
            this._NO_TRANSLATE_lblRawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblRawLog.Location = new System.Drawing.Point(3, 116);
            this._NO_TRANSLATE_lblRawLog.Name = "_NO_TRANSLATE_lblRawLog";
            this._NO_TRANSLATE_lblRawLog.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblRawLog.Text = "Message";
            this._NO_TRANSLATE_lblRawLog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblLimit
            // 
            this._NO_TRANSLATE_lblLimit.AutoSize = true;
            this._NO_TRANSLATE_lblLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblLimit.Location = new System.Drawing.Point(3, 165);
            this._NO_TRANSLATE_lblLimit.Name = "_NO_TRANSLATE_lblLimit";
            this._NO_TRANSLATE_lblLimit.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblLimit.Text = "Limit";
            this._NO_TRANSLATE_lblLimit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_lblIgnoreCase
            // 
            this._NO_TRANSLATE_lblIgnoreCase.AutoSize = true;
            this._NO_TRANSLATE_lblIgnoreCase.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblIgnoreCase.Location = new System.Drawing.Point(3, 145);
            this._NO_TRANSLATE_lblIgnoreCase.Name = "_NO_TRANSLATE_lblIgnoreCase";
            this._NO_TRANSLATE_lblIgnoreCase.Size = new System.Drawing.Size(67, 20);
            this._NO_TRANSLATE_lblIgnoreCase.Text = "Ignore case";
            this._NO_TRANSLATE_lblIgnoreCase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblSince, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.SinceCheck, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Since, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblUntil, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CheckUntil, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Until, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblAuthor, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.AuthorCheck, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.Author, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblCommitter, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.CommitterCheck, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.Committer, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblMessage, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.MessageCheck, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.Message, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblDiffContent, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.DiffContentCheck, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.DiffContent, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblRawLog, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.RawLogCheck, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.RawLog, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblIgnoreCase, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.IgnoreCase, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblLimit, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.CommitsLimitCheck, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_CommitsLimit, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblPathFilter, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.PathFilterCheck, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.PathFilter, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.ReflogCheck, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblReflogCheck, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.CurrentBranchOnlyCheck, 1, 11);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblCurrentBranchOnlyCheck, 2, 11);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblBranches, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilterCheck, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilter, 2, 12);
            this.tableLayoutPanel1.Controls.Add(this.ShowOnlyFirstParentCheck, 1, 13);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblShowOnlyFirstParent, 2, 13);
            this.tableLayoutPanel1.Controls.Add(this.ShowMergeCommitsCheck, 1, 14);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblShowMergeCommits, 2, 14);
            this.tableLayoutPanel1.Controls.Add(this.SimplifyByDecorationCheck, 1, 15);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblSimplifyByDecoration, 2, 15);
            this.tableLayoutPanel1.Controls.Add(this.FullHistoryCheck, 1, 16);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblFullHistory, 2, 16);
            this.tableLayoutPanel1.Controls.Add(this.SimplifyMergesCheck, 1, 17);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_lblSimplifyMerges, 2, 17);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 18;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(384, 312);
            // 
            // SinceCheck
            // 
            this.SinceCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SinceCheck.AutoSize = true;
            this.SinceCheck.Location = new System.Drawing.Point(76, 7);
            this.SinceCheck.Name = "SinceCheck";
            this.SinceCheck.Size = new System.Drawing.Size(14, 14);
            this.SinceCheck.UseVisualStyleBackColor = true;
            this.SinceCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // CheckUntil
            // 
            this.CheckUntil.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CheckUntil.AutoSize = true;
            this.CheckUntil.Location = new System.Drawing.Point(76, 36);
            this.CheckUntil.Name = "CheckUntil";
            this.CheckUntil.Size = new System.Drawing.Size(14, 14);
            this.CheckUntil.UseVisualStyleBackColor = true;
            this.CheckUntil.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // AuthorCheck
            // 
            this.AuthorCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AuthorCheck.AutoSize = true;
            this.AuthorCheck.Location = new System.Drawing.Point(76, 65);
            this.AuthorCheck.Name = "AuthorCheck";
            this.AuthorCheck.Size = new System.Drawing.Size(14, 14);
            this.AuthorCheck.UseVisualStyleBackColor = true;
            this.AuthorCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // CommitterCheck
            // 
            this.CommitterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CommitterCheck.AutoSize = true;
            this.CommitterCheck.Location = new System.Drawing.Point(76, 94);
            this.CommitterCheck.Name = "CommitterCheck";
            this.CommitterCheck.Size = new System.Drawing.Size(14, 14);
            this.CommitterCheck.UseVisualStyleBackColor = true;
            this.CommitterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // MessageCheck
            // 
            this.MessageCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.MessageCheck.AutoSize = true;
            this.MessageCheck.Location = new System.Drawing.Point(76, 123);
            this.MessageCheck.Name = "MessageCheck";
            this.MessageCheck.Size = new System.Drawing.Size(14, 14);
            this.MessageCheck.UseVisualStyleBackColor = true;
            this.MessageCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // DiffContentCheck
            // 
            this.DiffContentCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DiffContentCheck.AutoSize = true;
            this.DiffContentCheck.Location = new System.Drawing.Point(76, 123);
            this.DiffContentCheck.Name = "DiffContentCheck";
            this.DiffContentCheck.Size = new System.Drawing.Size(14, 14);
            this.DiffContentCheck.UseVisualStyleBackColor = true;
            this.DiffContentCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // RawLogCheck
            // 
            this.RawLogCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.RawLogCheck.AutoSize = true;
            this.RawLogCheck.Location = new System.Drawing.Point(76, 123);
            this.RawLogCheck.Name = "RawLogCheck";
            this.RawLogCheck.Size = new System.Drawing.Size(14, 14);
            this.RawLogCheck.UseVisualStyleBackColor = true;
            this.RawLogCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.Checked = true;
            this.IgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreCase.Location = new System.Drawing.Point(76, 148);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(14, 14);
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // CommitsLimitCheck
            // 
            this.CommitsLimitCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CommitsLimitCheck.AutoSize = true;
            this.CommitsLimitCheck.Location = new System.Drawing.Point(76, 172);
            this.CommitsLimitCheck.Name = "CommitsLimitCheck";
            this.CommitsLimitCheck.Size = new System.Drawing.Size(14, 14);
            this.CommitsLimitCheck.UseVisualStyleBackColor = true;
            this.CommitsLimitCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblPathFilter
            // 
            this._NO_TRANSLATE_lblPathFilter.AutoSize = true;
            this._NO_TRANSLATE_lblPathFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblPathFilter.Location = new System.Drawing.Point(3, 194);
            this._NO_TRANSLATE_lblPathFilter.Name = "_NO_TRANSLATE_lblPathFilter";
            this._NO_TRANSLATE_lblPathFilter.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblPathFilter.Text = "Path filter";
            this._NO_TRANSLATE_lblPathFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PathFilterCheck
            // 
            this.PathFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.PathFilterCheck.AutoSize = true;
            this.PathFilterCheck.Location = new System.Drawing.Point(76, 201);
            this.PathFilterCheck.Name = "PathFilterCheck";
            this.PathFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.PathFilterCheck.UseVisualStyleBackColor = true;
            this.PathFilterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // PathFilter
            // 
            this.PathFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PathFilter.Location = new System.Drawing.Point(96, 197);
            this.PathFilter.Name = "PathFilter";
            this.PathFilter.Size = new System.Drawing.Size(285, 23);
            // 
            // _NO_TRANSLATE_lblBranches
            // 
            this._NO_TRANSLATE_lblBranches.AutoSize = true;
            this._NO_TRANSLATE_lblBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblBranches.Location = new System.Drawing.Point(3, 223);
            this._NO_TRANSLATE_lblBranches.Name = "_NO_TRANSLATE_lblBranches";
            this._NO_TRANSLATE_lblBranches.Size = new System.Drawing.Size(67, 29);
            this._NO_TRANSLATE_lblBranches.Text = "Branches";
            this._NO_TRANSLATE_lblBranches.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BranchFilterCheck
            // 
            this.BranchFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BranchFilterCheck.AutoSize = true;
            this.BranchFilterCheck.Location = new System.Drawing.Point(76, 230);
            this.BranchFilterCheck.Name = "BranchFilterCheck";
            this.BranchFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.BranchFilterCheck.UseVisualStyleBackColor = true;
            this.BranchFilterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // BranchFilter
            // 
            this.BranchFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchFilter.Location = new System.Drawing.Point(96, 226);
            this.BranchFilter.Name = "BranchFilter";
            this.BranchFilter.Size = new System.Drawing.Size(285, 23);
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(76, 255);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 19);
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblCurrentBranchOnlyCheck
            // 
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.AutoSize = true;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Name = "_NO_TRANSLATE_lblCurrentBranchOnlyCheck";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TabIndex = 28;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Text = "Show current branch only";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(76, 255);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 19);
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblCurrentBranchOnlyCheck
            // 
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.AutoSize = true;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Name = "_NO_TRANSLATE_lblCurrentBranchOnlyCheck";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TabIndex = 28;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Text = "Show current branch only";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(76, 255);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 19);
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblCurrentBranchOnlyCheck
            // 
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.AutoSize = true;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Name = "_NO_TRANSLATE_lblCurrentBranchOnlyCheck";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TabIndex = 28;
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.Text = "Show current branch only";
            this._NO_TRANSLATE_lblCurrentBranchOnlyCheck.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShowOnlyFirstParentCheck
            // 
            this.ShowOnlyFirstParentCheck.AutoSize = true;
            this.ShowOnlyFirstParentCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowOnlyFirstParentCheck.Location = new System.Drawing.Point(76, 280);
            this.ShowOnlyFirstParentCheck.Name = "ShowOnlyFirstParentCheck";
            this.ShowOnlyFirstParentCheck.Size = new System.Drawing.Size(14, 14);
            this.ShowOnlyFirstParentCheck.UseVisualStyleBackColor = true;
            this.ShowOnlyFirstParentCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblShowOnlyFirstParent
            // 
            this._NO_TRANSLATE_lblShowOnlyFirstParent.AutoSize = true;
            this._NO_TRANSLATE_lblShowOnlyFirstParent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblShowOnlyFirstParent.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblShowOnlyFirstParent.Name = "_NO_TRANSLATE_lblShowOnlyFirstParent";
            this._NO_TRANSLATE_lblShowOnlyFirstParent.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblShowOnlyFirstParent.TabIndex = 29;
            this._NO_TRANSLATE_lblShowOnlyFirstParent.Text = "ShowOnlyFirstParent";
            this._NO_TRANSLATE_lblShowOnlyFirstParent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShowMergeCommitsCheck
            // 
            this.ShowMergeCommitsCheck.AutoSize = true;
            this.ShowMergeCommitsCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowMergeCommitsCheck.Location = new System.Drawing.Point(76, 280);
            this.ShowMergeCommitsCheck.Name = "ShowMergeCommitsCheck";
            this.ShowMergeCommitsCheck.Size = new System.Drawing.Size(14, 14);
            this.ShowMergeCommitsCheck.UseVisualStyleBackColor = true;
            this.ShowMergeCommitsCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblShowMergeCommits
            // 
            this._NO_TRANSLATE_lblShowMergeCommits.AutoSize = true;
            this._NO_TRANSLATE_lblShowMergeCommits.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblShowMergeCommits.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblShowMergeCommits.Name = "_NO_TRANSLATE_lblShowMergeCommits";
            this._NO_TRANSLATE_lblShowMergeCommits.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblShowMergeCommits.TabIndex = 29;
            this._NO_TRANSLATE_lblShowMergeCommits.Text = "ShowMergeCommits";
            this._NO_TRANSLATE_lblShowMergeCommits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SimplifyByDecorationCheck
            // 
            this.SimplifyByDecorationCheck.AutoSize = true;
            this.SimplifyByDecorationCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SimplifyByDecorationCheck.Location = new System.Drawing.Point(76, 280);
            this.SimplifyByDecorationCheck.Name = "SimplifyByDecorationCheck";
            this.SimplifyByDecorationCheck.Size = new System.Drawing.Size(14, 14);
            this.SimplifyByDecorationCheck.UseVisualStyleBackColor = true;
            this.SimplifyByDecorationCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblSimplifyByDecoration
            // 
            this._NO_TRANSLATE_lblSimplifyByDecoration.AutoSize = true;
            this._NO_TRANSLATE_lblSimplifyByDecoration.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblSimplifyByDecoration.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblSimplifyByDecoration.Name = "_NO_TRANSLATE_lblSimplifyByDecoration";
            this._NO_TRANSLATE_lblSimplifyByDecoration.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblSimplifyByDecoration.TabIndex = 29;
            this._NO_TRANSLATE_lblSimplifyByDecoration.Text = "Simplify by decoration";
            this._NO_TRANSLATE_lblSimplifyByDecoration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FullHistoryCheck
            // 
            this.FullHistoryCheck.AutoSize = true;
            this.FullHistoryCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FullHistoryCheck.Location = new System.Drawing.Point(76, 280);
            this.FullHistoryCheck.Name = "FullHistoryCheck";
            this.FullHistoryCheck.Size = new System.Drawing.Size(14, 14);
            this.FullHistoryCheck.UseVisualStyleBackColor = true;
            this.FullHistoryCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblFullHistory
            // 
            this._NO_TRANSLATE_lblFullHistory.AutoSize = true;
            this._NO_TRANSLATE_lblFullHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblFullHistory.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblFullHistory.Name = "_NO_TRANSLATE_lblFullHistory";
            this._NO_TRANSLATE_lblFullHistory.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblFullHistory.TabIndex = 29;
            this._NO_TRANSLATE_lblFullHistory.Text = "FullHistory";
            this._NO_TRANSLATE_lblFullHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SimplifyMergesCheck
            // 
            this.SimplifyMergesCheck.AutoSize = true;
            this.SimplifyMergesCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SimplifyMergesCheck.Location = new System.Drawing.Point(76, 280);
            this.SimplifyMergesCheck.Name = "SimplifyMergesCheck";
            this.SimplifyMergesCheck.Size = new System.Drawing.Size(14, 14);
            this.SimplifyMergesCheck.UseVisualStyleBackColor = true;
            this.SimplifyMergesCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // _NO_TRANSLATE_lblSimplifyMerges
            // 
            this._NO_TRANSLATE_lblSimplifyMerges.AutoSize = true;
            this._NO_TRANSLATE_lblSimplifyMerges.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_lblSimplifyMerges.Location = new System.Drawing.Point(96, 277);
            this._NO_TRANSLATE_lblSimplifyMerges.Name = "_NO_TRANSLATE_lblSimplifyMerges";
            this._NO_TRANSLATE_lblSimplifyMerges.Size = new System.Drawing.Size(285, 20);
            this._NO_TRANSLATE_lblSimplifyMerges.TabIndex = 29;
            this._NO_TRANSLATE_lblSimplifyMerges.Text = "SimplifyMerges";
            this._NO_TRANSLATE_lblSimplifyMerges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormRevisionFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(408, 525);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRevisionFilter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter";
            this.MainPanel.ResumeLayout(false);
            this.ControlsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitsLimit)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_CommitsLimit;
        private System.Windows.Forms.TextBox Message;
        private System.Windows.Forms.TextBox DiffContent;
        private System.Windows.Forms.TextBox RawLog;
        private System.Windows.Forms.TextBox Author;
        private System.Windows.Forms.DateTimePicker Since;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblSince;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblUntil;
        private System.Windows.Forms.DateTimePicker Until;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblAuthor;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblCommitter;
        private System.Windows.Forms.TextBox Committer;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblMessage;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblDiffContent;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblRawLog;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblLimit;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblIgnoreCase;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox CommitsLimitCheck;
        private System.Windows.Forms.CheckBox IgnoreCase;
        private System.Windows.Forms.CheckBox MessageCheck;
        private System.Windows.Forms.CheckBox DiffContentCheck;
        private System.Windows.Forms.CheckBox RawLogCheck;
        private System.Windows.Forms.CheckBox CommitterCheck;
        private System.Windows.Forms.CheckBox AuthorCheck;
        private System.Windows.Forms.CheckBox CheckUntil;
        private System.Windows.Forms.CheckBox SinceCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblPathFilter;
        private System.Windows.Forms.CheckBox PathFilterCheck;
        private System.Windows.Forms.TextBox PathFilter;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblBranches;
        private System.Windows.Forms.TextBox BranchFilter;
        private System.Windows.Forms.CheckBox ReflogCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblReflogCheck;
        private System.Windows.Forms.CheckBox CurrentBranchOnlyCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblCurrentBranchOnlyCheck;
        private System.Windows.Forms.CheckBox BranchFilterCheck;
        private System.Windows.Forms.CheckBox ShowOnlyFirstParentCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblShowOnlyFirstParent;
        private System.Windows.Forms.CheckBox ShowMergeCommitsCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblShowMergeCommits;
        private System.Windows.Forms.CheckBox SimplifyByDecorationCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblSimplifyByDecoration;
        private System.Windows.Forms.CheckBox FullHistoryCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblFullHistory;
        private System.Windows.Forms.CheckBox SimplifyMergesCheck;
        private System.Windows.Forms.Label _NO_TRANSLATE_lblSimplifyMerges;
    }
}
