using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Models;
using System.Linq;
using System.Collections.Generic;
using DB;

namespace MedScheduler.forms
{
    public partial class MainForm : Form
    {
        // Member variables for controls we want accessible in the designer
        private Panel headerPanel;
        private Label headerLabel;
        private Panel navPanel;
        private Panel contentPanel;
        private Panel currentlyActiveNavItem;

        // Dashboard is already there from your code
        public static DoctorsForm doctorsForm;
        public static PatientForm patientForm;
        private GeneticAlgorithmPanel geneticAlgorithmPanel;

        // Dashboard is already there from your code
            public static SchedulesForm schedulesForm;
            public static SchedulerOrchestrator s;
        
        public DataManager db = new DataManager();
        public static Schedule main = new Schedule();
        
        public MainForm()
        {
            DataSingelton.Instance.LoadDataFromDatabase(db);
            // First create the orchestrator and generate the schedule
           
            s = new SchedulerOrchestrator();
            
            

            geneticAlgorithmPanel = new GeneticAlgorithmPanel
            {
                Dock = DockStyle.Fill
            };
            // The rest of your initialization
            InitializeComponent();
            InitializeCustomComponents();

            this.Text = "MedScheduler";
            this.Size = new Size(1016, 739); // Added some extra space for window borders
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void InitializeCustomComponents()
        {
            // Create components
            this.SuspendLayout();
            this.BackColor = ColorTranslator.FromHtml("#ECF0F1");

            // Initialize header panel
            if (headerPanel == null)
            {
                headerPanel = new Panel
                {
                    BackColor = ColorTranslator.FromHtml("#2C3E50"),
                    Location = new Point(0, 0),
                    Size = new Size(1000, 60),
                    Dock = DockStyle.Top
                };
                this.Controls.Add(headerPanel);
            }

            // Initialize header label
            if (headerLabel == null)
            {
                headerLabel = new Label
                {
                    Text = "MedScheduler",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    Location = new Point(20, 10),
                    AutoSize = true
                };
                headerPanel.Controls.Add(headerLabel);
            }

            // Initialize navigation panel
            if (navPanel == null)
            {
                navPanel = new Panel
                {
                    BackColor = ColorTranslator.FromHtml("#34495E"),
                    Location = new Point(0, 60),
                    Size = new Size(200, 640),
                    Dock = DockStyle.Left
                };
                this.Controls.Add(navPanel);
            }

            // Initialize content panel
            if (contentPanel == null)
            {
                contentPanel = new Panel
                {
                    BackColor = ColorTranslator.FromHtml("#ECF0F1"),
                    Location = new Point(200, 60),
                    Size = new Size(800, 640),
                    Dock = DockStyle.Fill
                };
                this.Controls.Add(contentPanel);
            }

            // Initialize page user controls
            doctorsForm = new DoctorsForm();
            patientForm = new PatientForm();
            // Create navigation items
            string[] navItems = { "Dashboard", "Doctors", "Patients", "Surgeries","Algorithm" };
            for (int i = 0; i < navItems.Length; i++)
            {
                Panel navItemPanel = new Panel
                {
                    Name = "navItem" + i,
                    Location = new Point(0, 60 + i * 50),
                    Size = new Size(200, 50),
                    BackColor = i == 0 ? ColorTranslator.FromHtml("#3498DB") : ColorTranslator.FromHtml("#34495E"),
                    Tag = i // Store the index for later identification
                };

                // Set initial active nav item
                if (i == 0)
                {
                    currentlyActiveNavItem = navItemPanel;
                }

                Label navLabel = new Label
                {
                    Text = navItems[i],
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    Location = new Point(50, 14),
                    AutoSize = true
                };

                // Add circle indicator
                navItemPanel.Paint += (sender, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(Brushes.White, 22, 16, 16, 16);
                };

                // Add click handler to navigate between pages
                int pageIndex = i; // Capture the value for the lambda
                navItemPanel.Click += (sender, e) => NavigateToPage(pageIndex, (Panel)sender);
                navLabel.Click += (sender, e) => NavigateToPage(pageIndex, navItemPanel);

                // Make the panel and label have a hand cursor to indicate they're clickable
                navItemPanel.Cursor = Cursors.Hand;
                navLabel.Cursor = Cursors.Hand;

                navItemPanel.Controls.Add(navLabel);
                navPanel.Controls.Add(navItemPanel);
            }

            // Create Dashboard Content (your existing code)
            Panel dashboardContent = CreateDashboardContent();
            contentPanel.Controls.Add(dashboardContent);
            dashboardContent.Dock = DockStyle.Fill;
            dashboardContent.BringToFront();

            this.ResumeLayout(false);
        }

        private void NavigateToPage(int pageIndex, Panel navItem)
        {
            // Update navigation highlight
            if (currentlyActiveNavItem != null)
            {
                currentlyActiveNavItem.BackColor = ColorTranslator.FromHtml("#34495E");
            }
            navItem.BackColor = ColorTranslator.FromHtml("#3498DB");
            currentlyActiveNavItem = navItem;

            // Clear current content
            contentPanel.Controls.Clear();

            // Add selected page content
            switch (pageIndex)
            {
                case 0: // Dashboard
                    Panel dashboardContent = CreateDashboardContent();
                    contentPanel.Controls.Add(dashboardContent);
                    dashboardContent.Dock = DockStyle.Fill;
                    dashboardContent.BringToFront();
                    break;
                case 1: // Doctors
                    contentPanel.Controls.Add(doctorsForm);
                    doctorsForm.BringToFront();
                    break;
                case 2: // Patients
                    contentPanel.Controls.Add(patientForm);
                    patientForm.BringToFront();
                    
                    break;
                case 3: // Schedules
                    if(schedulesForm != null)
                    {
                        contentPanel.Controls.Add(schedulesForm);
                        schedulesForm.BringToFront();
                    }
                    break;
                case 4:
                    
                    contentPanel.Controls.Add(geneticAlgorithmPanel);
                    geneticAlgorithmPanel.BringToFront();
                    break;
            }
        }

        private void ShowPlaceholderPage(string pageName)
        {
            Panel placeholderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#ECF0F1")
            };

            Label placeholderLabel = new Label
            {
                Text = pageName + " Page",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                AutoSize = true,
                Location = new Point(50, 50)
            };

            Label infoLabel = new Label
            {
                Text = "This page is under construction",
                Font = new Font("Segoe UI", 16),
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                AutoSize = true,
                Location = new Point(50, 100)
            };

            placeholderPanel.Controls.Add(placeholderLabel);
            placeholderPanel.Controls.Add(infoLabel);
            contentPanel.Controls.Add(placeholderPanel);
        }

