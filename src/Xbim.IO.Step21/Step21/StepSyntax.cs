using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xbim.IO.Step21
{
    // we are hacking this class to build ifc parsing up from the block level.
    //
    public sealed partial class StepSyntax : SyntaxNode
    {
        public StepSyntax(Uri syntaxTree,
            ImmutableArray<SyntaxNode> headers,
            ImmutableArray<SyntaxNode> members,
            SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Headers = headers;
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.StepFileKeyword;

        public ImmutableArray<SyntaxNode> Headers { get; }
        public ImmutableArray<SyntaxNode> Members { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var m in Members)
                yield return m;
            yield return EndOfFileToken;
        }
    }
}