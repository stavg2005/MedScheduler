using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DB;
using Models;
using static System.Windows.Forms.LinkLabel;

namespace MedScheduler.forms
{
    public partial class PatientForm: UserControl
    {
       
             // Data Access
             // Assuming DataManager is in DB namespace

            // Related Forms/Data (Assumed from original)
        private SchedulesForm schedulesForm;
        public static SchedulerOrchestrator s;
        public static Schedule main = new Schedule();

        // --- Pagination Fields ---
        private List<Patient> allPatients = new List<Patient>(); // Store all doctors here
        private int currentPage = 1;
        private int doctorsPerPage = 10; // How many doctors to show per page
        private int totalPages = 1;

        // --- UI Element Fields (Class Level) ---
        private Panel tablePanel;      // Panel holding the doctor rows (will be Dock.Fill)
        private Panel paginationPanel; // Panel holding the pagination buttons (will be Dock.Bottom)
        private Panel searchPanel;     // Panel for search/actions (will be Dock.Top)
        private Panel contentPanel;    // Main container panel (Dock.Fill within UserControl)

        // --- Constructor ---
        public PatientForm()
        {
            InitializeComponentDoctors(); // Create UI elements using Docking
            this.Size = new Size(1000, 700); // Set desired initial size
            this.Dock = DockStyle.Fill;      // Make UserControl fill its container
            LoadDoctorsData();              // Load initial data AFTER UI is built
        }

        // --- Data Loading & Refreshing ---

        private void LoadDoctorsData()
        {
            try
            {
                // Fetch all doctors once - ensure list is never null
                allPatients = DataSingelton.Instance.Patients ?? new List<Patient>();

                // Calculate total pages based on the loaded data
                totalPages = (!allPatients.Any()) ? 1 : (int)Math.Ceiling((double)allPatients.Count / doctorsPerPage);

                // Ensure current page is valid after load/refresh
                currentPage = 1; // Always reset to page 1 after full load/refresh

                // Display the first page
                DisplayPage(currentPage);

                // Update pagination button layout after data load might change totalPages
                CenterPaginationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading doctor data: {ex.Message}", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Ensure a safe state if loading fails
                allPatients = new List<Patient>();
                totalPages = 1;
                currentPage = 1;
                // Display an empty page if possible
                if (tablePanel != null && paginationPanel != null)
                {
                    DisplayPage(currentPage);
                    CenterPaginationButtons();
                }
            }
        }

        // Called by the Refresh button
        private void RefreshDoctorList()
        {
            AddLogMessage("Refreshing doctor list..."); // Optional logging
            LoadDoctorsData(); // Reloading data handles refresh and UI updates
            AddLogMessage("Doctor list refreshed.");
        }

        // --- Pagination Display Logic ---

        private void DisplayPage(int pageNumber)
        {
            // Safety checks
            if (tablePanel == null || paginationPanel == null)
            {
                AddLogMessage("Error: UI Panels not initialized in DisplayPage.");
                return;
            }
            if (allPatients == null)
            {
                AddLogMessage("Error: allDoctors list is null in DisplayPage. Recovering.");
                allPatients = new List<Patient>();
                totalPages = 1;
                pageNumber = 1;
            }

            // Ensure page number is valid
            currentPage = Math.Max(1, Math.Min(pageNumber, totalPages));

            // Suspend layout for bulk changes
            tablePanel.SuspendLayout();

            // --- Clear existing doctor rows (but not the header) ---
            var rowsToRemove = tablePanel.Controls.OfType<Panel>()
                                       .Where(p => p.Tag?.ToString() == "PatientRow")
                                       .ToList();
            foreach (var row in rowsToRemove)
            {
                tablePanel.Controls.Remove(row);
                row.Dispose();
            }

            // --- Get doctors for the current page using LINQ ---
            var PatientToShow = allPatients
                                .Skip((currentPage - 1) * doctorsPerPage)
                                .Take(doctorsPerPage)
                                .ToList();

            // --- Add rows for the current page's doctors ---
            int yPos = 0; // Start below header
            foreach (var pt in PatientToShow)
            {
                string name = pt.Name ?? "N/A";
                string condition = pt.Condition ?? "N/A";
                string requieres_surgery = pt.NeedsSurgery.ToString() ?? "0";

                AddPatientRow(tablePanel, 0, yPos, name, condition, requieres_surgery, pt.Id);
                // yPos will be managed by the docking/layout of rows if we switch rows to use Dock.Top
                // For now, we keep manual Y, but relative to header bottom
                // We find the last added row to calculate next Y
                Panel lastRow = tablePanel.Controls.OfType<Panel>().LastOrDefault(p => p.Tag?.ToString() == "PatientRow");
                yPos = (lastRow?.Bottom ?? yPos); // Start next row below the last one
            }

            // Resume layout after adding rows
            tablePanel.ResumeLayout(true);


            // --- Update the appearance of pagination buttons ---
            UpdatePaginationButtons();
        }

