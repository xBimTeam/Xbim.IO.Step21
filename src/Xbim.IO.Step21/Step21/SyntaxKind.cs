namespace Xbim.IO.Step21
{
    /// <summary>
    /// Classifies the components of a STEP syntax
    /// </summary>
    public enum SyntaxKind
    {
        // banners prepared with https://www.patorjk.com/software/taag/#p=display&f=Standard&t=Step%20parts
        /*
          _____     _       _       
         |_   _| __(_)_   _(_) __ _ 
           | || '__| \ \ / / |/ _` |
           | || |  | |\ V /| | (_| |
           |_||_|  |_| \_/ |_|\__,_|
                                     
         */

        /// <summary>
        /// Unexpected data configuration.
        /// </summary>
        BadTokenTrivia,
        /// <summary>
        /// Meaningless whitespace between tokens
        /// </summary>
        WhitespaceTrivia,
        /// <summary>
        /// Comments in the data
        /// </summary>
        MultiLineCommentTrivia,

        /*
          _____     _                  
         |_   _|__ | | _____ _ __  ___ 
           | |/ _ \| |/ / _ \ '_ \/ __|
           | | (_) |   <  __/ | | \__ \
           |_|\___/|_|\_\___|_| |_|___/

         */

        /// <summary>
        /// EOF
        /// </summary>
        EndOfFileToken,
        /// <summary>
        /// #
        /// </summary>
        HashToken,
        /// <summary>
        /// =
        /// </summary>
        EqualsToken,
        /// <summary>
        /// (
        /// </summary>
        OpenParenthesisToken,
        /// <summary>
        /// )
        /// </summary>
        CloseParenthesisToken,
        /// <summary>
        /// ;
        /// </summary>
        SemiColonToken,
        /// <summary>
        /// ,
        /// </summary>
        CommaToken,

        /*
          _  __                                _     
         | |/ /___ _   ___      _____  _ __ __| |___ 
         | ' // _ \ | | \ \ /\ / / _ \| '__/ _` / __|
         | . \  __/ |_| |\ V  V / (_) | | | (_| \__ \
         |_|\_\___|\__, | \_/\_/ \___/|_|  \__,_|___/
                   |___/                                      
         */

        /// <summary>
        /// entire file level used in the <see cref="StepSyntax"/> class.
        /// </summary>
        StepFileKeyword,
        /// <summary>
        /// Files should start with STEP or ISO (e.g. ISO-10303-21;)
        /// </summary>
        StepOrIsoStartKeyword,
        /// <summary>
        /// Files should end with END-STEP or END-ISO (e.g. END-ISO-10303-21;)
        /// </summary>
        StepOrIsoEndKeyword,
        /// <summary>
        /// encounterd the HEADER; token, which will be closed by ENDSEC;
        /// </summary>
        StepStartHeaderSectionKeyword,
        /// <summary>
        /// encounterd the ENDSEC; token that closes an <see cref="StepStartHeaderSectionKeyword"/> or <see cref="StepStartDataSectionKeyword"/>
        /// </summary>
        StepEndSectionKeyword,
        /// <summary>
        /// encounterd the DATA; token, which will be closed by ENDSEC;. See <see cref="StepEndSectionKeyword"/>.
        /// </summary>
        StepStartDataSectionKeyword,

        /*
         __     __    _                 
         \ \   / /_ _| |_   _  ___  ___ 
          \ \ / / _` | | | | |/ _ \/ __|
           \ V / (_| | | |_| |  __/\__ \
            \_/ \__,_|_|\__,_|\___||___/
                                         
         */
        /// <summary>
        /// A string value, encoded in step
        /// </summary>
        StepString,
        /// <summary>
        /// An hex value, encoded in step
        /// </summary>
        StepHex,
        /// <summary>
        /// An integer value, encoded in step
        /// </summary>
        StepInteger, // 1, +1, -1090
        /// <summary>
        /// a float value, encoded in step
        /// </summary>
        StepFloat,
        /// <summary>
        /// the undefined value, encoded in step
        /// </summary>
        StepUndefined,
        /// <summary>
        /// arguments being overridden with the * token
        /// </summary>
        StepOverride,
        /// <summary>
        /// A boolean value, encoded in step
        /// </summary>
        StepBoolean,
        /// <summary>
        /// An enumeration value, encoded in step
        /// </summary>
        StepEnumeration,
        /// <summary>
        /// Unexpected argument error
        /// </summary>
        StepArgumentError,
        /*
          ____  _                                _       
         / ___|| |_ ___ _ __    _ __   __ _ _ __| |_ ___ 
         \___ \| __/ _ \ '_ \  | '_ \ / _` | '__| __/ __|
          ___) | ||  __/ |_) | | |_) | (_| | |  | |_\__ \
         |____/ \__\___| .__/  | .__/ \__,_|_|   \__|___/
                       |_|     |_|                       
         */
        /// <summary>
        /// The name of an instance being created
        /// </summary>
        StepIdentifierToken,
        /// <summary>
        /// Entity label of an instance being declared
        /// </summary>
        StepIdentityToken, // #123 when used to define an entity
        /// <summary>
        /// When using the fast parsing option the fields within an instance are ignored
        /// #12=IfcStuff(IGNORED);
        /// </summary>
        StepEntityAssignmentFast, // #12=IfcStuff( this is ignored, but consumed );
        /// <summary>
        /// When using the full parsing option the fields within an instance are parsed
        /// #12=IfcStuff(CONSIDERED);
        /// </summary>
        StepEntityAssignment, // #12=IfcStuff(this is parsed);
        /// <summary>
        /// Instantiation of an entity either persisted or not
        /// </summary>
        StepEntity, // e.g. IFCDIRECTION((1.,0.,0.))  <-- no termination with ";"
        /// <summary>
        /// Entity label of an instance being referenced
        /// </summary>
        StepEntityReference, // eg. #123 when used to point to another entity
        /// <summary>
        /// Comma separated parts within paranthesis
        /// </summary>
        StepArgumentsList, // eg. (*,$,1.0)
    }
}