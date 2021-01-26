using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// A full assignment, entirely parsed
    /// E.g. #IDENTITY=ENTITY;
    /// </summary>
    public class StepEntityAssignmentSyntax : SyntaxNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntityAssignmentSyntax(Uri syntaxTree, SyntaxToken identity, StepEntitySyntax entity)
            : base (syntaxTree)
        {
            Identity = identity;
            Entity = entity;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepEntityAssignment;

        /// <summary>
        /// The entity label being assigned
        /// </summary>
        public SyntaxToken Identity { get; }

        /// <summary>
        /// The value of the entity assigned
        /// </summary>
        public StepEntitySyntax Entity { get; }

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identity;
            yield return Entity;
        }
    }
}