        private Panel CreateDashboardContent()
        {
            // Create a container panel for dashboard content
            Panel dashboardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var doctors = db.GetDoctors();
            var patients = db.GetPatients();

            // Dashboard Title
            Label dashboardTitle = new Label
            {
                Text = "Dashboard",
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            dashboardPanel.Controls.Add(dashboardTitle);

            // Stats Cards
            CreateStatsCard(dashboardPanel, 20, 60, "Total Doctors", doctors.Count.ToString(), "👨‍⚕️", "#3498DB");
            CreateStatsCard(dashboardPanel, 280, 60, "Total Patients", patients.Count.ToString(), "🤒", "#E74C3C");
            CreateStatsCard(dashboardPanel, 540, 60, "Scheduled Surgeries", 404.ToString(), "🔪", "#2ECC71");

            // Today's Schedule Panel
            Panel schedulePanel = CreatePanel(dashboardPanel, 20, 200, 760, 200, "Today's Schedule");

            // Schedule Table Header
            Panel tableHeader = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                Location = new Point(20, 50),
                Size = new Size(720, 30)
            };

            string[] columnHeaders = { "Patient", "Condition", "Doctor", "Specialization", "Time", "Status" };
            int[] columnPositions = { 10, 130, 280, 410, 560, 660 };

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                Label headerLabelinner = new Label
                {
                    Text = columnHeaders[i],
                    ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(columnPositions[i], 5),
                    AutoSize = true
                };
                tableHeader.Controls.Add(headerLabelinner);
            }
            schedulePanel.Controls.Add(tableHeader);

            // Schedule Table Rows
            AddScheduleRow(schedulePanel, 20, 90, "John Doe", "Heart Disease", "Dr. Smith", "Cardiology", "9:00 AM", "DONE", "#2ECC71");
            AddScheduleRow(schedulePanel, 20, 120, "Jane Smith", "Broken Leg", "Dr. Johnson", "Orthopedics", "10:30 AM", "ACTIVE", "#3498DB");
            AddScheduleRow(schedulePanel, 20, 150, "Bob Johnson", "Heart Attack", "Dr. Williams", "Cardiology", "1:00 PM", "URGENT", "#E74C3C", true);

            // Algorithm Performance Panel
            Panel algoPanel = CreatePanel(dashboardPanel, 20, 420, 370, 200, "Algorithm Performance");

            // Add graph
            algoPanel.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw axes
                using (Pen grayPen = new Pen(ColorTranslator.FromHtml("#7F8C8D"), 1))
                {
                    e.Graphics.DrawLine(grayPen, 30, 150, 330, 150); // X axis
                    e.Graphics.DrawLine(grayPen, 30, 50, 30, 150);  // Y axis
                }

                // Draw labels
                using (Font smallFont = new Font("Segoe UI", 10))
                {
                    e.Graphics.DrawString("Generations", smallFont, Brushes.Gray, 160, 160);

                    // Y axis label (rotated)
                    e.Graphics.TranslateTransform(15, 100);
                    e.Graphics.RotateTransform(-90);
                    e.Graphics.DrawString("Fitness Score", smallFont, Brushes.Gray, 0, 0);
                    e.Graphics.ResetTransform();
                }

