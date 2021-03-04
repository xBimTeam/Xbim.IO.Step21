using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Xbim.IO.Step21.Step21.Text;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Helper class to expose parsing features
    /// </summary>
    public sealed class StepParsing
    {
        private delegate void ParseHandler(ISourceText sourceText, out StepSyntax root, out ImmutableArray<Diagnostic> diagnostics);

        private StepParsing(ISourceText text, ParseHandler stepSyntaxGenerator)
        {
            stepSyntaxGenerator(text, out var root, out var diagnostics);

            Diagnostics = diagnostics;
            Root = root;
        }


        /// <summary>
        /// The diagnostic information returned from the parsing process
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Returns the root node of the file hierarchy.
        /// </summary>
        public StepSyntax Root { get; }

        /// <summary>
        /// Static metod to get the parsing from a filename
        /// </summary>
        public static StepParsing From(string fileName)
        {
            var text = File.ReadAllText(fileName);
            var uri = new Uri(fileName);
            var sourceText = SourceText.From(text, uri);
            return Parse(sourceText);
        }

        private static void Parse(ISourceText sourceText, out StepSyntax root, out ImmutableArray<Diagnostic> diagnostics)
        {
            var parser = new Parser(sourceText);
            root = parser.ParseStep();
            diagnostics = parser.Diagnostics.ToImmutableArray();
        }

        /// <summary>
        /// Parse a text directly
        /// </summary>
        public static StepParsing Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        /// <summary>
        /// Parse an <see cref="ISourceText"/> class
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static StepParsing Parse(ISourceText text)
        {
            return new StepParsing(text, Parse);
        }


        internal static ImmutableArray<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        internal static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out diagnostics);
        }

        internal static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
        {
            return ParseTokens(text, out _); // ignoring diagnostics
        }

        internal static ImmutableArray<SyntaxToken> ParseTokens(ISourceText text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var tokens = new List<SyntaxToken>();
            void ParseTokens(ISourceText st, out StepSyntax root, out ImmutableArray<Diagnostic> d)
            {
                var l = new Lexer(st);
                while (true)
                {
                    var token = l.Lex();
                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        root = new StepSyntax(st.Source, ImmutableArray<SyntaxNode>.Empty, ImmutableArray<SyntaxNode>.Empty, token);
                        break;
                    }
                    tokens.Add(token);
                }

                d = l.Diagnostics.ToImmutableArray();
            }

            var syntaxTree = new StepParsing(text, ParseTokens);
            diagnostics = syntaxTree.Diagnostics.ToImmutableArray();
            return tokens.ToImmutableArray();
        }

        /// <summary>
        /// Static helper for full token parsing
        /// </summary>
        public static IEnumerable<Diagnostic> ParseWithEvents(BufferedUri st, NewHeaderEntity newHeader, NewAssignment newAssignment)
        {
            Parser p = new Parser(st);
            p.ParseStepWithEvents(newHeader, newAssignment);
            return p.Diagnostics;
        }

        /// <summary>
        /// Static helper for fast but simplified token parsing
        /// </summary>
        public static IEnumerable<Diagnostic> ParseWithEvents(BufferedUri st, NewHeaderEntity newHeader, NewFastAssignment newAssignment)
        {
            Parser p = new Parser(st);
            p.ParseStepWithEvents(newHeader, newAssignment);
            return p.Diagnostics;
        }
    }
}