using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClassLibrary1;
using DB; // Assuming this contains DataManager and DataSingelton
using Models; // Assuming this contains your Doctor, Patient, Schedule classes

namespace MedScheduler.forms
{
    public partial class DoctorsForm : UserControl
    {
        // Data Access
        private List<Doctor> allDoctors = new List<Doctor>();
        private List<Patient> allPatients = new List<Patient>();
        // Pagination Fields
        private int currentPage = 1;
        private int doctorsPerPage = 10;
        private int totalPages = 1;

        // UI Element Fields
        private Panel tablePanel;      // Panel holding the doctor rows
        private Panel paginationPanel; // Panel holding the pagination buttons
        private Panel searchPanel;     // Panel for search/actions
        private Panel contentPanel;    // Main container panel
        private Panel tableHeaderPanel;// Header for the table
        private Panel doctorDetailPanel; // Panel to show doctor details (initially null)
        private Panel patientDetailPanel;
        // --- Constructor ---
        public DoctorsForm()
        {
            // It's good practice to check if running in Design Mode
            if (DesignMode) return;

            InitializeComponentDoctors(); // Create UI elements using Docking
            this.Size = new Size(1000, 700);
            this.Dock = DockStyle.Fill;
            LoadDoctorsData();
        }

        // --- Data Loading & Refreshing ---

        private void LoadDoctorsData()
        {
            try
            {
                // Fetch all doctors - ensure list is never null
                // Replace DataSingelton.Instance with your actual data source if different
                allDoctors = DataSingelton.Instance?.Doctors ?? new List<Doctor>();
                allPatients = DataSingelton.Instance?.Patients ?? new List<Patient>();
                totalPages = (!allDoctors.Any()) ? 1 : (int)Math.Ceiling((double)allDoctors.Count / doctorsPerPage);
                currentPage = 1;

                DisplayPage(currentPage);
                CenterPaginationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading doctor data: {ex.Message}", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                allDoctors = new List<Doctor>();
                totalPages = 1;
                currentPage = 1;
                if (tablePanel != null && paginationPanel != null)
                {
                    DisplayPage(currentPage);
                    CenterPaginationButtons();
                }
            }
        }


        /// <summary>
        /// Shows only the main list view panels.
        /// </summary>
        private void ShowListView()
        {
            HideDoctorDetailPanel(); // Hide doctor detail if visible
            HidePatientDetailPanel(); // Hide patient detail if visible

            if (searchPanel != null) searchPanel.Visible = true;
            if (tableHeaderPanel != null) tableHeaderPanel.Visible = true;
            if (tablePanel != null) tablePanel.Visible = true;
            if (paginationPanel != null) paginationPanel.Visible = true;

            DisplayPage(currentPage); // Refresh the current page of the list
            CenterPaginationButtons();
        }

        /// <summary>
        /// Hides the main list view and shows the doctor detail panel.
        /// </summary>
        private void ShowDoctorDetailView(int doctorId)
        {
            Doctor selectedDoctor = allDoctors.FirstOrDefault(d => d.Id == doctorId);
            if (selectedDoctor == null) { MessageBox.Show("Could not find doctor details.", "Error"); return; }

            // Hide other views
            HideListViewPanels();
            HidePatientDetailPanel();

            // Create and show panel
            doctorDetailPanel = CreateDoctorDetailPanel(selectedDoctor);
            doctorDetailPanel.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(doctorDetailPanel);
            doctorDetailPanel.BringToFront();
            doctorDetailPanel.Visible = true;
        }

        /// <summary>
        /// Hides the main list view and doctor detail, shows patient detail panel.
        /// </summary>
        private void ShowPatientDetailView(int patientId)
        {
            Patient selectedPatient = allPatients.FirstOrDefault(p => p.Id == patientId);
            if (selectedPatient == null) { MessageBox.Show("Could not find patient details.", "Error"); return; }

            // Hide other views
            HideListViewPanels();
            HideDoctorDetailPanel();

            // Create and show panel
            patientDetailPanel = CreatePatientDetailPanel(selectedPatient);
            patientDetailPanel.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(patientDetailPanel);
            patientDetailPanel.BringToFront();
            patientDetailPanel.Visible = true;
        }

        private void HideListViewPanels()
        {
            if (searchPanel != null) searchPanel.Visible = false;
            if (tableHeaderPanel != null) tableHeaderPanel.Visible = false;
            if (tablePanel != null) tablePanel.Visible = false;
            if (paginationPanel != null) paginationPanel.Visible = false;
        }

