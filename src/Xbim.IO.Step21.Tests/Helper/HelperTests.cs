using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Xbim.IO.Step21.Tests.Helper
{
    public class HelperTests
    {
        [Fact]
        public void CanFixFile()
        {
            var t = new FileInfo(@"TestFiles\Strings.ifc");
            var fixedPath = Xbim.IO.Step21.Helper.Part21Fix(t);
            Assert.False(string.IsNullOrEmpty(fixedPath));
        }

        [Fact]
        public void CanFixOther()
        {
            var t = new FileInfo(@"C:\Data\Ifc\_DebugSupport\1\001_Asse.ifc");
            var fixedPath = Xbim.IO.Step21.Helper.Part21Fix(t);
            Assert.False(string.IsNullOrEmpty(fixedPath));
        }
    }
}
