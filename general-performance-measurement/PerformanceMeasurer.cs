using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

public class PerformanceResult
{
    public string Environment { get; set; }
    public DateTime Timestamp { get; set; }
    public string Url { get; set; }
    public int TestDuration { get; set; }
    public List<Metric> Metrics { get; set; }
    public FpsStats FpsStats { get; set; }
    public SystemMetrics SystemMetrics { get; set; }
}

public class Metric
{
    public long Timestamp { get; set; }
    public double ScriptDuration { get; set; }
    public long JSHeapUsedSize { get; set; }
}

public class FpsStats
{
    public List<double> Values { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public class SystemMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
}

class PerformanceMeasurer
{
    public static async Task Main(string[] args)
    {
        var config = new {
            Url = Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:3000",
            Environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "local",
            OutputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? "./performance-data",
            TestDuration = int.TryParse(Environment.GetEnvironmentVariable("TEST_DURATION"), out int td) ? td : 30000
        };

        Directory.CreateDirectory(config.OutputDir);

        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--disable-gpu", "--no-sandbox", "--disable-setuid-sandbox" }
        };

        using (var browser = await Puppeteer.LaunchAsync(launchOptions))
        {
            var page = await browser.NewPageAsync();

            // FPS Measurement
            await page.EvaluateOnNewDocumentAsync(@"
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

            await page.GoToAsync(config.Url, WaitUntilNavigation.Networkidle0);

            var metrics = new List<Metric>();
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var systemMonitor = new SystemMonitor();

            // Measurement loop
            for (int i = 0; i < config.TestDuration / 1000; i++)
            {
                var pageMetrics = await page.MetricsAsync();
                metrics.Add(new Metric {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime,
                    ScriptDuration = pageMetrics.ContainsKey("ScriptDuration") ? 
                        Convert.ToDouble(pageMetrics["ScriptDuration"]) : 0,
                    JSHeapUsedSize = pageMetrics.ContainsKey("JSHeapUsedSize") ? 
                        Convert.ToInt64(pageMetrics["JSHeapUsedSize"]) : 0
                });

                await Task.Delay(1000);
            }

            // Collect system metrics
            var systemMetrics = systemMonitor.GetMetrics();

            // Get FPS data
            var fpsData = await page.EvaluateFunctionAsync<dynamic>("() => ({ values: window.fpsValues })");
            
            var result = new PerformanceResult {
                Environment = config.Environment,
                Timestamp = DateTime.UtcNow,
                Url = config.Url,
                TestDuration = config.TestDuration,
                Metrics = metrics,
                FpsStats = new FpsStats {
                    Values = ((IEnumerable<object>)fpsData.values).Select(Convert.ToDouble).ToList(),
                    Average = ((IEnumerable<object>)fpsData.values).Select(Convert.ToDouble).Average(),
                    Min = ((IEnumerable<object>)fpsData.values).Select(Convert.ToDouble).Min(),
                    Max = ((IEnumerable<object>)fpsData.values).Select(Convert.ToDouble).Max()
                },
                SystemMetrics = systemMetrics
            };

            var fileName = $"{config.Environment}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.json";
            File.WriteAllText(Path.Combine(config.OutputDir, fileName), 
                JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}

public class SystemMonitor
{
    public SystemMetrics GetMetrics()
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return GetUnixMetrics();
        }
        return GetWindowsMetrics();
    }

    private SystemMetrics GetUnixMetrics()
    {
        var cpu = ExecuteShellCommand("top -bn1 | grep 'Cpu(s)' | awk '{print $2 + $4}'");
        var mem = ExecuteShellCommand("free -m | awk '/Mem:/ {print $3/$2 * 100.0}'");
        
        return new SystemMetrics {
            CpuUsage = double.TryParse(cpu, out var c) ? c : 0,
            MemoryUsage = double.TryParse(mem, out var m) ? m : 0
        };
    }

    private SystemMetrics GetWindowsMetrics()
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        var memCounter = new PerformanceCounter("Memory", "Available MBytes");
        
        cpuCounter.NextValue();
        Thread.Sleep(1000);
        
        return new SystemMetrics {
            CpuUsage = cpuCounter.NextValue(),
            MemoryUsage = (1 - (memCounter.NextValue() / 
                new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory * 1e-6)) * 100
        };
    }

    private string ExecuteShellCommand(string command)
    {
        try
        {
            var psi = new ProcessStartInfo("bash", "-c \"" + command + "\"") {
                RedirectStandardOutput = true
            };
            return Process.Start(psi).StandardOutput.ReadToEnd().Trim();
        }
        catch { return "0"; }
    }
}