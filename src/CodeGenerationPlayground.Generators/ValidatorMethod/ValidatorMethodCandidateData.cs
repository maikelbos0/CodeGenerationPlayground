namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodCandidateData(ParameterType FirstParameterType, ParameterType SecondParameterType, bool HasValidSignature, bool IsAccessible, bool IsStatic);

