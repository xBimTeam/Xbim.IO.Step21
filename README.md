# Xbim.IO.Step21

Fast Express STEP part 21 parsing in .NET

## Description

We want to make xbim parsing better, faster and stronger, so we have hand written a custom parser in C# instead of relying on
the one generated by GPPG.

This is a basic implementation at the moment, but does parse sample models with no problems, and appears much faster and
maintainable than the old version.

## How to

Initial guidance for the use of the library can be found in the Xbim.Extract sample
application in the solution.
As we investigate the use of this library in the main libraries of xbim the APIs profile might
change substantially and we will use the experience to make the documentation richer.

Feel free to
[add a new issue](https://github.com/xBimTeam/Xbim.IO.Step21/issues/new?body=Hello%20%40CBenghi%2C)
for any questions, happy to help there.

## Done

- [x] Solve the int32 limit of file parsing with GPPG.
- [x] Make it fast.
- [x] Reduce and simplify public API profile.
- [x] Add XML documentation comments to all public members.
- [x] Fire events when relevant data found while parsing.
- [x] Stripping features added to Xbim.Extract
- [x] Stripping features extended to inject spatial relationships to site and project

## Todo

- [ ] Should we retain SyntaxTrivia (e.g. spaces and comments)? They are currently dropped when parsing.
- [ ] Provide generic access to parsing via custom ISouceText implementations.

## License

The code is published under the CDDL-1.0 license. Additional licensing options are available on request.

## Acknowledgements

The author is grateful to Immo Landwerth (@terrajobst) for the exceptional resources provided in the 
[Minsk project](https://github.com/terrajobst/minsk), that largely informed the parser's architecture.

## Contact

For questions or problems on the library you can
[reference me in a new issue](https://github.com/xBimTeam/Xbim.IO.Step21/issues/new?body=Hello%20%40CBenghi%2C)
or email claudio@xbim.it for consultancy requests.
