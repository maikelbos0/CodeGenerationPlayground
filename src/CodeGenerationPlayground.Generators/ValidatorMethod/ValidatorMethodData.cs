using System.Collections.Generic;
using System.Collections.Immutable;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodData(string? Name, ImmutableArray<string> Ancestors, ImmutableArray<ValidatorMethodCandidateData> MethodCandidates) {
    public readonly ImmutableArray<ValidatorMethodCandidateData> GetValidMethodCandidates() {
        var validMethodCandidates = new List<ValidatorMethodCandidateData>();

        foreach (var candidate in MethodCandidates) {
            if (candidate.IsValid) {
                validMethodCandidates.Add(candidate);
            }
        }

        return ImmutableArray.CreateRange(validMethodCandidates);
    }
}
