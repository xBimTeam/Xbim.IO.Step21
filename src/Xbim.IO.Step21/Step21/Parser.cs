using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

// 11e3655e576ec5a9

[assembly: InternalsVisibleTo("Xbim.IO.Step21.Tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010029a3c6da60efcb3ebe48c3ce14a169b5fa08ffbf5f276392ffb2006a9a2d596f5929cf0e68568d14ac7cbe334440ca0b182be7fa6896d2a73036f24bca081b2427a8dec5689a97f3d62547acd5d471ee9f379540f338bbb0ae6a165b44b1ae34405624baa4388404bce6d3e30de128cec379147af363ce9c5845f4f92d405ed0")] 

namespace Xbim.IO.Step21
{
    /// <summary>
    /// Triggered when encountering new header entities 
    /// </summary>
    /// <param name="headerEntityFound">data of the entity encountered</param>
    public delegate void NewHeaderEntity(StepHeaderEntity headerEntityFound);

    /// <summary>
    /// Triggered when encountering new body entities
    /// </summary>
    /// <param name="AssignmentFound">simplified data of the entity encountered</param>
    public delegate void NewAssignmentIgnoreAttributes(StepEntityAssignmentBare AssignmentFound);

    /// <summary>
    /// Triggered when encountering new body entities
    /// </summary>
    /// <param name="assignmentFound">data of the entity encountered</param>
    public delegate void NewAssignment(StepEntityAssignment assignmentFound);

    /// <summary>
    /// Triggered when encountering a parsing issue
    /// </summary>
    /// <param name="issue">data of the issue encountered</param>
    public delegate void DiagnosticIssue(Diagnostic issue);

    internal sealed class Parser
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly Uri _source;
        readonly Lexer _lexer;

        public Parser(ISourceText source) 
        {
            _lexer = new Lexer(source);
            Current = FirstNonTrivia();
            _source = source.Source;
        }

        private StepToken FirstNonTrivia()
        {
            var nextToken = _lexer.Lex();
            while (nextToken.Kind.IsTrivia()) // EOF is not trivia so it always exits
            {
                nextToken = _lexer.Lex();
            }
            return nextToken;
        }

        public DiagnosticBag Diagnostics => new(_lexer.Diagnostics, _diagnostics);

        private StepToken Current { get; set; }

        private StepToken NextToken()
        {
            var current = Current;
            Current = FirstNonTrivia();
            return current;
        }


