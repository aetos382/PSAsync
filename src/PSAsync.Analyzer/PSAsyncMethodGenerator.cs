using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PSAsync.Analyzer
{
    [Generator]
    public class PSAsyncMethodGenerator :
        ISourceGenerator
    {
        public void Initialize(
            InitializationContext context)
        {
        }

        public void Execute(
            SourceGeneratorContext context)
        {
            context.AddSource("Foo.cs", SourceText.From("internal class Foo {}", Encoding.UTF8));
        }
    }
}
