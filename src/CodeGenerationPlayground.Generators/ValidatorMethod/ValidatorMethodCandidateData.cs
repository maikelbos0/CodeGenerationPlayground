namespace CodeGenerationPlayground.Generators.ValidatorMethod;

// TODO is static
public record struct ValidatorMethodCandidateData(ParameterType FirstParameterType, ParameterType SecondParameterType, bool HasValidSignature, bool IsAccessible);

