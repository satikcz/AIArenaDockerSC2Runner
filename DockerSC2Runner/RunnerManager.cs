using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace DockerSC2Runner
{
    public class RunnerManager
    {
        private readonly RunnerConfig cfg;
        private readonly BotConfig bot1;
        private readonly BotConfig bot2;

        private readonly RunnerInstance[] runnerInstances;

        public RunnerManager(RunnerConfig cfg)
        {
            bot1 = BotConfig.Parse(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot1Name, RunnerConfig.BotJsonFile));
            bot2 = BotConfig.Parse(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot2Name, RunnerConfig.BotJsonFile));
            this.cfg = cfg;

            runnerInstances = new RunnerInstance[cfg.RunnerCount];
            Prepare();
        }

        public void Prepare()
        {
            for (int i = 0; i < cfg.RunnerCount; i++)
            {
                _ = PrepareRunnerAsync(i + 1);
            }

            while (runnerInstances.Any(r => r == null))
            {
                Thread.Sleep(100);
                Thread.Yield();
            }
        }

        public void Run()
        {
            DateTime start = DateTime.Now;

            for (int iGame = 0; iGame < cfg.MatchCount; iGame++)
            {
                RunnerInstance? freeRunner = null;
                while (freeRunner == null)
                {
                    freeRunner = runnerInstances.FirstOrDefault(r => !r.IsRunning);
                    Thread.Sleep(100);
                    Thread.Yield();
                }

                freeRunner.RunGame(iGame);
            }

            while (runnerInstances.Any(r => r.IsRunning))
            {
                Thread.Sleep(100);
                Thread.Yield();
            }

            var results = runnerInstances.SelectMany(x => x.Results);

            PrintResults(results);

            Console.WriteLine();
            Console.WriteLine($"Finished in {(DateTime.Now - start).ToString(@"hh\:mm\:ss")}");
        }

        private void PrintResults(IEnumerable<GameSummary> results)
        {
            var sb = new StringBuilder(10000);

            sb.AppendLine(new String('=', 20));
            int index = 1;
            foreach (GameSummary summary in results)
            {
                sb.AppendLine($"{(index++).ToString().PadLeft(3)}/{results.Count()} {summary}");
            }
            sb.AppendLine(new String('=', 20));
            sb.AppendLine($"Average game length: {results.Average(x => x.Frames)} frames (Min {results.Min(x => x.Frames)} frames Max {results.Max(x => x.Frames)} frames)");
            sb.AppendLine($"Average game length: {Helpers.ConvertGameLength((int)results.Average(x => x.Frames)):hh\\:mm\\:ss} (Min {Helpers.ConvertGameLength((int)results.Min(x => x.Frames)):hh\\:mm\\:ss} Max {Helpers.ConvertGameLength((int)results.Max(x => x.Frames)):hh\\:mm\\:ss})");
            sb.AppendLine($"Step {cfg.Bot1Name} {results.Average(x => x.Bot1AvgStepTime):F2}ms (Min {results.Min(x => x.Bot1AvgStepTime):F2}ms Max {results.Max(x => x.Bot1AvgStepTime):F2}ms)");
            sb.AppendLine($"Step {cfg.Bot2Name} {results.Average(x => x.Bot2AvgStepTime):F2}ms (Min {results.Min(x => x.Bot2AvgStepTime):F2}ms Max {results.Max(x => x.Bot2AvgStepTime):F2}ms)");

            sb.AppendLine("Result summary:");
            foreach (GameResult result in Enum.GetValues(typeof(GameResult)))
            {
                var count = results.Count(x => x.Result == result);
                if (count > 0)
                {
                    sb.AppendLine($"{count} {result}");
                }
            }

            Console.WriteLine(sb.ToString());
            File.WriteAllText(Path.Combine(cfg.ResultsFolder, cfg.Start.ToString("yyyy-MM-dd-hh-mm-ss"), "results.txt"), sb.ToString());
        }

        private async Task PrepareRunnerAsync(int i)
        {
            var folder = Path.Combine(cfg.RunnersFolder, $"SC2Runner{i}");

            try
            {
                // Stop any leftover
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = folder;
                p.StartInfo.Arguments = $"/C docker stop sc2runner{i}-arena-client-1";
                p.Start();
                var output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();
                Thread.Yield();
                Thread.Sleep(1000);
            }
            catch (Win32Exception)
            {

            }

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            CopyFilesRecursively("ArenaClient", folder);
            CopyFilesRecursively(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot1Name), Path.Combine(folder, RunnerConfig.BotsFolder, cfg.Bot1Name));
            CopyFilesRecursively(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot2Name), Path.Combine(folder, RunnerConfig.BotsFolder, cfg.Bot2Name));

            runnerInstances[i-1] = new RunnerInstance(folder, i, cfg, bot1, bot2);
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
