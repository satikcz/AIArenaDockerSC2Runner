using System.Globalization;
using System.Text.RegularExpressions;

namespace DockerSC2Runner
{
    public class GameSummary
    {
        public GameResult Result { get; private set; } = GameResult.Unknown;
        public string Winner { get; private set; } = string.Empty;
        public int GameTime { get; private set; }
        public float Bot1AvgStepTime { get; private set; }
        public float Bot2AvgStepTime { get; private set; }
        public string Map { get; private set; }

        public GameSummary(string result, string map)
        {
            try
            {
                Result = Enum.Parse<GameResult>(Regex.Matches(result, @"\|\s*Result\s*=\s*(\w+)\s*\n")[0].Groups[1].Value);
                Winner = Regex.Matches(result, @"\|\s*Winner\s*=\s*(.+)\s*\n")[0].Groups[1].Value;
                GameTime = int.Parse(Regex.Matches(result, @"\|\s*GameTime\s*=\s*(\d+)\s*\n")[0].Groups[1].Value);
                Bot1AvgStepTime = float.Parse(Regex.Matches(result, @"\|\s*Bot1AvgStepTime\s*=\s*(\d*\.\d*)\s*\n")[0].Groups[1].Value, CultureInfo.InvariantCulture);
                Bot2AvgStepTime = float.Parse(Regex.Matches(result, @"\|\s*Bot2AvgStepTime\s*=\s*(\d*\.\d*)\s*\n")[0].Groups[1].Value, CultureInfo.InvariantCulture);
                Map = map;
            }
            catch
            {

            }
            Map = map;
        }

        public override string ToString()
        {
            return $"{Result.ToString().PadRight(20)} winner {Winner} on {Map} in {GameTime} frames ({TimeSpan.FromMilliseconds(GameTime * 22.4f)}), step {Bot1AvgStepTime:f2}ms and {Bot2AvgStepTime:F2}ms";
        }
    }
}
