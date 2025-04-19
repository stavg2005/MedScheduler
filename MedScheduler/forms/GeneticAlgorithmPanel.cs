using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DB;

// using DB; // Assuming DataSingelton is accessible or using DataManager
using Models;

namespace MedScheduler.forms
{
    public partial class GeneticAlgorithmPanel : UserControl
    {
        // --- Orchestrator and Task Management ---
        private SchedulerOrchestrator orchestrator; // Use the orchestrator
        private CancellationTokenSource cancellationSource;
        private Task<Schedule> algorithmTask; // Task now returns the GA schedule part

        // --- State Tracking ---
        private bool isRunning = false;
        private bool isSearching = false; // Flag for random search

        // --- Data Storage (Optional - Orchestrator holds primary data) ---
        private Schedule bestSchedule; // Store the final doctor schedule from orchestrator
        private Statistics finalStats; // Store the final statistics object

        // --- UI Elements ---
        private Panel leftPanel;
        private Panel rightPanel;
        // Parameter TextBoxes
        private TextBox populationSizeTextBox;
        private TextBox maxGenerationsTextBox;
        private TextBox crossoverRateTextBox;
        private TextBox mutationRateTextBox;
        private TextBox specializationMatchTextBox;
        private TextBox workloadBalanceTextBox;
        private TextBox urgencyPriorityTextBox;
        private TextBox continuityOfCareTextBox;
        private TextBox experienceLevelTextBox;
        private TextBox preferenceMatchTextBox;
        // Control Buttons
        private Button runButton;
        private Button stopButton;
        private Button findBestParamsButton; // New Button
        private NumericUpDown iterationsInput; // Input for search iterations
        // Status Display
        private Label statusLabel;
        private Panel statusIndicator;
        // Log Display
        private TextBox algorithmLogTextBox;
        // Results Display Panel
        private Panel bestSolutionPanel;
        private Label patientsAssignedLabel;
        private Label specializationMatchLabel;
        private Label workloadBalanceLabel;
        private Label surgeryCompletionLabel;
        private Label continuityLabel;
        private Label experienceMatchLabel;
        private Label avgPreferenceLabel;
        private Label timeElapsedLabel;
        private Label gaGenerationsLabel;
        private Label convergenceLabel; // *** DECLARED FIELD ***
        private Label bestParamsLabel; // To display best params found

        public GeneticAlgorithmPanel()
        {
            InitializeComponentinner(); // Call base InitializeComponent if it exists
            InitializeCustomComponents(); // Setup custom UI
            SetupInitialValues(); // Set default parameter values in textboxes
        }

        // --- UI Initialization ---

