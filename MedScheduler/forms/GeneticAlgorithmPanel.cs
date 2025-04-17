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
using Models;

namespace MedScheduler.forms
{
    public partial class GeneticAlgorithmPanel : UserControl
    {
        // Threading and progress tracking
        private CancellationTokenSource cancellationSource;
        private Task<Schedule> algorithmTask;
        private DoctorScheduler currentScheduler;
        private DoctorSchedulerProgressTracker progressTracker;
        private readonly object updateLock = new object();

        // Progress data
        private List<double> fitnessValues = new List<double>();
        private bool isRunning = false;
        private int currentGeneration = 0;
        private int maxGenerations = 50;
        private double convergence = 0;
        private double bestFitness = 0;

        // Data manager for doctors and patients
        
        SchedulerOrchestrator s = new SchedulerOrchestrator();

        // Algorithm parameters that can be modified in the UI
        private int populationSize = 100;
        private double crossoverRate = 0.85;
        private double mutationRate = 0.35;
        private double specializationMatchWeight = 60.0;
        private double urgencyWeight = 45.0;
        private double workloadBalanceWeight = 55.0;
        private double patientAssignmentWeight = 15.0;
        private double continuityOfCareWeight = 20.0;

        // Schedule results
        private Schedule bestSchedule;
        private int patientsAssigned = 0;
        private int totalPatients = 0;
        private double specializationMatchPercent = 0;
        private double workloadBalancePercent = 0;
        private double urgencyHandlingPercent = 0;

        // UI Elements
        private Panel leftPanel;
        private Panel rightPanel;
        private TextBox populationSizeTextBox;
        private TextBox maxGenerationsTextBox;
        private TextBox crossoverRateTextBox;
        private TextBox mutationRateTextBox;
        private TextBox specializationMatchTextBox;
        private TextBox workloadBalanceTextBox;
        private TextBox urgencyPriorityTextBox;
        private TextBox continuityOfCareTextBox;
        private Button runButton;
        private Button stopButton;
        private Label statusLabel;
        private Panel statusIndicator;
        private Label generationLabel;
        private ProgressBar generationProgressBar;
        private Label convergenceLabel;
        private ProgressBar convergenceProgressBar;
        private Panel chartPanel;
        private Panel bestSolutionPanel;
        private Label patientsAssignedLabel;
        private Label specializationMatchLabel;
        private Label workloadBalanceLabel;
        private Label urgencyHandlingLabel;
        private TextBox algorithmLogTextBox;
        private System.Windows.Forms.Timer uiUpdateTimer;

        public GeneticAlgorithmPanel()
        {
            InitializeComponentinner();
            InitializeCustomComponents();
            SetupInitialValues();

            // Timer for UI updates only (not for simulation)
            uiUpdateTimer = new System.Windows.Forms.Timer();
            uiUpdateTimer.Interval = 500; // Update UI twice per second
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        }

