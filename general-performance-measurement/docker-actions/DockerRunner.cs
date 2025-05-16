using System;
using System.Diagnostics;
using GeneralPerformanceMeasurement.Models;

namespace GeneralPerformanceMeasurement.Models
{
    public class DockerMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }
}

namespace GeneralPerformanceMeasurement
{
    public class DockerMonitor
    {
        public DockerMetrics GetContainerStats(string containerId)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = $"stats {containerId} --no-stream --format \"{{{{.CPUPerc}}}},{{{{.MemPerc}}}}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd().Trim().Split(',');

                if (output.Length != 2)
                {
                    throw new InvalidOperationException("Invalid docker stats output format");
                }

                return new DockerMetrics
                {
                    CpuUsage = ParsePercentage(output[0]),
                    MemoryUsage = ParsePercentage(output[1])
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Docker stats: {ex.Message}");
                return new DockerMetrics();
            }
        }

        private static double ParsePercentage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            var cleanValue = value.Replace("%", "").Trim();
            return double.TryParse(cleanValue, out var result) ? result : 0;
        }
    }
}
