namespace DockerSC2Runner
{
    public static class Helpers
    {
        public static TimeSpan ConvertGameLength(long frames)
        {
            return TimeSpan.FromSeconds(frames / 22.4f);
        }
    }
}
