using System;
using System.Collections.Generic;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// An entity type and its arguments IFCXXX(ARG1, ARG2, ..., ARGn)
    /// </summary>
    public class StepEntitySyntax : SyntaxNode
    {
        /// <summary>
        /// The class name of the entity in the model (i.e. express class name)
        /// </summary>
        public readonly SyntaxToken ExpressType;
        
        private readonly StepAttributeListSyntax _attributes;

        /// <summary>
        /// List of explicit attributes in the entity.
        /// The parser does not provide access to derived and inverse attributes.
        /// See https://en.wikipedia.org/wiki/EXPRESS_(data_modeling_language)#Entity-Attribute
        /// </summary>
        public IEnumerable<SyntaxNode> Attributes => _attributes.Attributes;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepEntitySyntax(Uri syntaxTree, SyntaxToken type, StepAttributeListSyntax attributeList)
            : base(syntaxTree)
        {
            ExpressType = type;
            _attributes = attributeList;
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
            yield return ExpressType;
            yield return _attributes;
        }
    }
}
