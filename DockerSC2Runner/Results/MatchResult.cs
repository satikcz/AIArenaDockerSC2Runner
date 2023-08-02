namespace DockerSC2Runner.Results
{
    public class MatchResult
    {
        public int match { get; set; }
        public float bot1_avg_step_time { get; set; }
        public float bot2_avg_step_time { get; set; }
        public string[] bot1_tags { get; set; } = new string[0];
        public string[] bot2_tags { get; set; } = new string[0];
        public string type { get; set; } = string.Empty;
        public int game_steps { get; set; }
    }
}
