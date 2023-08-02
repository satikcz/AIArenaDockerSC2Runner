namespace DockerSC2Runner
{
    public class RunnerConfig
    {
        /// <summary>
        /// Feel free to make those consts json property if needed
        /// </summary>
        public const string BotsFolder = "Bots";
        public const string BotJsonFile = "ladderbots.json";

        public const string LocalPlayBootstrapGitFolder = "Local";

        public string RunnersFolder { get; set; } = @"C:\Temp\Docker\SC2\MultiRunner";
        public string ResultsFolder { get; set; } = @"C:\Temp\Docker\SC2\MultiRunner\Results";
        public string Maps { get; set; } = "2000AtmospheresAIE";
        public int RunnerCount { get; set; } = 10;
        public int MatchCount { get; set; } = 50;
        public string Bot1Name { get; set; } = "DadBot";
        public string Bot2Name { get; set; } = "MechaShark";
        public bool StopOnFinish { get; set; } = false;

        public DateTime Start { get; set; } = DateTime.Now;
    }
}
