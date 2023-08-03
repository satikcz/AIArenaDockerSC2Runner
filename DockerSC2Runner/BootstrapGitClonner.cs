namespace DockerSC2Runner
{
    /// <summary>
    /// Class that obtains the git bootstrap data
    /// </summary>
    public class BootstrapGitClonner
    {
        public void CloneBootstrap(string workingDir = "", string repoPath = "https://github.com/aiarena/local-play-bootstrap")
        {
            try
            {
                {
                    using Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "git";
                    p.StartInfo.WorkingDirectory = workingDir;
                    p.StartInfo.CreateNoWindow = true;

                    if (!Directory.Exists("local-play-bootstrap"))
                    {
                        Console.WriteLine($"Clonning bootstrap from {repoPath}, please wait...");
                        p.StartInfo.Arguments = $"clone {repoPath} --recurse-submodules";
                    }
                    else
                    {
                        Console.WriteLine($"Pulling latest bootstrap version...");
                        p.StartInfo.Arguments = $"pull --recurse-submodules";
                    }

                    p.Start();
                    p.WaitForExit();
                    var output = p.StandardOutput.ReadToEnd();
                    Console.WriteLine($"Bootstrap result: {output}");
                }

                {
                    using Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "git";
                    p.StartInfo.WorkingDirectory = workingDir + "/local-play-bootstrap";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = $"reset --hard 0693cf9098a0ea35546f999677a85ec874bb6173";

                    p.Start();
                    p.WaitForExit();
                    var output = p.StandardOutput.ReadToEnd();
                    Console.WriteLine($"Set to 0693cf9098a0ea35546f999677a85ec874bb6173");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}
