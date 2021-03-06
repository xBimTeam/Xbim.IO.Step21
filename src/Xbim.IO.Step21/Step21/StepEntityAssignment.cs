using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// A full assignment, entirely parsed
    /// E.g. #IDENTITY=ENTITY;
    /// </summary>
    public class StepEntityAssignment : StepNode
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntityAssignment(Uri syntaxTree, StepToken identity, StepEntity entity, StepToken closing)
            : base (syntaxTree)
        {
            Identity = identity;
            Entity = entity;
            ClosingSemicolon = closing;
        }

        /// <summary>
        /// The type of step class being assigned
        /// </summary>
        public StepToken ExpressType => Entity.ExpressType;

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepEntityAssignment;

        /// <summary>
        /// The entity label being assigned
        /// </summary>
        public StepToken Identity { get; }

        /// <summary>
        /// The value of the entity assigned
        /// </summary>
        public StepEntity Entity { get; }

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
            yield return Entity;
            yield return ClosingSemicolon;
        }
    }
}
