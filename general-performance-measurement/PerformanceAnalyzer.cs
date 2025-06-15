using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using GeneralPerformanceMeasurement.Models;

namespace GeneralPerformanceMeasurement.Analysis
{
    class PerformanceAnalyzer
    {
        public static void RunAnalysis()
        {
            var dockerDataPath = Environment.GetEnvironmentVariable("DOCKER_DATA_PATH") ?? "./docker-actions";
            var vmDataPath = Environment.GetEnvironmentVariable("VM_DATA_PATH") ?? "./vm-actions";

            if (!Directory.Exists(dockerDataPath) || !Directory.Exists(vmDataPath))
            {
                Console.WriteLine($"Error: Data directories not found. Checked:\n- {dockerDataPath}\n- {vmDataPath}");
                return;
            }

            var dockerData = LoadData(dockerDataPath);
            var vmData = LoadData(vmDataPath);

            if (!dockerData.HasValue || !vmData.HasValue)
            {
                Console.WriteLine("Error: Failed to load performance data.");
                return;
            }

            var analysis = new AnalysisResult
            {
                Docker = CalculateAverages(dockerData.Value),
                VM = CalculateAverages(vmData.Value),
                Comparison = new Comparison
                {
                    FpsDifference = CalculateDifference(dockerData.Value.AverageFps, vmData.Value.AverageFps),
                    CpuDifference = CalculateDifference(dockerData.Value.AverageCpu, vmData.Value.AverageCpu),
                    MemoryDifference = CalculateDifference(dockerData.Value.AverageMemory, vmData.Value.AverageMemory)
                }
            };

            File.WriteAllText("analysis.json", JsonConvert.SerializeObject(analysis, Formatting.Indented));
            PrintResults(analysis);
        }

        static (double AverageFps, double AverageCpu, double AverageMemory)? LoadData(string path)
        {
            try
            {
                var files = Directory.GetFiles(path, "*.json");
                if (files.Length == 0)
                {
                    Console.WriteLine($"No JSON files found in {path}");
                    return null;
                }

                var dataPoints = new List<PerformanceResult>();

                foreach (var file in files)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<PerformanceResult>(File.ReadAllText(file));
                        if (result != null &&
                            result.FpsStats != null &&
                            result.SystemMetrics != null)
                        {
                            // Skip initial placeholder files
                            if (JObject.Parse(File.ReadAllText(file)).ContainsKey("IsInitialFile"))
                            {
                                continue;
                            }
                            
                            // Calculate average FPS if not already set and there are values
                            if (result.FpsStats.Values != null && result.FpsStats.Values.Any())
                            {
                                result.FpsStats.Average = result.FpsStats.Values.Average();
                            }
                            else
                            {
                                // Set to 0 or the already existing average if Values is empty
                                result.FpsStats.Average = result.FpsStats.Average;
                            }

                            // Add to the list if valid
                            dataPoints.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading file {file}: {ex.Message}");
                    }
                }

                if (dataPoints.Count == 0)
                {
                    Console.WriteLine($"No valid performance results found in {path}");
                    return null;
                }

                // Safe average calculations with default values
                return (
                    dataPoints.Average(r => r.FpsStats?.Average ?? 0),
                    dataPoints.Average(r => r.SystemMetrics?.CpuUsage ?? 0),
                    dataPoints.Average(r => r.SystemMetrics?.MemoryUsage ?? 0)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from {path}: {ex.Message}");
                return null;
            }
        }

        static EnvironmentMetrics CalculateAverages((double Fps, double Cpu, double Memory) data) =>
            new EnvironmentMetrics
            {
            AverageFps = data.Fps,
            AverageCpu = data.Cpu,
            AverageMemory = data.Memory
            };

        static double CalculateDifference(double a, double b) =>
            b == 0 ? 0 : Math.Round((a - b) / b * 100, 2);

        static void PrintResults(AnalysisResult results)
        {
            Console.WriteLine("Performance Comparison Results\n");
            Console.WriteLine($"Environment      | FPS (Avg) | CPU (%) | Memory (%)");
            Console.WriteLine($"-------------------------------------------------");
            Console.WriteLine($"Docker           | {results.Docker?.AverageFps:F2}    | {results.Docker?.AverageCpu:F2}  | {results.Docker?.AverageMemory:F2}");
            Console.WriteLine($"VM               | {results.VM?.AverageFps:F2}    | {results.VM?.AverageCpu:F2}  | {results.VM?.AverageMemory:F2}");
            Console.WriteLine($"\nComparison (% Difference):");
            Console.WriteLine($"FPS: {results.Comparison?.FpsDifference}%");
            Console.WriteLine($"CPU Usage: {results.Comparison?.CpuDifference}%");
            Console.WriteLine($"Memory Usage: {results.Comparison?.MemoryDifference}%");
        }
    }

}