        private StepToken MatchToken(StepKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind, kind);
            return new StepToken(_source, kind, Current.Position, null, null);
        }

        public bool QuickParse { get; set; } = false;

        public void ParseStepWithEvents(NewHeaderEntity? head, NewAssignment? assignment, DiagnosticIssue? issue)
        {
            // Start of step format
            if (issue != null)
            {
                _diagnostics.IssueEncountered += issue;
                _lexer.Diagnostics.IssueEncountered += issue;
            }
            var stepStart = MatchToken(StepKind.StepOrIsoStartKeyword);
            var headerStart = MatchToken(StepKind.StepStartHeaderSectionKeyword);

            while (Current.Kind == StepKind.StepIdentifierToken)
            {
                var headerEntity = ParseHeaderEntity();
                head?.Invoke(headerEntity);
            }
            var endH = MatchToken(StepKind.StepEndSectionKeyword);
            var startData = MatchToken(StepKind.StepStartDataSectionKeyword);           
            while (Current.Kind == StepKind.StepIdentityToken)
            {
                assignment?.Invoke(ParseStepEntityAssignment());
            }
            _ = MatchToken(StepKind.StepEndSectionKeyword);
            _ = MatchToken(StepKind.StepOrIsoEndKeyword);
            if (issue != null)
            {
                _diagnostics.IssueEncountered -= issue;
                _lexer.Diagnostics.IssueEncountered -= issue;
            }
        }

        public void ParseStepWithEvents(NewHeaderEntity? head, NewAssignmentIgnoreAttributes? assignment)
        {
            // Start of step format
            var stepStart = MatchToken(StepKind.StepOrIsoStartKeyword);
            var headerStart = MatchToken(StepKind.StepStartHeaderSectionKeyword);

            while (Current.Kind == StepKind.StepIdentifierToken)
            {
                var headerEntity = ParseHeaderEntity();
                head?.Invoke(headerEntity);
            }
            var endH = MatchToken(StepKind.StepEndSectionKeyword);
            var startData = MatchToken(StepKind.StepStartDataSectionKeyword);
            while (Current.Kind == StepKind.StepIdentityToken)
            {
                assignment?.Invoke(ParseStepEntityAssignmentBare());
            }
            _ = MatchToken(StepKind.StepEndSectionKeyword);
            _ = MatchToken(StepKind.StepOrIsoEndKeyword);
        }


        public StepFile ParseStep()
        {
            // Start of step format
            _ = MatchToken(StepKind.StepOrIsoStartKeyword);
            _ = MatchToken(StepKind.StepStartHeaderSectionKeyword);

            var headerEntities = ImmutableArray.CreateBuilder<StepNode>();
            while (Current.Kind == StepKind.StepIdentifierToken)
            {
                var headerEntity = ParseStepEntity();
                headerEntities.Add(headerEntity);
                _ = MatchToken(StepKind.SemiColonToken);
            }

            _ = MatchToken(StepKind.StepEndSectionKeyword);
            _ = MatchToken(StepKind.StepStartDataSectionKeyword);
            var DataAssingments = ImmutableArray.CreateBuilder<StepNode>();
            if (QuickParse) // test only once, then do the loop
            {
                while (Current.Kind == StepKind.StepIdentityToken)
                {
                    DataAssingments.Add(ParseStepEntityAssignmentBare());
                }
            }
            else
            {
                while (Current.Kind == StepKind.StepIdentityToken)
                {
                    DataAssingments.Add(ParseStepEntityAssignment());
                }
            }
            _ = MatchToken(StepKind.StepEndSectionKeyword);
            var end = MatchToken(StepKind.StepOrIsoEndKeyword);
            return new StepFile(_source,
                headerEntities.ToImmutableArray(),
                DataAssingments.ToImmutableArray(),
                end
                );
        }

        private StepEntityAssignmentBare ParseStepEntityAssignmentBare()
        {
            var identity = MatchToken(StepKind.StepIdentityToken);
            _ = MatchToken(StepKind.EqualsToken);
            var type = MatchToken(StepKind.StepIdentifierToken);
            _ = MatchToken(StepKind.OpenParenthesisToken);
            StepToken? firstString = null;
            if (Current.Kind == StepKind.StepString)
                firstString = MatchToken(StepKind.StepString);

            _lexer.IgnoreValues = true;
            while (Current.Kind != StepKind.SemiColonToken)
            {
                NextToken(); // comments and strings are dealt by the lexer.
            }
            // MatchToken populates the value of 'Current' token after the semicolon,
            // that's why IgnoreValues needs to false in order for the next assignment to
            // be parsed correctly.           
            _lexer.IgnoreValues = false;

            // the semicolon token is a special case that gets the text from the
            // syntaxfacts class, so it's span is correctly evaluated even if ignoreValues
            // was set when it was first parsed in the loop of NextToken() above.
            var closing = MatchToken(StepKind.SemiColonToken);
            
            Debug.WriteLine($"{identity.Text}={type.Text}");
            return new StepEntityAssignmentBare(_source, identity, type, firstString, closing);
        }


        internal StepEntityAssignment ParseStepEntityAssignment()
        {
            var identity = MatchToken(StepKind.StepIdentityToken);
            _ = MatchToken(StepKind.EqualsToken);
            var entity = ParseStepEntity();
            var closing = MatchToken(StepKind.SemiColonToken);
            return new StepEntityAssignment(_source, identity, entity, closing);
        }


        private StepHeaderEntity ParseHeaderEntity()
        {
            var ent = ParseStepEntity();
            var closing = MatchToken(StepKind.SemiColonToken);
            return new StepHeaderEntity(_source, ent, closing);
        }

        private StepEntity ParseStepEntity()
        {
            // todo: complex should start here
            var type = MatchToken(StepKind.StepIdentifierToken);
            var argumentList = ParseStepArgumentList();
            return new StepEntity(_source, type, argumentList);
        }

        private StepAttributeList ParseStepArgumentList()
        {
            var openParenthesisToken = MatchToken(StepKind.OpenParenthesisToken);
            var nodesAndSeparators = ImmutableArray.CreateBuilder<StepNode>();

            var parseNextArgument = true;
            while (parseNextArgument &&
                   Current.Kind != StepKind.CloseParenthesisToken &&
                   Current.Kind != StepKind.EndOfFileToken)
            {
                var arg = ParseStepArgument();
                nodesAndSeparators.Add(arg);

                if (Current.Kind == StepKind.CommaToken)
                {
                    var comma = MatchToken(StepKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }
            var arguments = new SeparatedNodeList<StepNode>(nodesAndSeparators.ToImmutable());

            var closeParenthesisToken = MatchToken(StepKind.CloseParenthesisToken);
            return new StepAttributeList(_source, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private StepNode ParseStepArgument()
        {
            switch (Current.Kind)
            {
                case StepKind.StepIdentityToken:
                case StepKind.StepInteger:
                case StepKind.StepFloat:
                case StepKind.StepString:
                case StepKind.StepBoolean:
                case StepKind.StepEnumeration:
                case StepKind.StepHex:
                case StepKind.StepUndefined:
                case StepKind.StepOverride:
                    return NextToken();
                case StepKind.OpenParenthesisToken:
                    return ParseStepArgumentList();
                case StepKind.StepIdentifierToken:
                    return ParseStepEntity();
            }

            // todo: should we consume until we get a state that is ok for the calling parsers?
            // e.g. arrive to the next expected token to allow the most faithful resume of parsing?
            //
            // new StepArgumentErrorSyntax consumes Current
            // 
            _diagnostics.ReportUnexpectedToken(Current.GetLocation(), Current.Kind, "StepArgument");
            return new StepArgumentError(_source, Current);
        }   
    }
}