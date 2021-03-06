using System;
using System.Collections.Generic;
using System.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// An entity type and its arguments IFCXXX(ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepEntity : StepNode
    {
        /// <summary>
        /// The class name of the entity in the model (i.e. express class name)
        /// </summary>
        public readonly StepToken ExpressType;

        /// <summary>
        /// The step wrapper of the Attributes
        /// </summary>
        public readonly StepAttributeList AttributesList;

        /// <summary>
        /// List of explicit attributes in the entity.
        /// The parser does not provide access to derived and inverse attributes.
        /// See https://en.wikipedia.org/wiki/EXPRESS_(data_modeling_language)#Entity-Attribute
        /// </summary>
        public IEnumerable<StepNode> Attributes => AttributesList.Attributes;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntity(Uri source, StepToken type, StepAttributeList attributeList)
            : base(source)
        {
            ExpressType = type;
            AttributesList = attributeList;
        }

        /// <summary>
        /// The classification of the node
        /// </summary>
        public override StepKind Kind => StepKind.StepEntity;

        /// <summary>
        /// Concrete implementation listing subcomponents of the node
        /// </summary>
        public override IEnumerable<StepNode> GetChildren()
        {
            yield return ExpressType;
            yield return AttributesList;
        }
    }
}