        private void HideDoctorDetailPanel()
        {
            if (doctorDetailPanel != null)
            {
                doctorDetailPanel.Visible = false; // Hide instead of removing immediately if needed
                contentPanel.Controls.Remove(doctorDetailPanel);
                doctorDetailPanel.Dispose();
                doctorDetailPanel = null;
            }
        }
        private void HidePatientDetailPanel()
        {
            if (patientDetailPanel != null)
            {
                patientDetailPanel.Visible = false;
                contentPanel.Controls.Remove(patientDetailPanel);
                patientDetailPanel.Dispose();
                patientDetailPanel = null;
            }
        }
        private void RefreshDoctorList()
        {
            AddLogMessage("Refreshing doctor list...");
            LoadDoctorsData();
            AddLogMessage("Doctor list refreshed.");
        }

        // --- Pagination Display Logic ---

        private void DisplayPage(int pageNumber)
        {
            if (tablePanel == null || paginationPanel == null || tableHeaderPanel == null) return; // Ensure panels exist
            if (allDoctors == null) allDoctors = new List<Doctor>(); // Ensure list exists

            currentPage = Math.Max(1, Math.Min(pageNumber, totalPages));

            tablePanel.SuspendLayout(); // Suspend layout for performance

            // Clear existing doctor rows (but not the header panel itself)
            var rowsToRemove = tablePanel.Controls.OfType<Panel>().Where(p => p.Tag?.ToString() == "DoctorRow").ToList();
            foreach (var row in rowsToRemove)
            {
                tablePanel.Controls.Remove(row);
                row.Dispose();
            }

            // Get doctors for the current page
            var doctorsToShow = allDoctors
                .Skip((currentPage - 1) * doctorsPerPage)
                .Take(doctorsPerPage)
                .ToList();

            // Add rows for the current page's doctors
            int yPos = 0; // Start rows at the top of tablePanel
            foreach (var doc in doctorsToShow)
            {
                string name = doc.Name ?? "N/A";
                string specialization = doc.Specialization ?? "N/A";
                // Use actual workload if available, otherwise calculate from schedule? Defaulting to stored value.
                string workload = doc.Workload.ToString();

                // *** Use the renamed method ***
                AddDoctorRow(tablePanel, 0, yPos, name, specialization, workload, doc.Id);

                Panel lastRow = tablePanel.Controls.OfType<Panel>().LastOrDefault(p => p.Tag?.ToString() == "DoctorRow");
                yPos = (lastRow?.Bottom ?? yPos) + 1; // Position next row below the last one with a small gap
            }

            tablePanel.ResumeLayout(true); // Resume layout

            UpdatePaginationButtons();
            tablePanel.ScrollControlIntoView(tablePanel.Controls.OfType<Control>().FirstOrDefault()); // Scroll to top
        }

        // --- Row Creation (Renamed and Modified) ---
        private void AddDoctorRow(Panel parent, int x, int y, string name, string specialty, string workload, int doctorId)
        {
            Panel rowPanel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(parent.ClientSize.Width - (SystemInformation.VerticalScrollBarWidth * (parent.VerticalScroll.Visible ? 1 : 0)), 40),
                BackColor = Color.White,
                Tag = "DoctorRow", // Updated Tag
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };

            rowPanel.Paint += (sender, e) => { /* ... border drawing ... */
                using (Pen grayPen = new Pen(ColorTranslator.FromHtml("#ECF0F1"), 1))
                    e.Graphics.DrawLine(grayPen, 0, rowPanel.Height - 1, rowPanel.Width, rowPanel.Height - 1);
            };

            // Define column widths/positions (match header)
            int col1_Name_Width = 200;
            int col2_Spec_X = col1_Name_Width + 20;
            int col2_Spec_Width = 200;
            int col3_Load_X = col2_Spec_X + col2_Spec_Width + 20;

            int labelY = (rowPanel.Height - 20) / 2;
            int leftPadding = 20;

            // Name Label - *** MADE CLICKABLE ***
            Label nameLabel = new Label
            {
                Text = name,
                ForeColor = ColorTranslator.FromHtml("#3498DB"), // Blue to look like a link
                Font = new Font("Segoe UI", 12, FontStyle.Underline), // Underline
                Location = new Point(leftPadding, labelY),
                AutoSize = true,
                Cursor = Cursors.Hand, // Indicate clickable
                Tag = doctorId // Store doctorId for the click event
            };
            nameLabel.Click += NameLabel_Click; // Attach click event handler
            rowPanel.Controls.Add(nameLabel);

