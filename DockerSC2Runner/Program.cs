global using DockerSC2Runner;
global using System.Text.Json;
global using DockerSC2Runner.Results;
global using System.Diagnostics;
global using System.Text;
global using System.ComponentModel;

var runnerCfg = JsonSerializer.Deserialize<RunnerConfig>(File.ReadAllText("config.json"));

if (runnerCfg == null)
{
    Console.WriteLine("Unable to parse 'config.json'. Quitting.");
    return;
}

Console.WriteLine("Initializing");
Console.WriteLine($"{runnerCfg.Bot1Name} vs {runnerCfg.Bot2Name}");
Console.WriteLine($"Maps: {string.Join(' ', runnerCfg.Maps)}");
Console.WriteLine($"{runnerCfg.MatchCount} matches on {runnerCfg.RunnerCount} runners");
Console.WriteLine($"===========================");
Console.WriteLine($"");

var runnerManager = new RunnerManager(runnerCfg);
runnerManager.Run();

Console.Beep();

if (runnerCfg.StopOnFinish)
{
    Console.ReadLine();
}
