namespace Xbim.IO.Step21
{
    public enum SyntaxKind
    {
        // Trivia
        BadTokenTrivia,
        WhitespaceTrivia,
        MultiLineCommentTrivia,

        // Tokens
        EndOfFileToken,
        StepIdentityToken, 
        StringToken,
        HexToken,
        HashToken,
        EqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        SemiColonToken,
        CommaToken,
        IdentifierToken,

        // Keywords
        StepFileKeyword,
        StepOrIsoStartKeyword,
        StepOrIsoEndKeyword,
        StepStartHeaderSectionKeyword,
        StepEndSectionKeyword,
        StepStartDataSectionKeyword,

        // stepVals
        StepInteger, // 1, +1, -1090
        StepFloat,
        StepUndefined,
        StepOverride,
        StepBoolean,
        StepEnumeration,
        StepArgumentError,

        // StepParts
        StepEntityAssignmentFast, // #12=IfcStuff( this is ignored, but consumed );
        StepEntityAssignment, // #12=IfcStuff( this is parsed);
        StepEntity, // e.g. IFCDIRECTION((1.,0.,0.))  <-- no termination with ";"
        StepEntityReference, // eg. #123 when used to point to another enity
        StepArgumentsList, // eg. (*,$,1.0)
    }
}