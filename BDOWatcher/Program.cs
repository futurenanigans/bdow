using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BDOWatcher
{
    class Program
    {
        private const string ProcName = "BlackDesert64";
        private const int Limit = 10;

        static void Main(string[] args)
        {
            Console.Title = "BDO! Please dont burn my GPU!";

            var count = 0;

            while (true)
            {
                var has = false;
                var procId = GetApplicationId();

                if (procId > 0)
                {
                    has = ProcHasNetworkActivity(procId);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(0,0);
                    Console.Write($"{DateTime.Now}: ");
                    Console.ForegroundColor = has ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write(has);
                    Console.Write(Environment.NewLine);
                }

                count = !has ? count + 1 : 0;

                if (count >= Limit) break;

                Thread.Sleep(3000);
                GC.Collect();
            }

            KillProc();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Environment.NewLine);
            Console.WriteLine("Press whatever to exit");
            Console.ReadLine();
        }

        private static void KillProc()
        {
            var proc = Process.GetProcessesByName(ProcName).FirstOrDefault();
            proc?.Kill();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Killed: {ProcName}");
        }

        private static int GetApplicationId()
        {
            var proc = Process.GetProcessesByName(ProcName).FirstOrDefault();
            return proc?.Id ?? 0;
        }

        private static bool ProcHasNetworkActivity(int id)
        {
            var ps = new ProcessStartInfo
            {
                Arguments = "-a -n -o",
                FileName = "netstat.exe",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var p = new Process{StartInfo = ps})
            {
                p.Start();

                var stdOutput = p.StandardOutput;

                while (!stdOutput.EndOfStream)
                {
                    var idAsString = id.ToString();

                    var line = stdOutput.ReadLine();

                    if (line != null && line.Contains(idAsString)) return true;
                }
            }

            return false;
        }
    }
}