            // Specialty Label
            Label specialtyLabel = new Label { Text = specialty, ForeColor = ColorTranslator.FromHtml("#2C3E50"), Font = new Font("Segoe UI", 12), Location = new Point(col2_Spec_X, labelY), AutoSize = true };
            rowPanel.Controls.Add(specialtyLabel);

            // Workload Label (previously patientCount)
            Label workloadLabel = new Label { Text = workload, ForeColor = ColorTranslator.FromHtml("#2C3E50"), Font = new Font("Segoe UI", 12), Location = new Point(col3_Load_X, labelY), AutoSize = true };
            rowPanel.Controls.Add(workloadLabel);

            // Action Buttons
            int actionButtonY = (rowPanel.Height - 28) / 2;
            int spacing = 5;
            int marginRight = 20;
            Button deleteButton = CreateActionButton("D", "#E74C3C", 0, actionButtonY);
            Button editButton = CreateActionButton("E", "#3498DB", 0, actionButtonY);
            editButton.Tag = doctorId;
            deleteButton.Tag = doctorId;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;
            deleteButton.Location = new Point(rowPanel.ClientSize.Width - deleteButton.Width - marginRight, actionButtonY);
            editButton.Location = new Point(deleteButton.Left - editButton.Width - spacing, actionButtonY);
            deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rowPanel.Controls.Add(editButton);
            rowPanel.Controls.Add(deleteButton);

            parent.Controls.Add(rowPanel);
        }

        // --- Navigation and Detail View ---

        /// <summary>
        /// Handles clicks on the doctor's name label.
        /// </summary>
        private void NameLabel_Click(object sender, EventArgs e)
        {
            if (sender is Label clickedLabel && clickedLabel.Tag is int doctorId)
            {
                ShowDoctorDetailPanel(doctorId);
            }
        }