        // Updates the visual state (enabled/disabled, highlight) of pagination buttons
        private void UpdatePaginationButtons()
        {
            if (paginationPanel == null)
            {
                AddLogMessage("Error: paginationPanel is null in UpdatePaginationButtons.");
                return;
            }

            // Determine which page numbers to show (e.g., max 5 buttons: Prev, 1, 2, 3, Next)
            // This logic might need adjustment based on desired look & feel for many pages
            int maxNumericButtons = 5; // Example: Show up to 5 numeric buttons
            int startPage = Math.Max(1, currentPage - (maxNumericButtons / 2));
            int endPage = Math.Min(totalPages, startPage + maxNumericButtons - 1);
            // Adjust startPage if endPage was capped
            if (endPage - startPage + 1 < maxNumericButtons)
            {
                startPage = Math.Max(1, endPage - maxNumericButtons + 1);
            }


            foreach (Control ctrl in paginationPanel.Controls)
            {
                if (ctrl is Button pageButton)
                {
                    // Reset defaults
                    pageButton.Enabled = true;
                    pageButton.Visible = true; // Assume visible unless hidden below
                    pageButton.ForeColor = ColorTranslator.FromHtml("#7F8C8D");
                    pageButton.BackColor = Color.White;
                    pageButton.Font = new Font("Segoe UI", 11); // Reset font

                    bool isNumericButton = int.TryParse(pageButton.Tag?.ToString(), out int pageNumTag); // Use Tag for page number
                    bool isPrevNextButton = (pageButton.Tag?.ToString() == "Prev" || pageButton.Tag?.ToString() == "Next");

                    if (isPrevNextButton)
                    {
                        if (pageButton.Tag.ToString() == "Prev")
                        {
                            pageButton.Enabled = currentPage > 1;
                            pageButton.ForeColor = pageButton.Enabled ? ColorTranslator.FromHtml("#3498DB") : Color.LightGray;
                        }
                        else // Next
                        {
                            pageButton.Enabled = currentPage < totalPages;
                            pageButton.ForeColor = pageButton.Enabled ? ColorTranslator.FromHtml("#3498DB") : Color.LightGray;
                        }
                    }
                    else if (isNumericButton)
                    {
                        // Logic to show only relevant page numbers
                        if (pageNumTag >= startPage && pageNumTag <= endPage)
                        {
                            pageButton.Text = pageNumTag.ToString(); // Set text correctly
                            pageButton.Visible = true;

                            // Style the currently selected page number
                            if (pageNumTag == currentPage)
                            {
                                pageButton.ForeColor = Color.White;
                                pageButton.BackColor = ColorTranslator.FromHtml("#3498DB"); // Active blue
                                pageButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                            }
                        }
                        else
                        {
                            // This numeric button is outside the range to display
                            pageButton.Visible = false;
                            pageButton.Enabled = false;
                        }
                    }
                    // Handle cases where totalPages is very small
                    if (totalPages <= 1 && isPrevNextButton)
                    {
                        pageButton.Enabled = false;
                        pageButton.ForeColor = Color.LightGray;
                    }
                    if (totalPages < pageNumTag && isNumericButton)
                    {
                        pageButton.Visible = false;
                        pageButton.Enabled = false;
                    }


                }
            }
            // Recenter buttons after visibility changes might alter total width
            CenterPaginationButtons();
        }

