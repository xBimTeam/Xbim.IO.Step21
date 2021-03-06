namespace Xbim.IO.Step21
{
    /// <summary>
    /// Convenience class defining STEP syntax information.
    /// </summary>
    public static class StepFacts
    {
        /// <summary>
        /// Helper method to deterime if a kind is of type comment
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool IsComment(this StepKind kind)
        {
            return kind == StepKind.MultiLineCommentTrivia;
        }

        /// <summary>
        /// Helper method to get kind from from special string
        /// </summary>
        public static StepKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "STEP":
                    return StepKind.StepOrIsoStartKeyword;
                case "HEADER":
                    return StepKind.StepStartHeaderSectionKeyword;
                case "ENDSEC":
                    return StepKind.StepEndSectionKeyword;
                case "DATA":
                    return StepKind.StepStartDataSectionKeyword;
                case "ENDSTEP":
                    return StepKind.StepOrIsoEndKeyword;
            }
            if (text.StartsWith("ISO"))
                return StepKind.StepOrIsoStartKeyword;
            if (text.StartsWith("END-ISO"))
                return StepKind.StepOrIsoEndKeyword;
            return StepKind.StepIdentifierToken;
        }

        /// <summary>
        /// Specific kinds have direct mapping to string
        /// </summary>
        public static string? GetText(StepKind kind)
        {
            return kind switch
            {
                StepKind.EqualsToken => "=",
                StepKind.SemiColonToken => ";",
                StepKind.OpenParenthesisToken => "(",
                StepKind.CloseParenthesisToken => ")",
                StepKind.CommaToken => ",",
                _ => null,
            };
        }

        /// <summary>
        /// Helper method to deterime if a kind is of type trivia
        /// </summary>
        public static bool IsTrivia(this StepKind kind)
        {
            return kind switch
            {
                StepKind.BadTokenTrivia
                    or StepKind.WhitespaceTrivia
                    or StepKind.MultiLineCommentTrivia => true,
                _ => false,
            };
        }

        /// <summary>
        /// Helper method to deterime if a kind is of type keyword
        /// </summary>
        public static bool IsKeyword(this StepKind kind)
        {
            return kind.ToString().EndsWith("Keyword");
        }
    }
}