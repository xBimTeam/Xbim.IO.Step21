using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// An entity type and its arguments IFCXXX(ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepEntitySyntax : SyntaxNode
    {
        private readonly SyntaxToken _type;
        private readonly StepArgumentListSyntax _argumentList;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntitySyntax(Uri syntaxTree, SyntaxToken type, StepArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            _type = type;
            _argumentList = argumentList;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override SyntaxKind Kind => SyntaxKind.StepEntity;

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return _type;
            yield return _argumentList;
        }
    }
}
