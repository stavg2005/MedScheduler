using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Models;

namespace MedScheduler.forms
{
    public partial class SchedulesForm : UserControl
    {
        DataManager db = new DataManager();

        public SchedulesForm()
        {
            InitializeComponentinner();
            this.Size = new Size(1000, 700);
            this.Dock = DockStyle.Fill;
        }

        private void SchedulesForm_Load(object sender, EventArgs e)
        {

        }

        private void InitializeComponentinner()
        {
            this.SuspendLayout();
            // 
            // SchedulesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Name = "SchedulesForm";
            this.Size = new System.Drawing.Size(800, 640);
            this.Load += new System.EventHandler(this.SchedulesForm_Load);
            this.ResumeLayout(false);

            InitializeScheduleComponents();
        }

        private void InitializeScheduleComponents()
        {
            // Create components
            this.SuspendLayout();
            this.BackColor = ColorTranslator.FromHtml("#ECF0F1");

            // Content Title
            Label scheduleTitle = new Label
            {
                Text = "Schedules",
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(scheduleTitle);

            // Date Selector and Controls Panel
            Panel controlsPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(760, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Month Label
            Label monthLabel = new Label
            {
                Text = "March 2025",
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 17),
                AutoSize = true
            };
            controlsPanel.Controls.Add(monthLabel);

            // View Type Buttons
            string[] viewTypes = { "Day", "Week", "Month" };
            for (int i = 0; i < viewTypes.Length; i++)
            {
                Button viewButton = new Button
                {
                    Text = viewTypes[i],
                    Location = new Point(130 + i * 110, 15),
                    Size = new Size(100, 30),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12),
                    ForeColor = i == 1 ? Color.White : ColorTranslator.FromHtml("#2C3E50"),
                    BackColor = i == 1 ? ColorTranslator.FromHtml("#3498DB") : ColorTranslator.FromHtml("#F8F9FA"),
                    Cursor = Cursors.Hand
                };
                viewButton.FlatAppearance.BorderSize = 0;
                controlsPanel.Controls.Add(viewButton);
            }

            // Generate Button
            Button generateButton = new Button
            {
                Text = "Generate",
                Location = new Point(510, 15),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#2ECC71"),
                Cursor = Cursors.Hand
            };
            generateButton.FlatAppearance.BorderSize = 0;
            controlsPanel.Controls.Add(generateButton);

            // Export Button
            Button exportButton = new Button
            {
                Text = "Export",
                Location = new Point(620, 15),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#3498DB"),
                Cursor = Cursors.Hand
            };
            exportButton.FlatAppearance.BorderSize = 0;
            controlsPanel.Controls.Add(exportButton);

            this.Controls.Add(controlsPanel);

            // Week Schedule Calendar Panel
            Panel calendarPanel = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(760, 490),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            int columnWidth = 110;
            int rowHeight = 40;
            int timeColumnWidth = 100;

            // Calendar Headers
            string[] headers = { "Time", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Weekend" };
            string[] dates = { "", "Mar 10", "Mar 11", "Mar 12", "Mar 13", "Mar 14", "Mar 15-16" };

            for (int i = 0; i < headers.Length; i++)
            {
                Panel headerPanel = new Panel
                {
                    Location = new Point(i == 0 ? 0 : (timeColumnWidth + (i - 1) * columnWidth), 0),
                    Size = new Size(i == 0 ? timeColumnWidth : columnWidth, rowHeight),
                    BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label headerLabel = new Label
                {
                    Text = headers[i],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(i == 0 ? 10 : (i == 3 ? 10 : 20), 10),
                    AutoSize = true
                };
                headerPanel.Controls.Add(headerLabel);

                if (i > 0)
                {
                    Label dateLabel = new Label
                    {
                        Text = dates[i],
                        ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                        Font = new Font("Segoe UI", 9),
                        Location = new Point(20, 25),
                        AutoSize = true
                    };
                    headerPanel.Controls.Add(dateLabel);
                }

                calendarPanel.Controls.Add(headerPanel);
            }

            // Time Slots and Calendar Cells
            string[] timeSlots = { "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM",
                                  "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM", "5:00 PM" };

            for (int row = 0; row < timeSlots.Length; row++)
            {
                // Time Label
                Panel timePanel = new Panel
                {
                    Location = new Point(0, rowHeight + row * rowHeight),
                    Size = new Size(timeColumnWidth, rowHeight),
                    BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label timeLabel = new Label
                {
                    Text = timeSlots[row],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 12),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                timePanel.Controls.Add(timeLabel);
                calendarPanel.Controls.Add(timePanel);

                // Calendar Cells
                for (int col = 0; col < 6; col++)
                {
                    Panel cellPanel = new Panel
                    {
                        Location = new Point(timeColumnWidth + col * columnWidth, rowHeight + row * rowHeight),
                        Size = new Size(columnWidth, rowHeight),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    calendarPanel.Controls.Add(cellPanel);
                }
            }

            // Add Scheduled Events
            AddScheduledEvent(calendarPanel, 1, 1, 2, "B. Johnson", "Heart Transplant", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);
            AddScheduledEvent(calendarPanel, 2, 1, 2, "J. Doe", "Cardiac Bypass", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
            AddScheduledEvent(calendarPanel, 3, 2, 2, "E. Wilson", "Appendectomy", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
            AddScheduledEvent(calendarPanel, 5, 5, 2, "S. Davis", "Tumor Removal", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
            AddScheduledEvent(calendarPanel, 2, 3, 2, "J. Smith", "Leg Surgery", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
            AddScheduledEvent(calendarPanel, 4, 4, 2, "M. Brown", "Lung Biopsy", "#3498DB", timeColumnWidth, columnWidth, rowHeight);

            this.Controls.Add(calendarPanel);

            this.ResumeLayout(false);
        }

        private void AddScheduledEvent(Panel parent, int column, int startRow, int rowSpan, string name, string procedure, string color, int timeColumnWidth, int columnWidth, int rowHeight)
        {
            Panel eventPanel = new Panel
            {
                Location = new Point(timeColumnWidth + (column - 1) * columnWidth, rowHeight + startRow * rowHeight),
                Size = new Size(columnWidth, rowHeight * rowSpan),
                BackColor = Color.FromArgb(50, ColorTranslator.FromHtml(color)),
                BorderStyle = BorderStyle.None
            };

            // Round the corners
            GraphicsPath path = new GraphicsPath();
            int radius = 5;
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(eventPanel.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(eventPanel.Width - radius * 2, eventPanel.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, eventPanel.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            eventPanel.Region = new Region(path);

            // Add a border
            eventPanel.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen borderPen = new Pen(ColorTranslator.FromHtml(color), 1))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            };

            // Name Label
            Label nameLabel = new Label
            {
                Text = name,
                ForeColor = ColorTranslator.FromHtml(color),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 5),
                AutoSize = true
            };
            eventPanel.Controls.Add(nameLabel);

            // Procedure Label
            Label procedureLabel = new Label
            {
                Text = procedure,
                ForeColor = ColorTranslator.FromHtml(color),
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 25),
                AutoSize = true
            };
            eventPanel.Controls.Add(procedureLabel);

            parent.Controls.Add(eventPanel);
            eventPanel.BringToFront();
        }
    }
}