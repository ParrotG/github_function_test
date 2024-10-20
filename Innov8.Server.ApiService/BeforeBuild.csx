public static Task<int> Pwsh(this string cmd)
{
  var source = new TaskCompletionSource<int>();
  var escapedArgs = cmd.Replace("\"", "\\\"");
  var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = "pwsh",
      Arguments = $"-c \"{escapedArgs}\"",
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    },
    EnableRaisingEvents = true
  };
  process.Exited += (sender, args) =>
  {
    if (process.ExitCode == 0)
    {
      source.SetResult(0);
    }
    else
    {
      source.SetException(
        new Exception(
          $"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
    }

    process.Dispose();
  };

  try
  {
    process.Start();
  }
  catch (Exception e)
  {
    source.SetException(e);
  }

  return source.Task;
}


private static void CopyFilesRecursively(string sourcePath, string targetPath)
{
  foreach(string dirPath
    in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
  {
    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
  }

  foreach (string newPath
    in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
  {
    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
  }
}

string workingDirectory = Environment.CurrentDirectory;

string projectDirectory =
  Directory.GetParent(workingDirectory).FullName + "\\Innov8.frontend";

Console.WriteLine(">> Start Building Frontend...");

await Pwsh($"cd {projectDirectory} && bun install");

await Pwsh($"cd {projectDirectory} && bun run build");

CopyFilesRecursively(
  $"{projectDirectory}\\dist", $"{workingDirectory}\\wwwroot");

Console.WriteLine(">> Frontend Built and Copied to wwwroot...");
