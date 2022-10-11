using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;

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

        /// <inheritdoc />
        public override void WritePart21(StreamWriter writer)
        {
            //bool firstArg = true; 
            _openParenthesisToken.WritePart21(writer);
            foreach (var argument in Attributes.GetWithSeparators())
            {
                //if (!firstArg)
                //{
                //    writer.Write(",");
                //    firstArg = false;
                //}
                argument.WritePart21(writer);
            }
            _closeParenthesisToken.WritePart21(writer);
        }

    }
}