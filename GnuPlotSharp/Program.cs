using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GnuPlotSharp
{
    public class GnuPlotLauncher
    {
        public void Launch(string arguments)
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFile.Directory.FullName, "gnuplot", "gnuplot.exe");

            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                throw new Exception("Unable to locate gnuplot at: " + fi.FullName);
            }

            var p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            Console.WriteLine(p.StandardOutput.ReadToEnd());

            var error = p.StandardError.ReadToEnd();
            Console.WriteLine(error);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new Exception("nonzero exit code: " + error);
            }
        }

    }

    public class Row<T1, T2>
    {
        public readonly KeyValuePair<T1, T2>[] Data;
        public readonly string Title;

        public Row(string title, T1[] xdata, T2[] ydata)
            : this(title, xdata.Zip(ydata, (a, b) => new KeyValuePair<T1, T2>(a, b)).ToArray())
        {

        }

        public Row(string title, KeyValuePair<T1, T2>[] data)
        {
            this.Title = title;
            this.Data = data;
        }
    }

    public class RenderResults
    {
        public readonly string[] DataFiles;
        public readonly string Outputfile;
        public readonly string ScriptFile;

        public RenderResults(string scriptFile, string[] dataFiles, string outputfile)
        {
            this.ScriptFile = scriptFile;
            this.DataFiles = dataFiles;
            this.Outputfile = outputfile;
        }
    }


    public class GnuPlotScript
    {
        private readonly string title;

        public GnuPlotScript(string title)
        {
            this.title = title;
        }

        public RenderResults Render<T1, T2>(params Row<T1, T2>[] rows)
        {
            var outputfile = Path.Combine(Path.GetTempFileName() + ".png");

            if (rows is Row<string, T2>[] _rows)
                return Render(outputfile, _rows);
            else
                return Render(outputfile, rows);
        }

        public RenderResults Render<T2>(string outputfile, params Row<string, T2>[] rows)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var scriptFile = Path.GetTempFileName() + ".txt";

            var scriptContent =
$@"set term png
set output ""{outputfile.Replace("\\", "/")}""
";

            if (rows.Any())
            {
                // Generate IDs for the labels
                //var labels = rows[0].Data.Select(t => t.Key).ToList();
                var labels = rows.SelectMany(t => t.Data).GroupBy(t => t.Key).Select(t => t.Key).ToList();
                var label_ids = Enumerable.Range(0, labels.Count);
                var paired_ids = labels.Zip(label_ids, (label, id) => $"\"{label}\" {id}");
                var args = paired_ids.Aggregate((c, next) => c + "," + next);

                // Assigning the labels to the x-axis
                scriptContent += $"set xtics({args})\n";

                // $@"plot [0.0:0.5] [2:6] ""{dataFile.Replace("\\", "\\\\")}"" with lines   title ""{title}""";

                var xmin = 0;
                var xmax = labels.Count - 1;
                var ymin = rows.SelectMany(r => r.Data).Select(v => v.Value).Min();
                var ymax = rows.SelectMany(r => r.Data).Select(v => v.Value).Max();

                scriptContent += $@"plot [{xmin}:{xmax}] [{ymin}:{ymax}] ";

                // row.Data.Select(t => labels.IndexOf(t.Key)).ToArray(), row.Data.Select(t => t.Value).ToArray()
                var new_rows = rows.Select(row => new Row<int, T2>(row.Title, row.Data.Select(t => new KeyValuePair<int, T2>(labels.IndexOf(t.Key), t.Value)).OrderBy(t => t.Key).ToArray())).ToArray();

                var dataFiles = WriteData(new_rows).ToArray();

                var plotBlocks = from dataFile in dataFiles
                                 select $@"""{dataFile.Item1.Replace("\\", "\\\\")}"" with lines   title ""{dataFile.Item2}""";

                scriptContent += String.Join(", ", plotBlocks);

                File.WriteAllText(scriptFile, scriptContent);

                var arguments = @"-c " + scriptFile;

                new GnuPlotLauncher().Launch(arguments);

                return new RenderResults(scriptFile, dataFiles.Select(d => d.Item1).ToArray(), outputfile);
            }

            return new RenderResults(scriptFile, new string[] { }, "");
        }

        public RenderResults Render<T1, T2>(string outputfile, params Row<T1, T2>[] rows)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");

            var scriptFile = Path.GetTempFileName() + ".txt";

            var scriptContent =
$@"set term png
set output ""{outputfile.Replace("\\", "/")}""
";

            if (rows.Any())
            {
                // $@"plot [0.0:0.5] [2:6] ""{dataFile.Replace("\\", "\\\\")}"" with lines   title ""{title}""";

                var xmin = rows.SelectMany(r => r.Data).Select(v => v.Key).Min();
                var xmax = rows.SelectMany(r => r.Data).Select(v => v.Key).Max();
                var ymin = rows.SelectMany(r => r.Data).Select(v => v.Value).Min();
                var ymax = rows.SelectMany(r => r.Data).Select(v => v.Value).Max();

                scriptContent += $@"plot [{xmin}:{xmax}] [{ymin}:{ymax}] ";
            }

            var dataFiles = WriteData(rows).ToArray();

            var plotBlocks = from dataFile in dataFiles
                             select $@"""{dataFile.Item1.Replace("\\", "\\\\")}"" with lines   title ""{dataFile.Item2}""";

            scriptContent += String.Join(", ", plotBlocks);

            File.WriteAllText(scriptFile, scriptContent);

            var arguments = @"-c " + scriptFile;

            new GnuPlotLauncher().Launch(arguments);

            return new RenderResults(scriptFile, dataFiles.Select(d => d.Item1).ToArray(), outputfile);
        }

        private IEnumerable<Tuple<string, string>> WriteData<T1, T2>(Row<T1, T2>[] rows)
        {
            int i = 0;
            foreach (var row in rows)
            {
                var data = String.Join("\n", row.Data.Select(kv => $"{kv.Key}\t{kv.Value}"));

                var dataFile = (Path.GetTempFileName() + $"plt_{i}.dat");
                File.WriteAllText(dataFile, data);

                yield return new Tuple<string, string>(dataFile, row.Title);
            }
        }
    }


    internal class Program
    {
        internal static void Main(string[] args)
        {
            var outputfile_strings = "printme4.png";
            new GnuPlotScript("With string labels")
                .Render(
                    outputfile: outputfile_strings,
                    new Row<string, int>("series 1", new[] { "a", "b", "c", "d" }, new[] { 5, 4, 3, 4 }), 
                    new Row<string, int>("series 2", new[] { "e", "b", "c", "d" }, new[] { 1, 5, 9, 2 }));

            Process.Start(outputfile_strings);

            var outputfile_nums = "printme5.png";
            new GnuPlotScript("pure numbers").Render(
                outputfile: outputfile_nums,
                rows: new Row<double, int>("pure numbers", new[] { 0.1, 0.2, 0.3, 0.4 }, new[] { 5, 4, 3, 4 }));
            Process.Start(outputfile_nums);

            //Console.ReadLine();
        }
    }
}
