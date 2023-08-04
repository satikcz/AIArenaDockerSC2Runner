namespace DockerSC2Runner
{
    public class CLI
    {
        /// <summary>
        /// Runs given commandline command
        /// </summary>
        /// <param name="cmd">Command to run</param>
        /// <param name="workingDir">Working directory</param>
        /// <returns></returns>
        public static async Task<string> RunAsync(string cmd, string workingDir = "")
        {
            try
            {
                using Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = workingDir;
                p.StartInfo.Arguments = $"/C {cmd}";
                p.Start();
                var output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        public static string Run(string cmd, string workingDir = "")
        {
            return RunAsync(cmd, workingDir).GetAwaiter().GetResult();
        }
    }
}
