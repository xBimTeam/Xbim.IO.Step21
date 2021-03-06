using Xbim.IO.Step21.Step21.Text;
using Xbim.IO.Step21.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Xbim.IO.Step21.Tests.Spans
{
    public class BufferedLexerTests
    {
        [Theory]
        [InlineData(@"TestFiles\Duplex_A.subset.ifc")]
        [InlineData(@"TestFiles\Numeric.ifc")]
        public void ReadsSameThanSourceText(string filename)
        {
            var f = new FileInfo(filename);

            // ///////////////////  COPY & PASTE RISK ////////// ///
            // this function is setting the buffer to a tiny value for testing purposes.
            // Don't do this in release, it's very slow
            using var st = new BufferedUri(f, 2);
            var tokens1 = StepParsing.ParseTokens(st, out var diagnostics).ToArray();
            var allfile = File.ReadAllText(filename);
            SourceText st2 = SourceText.From(allfile, new System.Uri(f.FullName));

            var tokens2 = StepParsing.ParseTokens(st2);

            Assert.Equal(tokens1.Length, tokens2.Length);

            var enum1 = tokens1.ToArray();
            var enum2 = tokens2.ToArray();
            for (int i = 0; i < enum1.Length; i++)
            {
                if (enum1[i].Text != enum2[i].Text)
                {
                    Debug.WriteLine($"{enum1[i].Text} != {enum2[i].Text}");
                    Debug.WriteLine($"- {enum1[i].Span}");
                    var tl = new TextLocation(st2, enum2[i].Span);
                    Debug.WriteLine($"- {tl}");
                    Assert.Equal(enum1[i].Text, enum2[i].Text);
                }
                if (enum1[i].Kind == StepKind.StepFloat)
                {
                    Debug.WriteLine($"{enum1[i].Value} {enum2[i].GetLocation()}");
                }
            }
        }

        [Theory]
        [InlineData(@"TestFiles\Duplex_A.subset.ifc")]
        [InlineData(@"TestFiles\Minimal.ifc")]
        public void CanBuffer(string filename)
        {
            var f = new FileInfo(filename);
            using var st = new BufferedUri(f);
            var res = StepParsing.Parse(st);
            Assert.False(res.Diagnostics.Any());
        }

        [Theory]
        [InlineData(@"TestFiles\Duplex_A.subset.ifc", 3, 125)]
        [InlineData(@"TestFiles\Minimal.ifc", 3, 19)]
        public void FastBuffer(string filename, int head, int ent)
        {
            var f = new FileInfo(filename);
            using var st = new BufferedUri(f);
            int entityCount = 0;
            int headerCount = 0;

            void NewHeaderEntity(StepHeaderEntity headerEntity)
            {
                headerCount++;
            }

            void NewEntityAssignment(StepEntityAssignment assignment)
            {
                entityCount++;
            }

            var res = StepParsing.ParseWithEvents(st, NewHeaderEntity, NewEntityAssignment);

            Assert.Equal(head, headerCount);
            Assert.Equal(ent, entityCount);
            Assert.False(res.Any());
        }

    }
}
