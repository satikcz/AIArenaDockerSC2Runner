namespace DockerSC2Runner
{
    public class GameSummary
    {
        public GameResult Result { get; private set; } = GameResult.Unknown;
        public string Winner { get; private set; } = string.Empty;
        public string Bot1 { get; private set; } = string.Empty;
        public string Bot2 { get; private set; } = string.Empty;
        public int Frames { get; private set; }
        public float Bot1AvgStepTime { get; private set; }
        public float Bot2AvgStepTime { get; private set; }
        public string Map { get; private set; }
        public TimeSpan RealTime { get; private set; }

        public TimeSpan GameLength => Helpers.ConvertGameLength(Frames);

        public GameSummary(MatchResult result, string map, TimeSpan realTime, string player1, string player2)
        {
            try
            {
                Result = Enum.Parse<GameResult>(result.type);
                Frames = result.game_steps;
                Bot1AvgStepTime = 1000f * result.bot1_avg_step_time;
                Bot2AvgStepTime = 1000f * result.bot2_avg_step_time;
                Map = map;
                RealTime = realTime;
                Bot1 = player1;
                Bot2 = player2;

                if (Result == GameResult.Error || Result == GameResult.InitializationError || Result == GameResult.Tie || Result == GameResult.Unknown)
                {
                    Winner = "Draw";
                }
                else
                {
                    Winner = Result == GameResult.Player1Win || Result == GameResult.Player2Crash ? player1 : player2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   !!! Exception when parsing game result: {ex.Message}");
            }
            Map = map;
        }

        private string FormatWinner(string botName)
        {
            if (Winner != botName)
                return botName;
            else
                return $"**{botName}**";
        }

        public override string ToString()
        {
            return $"{Result.ToString().PadRight(20)} {FormatWinner(Bot1)} vs {FormatWinner(Bot2)} on {Map} in {Frames} frames ({GameLength:hh\\:mm\\:ss}), step {Bot1AvgStepTime:f2}ms and {Bot2AvgStepTime:F2}ms";
        }
    }
}