        /// <summary>
        /// Finds the doctor and displays their details in a new panel.
        /// </summary>
        private void ShowDoctorDetailPanel(int doctorId)
        {
            Doctor selectedDoctor = allDoctors.FirstOrDefault(d => d.Id == doctorId);
            if (selectedDoctor == null)
            {
                MessageBox.Show("Could not find doctor details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hide the main list view panels
            searchPanel.Visible = false;
            tableHeaderPanel.Visible = false;
            tablePanel.Visible = false;
            paginationPanel.Visible = false;

            // Create and show the detail panel
            doctorDetailPanel = CreateDoctorDetailPanel(selectedDoctor);
            doctorDetailPanel.Dock = DockStyle.Fill; // Make it fill the content area
            contentPanel.Controls.Add(doctorDetailPanel);
            doctorDetailPanel.BringToFront(); // Ensure it's visible
        }

        /// <summary>
        /// Hides the detail panel and shows the main list view again.
        /// </summary>


        /// <summary>
        /// Creates the panel to display detailed doctor information.
        /// </summary>
        private Panel CreateDoctorDetailPanel(Doctor doctor)
        {
            Panel detailPanel = new Panel { Name = "DoctorDetailView", BackColor = Color.White, Padding = new Padding(20), AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
            int currentY = 10; int labelX = 20; int spacing = 28;
            Font labelFont = new Font("Segoe UI", 11F); Font valueFont = new Font("Segoe UI", 11F);
            Color valueColor = ColorTranslator.FromHtml("#2C3E50");

            // Back Button
            Button backButton = new Button { Text = "⬅ Back to List", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(labelX, currentY), Size = new Size(120, 30), FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#95A5A6"), ForeColor = Color.White, Cursor = Cursors.Hand };
            backButton.FlatAppearance.BorderSize = 0;
            backButton.Click += (s, e) => ShowListView(); // Go back to list
            detailPanel.Controls.Add(backButton); currentY += 50;

            // Doctor Info
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "ID:", doctor.Id.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Name:", doctor.Name);
            // ... (Add other doctor fields: Specialization, Experience, Workload, Max Workload) ...
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Specialization:", doctor.Specialization);
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Experience:", doctor.ExperienceLevel.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Current Workload:", doctor.Workload.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Max Workload:", doctor.MaxWorkload.ToString());


            // Preferences TextBox
            AddDetailTextBox(detailPanel, ref currentY, spacing, labelX, labelFont, "Preferences:", doctor.Preferences?.Any() == true ? FormatPreferences(doctor.Preferences) : "No preferences defined.");

            // Surgeon Details
            if (doctor is Surgeon surgeon)
            {
                AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Is Surgeon:", "Yes");
                AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Available for Surgery:", surgeon.IsAvailableForSurgery ? "Yes" : "No");
                AddDetailTextBox(detailPanel, ref currentY, spacing, labelX, labelFont, "Availability Slots:", surgeon.Availability?.Any() == true ? FormatAvailability(surgeon.Availability) : "No availability slots defined.", 100); // Larger textbox
            }
            else { AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Is Surgeon:", "No"); }

            // --- Assigned Patients Section ---
            Label assignedLabel = new Label { Text = "Assigned Patients (Current Schedule):", Location = new Point(labelX, currentY), Font = labelFont, AutoSize = true };
            detailPanel.Controls.Add(assignedLabel);
            currentY += spacing;

            // Container for patient labels
            FlowLayoutPanel patientsPanel = new FlowLayoutPanel
            {
                Location = new Point(labelX + 5, currentY),
                Size = new Size(detailPanel.ClientSize.Width - labelX * 2 - 15, 120), // Give it some height
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            detailPanel.Controls.Add(patientsPanel);

            // Get assigned patient IDs from the schedule
            List<int> assignedPatientIds =doctor.patientsIDS;
            if (doctor.patientsIDS!=null)
            {

                foreach (int patientId in assignedPatientIds)
                {
                    Patient assignedPatient = allPatients.FirstOrDefault(p => p.Id == patientId);
                    if (assignedPatient != null)
                    {
                        Label patientLabel = new Label
                        {
                            Text = $"- {assignedPatient.Name} (ID: {assignedPatient.Id})",
                            Font = new Font("Segoe UI", 10F, FontStyle.Underline),
                            ForeColor = ColorTranslator.FromHtml("#3498DB"), // Blue link color
                            Cursor = Cursors.Hand,
                            Tag = patientId, // Store patient ID
                            AutoSize = true, // Let label size itself
                            Margin = new Padding(3) // Add some margin
                        };
                        patientLabel.Click += PatientNameLabel_Click; // Add click handler
                        patientsPanel.Controls.Add(patientLabel);
                    }
                }
                
                
            }
            else { patientsPanel.Controls.Add(new Label { Text = "None assigned in current schedule.", AutoSize = true, Margin = new Padding(3) }); }
            currentY += patientsPanel.Height + 10;


            return detailPanel;
        }

        // Helper to add a Label-Value row to the detail panel
        private void AddDetailRow(Panel panel, ref int y, int spacing, int labelX, int valueX, Font labelFont, Font valueFont, Color valueColor, string labelText, string valueText)
        {
            // Create Description Label
            Label lblDesc = new Label
            {
                Text = labelText,
                Font = labelFont,
                Location = new Point(labelX, y),
                AutoSize = true
            };
            panel.Controls.Add(lblDesc);

            // Calculate position for Value Label based on Description Label
            valueX = lblDesc.Right + 8; // Start value 8px after the description

            // Create Value Label
            Label lblValue = new Label
            {
                Text = valueText ?? "N/A", // Use N/A for null values
                Font = valueFont,
                ForeColor = valueColor,
                Location = new Point(valueX, y), // Position relative to description
                // Disable AutoSize, use Anchoring for width instead
                AutoSize = false,
                Size = new Size(panel.ClientSize.Width - valueX - labelX, 20), // Initial size, adjust height if needed
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, // Anchor left and right
                TextAlign = ContentAlignment.MiddleLeft
            };
            // Adjust height based on content if needed (e.g., for potential wrapping, though less likely now)
            // lblValue.Height = TextRenderer.MeasureText(lblValue.Text, lblValue.Font, lblValue.ClientSize.Width, TextFormatFlags.WordBreak).Height;

            panel.Controls.Add(lblValue);

            // Increment Y position based on the taller of the two labels for this row
            y += Math.Max(lblDesc.Height, lblValue.Height) + (spacing / 2);
        }

        // Creates the panel to display detailed patient information
        private Panel CreatePatientDetailPanel(Patient patient)
        {
            Panel detailPanel = new Panel { Name = "PatientDetailView", BackColor = Color.White, Padding = new Padding(20), AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
            int currentY = 10; int labelX = 20; int spacing = 28;
            Font labelFont = new Font("Segoe UI", 11F); Font valueFont = new Font("Segoe UI", 11F);
            Color valueColor = ColorTranslator.FromHtml("#2C3E50");

            // Back Button (Goes back to Doctor Detail)
            Button backButton = new Button { Text = "⬅ Back to Doctor", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(labelX, currentY), Size = new Size(130, 30), FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#7F8C8D"), ForeColor = Color.White, Cursor = Cursors.Hand };
            backButton.FlatAppearance.BorderSize = 0;
            // We need the ID of the doctor whose detail panel we came from to go back correctly.
            // For now, just hide this panel and assume the doctor panel is still loaded but hidden.
            // A better approach might involve passing the doctor ID or using a navigation stack.
            backButton.Click += (s, e) => {
                HidePatientDetailPanel();
                // Re-show doctor panel - requires knowing which doctor was viewed
                // This simple version just hides patient, assumes doctor panel exists underneath
                if (doctorDetailPanel != null) doctorDetailPanel.Visible = true; else ShowListView(); // Fallback to list if doctor panel gone
            };
            detailPanel.Controls.Add(backButton); currentY += 50;

            // Patient Info
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "ID:", patient.Id.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Name:", patient.Name);
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Condition:", patient.Condition);
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Urgency:", patient.Urgency.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Complexity:", patient.ComplexityLevel.ToString());
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Required Spec:", patient.RequiredSpecialization);
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Needs Surgery:", patient.NeedsSurgery ? "Yes" : "No");
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Admission Date:", patient.AdmissionDate.ToString("yyyy-MM-dd HH:mm"));

            // Assignment Info
            string assignedDocName = patient.AssignedDoctorId.HasValue ? (allDoctors.FirstOrDefault(d => d.Id == patient.AssignedDoctorId.Value)?.Name ?? "Unknown") : "None";
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Assigned Doctor:", $"{assignedDocName} (ID: {patient.AssignedDoctorId?.ToString() ?? "N/A"})");

            string assignedSurgeonName = patient.AssignedSurgeonId.HasValue ? (allDoctors.FirstOrDefault(d => d.Id == patient.AssignedSurgeonId.Value)?.Name ?? "Unknown") : "None";
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Assigned Surgeon:", $"{assignedSurgeonName} (ID: {patient.AssignedSurgeonId?.ToString() ?? "N/A"})");

            string orName = patient.AssignedOperatingRoomId.HasValue ? $"OR {patient.AssignedOperatingRoomId.Value}" : "None"; // Assuming OR name isn't easily accessible here
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Assigned OR:", orName);
            AddDetailRow(detailPanel, ref currentY, spacing, labelX, labelFont, valueFont, valueColor, "Scheduled Surgery:", patient.ScheduledSurgeryDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A");


            return detailPanel;
        }
        // --- Event Handlers ---
        private void PageButton_Click(object sender, EventArgs e)
        { /* ... Implementation from previous response ... */
            Button clickedButton = sender as Button; if (clickedButton == null || !clickedButton.Enabled) return;
            int newPage = currentPage; string tag = clickedButton.Tag?.ToString();
            if (tag == "Prev") { if (currentPage > 1) newPage--; }
            else if (tag == "Next") { if (currentPage < totalPages) newPage++; }
            else if (int.TryParse(tag, out int pageNum)) { if (pageNum >= 1 && pageNum <= totalPages) newPage = pageNum; }
            if (newPage != currentPage) DisplayPage(newPage);
        }
        private void RefreshButton_Click(object sender, EventArgs e) { RefreshDoctorList(); }
        private void EditButton_Click(object sender, EventArgs e)
        { /* ... Implementation ... */
            Button clickedButton = sender as Button; if (clickedButton?.Tag is int doctorId) MessageBox.Show($"Edit action for Doctor ID: {doctorId}", "Edit Doctor");
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        { /* ... Implementation ... */
            Button clickedButton = sender as Button; if (clickedButton?.Tag is int doctorId) { var confirmResult = MessageBox.Show($"Delete Doctor ID: {doctorId}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning); if (confirmResult == DialogResult.Yes) MessageBox.Show($"Delete confirmed for Doctor ID: {doctorId}", "Delete Doctor"); }
        }
        private void AddButton_Click(object sender, EventArgs e)
        { /* ... Implementation ... */
            MessageBox.Show($"Add New Doctor action.", "Add Doctor");
        }

        private void PatientNameLabel_Click(object sender, EventArgs e) { if (sender is Label clickedLabel && clickedLabel.Tag is int patientId) { ShowPatientDetailView(patientId); } }
        // --- UI Initialization using Docking (Simplified) ---
        private void InitializeComponentDoctors()
        {
            this.SuspendLayout();
            this.BackColor = ColorTranslator.FromHtml("#ECF0F1");

            contentPanel = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(contentPanel);

            searchPanel = new Panel { Height = 50, BackColor = Color.White, Dock = DockStyle.Top, Padding = new Padding(10) };
            InitializeSearchPanelControls(searchPanel);
            contentPanel.Controls.Add(searchPanel);

            tableHeaderPanel = CreateTableHeaderPanel(); // Use helper to create
            tableHeaderPanel.Dock = DockStyle.Top;
            contentPanel.Controls.Add(tableHeaderPanel); // Add header AFTER search

            paginationPanel = new Panel { Height = 40, BackColor = Color.Transparent, Dock = DockStyle.Bottom, Padding = new Padding(0, 5, 0, 5) };
            InitializePaginationControls(paginationPanel);
            paginationPanel.Resize += PaginationPanel_Resize;
            contentPanel.Controls.Add(paginationPanel); // Add pagination

            tablePanel = new Panel { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, AutoScroll = true };
            contentPanel.Controls.Add(tablePanel); // Add table panel LAST to fill remaining space

            // Ensure correct Z-order for docked controls
            tablePanel.BringToFront();
            paginationPanel.BringToFront();
            tableHeaderPanel.BringToFront();
            searchPanel.BringToFront();

            this.ResumeLayout(true);
        }

        // Creates the header panel (adjust columns as needed)
        private Panel CreateTableHeaderPanel()
        {
            Panel header = new Panel { Height = 40, BackColor = ColorTranslator.FromHtml("#F8F9FA"), Dock = DockStyle.Top, Padding = new Padding(20, 0, 20, 0), Tag = "TableHeader" };
            string[] columnHeaders = { "Name", "Specialization", "Workload", "Actions" };
            int[] columnStartX = { header.Padding.Left, header.Padding.Left + 220, header.Padding.Left + 440, header.Width - header.Padding.Right - 100 }; // Example positions

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                Label lbl = new Label
                {
                    Text = columnHeaders[i],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(columnStartX[i], (header.ClientSize.Height - 18) / 2),
                    AutoSize = true
                };
                if (columnHeaders[i] == "Actions") lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right; else lbl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                // Adjust X for Actions label based on actual panel width (might need resize event)
                if (columnHeaders[i] == "Actions") lbl.Left = header.ClientSize.Width - header.Padding.Right - lbl.Width - 60; // Approx position actions buttons

                header.Controls.Add(lbl);
            }
            return header;
        }

        // --- (Keep other helper methods: InitializeSearchPanelControls, InitializePaginationControls, UpdatePaginationButtons, CenterPaginationButtons, PaginationPanel_Resize, CreateActionButton, AddLogMessage) ---
        private void InitializeSearchPanelControls(Panel parent)
        { /* ... Implementation ... */
            TextBox searchBox = new TextBox { Location = new Point(parent.Padding.Left, (parent.ClientSize.Height - 25) / 2), Size = new Size(250, 25), Font = new Font("Segoe UI", 11), Text = "Search doctors...", ForeColor = Color.Gray, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            searchBox.Enter += (s, e) => { if (searchBox.Text == "Search doctors...") { searchBox.Text = ""; searchBox.ForeColor = Color.Black; } }; searchBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(searchBox.Text)) { searchBox.Text = "Search doctors..."; searchBox.ForeColor = Color.Gray; } };
            Button refreshButton = new Button { Size = new Size(90, 28), Font = new Font("Segoe UI", 10, FontStyle.Bold), Text = "Refresh", FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#2ECC71"), ForeColor = Color.White, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right }; refreshButton.FlatAppearance.BorderSize = 0; refreshButton.Click += RefreshButton_Click; refreshButton.Location = new Point(parent.ClientSize.Width - parent.Padding.Right - refreshButton.Width, (parent.ClientSize.Height - refreshButton.Height) / 2);
            Button addButton = new Button { Size = new Size(140, 28), Font = new Font("Segoe UI", 10, FontStyle.Bold), Text = "Add New Doctor", FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#3498DB"), ForeColor = Color.White, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right }; addButton.FlatAppearance.BorderSize = 0; addButton.Click += AddButton_Click; addButton.Location = new Point(refreshButton.Left - 10 - addButton.Width, refreshButton.Top);
            parent.Controls.Add(searchBox); parent.Controls.Add(addButton); parent.Controls.Add(refreshButton);
        }
        private void InitializePaginationControls(Panel parent)
        { /* ... Implementation ... */
            int buttonWidth = 30; int buttonHeight = 30; int maxNumericButtons = 5;
            Button prevButton = new Button { Text = "◀", Tag = "Prev", Size = new Size(buttonWidth, buttonHeight), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), BackColor = Color.White, ForeColor = ColorTranslator.FromHtml("#7F8C8D"), Cursor = Cursors.Hand }; prevButton.FlatAppearance.BorderSize = 0; prevButton.Click += PageButton_Click; parent.Controls.Add(prevButton);
            for (int i = 1; i <= maxNumericButtons; i++) { Button pageButton = new Button { Text = i.ToString(), Tag = i, Size = new Size(buttonWidth, buttonHeight), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), BackColor = Color.White, ForeColor = ColorTranslator.FromHtml("#7F8C8D"), Cursor = Cursors.Hand, Visible = false }; pageButton.FlatAppearance.BorderSize = 0; pageButton.Click += PageButton_Click; parent.Controls.Add(pageButton); }
            Button nextButton = new Button { Text = "▶", Tag = "Next", Size = new Size(buttonWidth, buttonHeight), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11), BackColor = Color.White, ForeColor = ColorTranslator.FromHtml("#7F8C8D"), Cursor = Cursors.Hand }; nextButton.FlatAppearance.BorderSize = 0; nextButton.Click += PageButton_Click; parent.Controls.Add(nextButton);
            CenterPaginationButtons(); // Initial attempt
        }
        private void UpdatePaginationButtons()
        { /* ... Implementation from previous response ... */
            if (paginationPanel == null) return;
            int maxNumericButtons = 5; int startPage = Math.Max(1, currentPage - (maxNumericButtons / 2)); int endPage = Math.Min(totalPages, startPage + maxNumericButtons - 1); if (endPage - startPage + 1 < maxNumericButtons) { startPage = Math.Max(1, endPage - maxNumericButtons + 1); }
            int currentNumericButtonIndex = 0;
            foreach (Control ctrl in paginationPanel.Controls)
            {
                if (ctrl is Button pageButton)
                {
                    pageButton.Enabled = true; pageButton.Visible = true; pageButton.ForeColor = ColorTranslator.FromHtml("#7F8C8D"); pageButton.BackColor = Color.White; pageButton.Font = new Font("Segoe UI", 11);
                    bool isPrevNext = pageButton.Tag?.ToString() == "Prev" || pageButton.Tag?.ToString() == "Next";
                    bool isNumeric = int.TryParse(pageButton.Tag?.ToString(), out int pageNumTag);
                    if (isPrevNext)
                    {
                        if (pageButton.Tag.ToString() == "Prev") { pageButton.Enabled = currentPage > 1; pageButton.ForeColor = pageButton.Enabled ? ColorTranslator.FromHtml("#3498DB") : Color.LightGray; }
                        else { pageButton.Enabled = currentPage < totalPages; pageButton.ForeColor = pageButton.Enabled ? ColorTranslator.FromHtml("#3498DB") : Color.LightGray; }
                        if (totalPages <= 1) { pageButton.Enabled = false; pageButton.ForeColor = Color.LightGray; }
                    }
                    else if (isNumeric)
                    {
                        int actualPageNum = startPage + currentNumericButtonIndex;
                        if (actualPageNum <= endPage && actualPageNum <= totalPages)
                        {
                            pageButton.Text = actualPageNum.ToString(); pageButton.Tag = actualPageNum; // Update tag too
                            pageButton.Visible = true; pageButton.Enabled = true;
                            if (actualPageNum == currentPage) { pageButton.ForeColor = Color.White; pageButton.BackColor = ColorTranslator.FromHtml("#3498DB"); pageButton.Font = new Font("Segoe UI", 11, FontStyle.Bold); }
                            currentNumericButtonIndex++;
                        }
                        else { pageButton.Visible = false; pageButton.Enabled = false; }
                    }
                }
            }
            CenterPaginationButtons();
        }
        private void CenterPaginationButtons()
        { /* ... Implementation from previous response ... */
            if (paginationPanel == null || !paginationPanel.IsHandleCreated || paginationPanel.ClientSize.Width <= 0) return;
            var visibleButtons = paginationPanel.Controls.OfType<Button>().Where(b => b.Visible).OrderBy(b => b.Left).ToList(); // Order by current position to maintain relative order
            if (!visibleButtons.Any()) return;
            int totalButtonWidth = visibleButtons.Sum(b => b.Width) + Math.Max(0, visibleButtons.Count - 1) * 5; // Use actual widths + 5px spacing
            int startX = Math.Max(0, (paginationPanel.ClientSize.Width - totalButtonWidth) / 2);
            int currentX = startX; int buttonY = (paginationPanel.ClientSize.Height - visibleButtons.First().Height) / 2;
            foreach (var button in visibleButtons) { button.Location = new Point(currentX, buttonY); currentX += button.Width + 5; }
        }
        private void PaginationPanel_Resize(object sender, EventArgs e) { CenterPaginationButtons(); }

        private void AddDetailTextBox(Panel panel, ref int y, int spacing, int labelX, Font labelFont, string labelText, string valueText, int height = 80) { /* ... Implementation ... */ Label lblDesc = new Label { Text = labelText, Font = labelFont, Location = new Point(labelX, y), AutoSize = true }; panel.Controls.Add(lblDesc); y += lblDesc.Height + 2; TextBox textBox = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Location = new Point(labelX + 5, y), Size = new Size(panel.ClientSize.Width - labelX * 2 - 10, height), Font = new Font("Segoe UI", 9F), BackColor = ColorTranslator.FromHtml("#F8F9FA"), BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Text = valueText ?? "" }; panel.Controls.Add(textBox); y += textBox.Height + 10; }
        private Button CreateActionButton(string text, string htmlColor, int x, int y)
        { /* ... Implementation ... */
            Button button = new Button { Text = text, Location = new Point(x, y), Size = new Size(28, 28), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml(htmlColor), BackColor = ControlPaint.LightLight(ColorTranslator.FromHtml(htmlColor)), Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(0) }; button.FlatAppearance.BorderSize = 0; return button;
        }
        private void AddLogMessage(string message) { Console.WriteLine($"[DoctorsForm] {DateTime.Now:HH:mm:ss}: {message}"); }

        private void AddDetailRow(Panel panel, ref int y, int spacing, int labelX, Font labelFont, Font valueFont, Color valueColor, string labelText, string valueText)
        { /* ... Implementation from previous response ... */
            Label lblDesc = new Label { Text = labelText, Font = labelFont, Location = new Point(labelX, y), AutoSize = true }; panel.Controls.Add(lblDesc);
            int valueX = lblDesc.Right + 8;
            Label lblValue = new Label { Text = valueText ?? "N/A", Font = valueFont, ForeColor = valueColor, Location = new Point(valueX, y), AutoSize = false, Size = new Size(panel.ClientSize.Width - valueX - labelX, 20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleLeft };
            // Optional: Adjust height for wrapping if needed
            // int preferredHeight = TextRenderer.MeasureText(lblValue.Text, lblValue.Font, lblValue.Width, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            // lblValue.Height = Math.Max(20, preferredHeight);
            panel.Controls.Add(lblValue);
            y += Math.Max(lblDesc.Height, lblValue.Height) + (spacing / 2);
        }

        private string FormatPreferences(List<DoctorPreference> preferences)
        { /* Helper to format preferences nicely */
            if (preferences == null || !preferences.Any()) return "None";
            StringBuilder sb = new StringBuilder();
            foreach (var p in preferences) sb.AppendLine($"- {p.Direction} {p.Type}" + (p.LevelValue.HasValue ? $" (Level: {(int)p.LevelValue})" : "") + (string.IsNullOrEmpty(p.ConditionValue) ? "" : $" (Condition: {p.ConditionValue})"));
            return sb.ToString();
        }
        private string FormatAvailability(List<AvailabilitySlot> slots)
        { /* Helper to format availability nicely */
            if (slots == null || !slots.Any()) return "None defined";
            StringBuilder sb = new StringBuilder();
            foreach (var s in slots.OrderBy(slot => slot.DayOfWeek).ThenBy(slot => slot.StartTime)) sb.AppendLine($"- {s.DayOfWeek}: {s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}");
            return sb.ToString();
        }
    } // End of DoctorsForm class

} // End of namespace
