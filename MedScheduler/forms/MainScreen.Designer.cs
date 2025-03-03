namespace MedScheduler
{
    partial class MainScreen
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
            this.MedScheduler = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.DataCenter = new System.Windows.Forms.Panel();
            this.surgeon = new System.Windows.Forms.PictureBox();
            this.patient = new System.Windows.Forms.PictureBox();
            this.doctor = new System.Windows.Forms.PictureBox();
            this.modernButton1 = new MedScheduler.StyleUtils.ModernButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.DataCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.surgeon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.patient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.doctor)).BeginInit();
            this.SuspendLayout();
            // 
            // MedScheduler
            // 
            this.MedScheduler.AutoSize = true;
            this.MedScheduler.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MedScheduler.Location = new System.Drawing.Point(240, 27);
            this.MedScheduler.Name = "MedScheduler";
            this.MedScheduler.Size = new System.Drawing.Size(733, 82);
            this.MedScheduler.TabIndex = 0;
            this.MedScheduler.Text = "MedScheduler(Alpha)";
            this.MedScheduler.Click += new System.EventHandler(this.label1_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(314, 506);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(576, 102);
            this.button1.TabIndex = 2;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MedScheduler.Properties.Resources.Logo;
            this.pictureBox1.Location = new System.Drawing.Point(314, 122);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(576, 356);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // DataCenter
            // 
            this.DataCenter.Controls.Add(this.label3);
            this.DataCenter.Controls.Add(this.label2);
            this.DataCenter.Controls.Add(this.label1);
            this.DataCenter.Controls.Add(this.modernButton1);
            this.DataCenter.Controls.Add(this.doctor);
            this.DataCenter.Controls.Add(this.patient);
            this.DataCenter.Controls.Add(this.surgeon);
            this.DataCenter.Location = new System.Drawing.Point(1125, 27);
            this.DataCenter.Name = "DataCenter";
            this.DataCenter.Size = new System.Drawing.Size(1208, 688);
            this.DataCenter.TabIndex = 3;
            this.DataCenter.Visible = false;
            this.DataCenter.Paint += new System.Windows.Forms.PaintEventHandler(this.DataCenter_Paint);
            // 
            // surgeon
            // 
            this.surgeon.Image = global::MedScheduler.Properties.Resources.Surgeon;
            this.surgeon.Location = new System.Drawing.Point(103, 70);
            this.surgeon.Name = "surgeon";
            this.surgeon.Size = new System.Drawing.Size(224, 238);
            this.surgeon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.surgeon.TabIndex = 0;
            this.surgeon.TabStop = false;
            // 
            // patient
            // 
            this.patient.Image = global::MedScheduler.Properties.Resources.patient;
            this.patient.Location = new System.Drawing.Point(476, 70);
            this.patient.Name = "patient";
            this.patient.Size = new System.Drawing.Size(227, 238);
            this.patient.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.patient.TabIndex = 1;
            this.patient.TabStop = false;
            // 
            // doctor
            // 
            this.doctor.Image = global::MedScheduler.Properties.Resources.Doctor;
            this.doctor.Location = new System.Drawing.Point(805, 70);
            this.doctor.Name = "doctor";
            this.doctor.Size = new System.Drawing.Size(236, 238);
            this.doctor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.doctor.TabIndex = 2;
            this.doctor.TabStop = false;
            // 
            // modernButton1
            // 
            this.modernButton1.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.modernButton1.BackgroundColor = System.Drawing.Color.DarkOliveGreen;
            this.modernButton1.BorderColor = System.Drawing.Color.PaleVioletRed;
            this.modernButton1.BorderRadius = 20;
            this.modernButton1.BorderSize = 0;
            this.modernButton1.FlatAppearance.BorderSize = 0;
            this.modernButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.modernButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modernButton1.ForeColor = System.Drawing.Color.White;
            this.modernButton1.Location = new System.Drawing.Point(382, 441);
            this.modernButton1.Name = "modernButton1";
            this.modernButton1.Size = new System.Drawing.Size(419, 164);
            this.modernButton1.TabIndex = 3;
            this.modernButton1.Text = "Schedule";
            this.modernButton1.TextColor = System.Drawing.Color.White;
            this.modernButton1.UseVisualStyleBackColor = false;
            this.modernButton1.Click += new System.EventHandler(this.modernButton1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(137, 328);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 37);
            this.label1.TabIndex = 4;
            this.label1.Text = "Surgeons";
            this.label1.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(495, 328);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 37);
            this.label2.TabIndex = 5;
            this.label2.Text = "Patients";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(845, 328);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 37);
            this.label3.TabIndex = 6;
            this.label3.Text = "Doctors";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(2, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1199, 675);
            this.panel1.TabIndex = 4;
            // 
            // MainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 692);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.DataCenter);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.MedScheduler);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainScreen";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.DataCenter.ResumeLayout(false);
            this.DataCenter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.surgeon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.patient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.doctor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MedScheduler;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel DataCenter;
        private System.Windows.Forms.PictureBox doctor;
        private System.Windows.Forms.PictureBox patient;
        private System.Windows.Forms.PictureBox surgeon;
        private StyleUtils.ModernButton modernButton1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
    }
}

