using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Models;
using System.Collections.Generic;
using System.Linq;
using DB;

namespace MedScheduler.forms
{
    public partial class SchedulesForm : UserControl
    {
        
        private List<Doctor> doctors;
        private List<Patient> patients;
        private List<OperatingRoom> operatingRooms;

        // Calendar configuration
        private int columnWidth = 110;
        private int rowHeight = 40;
        private int timeColumnWidth = 100;
        private DateTime startDate;

        // Room filter
        private int? selectedRoomId = null;
        private ComboBox roomSelector;

        public SchedulesForm(Schedule schedule = null)
        {
            InitializeComponent();
            this.Size = new Size(1000, 700);
            this.Dock = DockStyle.Fill;

            // Get data
            doctors = DataSingelton.Instance.Doctors;
            patients = DataSingelton.Instance.Patients;
            operatingRooms = DataSingelton.Instance.OperatingRooms;

            // If no schedule is explicitly provided, use the one from MainForm
            if (schedule == null)
            {
                schedule = MainForm.main;
            }

            // Set start date to current Monday
            startDate = DateTime.Now.Date;
            while (startDate.DayOfWeek != DayOfWeek.Monday)
            {
                startDate = startDate.AddDays(-1);
            }

            // Try to find the first surgery date in the schedule
            if (schedule?.SurgerySchedule != null && schedule.SurgerySchedule.Any())
            {
                DateTime firstSurgeryDate = schedule.SurgerySchedule.Keys.OrderBy(d => d).First();

                // Set startDate to the Monday of the week containing the first surgery
                DateTime weekStart = firstSurgeryDate;
                while (weekStart.DayOfWeek != DayOfWeek.Monday)
                {
                    weekStart = weekStart.AddDays(-1);
                }

                startDate = weekStart;
            }

            InitializeComponentinner();
        }

        private void SchedulesForm_Load(object sender, EventArgs e)
        {
            // Additional setup if needed
        }

        private void InitializeComponentinner()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            this.BackColor = ColorTranslator.FromHtml("#ECF0F1");

            // Content Title
            Label scheduleTitle = new Label
            {
                Text = "Operating Room Schedules",
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

            // Month & Week Label
            string weekRange = $"{startDate.ToShortDateString()} - {startDate.AddDays(6).ToShortDateString()}";
            Label dateLabel = new Label
            {
                Text = weekRange,
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 17),
                AutoSize = true
            };
            controlsPanel.Controls.Add(dateLabel);

            // Room Selector
            Label roomLabel = new Label
            {
                Text = "Select Room:",
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Font = new Font("Segoe UI", 12),
                Location = new Point(250, 20),
                AutoSize = true
            };
            controlsPanel.Controls.Add(roomLabel);

            roomSelector = new ComboBox
            {
                Location = new Point(360, 17),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12)
            };

            // Add "All Rooms" option
            roomSelector.Items.Add(new { Name = "All Rooms", Id = -1 });

            // Add rooms from the database
            if (operatingRooms != null && operatingRooms.Any())
            {
                foreach (var room in operatingRooms)
                {
                    roomSelector.Items.Add(new { Name = room.Name ?? $"Room {room.Id}", Id = room.Id });
                }
            }
            else
            {
                // Add some demo rooms if none exist
                roomSelector.Items.Add(new { Name = "OR 1", Id = 1 });
                roomSelector.Items.Add(new { Name = "OR 2", Id = 2 });
                roomSelector.Items.Add(new { Name = "OR 3", Id = 3 });
            }

            roomSelector.DisplayMember = "Name";
            roomSelector.ValueMember = "Id";
            roomSelector.SelectedIndex = 0; // Select "All Rooms" by default

            roomSelector.SelectedIndexChanged += (sender, e) =>
            {
                // Get selected room ID
                dynamic selectedItem = roomSelector.SelectedItem;
                selectedRoomId = selectedItem.Id;

                if (selectedRoomId == -1) // "All Rooms" option
                {
                    selectedRoomId = null;
                }

                // Refresh the calendar with the selected room
                RefreshCalendar();
            };

            controlsPanel.Controls.Add(roomSelector);

            // Previous Week Button
            Button prevButton = new Button
            {
                Text = "◀ Previous",
                Location = new Point(550, 17),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#7F8C8D"),
                Cursor = Cursors.Hand
            };
            prevButton.FlatAppearance.BorderSize = 0;
            prevButton.Click += (sender, e) => {
                startDate = startDate.AddDays(-7);
                InitializeComponentinner();
            };
            controlsPanel.Controls.Add(prevButton);

