namespace Xbim.IO.Step21
{
    /// <summary>
    /// Convenience class defining STEP syntax information.
    /// </summary>
    public static class SyntaxFacts
    {
        /// <summary>
        /// Helper method to deterime if a kind is of type comment
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsComment(this SyntaxKind kind)
        {
            return kind == SyntaxKind.MultiLineCommentTrivia;
        }

        /// <summary>
        /// Helper method to get kind from from special string
        /// </summary>
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

        /// <summary>
        /// Specific kinds have direct mapping to string
        /// </summary>
        public static string? GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.OpenParenthesisToken => "(",
                SyntaxKind.CloseParenthesisToken => ")",
                SyntaxKind.CommaToken => ",",
                _ => null,
            };
        }

        /// <summary>
        /// Helper method to deterime if a kind is of type trivia
        /// </summary>
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

        /// <summary>
        /// Helper method to deterime if a kind is of type keyword
        /// </summary>
        public static bool IsKeyword(this SyntaxKind kind)
        {
            return kind.ToString().EndsWith("Keyword");
        }
    }
}