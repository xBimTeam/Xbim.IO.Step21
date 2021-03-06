using System;
using System.Collections.Generic;
using Xunit;

namespace Xbim.IO.Step21.Tests.Syntax
{
    public class SyntaxFactTests
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFact_GetText_RoundTrips(StepKind kind)
        {
            var text = StepFacts.GetText(kind);
            if (text == null)
                return;

            var tokens = StepParsing.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = (StepKind[])Enum.GetValues(typeof(StepKind));
            foreach (var kind in kinds)
                yield return new object[] { kind };
        }
    }
}
