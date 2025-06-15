using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GeneralPerformanceMeasurement.Models;
using System.Text.RegularExpressions;

namespace GeneralPerformanceMeasurement
{
    public class PerformanceMeasurer
    {
        public static async Task RunAsync(string[] args)
        {
            if (!Directory.Exists("./docker-actions"))
                Directory.CreateDirectory("./docker-actions");
            
            if (!Directory.Exists("./vm-actions"))
                Directory.CreateDirectory("./vm-actions");

            CreateInitialJsonIfNeeded("./docker-actions");
            CreateInitialJsonIfNeeded("./vm-actions");

            // Get environment from args
            var environment = args.Length > 1 ? args[1] : "docker";
            var outputDir = environment == "docker" ? "./docker-actions" : "./vm-actions";

            string containerId = "";
            if (args.Length > 2)
            {
                containerId = args[2];
            }
            else if (environment == "docker")
            {
                Console.WriteLine("Enter Docker container ID or name to monitor:");
                if (containerId == "")
                {
                    Console.WriteLine("Container ID cannot be empty. Exiting.");
                    return;
                }
            }

            var config = new
            {
                ContainerId = containerId,
                Environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? environment,
                OutputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? outputDir,
                TestDuration = int.TryParse(Environment.GetEnvironmentVariable("TEST_DURATION"), out int td) ? td : 30000
            };

            if (!Directory.Exists(config.OutputDir))
                Directory.CreateDirectory(config.OutputDir);

            Console.WriteLine($"Starting performance measurement in {config.Environment} environment");
            Console.WriteLine($"Test duration: {config.TestDuration / 1000} seconds");
            
            if (environment == "docker" && !string.IsNullOrEmpty(config.ContainerId))
            {
                Console.WriteLine($"Monitoring Docker container: {config.ContainerId}");
                await MeasureDockerPerformance(config);
            }
            else
            {
                Console.WriteLine("Measuring system performance");
                await MeasureSystemPerformance(config);
            }
        }
        
        private static async Task MeasureDockerPerformance(dynamic config)
        {
            var dockerMonitor = new DockerMonitor();
            var metrics = new List<DockerMetrics>();
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            Console.WriteLine("Collecting Docker stats...");
            
            // Collect metrics at regular intervals
            for (int i = 0; i < config.TestDuration / 1000; i++)
            {
                var dockerMetrics = dockerMonitor.GetContainerStats(config.ContainerId);
                metrics.Add(dockerMetrics);
                
                Console.WriteLine($"[{i+1}/{config.TestDuration/1000}] CPU: {dockerMetrics.CpuUsage}%, Memory: {dockerMetrics.MemoryUsage}%");
                
                await Task.Delay(1000);
            }
            
            // Calculate averages
            var avgCpu = metrics.Average(m => m.CpuUsage);
            var avgMemory = metrics.Average(m => m.MemoryUsage);
            var minCpu = metrics.Min(m => m.CpuUsage);
            var maxCpu = metrics.Max(m => m.CpuUsage);
            var minMemory = metrics.Min(m => m.MemoryUsage);
            var maxMemory = metrics.Max(m => m.MemoryUsage);
            
            // Get additional system metrics
            var systemMonitor = new Models.GeneralPerformanceMeasurement.Monitors.SystemMonitor();
            var systemMetrics = systemMonitor.GetMetrics();
            
            // Create result object
            var result = new PerformanceResult
            {
                TestEnvironment = config.Environment,
                Timestamp = DateTime.UtcNow,
                TestDuration = config.TestDuration,
                Metrics = metrics.Select(m => new Metric 
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime,
                    ScriptDuration = 0, // Not applicable for Docker
                    JSHeapUsedSize = 0  // Not applicable for Docker
                }).ToList(),
                FpsStats = new FpsStats // Not applicable for Docker, but keeping structure
                {
                    Values = new List<double>(),
                    Average = 0,
                    Min = 0,
                    Max = 0
                },
                SystemMetrics = new SystemResourceMetrics
                {
                    CpuUsage = avgCpu,
                    MemoryUsage = avgMemory
                }
            };
            
