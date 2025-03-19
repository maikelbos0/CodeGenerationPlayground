using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

// TODO turn into some other struct
public record struct ValidatorMethodData(string? Name, ImmutableArray<MethodDeclarationSyntax> CandidateMethods);
