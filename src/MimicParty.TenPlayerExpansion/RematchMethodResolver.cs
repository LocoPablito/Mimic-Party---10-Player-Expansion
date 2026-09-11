using System.Reflection;

namespace Arribbaa.MimicParty.TenPlayerExpansion;

// Resolve the actual CLR signature, not a presumed zero-argument call site.
// No game method is invoked here. Unknown or ambiguous signatures fail closed.
internal static class RematchMethodResolver
{
    internal const string NetworkServiceTypeName = "Mimick.Networking.INetworkService";

    internal static MethodInfo Resolve(Type resultsScreen)
    {
        ArgumentNullException.ThrowIfNull(resultsScreen);
        var named = resultsScreen.GetMethods(BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == "LaunchRematch").ToArray();
        var supported = named.Where(m => !m.IsStatic && !m.IsAbstract && !m.IsGenericMethod &&
            m.ReturnType == typeof(void) && HasSupportedParameters(m)).ToArray();

        if (supported.Length == 1)
            return supported[0];

        string found = named.Length == 0 ? "<none>" : string.Join("; ", named.Select(Describe));
        string message = $"Expected exactly one instance void LaunchRematch() or " +
            $"LaunchRematch({NetworkServiceTypeName}) on {resultsScreen.FullName}. Found: {found}";
        if (supported.Length > 1)
            throw new AmbiguousMatchException(message);
        throw new MissingMethodException(message);
    }

    private static bool HasSupportedParameters(MethodInfo method)
    {
        ParameterInfo[] p = method.GetParameters();
        return p.Length == 0 || (p.Length == 1 && !p[0].ParameterType.IsByRef &&
            p[0].ParameterType.FullName == NetworkServiceTypeName);
    }

    internal static string Describe(MethodInfo method) =>
        $"{(method.IsStatic ? "static" : "instance")} {method.ReturnType.FullName} " +
        $"{method.DeclaringType?.FullName}.{method.Name}(" +
        string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName)) + ")";
}
