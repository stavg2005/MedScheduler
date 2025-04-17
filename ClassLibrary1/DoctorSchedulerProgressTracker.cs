using System;
using System.Reflection;
using System.Threading;
using Models;

public class DoctorSchedulerProgressTracker
{
    private readonly DoctorScheduler scheduler;
    private readonly System.Threading.Timer progressTimer;
    private readonly Action<int, double, string> progressCallback;
    private readonly Action<string> logCallback;
    private int lastReportedGeneration = -1;
    private double lastReportedFitness = 0;

    public DoctorSchedulerProgressTracker(
        DoctorScheduler scheduler,
        Action<int, double, string> progressCallback,
        Action<string> logCallback)
    {
        this.scheduler = scheduler;
        this.progressCallback = progressCallback;
        this.logCallback = logCallback;

        // Log that we're starting
        logCallback("Progress tracker initializing");

        // Use reflection to make fields accessible
        MakeFieldsAccessible();

        // Check immediately then every 500ms
        progressTimer = new System.Threading.Timer(CheckProgress, null, 0, 500);
        logCallback("Progress tracker timer started");
    }

    private void MakeFieldsAccessible()
    {
        Type type = typeof(DoctorScheduler);

        // Make fields public or accessible
        var fields = new[]
        {
            "currentGeneration",
            "bestFitness",
            "maxGenerations"
        };

        foreach (var fieldName in fields)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(scheduler, field.GetValue(scheduler));
            }
        }
    }

    private void CheckProgress(object state)
    {
        try
        {
            // Log that we're checking
            logCallback("Checking scheduler progress...");

            // Use reflection to get current values
            Type type = typeof(DoctorScheduler);

            // Try to get fields using different binding flags
            var genField = type.GetField("currentGeneration",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var fitnessField = type.GetField("bestFitness",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var maxGenField = type.GetField("maxGenerations",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (genField == null || fitnessField == null || maxGenField == null)
            {
                logCallback("ERROR: Could not find required fields in DoctorScheduler");
                return;
            }

            int currentGen = (int)genField.GetValue(scheduler);
            double currentFitness = (double)fitnessField.GetValue(scheduler);
            int maxGenerations = (int)maxGenField.GetValue(scheduler);

            logCallback($"Current values - Gen: {currentGen}/{maxGenerations}, Fitness: {currentFitness}");

            // Report if values have changed
            if (currentGen > lastReportedGeneration || Math.Abs(currentFitness - lastReportedFitness) > 0.1)
            {
                lastReportedGeneration = currentGen;
                lastReportedFitness = currentFitness;

                logCallback("Reporting progress update");
                progressCallback(currentGen, currentFitness,
                    $"Generation {currentGen}/{maxGenerations}: fitness={currentFitness:F1}");
            }
        }
        catch (Exception ex)
        {
            logCallback($"ERROR in progress tracking: {ex.Message}");
        }
    }

    public void Stop()
    {
        progressTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        progressTimer?.Dispose();
        logCallback("Progress tracker stopped");
    }
}