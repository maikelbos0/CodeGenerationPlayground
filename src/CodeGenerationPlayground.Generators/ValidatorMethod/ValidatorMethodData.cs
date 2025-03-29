using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public record struct ValidatorMethodData(string? TypeName, string? PropertyName, string? MethodName, ImmutableArray<ValidatorMethodCandidateData> MethodCandidates) {
    public readonly ImmutableArray<ValidatorMethodCandidateData> GetValidMethodCandidates() {
        var validMethodCandidates = new List<ValidatorMethodCandidateData>();

        foreach (var candidate in MethodCandidates) {
            if (candidate.IsValid) {
                validMethodCandidates.Add(candidate);
            }
        }

        return ImmutableArray.CreateRange(validMethodCandidates);
    }

    public void WriteSource(StringBuilder sourceBuilder, ref int typedInstance) {
        var method = MethodCandidates.Single(methodCandidate => methodCandidate.IsValid);

        if (method.IsStatic) {
            sourceBuilder.Append(TypeName);
        }
        else {
            sourceBuilder.Append(ValidatorMethodConstants.ValidationContextParameterName)
                .Append(".ObjectInstance is ")
                .Append(TypeName)
                .Append(" obj")
                .Append(typedInstance)
                .Append(" && obj")
                .Append(typedInstance++);
        }

        sourceBuilder.Append(".").Append(MethodName).Append("(");

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
