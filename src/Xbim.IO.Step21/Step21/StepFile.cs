using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// representation syntax for an entire STEP file
    /// </summary>
    public sealed partial class StepFile : StepNode
    {
        /// <summary>
        /// Default contructor
        /// </summary>
        public StepFile(Uri syntaxTree,
            ImmutableArray<StepNode> headers,
            ImmutableArray<StepNode> members,
            StepToken endOfFileToken)
            : base(syntaxTree)
        {
            Headers = headers;
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepFileKeyword;

        /// <summary>
        /// All members 
        /// </summary>
        public ImmutableArray<StepNode> Headers { get; }

        /// <summary>
        /// All members in the body section
        /// </summary>
        public ImmutableArray<StepNode> Members { get; }

        /// <summary>
        /// The token at the end of the file
        /// </summary>
        public StepToken EndOfFileToken { get; }

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<StepNode> GetChildren()
        {
            foreach (var m in Members)
                yield return m;
            yield return EndOfFileToken;
        }
    }
}