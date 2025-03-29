using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodData(string? Name, string? TypeName, ImmutableArray<ValidatorMethodCandidateData> MethodCandidates) {
    public readonly ImmutableArray<ValidatorMethodCandidateData> GetValidMethodCandidates() {
        var validMethodCandidates = new List<ValidatorMethodCandidateData>();

        foreach (var candidate in MethodCandidates) {
            if (candidate.IsValid) {
                validMethodCandidates.Add(candidate);
            }
        }

        return ImmutableArray.CreateRange(validMethodCandidates);
    }

    public void WriteSource(StringBuilder sourceBuilder) {
        var method = MethodCandidates.Single(methodCandidate => methodCandidate.IsValid);

        sourceBuilder.Append(Name)
            .Append("(");

        switch (method.FirstParameterType) {
            case ParameterType.Object:
                sourceBuilder.Append(ValidatorMethodConstants.ValueParameterName);
                break;
            case ParameterType.ValidationContext:
                sourceBuilder.Append(ValidatorMethodConstants.ValidationContextParameterName);
                break;
        }

        switch (method.SecondParameterType) {
            case ParameterType.Object:
                sourceBuilder.Append(", ").Append(ValidatorMethodConstants.ValueParameterName);
                break;
            case ParameterType.ValidationContext:
                sourceBuilder.Append(", ").Append(ValidatorMethodConstants.ValidationContextParameterName);
                break;
        }

        sourceBuilder.Append(")");
    }
}
