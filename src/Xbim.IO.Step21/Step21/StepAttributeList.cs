using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Argument lists of an entity
    /// E.G.: (ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepAttributeList : StepNode
    {
        
        private readonly StepToken _openParenthesisToken;
        /// <summary>
        /// Attributes defined in the list
        /// </summary>
        public readonly SeparatedNodeList<StepNode> Attributes;
        private readonly StepToken _closeParenthesisToken;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepAttributeList(Uri syntaxTree, StepToken openParenthesisToken, SeparatedNodeList<StepNode> arguments, StepToken closeParenthesisToken)
            : base (syntaxTree)
        {
            this._openParenthesisToken = openParenthesisToken;
            this.Attributes = arguments;
            this._closeParenthesisToken = closeParenthesisToken;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepArgumentsList;

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<StepNode> GetChildren()
        {
            yield return _openParenthesisToken;
            foreach (var argument in Attributes)
            {
                yield return argument;
            }
            yield return _closeParenthesisToken;
        }
    }
}