        // Centers the visible pagination buttons within the paginationPanel
        private void CenterPaginationButtons()
        {
            if (paginationPanel == null || !paginationPanel.IsHandleCreated || paginationPanel.ClientSize.Width <= 0)
            {
                //AddLogMessage("Warning: Cannot center pagination buttons - panel not ready.");
                return; // Avoid calculation if panel isn't ready or has no width
            }

            var visibleButtons = paginationPanel.Controls.OfType<Button>().Where(b => b.Visible).ToList();
            if (!visibleButtons.Any()) return;

            int buttonWidth = 30; // Assuming fixed width
            int buttonSpacing = 5;
            int totalButtonWidth = visibleButtons.Count * buttonWidth + (visibleButtons.Count - 1) * buttonSpacing;
            int startX = Math.Max(0, (paginationPanel.ClientSize.Width - totalButtonWidth) / 2); // Center horizontally
            int currentX = startX;
            int buttonY = (paginationPanel.ClientSize.Height - visibleButtons.First().Height) / 2; // Center vertically

            foreach (var button in visibleButtons)
            {
                button.Location = new Point(currentX, buttonY);
                currentX += button.Width + buttonSpacing;
            }
        }

        // Event handler for resizing the pagination panel to recenter buttons
        private void PaginationPanel_Resize(object sender, EventArgs e)
        {
            CenterPaginationButtons();
        }


        // --- Row Creation ---

        // Adds a single doctor row panel to the specified parent panel
        private void AddPatientRow(Panel parent, int x, int y, string name, string specialty, string patientCount, int doctorId)
        {
            Panel rowPanel = new Panel
            {
                // Docking rows might be better, but for now, keep manual Y positioning
                Location = new Point(x, y),
                // Use parent's ClientSize for width, considering scrollbars if any
                Size = new Size(parent.ClientSize.Width - (System.Windows.Forms.SystemInformation.VerticalScrollBarWidth * (parent.VerticalScroll.Visible ? 1 : 0)), 40),
                BackColor = Color.White,
                Tag = "PatientRow", // Tag to identify for removal
                // Anchor Left/Right so it resizes horizontally with parent
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Visible = true
            };

            // Draw bottom border line for the row
            rowPanel.Paint += (sender, e) => {
                using (Pen grayPen = new Pen(ColorTranslator.FromHtml("#ECF0F1"), 1)) // Light gray border
                {
                    e.Graphics.DrawLine(grayPen, 0, rowPanel.Height - 1, rowPanel.Width, rowPanel.Height - 1);
                }
            };

            // Define column widths/positions (adjust as needed)
            int col1_Name_Width = 200;
            int col2_Spec_X = col1_Name_Width + 20; // Start Specialty label here
            int col2_Spec_Width = 200;
            int col3_Pat_X = col2_Spec_X + col2_Spec_Width + 20; // Start Patient label here
            // Actions buttons positioned from the right

            int labelY = (rowPanel.Height - 20) / 2; // Approx vertical center
            int leftPadding = 20;

            // Name Label
            Label nameLabel = new Label { Text = name, ForeColor = ColorTranslator.FromHtml("#2C3E50"), Font = new Font("Segoe UI", 12), Location = new Point(leftPadding, labelY), AutoSize = true };
            rowPanel.Controls.Add(nameLabel);

            // Specialty Label
            Label specialtyLabel = new Label { Text = specialty, ForeColor = ColorTranslator.FromHtml("#2C3E50"), Font = new Font("Segoe UI", 12), Location = new Point(col2_Spec_X, labelY), AutoSize = true };
            rowPanel.Controls.Add(specialtyLabel);

            // Patient Count Label
            Label patientLabel = new Label { Text = patientCount, ForeColor = ColorTranslator.FromHtml("#2C3E50"), Font = new Font("Segoe UI", 12), Location = new Point(col3_Pat_X, labelY), AutoSize = true };
            rowPanel.Controls.Add(patientLabel);

            // --- Action Buttons (Edit & Delete) ---
            int actionButtonY = (rowPanel.Height - 28) / 2; // Center button vertically
            int spacing = 5;
            int marginRight = 20; // Margin from the right edge

            Button deleteButton = CreateActionButton("D", "#E74C3C", 0, actionButtonY); // Red
            Button editButton = CreateActionButton("E", "#3498DB", 0, actionButtonY);   // Blue

            // Assign Doctor ID to buttons' Tag
            editButton.Tag = doctorId;
            deleteButton.Tag = doctorId;

            // Add click handlers
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;

            // Position from right edge - Anchor them right
            deleteButton.Location = new Point(rowPanel.ClientSize.Width - deleteButton.Width - marginRight, actionButtonY);
            editButton.Location = new Point(deleteButton.Left - editButton.Width - spacing, actionButtonY);
            deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;


            rowPanel.Controls.Add(editButton);
            rowPanel.Controls.Add(deleteButton);

            // Add the completed row to the table panel
            parent.Controls.Add(rowPanel);


        }


