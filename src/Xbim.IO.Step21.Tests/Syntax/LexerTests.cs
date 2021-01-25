using System.IO;
using System.Linq;
using Xbim.IO.Step21.Text;
using Xunit;

namespace Xbim.IO.Step21.Tests.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            var text = "'text";
            var tokens = StepParsing.ParseTokens(text, out var diagnostics);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StepString, token.Kind);
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 5), diagnostic.Location.Span);
            Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("foo42")]
        [InlineData("foo_42")]
        [InlineData("_foo")]
        public void Lexer_Lexes_Identifiers(string name)
        {
            var tokens = StepParsing.ParseTokens(name).ToArray();

            Assert.Single(tokens);

            var token = tokens[0];
            Assert.Equal(SyntaxKind.StepIdentifierToken, token.Kind);
            Assert.Equal(name, token.Text);
        }

        [Theory]
        [InlineData(@"TestFiles\Duplex_A.subset.ifc", 0, 0, "ViewDefinition [CoordinationView]", "1OaF7j$Fj2cP4Xy3mK4kIj", 0, -839999)]
        [InlineData(@"TestFiles\Numeric.ifc", 100000.1000001, 100000.1000001, "ViewDefinition [CoordinationView]", "Project Status", 32, 42)]
        [InlineData(@"TestFiles\Minimal.ifc", 0, 1E-05, "ViewDefinition [CoordinationView]", "Project ' Status", 0, -12)]
        public void LexerValues(string fileName, double firstFloat, double lastFloat, string firstString, string lastString, int firstInt, int lastInt)
        {
            FileInfo f = new FileInfo(fileName);
            var allfile = File.ReadAllText(fileName);
            SourceText st2 = SourceText.From(allfile, new System.Uri(f.FullName));
            var tokens = StepParsing.ParseTokens(st2);

            // floats
            var toCheck = tokens.FirstOrDefault(x => x.Kind == SyntaxKind.StepFloat);
            Assert.Equal(firstFloat, toCheck.Value);
            toCheck = tokens.LastOrDefault(x => x.Kind == SyntaxKind.StepFloat);
            Assert.Equal(lastFloat, toCheck.Value);

            // ints
            toCheck = tokens.FirstOrDefault(x => x.Kind == SyntaxKind.StepInteger);
            Assert.Equal(firstInt, toCheck.Value);
            toCheck = tokens.LastOrDefault(x => x.Kind == SyntaxKind.StepInteger);
            Assert.Equal(lastInt, toCheck.Value);

            // strings
            toCheck = tokens.FirstOrDefault(x => x.Kind == SyntaxKind.StepString);
            Assert.Equal(firstString, toCheck.Value);
            toCheck = tokens.LastOrDefault(x => x.Kind == SyntaxKind.StepString);
            Assert.Equal(lastString, toCheck.Value);
        }
    }
}
