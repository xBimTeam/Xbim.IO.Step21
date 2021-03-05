using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xbim.IO.Step21;
using Xbim.IO.Step21.Step21.Text;
using static Xbim.Extract.Program;

namespace Xbim.Extract
{
    [Verb("strip", HelpText = "Reduces ifc files retaining selected entities only. Example: strip file.ifc -e 1 23")]
    class StripOptions
    {
        [Value(0,
            MetaName = "input file",
            HelpText = "Input file to be processed.",
            Required = true)]
        public string FileName { get; set; }

        [Option(
            'e', "entities",
            HelpText = "list of entities to be retained."
            )]
        public IEnumerable<string> Entities { get; set; }

        internal static Status Run(StripOptions opts)
        {
            FileInfo f = new FileInfo(opts.FileName);
            if (!f.Exists)
            {
                Console.WriteLine($"File not found: {f.FullName}");
                return Status.NotFoundError;
            }

            HashSet<int> required = new HashSet<int>();
            foreach (var item in opts.Entities)
            {
                if (int.TryParse(item, out var converted))
                {
                    required.Add(converted);
                }
                else
                {
                    Console.WriteLine($"Invalid entity id: {item}");
                    return Status.CommandLineError;
                }    
            }


            var totL = (double)f.Length;
            int entityCount = 0;
            int headerCount = 0;
            IEnumerable<Diagnostic> res;
            void OnHeaderFound(StepEntitySyntax headerEntity)
            {
                Console.WriteLine(headerEntity);
                headerCount++;
            }
            using (var progress = new ProgressBar())
            {
                void OnEntityFound(StepEntityAssignmentBareSyntax assignment)
                {
                    var t = int.Parse(assignment.Identity.Text.Substring(1));
                    if (required.Contains(t))
                    {

                    }
                    entityCount++;
                    if (entityCount % 1000 == 0)
                    {
                        var startPos = assignment.Span.Start;
                        var perc = startPos / totL;
                        progress.Report(perc);
                    }
                }
                entityCount = 0;
                headerCount = 0;
                Stopwatch s = new Stopwatch();
                s.Start();
                using (BufferedUri st = new BufferedUri(f))
                {
                    res = StepParsing.ParseWithEvents(st, OnHeaderFound, OnEntityFound);
                    s.Stop();
                    Thread.Sleep(200);
                    progress.Report(1);
                }
                Console.WriteLine($"Parsed {headerCount} header entities and {entityCount} assignments in {s.ElapsedMilliseconds} ms.");
            }

            return Status.NotImplemented;
        }
    }
}
