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
using Xbim.IO.Step21.Text;
using static Xbim.IfcTool.Program;

namespace Xbim.IfcTool
{
    [Verb("fix", HelpText = "Attempts to fix the syntax and encoding of an invalid IFC file.")]
    class FixOptions
    {
        [Option('s', "stopOnIssue", Required = false, HelpText = "Stops on first issue reporting the last valid entity.", Default = false)]
        public bool StopOnIssue { get; set; }

        [Value(0,
           MetaName = "source",
           HelpText = "Input file to be processed.",
           Required = true)]
        public string FileName { get; set; } = "";

        internal static Status Run(FixOptions opts)
        {
            var f = new FileInfo(opts.FileName);
            if (!f.Exists)
            {
                Console.WriteLine($"File not found: {f.FullName}");
                return Status.NotFoundError;
            }

            try
            {

                var totL = (double)f.Length;
                
                // preparing out file
                var outName = Path.ChangeExtension(f.FullName, ".fixed.ifc");

                if (File.Exists(outName))
                    File.Delete(outName);
                using var fw = File.CreateText(outName);
                using var fr = f.OpenText();
                fw.WriteLine("ISO-10303-21;");
                fw.WriteLine("HEADER;");
                // parse the entire file to get the addresses
                //
                void OnHeaderFound(StepHeaderEntity headerEntity)
                {
                   //  fw.WriteLine(headerEntity.);
                   headerEntity.WritePart21(fw);
                }
                bool firstEntity = true;
                using (var progress = new ProgressBar())
                {
                    void OnEntityFound(StepEntityAssignment assignment)
                    {
                        if (firstEntity)
                        {
                            fw.WriteLine("ENDSEC;");
                            fw.WriteLine("DATA;");
                            firstEntity = false;
                        }
                        // var id = Convert.ToUInt32(assignment.Identity.Value);
                        assignment.WritePart21(fw);
                        // fw.WriteLine(assignment.Span);
                        var perc = assignment.Span.End / totL;
                        progress.Report(perc);
                    }
                    using var st = new BufferedUri(f);
                    StepParsing.ParseWithEvents(st, OnHeaderFound, OnEntityFound);   
                }
                fw.WriteLine("ENDSEC;");
                fw.WriteLine("END-ISO-10303-21;");
            }
            catch (Exception ex)
            {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write(ex.Message);
                Console.ForegroundColor = c;
                return Status.Exception;
            }
            Console.WriteLine("Completed.");
            return Status.Ok;
        }



    }
}
