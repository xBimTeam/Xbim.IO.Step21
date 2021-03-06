using Xbim.IO.Step21;
using Xbim.IO.Step21.Step21.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;

namespace Xbim.Extract
{
    class Program
    {


        static int Main(string[] args)
        {
            var t = Parser.Default.ParseArguments<
                    StripOptions,
                    ParseOptions
                >(args)
              .MapResult(
                (StripOptions opts) => StripOptions.Run(opts),
                (ParseOptions opts) => ParseOptions.Run(opts),
                errs => Status.CommandLineError);
            return (int)t;        
        }

        [Flags]
        internal enum Status
        {
            Ok = 0,
            NotImplemented = 1,
            CommandLineError = 2,
            NotFoundError = 4,
            Exception = 8,
        }

    }
}