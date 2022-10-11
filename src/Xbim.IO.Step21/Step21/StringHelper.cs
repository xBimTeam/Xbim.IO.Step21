using System;
using System.Collections.Generic;
using System.Text;

namespace Xbim.IO.Step21.Step21
{
    internal static class StringHelper
    {
        internal static string StripBoundaries(string inputString)
        {
#if NETSTANDARD2_1_OR_GREATER
            return inputString[1..^1];
#else
            return inputString.Substring(1, inputString.Length - 2);
#endif
        }
    }
}
