using Xbim.IO.Step21;
using Xbim.IO.Step21.Step21.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace NeXtract
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Xbim.Extract [FileName.Ifc]");
                return;
            }

            var f = new FileInfo(args[0]);
            var totL = (double)f.Length;

            int entityCount = 0;
            int headerCount = 0;

            void NewHeaderEntity(StepEntitySyntax headerEntity)
            {
                headerCount++;
            }

            IEnumerable<Diagnostic> res;
            using (var progress = new ProgressBar())
            {
                void NewEntityAssignment(StepEntityAssignmentSyntax assignment)
                {
                    entityCount++;
                    if (entityCount % 1000 == 0)
                    {
                        var startPos = assignment.Span.Start;
                        var perc = startPos / totL;
                        progress.Report(perc);
                    }

                }
                Stopwatch s = new Stopwatch();
                s.Start();
                using (BufferedUri st = new BufferedUri(f))
                { 
                    res = StepParsing.ParseWithEvents(st, NewHeaderEntity, NewEntityAssignment);
                    s.Stop();
                    Thread.Sleep(200);
                    progress.Report(1);
                }
                Console.WriteLine($"Parsed {headerCount} header entities and {entityCount} assignments in {s.ElapsedMilliseconds} ms.");
            }

            DiagnosticReport(res, f);

            using (var progress = new ProgressBar())
            {
                void FastEntityAssignment(StepFastEntityAssignmentSyntax assignment)
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
                Stopwatch s = new Stopwatch();
                s.Start();
                using (BufferedUri st = new BufferedUri(f))
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