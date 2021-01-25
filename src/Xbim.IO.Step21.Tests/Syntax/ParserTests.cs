using Xbim.IO.Step21.Text;
using System.IO;
using Xunit;

namespace Xbim.IO.Step21.Tests.Syntax
{
    public class ParserTests
    {
        [Theory]
        [InlineData(@"C:\Data\Ifc\Basics\OneWall.ifc")]
        [InlineData(@"C:\Data\Ifc\Basics\Duplex_MEP_20110907.ifc")]
        [InlineData(@"C:\Data\Ifc\Basics\Duplex_A_20110907.ifc")]
        public void CanParse(string fileName)
        {
            var s = SourceText.From(new FileInfo(fileName));
            var syntaxTree = StepParsing.Parse(s);
            var root = syntaxTree.Root;
            Assert.Empty(syntaxTree.Diagnostics);
            Assert.IsType<StepSyntax>(root);
            Assert.NotEmpty(root.Headers);
            Assert.NotEmpty(root.Members);
        }
    }
}
