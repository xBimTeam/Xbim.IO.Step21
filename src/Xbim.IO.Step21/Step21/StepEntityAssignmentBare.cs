using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Simplified Entity assignment, does not parse the attributes of the elements (except for the first one if it's a string)
    /// E.g.: #230=IFCWALL('guidvalue', ... IGNORED ... );
    /// </summary>
    public class StepEntityAssignmentBare : StepNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntityAssignmentBare(Uri syntaxTree, StepToken identity, StepToken type, StepToken? firstString, StepToken closingSemiColon)
            : base(syntaxTree)
        {
            Identity = identity;
            ExpressType = type;
            FirstString = firstString;
            ClosingSemicolon = closingSemiColon;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepEntityAssignmentFast;

        /// <summary>
        /// the entity label of the assignment
        /// </summary>
        public StepToken Identity { get; }

        /// <summary>
        /// The type of step class being assigned
        /// </summary>
        public StepToken ExpressType { get; }

        /// <summary>
        /// If the first of the arguments is a string it gets captured here.
        /// This is useful for types that hold Guid in the first position.
        /// </summary>
        public StepToken? FirstString { get; }

        /// <summary>
        /// The token that ends the assignment
        /// </summary>
        public StepToken ClosingSemicolon { get; }

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<StepNode> GetChildren()
        {
            yield return Identity;
            yield return ExpressType;
            if (FirstString != null)
                yield return FirstString;
            yield return ClosingSemicolon;
        }
    }
}
