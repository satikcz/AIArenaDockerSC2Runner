namespace DockerSC2Runner
{
    /// <summary>
    /// Class that obtains the git bootstrap data
    /// </summary>
    public class BootstrapGitClonner
    {
        public const string BootstrapDir = "local-play-bootstrap";

        public void CloneBootstrap(string workingDir = "", string repoPath = "https://github.com/aiarena/local-play-bootstrap")
        {
            try
            {
                string res = string.Empty;
                if (!Directory.Exists(BootstrapDir))
                {
                    res = CLI.Run($"git clone {repoPath} --recurse-submodules", workingDir);
                }
                else
                {
                    res = CLI.Run("git pull --recurse-submodules", workingDir);
                }

                Console.WriteLine($"Bootstrap result: {res}");

                // Workaround for when the newest bootstrap had some issues
                //res = CLI.Run("git reset --hard 0693cf9098a0ea35546f999677a85ec874bb6173", workingDir + "/local-play-bootstrap");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}
