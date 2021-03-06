using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Provides structure of an unexpected sequence in the data
    /// </summary>
    public class StepArgumentError : StepNode
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StepArgumentError(Uri syntaxTree, StepToken value)
            : base(syntaxTree)
        {
            Value = value;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepArgumentError;

        /// <summary>
        /// The unexpected token found
        /// </summary>
        public StepToken Value { get; }

        /// <summary>
        /// The unexpected token found is the only component of the syntax.
        /// </summary>
        public override IEnumerable<StepNode> GetChildren()
        {
            yield return Value;
        }
    }
}