        private void InitializeComponentinner()
        { /* ... Basic form setup ... */
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Name = "GeneticAlgorithmPanel";
            this.Size = new System.Drawing.Size(800, 650);
            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            this.Controls.Clear(); // Clear existing controls if re-initializing

            // Main title
            Label titleLabel = new Label { Text = "Scheduler Control Panel", Location = new Point(20, 15), Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, ForeColor = ColorTranslator.FromHtml("#2C3E50") };
            this.Controls.Add(titleLabel);

            // Main content container
            Panel mainContentPanel = new Panel { Location = new Point(15, 60), Size = new Size(770, 580), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(mainContentPanel);

            // Left panel (Parameters)
            leftPanel = new Panel { Location = new Point(15, 15), Size = new Size(340, 550), BackColor = ColorTranslator.FromHtml("#F8F9FA"), BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };
            mainContentPanel.Controls.Add(leftPanel);

            // Right panel (Status & Results)
            rightPanel = new Panel { Location = new Point(370, 15), Size = new Size(385, 550), BackColor = ColorTranslator.FromHtml("#F8F9FA"), BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            mainContentPanel.Controls.Add(rightPanel);

            InitializeLeftPanel(); // Setup parameter controls
            InitializeRightPanel(); // Setup status and results display
        }

        private void InitializeLeftPanel()
        {
            int currentY = 10;
            int spacing = 38;

            // Parameters section title
            Label parametersTitle = new Label { Text = "Algorithm Parameters", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(15, currentY), AutoSize = true };
            leftPanel.Controls.Add(parametersTitle);
            currentY += 40;

            // GA Parameters
            populationSizeTextBox = AddParameterRow("Population Size:", "100", currentY); currentY += spacing;
            maxGenerationsTextBox = AddParameterRow("Max Generations:", "50", currentY); currentY += spacing;
            crossoverRateTextBox = AddParameterRow("Crossover Rate:", "0.85", currentY); currentY += spacing;
            mutationRateTextBox = AddParameterRow("Mutation Rate:", "0.35", currentY); currentY += spacing;

            // Fitness Weights Title
            Label fitnessWeightsTitle = new Label { Text = "Fitness Weights", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(15, currentY), AutoSize = true };
            leftPanel.Controls.Add(fitnessWeightsTitle);
            currentY += 30;

            // Fitness Weights
            specializationMatchTextBox = AddParameterRow("Specialization Match:", "6.0", currentY); currentY += spacing;
            urgencyPriorityTextBox = AddParameterRow("Urgency Priority:", "4.5", currentY); currentY += spacing;
            workloadBalanceTextBox = AddParameterRow("Workload Balance:", "5.5", currentY); currentY += spacing;
            continuityOfCareTextBox = AddParameterRow("Continuity of Care:", "2.0", currentY); currentY += spacing;
            experienceLevelTextBox = AddParameterRow("Experience Level:", "3.0", currentY); currentY += spacing;
            preferenceMatchTextBox = AddParameterRow("Preference Match:", "4.0", currentY); currentY += spacing;

            // Run and Stop buttons
            runButton = new Button { Text = "Run Scheduler", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, BackColor = ColorTranslator.FromHtml("#2ECC71"), Location = new Point(15, currentY + 10), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            runButton.FlatAppearance.BorderSize = 0;
            runButton.Click += RunButton_Click;
            leftPanel.Controls.Add(runButton);

            stopButton = new Button { Text = "Stop", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Gray, Location = new Point(175, currentY + 10), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, Enabled = false, Cursor = Cursors.Hand };
            stopButton.FlatAppearance.BorderSize = 0;
            stopButton.Click += StopButton_Click;
            leftPanel.Controls.Add(stopButton);
        }

        // Helper to add parameter rows consistently
        private TextBox AddParameterRow(string labelText, string defaultValue, int yPos)
        {
            Label label = new Label { Text = labelText, Font = new Font("Segoe UI", 11F), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(15, yPos + 3), AutoSize = true };
            leftPanel.Controls.Add(label);

            TextBox textBox = new TextBox { Text = defaultValue, Font = new Font("Segoe UI", 11F), Location = new Point(190, yPos), Size = new Size(60, 27), TextAlign = HorizontalAlignment.Right };
            leftPanel.Controls.Add(textBox);

            Button defaultButton = new Button { Text = "Default", Font = new Font("Segoe UI", 9F), Location = new Point(260, yPos), Size = new Size(60, 27), FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#ECF0F1"), Cursor = Cursors.Hand };
            defaultButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#BDC3C7");
            defaultButton.Click += (sender, e) => { textBox.Text = defaultValue; };
            leftPanel.Controls.Add(defaultButton);

            return textBox;
        }


        private void InitializeRightPanel()
        {
            int currentY = 10;
            int labelX = 15;
            int valueX = 200; // X position for value labels

            // Status section title
            Label statusTitle = new Label { Text = "Run Status & Log", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(labelX, currentY), AutoSize = true };
            rightPanel.Controls.Add(statusTitle);
            currentY += 40;

            // Status indicator
            Label statusTextLabel = new Label { Text = "Status:", Font = new Font("Segoe UI", 11F), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(labelX, currentY + 3), AutoSize = true };
            rightPanel.Controls.Add(statusTextLabel);

            statusIndicator = new Panel { Location = new Point(valueX, currentY + 6), Size = new Size(10, 10), BackColor = ColorTranslator.FromHtml("#95A5A6") }; // Start Grey
            RoundCorners(statusIndicator, 5);
            rightPanel.Controls.Add(statusIndicator);

            statusLabel = new Label { Text = "Idle", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#95A5A6"), Location = new Point(valueX + 20, currentY + 3), AutoSize = true };
            rightPanel.Controls.Add(statusLabel);
            currentY += 35;


            // Log Text Box
            Label logLabel = new Label { Text = "Log:", Font = new Font("Segoe UI", 11F), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(labelX, currentY), AutoSize = true };
            rightPanel.Controls.Add(logLabel);
            currentY += 25;

            algorithmLogTextBox = new TextBox { Location = new Point(labelX, currentY), Size = new Size(355, 80), Font = new Font("Consolas", 8), ForeColor = ColorTranslator.FromHtml("#2C3E50"), BackColor = Color.White, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            rightPanel.Controls.Add(algorithmLogTextBox);
            currentY += 95;


            // Results section title
            Label resultsTitle = new Label { Text = "Final Schedule Metrics", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(labelX, currentY), AutoSize = true };
            rightPanel.Controls.Add(resultsTitle);
            currentY += 35;

            // Results Panel
            bestSolutionPanel = new Panel { Location = new Point(labelX, currentY), Size = new Size(355, 260), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom }; // Anchor all sides
            rightPanel.Controls.Add(bestSolutionPanel);

            // Add result labels to the panel
            int resultY = 10;
            int resultSpacing = 22; // Adjusted spacing
            patientsAssignedLabel = AddResultLabel("Reg Patients Assigned:", "-", resultY); resultY += resultSpacing;
            specializationMatchLabel = AddResultLabel("Specialization Match:", "-", resultY); resultY += resultSpacing;
            workloadBalanceLabel = AddResultLabel("Avg Workload:", "-", resultY); resultY += resultSpacing;
            continuityLabel = AddResultLabel("Continuity of Care:", "-", resultY); resultY += resultSpacing;
            experienceMatchLabel = AddResultLabel("Experience Match:", "-", resultY); resultY += resultSpacing;
            avgPreferenceLabel = AddResultLabel("Avg Preference Score:", "-", resultY); resultY += resultSpacing;
            surgeryCompletionLabel = AddResultLabel("Surgery Completion:", "-", resultY); resultY += resultSpacing;
            timeElapsedLabel = AddResultLabel("Time Elapsed:", "-", resultY); resultY += resultSpacing;
            gaGenerationsLabel = AddResultLabel("GA Generations:", "-", resultY); resultY += resultSpacing;
            convergenceLabel = AddResultLabel("Final GA Fitness:", "-", resultY); // Assign the created label to the field
        }

        // Helper to add result labels consistently
        private Label AddResultLabel(string labelText, string defaultValue, int yPos)
        {
            Label labelDesc = new Label { Text = labelText, Font = new Font("Segoe UI", 10F), ForeColor = ColorTranslator.FromHtml("#34495E"), Location = new Point(10, yPos), AutoSize = true };
            bestSolutionPanel.Controls.Add(labelDesc);

            Label labelValue = new Label { Text = defaultValue, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#2C3E50"), Location = new Point(190, yPos), Size = new Size(150, 20), TextAlign = ContentAlignment.MiddleLeft };
            bestSolutionPanel.Controls.Add(labelValue);
            return labelValue; // Return the value label to update later
        }

        private void SetupInitialValues()
        {
            // Reset parameters to defaults shown in textboxes
            populationSizeTextBox.Text = "400";
            maxGenerationsTextBox.Text = "500";
            crossoverRateTextBox.Text = "0.85";
            mutationRateTextBox.Text = "0.1";
            specializationMatchTextBox.Text = "6.0";
            workloadBalanceTextBox.Text = "5.5";
            urgencyPriorityTextBox.Text = "4.5";
            continuityOfCareTextBox.Text = "2.0";
            experienceLevelTextBox.Text = "3.0";
            preferenceMatchTextBox.Text = "4.0";

            // Reset status and results UI
            ResetAlgorithmStateAndUI();
        }

        // --- Event Handlers ---

        private async void RunButton_Click(object sender, EventArgs e)
        {
            if (isRunning) return;

            if (!ValidateInputs()) return;

            ResetAlgorithmStateAndUI();
            AddLogMessage("Initializing Scheduler Orchestrator...");

            orchestrator = new SchedulerOrchestrator();
            if (!SetOrchestratorParameters())
            {
                AddLogMessage("Failed to set parameters. Aborting.");
                return;
            }

            UpdateStatusToRunning();

            cancellationSource = new CancellationTokenSource();
            try
            {
                AddLogMessage("Starting scheduling process...");
                algorithmTask = orchestrator.GenerateOptimalSchedule();
                bestSchedule = await algorithmTask;

                // Process Results based on Task Status
                if (algorithmTask.Status == TaskStatus.RanToCompletion) // *** CORRECTED CHECK ***
                {
                    finalStats = orchestrator.finalStatistics;
                    AddLogMessage($"Scheduling completed successfully in {finalStats.TotalElapsedTimeSeconds:F2} seconds.");
                    AddLogMessage($"GA ran for {finalStats.GaGenerations} generations. Final Fitness: {finalStats.GaFinalFitness:F2}");
                   
                    UpdateResultsUI(finalStats);
                    UpdateStatusToCompleted();

                }
                else if (algorithmTask.IsCanceled)
                {
                    AddLogMessage("Scheduling was cancelled by request.");
                    UpdateStatusToStopped();
                    statusLabel.Text = "Cancelled";
                    statusLabel.ForeColor = ColorTranslator.FromHtml("#E67E22"); // Orange
                    statusIndicator.BackColor = ColorTranslator.FromHtml("#E67E22");
                }
                else if (algorithmTask.IsFaulted)
                {
                    AddLogMessage($"ERROR: Scheduling failed: {algorithmTask.Exception?.InnerExceptions.FirstOrDefault()?.Message ?? algorithmTask.Exception?.Message}");
                    UpdateStatusToStopped();
                    statusLabel.Text = "Error";
                    statusLabel.ForeColor = Color.Red;
                    statusIndicator.BackColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error during scheduling execution: {ex.Message}");
                UpdateStatusToStopped();
                statusLabel.Text = "Error";
                statusLabel.ForeColor = Color.Red;
                statusIndicator.BackColor = Color.Red;
            }
            finally
            {
                isRunning = false;
                // Ensure UI state matches final task status (handles cases where stop might be clicked after task finishes but before UI update)
                if (algorithmTask?.Status == TaskStatus.RanToCompletion && statusLabel.Text != "Completed")
                { UpdateStatusToCompleted(); }
                else if (algorithmTask?.Status == TaskStatus.Canceled && statusLabel.Text != "Cancelled")
                { UpdateStatusToStopped(); statusLabel.Text = "Cancelled"; statusLabel.ForeColor = ColorTranslator.FromHtml("#E67E22"); statusIndicator.BackColor = ColorTranslator.FromHtml("#E67E22"); }
                else if ((algorithmTask?.IsFaulted ?? false) && statusLabel.Text != "Error")
                { UpdateStatusToStopped(); statusLabel.Text = "Error"; statusLabel.ForeColor = Color.Red; statusIndicator.BackColor = Color.Red; }
                else if (statusLabel.Text == "Running") // If stopped manually before completion
                { UpdateStatusToStopped(); }


                cancellationSource?.Dispose();
                cancellationSource = null;
                algorithmTask = null;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (isRunning && cancellationSource != null && !cancellationSource.IsCancellationRequested)
            {
                AddLogMessage("Stop requested by user...");
                cancellationSource.Cancel();
            }
            UpdateStatusToStopped(); // Update UI immediately regardless of task cancellation success
        }

        // --- Helper Methods ---

        private bool SetOrchestratorParameters()
        {
            try
            {
                orchestrator.GaPopulationSize = int.Parse(populationSizeTextBox.Text);
                orchestrator.GaMaxGenerations = int.Parse(maxGenerationsTextBox.Text);
                orchestrator.GaCrossoverRate = double.Parse(crossoverRateTextBox.Text);
                orchestrator.GaMutationRate = double.Parse(mutationRateTextBox.Text);
                orchestrator.GaSpecializationMatchWeight = double.Parse(specializationMatchTextBox.Text);
                orchestrator.GaWorkloadBalanceWeight = double.Parse(workloadBalanceTextBox.Text);
                orchestrator.GaUrgencyWeight = double.Parse(urgencyPriorityTextBox.Text);
                orchestrator.GaContinuityOfCareWeight = double.Parse(continuityOfCareTextBox.Text);
                orchestrator.GaExperienceLevelWeight = double.Parse(experienceLevelTextBox.Text);
                orchestrator.GaPreferenceMatchWeight = double.Parse(preferenceMatchTextBox.Text);
                return true;
            }
            catch (FormatException ex) { MessageBox.Show($"Invalid parameter format: {ex.Message}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            catch (Exception ex) { MessageBox.Show($"Error setting parameters: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
        }

        private void UpdateResultsUI(Statistics stats)
        {
            if (stats == null) { AddLogMessage("Error: Statistics object is null."); ResetResultsLabels(); return; }
            ;
            try
            {
                patientsAssignedLabel.Text = $"{stats.AssignedRegularPatients} / {stats.TotalRegularPatients} ({stats.RegularPatientAssignmentPercentage:F1}%)";
                specializationMatchLabel.Text = $"{stats.SpecializationMatchRatePercent:F1}%";
                workloadBalanceLabel.Text = $"Avg: {stats.AverageDoctorWorkloadPercent:F1}% (StdDev: {stats.StdDevDoctorWorkloadPercent:F1}%)";
                continuityLabel.Text = $"{stats.ContinuityOfCareRatePercent:F1}%";
                experienceMatchLabel.Text = $"{stats.ExperienceMatchRatePercent:F1}%";
                avgPreferenceLabel.Text = $"{stats.AveragePreferenceScore:F2}";
                surgeryCompletionLabel.Text = $"{stats.ScheduledSurgeriesCount} / {stats.TotalSurgeryPatients} ({stats.SurgeryCompletionRatePercent:F1}%)";
                timeElapsedLabel.Text = $"{stats.TotalElapsedTimeSeconds:F2} s";
                gaGenerationsLabel.Text = $"{stats.GaGenerations}";
                convergenceLabel.Text = $"{stats.GaFinalFitness:F1}"; // Update final fitness label
            }
            catch (Exception ex) { AddLogMessage($"Error updating results UI: {ex.Message}"); }
        }

        private void ResetResultsLabels()
        {
            patientsAssignedLabel.Text = "-"; specializationMatchLabel.Text = "-"; workloadBalanceLabel.Text = "-";
            continuityLabel.Text = "-"; experienceMatchLabel.Text = "-"; avgPreferenceLabel.Text = "-";
            surgeryCompletionLabel.Text = "-"; timeElapsedLabel.Text = "-"; gaGenerationsLabel.Text = "-";
            convergenceLabel.Text = "-"; // Reset fitness label
        }

        private void ResetAlgorithmStateAndUI()
        {
            bestSchedule = null;
            finalStats = null;
            isRunning = false;

            statusLabel.Text = "Idle";
            statusLabel.ForeColor = ColorTranslator.FromHtml("#95A5A6");
            statusIndicator.BackColor = ColorTranslator.FromHtml("#95A5A6");

            ResetResultsLabels(); // Use helper

            algorithmLogTextBox?.Clear();

            // *** REMOVE lines accessing non-existent fields ***
            // currentGeneration = 0; // REMOVED
            // bestFitness = 0;       // REMOVED
            // fitnessValues.Clear(); // REMOVED

            if (runButton != null) runButton.Enabled = true;
            if (stopButton != null) { stopButton.Enabled = false; stopButton.BackColor = Color.Gray; }
            SetInputsEnabled(true);
        }

        private void AddLogMessage(string message)
        {
            if (algorithmLogTextBox != null && algorithmLogTextBox.IsHandleCreated)
            {
                if (algorithmLogTextBox.InvokeRequired)
                {
                    try { algorithmLogTextBox.BeginInvoke(new Action<string>(AddLogMessageInternal), message); }
                    catch (Exception) { /* Ignore */ }
                }
                else { AddLogMessageInternal(message); }
            }
            else { Console.WriteLine($"Log (UI Unavail): {message}"); }
        }
        private void AddLogMessageInternal(string message)
        {
            try
            {
                if (algorithmLogTextBox.Text.Length > 5000) { algorithmLogTextBox.Text = algorithmLogTextBox.Text.Substring(algorithmLogTextBox.Text.Length - 4000); }
                algorithmLogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
                algorithmLogTextBox.SelectionStart = algorithmLogTextBox.Text.Length;
                algorithmLogTextBox.ScrollToCaret();
            }
            catch (Exception ex) { Console.WriteLine($"Log Err: {ex.Message}"); }
        }

        // --- Status Update Methods ---
        private void UpdateStatusToRunning()
        { /* ... Implementation ... */
            statusLabel.Text = "Running"; statusLabel.ForeColor = ColorTranslator.FromHtml("#F39C12"); statusIndicator.BackColor = ColorTranslator.FromHtml("#F39C12");
            runButton.Enabled = false; stopButton.Enabled = true; stopButton.BackColor = ColorTranslator.FromHtml("#E74C3C"); SetInputsEnabled(false); isRunning = true;
        }
        private void UpdateStatusToStopped()
        { /* ... Implementation ... */
            statusLabel.Text = "Stopped"; statusLabel.ForeColor = ColorTranslator.FromHtml("#E74C3C"); statusIndicator.BackColor = ColorTranslator.FromHtml("#E74C3C");
            runButton.Enabled = true; stopButton.Enabled = false; stopButton.BackColor = Color.Gray; SetInputsEnabled(true); isRunning = false;
        }
        private void UpdateStatusToCompleted()
        { /* ... Implementation ... */
            statusLabel.Text = "Completed"; statusLabel.ForeColor = ColorTranslator.FromHtml("#2ECC71"); statusIndicator.BackColor = ColorTranslator.FromHtml("#2ECC71");
            runButton.Enabled = true; stopButton.Enabled = false; stopButton.BackColor = Color.Gray; SetInputsEnabled(true); isRunning = false;
        }
        private void SetInputsEnabled(bool enabled)
        { /* ... Implementation ... */
            populationSizeTextBox.Enabled = enabled; maxGenerationsTextBox.Enabled = enabled; crossoverRateTextBox.Enabled = enabled; mutationRateTextBox.Enabled = enabled;
            specializationMatchTextBox.Enabled = enabled; workloadBalanceTextBox.Enabled = enabled; urgencyPriorityTextBox.Enabled = enabled; continuityOfCareTextBox.Enabled = enabled;
            experienceLevelTextBox.Enabled = enabled; preferenceMatchTextBox.Enabled = enabled;
            foreach (var btn in leftPanel.Controls.OfType<Button>().Where(b => b.Text == "Default")) { btn.Enabled = enabled; }
        }

        // --- Input Validation ---
        private bool ValidateInputs()
        { /* ... Implementation ... */
            bool valid = true;
            valid &= ValidateInt(populationSizeTextBox, "Population Size", 1, 10000); valid &= ValidateInt(maxGenerationsTextBox, "Max Generations", 1, 100000);
            valid &= ValidateDouble(crossoverRateTextBox, "Crossover Rate", 0.0, 1.0); valid &= ValidateDouble(mutationRateTextBox, "Mutation Rate", 0.0, 1.0);
            valid &= ValidateDouble(specializationMatchTextBox, "Specialization Weight", 0.0, 1000.0); valid &= ValidateDouble(workloadBalanceTextBox, "Workload Weight", 0.0, 1000.0);
            valid &= ValidateDouble(urgencyPriorityTextBox, "Urgency Weight", 0.0, 1000.0); valid &= ValidateDouble(continuityOfCareTextBox, "Continuity Weight", 0.0, 1000.0);
            valid &= ValidateDouble(experienceLevelTextBox, "Experience Weight", 0.0, 1000.0); valid &= ValidateDouble(preferenceMatchTextBox, "Preference Weight", 0.0, 1000.0);
            return valid;
        }
        private bool ValidateInt(TextBox tb, string name, int min, int max)
        { /* ... Implementation ... */
            if (!int.TryParse(tb.Text, out int val) || val < min || val > max) { MessageBox.Show($"{name} must be an integer between {min} and {max}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            return true;
        }
        private bool ValidateDouble(TextBox tb, string name, double min, double max)
        { /* ... Implementation ... */
            if (!double.TryParse(tb.Text, out double val) || val < min || val > max) { MessageBox.Show($"{name} must be a number between {min} and {max}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
            return true;
        }

        // --- UI Helpers ---
        private void RoundCorners(Control control, int radius)
        { /* ... Implementation ... */
            try { control.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius)); } catch { /* Ignore potential errors during design time or handle creation */ }
        }
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    }
}