                // Draw curve
                using (Pen bluePen = new Pen(ColorTranslator.FromHtml("#3498DB"), 2))
                {
                    Point[] points = {
                        new Point(30, 140),
                        new Point(80, 120),
                        new Point(130, 80),
                        new Point(180, 70),
                        new Point(230, 60),
                        new Point(280, 50),
                        new Point(330, 50)
                    };

                    e.Graphics.DrawCurve(bluePen, points, 0.5f);
                }
            };

            // Schedule Metrics Panel
            Panel metricsPanel = CreatePanel(dashboardPanel, 410, 420, 370, 200, "Schedule Metrics");


            string[] metrics = {
    "Specialization Match Rate:", $"{s.finalStatistics.specializationMatchRate:F1}%", "#2ECC71",
    "Average Doctor Workload:", $"{s.finalStatistics.AvarageDoctorWorkLoad:F1}%", "#3498DB",
    "Patients Assigned:", $"{s.finalStatistics.assignmentPercentage}%", "#2C3E50"
};

            for (int i = 0; i < 3; i++)
            {
                Label metricLabel = new Label
                {
                    Text = metrics[i * 3],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    Location = new Point(20, 50 + i * 30),
                    AutoSize = true
                };

                Label valueLabel = new Label
                {
                    Text = metrics[i * 3 + 1],
                    ForeColor = ColorTranslator.FromHtml(metrics[i * 3 + 2]),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(250, 50 + i * 30),
                    TextAlign = ContentAlignment.MiddleRight,
                    AutoSize = true
                };

                metricsPanel.Controls.Add(metricLabel);
                metricsPanel.Controls.Add(valueLabel);
            }

            return dashboardPanel;
        }

        private Panel CreatePanel(Control parent, int x, int y, int width, int height, string title)
        {
            Panel panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            panel.Controls.Add(titleLabel);
            parent.Controls.Add(panel);

            return panel;
        }

        private void CreateStatsCard(Control parent, int x, int y, string title, string value, string emoji, string color)
        {
            Panel cardPanel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(240, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label valueLabel = new Label
            {
                Text = value,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                Location = new Point(20, 45),
                AutoSize = true
            };

            Panel emojiPanel = new Panel
            {
                Location = new Point(160, 50),
                Size = new Size(60, 50),
                BackColor = ColorTranslator.FromHtml(color)
            };
            emojiPanel.BackColor = Color.FromArgb(50, emojiPanel.BackColor);

            Label emojiLabel = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 28, FontStyle.Regular),
                ForeColor = ColorTranslator.FromHtml(color),
                Location = new Point(10, 5),
                AutoSize = true
            };

            emojiPanel.Controls.Add(emojiLabel);
            cardPanel.Controls.Add(emojiPanel);
            cardPanel.Controls.Add(titleLabel);
            cardPanel.Controls.Add(valueLabel);

            parent.Controls.Add(cardPanel);
        }

        private void AddScheduleRow(Panel parent, int x, int y, string patient, string condition, string doctor,
                                   string specialization, string time, string status, string statusColor, bool isUrgent = false)
        {
            int[] columnPositions = { 10, 130, 280, 410, 560, 660 };

            Panel rowPanel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(720, 25),
                BackColor = Color.Transparent
            };

            string[] rowData = { patient, condition, doctor, specialization, time };

            for (int i = 0; i < rowData.Length; i++)
            {
                Label dataLabel = new Label
                {
                    Text = rowData[i],
                    ForeColor = i == 1 && isUrgent ? ColorTranslator.FromHtml("#E74C3C") : ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 12, isUrgent && i == 1 ? FontStyle.Bold : FontStyle.Regular),
                    Location = new Point(columnPositions[i], 0),
                    AutoSize = true
                };
                rowPanel.Controls.Add(dataLabel);
            }

            // Status badge
            Panel statusPanel = new Panel
            {
                Location = new Point(columnPositions[5], 0),
                Size = new Size(60, 20),
                BackColor = ColorTranslator.FromHtml(statusColor)
            };

            // Make it semi-transparent
            statusPanel.BackColor = Color.FromArgb(50, statusPanel.BackColor);

            // Round the corners
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(statusPanel.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(statusPanel.Width - 20, statusPanel.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, statusPanel.Height - 20, 20, 20, 90, 90);
            statusPanel.Region = new Region(path);

            Label statusLabel = new Label
            {
                Text = status,
                ForeColor = ColorTranslator.FromHtml(statusColor),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(12, 2),
                AutoSize = true
            };

            statusPanel.Controls.Add(statusLabel);
            rowPanel.Controls.Add(statusPanel);

            // Add divider line
            rowPanel.Paint += (sender, e) =>
            {
                using (Pen grayPen = new Pen(ColorTranslator.FromHtml("#ECF0F1"), 1))
                {
                    e.Graphics.DrawLine(grayPen, 0, 24, 720, 24);
                }
            };

            parent.Controls.Add(rowPanel);
        }


        private void contentPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}