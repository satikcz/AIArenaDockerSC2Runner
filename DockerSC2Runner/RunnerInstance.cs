namespace DockerSC2Runner
{
    public class RunnerInstance
    {
        private string folder;
        private int runnerIndex;
        private RunnerConfig cfg;
        private Random random;

        public List<GameSummary> Results { get; private set; } = new List<GameSummary>();

        public bool IsRunning { get; private set; }

        public RunnerInstance(string folder, int index, RunnerConfig config)
        {
            random = new Random((int)(DateTime.Now.Ticks % 10000000) + index);
            this.folder = folder;
            this.runnerIndex = index;
            this.cfg = config;
        }

        public void RunGame(int gameId, BotConfig bot1, BotConfig bot2)
        {
            IsRunning = true;

            _ = RunGameAsync(gameId, bot1, bot2);
        }

        private void PrepareMatchesFile(string map, BotConfig bot1, BotConfig bot2)
        {
            var sb = new StringBuilder(500);
            sb.AppendLine("# Bot1 ID, Bot1 name, Bot1 race, Bot1 type, Bot2 ID, Bot2 name, Bot2 race, Bot2 type, Map");
            sb.AppendLine($"1,{bot1.Name},{bot1.Race[0]},{bot1.Type},2,{bot2.Name},{bot2.Race[0]},{bot2.Type},{map}");

            File.WriteAllText(Path.Combine(folder, "matches"), sb.ToString());
        }

        private async Task RunGameAsync(int gameId, BotConfig bot1, BotConfig bot2)
        {
            var maps = cfg.Maps.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var map = maps[random.Next(maps.Count())];

            PrepareMatchesFile(map, bot1, bot2);

            Console.WriteLine($"Starting game: {gameId}/{cfg.MatchCount}, runner index {runnerIndex}, {bot1.Name} ({bot1.Race}) vs {bot2.Name} ({bot2.Race}) on {map}");

            DateTime start = DateTime.Now;

            var res = await CLI.RunAsync($"docker compose -p runner{runnerIndex} up", folder);

            MatchResults? results = null;
            RunIOWithRetries(() =>
            {
                results = JsonSerializer.Deserialize<MatchResults>(File.ReadAllText(Path.Combine(folder, "results.json")));
            }, $"Unable to parse results from {folder}");

            if (results is not null && results.results.Any())
            {
                Results.Add(new GameSummary(results.results.Last(), map, DateTime.Now - start, bot1.Name, bot2.Name));
                Console.WriteLine($"Game finished: {gameId}/{cfg.MatchCount}, runner index {runnerIndex}, {bot1.Name} ({bot1.Race}) vs {bot2.Name} ({bot2.Race}) on {map}{Environment.NewLine}  result {Results.Last().Result}, winner {Results.Last().Winner} in {Results.Last().GameLength:hh\\:mm\\:ss}, realtime {DateTime.Now - start:hh\\:mm\\:ss}");
                SaveReplaysAndLogs(gameId, folder);
            }
            else
            {
                Console.WriteLine($"Game failed: {gameId}/{cfg.MatchCount}, runner index {runnerIndex}, {bot1.Name} ({bot1.Race}) vs {bot2.Name} ({bot2.Race}) on {map}{Environment.NewLine}, {DateTime.Now - start:hh\\:mm\\:ss}");
            }

            IsRunning = false;
        }

        /// <summary>
        /// Runs IO operation that can fail, in that case operation is retried
        /// </summary>
        private void RunIOWithRetries(Action code, string errorMessage, int retries = 10, int sleepTimeMs = 1000)
        {
            for (int i = 0; i<retries; i++)
            {
                try
                {
                    code();
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);

                    if (i>0)
                        Console.WriteLine($"{errorMessage} retry {i}/{retries}");
                }
            }
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

                RunIOWithRetries(() =>
                {
                    logFolder = Directory.EnumerateDirectories(logFolder).First();
                    Directory.Move(logFolder, archiveFolder); // sometimes still in use
                }, $"Could not move '{logFolder}' for game {gameId}");

                // Replays
                RunIOWithRetries(() => 
                {
                    var replayFile = Directory.EnumerateFiles(replayFolder, "*.SC2Replay").First();
                    File.Move(replayFile, Path.Combine(archiveFolder, $"game{gameId}.SC2Replay"));
                }, $"Could not move '{archiveFolder}' for game {gameId}");

                // Game result
                File.WriteAllText(Path.Combine(archiveFolder, "result.txt"), Results.Last().ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   !!! Exception when saving logs or replay: {ex.Message}");
            }
        }
    }
}
