﻿namespace DockerSC2Runner
{
    public class RunnerManager
    {
        private readonly RunnerConfig cfg;

        private readonly RunnerInstance[] runnerInstances;

        private readonly List<BotConfig> botConfigs = new List<BotConfig>();

        public RunnerManager(RunnerConfig cfg)
        {
            foreach (var dir in Directory.EnumerateDirectories(RunnerConfig.BotsFolder))
            {
                var bot = BotConfig.Parse(Path.Combine(dir, RunnerConfig.BotJsonFile), dir.Split('\\').Last());

                if (bot is not null)
                {
                    botConfigs.Add(bot);
                }
            }

            this.cfg = cfg;

            runnerInstances = new RunnerInstance[cfg.RunnerCount];
            Prepare();

            Console.WriteLine($"Runners ready");
            Console.WriteLine($"=============");
            Console.WriteLine($"");
        }

        private void CopyMapsToBootstrapDir()
        {
            Console.WriteLine("Copying maps");
            foreach (var file in Directory.EnumerateFiles($"{Directory.GetCurrentDirectory()}\\Maps", "*.sc2map"))
            {
                File.Copy(file, $"{Directory.GetCurrentDirectory()}\\{BootstrapGitClonner.BootstrapDir}\\Maps\\{Path.GetFileName(file)}", true);
                Console.WriteLine($"  {Path.GetFileNameWithoutExtension(file)}");
            }
        }

        private void CopyBotsToBootstrapDir()
        {
            if (cfg.Bot1Name == "?" || cfg.Bot2Name == "?")
            {
                Console.WriteLine("Copying all bots");
                CopyFilesRecursively($"{Directory.GetCurrentDirectory()}\\Bots", $"{Directory.GetCurrentDirectory()}\\{BootstrapGitClonner.BootstrapDir}\\Bots");
            }
            else
            {
                Console.WriteLine($"Copying {cfg.Bot1Name} and {cfg.Bot2Name}");
                CopyFilesRecursively(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot1Name), Path.Combine($"{Directory.GetCurrentDirectory()}\\{BootstrapGitClonner.BootstrapDir}", RunnerConfig.BotsFolder, cfg.Bot1Name));
                CopyFilesRecursively(Path.Combine(RunnerConfig.BotsFolder, cfg.Bot2Name), Path.Combine($"{Directory.GetCurrentDirectory()}\\{BootstrapGitClonner.BootstrapDir}", RunnerConfig.BotsFolder, cfg.Bot2Name));
            }
        }

        public void Prepare()
        {
            Console.WriteLine("Preparing runner environment");
            BootstrapGitClonner bootstrap = new BootstrapGitClonner();
            bootstrap.CloneBootstrap(Directory.GetCurrentDirectory());

            CopyMapsToBootstrapDir();
            CopyBotsToBootstrapDir();

            Console.WriteLine($"Preparing {cfg.RunnerCount} runners");
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

            for (int iGame = 1; iGame <= cfg.MatchCount; iGame++)
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

            sb.AppendLine(new string('=', 20));
            int index = 1;
            foreach (GameSummary summary in results)
            {
                sb.AppendLine($"{(index++).ToString().PadLeft(3)}/{results.Count()} {summary}");
            }
            sb.AppendLine(new string('=', 20));
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
                if (Directory.Exists(folder))
                {
                    await CLI.RunAsync("docker stop sc2runner{i}-arena-client-1", folder);
                    Thread.Yield();
                    Thread.Sleep(1000);
                }
            }
            catch (Win32Exception)
            {

            }

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            // Copy bootstrap
            CopyFilesRecursively("local-play-bootstrap", folder);

            runnerInstances[i-1] = new RunnerInstance(folder, i, cfg, botConfigs.First(x=>x.Name == cfg.Bot1Name), botConfigs.First(x => x.Name == cfg.Bot2Name));

            Console.WriteLine($"Runner {i} prepared");
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