        // Helper method to create styled action buttons
        private Button CreateActionButton(string text, string htmlColor, int x, int y)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(28, 28), // Small square button
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml(htmlColor),
                BackColor = ControlPaint.LightLight(ColorTranslator.FromHtml(htmlColor)), // Use a very light version for background
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0) // Remove default padding
            };
            button.FlatAppearance.BorderSize = 0; // No border outline
            return button;
        }

        // --- Event Handlers ---

        // Handles clicks on ANY pagination button ("<", "1", "2", ">", etc.)
        private void PageButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null || !clickedButton.Enabled) return; // Ignore disabled buttons

            int newPage = currentPage;
            string tag = clickedButton.Tag?.ToString();

            if (tag == "Prev")
            {
                if (currentPage > 1) newPage--;
            }
            else if (tag == "Next")
            {
                if (currentPage < totalPages) newPage++;
            }
            else if (int.TryParse(tag, out int pageNum))
            {
                if (pageNum >= 1 && pageNum <= totalPages)
                {
                    newPage = pageNum;
                }
            }

            if (newPage != currentPage)
            {
                DisplayPage(newPage);
            }
        }

        // Handles click on the Refresh button
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshDoctorList();
        }

        // Placeholder for Edit button click handler
        private void EditButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton?.Tag is int doctorId)
            {
                MessageBox.Show($"Edit action triggered for Doctor ID: {doctorId}", "Edit Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // TODO: Implement actual edit logic
                // Example:
                // using (var editForm = new EditDoctorForm(doctorId)) {
                //     if (editForm.ShowDialog() == DialogResult.OK) {
                //         RefreshDoctorList();
                //     }
                // }
            }
        }

        // Placeholder for Delete button click handler
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton?.Tag is int doctorId)
            {
                var confirmResult = MessageBox.Show($"Are you sure you want to delete Doctor ID: {doctorId}?",
                                                      "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmResult == DialogResult.Yes)
                {
                    MessageBox.Show($"Delete action confirmed for Doctor ID: {doctorId}", "Delete Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // TODO: Implement actual delete logic
                    // Example:
                    // bool deleted = db.DeleteDoctor(doctorId); // Assuming method exists
                    // if (deleted) {
                    //      DataSingelton.Instance.RemoveDoctor(doctorId); // Update cache if needed
                    //      RefreshDoctorList(); // Refresh view
                    // } else {
                    //      MessageBox.Show("Failed to delete doctor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // }
                }
            }
        }

        // Placeholder for Add button click handler
        private void AddButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Add New Doctor action triggered.", "Add Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Implement actual Add logic
            // Example:
            // using (var addForm = new AddDoctorForm()) {
            //     if (addForm.ShowDialog() == DialogResult.OK) {
            //          // Maybe addForm returns the new doctor, update cache?
            //          RefreshDoctorList(); // Refresh view
            //     }
            // }
        }


        // --- UI Initialization using Docking ---

        private void InitializeComponentDoctors()
        {
            this.SuspendLayout(); // Suspend layout

            this.BackColor = ColorTranslator.FromHtml("#ECF0F1"); // Light gray background

            // --- Main Content Panel (fills the UserControl) ---
            contentPanel = new Panel
            {
                BackColor = Color.Transparent, // Inherit background or set explicitly
                Dock = DockStyle.Fill,         // Fill the UserControl
                Padding = new Padding(20)      // Add padding around the content
            };
            this.Controls.Add(contentPanel);



            // --- Search and Action Panel (Dock Top, below title) ---
            searchPanel = new Panel
            {
                Height = 50, // Fixed height
                BackColor = Color.White,
                // BorderStyle = BorderStyle.FixedSingle, // Optional: Add border
                Dock = DockStyle.Top, // DOCK TO TOP (below anything already there)
                Padding = new Padding(10) // Padding inside the search panel
            };
            // Add controls to searchPanel BEFORE adding searchPanel to contentPanel
            InitializeSearchPanelControls(searchPanel);
            contentPanel.Controls.Add(searchPanel);

            Panel tableHeaderPanel = CreateTableHeaderPanel(); // Create the header
            tableHeaderPanel.Dock = DockStyle.Top;             // Dock it Top
            contentPanel.Controls.Add(tableHeaderPanel);

            // --- Pagination Panel (Dock Bottom) ---
            paginationPanel = new Panel
            {
                Height = 40, // Fixed height
                BackColor = Color.Transparent, // Transparent background
                Dock = DockStyle.Bottom, // DOCK TO BOTTOM
                Padding = new Padding(0, 5, 0, 5) // Vertical padding
            };
            // Add controls to paginationPanel BEFORE adding paginationPanel to contentPanel
            InitializePaginationControls(paginationPanel);
            paginationPanel.Resize += PaginationPanel_Resize; // Add resize handler for centering
            contentPanel.Controls.Add(paginationPanel); // Add to contentPanel


            // --- Table Panel (Dock Fill - takes remaining space) ---
            tablePanel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                AutoScroll =true
            };


            // ---Adjust Z - Order(Important!)-- -
            // Controls added later are generally drawn on top.
            // Ensure the order of addition reflects the desired layering for docked controls.
            // Adding searchPanel (Top), then tableHeaderPanel (Top) makes the header appear below search.
            // Adding paginationPanel (Bottom) reserves space at the bottom.
            // Adding tablePanel (Fill) takes the space between tableHeaderPanel and paginationPanel.
            // REMOVED/UNNECESSARY NOW: searchPanel.BringToFront();
            contentPanel.Controls.Add(tablePanel); // Add to contentPanel






            this.ResumeLayout(true); // Resume layout and perform layout actions
            // DIAGNOSTICS: Add these lines
    AddLogMessage($"--- Layout Diagnostics ---");
            if (searchPanel != null) AddLogMessage($"SearchPanel Bounds: {searchPanel.Bounds}");
            // Find the header panel in the content panel's controls
            var header = contentPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Tag?.ToString() == "TableHeader");
            if (header != null) AddLogMessage($"TableHeaderPanel Bounds: {header.Bounds}"); else AddLogMessage("TableHeaderPanel NOT FOUND in contentPanel!");
            if (tablePanel != null) AddLogMessage($"TablePanel (Rows) Bounds: {tablePanel.Bounds}");
            if (paginationPanel != null) AddLogMessage($"PaginationPanel Bounds: {paginationPanel.Bounds}");
            AddLogMessage($"ContentPanel ClientRectangle: {contentPanel.ClientRectangle}");
            AddLogMessage($"--- End Diagnostics ---");
        }

        private Panel CreateTableHeaderPanel() // Renamed for clarity
        {
            Panel tableHeaderPanel = new Panel
            {
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#F8F9FA"), // Very light gray
                                                                 // Dock is set where it's added now
                Padding = new Padding(20, 0, 20, 0),
                Tag = "TableHeader" // Horizontal padding
            };
            // Removed: parentTablePanel.Controls.Add(tableHeaderPanel);

            // Define column headers
            string[] columnHeaders = { "Name", "condition", "surgery", "Actions" };
            int[] columnStartX = { tableHeaderPanel.Padding.Left,                // Name starts at left padding (Index 0)
                 tableHeaderPanel.Padding.Left + 220,          // Specialty starts after Name column space (Index 1)
                 tableHeaderPanel.Padding.Left + 220 + 220,    // Patients starts after Specialty column space (Index 2)
                 tableHeaderPanel.Padding.Left + 220 + 220 + 170  };

            if (columnStartX.Length != columnHeaders.Length)
            {
                AddLogMessage($"CRITICAL ERROR: Mismatch between columnHeaders ({columnHeaders.Length}) and columnStartX ({columnStartX.Length}) array lengths in CreateTableHeaderPanel.");
                // Maybe return null or an empty panel in case of error
                return tableHeaderPanel; // Or handle error differently
            }

            for (int i = 0; i < columnHeaders.Length; i++)
            {
                Label headerLabel = new Label
                {
                    Text = columnHeaders[i],
                    ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(columnStartX[i], (tableHeaderPanel.ClientSize.Height - 18) / 2), // Vertical center
                    AutoSize = true
                };

                if (columnHeaders[i] == "Actions")
                {
                    int approxActionButtonsWidth = 100;
                    // Use ClientSize cautiously here as panel might not be sized yet, but Anchoring helps
                    headerLabel.Location = new Point(tableHeaderPanel.Width - tableHeaderPanel.Padding.Right - approxActionButtonsWidth, headerLabel.Top); // Position from Right might need Width adjustment later
                    headerLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                }
                else
                {
                    headerLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                }
                tableHeaderPanel.Controls.Add(headerLabel);
            }
            return tableHeaderPanel; // Return the created panel
        }

        // Helper to setup controls within the search panel
        private void InitializeSearchPanelControls(Panel parent)
        {
            // Controls inside Search Panel (positioned relative to parent's padding)
            TextBox searchBox = new TextBox
            {
                Location = new Point(parent.Padding.Left, (parent.ClientSize.Height - 25) / 2), // Vertically center
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11),
                Text = "Search doctors...",
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left // Anchor left
            };
            // Placeholder text logic for searchBox
            searchBox.Enter += (s, e) => { if (searchBox.Text == "Search Patients...") { searchBox.Text = ""; searchBox.ForeColor = Color.Black; } };
            searchBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(searchBox.Text)) { searchBox.Text = "Search Patients..."; searchBox.ForeColor = Color.Gray; } };

            

            // Buttons anchored to the right
            Button refreshButton = new Button // Create Refresh Button
            {
                Size = new Size(90, 28),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "Refresh",
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#2ECC71"), // Green
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Anchor right
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshButton_Click;
            // Position from right edge
            refreshButton.Location = new Point(parent.ClientSize.Width - parent.Padding.Right - refreshButton.Width, (parent.ClientSize.Height - refreshButton.Height) / 2);


            Button addButton = new Button // Create Add Button
            {
                Size = new Size(140, 28),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "Add New Doctor",
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#3498DB"), // Blue
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Anchor right
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += AddButton_Click; // Add handler
                                                // Position left of refresh button
            addButton.Location = new Point(refreshButton.Left - 10 - addButton.Width, refreshButton.Top);


            parent.Controls.Add(searchBox);
            
            parent.Controls.Add(addButton);   // Add before refresh if positioning left-of
            parent.Controls.Add(refreshButton);

        }

        // Helper to setup controls within the pagination panel
        private void InitializePaginationControls(Panel parent)
        {
            int buttonWidth = 40;
            int buttonHeight = 30;
            // We create a fixed number of buttons initially, UpdatePaginationButtons will manage visibility/text
            int maxTotalButtons = (DataSingelton.Instance.Patients.Count) / 10 + 2; // e.g., Prev + 5 numbers + Next

            // Create Prev Button
            Button prevButton = new Button
            {
                Text = "◀",
                Tag = "Prev",
                Size = new Size(buttonWidth, buttonHeight),
                // Style properties... (same as numeric)
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                Cursor = Cursors.Hand
            };
            prevButton.FlatAppearance.BorderSize = 0;
            prevButton.Click += PageButton_Click;
            parent.Controls.Add(prevButton);


            // Create placeholder numeric buttons (max 5 in this example)
            for (int i = 1; i <= maxTotalButtons - 2; i++)
            {
                Button pageButton = new Button
                {
                    Text = i.ToString(), // Initial text
                    Tag = i,             // Store page number in Tag
                    Size = new Size(buttonWidth, buttonHeight),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11),
                    BackColor = Color.White,
                    ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                    Cursor = Cursors.Hand,
                    Visible = false // Initially hidden, UpdatePaginationButtons shows relevant ones
                };
                pageButton.FlatAppearance.BorderSize = 0;
                pageButton.Click += PageButton_Click;
                parent.Controls.Add(pageButton);
            }

            // Create Next Button
            Button nextButton = new Button
            {
                Text = "▶",
                Tag = "Next",
                Size = new Size(buttonWidth, buttonHeight),
                // Style properties...
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#7F8C8D"),
                Cursor = Cursors.Hand
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += PageButton_Click;
            parent.Controls.Add(nextButton);


            // Initial centering - might be slightly off until first resize/update
            CenterPaginationButtons();
        }


       


        // --- Utility / Other Methods ---

        // Optional logging method
        private void AddLogMessage(string message)
        {
            Console.WriteLine($"[DoctorsForm] {DateTime.Now:HH:mm:ss}: {message}");
            // Update status bar etc. here if needed (using Invoke if necessary)
        }

        // Included 'start' method from original code if needed elsewhere
        public async void start()
        {
            try
            {
                s = new SchedulerOrchestrator(); // Assumes this class exists
                main = await s.GenerateOptimalSchedule(); // Assumes this method exists
                schedulesForm = new SchedulesForm(main); // Assumes this class exists
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during 'start' execution: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    }
