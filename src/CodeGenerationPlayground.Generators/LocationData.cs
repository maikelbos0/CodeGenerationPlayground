using Microsoft.CodeAnalysis.Text;

namespace CodeGenerationPlayground.Generators;

public record struct LocationData(string FilePath, TextSpan TextSpan);
