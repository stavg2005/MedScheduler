namespace MedScheduler.forms
{
    partial class SchedulesForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scheduleTitle = new System.Windows.Forms.Label();
            this.controlsPanel = new System.Windows.Forms.Panel();
            this.monthLabel = new System.Windows.Forms.Label();
            this.dayButton = new System.Windows.Forms.Button();
            this.weekButton = new System.Windows.Forms.Button();
            this.monthButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.calendarPanel = new System.Windows.Forms.Panel();
            this.controlsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // scheduleTitle
            // 
            this.scheduleTitle.AutoSize = true;
            this.scheduleTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scheduleTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.scheduleTitle.Location = new System.Drawing.Point(30, 31);
            this.scheduleTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.scheduleTitle.Name = "scheduleTitle";
            this.scheduleTitle.Size = new System.Drawing.Size(253, 65);
            this.scheduleTitle.TabIndex = 0;
            this.scheduleTitle.Text = "Schedules";
            // 
            // controlsPanel
            // 
            this.controlsPanel.BackColor = System.Drawing.Color.White;
            this.controlsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.controlsPanel.Controls.Add(this.monthLabel);
            this.controlsPanel.Controls.Add(this.dayButton);
            this.controlsPanel.Controls.Add(this.weekButton);
            this.controlsPanel.Controls.Add(this.monthButton);
            this.controlsPanel.Controls.Add(this.generateButton);
            this.controlsPanel.Controls.Add(this.exportButton);
            this.controlsPanel.Location = new System.Drawing.Point(30, 92);
            this.controlsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Size = new System.Drawing.Size(1139, 91);
            this.controlsPanel.TabIndex = 1;
            // 
            // monthLabel
            // 
            this.monthLabel.AutoSize = true;
            this.monthLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.monthLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.monthLabel.Location = new System.Drawing.Point(30, 26);
            this.monthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.monthLabel.Name = "monthLabel";
            this.monthLabel.Size = new System.Drawing.Size(196, 45);
            this.monthLabel.TabIndex = 0;
            this.monthLabel.Text = "March 2025";
            // 
            // dayButton
            // 
            this.dayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.dayButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dayButton.FlatAppearance.BorderSize = 0;
            this.dayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dayButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.dayButton.Location = new System.Drawing.Point(195, 23);
            this.dayButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dayButton.Name = "dayButton";
            this.dayButton.Size = new System.Drawing.Size(150, 46);
            this.dayButton.TabIndex = 1;
            this.dayButton.Text = "Day";
            this.dayButton.UseVisualStyleBackColor = false;
            // 
            // weekButton
            // 
            this.weekButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.weekButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.weekButton.FlatAppearance.BorderSize = 0;
            this.weekButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.weekButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weekButton.ForeColor = System.Drawing.Color.White;
            this.weekButton.Location = new System.Drawing.Point(360, 23);
            this.weekButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.weekButton.Name = "weekButton";
            this.weekButton.Size = new System.Drawing.Size(150, 46);
            this.weekButton.TabIndex = 2;
            this.weekButton.Text = "Week";
            this.weekButton.UseVisualStyleBackColor = false;
            // 
            // monthButton
            // 
            this.monthButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.monthButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.monthButton.FlatAppearance.BorderSize = 0;
            this.monthButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.monthButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.monthButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.monthButton.Location = new System.Drawing.Point(525, 23);
            this.monthButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.monthButton.Name = "monthButton";
            this.monthButton.Size = new System.Drawing.Size(150, 46);
            this.monthButton.TabIndex = 3;
            this.monthButton.Text = "Month";
            this.monthButton.UseVisualStyleBackColor = false;
            // 
            // generateButton
            // 
            this.generateButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.generateButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.generateButton.FlatAppearance.BorderSize = 0;
            this.generateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.generateButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateButton.ForeColor = System.Drawing.Color.White;
            this.generateButton.Location = new System.Drawing.Point(765, 23);
            this.generateButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(150, 46);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = false;
            // 
            // exportButton
            // 
            this.exportButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.exportButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.exportButton.FlatAppearance.BorderSize = 0;
            this.exportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exportButton.ForeColor = System.Drawing.Color.White;
            this.exportButton.Location = new System.Drawing.Point(930, 23);
            this.exportButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(150, 46);
            this.exportButton.TabIndex = 5;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = false;
            // 
            // calendarPanel
            // 
            this.calendarPanel.AutoScroll = true;
            this.calendarPanel.BackColor = System.Drawing.Color.White;
            this.calendarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.calendarPanel.Location = new System.Drawing.Point(41, 202);
            this.calendarPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.calendarPanel.Name = "calendarPanel";
            this.calendarPanel.Size = new System.Drawing.Size(1139, 753);
            this.calendarPanel.TabIndex = 2;
            this.calendarPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.calendarPanel_Paint);
            // 
            // SchedulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Controls.Add(this.scheduleTitle);
            this.Controls.Add(this.controlsPanel);
            this.Controls.Add(this.calendarPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "SchedulesForm";
            this.Size = new System.Drawing.Size(1200, 985);
            this.Load += new System.EventHandler(this.SchedulesForm_Load);
            this.controlsPanel.ResumeLayout(false);
            this.controlsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label scheduleTitle;
        private System.Windows.Forms.Panel controlsPanel;
        private System.Windows.Forms.Label monthLabel;
        private System.Windows.Forms.Button dayButton;
        private System.Windows.Forms.Button weekButton;
        private System.Windows.Forms.Button monthButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Panel calendarPanel;
    }
}