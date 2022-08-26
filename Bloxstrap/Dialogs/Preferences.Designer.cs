namespace Bloxstrap.Dialogs
{
    partial class Preferences
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
            if (disposing && (components != null))
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.DialogTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RFUWebsite = new System.Windows.Forms.LinkLabel();
            this.ToggleRFUAutoclose = new System.Windows.Forms.CheckBox();
            this.ToggleRFUEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.ToggleRPCButtons = new System.Windows.Forms.CheckBox();
            this.ToggleDiscordRichPresence = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.IconPreview = new System.Windows.Forms.PictureBox();
            this.IconSelection = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.StyleSelection = new System.Windows.Forms.ListBox();
            this.InstallationTab = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.ToggleShowAllChannels = new System.Windows.Forms.CheckBox();
            this.LabelChannelInfo = new System.Windows.Forms.Label();
            this.SelectChannel = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.LabelModFolderInstall = new System.Windows.Forms.Label();
            this.ButtonOpenModFolder = new System.Windows.Forms.Button();
            this.ToggleMouseCursor = new System.Windows.Forms.CheckBox();
            this.ToggleDeathSound = new System.Windows.Forms.CheckBox();
            this.GroupBoxInstallLocation = new System.Windows.Forms.GroupBox();
            this.InstallLocationBrowseButton = new System.Windows.Forms.Button();
            this.InstallLocation = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ToggleCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.PreviewButton = new System.Windows.Forms.Button();
            this.InstallLocationBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.Tabs.SuspendLayout();
            this.DialogTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.InstallationTab.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.GroupBoxInstallLocation.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.Navy;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(237, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Configure Bloxstrap";
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.DialogTab);
            this.Tabs.Controls.Add(this.InstallationTab);
            this.Tabs.Location = new System.Drawing.Point(12, 40);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(442, 247);
            this.Tabs.TabIndex = 2;
            // 
            // DialogTab
            // 
            this.DialogTab.Controls.Add(this.groupBox1);
            this.DialogTab.Controls.Add(this.groupBox5);
            this.DialogTab.Controls.Add(this.groupBox3);
            this.DialogTab.Controls.Add(this.groupBox2);
            this.DialogTab.Location = new System.Drawing.Point(4, 24);
            this.DialogTab.Name = "DialogTab";
            this.DialogTab.Padding = new System.Windows.Forms.Padding(3);
            this.DialogTab.Size = new System.Drawing.Size(434, 219);
            this.DialogTab.TabIndex = 0;
            this.DialogTab.Text = "Bootstrapper";
            this.DialogTab.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RFUWebsite);
            this.groupBox1.Controls.Add(this.ToggleRFUAutoclose);
            this.groupBox1.Controls.Add(this.ToggleRFUEnabled);
            this.groupBox1.Location = new System.Drawing.Point(192, 146);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(235, 67);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FPS Unlocker";
            // 
            // RFUWebsite
            // 
            this.RFUWebsite.BackColor = System.Drawing.Color.White;
            this.RFUWebsite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RFUWebsite.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.RFUWebsite.Location = new System.Drawing.Point(174, 0);
            this.RFUWebsite.Margin = new System.Windows.Forms.Padding(0);
            this.RFUWebsite.Name = "RFUWebsite";
            this.RFUWebsite.Size = new System.Drawing.Size(55, 18);
            this.RFUWebsite.TabIndex = 2;
            this.RFUWebsite.TabStop = true;
            this.RFUWebsite.Tag = "";
            this.RFUWebsite.Text = "(website)";
            this.RFUWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RFUWebsite_LinkClicked);
            // 
            // ToggleRFUAutoclose
            // 
            this.ToggleRFUAutoclose.AutoSize = true;
            this.ToggleRFUAutoclose.Checked = true;
            this.ToggleRFUAutoclose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleRFUAutoclose.Location = new System.Drawing.Point(9, 40);
            this.ToggleRFUAutoclose.Name = "ToggleRFUAutoclose";
            this.ToggleRFUAutoclose.Size = new System.Drawing.Size(209, 19);
            this.ToggleRFUAutoclose.TabIndex = 1;
            this.ToggleRFUAutoclose.Text = "Automatically close on Roblox exit";
            this.InfoTooltip.SetToolTip(this.ToggleRFUAutoclose, "If enabled, rbxfpsunlocker will automatically close when Roblox is closed.");
            this.ToggleRFUAutoclose.UseVisualStyleBackColor = true;
            this.ToggleRFUAutoclose.CheckedChanged += new System.EventHandler(this.ToggleRFUAutoclose_CheckedChanged);
            // 
            // ToggleRFUEnabled
            // 
            this.ToggleRFUEnabled.AutoSize = true;
            this.ToggleRFUEnabled.Checked = true;
            this.ToggleRFUEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleRFUEnabled.Location = new System.Drawing.Point(9, 19);
            this.ToggleRFUEnabled.Name = "ToggleRFUEnabled";
            this.ToggleRFUEnabled.Size = new System.Drawing.Size(127, 19);
            this.ToggleRFUEnabled.TabIndex = 0;
            this.ToggleRFUEnabled.Text = "Use rbxfpsunlocker";
            this.InfoTooltip.SetToolTip(this.ToggleRFUEnabled, "If enabled, rbxfpsunlocker is downloaded.\r\nWhen Roblox is started, rbxfpsunlocker" +
        " will automatically start too, \r\nbeing minimized to your system tray by default." +
        "");
            this.ToggleRFUEnabled.UseVisualStyleBackColor = true;
            this.ToggleRFUEnabled.CheckedChanged += new System.EventHandler(this.ToggleRFUEnabled_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.ToggleRPCButtons);
            this.groupBox5.Controls.Add(this.ToggleDiscordRichPresence);
            this.groupBox5.Location = new System.Drawing.Point(5, 146);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(179, 67);
            this.groupBox5.TabIndex = 7;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Discord Rich Presence";
            // 
            // ToggleRPCButtons
            // 
            this.ToggleRPCButtons.AutoSize = true;
            this.ToggleRPCButtons.Location = new System.Drawing.Point(9, 40);
            this.ToggleRPCButtons.Name = "ToggleRPCButtons";
            this.ToggleRPCButtons.Size = new System.Drawing.Size(155, 19);
            this.ToggleRPCButtons.TabIndex = 1;
            this.ToggleRPCButtons.Text = "Hide interaction buttons";
            this.InfoTooltip.SetToolTip(this.ToggleRPCButtons, "Choose whether the buttons to play/view game details should be hidden from your a" +
        "ctivity status.");
            this.ToggleRPCButtons.UseVisualStyleBackColor = true;
            this.ToggleRPCButtons.CheckedChanged += new System.EventHandler(this.ToggleRPCButtons_CheckedChanged);
            // 
            // ToggleDiscordRichPresence
            // 
            this.ToggleDiscordRichPresence.AutoSize = true;
            this.ToggleDiscordRichPresence.Checked = true;
            this.ToggleDiscordRichPresence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleDiscordRichPresence.Location = new System.Drawing.Point(9, 19);
            this.ToggleDiscordRichPresence.Name = "ToggleDiscordRichPresence";
            this.ToggleDiscordRichPresence.Size = new System.Drawing.Size(129, 19);
            this.ToggleDiscordRichPresence.TabIndex = 0;
            this.ToggleDiscordRichPresence.Text = "Show game activity";
            this.InfoTooltip.SetToolTip(this.ToggleDiscordRichPresence, "Choose whether to show what game you\'re playing on your Discord activity status.\r" +
        "\nThis will only work when you launch a game from the website, and is not support" +
        "ed in the Beta App.");
            this.ToggleDiscordRichPresence.UseVisualStyleBackColor = true;
            this.ToggleDiscordRichPresence.CheckedChanged += new System.EventHandler(this.ToggleDiscordRichPresence_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.IconPreview);
            this.groupBox3.Controls.Add(this.IconSelection);
            this.groupBox3.Location = new System.Drawing.Point(192, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(235, 140);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Icon";
            // 
            // IconPreview
            // 
            this.IconPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.IconPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IconPreview.Location = new System.Drawing.Point(117, 21);
            this.IconPreview.Name = "IconPreview";
            this.IconPreview.Size = new System.Drawing.Size(109, 109);
            this.IconPreview.TabIndex = 3;
            this.IconPreview.TabStop = false;
            // 
            // IconSelection
            // 
            this.IconSelection.FormattingEnabled = true;
            this.IconSelection.ItemHeight = 15;
            this.IconSelection.Location = new System.Drawing.Point(9, 21);
            this.IconSelection.Name = "IconSelection";
            this.IconSelection.Size = new System.Drawing.Size(100, 109);
            this.IconSelection.TabIndex = 4;
            this.InfoTooltip.SetToolTip(this.IconSelection, "Choose what icon the bootstrapper should use.");
            this.IconSelection.SelectedIndexChanged += new System.EventHandler(this.IconSelection_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.StyleSelection);
            this.groupBox2.Location = new System.Drawing.Point(5, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(179, 140);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Style";
            // 
            // StyleSelection
            // 
            this.StyleSelection.FormattingEnabled = true;
            this.StyleSelection.ItemHeight = 15;
            this.StyleSelection.Location = new System.Drawing.Point(9, 21);
            this.StyleSelection.Name = "StyleSelection";
            this.StyleSelection.Size = new System.Drawing.Size(161, 109);
            this.StyleSelection.TabIndex = 3;
            this.InfoTooltip.SetToolTip(this.StyleSelection, "Choose how the bootstrapper dialog should look.\r\nYou can use the \'Preview\' button" +
        " to preview the bootstrapper look.");
            this.StyleSelection.SelectedIndexChanged += new System.EventHandler(this.StyleSelection_SelectedIndexChanged);
            // 
            // InstallationTab
            // 
            this.InstallationTab.Controls.Add(this.groupBox6);
            this.InstallationTab.Controls.Add(this.groupBox4);
            this.InstallationTab.Controls.Add(this.GroupBoxInstallLocation);
            this.InstallationTab.Location = new System.Drawing.Point(4, 24);
            this.InstallationTab.Name = "InstallationTab";
            this.InstallationTab.Padding = new System.Windows.Forms.Padding(3);
            this.InstallationTab.Size = new System.Drawing.Size(434, 219);
            this.InstallationTab.TabIndex = 2;
            this.InstallationTab.Text = "Installation";
            this.InstallationTab.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.ToggleShowAllChannels);
            this.groupBox6.Controls.Add(this.LabelChannelInfo);
            this.groupBox6.Controls.Add(this.SelectChannel);
            this.groupBox6.Location = new System.Drawing.Point(5, 158);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(422, 56);
            this.groupBox6.TabIndex = 3;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Build Channel";
            // 
            // ToggleShowAllChannels
            // 
            this.ToggleShowAllChannels.AutoSize = true;
            this.ToggleShowAllChannels.BackColor = System.Drawing.Color.White;
            this.ToggleShowAllChannels.Location = new System.Drawing.Point(289, 0);
            this.ToggleShowAllChannels.Name = "ToggleShowAllChannels";
            this.ToggleShowAllChannels.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.ToggleShowAllChannels.Size = new System.Drawing.Size(127, 19);
            this.ToggleShowAllChannels.TabIndex = 2;
            this.ToggleShowAllChannels.Text = "Show all channels";
            this.ToggleShowAllChannels.UseVisualStyleBackColor = false;
            this.ToggleShowAllChannels.CheckedChanged += new System.EventHandler(this.ToggleShowAllChannels_CheckedChanged);
            // 
            // LabelChannelInfo
            // 
            this.LabelChannelInfo.Location = new System.Drawing.Point(134, 18);
            this.LabelChannelInfo.Name = "LabelChannelInfo";
            this.LabelChannelInfo.Size = new System.Drawing.Size(282, 28);
            this.LabelChannelInfo.TabIndex = 1;
            this.LabelChannelInfo.Text = "Getting latest deploy, please wait...";
            this.LabelChannelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SelectChannel
            // 
            this.SelectChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectChannel.DropDownWidth = 265;
            this.SelectChannel.FormattingEnabled = true;
            this.SelectChannel.Location = new System.Drawing.Point(9, 21);
            this.SelectChannel.Name = "SelectChannel";
            this.SelectChannel.Size = new System.Drawing.Size(120, 23);
            this.SelectChannel.TabIndex = 0;
            this.InfoTooltip.SetToolTip(this.SelectChannel, "Choose what deploy channel to use.\r\nThe default channel is LIVE.\r\nYou should only" +
        " change this if you\'re know exactly what you\'re doing.\r\n");
            this.SelectChannel.SelectedValueChanged += new System.EventHandler(this.SelectChannel_SelectedValueChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.LabelModFolderInstall);
            this.groupBox4.Controls.Add(this.ButtonOpenModFolder);
            this.groupBox4.Controls.Add(this.ToggleMouseCursor);
            this.groupBox4.Controls.Add(this.ToggleDeathSound);
            this.groupBox4.Location = new System.Drawing.Point(5, 60);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(422, 95);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Modifications";
            // 
            // LabelModFolderInstall
            // 
            this.LabelModFolderInstall.AutoSize = true;
            this.LabelModFolderInstall.Location = new System.Drawing.Point(6, 67);
            this.LabelModFolderInstall.Margin = new System.Windows.Forms.Padding(0);
            this.LabelModFolderInstall.Name = "LabelModFolderInstall";
            this.LabelModFolderInstall.Size = new System.Drawing.Size(329, 15);
            this.LabelModFolderInstall.TabIndex = 7;
            this.LabelModFolderInstall.Text = "Other modifications can be added once Bloxstrap is installed.";
            this.LabelModFolderInstall.Visible = false;
            // 
            // ButtonOpenModFolder
            // 
            this.ButtonOpenModFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ButtonOpenModFolder.Location = new System.Drawing.Point(8, 62);
            this.ButtonOpenModFolder.Name = "ButtonOpenModFolder";
            this.ButtonOpenModFolder.Size = new System.Drawing.Size(122, 25);
            this.ButtonOpenModFolder.TabIndex = 6;
            this.ButtonOpenModFolder.Text = "Open Mod Folder";
            this.InfoTooltip.SetToolTip(this.ButtonOpenModFolder, "Open the folder for applying Roblox client modifications.");
            this.ButtonOpenModFolder.UseVisualStyleBackColor = true;
            this.ButtonOpenModFolder.Click += new System.EventHandler(this.ButtonOpenModFolder_Click);
            // 
            // ToggleMouseCursor
            // 
            this.ToggleMouseCursor.AutoSize = true;
            this.ToggleMouseCursor.Location = new System.Drawing.Point(9, 40);
            this.ToggleMouseCursor.Margin = new System.Windows.Forms.Padding(2);
            this.ToggleMouseCursor.Name = "ToggleMouseCursor";
            this.ToggleMouseCursor.Size = new System.Drawing.Size(140, 19);
            this.ToggleMouseCursor.TabIndex = 2;
            this.ToggleMouseCursor.Text = "Use old mouse cursor";
            this.ToggleMouseCursor.UseVisualStyleBackColor = true;
            this.ToggleMouseCursor.CheckedChanged += new System.EventHandler(this.ToggleMouseCursor_CheckedChanged);
            // 
            // ToggleDeathSound
            // 
            this.ToggleDeathSound.AutoSize = true;
            this.ToggleDeathSound.Checked = true;
            this.ToggleDeathSound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleDeathSound.Location = new System.Drawing.Point(9, 19);
            this.ToggleDeathSound.Margin = new System.Windows.Forms.Padding(2);
            this.ToggleDeathSound.Name = "ToggleDeathSound";
            this.ToggleDeathSound.Size = new System.Drawing.Size(134, 19);
            this.ToggleDeathSound.TabIndex = 1;
            this.ToggleDeathSound.Text = "Use old death sound";
            this.ToggleDeathSound.UseVisualStyleBackColor = true;
            this.ToggleDeathSound.CheckedChanged += new System.EventHandler(this.ToggleDeathSound_CheckedChanged);
            // 
            // GroupBoxInstallLocation
            // 
            this.GroupBoxInstallLocation.Controls.Add(this.InstallLocationBrowseButton);
            this.GroupBoxInstallLocation.Controls.Add(this.InstallLocation);
            this.GroupBoxInstallLocation.Location = new System.Drawing.Point(5, 3);
            this.GroupBoxInstallLocation.Name = "GroupBoxInstallLocation";
            this.GroupBoxInstallLocation.Size = new System.Drawing.Size(422, 54);
            this.GroupBoxInstallLocation.TabIndex = 0;
            this.GroupBoxInstallLocation.TabStop = false;
            this.GroupBoxInstallLocation.Text = "Location";
            // 
            // InstallLocationBrowseButton
            // 
            this.InstallLocationBrowseButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InstallLocationBrowseButton.Location = new System.Drawing.Point(335, 20);
            this.InstallLocationBrowseButton.Name = "InstallLocationBrowseButton";
            this.InstallLocationBrowseButton.Size = new System.Drawing.Size(79, 25);
            this.InstallLocationBrowseButton.TabIndex = 5;
            this.InstallLocationBrowseButton.Text = "Browse...";
            this.InstallLocationBrowseButton.UseVisualStyleBackColor = true;
            this.InstallLocationBrowseButton.Click += new System.EventHandler(this.InstallLocationBrowseButton_Click);
            // 
            // InstallLocation
            // 
            this.InstallLocation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InstallLocation.Location = new System.Drawing.Point(9, 21);
            this.InstallLocation.MaxLength = 255;
            this.InstallLocation.Name = "InstallLocation";
            this.InstallLocation.Size = new System.Drawing.Size(319, 23);
            this.InstallLocation.TabIndex = 4;
            this.InfoTooltip.SetToolTip(this.InstallLocation, "Choose where Bloxstrap should install to.\r\nThis is useful if you typically instal" +
        "l all your games to a separate storage drive.");
            // 
            // SaveButton
            // 
            this.SaveButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SaveButton.Location = new System.Drawing.Point(380, 9);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(73, 23);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.ToggleCheckForUpdates);
            this.panel1.Controls.Add(this.PreviewButton);
            this.panel1.Controls.Add(this.SaveButton);
            this.panel1.Location = new System.Drawing.Point(-1, 298);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(466, 42);
            this.panel1.TabIndex = 6;
            // 
            // ToggleCheckForUpdates
            // 
            this.ToggleCheckForUpdates.AutoSize = true;
            this.ToggleCheckForUpdates.Checked = true;
            this.ToggleCheckForUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleCheckForUpdates.Location = new System.Drawing.Point(14, 12);
            this.ToggleCheckForUpdates.Name = "ToggleCheckForUpdates";
            this.ToggleCheckForUpdates.Size = new System.Drawing.Size(179, 19);
            this.ToggleCheckForUpdates.TabIndex = 7;
            this.ToggleCheckForUpdates.Text = "Check for updates on startup";
            this.ToggleCheckForUpdates.UseVisualStyleBackColor = true;
            this.ToggleCheckForUpdates.CheckedChanged += new System.EventHandler(this.ToggleCheckForUpdates_CheckedChanged);
            // 
            // PreviewButton
            // 
            this.PreviewButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PreviewButton.Location = new System.Drawing.Point(297, 9);
            this.PreviewButton.Name = "PreviewButton";
            this.PreviewButton.Size = new System.Drawing.Size(73, 23);
            this.PreviewButton.TabIndex = 5;
            this.PreviewButton.Text = "Preview";
            this.PreviewButton.UseVisualStyleBackColor = true;
            this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
            // 
            // InfoTooltip
            // 
            this.InfoTooltip.ShowAlways = true;
            this.InfoTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.InfoTooltip.ToolTipTitle = "Information";
            // 
            // Preferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(464, 339);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preferences";
            this.Load += new System.EventHandler(this.Preferences_Load);
            this.Tabs.ResumeLayout(false);
            this.DialogTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.InstallationTab.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.GroupBoxInstallLocation.ResumeLayout(false);
            this.GroupBoxInstallLocation.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private TabControl Tabs;
        private TabPage DialogTab;
        private TabPage InstallationTab;
        private Button SaveButton;
        private Panel panel1;
        private ListBox StyleSelection;
        private GroupBox GroupBoxInstallLocation;
        private Button InstallLocationBrowseButton;
        private TextBox InstallLocation;
        private FolderBrowserDialog InstallLocationBrowseDialog;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private PictureBox IconPreview;
        private ListBox IconSelection;
        private Button PreviewButton;
        private CheckBox ToggleDeathSound;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private CheckBox ToggleDiscordRichPresence;
        private ToolTip InfoTooltip;
        private CheckBox ToggleMouseCursor;
        private CheckBox ToggleRPCButtons;
        private GroupBox groupBox1;
        private LinkLabel RFUWebsite;
        private CheckBox ToggleRFUAutoclose;
        private CheckBox ToggleRFUEnabled;
        private CheckBox ToggleCheckForUpdates;
        private Button ButtonOpenModFolder;
        private Label LabelModFolderInstall;
        private GroupBox groupBox6;
        private ComboBox SelectChannel;
        private Label LabelChannelInfo;
        private CheckBox ToggleShowAllChannels;
    }
}