using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Xbim.IO.Step21.Step21.Text;
using Xbim.IO.Step21.Text;

namespace Xbim.IO.Step21
{
    public sealed class StepParsing
    {
        private delegate void ParseHandler(ISourceText sourceText, out StepSyntax root, out ImmutableArray<Diagnostic> diagnostics);

        private StepParsing(ISourceText text, ParseHandler stepSyntaxGenerator)
        {
            stepSyntaxGenerator(text, out var root, out var diagnostics);

            Diagnostics = diagnostics;
            Root = root;
        }

        
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public StepSyntax Root { get; }

        // todo: 2021: use better ISource than SourceText

        public static StepParsing Load(string fileName)
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

        public static StepParsing Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        public static StepParsing Parse(ISourceText text)
        {
            return new StepParsing(text, Parse);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out diagnostics);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
        {
            return ParseTokens(text, out _); // ignoring diagnostics
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(ISourceText text, out ImmutableArray<Diagnostic> diagnostics)
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

        public static IEnumerable<Diagnostic> ParseWithEvents(BufferedUri st, NewHeaderEntity newHeader, NewAssignment newAssignment)
        {
            Parser p = new Parser(st);
            p.ParseStepWithEvents(newHeader, newAssignment);
            return p.Diagnostics;
        }
        public static IEnumerable<Diagnostic> ParseWithEvents(BufferedUri st, NewHeaderEntity newHeader, NewFastAssignment newAssignment)
        {
            Parser p = new Parser(st);
            p.ParseStepWithEvents(newHeader, newAssignment);
            return p.Diagnostics;
        }
    }
}