            // Next Week Button
            Button nextButton = new Button
            {
                Text = "Next ▶",
                Location = new Point(650, 17),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#7F8C8D"),
                Cursor = Cursors.Hand
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += (sender, e) => {
                startDate = startDate.AddDays(7);
                InitializeComponentinner();
            };
            controlsPanel.Controls.Add(nextButton);

            this.Controls.Add(controlsPanel);

            // Create the calendar panel
            Panel calendarPanel = CreateCalendarPanel();
            this.Controls.Add(calendarPanel);

            this.ResumeLayout(false);
        }

        private Panel CreateCalendarPanel()
        {
            // Week Schedule Calendar Panel
            Panel calendarPanel = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(760, 490),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Name = "calendarPanel"
            };

            // Calendar Headers
            string[] headers = { "Time", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday/Sunday" };
            string[] dates = new string[7];
            dates[0] = "";

            // Generate date strings for the week
            for (int i = 0; i < 6; i++)
            {
                DateTime day = startDate.AddDays(i);
                dates[i + 1] = day.ToString("MMM dd");
            }

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
                    Label dateLabelinner = new Label
                    {
                        Text = dates[i],
                        ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                        Font = new Font("Segoe UI", 9),
                        Location = new Point(20, 25),
                        AutoSize = true
                    };
                    headerPanel.Controls.Add(dateLabelinner);
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

            // Populate the calendar with scheduled surgeries
            PopulateScheduledSurgeries(calendarPanel, timeColumnWidth, columnWidth, rowHeight);

            return calendarPanel;
        }

        private void RefreshCalendar()
        {
            // Find the existing calendar panel and remove it
            Control existingCalendar = this.Controls.Find("calendarPanel", false).FirstOrDefault();
            if (existingCalendar != null)
            {
                this.Controls.Remove(existingCalendar);
                existingCalendar.Dispose();
            }

            // Create a new calendar panel with updated data
            Panel calendarPanel = CreateCalendarPanel();
            this.Controls.Add(calendarPanel);
            calendarPanel.BringToFront();
        }

        private void PopulateScheduledSurgeries(Panel calendarPanel, int timeColumnWidth, int columnWidth, int rowHeight)
        {
            // Get the main schedule from the MainForm static property
            Schedule mainSchedule = MainForm.main;

            // Ensure the schedule exists and has surgery data
            if (mainSchedule?.SurgerySchedule == null || !mainSchedule.SurgerySchedule.Any())
            {
                AddRoomSpecificDemoSurgeries(calendarPanel, timeColumnWidth, columnWidth, rowHeight);
                return;
            }

            // Map days of week to calendar columns (Monday = 1, Tuesday = 2, etc.)
            Dictionary<DayOfWeek, int> dayToColumn = new Dictionary<DayOfWeek, int>
            {
                { DayOfWeek.Monday, 1 },
                { DayOfWeek.Tuesday, 2 },
                { DayOfWeek.Wednesday, 3 },
                { DayOfWeek.Thursday, 4 },
                { DayOfWeek.Friday, 5 },
                { DayOfWeek.Saturday, 6 },
                { DayOfWeek.Sunday, 6 }  // Weekend combined
            };

            // Time slots to row mapping (8:00 AM = row 0, 9:00 AM = row 1, etc.)
            Dictionary<int, int> hourToRow = new Dictionary<int, int>
            {
                { 8, 0 }, { 9, 1 }, { 10, 2 }, { 11, 3 }, { 12, 4 },
                { 13, 5 }, { 14, 6 }, { 15, 7 }, { 16, 8 }, { 17, 9 }
            };

            // Calculate the end date for the week view
            DateTime endDate = startDate.AddDays(6);

            bool addedAnySurgeries = false;

            // Iterate through surgery schedule
            foreach (var dateEntry in mainSchedule.SurgerySchedule)
            {
                DateTime surgeryDate = dateEntry.Key;

                // Skip if not in the current week view
                if (surgeryDate < startDate || surgeryDate > endDate)
                {
                    continue;
                }

                // Get day column
                if (!dayToColumn.TryGetValue(surgeryDate.DayOfWeek, out int column))
                {
                    continue;
                }

                foreach (var roomEntry in dateEntry.Value)
                {
                    int operatingRoomId = roomEntry.Key;

                    // Skip if room filter is active and doesn't match
                    if (selectedRoomId.HasValue && operatingRoomId != selectedRoomId.Value)
                    {
                        continue;
                    }

                    var patientsInRoom = roomEntry.Value;

                    // If no patients in this room on this day, skip
                    if (patientsInRoom == null || patientsInRoom.Count == 0) continue;

                    // For each patient scheduled in this room
                    foreach (int patientId in patientsInRoom)
                    {
                        // Find patient details
                        var patient = patients.FirstOrDefault(p => p.Id == patientId);
                        if (patient == null)
                        {
                            continue;
                        }

                        // Find assigned surgeon
                        int? surgeonId = patient.AssignedSurgeonId;
                        if (!surgeonId.HasValue)
                        {
                            continue;
                        }

                        var surgeon = doctors.FirstOrDefault(d => d.Id == surgeonId.Value);
                        if (surgeon == null)
                        {
                            continue;
                        }

                        // Find operating room
                        var operatingRoom = operatingRooms.FirstOrDefault(r => r.Id == operatingRoomId);
                        string roomName = operatingRoom?.Name ?? $"Room {operatingRoomId}";

                        // Determine row based on time
                        int hour = 8 + (operatingRoomId + patientId) % 10;  // Between 8 AM and 5 PM
                        int row = hourToRow.ContainsKey(hour) ? hourToRow[hour] : 0;

                        // Calculate duration based on complexity (between 1-3 hours)
                        int duration = Math.Max(1, patient.ComplexityLevel);

                        try
                        {
                            // Add the event to the calendar
                            // Include room name if showing all rooms
                            string patientName = patient.Name ?? $"Patient {patientId}";
                            string surgeryTitle = selectedRoomId.HasValue ?
                                patientName :
                                $"{patientName} (Room {operatingRoomId})";

                            AddScheduledEvent(calendarPanel, column, row, duration,
                                surgeryTitle,
                                patient.Condition ?? "Scheduled Surgery",
                                GetColorForSpecialization(surgeon.Specialization),
                                timeColumnWidth, columnWidth, rowHeight);

                            addedAnySurgeries = true;
                        }
                        catch (Exception)
                        {
                            // Handle any rendering errors
                            continue;
                        }
                    }
                }
            }

            // If we didn't find any surgeries for the current week or room, add demo data
            if (!addedAnySurgeries)
            {
                AddRoomSpecificDemoSurgeries(calendarPanel, timeColumnWidth, columnWidth, rowHeight);
            }
        }

