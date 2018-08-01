using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Benchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string[]> results = new List<string[]>();

            Console.WriteLine("Running tests...");
            foreach (string command in new[] {"dotnet", "BenchmarkEntt.exe"})
            foreach (string shift in new[] {"14", "16", "18", "20"})
            foreach (string mods in new[] {"1", "2", "4"})
            {
                Console.WriteLine($"Running {command}, Args {shift} {mods}");
                var proc = new Process {StartInfo = new ProcessStartInfo(
                    command, (command == "dotnet" ? "minECS.dll " : "") + $"{shift} {mods}")};
                proc.Start();
                proc.WaitForExit();

                string test = (command == "dotnet" ? "minECS" : "EnTT");
                var lines = File.ReadAllLines("results_" + test + $"_{shift}_{mods}.txt");
                var result = lines.Where(x => x.Split(':')[0] == "looping").Select(x => Convert.ToSingle(x.Split(':')[1])).OrderBy(x => x).Take(3).Sum()/3f;
                results.Add(new []{test,shift,mods,result.ToString("F0")});
            }

            var sl = results.OrderBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[0]).ToArray();

            var allResults = new StreamWriter("results_ALL.txt");
            allResults.WriteLine("c2f is the fraction of the 2nd component, e.g. 1/2 means 2nd comp was added to half entities");
            allResults.WriteLine("ecs       count c2f  result(µs)");
            int c = 0;
            foreach (string[] s in sl)
            {
                if (c % 2 == 0)
                    allResults.WriteLine();
                allResults.WriteLine($"{s[0].PadRight(7)} {(1 << int.Parse(s[1])).ToString().PadLeft(7)} {"1/"+s[2].PadRight(2)} {s[3].PadLeft(7)}µs");
                c++;
            }
            allResults.Close();

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
