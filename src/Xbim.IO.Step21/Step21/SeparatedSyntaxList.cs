using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Convenience class to manage lists
    /// </summary>
    public abstract class SeparatedSyntaxList
    {
        /// <summary>
        /// returns both separated and separator nodes.
        /// </summary>
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
    }


    /// <summary>
    /// Convenience generic class to manage lists
    /// </summary>
    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
        where T: SyntaxNode
    {
        private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="nodesAndSeparators"></param>
        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            _nodesAndSeparators = nodesAndSeparators;
        }

        /// <summary>
        /// count of separated nodes, ignores separators
        /// </summary>
        public int Count => (_nodesAndSeparators.Length + 1) / 2;

        /// <summary>
        /// Indexed accessor to separated nodes, ignores separators
        /// </summary>
        public T this[int index] => (T) _nodesAndSeparators[index * 2];

        /// <summary>
        /// Query separating elements
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SyntaxToken GetSeparator(int index)
        {
            if (index < 0 || index >= Count - 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            return (SyntaxToken) _nodesAndSeparators[index * 2 + 1];
        }

        /// <summary>
        /// returns both separated and separator nodes.
        /// </summary>
        public override ImmutableArray<SyntaxNode> GetWithSeparators() => _nodesAndSeparators;

        /// <summary>
        /// enumerator of separated nodes, ignores separators
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <summary>
        /// enumerator of separated nodes, ignores separators
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}