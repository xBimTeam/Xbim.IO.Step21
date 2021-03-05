using Xbim.IO.Step21.Text;
using System.IO;
using Xunit;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

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

        [Theory]
        [InlineData(@"TestFiles\Minimal.ifc")]
        public void SameParse(string fileName)
        {
            var sFast = SourceText.From(new FileInfo(fileName));
            var sFull = SourceText.From(new FileInfo(fileName));
            int headerCount = 0;
            void OnHeaderFound(StepEntitySyntax headerEntity)
            {
                headerCount++;
            }
            List<needMatch> fast = new List<needMatch>();
            void OnBareEntityFound(StepEntityAssignmentBareSyntax assignment)
            {
                var found = new needMatch
                {
                    label = Convert.ToInt32(assignment.Identity.Value),
                    type = assignment.ExpressType.Text,
                    guid = assignment.FirstString?.Value.ToString()
                };
                fast.Add(found);
                
            }
            List<needMatch> full = new List<needMatch>();
            void OnFullEntityFound(StepEntityAssignmentSyntax assignment)
            {
                string t = null;
                var first = assignment.Entity.Attributes.FirstOrDefault();
                if (first.Kind == SyntaxKind.StepString && first is SyntaxToken tok)
                {
                    t = tok.Value.ToString();
                }
                var found = new needMatch
                {
                    label = Convert.ToInt32(assignment.Identity.Value),
                    type = assignment.Entity.ExpressType.Text,
                    guid = t
                };
                full.Add(found);
            }

            headerCount = 0;
            var fastRes = StepParsing.ParseWithEvents(sFast, OnHeaderFound, OnBareEntityFound);
            var fastCount = headerCount;

            headerCount = 0;
            var fullRes = StepParsing.ParseWithEvents(sFull, OnHeaderFound, OnFullEntityFound);
            var fullCount = headerCount;

            Assert.Equal(headerCount, fullCount);
            for (int i = 0; i < fast.Count; i++)
            {
                Assert.Equal(fast[i], full[i]);
            }
        }

        private class needMatch : IEquatable<needMatch>
        {
            public int label;
            public string type;
            public string guid;

            public bool Equals([AllowNull] needMatch other)
            {
                if (other == null)
                    return false;
                if (!label.Equals(other.label))
                    return false;
                if (!type.Equals(other.type))
                    return false;
                if (guid == null)
                {
                    if (other.guid != null)
                        return false;
                    else
                        return true;
                }
                if (!guid.Equals(other.guid))
                    return false;
                return true;
            }
        }

    }
}
