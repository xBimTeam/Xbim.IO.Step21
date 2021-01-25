namespace Xbim.IO.Step21
{
    /// <summary>
    /// Convenience class defining STEP syntax information.
    /// </summary>
    public static class SyntaxFacts
    {

        public static bool IsComment(this SyntaxKind kind)
        {
            return kind == SyntaxKind.MultiLineCommentTrivia;
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "STEP":
                    return SyntaxKind.StepOrIsoStartKeyword;
                case "HEADER":
                    return SyntaxKind.StepStartHeaderSectionKeyword;
                case "ENDSEC":
                    return SyntaxKind.StepEndSectionKeyword;
                case "DATA":
                    return SyntaxKind.StepStartDataSectionKeyword;
                case "ENDSTEP":
                    return SyntaxKind.StepOrIsoEndKeyword;
            }
            if (text.StartsWith("ISO"))
                return SyntaxKind.StepOrIsoStartKeyword;
            if (text.StartsWith("END-ISO"))
                return SyntaxKind.StepOrIsoEndKeyword;
            return SyntaxKind.StepIdentifierToken;
        }

        // this has got to return null or lots of tests fail
        public static string? GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.CommaToken => ",",
                // todo: implement all missing kinds
                _ => null,
            };
        }

        public static bool IsTrivia(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.BadTokenTrivia
                    or SyntaxKind.WhitespaceTrivia
                    or SyntaxKind.MultiLineCommentTrivia => true,
                _ => false,
            };
        }

        public static bool IsKeyword(this SyntaxKind kind)
        {
            return kind.ToString().EndsWith("Keyword");
        }

        public static bool IsToken(this SyntaxKind kind)
        {
            return !kind.IsTrivia() &&
                   (kind.IsKeyword() || kind.ToString().EndsWith("Token"));
        }
    }
}