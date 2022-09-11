using DockerSC2Runner;
using System.Text.Json;

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
