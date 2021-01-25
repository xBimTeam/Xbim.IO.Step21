using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Xbim.IO.Step21.Tests")]

namespace Xbim.IO.Step21
{
    public delegate void NewHeaderEntity(StepEntitySyntax headerEntity);
    public delegate void NewFastAssignment(StepFastEntityAssignmentSyntax fastassignment);
    public delegate void NewAssignment(StepEntityAssignmentSyntax assignment);
    
    internal sealed class Parser
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly Uri _source;
        readonly Lexer _lexer;

        public Parser(ISourceText syntaxTree) 
        {
            _lexer = new Lexer(syntaxTree);
            Current = FirstNonTrivia();
            _source = syntaxTree.Source;
        }

        private SyntaxToken FirstNonTrivia()
        {
            var c = _lexer.Lex();
            while (c.Kind.IsTrivia()) // EOF is not trivia so it always exits
            {
                c = _lexer.Lex();
            }
            return c;
        }

        public DiagnosticBag Diagnostics => new DiagnosticBag(_lexer.Diagnostics, _diagnostics);

        private SyntaxToken Current { get; set; }

        private SyntaxToken NextToken()
        {
            var current = Current;
            Current = FirstNonTrivia();
            return current;
        }


        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind, kind);
            return new SyntaxToken(_source, kind, Current.Position, null, null);
        }


        public bool QuickParse { get; set; } = false;


        public void ParseStepWithEvents(NewHeaderEntity? head, NewAssignment? assignment)
        {
            // Start of step format
            var stepStart = MatchToken(SyntaxKind.StepOrIsoStartKeyword);
            var headerStart = MatchToken(SyntaxKind.StepStartHeaderSectionKeyword);

            while (Current.Kind == SyntaxKind.IdentifierToken)
            {
                var headerEntity = ParseStepEntity();
                head?.Invoke(headerEntity);
                _ = MatchToken(SyntaxKind.SemiColonToken);
            }
            var endH = MatchToken(SyntaxKind.StepEndSectionKeyword);
            var startData = MatchToken(SyntaxKind.StepStartDataSectionKeyword);           
            while (Current.Kind == SyntaxKind.StepIdentityToken)
            {
                assignment?.Invoke(ParseStepEntityAssignment());
            }
            _ = MatchToken(SyntaxKind.StepEndSectionKeyword);
            _ = MatchToken(SyntaxKind.StepOrIsoEndKeyword);
        }

        public void ParseStepWithEvents(NewHeaderEntity? head, NewFastAssignment? assignment)
        {
            // Start of step format
            var stepStart = MatchToken(SyntaxKind.StepOrIsoStartKeyword);
            var headerStart = MatchToken(SyntaxKind.StepStartHeaderSectionKeyword);

            while (Current.Kind == SyntaxKind.IdentifierToken)
            {
                var headerEntity = ParseStepEntity();
                head?.Invoke(headerEntity);
                _ = MatchToken(SyntaxKind.SemiColonToken);
            }
            var endH = MatchToken(SyntaxKind.StepEndSectionKeyword);
            var startData = MatchToken(SyntaxKind.StepStartDataSectionKeyword);
            while (Current.Kind == SyntaxKind.StepIdentityToken)
            {
                assignment?.Invoke(ParseFastStepEntityAssignment());
            }
            _ = MatchToken(SyntaxKind.StepEndSectionKeyword);
            _ = MatchToken(SyntaxKind.StepOrIsoEndKeyword);
        }


        public StepSyntax ParseStep()
        {
            // Start of step format
            _ = MatchToken(SyntaxKind.StepOrIsoStartKeyword);
            _ = MatchToken(SyntaxKind.StepStartHeaderSectionKeyword);

            var headerEntities = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind == SyntaxKind.IdentifierToken)
            {
                var headerEntity = ParseStepEntity();
                headerEntities.Add(headerEntity);
                _ = MatchToken(SyntaxKind.SemiColonToken);
            }

            _ = MatchToken(SyntaxKind.StepEndSectionKeyword);
            _ = MatchToken(SyntaxKind.StepStartDataSectionKeyword);
            var DataAssingments = ImmutableArray.CreateBuilder<SyntaxNode>();
            if (QuickParse) // test only once, then do the loop
            {
                while (Current.Kind == SyntaxKind.StepIdentityToken)
                {
                    DataAssingments.Add(ParseFastStepEntityAssignment());
                }
            }
            else
            {
                while (Current.Kind == SyntaxKind.StepIdentityToken)
                {
                    DataAssingments.Add(ParseStepEntityAssignment());
                }
            }
            _ = MatchToken(SyntaxKind.StepEndSectionKeyword);
            var end = MatchToken(SyntaxKind.StepOrIsoEndKeyword);
            return new StepSyntax(_source,
                headerEntities.ToImmutableArray(),
                DataAssingments.ToImmutableArray(),
                end
                );
        }

        private StepFastEntityAssignmentSyntax ParseFastStepEntityAssignment()
        {
            var identity = MatchToken(SyntaxKind.StepIdentityToken);
            _ = MatchToken(SyntaxKind.EqualsToken);
            var type = MatchToken(SyntaxKind.IdentifierToken);
            _ = MatchToken(SyntaxKind.OpenParenthesisToken);
            SyntaxToken? firstString = null;
            if (Current.Kind == SyntaxKind.StringToken)
                firstString = MatchToken(SyntaxKind.StringToken);

            _lexer.IgnoreValues = true;
            while (Current.Kind != SyntaxKind.SemiColonToken)
            {
                NextToken(); // comments and strings are dealt by the lexer.
            }
            _ = MatchToken(SyntaxKind.SemiColonToken);
            _lexer.IgnoreValues = false;
            // Debug.WriteLine($"{identity}={type}");
            return new StepFastEntityAssignmentSyntax(_source, identity, type, firstString);
        }


        private StepEntityAssignmentSyntax ParseStepEntityAssignment()
        {
            var identity = MatchToken(SyntaxKind.StepIdentityToken);
            _ = MatchToken(SyntaxKind.EqualsToken);
            var entity = ParseStepEntity();
            _ = MatchToken(SyntaxKind.SemiColonToken);
            return new StepEntityAssignmentSyntax(_source, identity, entity);
        }

        private StepEntitySyntax ParseStepEntity()
        {
            // todo: complex should start here
            var type = MatchToken(SyntaxKind.IdentifierToken);
            var argumentList = ParseStepArgumentList();
            return new StepEntitySyntax(_source, type, argumentList);
        }

        private StepArgumentListSyntax ParseStepArgumentList()
        {
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            var parseNextArgument = true;
            while (parseNextArgument &&
                   Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var arg = ParseStepArgument();
                nodesAndSeparators.Add(arg);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }
            var arguments = new SeparatedSyntaxList<SyntaxNode>(nodesAndSeparators.ToImmutable());

            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new StepArgumentListSyntax(_source, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SyntaxNode ParseStepArgument()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.StepIdentityToken:
                case SyntaxKind.StepInteger:
                case SyntaxKind.StepFloat:
                case SyntaxKind.StringToken:
                case SyntaxKind.StepBoolean:
                case SyntaxKind.StepEnumeration:
                case SyntaxKind.HexToken:
                case SyntaxKind.StepUndefined:
                case SyntaxKind.StepOverride:
                    return NextToken();
                case SyntaxKind.OpenParenthesisToken:
                    return ParseStepArgumentList();
                case SyntaxKind.IdentifierToken:
                    return ParseStepEntity();
            }

            // todo: should we consume until we get a state that is ok for the calling parsers?
            // e.g. arrive to the next expected token to allow the most faithful resume of parsing?
            //
            // new StepArgumentErrorSyntax consumes Current
            // 
            _diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind, "StepArgument");
            return new StepArgumentErrorSyntax(_source, Current);
        }   
    }
}