using System.Collections.Generic;
using System.Linq;

using Microsoft;
using Microsoft.CodeAnalysis;

namespace PSAsync.Analyzer
{
    internal static class NamedTypeSymbolExtensions
    {
        public static string GetFullName(
            this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            var symbolNames = new List<string> { symbol.Name };

            var containingType = symbol.ContainingType;

            while (containingType != null)
            {
                symbolNames.Add($"{containingType.Name}+");

                if (containingType.ContainingType is null)
                {
                    break;
                }

                containingType = containingType.ContainingType;
            }

            var containingNamespace = (containingType ?? symbol).ContainingNamespace;

            while (containingNamespace != null)
            {
                if (containingNamespace.IsGlobalNamespace)
                {
                    break;
                }

                symbolNames.Add($"{containingNamespace.Name}.");

                containingNamespace = containingNamespace.ContainingNamespace;
            }

            symbolNames.Reverse();

            return string.Join("", symbolNames);
        }

        public static bool IsCmdletClass(
            this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            if (symbol.TypeKind != TypeKind.Class)
            {
                return false;
            }

            string fullName = GetFullName(symbol);

            return
                fullName == "System.Management.Automation.Cmdlet" ||
                fullName == "System.Management.Automation.PSCmdlet";
        }

        public static bool IsAsyncCmdletClass(
            this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            if (symbol.BaseType is null)
            {
                return false;
            }

            if (!IsCmdletClass(symbol.BaseType))
            {
                return false;
            }

            return symbol.Interfaces.Any(IsAsyncCmdletInterface);
        }

        public static bool IsAsyncCmdletInterface(
            this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            if (symbol.TypeKind != TypeKind.Interface)
            {
                return false;
            }

            var fullName = symbol.GetFullName();

            return fullName == "PSAsync.IAsyncCmdlet";
        }

        public static IEnumerable<AttributeData> GetAttributes(
            this INamedTypeSymbol symbol,
            string attributeTypeFullName)
        {
            Requires.NotNull(symbol, nameof(symbol));

            foreach (var attribute in symbol.GetAttributes())
            {
                var attributeType = attribute.AttributeClass;

                if (attributeType is null)
                {
                    continue;
                }

                string fullName = attributeType.GetFullName();

                if (fullName == attributeTypeFullName)
                {
                    yield return attribute;
                }
            }
        }

        public static bool TryGetCmdletAttribute(
            this INamedTypeSymbol symbol,

            // [MaybeNullWhen(false)]
            out AttributeData attribute)
        {
            Requires.NotNull(symbol, nameof(symbol));

            attribute = null;

            var attributes = symbol
                .GetAttributes("System.Management.Automation.CmdletAttribute")
                .ToArray();

            if (attributes.Any())
            {
                attribute = attributes[0];
                return true;
            }

            return false;
        }

        public static bool HasCmdletAttribute(
            this INamedTypeSymbol symbol)
        {
            Requires.NotNull(symbol, nameof(symbol));

            return symbol.TryGetCmdletAttribute(out _);
        }

        public static IMethodSymbol GetCmdletMethodOverride(
            this INamedTypeSymbol cmdletClassSymbol,
            string methodName)
        {
            Requires.NotNull(cmdletClassSymbol, nameof(cmdletClassSymbol));

            var methodSymbol = cmdletClassSymbol
                .GetMembers(methodName)
                .Select(x => (IMethodSymbol)x)
                .Where(x =>
                    x.IsOverride &&
                    x.Parameters.Length == 0 &&
                    x.ReturnsVoid &&
                    x.DeclaredAccessibility == Accessibility.Protected &&
                    x.OverriddenMethod.ContainingType.IsCmdletClass())
                .SingleOrDefault();

            return methodSymbol;
        }

        public static IMethodSymbol GetCmdletStopProcessing(
            this INamedTypeSymbol cmdletClassSymbol)
        {
            Requires.NotNull(cmdletClassSymbol, nameof(cmdletClassSymbol));

            return GetCmdletMethodOverride(cmdletClassSymbol, "StopProcessing");
        }
    }
}
