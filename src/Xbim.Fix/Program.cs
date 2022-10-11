using CommandLine;

namespace Xbim.IfcTool
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var t = Parser.Default.ParseArguments<
                   FixOptions
               >(args)
             .MapResult(
               (FixOptions opts) => FixOptions.Run(opts),
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
            IfcError = 16,
        }

    }
}