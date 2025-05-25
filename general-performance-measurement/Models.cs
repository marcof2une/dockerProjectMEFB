using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace GeneralPerformanceMeasurement.Models
{
    public class PerformanceResult
    {
        public string TestEnvironment { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Url { get; set; } = string.Empty;
        public int TestDuration { get; set; }
        public List<Metric>? Metrics { get; set; }
        public FpsStats? FpsStats { get; set; }
        public SystemResourceMetrics? SystemMetrics { get; set; }
    }

    public class Metric
    {
        public long Timestamp { get; set; }
        public double ScriptDuration { get; set; }
        public long JSHeapUsedSize { get; set; }
    }

    public class FpsStats
    {
        public List<double>? Values { get; set; }
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class SystemResourceMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }

        public static implicit operator SystemResourceMetrics(SystemMetrics v)
        {
            throw new NotImplementedException();
        }
    }

    // Analysis-related models
    public class AnalysisResult
    {
        public EnvironmentMetrics? Docker { get; set; }
        public EnvironmentMetrics? VM { get; set; }
        public Comparison? Comparison { get; set; }
    }

    public class EnvironmentMetrics
    {
        public double AverageFps { get; set; }
        public double AverageCpu { get; set; }
        public double AverageMemory { get; set; }
    }

    public class Comparison
    {
        public double FpsDifference { get; set; }
        public double CpuDifference { get; set; }
        public double MemoryDifference { get; set; }
    }

    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }

    namespace GeneralPerformanceMeasurement.Monitors
    {
        public class SystemMonitor
        {
            // P/Invoke for Windows memory status
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private struct MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

            public SystemMetrics GetMetrics()
            {
                return Environment.OSVersion.Platform switch
                {
                    PlatformID.Unix => ConvertToSystemMetrics(GetUnixMetrics()),
                    PlatformID.Win32NT => ConvertToSystemMetrics(GetWindowsMetrics()),
                    _ => new SystemMetrics()
                };
            }

            private SystemResourceMetrics GetWindowsMetrics()
            {
#if WINDOWS || NET6_0_WINDOWS
                            try
                            {
                                using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                                cpuCounter.NextValue();
                                Thread.Sleep(1000);    
                                double cpuUsage = cpuCounter.NextValue();
                                double memoryUsage = 0;
            
                                try
                                {
                                    using var process = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "wmic",
                                            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                                            RedirectStandardOutput = true,
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        }
                                    };
                                    
                                    process.Start();
                                    string output = process.StandardOutput.ReadToEnd();
                                    process.WaitForExit();
                                    
                                    // Extract values from WMIC output format
                                    var freeMemMatch = System.Text.RegularExpressions.Regex.Match(output, @"FreePhysicalMemory=(\d+)");
                                    var totalMemMatch = System.Text.RegularExpressions.Regex.Match(output, @"TotalVisibleMemorySize=(\d+)");
                                    
                                    if (freeMemMatch.Success && totalMemMatch.Success)
                                    {
                                        long freeMemKB = long.Parse(freeMemMatch.Groups[1].Value);
                                        long totalMemKB = long.Parse(totalMemMatch.Groups[1].Value);
                                        
                                        // Calculate memory usage percentage
                                        memoryUsage = Math.Round(100.0 * (1 - (double)freeMemKB / totalMemKB), 2);
                                    }
                                }
                                catch (Exception memEx)
                                {
                                    // Fallback using GlobalMemoryStatusEx API
                                    try
                                    {
                                        MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                                        memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                                        
                                        if (GlobalMemoryStatusEx(ref memStatus))
                                        {
                                            memoryUsage = memStatus.dwMemoryLoad; // Already a percentage
                                        }
                                    }
                                    catch (Exception fallbackEx)
                                    {
                                        Console.WriteLine($"Error getting memory info: {fallbackEx.Message}");
                                        memoryUsage = 0;
                                    }
                                }
            
                                return new SystemResourceMetrics
                                {
                                    CpuUsage = Math.Round(cpuUsage, 2),
                                    MemoryUsage = memoryUsage
                                };
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error collecting Windows metrics: {ex.Message}");
                                return new SystemResourceMetrics { CpuUsage = 0, MemoryUsage = 0 };
                            }
#else
                // Not supported on non-Windows platforms
                return new SystemResourceMetrics { CpuUsage = 0, MemoryUsage = 0 };
#endif
            }

            private SystemMetrics ConvertToSystemMetrics(SystemResourceMetrics resourceMetrics)
            {
                return new SystemMetrics
                {
                    CpuUsage = resourceMetrics.CpuUsage,
                    MemoryUsage = resourceMetrics.MemoryUsage
                };
            }

            private SystemResourceMetrics GetUnixMetrics()
            {
                try
                {
                    // Get CPU usage using /proc/stat
                    double cpuUsage = GetLinuxCpuUsage();

                    // Get memory usage using free command
                    double memoryUsage = GetLinuxMemoryUsage();

                    return new SystemResourceMetrics
                    {
                        CpuUsage = cpuUsage,
                        MemoryUsage = memoryUsage
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting Unix system metrics: {ex.Message}");
                    return new SystemResourceMetrics { CpuUsage = 0, MemoryUsage = 0 };
                }
            }

            private double GetLinuxCpuUsage()
            {
                // Get two snapshots of /proc/stat to calculate CPU usage percentage
                string stat1 = File.ReadAllText("/proc/stat").Split('\n')[0];
                long idle1 = GetIdleTime(stat1);
                long total1 = GetTotalTime(stat1);

                // Wait a short interval for measurement
                Thread.Sleep(500);

                string stat2 = File.ReadAllText("/proc/stat").Split('\n')[0];
                long idle2 = GetIdleTime(stat2);
                long total2 = GetTotalTime(stat2);

                // Calculate the CPU usage percentage
                long idleDelta = idle2 - idle1;
                long totalDelta = total2 - total1;

                return Math.Round(100.0 * (1.0 - (double)idleDelta / totalDelta), 2);
            }

            private long GetIdleTime(string stat)
            {
                // CPU stats format: cpu user nice system idle iowait irq softirq steal guest guest_nice
                string[] parts = stat.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return long.Parse(parts[4]); // idle time is the 5th value
            }

            private long GetTotalTime(string stat)
            {
                string[] parts = stat.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Skip(1).Select(long.Parse).Sum(); // Sum of all CPU time values
            }

            private double GetLinuxMemoryUsage()
            {
                try
                {
                    // Use the 'free' command to get memory information
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "free",
                            Arguments = "-m", // Output in MB
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Parse the output to get memory usage
                    string[] lines = output.Split('\n');
                    if (lines.Length >= 2)
                    {
                        string memLine = lines[1];
                        string[] memValues = memLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (memValues.Length >= 3)
                        {
                            long total = long.Parse(memValues[1]);
                            long used = long.Parse(memValues[2]);

                            if (total > 0)
                            {
                                return Math.Round(100.0 * used / total, 2);
                            }
                        }
                    }

                    // Alternative method using /proc/meminfo if 'free' command fails
                    return GetMemoryUsageFromProcMemInfo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting memory usage: {ex.Message}");
                    return GetMemoryUsageFromProcMemInfo(); // Try alternative method
                }
            }

            private double GetMemoryUsageFromProcMemInfo()
            {
                try
                {
                    string meminfo = File.ReadAllText("/proc/meminfo");
                    string[] lines = meminfo.Split('\n');

                    long totalMem = 0;
                    long freeMem = 0;
                    long buffers = 0;
                    long cached = 0;

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("MemTotal:"))
                        {
                            totalMem = ParseMemValue(line);
                        }
                        else if (line.StartsWith("MemFree:"))
                        {
                            freeMem = ParseMemValue(line);
                        }
                        else if (line.StartsWith("Buffers:"))
                        {
                            buffers = ParseMemValue(line);
                        }
                        else if (line.StartsWith("Cached:"))
                        {
                            cached = ParseMemValue(line);
                        }
                    }

                    if (totalMem > 0)
                    {
                        // Calculate used memory percentage (excluding buffers/cache)
                        long usedMem = totalMem - freeMem - buffers - cached;
                        return Math.Round(100.0 * usedMem / totalMem, 2);
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading /proc/meminfo: {ex.Message}");
                    return 0;
                }
            }

            private long ParseMemValue(string line)
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return long.Parse(parts[1]);
                }
                return 0;
            }
        }
    }

}


