global using DockerSC2Runner;
global using System.Text.Json;
global using DockerSC2Runner.Results;
global using System;
global using System.Diagnostics;
global using System.Reflection;
global using System.Text;
global using System.ComponentModel;

var runnerCfg = JsonSerializer.Deserialize<RunnerConfig>(File.ReadAllText("config.json"));

if (runnerCfg == null)
{
    Console.WriteLine("Unable to parse 'config.json'. Quitting.");
    return;
}

var runner = new RunnerManager(runnerCfg);
runner.Run();

Console.Beep();

if (runnerCfg.StopOnFinish)
{
    Console.ReadLine();
}
