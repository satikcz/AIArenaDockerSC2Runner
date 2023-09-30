using System.Text.RegularExpressions;

namespace DockerSC2Runner
{
    public class BotConfig
    {
        public string Race { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string RootPath { get; set; }
        public string FileName { get; set; }

        public BotConfig(string race, string type, string rootPath, string fileName, string dirName)
        {
            Race = race;
            Type = type;
            Name = dirName;
            FileName = fileName;
            RootPath = rootPath;
        }

        public static BotConfig? Parse(string file, string dirName)
        {
            if (!File.Exists(file))
                return null;

            var json = File.ReadAllText(file) ?? string.Empty;

            var options = RegexOptions.IgnoreCase | RegexOptions.Compiled;// | RegexOptions.ExplicitCapture;

            var race = Regex.Matches(json, "\"Race\"\\s*:\\s*\"(\\w*)\"", options)[0].Groups[1].Value;
            var type = Regex.Matches(json, "\"Type\"\\s*:\\s*\"(\\w*)\"")[0].Groups[1].Value;
            var rootPath = Regex.Matches(json, "\"RootPath\"\\s*:\\s*\"(.*)\"")[0].Groups[1].Value;
            var fileName = Regex.Matches(json, "\"FileName\"\\s*:\\s*\"(.*)\"")[0].Groups[1].Value;

            return new BotConfig(race, type, rootPath, fileName, dirName);
        }

        public override string ToString()
        {
            return $"{Name} ({Race}, {Type}), path {RootPath}\\{FileName}";
        }
    }
}
