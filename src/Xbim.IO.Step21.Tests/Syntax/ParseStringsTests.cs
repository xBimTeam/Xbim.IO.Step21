using Xbim.IO.Step21.Text;
using System.IO;
using Xunit;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Xbim.IO.Step21.Tests.Syntax
{
    public class ParseStringsTests
    {
        [Theory]
        [InlineData(@"TestFiles\Strings.ifc")]
        public void ParseStringCases(string fileName)
        {
            var sFast = SourceText.From(new FileInfo(fileName));
            var sFull = SourceText.From(new FileInfo(fileName));
            int headerCount = 0;
            int fullElementTests = 0;

            void OnHeaderFound(StepHeaderEntity headerEntity)
            {
                headerCount++;
            }
            
            void OnBareEntityFound(StepEntityAssignmentBare assignment)
            {
            

            }
            
            void OnFullEntityFound(StepEntityAssignment assignment)
            {
                StepToken el = null;
                switch (assignment.Identity.Text)
                {
                    case "#1":
                        el = assignment.Entity.Attributes.ElementAt(1) as StepToken;
                        Assert.StartsWith("'2011'", el.Text);
                        fullElementTests++;
                        break;
                    case "#2":
                        el = assignment.Entity.Attributes.ElementAt(1) as StepToken;
                        Assert.StartsWith("'20\\S\\'11'", el.Text);
                        fullElementTests++;
                        break;
                    case "#3":
                        el = assignment.Entity.Attributes.ElementAt(1) as StepToken;
                        Assert.StartsWith("'20\\S\\''", el.Text);
                        fullElementTests++;
                        break;
                    case "#4":
                        el = assignment.Entity.Attributes.ElementAt(1) as StepToken;
                        Assert.StartsWith("'20\\Some'", el.Text);
                        fullElementTests++;
                        break;
                    case "#5":
                        el = assignment.Entity.Attributes.ElementAt(1) as StepToken;
                        Assert.StartsWith("'20\\'", el.Text);
                        fullElementTests++;
                        break;
                }
            }

            

            headerCount = 0;
            var fastRes = StepParsing.ParseWithEvents(sFast, OnHeaderFound, OnBareEntityFound);
            var fastHeaderCount = headerCount;

            headerCount = 0;
            var fullRes = StepParsing.ParseWithEvents(sFull, OnHeaderFound, OnFullEntityFound);
            var fullHeaderCount = headerCount;

            Assert.Equal(3, fastHeaderCount);
            Assert.Equal(3, fullHeaderCount);


            Assert.Equal(5, fullElementTests);            
            
        }

    }
}