        private void InitializeComponentinner()
        {
            this.SuspendLayout();
            // 
            // GeneticAlgorithmPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.Name = "GeneticAlgorithmPanel";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // Main title
            Label titleLabel = new Label
            {
                Text = "Genetic Algorithm Control Panel",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Main content container
            Panel mainContentPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(760, 520),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainContentPanel);

            // Left panel (Algorithm Parameters)
            leftPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(320, 480),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainContentPanel.Controls.Add(leftPanel);

            // Right panel (Algorithm Visualization)
            rightPanel = new Panel
            {
                Location = new Point(360, 20),
                Size = new Size(380, 480),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainContentPanel.Controls.Add(rightPanel);

            // Initialize left panel components
            InitializeLeftPanel();

            // Initialize right panel components
            InitializeRightPanel();
        }

        private void InitializeLeftPanel()
        {
            // Parameters section title
            Label parametersTitle = new Label
            {
                Text = "Algorithm Parameters",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 10),
                AutoSize = true
            };
            leftPanel.Controls.Add(parametersTitle);

            // Population Size
            Label populationSizeLabel = new Label
            {
                Text = "Population Size:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 50),
                AutoSize = true
            };
            leftPanel.Controls.Add(populationSizeLabel);

            populationSizeTextBox = new TextBox
            {
                Text = "100",
                Font = new Font("Segoe UI", 14),
                Location = new Point(170, 50),
                Size = new Size(80, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(populationSizeTextBox);

            Button populationSizeDefaultButton = new Button
            {
                Text = "Default",
                Font = new Font("Segoe UI", 10),
                Location = new Point(260, 50),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#F8F9FA")
            };
            populationSizeDefaultButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D8E0");
            populationSizeDefaultButton.Click += (sender, e) => { populationSizeTextBox.Text = "100"; };
            leftPanel.Controls.Add(populationSizeDefaultButton);

            // Max Generations
            Label maxGenerationsLabel = new Label
            {
                Text = "Max Generations:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 90),
                AutoSize = true
            };
            leftPanel.Controls.Add(maxGenerationsLabel);

            maxGenerationsTextBox = new TextBox
            {
                Text = "50",
                Font = new Font("Segoe UI", 14),
                Location = new Point(170, 90),
                Size = new Size(80, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(maxGenerationsTextBox);

            Button maxGenerationsDefaultButton = new Button
            {
                Text = "Default",
                Font = new Font("Segoe UI", 10),
                Location = new Point(260, 90),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#F8F9FA")
            };
            maxGenerationsDefaultButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D8E0");
            maxGenerationsDefaultButton.Click += (sender, e) => { maxGenerationsTextBox.Text = "50"; };
            leftPanel.Controls.Add(maxGenerationsDefaultButton);

            // Crossover Rate
            Label crossoverRateLabel = new Label
            {
                Text = "Crossover Rate:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 130),
                AutoSize = true
            };
            leftPanel.Controls.Add(crossoverRateLabel);

            crossoverRateTextBox = new TextBox
            {
                Text = "0.85",
                Font = new Font("Segoe UI", 14),
                Location = new Point(170, 130),
                Size = new Size(80, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(crossoverRateTextBox);

            Button crossoverRateDefaultButton = new Button
            {
                Text = "Default",
                Font = new Font("Segoe UI", 10),
                Location = new Point(260, 130),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#F8F9FA")
            };
            crossoverRateDefaultButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D8E0");
            crossoverRateDefaultButton.Click += (sender, e) => { crossoverRateTextBox.Text = "0.85"; };
            leftPanel.Controls.Add(crossoverRateDefaultButton);

            // Mutation Rate
            Label mutationRateLabel = new Label
            {
                Text = "Mutation Rate:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 170),
                AutoSize = true
            };
            leftPanel.Controls.Add(mutationRateLabel);

            mutationRateTextBox = new TextBox
            {
                Text = "0.35",
                Font = new Font("Segoe UI", 14),
                Location = new Point(170, 170),
                Size = new Size(80, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(mutationRateTextBox);

            Button mutationRateDefaultButton = new Button
            {
                Text = "Default",
                Font = new Font("Segoe UI", 10),
                Location = new Point(260, 170),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#F8F9FA")
            };
            mutationRateDefaultButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#D1D8E0");
            mutationRateDefaultButton.Click += (sender, e) => { mutationRateTextBox.Text = "0.35"; };
            leftPanel.Controls.Add(mutationRateDefaultButton);

            // Fitness Weights section
            Label fitnessWeightsTitle = new Label
            {
                Text = "Fitness Weights",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 210),
                AutoSize = true
            };
            leftPanel.Controls.Add(fitnessWeightsTitle);

            // Specialization Match Weight
            Label specializationMatchLabel = new Label
            {
                Text = "Specialization Match:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 250),
                AutoSize = true
            };
            leftPanel.Controls.Add(specializationMatchLabel);

            specializationMatchTextBox = new TextBox
            {
                Text = "60.0",
                Font = new Font("Segoe UI", 14),
                Location = new Point(190, 250),
                Size = new Size(60, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(specializationMatchTextBox);

            Panel specializationMatchWeightIndicator = new Panel
            {
                Location = new Point(260, 250),
                Size = new Size(50, 25),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            Panel specializationMatchBar = new Panel
            {
                Location = new Point(5, 5),
                Size = new Size(40, 15),
                BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#3498DB"))
            };
            RoundCorners(specializationMatchBar, 8);
            specializationMatchWeightIndicator.Controls.Add(specializationMatchBar);
            leftPanel.Controls.Add(specializationMatchWeightIndicator);

            // Workload Balance Weight
            Label workloadBalanceLabel = new Label
            {
                Text = "Workload Balance:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 290),
                AutoSize = true
            };
            leftPanel.Controls.Add(workloadBalanceLabel);

            workloadBalanceTextBox = new TextBox
            {
                Text = "55.0",
                Font = new Font("Segoe UI", 14),
                Location = new Point(190, 290),
                Size = new Size(60, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(workloadBalanceTextBox);

            Panel workloadBalanceWeightIndicator = new Panel
            {
                Location = new Point(260, 290),
                Size = new Size(50, 25),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            Panel workloadBalanceBar = new Panel
            {
                Location = new Point(5, 5),
                Size = new Size(35, 15),
                BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#2ECC71"))
            };
            RoundCorners(workloadBalanceBar, 8);
            workloadBalanceWeightIndicator.Controls.Add(workloadBalanceBar);
            leftPanel.Controls.Add(workloadBalanceWeightIndicator);

            // Urgency Priority Weight
            Label urgencyPriorityLabel = new Label
            {
                Text = "Urgency Priority:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 330),
                AutoSize = true
            };
            leftPanel.Controls.Add(urgencyPriorityLabel);

            urgencyPriorityTextBox = new TextBox
            {
                Text = "45.0",
                Font = new Font("Segoe UI", 14),
                Location = new Point(190, 330),
                Size = new Size(60, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(urgencyPriorityTextBox);

            Panel urgencyPriorityWeightIndicator = new Panel
            {
                Location = new Point(260, 330),
                Size = new Size(50, 25),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            Panel urgencyPriorityBar = new Panel
            {
                Location = new Point(5, 5),
                Size = new Size(30, 15),
                BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#E74C3C"))
            };
            RoundCorners(urgencyPriorityBar, 8);
            urgencyPriorityWeightIndicator.Controls.Add(urgencyPriorityBar);
            leftPanel.Controls.Add(urgencyPriorityWeightIndicator);

            // Continuity of Care Weight
            Label continuityOfCareLabel = new Label
            {
                Text = "Continuity of Care:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 370),
                AutoSize = true
            };
            leftPanel.Controls.Add(continuityOfCareLabel);

            continuityOfCareTextBox = new TextBox
            {
                Text = "20.0",
                Font = new Font("Segoe UI", 14),
                Location = new Point(190, 370),
                Size = new Size(60, 30),
                TextAlign = HorizontalAlignment.Center
            };
            leftPanel.Controls.Add(continuityOfCareTextBox);

            Panel continuityOfCareWeightIndicator = new Panel
            {
                Location = new Point(260, 370),
                Size = new Size(50, 25),
                BackColor = ColorTranslator.FromHtml("#F8F9FA"),
                BorderStyle = BorderStyle.FixedSingle
            };
            Panel continuityOfCareBar = new Panel
            {
                Location = new Point(5, 5),
                Size = new Size(15, 15),
                BackColor = Color.FromArgb(180, ColorTranslator.FromHtml("#F39C12"))
            };
            RoundCorners(continuityOfCareBar, 7);
            continuityOfCareWeightIndicator.Controls.Add(continuityOfCareBar);
            leftPanel.Controls.Add(continuityOfCareWeightIndicator);

            // Run and Stop buttons
            runButton = new Button
            {
                Text = "Run",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#2ECC71"),
                Location = new Point(20, 430),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat
            };
            runButton.FlatAppearance.BorderSize = 0;
            RoundCorners(runButton, 20);
            runButton.Click += RunButton_Click;
            leftPanel.Controls.Add(runButton);

            stopButton = new Button
            {
                Text = "Stop",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, ColorTranslator.FromHtml("#E74C3C")),
                Location = new Point(180, 430),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            stopButton.FlatAppearance.BorderSize = 0;
            RoundCorners(stopButton, 20);
            stopButton.Click += StopButton_Click;
            leftPanel.Controls.Add(stopButton);
        }

        private void InitializeRightPanel()
        {
            // Algorithm Progress title
            Label progressTitle = new Label
            {
                Text = "Algorithm Progress",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 10),
                AutoSize = true
            };
            rightPanel.Controls.Add(progressTitle);

            // Status indicator
            Label statusTextLabel = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 50),
                AutoSize = true
            };
            rightPanel.Controls.Add(statusTextLabel);

            Panel statusBadge = new Panel
            {
                Location = new Point(100, 50),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(50, ColorTranslator.FromHtml("#2ECC71"))
            };
            RoundCorners(statusBadge, 12);
            rightPanel.Controls.Add(statusBadge);

            statusLabel = new Label
            {
                Text = "Idle",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2ECC71"),
                Location = new Point(20, 0),
                Size = new Size(60, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            statusBadge.Controls.Add(statusLabel);

            statusIndicator = new Panel
            {
                Location = new Point(225, 58),
                Size = new Size(10, 10),
                BackColor = ColorTranslator.FromHtml("#2ECC71")
            };
            RoundCorners(statusIndicator, 5);
            rightPanel.Controls.Add(statusIndicator);

            // Generation progress
            Label generationTextLabel = new Label
            {
                Text = "Generation:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 90),
                AutoSize = true
            };
            rightPanel.Controls.Add(generationTextLabel);

            generationLabel = new Label
            {
                Text = "0 / 50",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(150, 90),
                AutoSize = true
            };
            rightPanel.Controls.Add(generationLabel);

            generationProgressBar = new ProgressBar
            {
                Location = new Point(20, 125),
                Size = new Size(340, 10),
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            rightPanel.Controls.Add(generationProgressBar);

            // Convergence progress
            Label convergenceTextLabel = new Label
            {
                Text = "Fitness Score:",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(20, 150),
                AutoSize = true
            };
            rightPanel.Controls.Add(convergenceTextLabel);

            convergenceLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(150, 150),
                AutoSize = true
            };
            rightPanel.Controls.Add(convergenceLabel);

            convergenceProgressBar = new ProgressBar
            {
                Location = new Point(20, 185),
                Size = new Size(340, 10),
                Value = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous
            };
            rightPanel.Controls.Add(convergenceProgressBar);

            // Custom Fitness Chart Panel
            chartPanel = new Panel
            {
                Location = new Point(20, 205),
                Size = new Size(340, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            rightPanel.Controls.Add(chartPanel);

            Label chartTitle = new Label
            {
                Text = "Fitness Over Time",
                Font = new Font("Segoe UI", 14),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(100, 5),
                AutoSize = true
            };
            chartPanel.Controls.Add(chartTitle);

            // Chart will be drawn directly on the panel when data is available
            chartPanel.Paint += ChartPanel_Paint;

            // Algorithm Log
            algorithmLogTextBox = new TextBox
            {
                Location = new Point(20, 365),
                Size = new Size(340, 50),
                Font = new Font("Consolas", 8),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                BackColor = Color.White,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            rightPanel.Controls.Add(algorithmLogTextBox);

            // Best Solution Preview
            bestSolutionPanel = new Panel
            {
                Location = new Point(20, 425),
                Size = new Size(340, 45),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            rightPanel.Controls.Add(bestSolutionPanel);

            // Solution metrics
            patientsAssignedLabel = new Label
            {
                Text = "Patients Assigned: 0/0",
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(10, 5),
                AutoSize = true
            };
            bestSolutionPanel.Controls.Add(patientsAssignedLabel);

            specializationMatchLabel = new Label
            {
                Text = "Specialization Match: 0%",
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(10, 25),
                AutoSize = true
            };
            bestSolutionPanel.Controls.Add(specializationMatchLabel);

            workloadBalanceLabel = new Label
            {
                Text = "Workload Balance: 0%",
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(180, 5),
                AutoSize = true
            };
            bestSolutionPanel.Controls.Add(workloadBalanceLabel);

            urgencyHandlingLabel = new Label
            {
                Text = "Urgency Handling: 0%",
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#2C3E50"),
                Location = new Point(180, 25),
                AutoSize = true
            };
            bestSolutionPanel.Controls.Add(urgencyHandlingLabel);
        }

        private void ChartPanel_Paint(object sender, PaintEventArgs e)
        {
            if (fitnessValues.Count == 0)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Chart drawing area
            Rectangle chartArea = new Rectangle(40, 30, 280, 110);

            // Draw axes
            Pen axisPen = new Pen(Color.LightGray, 1);
            g.DrawLine(axisPen, chartArea.Left, chartArea.Bottom, chartArea.Right, chartArea.Bottom); // X-axis
            g.DrawLine(axisPen, chartArea.Left, chartArea.Top, chartArea.Left, chartArea.Bottom);    // Y-axis

            // Draw labels
            Font labelFont = new Font("Segoe UI", 8);
            g.DrawString("Generations", labelFont, Brushes.Gray, chartArea.Left + chartArea.Width / 2 - 30, chartArea.Bottom + 5);
            g.DrawString("Fitness", labelFont, Brushes.Gray, chartArea.Left - 35, chartArea.Top + chartArea.Height / 2 - 15);

            // Find the max fitness value
            double maxFitnessValue = fitnessValues.Max();
            if (maxFitnessValue == 0) maxFitnessValue = 1; // Prevent division by zero

            // Calculate scale for X and Y axes
            int xScale = fitnessValues.Count <= 1 ? 1 : chartArea.Width / (fitnessValues.Count - 1);
            float yScale = chartArea.Height / (float)maxFitnessValue;

            // Draw the fitness curve
            if (fitnessValues.Count > 1)
            {
                Pen fitnessPen = new Pen(ColorTranslator.FromHtml("#3498DB"), 2);
                Point[] points = new Point[fitnessValues.Count];

                for (int i = 0; i < fitnessValues.Count; i++)
                {
                    int x = chartArea.Left + i * xScale;
                    int y = chartArea.Bottom - (int)(fitnessValues[i] * yScale);
                    points[i] = new Point(x, y);
                }

                g.DrawLines(fitnessPen, points);

                // Draw the last point as a circle
                if (fitnessValues.Count > 0)
                {
                    int lastIndex = fitnessValues.Count - 1;
                    int x = chartArea.Left + lastIndex * xScale;
                    int y = chartArea.Bottom - (int)(fitnessValues[lastIndex] * yScale);
                    g.FillEllipse(new SolidBrush(ColorTranslator.FromHtml("#3498DB")), x - 4, y - 4, 8, 8);
                }
            }
            else if (fitnessValues.Count == 1)
            {
                // If there's only one point, draw it
                int x = chartArea.Left;
                int y = chartArea.Bottom - (int)(fitnessValues[0] * yScale);
                g.FillEllipse(new SolidBrush(ColorTranslator.FromHtml("#3498DB")), x - 4, y - 4, 8, 8);
            }
        }

        private void SetupInitialValues()
        {
            populationSizeTextBox.Text = "100";
            maxGenerationsTextBox.Text = "50";
            crossoverRateTextBox.Text = "0.85";
            mutationRateTextBox.Text = "0.35";
            specializationMatchTextBox.Text = "60.0";
            workloadBalanceTextBox.Text = "55.0";
            urgencyPriorityTextBox.Text = "45.0";
            continuityOfCareTextBox.Text = "20.0";

            statusLabel.Text = "Idle";
            statusLabel.ForeColor = ColorTranslator.FromHtml("#2ECC71");
            statusIndicator.BackColor = ColorTranslator.FromHtml("#2ECC71");

            generationLabel.Text = "0 / 50";
            generationProgressBar.Value = 0;
            convergenceLabel.Text = "0";
            convergenceProgressBar.Value = 0;

            patientsAssignedLabel.Text = "Patients Assigned: 0/0";
            specializationMatchLabel.Text = "Specialization Match: 0%";
            workloadBalanceLabel.Text = "Workload Balance: 0%";
            urgencyHandlingLabel.Text = "Urgency Handling: 0%";

            // Clear fitness values
            fitnessValues.Clear();
            chartPanel.Invalidate();

            // Clear log
            algorithmLogTextBox.Clear();
        }

        // This is called by the UI update timer, not for algorithm simulation
        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            // No need to update if we're not running
            if (!isRunning) return;

            // Check if the algorithm has finished
            if (algorithmTask != null && algorithmTask.IsCompleted)
            {
                uiUpdateTimer.Stop();
                isRunning = false;

                // Stop the progress tracker
                if (progressTracker != null)
                {
                    progressTracker.Stop();
                }

                UpdateStatusToCompleted();
            }
        }




        // This is called by the progress tracker when there's an update from the algorithm
        private void OnProgressUpdate(int generation, double fitness, string message)
        {

            // Ensure thread-safe UI updates
            if (InvokeRequired)
            {
                Invoke(new Action<int, double, string>(OnProgressUpdate), generation, fitness, message);
                return;
            }

            try
            {
                // Validate inputs
                generation = Math.Max(0, generation);
                fitness = Math.Max(0, fitness);

                // Update our state
                currentGeneration = generation;
                bestFitness = fitness;

                // Add to fitness values
                fitnessValues.Add(fitness);

                // Keep only recent fitness values to prevent memory growth
                if (fitnessValues.Count > 100)
                {
                    fitnessValues.RemoveAt(0);
                }

                // Log the message
                AddLogMessage(message);

                // Update UI 
                UpdateUI();

                // Update chart
                chartPanel.Invalidate();
            }
            catch (Exception ex)
            {
                // Log any errors during progress update
                AddLogMessage($"Error in progress update: {ex.Message}");
            }
        }
        private async void RunButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Get parameters from inputs
            populationSize = int.Parse(populationSizeTextBox.Text);
            maxGenerations = int.Parse(maxGenerationsTextBox.Text);
            crossoverRate = double.Parse(crossoverRateTextBox.Text);
            mutationRate = double.Parse(mutationRateTextBox.Text);
            specializationMatchWeight = double.Parse(specializationMatchTextBox.Text);
            workloadBalanceWeight = double.Parse(workloadBalanceTextBox.Text);
            urgencyWeight = double.Parse(urgencyPriorityTextBox.Text);
            continuityOfCareWeight = double.Parse(continuityOfCareTextBox.Text);

            // Reset UI state
            ResetAlgorithmState();

            // Initialize cancellation token source
            cancellationSource = new CancellationTokenSource();

            // Start UI update timer
            uiUpdateTimer.Start();

            // Update UI
            UpdateStatusToRunning();

            try
            {
                // Get the data from the data manager
                List<Doctor> doctors = DataSingelton.Instance.Doctors;
                List<Patient> patients = DataSingelton.Instance.Patients;
                
                totalPatients = patients.Count(p => !p.NeedsSurgery);

                // Create a scheduler with the user parameters
                var scheduler = new DoctorScheduler(populationSize, doctors, patients);
              
                // Set parameters via reflection
                SetSchedulerParameters(scheduler);

                AddLogMessage($"Starting genetic algorithm with population size: {populationSize}, generations: {maxGenerations}");
                AddLogMessage($"Doctors: {doctors.Count}, Patients: {patients.Count}");

                // Store the scheduler for progress monitoring
                currentScheduler = scheduler;

                // Create a progress tracker that will monitor the algorithm
                progressTracker = new DoctorSchedulerProgressTracker(
        scheduler,
        OnProgressUpdate,
        AddLogMessage  // Pass the logging function
    );

                // Run the algorithm on a background thread
                algorithmTask = Task.Run(() => scheduler.Solve(), cancellationSource.Token);

                // DON'T await here - let it run in background
                // The UI updates will come from the progress tracker
                // The task completion will be detected by the UI timer

                // Final processing will happen when the task completes
                await algorithmTask.ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                    {
                        // Get the result
                        var result = task.Result;

                        // Use Invoke to safely update UI from a background thread
                        this.Invoke((Action)(() =>
                        {
                            bestSchedule = result;
                            CalculateMetrics(bestSchedule, DataSingelton.Instance.Doctors, DataSingelton.Instance.Patients);
                            AddLogMessage($"Algorithm completed. Final fitness: {bestFitness:F1}");
                            UpdateStatusToCompleted();
                        }));
                    }
                }, TaskScheduler.Default);

                // Calculate final metrics
                
                MainForm.main = bestSchedule;
                MainForm.schedulesForm = new SchedulesForm(MainForm.main);
                AddLogMessage($"Algorithm completed. Final fitness: {bestFitness:F1}");
            }
            catch (OperationCanceledException)
            {
                AddLogMessage("Algorithm was canceled by user.");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error: {ex.Message}");
                UpdateStatusToStopped();
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (cancellationSource != null && !cancellationSource.IsCancellationRequested)
            {
                cancellationSource.Cancel();
                AddLogMessage("Stopping algorithm...");
            }

            uiUpdateTimer.Stop();
            isRunning = false;
            UpdateStatusToStopped();
        }

        private async void RunGeneticAlgorithm(CancellationToken cancellationToken)
        {
            try
            {
                // Get the data from the data manager
                List<Doctor> doctors = DataSingelton.Instance.Doctors;
                List<Patient> patients = DataSingelton.Instance.Patients;

                totalPatients = patients.Count(p => !p.NeedsSurgery);

                // Create our scheduler with the user parameters
                DoctorScheduler scheduler = new DoctorScheduler(
                    populationSize,
                    doctors,
                    patients
                );

                // Set parameters via reflection
                SetSchedulerParameters(scheduler);

                AddLogMessage($"Starting genetic algorithm with population size: {populationSize}, generations: {maxGenerations}");
                AddLogMessage($"Doctors: {doctors.Count}, Patients: {patients.Count}");

                // Store the scheduler in the task state so we can access it for progress updates
                var scheduler_X = new DoctorScheduler(populationSize, doctors, patients);

                // Set parameters via reflection
                SetSchedulerParameters(scheduler_X);

                AddLogMessage($"Starting genetic algorithm with population size: {populationSize}, generations: {maxGenerations}");
                AddLogMessage($"Doctors: {doctors.Count}, Patients: {patients.Count}");

                // Create a TaskCompletionSource to manage the task manually
                var tcs = new TaskCompletionSource<Schedule>();

                // Start the algorithm on a background thread
                Task.Run(() =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                            return;
                        }

                        // Run the solve method
                        Schedule result = scheduler.Solve();

                        // Set the result
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        // Propagate the exception
                        tcs.SetException(ex);
                    }
                });

                // Store references for progress monitoring
                algorithmTask = tcs.Task;

                // Set the scheduler as a field so we can access it for monitoring
                currentScheduler = scheduler;

                // Wait for completion
                bestSchedule = await algorithmTask;

                // Calculate final metrics
                CalculateMetrics(bestSchedule, doctors, patients);

                AddLogMessage($"Algorithm completed. Final fitness: {bestFitness:F1}");
            }
            catch (OperationCanceledException)
            {
                throw; // Rethrow cancellation
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error in algorithm: {ex.Message}");
                throw; // Rethrow other exceptions
            }
        }

        // Set scheduler parameters via reflection
        private void SetSchedulerParameters(DoctorScheduler scheduler)
        {
            try
            {
                var type = typeof(DoctorScheduler);

                // Set maxGenerations
                var maxGenField = type.GetField("maxGenerations",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (maxGenField != null)
                {
                    maxGenField.SetValue(scheduler, maxGenerations);
                }

                // Set crossoverRate
                var crossoverField = type.GetField("crossoverRate",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (crossoverField != null)
                {
                    crossoverField.SetValue(scheduler, crossoverRate);
                }

                // Set mutationRate
                var mutationField = type.GetField("mutationRate",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (mutationField != null)
                {
                    mutationField.SetValue(scheduler, mutationRate);
                }

                // Set fitness weights
                SetFitnessWeight(scheduler, "specializationMatchWeight", specializationMatchWeight);
                SetFitnessWeight(scheduler, "urgencyWeight", urgencyWeight);
                SetFitnessWeight(scheduler, "workloadBalanceWeight", workloadBalanceWeight);
                SetFitnessWeight(scheduler, "patientAssignmentWeight", patientAssignmentWeight);
                SetFitnessWeight(scheduler, "continuityOfCareWeight", continuityOfCareWeight);

                AddLogMessage("Parameters set successfully");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error setting parameters: {ex.Message}");
            }
        }

        private void SetFitnessWeight(DoctorScheduler scheduler, string fieldName, double value)
        {
            var field = typeof(DoctorScheduler).GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(scheduler, value);
            }
        }

        private void CalculateMetrics(Schedule schedule, List<Doctor> doctors, List<Patient> patients)
        {
            if (schedule == null)
                return;

            // Calculate metrics based on the schedule
            patientsAssigned = schedule.PatientToDoctor.Count;

            // Specialization match percentage
            int correctSpecializationCount = 0;
            int highUrgencyCorrectSpecCount = 0;
            int highUrgencyCount = 0;

            foreach (var pair in schedule.PatientToDoctor)
            {
                int patientId = pair.Key;
                int doctorId = pair.Value;

                Patient patient = patients.FirstOrDefault(p => p.Id == patientId);
                Doctor doctor = doctors.FirstOrDefault(d => d.Id == doctorId);

                if (patient != null && doctor != null)
                {
                    bool isSpecMatch = doctor.Specialization == patient.RequiredSpecialization;

                    if (isSpecMatch)
                    {
                        correctSpecializationCount++;
                    }

                    // Track high urgency cases specifically
                    if (patient.GetUrgencyValue() == 3) // High urgency
                    {
                        highUrgencyCount++;
                        if (isSpecMatch)
                        {
                            highUrgencyCorrectSpecCount++;
                        }
                    }
                }
            }

            specializationMatchPercent = patientsAssigned > 0 ?
                (double)correctSpecializationCount / patientsAssigned * 100 : 0;

            urgencyHandlingPercent = highUrgencyCount > 0 ?
                (double)highUrgencyCorrectSpecCount / highUrgencyCount * 100 : 100;

            // Workload balance
            var doctorWorkloads = doctors
                .Where(d => schedule.DoctorToPatients.ContainsKey(d.Id))
                .Select(d => (double)schedule.DoctorToPatients[d.Id].Count / d.MaxWorkload)
                .ToList();

            if (doctorWorkloads.Any())
            {
                double avgWorkload = doctorWorkloads.Average();
                double maxDeviation = doctorWorkloads.Max(w => Math.Abs(w - avgWorkload));

                // Calculate workload balance as inversely related to deviation
                workloadBalancePercent = 100 * (1 - maxDeviation);
                if (workloadBalancePercent < 0) workloadBalancePercent = 0;
            }
            else
            {
                workloadBalancePercent = 0;
            }
        }

        private void UpdateUI()
        {
            


            // Update generation information
            generationLabel.Text = $"{currentGeneration} / {maxGenerations}";
            generationProgressBar.Maximum = maxGenerations;
            generationProgressBar.Value = Math.Min(currentGeneration, maxGenerations);

            // Update fitness information
            convergenceLabel.Text = $"{bestFitness:F1}";

            // Normalize convergence to 0-100 for progress bar
            // Assuming a maximum possible score based on patients/doctors
            int totalDoctors = DataSingelton.Instance.Doctors.Count;
            int totalPatients = DataSingelton.Instance.Patients.Count;
            double estimatedMaxScore = totalPatients * 100; // Rough estimate

            int convergencePercent = (int)Math.Min(100, (bestFitness / estimatedMaxScore) * 100);
            convergenceProgressBar.Value = convergencePercent;

            // Update fitness chart
            chartPanel.Invalidate();

            // Update best solution metrics if we have a schedule
            if (bestSchedule != null)
            {
                patientsAssignedLabel.Text = $"Patients Assigned: {patientsAssigned}/{totalPatients}";
                specializationMatchLabel.Text = $"Specialization Match: {specializationMatchPercent:F1}%";
                workloadBalanceLabel.Text = $"Workload Balance: {workloadBalancePercent:F1}%";
                urgencyHandlingLabel.Text = $"Urgency Handling: {urgencyHandlingPercent:F1}%";
            }
        }

        private void ResetAlgorithmState()
        {
            currentGeneration = 0;
            bestFitness = 0;
            convergence = 0;
            fitnessValues.Clear();
            bestSchedule = null;
            patientsAssigned = 0;
            specializationMatchPercent = 0;
            workloadBalancePercent = 0;
            urgencyHandlingPercent = 0;

            algorithmLogTextBox.Clear();
        }

        private void AddLogMessage(string message)
        {
            // Ensure thread safety
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLogMessage), message);
                return;
            }

            algorithmLogTextBox.AppendText(message + Environment.NewLine);

            // Keep most recent messages visible
            algorithmLogTextBox.SelectionStart = algorithmLogTextBox.Text.Length;
            algorithmLogTextBox.ScrollToCaret();
        }

        private void UpdateStatusToRunning()
        {
            statusLabel.Text = "Running";
            statusLabel.ForeColor = ColorTranslator.FromHtml("#2ECC71");
            statusIndicator.BackColor = ColorTranslator.FromHtml("#2ECC71");

            runButton.Enabled = false;
            stopButton.Enabled = true;
            stopButton.BackColor = ColorTranslator.FromHtml("#E74C3C");

            // Disable inputs while running
            populationSizeTextBox.Enabled = false;
            maxGenerationsTextBox.Enabled = false;
            crossoverRateTextBox.Enabled = false;
            mutationRateTextBox.Enabled = false;
            specializationMatchTextBox.Enabled = false;
            workloadBalanceTextBox.Enabled = false;
            urgencyPriorityTextBox.Enabled = false;
            continuityOfCareTextBox.Enabled = false;

            isRunning = true;
        }

        private void UpdateStatusToStopped()
        {
            statusLabel.Text = "Stopped";
            statusLabel.ForeColor = ColorTranslator.FromHtml("#E74C3C");
            statusIndicator.BackColor = ColorTranslator.FromHtml("#E74C3C");

            runButton.Enabled = true;
            stopButton.Enabled = false;
            stopButton.BackColor = Color.FromArgb(100, ColorTranslator.FromHtml("#E74C3C"));

            // Re-enable inputs
            populationSizeTextBox.Enabled = true;
            maxGenerationsTextBox.Enabled = true;
            crossoverRateTextBox.Enabled = true;
            mutationRateTextBox.Enabled = true;
            specializationMatchTextBox.Enabled = true;
            workloadBalanceTextBox.Enabled = true;
            urgencyPriorityTextBox.Enabled = true;
            continuityOfCareTextBox.Enabled = true;

            isRunning = false;
        }

        private void UpdateStatusToCompleted()
        {
            MainForm.assignments = new assignment();
            statusLabel.Text = "Completed";
            statusLabel.ForeColor = ColorTranslator.FromHtml("#3498DB");
            statusIndicator.BackColor = ColorTranslator.FromHtml("#3498DB");

            runButton.Enabled = true;
            stopButton.Enabled = false;
            stopButton.BackColor = Color.FromArgb(100, ColorTranslator.FromHtml("#E74C3C"));

            // Re-enable inputs
            populationSizeTextBox.Enabled = true;
            maxGenerationsTextBox.Enabled = true;
            crossoverRateTextBox.Enabled = true;
            mutationRateTextBox.Enabled = true;
            specializationMatchTextBox.Enabled = true;
            workloadBalanceTextBox.Enabled = true;
            urgencyPriorityTextBox.Enabled = true;
            continuityOfCareTextBox.Enabled = true;

            isRunning = false;
        }

        private bool ValidateInputs()
        {
            // Validate population size
            if (!int.TryParse(populationSizeTextBox.Text, out int populationSize) || populationSize <= 0)
            {
                MessageBox.Show("Population size must be a positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate max generations
            if (!int.TryParse(maxGenerationsTextBox.Text, out int generations) || generations <= 0)
            {
                MessageBox.Show("Max generations must be a positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate crossover rate
            if (!double.TryParse(crossoverRateTextBox.Text, out double crossoverRate) || crossoverRate < 0 || crossoverRate > 1)
            {
                MessageBox.Show("Crossover rate must be a value between 0 and 1.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate mutation rate
            if (!double.TryParse(mutationRateTextBox.Text, out double mutationRate) || mutationRate < 0 || mutationRate > 1)
            {
                MessageBox.Show("Mutation rate must be a value between 0 and 1.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate specialization match weight
            if (!double.TryParse(specializationMatchTextBox.Text, out double specializationMatch) || specializationMatch < 0)
            {
                MessageBox.Show("Specialization match weight must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate workload balance weight
            if (!double.TryParse(workloadBalanceTextBox.Text, out double workloadBalance) || workloadBalance < 0)
            {
                MessageBox.Show("Workload balance weight must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate urgency priority weight
            if (!double.TryParse(urgencyPriorityTextBox.Text, out double urgencyPriority) || urgencyPriority < 0)
            {
                MessageBox.Show("Urgency priority weight must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Validate continuity of care weight
            if (!double.TryParse(continuityOfCareTextBox.Text, out double continuityOfCare) || continuityOfCare < 0)
            {
                MessageBox.Show("Continuity of care weight must be a positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void RoundCorners(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            control.Region = new Region(path);
        }

        // Patched DoctorScheduler class is no longer needed, as we're using reflection directly
        // to monitor the algorithm's progress
    }
}