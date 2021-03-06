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
        private delegate void ParseHandler(ISourceText sourceText, out StepFile root, out ImmutableArray<Diagnostic> diagnostics);

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
        public StepFile Root { get; }

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

        private static void Parse(ISourceText sourceText, out StepFile root, out ImmutableArray<Diagnostic> diagnostics)
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


        internal static ImmutableArray<StepToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        internal static ImmutableArray<StepToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out diagnostics);
        }

        internal static ImmutableArray<StepToken> ParseTokens(SourceText text)
        {
            return ParseTokens(text, out _); // ignoring diagnostics
        }

        internal static ImmutableArray<StepToken> ParseTokens(ISourceText text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var tokens = new List<StepToken>();
            void ParseTokens(ISourceText st, out StepFile root, out ImmutableArray<Diagnostic> d)
            {
                var l = new Lexer(st);
                while (true)
                {
                    var token = l.Lex();
                    if (token.Kind == StepKind.EndOfFileToken)
                    {
                        root = new StepFile(st.Source, ImmutableArray<StepNode>.Empty, ImmutableArray<StepNode>.Empty, token);
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
        /// Static helper for full Entity parsing from string
        /// </summary>
        /// <param name="express">Step21 string of entity</param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static StepEntityAssignment ParseEntityAssignment(string express, Uri reference)
        {
            if (reference == null)
                reference = new Uri("http://undefined/");
            
            var sourceText = SourceText.From(express, reference);
            var p = new Parser(sourceText);
            return p.ParseStepEntityAssignment();
        }

        /// <summary>
        /// Static helper for full token parsing
        /// </summary>
        public static IEnumerable<Diagnostic> ParseWithEvents(ISourceText st, NewHeaderEntity newHeader, NewAssignment newAssignment)
        {
            var p = new Parser(st);
            p.ParseStepWithEvents(newHeader, newAssignment);
            return p.Diagnostics;
        }

        /// <summary>
        /// Static helper for fast but simplified token parsing
        /// </summary>
        public static IEnumerable<Diagnostic> ParseWithEvents(ISourceText st, NewHeaderEntity newHeader, NewAssignmentIgnoreAttributes newFastAssignment)
        {
            Parser p = new(st);
            p.ParseStepWithEvents(newHeader, newFastAssignment);
            return p.Diagnostics;
        }
    }
}