using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DockerSC2Runner
{
    public class RunnerInstance
    {
        private string folder;
        private int runnerIndex;
        private RunnerConfig cfg;
        private BotConfig bot1;
        private BotConfig bot2;
        private Random random;

        public List<GameSummary> Results { get; private set; } = new List<GameSummary>();

        public bool IsRunning { get; private set; }

        public RunnerInstance(string folder, int index, RunnerConfig config, BotConfig b1, BotConfig b2)
        {
            random = new Random((int)(DateTime.Now.Ticks % 10000000) + index);
            this.folder = folder;
            this.runnerIndex = index;
            this.cfg = config;
            this.bot1 = b1;
            this.bot2 = b2;
        }

        public void RunGame(int gameId)
        {
            IsRunning = true;

            _ = RunGameAsync(gameId);
        }

        private void PrepareMatchesFile(string map)
        {
            var sb = new StringBuilder(500);
            sb.AppendLine("# Bot1 name, Bot1 race, Bot1 type, Bot2 name, Bot2 race, Bot2 type, Map");
            sb.AppendLine($"{cfg.Bot1Name},{bot1.Race[0]},{bot1.Type},{cfg.Bot2Name},{bot2.Race[0]},{bot2.Type},{map}");

            File.WriteAllText(Path.Combine(folder, "matches"), sb.ToString());
        }

        private async Task RunGameAsync(int gameId)
        {
            var maps = cfg.Maps.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var map = maps[random.Next(maps.Count())];

            PrepareMatchesFile(map);

            Console.WriteLine($"Starting game: {gameId + 1}/{cfg.MatchCount}, runner index {runnerIndex}, {cfg.Bot1Name} ({bot1.Race}) vs {cfg.Bot2Name} ({bot2.Race}) on {map}");

            DateTime start = DateTime.Now;

            // Run
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = folder;
                p.StartInfo.Arguments = "/C docker compose up";
                p.Start();
                var output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();

                Results.Add(new GameSummary(output, map, DateTime.Now - start));

                Console.WriteLine($"Game finished: {gameId + 1}/{cfg.MatchCount}, runner index {runnerIndex}, {cfg.Bot1Name} ({bot1.Race}) vs {cfg.Bot2Name} ({bot2.Race}) on {map}{Environment.NewLine}  result {Results.Last().Result}, winner {Results.Last().Winner} in {Results.Last().GameLength:hh\\:mm\\:ss}, realtime {DateTime.Now - start:hh\\:mm\\:ss}");

                SaveReplaysAndLogs(gameId, folder);
            }
            IsRunning = false;
        }

        private void SaveReplaysAndLogs(int gameId, string folder)
        {
            try
            {
                var replayFolder = Path.Combine(folder, "replays");
                var logFolder = Path.Combine(folder, @"logs");

                var archiveFolder = Path.Combine(cfg.ResultsFolder, cfg.Start.ToString("yyyy-MM-dd-hh-mm-ss"), $"game_{gameId}");

                // Logs
                Directory.CreateDirectory(Path.Combine(cfg.ResultsFolder, cfg.Start.ToString("yyyy-MM-dd-hh-mm-ss")));
                logFolder = Directory.EnumerateDirectories(logFolder).First();
                Directory.Move(logFolder, archiveFolder);

                // Replays
                var replayFile = Directory.EnumerateFiles(replayFolder, "*.SC2Replay").First();
                File.Move(replayFile, Path.Combine(archiveFolder, $"game{gameId}.SC2Replay"));

                // Game result
                File.WriteAllText(Path.Combine(archiveFolder, "result.txt"), Results.Last().ToString());
            }
            catch
            {

            }
        }
    }
}
