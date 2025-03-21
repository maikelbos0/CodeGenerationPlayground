using System.Collections.Immutable;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodData(string? Name, ImmutableArray<ValidatorMethodCandidateData> ValidatorMethodCandidates);
