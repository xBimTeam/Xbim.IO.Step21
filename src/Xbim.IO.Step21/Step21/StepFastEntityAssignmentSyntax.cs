using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Simplified Entity assignment, does not parse the attributes of the elements (except for the first one if it's a string)
    /// E.g.: #230=IFCWALL('guidvalue', ... IGNORED ... );
    /// </summary>
    public class StepFastEntityAssignmentSyntax : SyntaxNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StepFastEntityAssignmentSyntax(Uri syntaxTree, SyntaxToken identity, SyntaxToken type, SyntaxToken? firstString)
            : base(syntaxTree)
        {
            Identity = identity;
            Type = type;
            FirstString = firstString;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepEntityAssignmentFast;

        /// <summary>
        /// the entity label of the assignment
        /// </summary>
        public SyntaxToken Identity { get; }

        /// <summary>
        /// The type of step class being assigned
        /// </summary>
        public SyntaxToken Type { get; }

        /// <summary>
        /// If the first of the arguments is a string it gets captured here.
        /// This is useful for types that hold Guid in the first position.
        /// </summary>
        public SyntaxToken? FirstString { get; }

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identity;
            yield return Type;
            if (FirstString != null)
                yield return FirstString;
        }
    }
}
