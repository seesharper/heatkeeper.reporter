using System;
using System.Diagnostics;
using System.Threading.Tasks;
namespace HeatKeeper.Reporter.Sdk
{
    public class Command
    {
        public static async Task<int> ExecuteAsync(string commandPath, string arguments, Action<string> dataReceived, string workingDirectory = null)
        {
            var process = CreateProcess(commandPath, arguments, workingDirectory);
            RedirectOutput(process, dataReceived);
            var exitCode = await StartProcessAsync(process);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"The command {commandPath} {arguments} failed.");
            }
            return exitCode;
        }

        private static Task<int> StartProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.Exited += (o, s) =>
            {
                tcs.SetResult(process.ExitCode);
            };
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;
        }

        private static void RedirectOutput(Process process, Action<string> dataReceived)
        {
            process.OutputDataReceived += (o, a) => WriteStandardOut(a);
            process.ErrorDataReceived += (o, a) => WriteStandardError(a);
            void WriteStandardOut(DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    Console.Error.WriteLine(args.Data);
                    dataReceived(args.Data);
                }
            }

            void WriteStandardError(DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    Console.Error.WriteLine(args.Data);
                }
            }
        }

        private static Process CreateProcess(string commandPath, string arguments, string workingDirectory)
        {
            var startInformation = new ProcessStartInfo($"{commandPath}");
            startInformation.CreateNoWindow = true;
            startInformation.Arguments = arguments;
            startInformation.RedirectStandardOutput = true;
            startInformation.RedirectStandardError = true;
            startInformation.UseShellExecute = false;
            startInformation.WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;
            var process = new Process();
            process.StartInfo = startInformation;
            return process;
        }
    }
}