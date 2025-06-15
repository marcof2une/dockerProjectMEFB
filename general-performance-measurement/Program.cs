using System;
using System.Threading.Tasks;
using GeneralPerformanceMeasurement;
using GeneralPerformanceMeasurement.Analysis;


if (args.Length == 0)
{
    PrintUsage();
    return;
}

switch (args[0].ToLower())
{
    case "measure":
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Environment (docker or vm) must be specified for measurement.");
            PrintUsage();
            return;
        }
        await PerformanceMeasurer.RunAsync(args);
        break;

    case "analyze":
        PerformanceAnalyzer.RunAnalysis();
        break;

    default:
        Console.WriteLine($"Unknown command: {args[0]}");
        PrintUsage();
        break;
}

void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- measure docker|vm   # Run performance measurement on specified environment");
    Console.WriteLine("  dotnet run -- analyze             # Analyze previously collected performance data");
}
