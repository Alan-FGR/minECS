using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GenericFormatter
{
    public string preText, postText;
    public List<string> generics;

    public GenericFormatter(){}

    public GenericFormatter(string preText, string postText, List<string> generics)
    {
        this.preText = preText;
        this.postText = postText;
        this.generics = generics;
    }

    public string Format(int qty)
    {
        if (generics == null) return preText;
        var gens = new List<String>();
        foreach (string s in generics)
        {
            if (s.Any(Char.IsDigit))
                for (int i = 0; i < qty; i++)
                    gens.Add(Utils.IncrementGeneric(s, i));
            else
                gens.Add(s);
        }
        return $"{preText}<{string.Join(", ",gens)}>{postText}";
    }
}

public static class Utils
{
    public static string IncrementGeneric(string str, int amount)
    {
        var newStr = "";
        foreach (char c in str)
            if (char.IsDigit(c))
                newStr += char.GetNumericValue(c) + amount;
            else
                newStr += c;
        return newStr;
    }

    public static string FormatSignature(string declStr, int genQty)
    {
        var func = new GenericFormatter();
        var parameters = new List<GenericFormatter>();

        var split = declStr.Split('(');
        var pars = split.Last().Split(')').First().Split(',');

        var split2 = split.First().Split('<');

        func.preText = split2.First();
        func.postText = "";

        func.generics = split2.Last().Split('>').First().Split(',').ToList();

        foreach (string par in pars)
        {
            if (par.Contains('<'))
            {
                var nam = par.Split('<').First();
                var pgens = par.Split('<').Last().Split('>').First().Split(',').ToList();
                var e = par.Split('<').Last().Split('>').Last();
                parameters.Add(new GenericFormatter(nam, e, pgens));
            }
            else
            {
                parameters.Add(new GenericFormatter(par, "", null));
                //if parameter is a generic (todo get generic parameters) repeat for qty
                if (par.Split(' ').Any(x => x == "T0"))
                {
                    for (int i = 1; i < genQty; i++)
                    {
                        parameters.Add(new GenericFormatter(Utils.IncrementGeneric(par, i), "", null));
                    }
                }
            }
        }

        return $"{func.Format(genQty)}({string.Join(", ",parameters.Select(x=>x.Format(genQty)))})";
    }
}

class Program
{
    private static List<string> curRegion;
    private static int curQty;

    public static Dictionary<string, (string descr, Func<string[], int, string> func)> modeFunctions = new Dictionary<string, (string descr, Func<string[], int, string> func)>
    {
        {"function", ("generates specified quantity of variadic functions", (options, line) =>
        {
            //var qty = int.Parse(options[0]); //todo allow override
            var fs = Utils.FormatSignature(curRegion[line], curQty);
            return fs;
        })},

        {"delegate", ("generates specified quantity of variadic delegates", (options, line) =>
        {
            return Utils.FormatSignature(curRegion[line], curQty)+";";
        })},

        {"duplicate", ("duplicates the line and increments identifiers, it's possible to specify the separator char", (options, line) =>
        {
            var sep = options.Length > 0 ? options[0] : null;
            var l = curRegion[line].Split("//").First();
            var sw = new StringWriter();
            for (int i = 0; i < curQty; i++)
            {
                sw.Write(Utils.IncrementGeneric(l,i) + (i!=curQty-1?((sep??"")+"\r\n"):"") );
            }
            return sw.ToString();
        })},

        {"quantity", ("sets variable to the quantity of variadics generated", (options, line) =>
        {
            var l = curRegion[line].Split("//").First();
            var split = l.Split('=');
            return split.First() + "= " + curQty + ";";
        })},
    };

    static void Main(string[] args)
    {
        String[] files = {"Registry", "ArchetypePool"};
        string path = "Archetypes";

        Console.WriteLine("C# VARIADIC GENERATOR. VALID TAGS:");
        foreach (var modeFunction in modeFunctions)
        {
            Console.WriteLine($"  {modeFunction.Key}");
            Console.WriteLine($"    Description: {modeFunction.Value.descr}");
        }

        foreach (string file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
        {
            var pathElements = file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (pathElements.Contains("obj"))
                continue;
            if (!files.Contains(Path.GetFileName(file)) && !files.Contains(Path.GetFileNameWithoutExtension(file)))
                continue;
            
            var code = File.ReadAllLines(file);
            var generatedCode = new StringWriter();

            Console.WriteLine($"PARSING FILE: {file}...");

            var regions = new List<(int qty, List<string> lines)>();

            List<string> currentRegion = null;

            foreach (string line in code)
            {
                if (line.ToLowerInvariant().Contains("#region variadic"))
                {
                    currentRegion = new List<string>();
                    regions.Add((int.Parse(line.Trim().Split(' ')[2]), currentRegion));
                    continue;
                }

                if (line.ToLowerInvariant().Contains("#endregion"))
                {
                    currentRegion = null;
                    continue;
                }

                currentRegion?.Add(line);
            }

            foreach (var tuple in regions)
            {
                curRegion = tuple.lines;
                for (int qti = 2; qti <= tuple.qty; qti++)
                {
                    curQty = qti;
                    for (var i = 0; i < tuple.lines.Count; i++)
                    {
                        string line = tuple.lines[i];

                        var trimmed = line.TrimStart(' ');
                        if (trimmed.Length >= 2)
                        {
                            string substring = trimmed.Substring(0, 2);
                            if (substring == "//")
                                continue;
                            if (substring == "/*")
                                Console.WriteLine($"MULTILINE COMMENT BLOCKS (DETECTED ON LINE {i}) NOT SUPPORTED." +
                                                  "YOU CAN USE THEM FOR COMMENTS AS USUAL BUT MAKE SURE THERE ARE NO TAGS IN THEM.");
                        }

                        if (line.ToLowerInvariant().Contains("genvariadic"))
                        {
                            var pars = line.Split("genvariadic")[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            Console.WriteLine(
                                $"found variadic on line {i}, {pars.Length} parameters: {string.Join(", ", pars)}");

                            if (pars.Length < 1)
                            {
                                Console.WriteLine("NO PARAMETERS!");
                                continue;
                            }

                            var mode = pars.First();
                            var options = pars.Skip(1).ToArray();

                            if (modeFunctions.TryGetValue(mode, out var value))
                            {
                                var str = value.Item2(options, i);
                                generatedCode.WriteLine(str);
                            }
                            else
                            {
                                string err = $"INVALID MODE: {mode}";
                                Console.WriteLine(err);
                                generatedCode.WriteLine(err);
                            }
                        }
                        else
                        {
                            generatedCode.WriteLine(line);
                        }
                    }
                }
            }

            Console.WriteLine($"PARSED FILE: {file}\n");


            var allcode = "";
            foreach (string line in code)
            {
                var trimmed = (line.Trim());
                if (trimmed.Length > 6)
                    if (trimmed.Substring(0, 5) == "using")
                        allcode += line+"\r\n";
            }

            allcode += $"public unsafe partial class {Path.GetFileNameWithoutExtension(file)} {{";

            allcode += generatedCode.ToString();

            allcode += "}";

            File.WriteAllText(Path.Combine(path, Path.GetFileNameWithoutExtension(file)+"GeneratedVariadic.cs"), allcode);

        }

        //Console.ReadKey();
    }
}

