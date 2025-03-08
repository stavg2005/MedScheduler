using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Models;

namespace MedScheduler.forms
{
    public partial class DoctorsForm : UserControl
    {
        DataManager db = new DataManager();

        public DoctorsForm()
        {
            InitializeComponentDoctors();
            this.Size = new Size(1000, 700);
            this.Dock = DockStyle.Fill;
        }

        private void DoctorsForm_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponentDoctors()
        {
            // Create components
            this.SuspendLayout();
            this.BackColor = ColorTranslator.FromHtml("#ECF0F1");

            // Main Content Panel without header and navigation

            // Main Content Panel (full size since we don't have header/nav in the control)
            Panel contentPanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#ECF0F1"),
                Location = new Point(0, 0),
                Size = new Size(800, 640),
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Doctors Title
            Label doctorsTitle = new Label
            {
                Text = "Doctors",
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            contentPanel.Controls.Add(doctorsTitle);

            // Search and Filter Bar Panel
            Panel searchPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(760, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Search TextBox
            TextBox searchBox = new TextBox
            {
                Location = new Point(20, 15),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                Text = "Search doctors..."
            };
            searchBox.Enter += (sender, e) =>
            {
                if (searchBox.Text == "Search doctors...")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = ColorTranslator.FromHtml("#2C3E50");
                }
            };
            searchBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "Search doctors...";
                    searchBox.ForeColor = ColorTranslator.FromHtml("#7F8C8D");
                }
            };

            // Filter ComboBox
            ComboBox filterBox = new ComboBox
            {
                Location = new Point(340, 15),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                Text = "Filter by specialty"
            };
            string[] specialties = { "All Specialties", "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "Oncology" };
            filterBox.Items.AddRange(specialties);

            // Add New Doctor Button
            Button addButton = new Button
            {
                Location = new Point(510, 15),
                Size = new Size(150, 25),
                Text = "Add New Doctor",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#3498DB"),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;

            searchPanel.Controls.Add(searchBox);
            searchPanel.Controls.Add(filterBox);
            searchPanel.Controls.Add(addButton);
            contentPanel.Controls.Add(searchPanel);

            // Doctors Table Panel
            Panel tablePanel = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(760, 490),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Table Header
            Panel tableHeaderPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(758, 40),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                Dock = DockStyle.Top
            };

            string[] columnHeaders = { "Name", "Specialty", "Assigned Patients", "Status", "Actions" };
            int[] columnWidths = { 160, 160, 160, 100, 100 };
            int headerStart = 20;

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                Label headerLabel = new Label
                {
                    Text = columnHeaders[i],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(headerStart, 8),
                    AutoSize = true
                };
                headerStart += columnWidths[i];
                tableHeaderPanel.Controls.Add(headerLabel);
            }

            tablePanel.Controls.Add(tableHeaderPanel);

            // Add Doctor Rows
            string[] doctorNames = { "Dr. Sarah Smith", "Dr. Michael Johnson", "Dr. Emily Williams",
                                      "Dr. David Brown", "Dr. Jennifer Davis", "Dr. Robert Wilson" };
            string[] specialties2 = { "Cardiology", "Orthopedics", "Neurology", "Pediatrics", "Oncology", "Cardiology" };
            int[] patientCounts = { 12, 8, 10, 15, 9, 11 };
            string[] statuses = { "ACTIVE", "ACTIVE", "ON LEAVE", "ACTIVE", "ACTIVE", "SURGERY" };
            string[] statusColors = { "#2ECC71", "#2ECC71", "#E74C3C", "#2ECC71", "#2ECC71", "#F39C12" };

            for (int i = 0; i < doctorNames.Length; i++)
            {
                AddDoctorRow(tablePanel, 0, 40 + i * 40, doctorNames[i], specialties2[i],
                             patientCounts[i].ToString(), statuses[i], statusColors[i]);
            }

            contentPanel.Controls.Add(tablePanel);

            // Pagination Panel
            Panel paginationPanel = new Panel
            {
                Location = new Point(320, 630),
                Size = new Size(200, 30),
                BackColor = Color.White
            };

            string[] pageButtons = { "◀", "1", "2", "3", "4", "▶" };
            for (int i = 0; i < pageButtons.Length; i++)
            {
                Button pageButton = new Button
                {
                    Text = pageButtons[i],
                    Location = new Point(i * 30, 0),
                    Size = new Size(30, 30),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12),
                    ForeColor = i == 0 || i == 1 || i == 5 ? ColorTranslator.FromHtml("#3498DB") : ColorTranslator.FromHtml("#7F8C8D"),
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };
                pageButton.FlatAppearance.BorderSize = 0;
                paginationPanel.Controls.Add(pageButton);
            }

            contentPanel.Controls.Add(paginationPanel);

            // Add panel to UserControl
            this.Controls.Add(contentPanel);

            this.ResumeLayout(false);
        }

        private void AddDoctorRow(Panel parent, int x, int y, string name, string specialty,
                                  string patientCount, string status, string statusColor)
        {
            Panel rowPanel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(758, 40),
                BackColor = Color.White
            };

            // Draw the horizontal line at the bottom of the row
            rowPanel.Paint += (sender, e) =>
            {
                using (Pen grayPen = new Pen(ColorTranslator.FromHtml("#ECF0F1"), 1))
                {
                    e.Graphics.DrawLine(grayPen, 0, 39, 758, 39);
                }
            };

            // Name Column
            Label nameLabel = new Label
            {
                Text = name,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 14),
                Location = new Point(20, 8),
                AutoSize = true
            };

            // Specialty Column
            Label specialtyLabel = new Label
            {
                Text = specialty,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 14),
                Location = new Point(180, 8),
                AutoSize = true
            };

            // Patient Count Column
            Label patientLabel = new Label
            {
                Text = patientCount,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 14),
                Location = new Point(340, 8),
                AutoSize = true
            };

            // Status Badge
            Panel statusPanel = new Panel
            {
                Location = new Point(500, 8),
                Size = new Size(80, 25),
                BackColor = ColorTranslator.FromHtml(statusColor)
            };
            // Make it semi-transparent
            statusPanel.BackColor = Color.FromArgb(50, statusPanel.BackColor);

            // Round the corners
            GraphicsPath path = new GraphicsPath();
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
                Location = new Point(status.Length > 6 ? 10 : 15, 2),
                AutoSize = true
            };

            statusPanel.Controls.Add(statusLabel);

            // Action Buttons
            Button editButton = CreateActionButton("E", "#3498DB", 660, 8);
            Button deleteButton = CreateActionButton("D", "#E74C3C", 695, 8);

            rowPanel.Controls.Add(nameLabel);
            rowPanel.Controls.Add(specialtyLabel);
            rowPanel.Controls.Add(patientLabel);
            rowPanel.Controls.Add(statusPanel);
            rowPanel.Controls.Add(editButton);
            rowPanel.Controls.Add(deleteButton);

            parent.Controls.Add(rowPanel);
        }

        private Button CreateActionButton(string text, string color, int x, int y)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml(color),
                BackColor = Color.FromArgb(50, ColorTranslator.FromHtml(color)),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}