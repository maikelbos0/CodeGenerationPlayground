namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodCandidateData(ParameterType FirstParameterType, ParameterType SecondParameterType, bool IsStatic, bool HasValidSignature, bool IsAccessible, bool IsGeneric);