        private void AddRoomSpecificDemoSurgeries(Panel calendarPanel, int timeColumnWidth, int columnWidth, int rowHeight)
        {
            // Clear existing events first
            ClearExistingEvents(calendarPanel);

            // Add room-specific demo data
            if (selectedRoomId.HasValue)
            {
                // Show surgeries for the specific room
                int roomId = selectedRoomId.Value;

                switch (roomId)
                {
                    case 1: // OR 1 - Cardiology Room
                        AddScheduledEvent(calendarPanel, 1, 1, 2, "John Doe", "Heart Surgery", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 3, 4, 2, "Mary Smith", "Pacemaker Implant", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 5, 2, 2, "James Wilson", "Cardiac Bypass", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);
                        break;

                    case 2: // OR 2 - Orthopedics Room
                        AddScheduledEvent(calendarPanel, 2, 1, 1, "Sarah Johnson", "Knee Replacement", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 4, 3, 1, "Mike Thompson", "Hip Surgery", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 5, 5, 1, "Lisa Brooks", "Shoulder Repair", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
                        break;

                    case 3: // OR 3 - Neurology Room
                        AddScheduledEvent(calendarPanel, 1, 3, 3, "Robert Brown", "Brain Tumor", "#2ECC71", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 3, 1, 2, "Jennifer Davis", "Spinal Surgery", "#2ECC71", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 4, 5, 2, "David Clark", "Nerve Repair", "#2ECC71", timeColumnWidth, columnWidth, rowHeight);
                        break;

                    default: // Other rooms
                        AddScheduledEvent(calendarPanel, 2, 2, 1, $"Patient in Room {roomId}", "Scheduled Surgery", "#34495E", timeColumnWidth, columnWidth, rowHeight);
                        AddScheduledEvent(calendarPanel, 4, 4, 1, $"Patient in Room {roomId}", "Scheduled Surgery", "#34495E", timeColumnWidth, columnWidth, rowHeight);
                        break;
                }
            }
            else
            {
                // Show surgeries for multiple rooms if "All Rooms" is selected
                // Room 1 - Cardiology
                AddScheduledEvent(calendarPanel, 1, 1, 2, "John Doe (Room 1)", "Heart Surgery", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);
                AddScheduledEvent(calendarPanel, 3, 4, 1, "Mary Smith (Room 1)", "Pacemaker", "#E74C3C", timeColumnWidth, columnWidth, rowHeight);

                // Room 2 - Orthopedics
                AddScheduledEvent(calendarPanel, 2, 1, 1, "Sarah Johnson (Room 2)", "Knee Surgery", "#3498DB", timeColumnWidth, columnWidth, rowHeight);
                AddScheduledEvent(calendarPanel, 4, 3, 1, "Mike Thompson (Room 2)", "Hip Surgery", "#3498DB", timeColumnWidth, columnWidth, rowHeight);

                // Room 3 - Neurology
                AddScheduledEvent(calendarPanel, 1, 3, 3, "Robert Brown (Room 3)", "Brain Tumor", "#2ECC71", timeColumnWidth, columnWidth, rowHeight);
                AddScheduledEvent(calendarPanel, 3, 1, 2, "Jennifer Davis (Room 3)", "Spinal Surgery", "#2ECC71", timeColumnWidth, columnWidth, rowHeight);
            }
        }

        private void ClearExistingEvents(Panel calendarPanel)
        {
            // Remove all existing event panels (anything that's not a time panel or cell panel)
            List<Control> controlsToRemove = new List<Control>();
            foreach (Control control in calendarPanel.Controls)
            {
                // Skip time panels and cell panels (which are part of the grid)
                if (control is Panel panel &&
                    panel.BorderStyle != BorderStyle.FixedSingle)
                {
                    controlsToRemove.Add(control);
                }
            }

            foreach (Control control in controlsToRemove)
            {
                calendarPanel.Controls.Remove(control);
                control.Dispose();
            }
        }

        private string GetColorForSpecialization(string specialization)
        {
            // Color mapping for different medical specializations
            switch (specialization)
            {
                case "Cardiology": return "#E74C3C";   // Red
                case "Orthopedics": return "#3498DB";  // Blue
                case "Neurology": return "#2ECC71";    // Green
                case "Pediatrics": return "#9B59B6";   // Purple
                case "Oncology": return "#F39C12";     // Orange
                case "Dermatology": return "#1ABC9C";  // Turquoise
                case "Emergency Medicine": return "#E67E22"; // Dark Orange
                case "Pulmonology": return "#34495E";  // Navy
                case "Gastroenterology": return "#27AE60"; // Dark Green
                case "Endocrinology": return "#8E44AD"; // Purple
                case "Rheumatology": return "#D35400"; // Pumpkin
                case "Ophthalmology": return "#2980B9"; // Blue
                case "Psychiatry": return "#16A085";   // Green
                case "Urology": return "#C0392B";      // Dark Red
                case "Nephrology": return "#7F8C8D";   // Gray
                case "Geriatrics": return "#2C3E50";   // Dark Blue
                case "Infectious Disease": return "#F1C40F"; // Yellow
                case "Hematology": return "#E74C3C";   // Red
                case "OB/GYN": return "#9B59B6";       // Purple
                default: return "#34495E";             // Dark Gray
            }
        }

        private void AddScheduledEvent(Panel parent, int column, int startRow, int rowSpan, string name, string procedure, string color, int timeColumnWidth, int columnWidth, int rowHeight)
        {
            try
            {
                // Ensure we don't exceed the bounds of the calendar
                startRow = Math.Min(startRow, 9);  // Max 10 rows (0-9)
                rowSpan = Math.Min(rowSpan, 10 - startRow);  // Ensure we don't go beyond the calendar bottom

                // Explicitly calculate coordinates to avoid issues
                int x = timeColumnWidth + (column - 1) * columnWidth;
                int y = rowHeight + startRow * rowHeight;
                int width = columnWidth - 4;  // Leave a small gap between events
                int height = rowHeight * rowSpan - 4;

                Panel eventPanel = new Panel
                {
                    Location = new Point(x + 2, y + 2), // Add a small offset
                    Size = new Size(width, height),
                    BackColor = Color.FromArgb(80, ColorTranslator.FromHtml(color)),
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
                    Location = new Point(5, 5),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    MaximumSize = new Size(width - 10, 0)
                };
                eventPanel.Controls.Add(nameLabel);

                // Procedure Label
                Label procedureLabel = new Label
                {
                    Text = procedure,
                    ForeColor = ColorTranslator.FromHtml(color),
                    Font = new Font("Segoe UI", 8),
                    Location = new Point(5, nameLabel.Height + 8),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    MaximumSize = new Size(width - 10, 0)
                };
                eventPanel.Controls.Add(procedureLabel);

                // Make sure it's visible
                eventPanel.Visible = true;

                parent.Controls.Add(eventPanel);
                eventPanel.BringToFront();

                // Make the panel show a tooltip with full details when hovered
                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(eventPanel, $"{name}\n{procedure}\nDuration: {rowSpan} hour(s)");
            }
            catch (Exception ex)
            {
                // Silently handle rendering errors
                System.Diagnostics.Debug.WriteLine($"Error adding event: {ex.Message}");
            }
        }
        private void calendarPanel_Paint(object sender, PaintEventArgs e)
        {
            // This is intentionally left empty
            // It's referenced in the designer code but not needed for functionality
        }
    }
}