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
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.ToggleDiscordRichPresence = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.IconPreview = new System.Windows.Forms.PictureBox();
            this.IconSelection = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.StyleSelection = new System.Windows.Forms.ListBox();
            this.InstallationTab = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ToggleDeathSound = new System.Windows.Forms.CheckBox();
            this.GroupBoxInstallLocation = new System.Windows.Forms.GroupBox();
            this.InstallLocationBrowseButton = new System.Windows.Forms.Button();
            this.InstallLocation = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PreviewButton = new System.Windows.Forms.Button();
            this.InstallLocationBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.Tabs.SuspendLayout();
            this.DialogTab.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.InstallationTab.SuspendLayout();
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
            this.Tabs.Size = new System.Drawing.Size(442, 226);
            this.Tabs.TabIndex = 2;
            // 
            // DialogTab
            // 
            this.DialogTab.Controls.Add(this.groupBox5);
            this.DialogTab.Controls.Add(this.groupBox3);
            this.DialogTab.Controls.Add(this.groupBox2);
            this.DialogTab.Location = new System.Drawing.Point(4, 24);
            this.DialogTab.Name = "DialogTab";
            this.DialogTab.Padding = new System.Windows.Forms.Padding(3);
            this.DialogTab.Size = new System.Drawing.Size(434, 198);
            this.DialogTab.TabIndex = 0;
            this.DialogTab.Text = "Bootstrapper";
            this.DialogTab.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.ToggleDiscordRichPresence);
            this.groupBox5.Location = new System.Drawing.Point(5, 146);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(422, 46);
            this.groupBox5.TabIndex = 7;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Launch";
            // 
            // ToggleDiscordRichPresence
            // 
            this.ToggleDiscordRichPresence.AutoSize = true;
            this.ToggleDiscordRichPresence.Checked = true;
            this.ToggleDiscordRichPresence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleDiscordRichPresence.Location = new System.Drawing.Point(9, 19);
            this.ToggleDiscordRichPresence.Name = "ToggleDiscordRichPresence";
            this.ToggleDiscordRichPresence.Size = new System.Drawing.Size(274, 19);
            this.ToggleDiscordRichPresence.TabIndex = 0;
            this.ToggleDiscordRichPresence.Text = "Show game activity with Discord Rich Presence";
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
            this.StyleSelection.SelectedIndexChanged += new System.EventHandler(this.StyleSelection_SelectedIndexChanged);
            // 
            // InstallationTab
            // 
            this.InstallationTab.Controls.Add(this.groupBox4);
            this.InstallationTab.Controls.Add(this.GroupBoxInstallLocation);
            this.InstallationTab.Location = new System.Drawing.Point(4, 24);
            this.InstallationTab.Name = "InstallationTab";
            this.InstallationTab.Padding = new System.Windows.Forms.Padding(3);
            this.InstallationTab.Size = new System.Drawing.Size(434, 198);
            this.InstallationTab.TabIndex = 2;
            this.InstallationTab.Text = "Installation";
            this.InstallationTab.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.ToggleDeathSound);
            this.groupBox4.Location = new System.Drawing.Point(5, 60);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(422, 46);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Modifications";
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
            this.GroupBoxInstallLocation.Text = "Install Location";
            // 
            // InstallLocationBrowseButton
            // 
            this.InstallLocationBrowseButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InstallLocationBrowseButton.Location = new System.Drawing.Point(328, 20);
            this.InstallLocationBrowseButton.Name = "InstallLocationBrowseButton";
            this.InstallLocationBrowseButton.Size = new System.Drawing.Size(86, 25);
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
            this.InstallLocation.Size = new System.Drawing.Size(312, 23);
            this.InstallLocation.TabIndex = 4;
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
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.PreviewButton);
            this.panel1.Controls.Add(this.SaveButton);
            this.panel1.Location = new System.Drawing.Point(-1, 277);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(466, 42);
            this.panel1.TabIndex = 6;
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
            this.ClientSize = new System.Drawing.Size(464, 318);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preferences";
            this.Tabs.ResumeLayout(false);
            this.DialogTab.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IconPreview)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.InstallationTab.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.GroupBoxInstallLocation.ResumeLayout(false);
            this.GroupBoxInstallLocation.PerformLayout();
            this.panel1.ResumeLayout(false);
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
    }
}