            // Add docker-specific properties using dynamic
            var resultObj = JObject.FromObject(result);
            resultObj["DockerMetrics"] = JObject.FromObject(new {
                AverageCpu = avgCpu,
                AverageMemory = avgMemory,
                MinCpu = minCpu,
                MaxCpu = maxCpu,
                MinMemory = minMemory,
                MaxMemory = maxMemory,
                ContainerId = config.ContainerId
            });
            
            // Save result
            var fileName = $"{config.Environment}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
            var resultJson = resultObj.ToString(Formatting.Indented);
            
            File.WriteAllText(Path.Combine(config.OutputDir, fileName), resultJson);
            
            if (config.OutputDir != "./docker-actions" && config.Environment == "docker") {
                File.WriteAllText(Path.Combine("./docker-actions", fileName), resultJson);
            }
            
            Console.WriteLine($"Docker performance data saved to {Path.Combine(config.OutputDir, fileName)}");
        }
        
        private static async Task MeasureSystemPerformance(dynamic config)
        {
            var systemMonitor = new Models.GeneralPerformanceMeasurement.Monitors.SystemMonitor();
            var metrics = new List<SystemMetrics>();
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            Console.WriteLine("Collecting system stats...");
            
            // Collect metrics at regular intervals
            for (int i = 0; i < config.TestDuration / 1000; i++)
            {
                var sysMetrics = systemMonitor.GetMetrics();
                metrics.Add(sysMetrics);
                
                Console.WriteLine($"[{i+1}/{config.TestDuration/1000}] CPU: {sysMetrics.CpuUsage}%, Memory: {sysMetrics.MemoryUsage}%");
                
                await Task.Delay(1000);
            }
            
            // Calculate averages
            var avgCpu = metrics.Average(m => m.CpuUsage);
            var avgMemory = metrics.Average(m => m.MemoryUsage);
            
            // Create result object
            var result = new PerformanceResult
            {
                TestEnvironment = config.Environment,
                Timestamp = DateTime.UtcNow,
                TestDuration = config.TestDuration,
                Metrics = metrics.Select((m, i) => new Metric 
                {
                    Timestamp = i * 1000,
                    ScriptDuration = 0, // Not applicable for system metrics
                    JSHeapUsedSize = 0  // Not applicable for system metrics
                }).ToList(),
                FpsStats = new FpsStats // Not applicable for system metrics, but keeping structure
                {
                    Values = new List<double>(),
                    Average = 0,
                    Min = 0,
                    Max = 0
                },
                SystemMetrics = new SystemResourceMetrics
                {
                    CpuUsage = avgCpu,
                    MemoryUsage = avgMemory
                }
            };
            
            // Save result
            var fileName = $"{config.Environment}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            
            File.WriteAllText(Path.Combine(config.OutputDir, fileName), resultJson);
            
            if (config.OutputDir != "./vm-actions" && config.Environment == "vm") {
                File.WriteAllText(Path.Combine("./vm-actions", fileName), resultJson);
            }
            
            Console.WriteLine($"System performance data saved to {Path.Combine(config.OutputDir, fileName)}");
        }

        private static void CreateInitialJsonIfNeeded(string directory)
        {
            if (!Directory.GetFiles(directory, "*.json").Any())
            {
                var initialData = new
                {
                    TestEnvironment = Path.GetFileName(directory).Replace("-actions", ""),
                    Timestamp = DateTime.UtcNow,
                    IsInitialFile = true,
                    Note = "This is an automatically generated placeholder file"
                };
                
                File.WriteAllText(
                    Path.Combine(directory, $"initial-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json"),
                    JsonConvert.SerializeObject(initialData, Formatting.Indented)
                );
            }
        }
    }
}