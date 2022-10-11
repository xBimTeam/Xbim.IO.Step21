using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xbim.IO.Step21.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Methods to help manage Part21 syntax.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Fixes an IFC file provided and returns the name of the fixed temp file.
        /// </summary>
        /// <param name="inputFile">File to fix.</param>
        /// <returns>Name of the file that has been fixed, or empty string in case of failure</returns>
        public static string Part21Fix(FileInfo inputFile)
        {
            if (!inputFile.Exists)
            {
                return "";
            }

            try
            {
                var totL = (double)inputFile.Length;

                // preparing out file

                FileInfo fOut;
                do
                {
                    fOut = new FileInfo(
                        Path.Combine(
                            Path.GetTempPath(),
                            Guid.NewGuid().ToString() + ".ifc"
                        ));
                } while (fOut.Exists);

                var outName = fOut.FullName;

                using var fw = File.CreateText(outName);
                using var fr = inputFile.OpenText();
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

                void OnEntityFound(StepEntityAssignment assignment)
                {
                    if (firstEntity)
                    {
                        fw.WriteLine("ENDSEC;");
                        fw.WriteLine("DATA;");
                        firstEntity = false;
                    }
                    assignment.WritePart21(fw);
                    var perc = assignment.Span.End / totL;
                }
                using var st = new BufferedUri(inputFile);
                StepParsing.ParseWithEvents(st, OnHeaderFound, OnEntityFound);

                fw.WriteLine("ENDSEC;");
                fw.WriteLine("END-ISO-10303-21;");
                return outName;
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
