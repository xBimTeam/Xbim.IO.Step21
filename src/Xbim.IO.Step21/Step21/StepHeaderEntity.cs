using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    public class StepHeaderEntity : StepNode
    {
        

        public StepHeaderEntity(Uri source, StepEntity entity, StepToken closing) : base(source)
        {
            Entity = entity;
            ClosingSemicolon = closing;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepHeaderEntity;

        /// <summary>
        /// The value of the entity assigned
        /// </summary>
        public StepEntity Entity { get; }

        /// <summary>
        /// The token that ends the assignment
        /// </summary>
        public StepToken ClosingSemicolon { get; }

        public override IEnumerable<StepNode> GetChildren()
        {
            yield return Entity;
            yield return ClosingSemicolon;
        }
    }
}
