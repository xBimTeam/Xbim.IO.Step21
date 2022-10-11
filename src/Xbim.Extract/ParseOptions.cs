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
    [Verb("parse", HelpText = "Checks the file structure using available parsing methods.")]
    class ParseOptions
    {
        [Option('f', "fast", Required = false, HelpText = "uses the fast parsing method. If neither `fast` or `complete` are set, both are performed.", Default = false)]
        public bool UseFast { get; set; }

        [Option('c', "complete", Required = false, HelpText = "uses the complete parsing method. If neither `fast` or `complete` are set, both are performed.", Default = false)]
        public bool UseComplete { get; set; }

        [Option('s', "stopOnIssue", Required = false, HelpText = "Stops on first issue reporting the last valid entity.", Default = false)]
        public bool StopOnIssue { get; set; }

        [Value(0,
           MetaName = "source",
           HelpText = "Input file to be processed.",
           Required = true)]
        public string FileName { get; set; }

        internal static Status Run(ParseOptions opts)
        {
            var f = new FileInfo(opts.FileName);
            if (!f.Exists)
            {
                Console.WriteLine($"File not found: {f.FullName}");
                return Status.NotFoundError;
            }
            var strippedName = f.FullName + (".stripped.ifc");
            var fw = new FileInfo(strippedName);
            if (fw.Exists)
                fw.Delete();


            Console.WriteLine($"Parsing {f.FullName}");
            if (
                !opts.UseComplete &&
                !opts.UseFast 
                )
            {
                opts.UseFast = true;
                opts.UseComplete = true;
            }
            FileReport(f, opts);
            return Status.Ok;
        }


        private static void FileReport(FileInfo f, ParseOptions opts)
        {
            var totL = (double)f.Length;
            int entityCount = 0;
            int headerCount = 0;

            void NewHeaderEntity(StepHeaderEntity headerEntity)
            {
                headerCount++;
            }

            if (opts.UseComplete)
            {
                StepEntityAssignment last = null;
                IEnumerable<Diagnostic> res;
                using (var progress = new ProgressBar())
                {
                    void NewEntityAssignment(StepEntityAssignment assignment)
                    {
                        entityCount++;

                        if (entityCount % 1000 == 0)
                        {
                            var startPos = assignment.Span.Start;
                            var perc = startPos / totL;
                            progress.Report(perc);
                        }
                    }

                    void FirstIssueEntityAssignment(StepEntityAssignment assignment)
                    {
                        entityCount++;
                        last = assignment;
                        if (entityCount % 1000 == 0)
                        {
                            var startPos = assignment.Span.Start;
                            var perc = startPos / totL;
                            progress.Report(perc);
                        }
                    }

                    void NotifyIssue(Diagnostic issue)
                    {
                        Console.WriteLine($"{issue.Message} @ {issue.Location}.");
                        Console.WriteLine($"Last valid entity is {last.Identity.Text} @ {last.Span}");
                        Environment.Exit((int)Status.IfcError);
                    }


                    var s = new Stopwatch();
                    s.Start();
                    using (var st = new BufferedUri(f))
                    {
                        res = StepParsing.ParseWithEvents(st, NewHeaderEntity,
                            opts.StopOnIssue ? FirstIssueEntityAssignment : NewEntityAssignment,
                            opts.StopOnIssue ? NotifyIssue : null);
                        s.Stop();
                        Thread.Sleep(400);
                        progress.Report(1);
                    }
                    Console.WriteLine($"Parsed {headerCount} header entities and {entityCount} assignments in {s.ElapsedMilliseconds} ms.");
                }
                DiagnosticReport(res, f);
            }

            if (opts.UseFast)
            {
                IEnumerable<Diagnostic> res;
                using (var progress = new ProgressBar())
                {
                    void FastEntityAssignment(StepEntityAssignmentBare assignment)
                    {
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
                    var s = new Stopwatch();
                    s.Start();
                    using (var st = new BufferedUri(f))
                    {
                        res = StepParsing.ParseWithEvents(st, NewHeaderEntity, FastEntityAssignment);
                        s.Stop();
                        Thread.Sleep(200);
                        progress.Report(1);
                    }
                    Console.WriteLine($"Parsed {headerCount} header entities and {entityCount} assignments in {s.ElapsedMilliseconds} ms.");
                }
                DiagnosticReport(res, f);
            }
        }

       

        private static void DiagnosticReport(IEnumerable<Diagnostic> res, FileInfo f)
        {
            if (res.Any())
            {
                using var tr = f.OpenText();
                foreach (var diagnostic in res)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{diagnostic.Message} [span: {diagnostic.Location.Span.Start}..{diagnostic.Location.Span.End}]");
                    Console.ResetColor();
                    var p1 = diagnostic.Location.Span.Start - 200;
                    var p2 = diagnostic.Location.Span.End + 200;
                    p1 = Math.Max(p1, 0);
                    p2 = Math.Min(p2, f.Length);
                    tr.BaseStream.Position = p1;
                    var len = (int)(p2 - p1);
                    var buf = new char[len];
                    tr.ReadBlock(buf, 0, len);

                    var l1 = (int)(diagnostic.Location.Span.Start - p1);
                    var l2 = (int)(diagnostic.Location.Span.End - diagnostic.Location.Span.Start);
                    var l3 = (int)(p2 - diagnostic.Location.Span.End);
                    Console.Write(buf, 0, l1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(buf, l1, l2);
                    Console.ResetColor();
                    Console.WriteLine(buf, l1 + l2, l3);
                }
            }
        }

    }
}
