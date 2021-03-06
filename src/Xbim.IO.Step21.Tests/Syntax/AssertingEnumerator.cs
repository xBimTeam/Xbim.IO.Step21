using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Xbim.IO.Step21.Tests.CodeAnalysis.Syntax
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly IEnumerator<StepNode> _enumerator;
        private bool _hasErrors;

        public AssertingEnumerator(StepNode node)
        {
            _enumerator = Flatten(node).GetEnumerator();
        }

        private bool MarkFailed()
        {
            _hasErrors = true;
            return false;
        }

        public void Dispose()
        {
            if (!_hasErrors)
                Assert.False(_enumerator.MoveNext());

            _enumerator.Dispose();
        }

        private static IEnumerable<StepNode> Flatten(StepNode node)
        {
            var stack = new Stack<StepNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;

                foreach (var child in n.GetChildren().Reverse())
                    stack.Push(child);
            }
        }

        public void AssertNode(StepKind kind)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.IsNotType<StepToken>(_enumerator.Current);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertToken(StepKind kind, string text)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                var token = Assert.IsType<StepToken>(_enumerator.Current);
                Assert.Equal(text, token.Text);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }
    }
}
