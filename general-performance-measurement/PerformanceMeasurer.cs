using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using GeneralPerformanceMeasurement.Models;
using GeneralPerformanceMeasurement;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace GeneralPerformanceMeasurement
{
    public class PerformanceMeasurer
    {
        public static async Task Main(string[] args)
        {
            if (!Directory.Exists("./docker-actions"))
                Directory.CreateDirectory("./docker-actions");
            
            if (!Directory.Exists("./vm-actions"))
                Directory.CreateDirectory("./vm-actions");

            CreateInitialJsonIfNeeded("./docker-actions");
            CreateInitialJsonIfNeeded("./vm-actions");

            // Fix: Use args[1] instead of args[0] to get the environment
            var environment = args.Length > 1 ? args[1] : "docker";
            var outputDir = environment == "docker" ? "./docker-actions" : "./vm-actions";
            
            var config = new
            {
                Url = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:3000",
                Environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? environment,
                OutputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? outputDir,
                TestDuration = int.TryParse(Environment.GetEnvironmentVariable("TEST_DURATION"), out int td) ? td : 30000
            };

            if (!Directory.Exists(config.OutputDir))
                Directory.CreateDirectory(config.OutputDir);

            var options = new FirefoxOptions();
            options.AddArgument("--headless");

            using var driver = new FirefoxDriver(options);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(120);

            driver.Navigate().GoToUrl(config.Url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(d => d != null && 
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.Equals("complete") == true);

            // Inject FPS tracking code
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                window.fpsValues = [];
                let lastFrameTimestamp = performance.now();
                function calculateFPS() {
                    const now = performance.now();
                    const fps = 1000 / (now - lastFrameTimestamp);
                    if(fps < 120) window.fpsValues.push(fps);
                    lastFrameTimestamp = now;
                    requestAnimationFrame(calculateFPS);
                }
                requestAnimationFrame(calculateFPS);
            ");

            var metrics = new List<Metric>();
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            for (int i = 0; i < config.TestDuration / 1000; i++)
            {
                var jsHeapSize = Convert.ToInt64(((IJavaScriptExecutor)driver).ExecuteScript(
                    "return window.performance.memory ? window.performance.memory.usedJSHeapSize : 0"));

                var scriptExecutionTime = Convert.ToDouble(((IJavaScriptExecutor)driver).ExecuteScript(@"
                    const entries = performance.getEntriesByType('measure');
                    let total = 0;
                    for (const entry of entries) {
                        total += entry.duration;
                    }
                    performance.clearMeasures();
                    return total;
                "));
                
                metrics.Add(new Metric 
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime,
                    ScriptDuration = scriptExecutionTime,
                    JSHeapUsedSize = jsHeapSize
                });
                
                await Task.Delay(1000);
            }

            var systemMonitor = new Models.GeneralPerformanceMeasurement.Monitors.SystemMonitor();

            var systemMetrics = systemMonitor.GetMetrics();

            var fpsValuesArray = ((IJavaScriptExecutor)driver).ExecuteScript("return window.fpsValues") 
                as IReadOnlyCollection<object>;
                
            List<double> fpsValues = new List<double>();
            if (fpsValuesArray != null)
            {
                fpsValues = fpsValuesArray.Select(v => Convert.ToDouble(v)).ToList();
            }

            var result = new PerformanceResult
            {
                TestEnvironment = config.Environment,
                Timestamp = DateTime.UtcNow,
                Url = config.Url,
                TestDuration = config.TestDuration,
                Metrics = metrics,
                FpsStats = fpsValues.Any() 
                    ? new FpsStats
                    {
                        Values = fpsValues,
                        Average = fpsValues.Average(),
                        Min = fpsValues.Min(),
                        Max = fpsValues.Max()
                    } 
                    : new FpsStats
                    {
                        Values = new List<double>(),
                        Average = 0,
                        Min = 0,
                        Max = 0
                    },
                SystemMetrics = systemMetrics
            };

            var fileName = $"{config.Environment}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            
            File.WriteAllText(Path.Combine(config.OutputDir, fileName), resultJson);
            
            if (config.OutputDir != "./docker-actions" && config.Environment == "docker") {
                File.WriteAllText(Path.Combine("./docker-actions", fileName), resultJson);
            }
            if (config.OutputDir != "./vm-actions" && config.Environment == "vm") {
                File.WriteAllText(Path.Combine("./vm-actions", fileName), resultJson);
            }
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