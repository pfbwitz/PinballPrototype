﻿namespace Sketchball
{
    partial class PlayForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMachineButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugModeButton = new System.Windows.Forms.ToolStripMenuItem();
            this.fullscreenButton = new System.Windows.Forms.ToolStripMenuItem();
            this.mainContainer = new System.Windows.Forms.ToolStripContainer();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1.SuspendLayout();
            this.mainContainer.TopToolStripPanel.SuspendLayout();
            this.mainContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(785, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.editToolStripMenuItem,
            this.closeMachineButton,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem,
            this.debugModeButton,
            this.fullscreenButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.onFileMenuOpening);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.startToolStripMenuItem.Text = "Reset";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.onResetClicked);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.onPauseClicked);
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Visible = false;
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.onResumeClicked);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.editToolStripMenuItem.Text = "Edit Machine";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.onEditClicked);
            // 
            // closeMachineButton
            // 
            this.closeMachineButton.Name = "closeMachineButton";
            this.closeMachineButton.Size = new System.Drawing.Size(156, 22);
            this.closeMachineButton.Text = "Close Machine";
            this.closeMachineButton.Click += new System.EventHandler(this.onCloseMachine);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.onExitClicked);
            // 
            // debugModeButton
            // 
            this.debugModeButton.CheckOnClick = true;
            this.debugModeButton.Name = "debugModeButton";
            this.debugModeButton.Size = new System.Drawing.Size(156, 22);
            this.debugModeButton.Text = "Debug Mode";
            this.debugModeButton.Visible = false;
            this.debugModeButton.CheckedChanged += new System.EventHandler(this.onDebugModeChanged);
            // 
            // fullscreenButton
            // 
            this.fullscreenButton.CheckOnClick = true;
            this.fullscreenButton.Name = "fullscreenButton";
            this.fullscreenButton.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.fullscreenButton.Size = new System.Drawing.Size(156, 22);
            this.fullscreenButton.Text = "Full Screen";
            this.fullscreenButton.CheckedChanged += new System.EventHandler(this.onSwitchFullscreen);
            // 
            // mainContainer
            // 
            // 
            // mainContainer.ContentPanel
            // 
            this.mainContainer.ContentPanel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.mainContainer.ContentPanel.Size = new System.Drawing.Size(785, 610);
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(0, 0);
            this.mainContainer.Name = "mainContainer";
            this.mainContainer.Size = new System.Drawing.Size(785, 634);
            this.mainContainer.TabIndex = 1;
            this.mainContainer.Text = "toolStripContainer1";
            // 
            // mainContainer.TopToolStripPanel
            // 
            this.mainContainer.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // PlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(785, 634);
            this.Controls.Add(this.mainContainer);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "PlayForm";
            this.Text = "PlayForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.mainContainer.TopToolStripPanel.ResumeLayout(false);
            this.mainContainer.TopToolStripPanel.PerformLayout();
            this.mainContainer.ResumeLayout(false);
            this.mainContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resumeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugModeButton;
        private System.Windows.Forms.ToolStripContainer mainContainer;
        private System.Windows.Forms.ToolStripMenuItem fullscreenButton;
        private System.Windows.Forms.ToolStripMenuItem closeMachineButton;
        private System.Windows.Forms.ImageList imageList1;
    }
}