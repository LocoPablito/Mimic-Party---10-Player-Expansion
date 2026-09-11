using System.Reflection;
using System.Security.Cryptography;
using Arribbaa.MimicParty.TenPlayerExpansion;
using dnlib.DotNet;

// Tests and offline metadata inspection only; never invokes a game function.
internal static class Program
{
    private static int passed;
    private static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception("FAIL: " + label);
        passed++; Console.WriteLine("PASS: " + label);
    }
    private static void Reject<T>(Action action, string label) where T : Exception
    {
        try { action(); } catch (T) { Check(true, label); return; }
        throw new Exception("Expected " + typeof(T).Name + ": " + label);
    }
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 2 && args[0] == "--inspect") { Inspect(args[1]); return 0; }
            if (args.Length != 0) throw new ArgumentException("Use no arguments for fixtures, or --inspect <private assembly directory>");
            Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Check(!typeof(Current).GetMethods(flags).Any(m => m.Name == "LaunchRematch" && m.GetParameters().Length == 0), "old zero-parameter lookup reproduces failure");
            var current = RematchMethodResolver.Resolve(typeof(Current));
            Check(current.GetParameters().Length == 1 && current.GetParameters()[0].ParameterType.FullName == RematchMethodResolver.NetworkServiceTypeName, "current INetworkService signature resolved");
            Check(RematchMethodResolver.Resolve(typeof(Legacy)).GetParameters().Length == 0, "legacy zero-parameter shape resolved");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(WrongParameter)), "wrong parameter rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(WrongReturn)), "wrong return rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(StaticMethod)), "static method rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(GenericMethod)), "generic method rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(ByRefMethod)), "byref parameter rejected");
            Reject<AmbiguousMatchException>(() => RematchMethodResolver.Resolve(typeof(Ambiguous)), "ambiguous supported overloads rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(Absent)), "missing method rejected");
            Reject<MissingMethodException>(() => RematchMethodResolver.Resolve(typeof(Inherited)), "inherited method not accidentally selected");
            Reject<ArgumentNullException>(() => RematchMethodResolver.Resolve(null!), "null rejected");
            try { RematchMethodResolver.Resolve(typeof(WrongParameter)); }
            catch (MissingMethodException e) { Check(e.Message.Contains("System.Boolean") && e.Message.Contains("INetworkService"), "failure explains found and expected signature"); }
            Console.WriteLine($"FIX6 CONTRACT TESTS PASS: {passed}");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
    private static void Inspect(string directory)
    {
        using var runtime = ModuleDefMD.Load(Path.Combine(directory, "Mimick.Runtime.dll"));
        using var fusion = ModuleDefMD.Load(Path.Combine(directory, "Mimick.Fusion.dll"));
        Console.WriteLine("Private supplied assembly metadata only; no game method execution.");
        foreach (string file in new[] { "Mimick.Runtime.dll", "Mimick.Fusion.dll" })
            Console.WriteLine(file + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, file)))).ToLowerInvariant());
        Verify(fusion, "Mimick.FusionNet.FusionNetworkService", "get_MaxPlayers", "System.Int32", false);
        Verify(runtime, "Mimick.Networking.OfflineNetworkService", "get_MaxPlayers", "System.Int32", false);
        Verify(runtime, "Mimick.Gameplay.RoundController", "SetPhase", "System.Void", false, "Mimick.Data.RoundPhase");
        Verify(runtime, "Mimick.Gameplay.RoundController", "PublishCurrentPlayer", "System.Void", false, "System.Int32");
        Verify(runtime, "Mimick.Gameplay.VoiceDirector", "get_Open", "System.Boolean", false);
        Verify(runtime, "Mimick.UI.ResultsScreen", "LaunchRematch", "System.Void", false, "Mimick.Networking.INetworkService");
        Verify(runtime, "Mimick.UI.ResultsScreen", "DropSilentPlayers", "System.Void", true, "Mimick.Networking.INetworkService");
        Verify(runtime, "Mimick.Data.GameState", "ResetForRematch", "System.Void", false);
        var phase = runtime.GetTypes().Single(t => t.FullName == "Mimick.Data.RoundPhase");
        Check(phase.IsEnum && Convert.ToInt32(phase.Fields.Single(f => f.Name == "Playback").Constant.Value) == 4, "Playback enum = 4");
        var type = runtime.GetTypes().Single(t => t.FullName == "Mimick.UI.ResultsScreen");
        Check(type.Fields.Any(f => f.Name == "NativeMethodInfoPtr_LaunchRematch_Private_Void_INetworkService_0"), "IL2CPP native method pointer field exists for current signature");
        Console.WriteLine($"FIX6 SUPPLIED METADATA CHECKS PASS: {passed}; not a runtime or multiplayer test");
    }
    private static void Verify(ModuleDefMD module, string typeName, string name, string ret, bool isStatic, params string[] parameters)
    {
        var type = module.GetTypes().Single(t => t.FullName == typeName);
        var named = type.Methods.Where(m => m.Name == name).ToArray();
        var matches = named.Where(m => m.IsStatic == isStatic && m.MethodSig.RetType.FullName == ret &&
            m.MethodSig.Params.Select(p => p.FullName).SequenceEqual(parameters)).ToArray();
        Check(matches.Length == 1, $"{typeName}.{name}({string.Join(", ", parameters)}) -> {ret}; static={isStatic}");
        if (name == "LaunchRematch") Check(!named.Any(m => m.MethodSig.Params.Count == 0), "supplied ResultsScreen has no zero-argument LaunchRematch");
    }
    private class Current { private void LaunchRematch(Mimick.Networking.INetworkService network) { } }
    private class Legacy { private void LaunchRematch() { } }
    private class WrongParameter { private void LaunchRematch(bool network) { } }
    private class WrongReturn { private int LaunchRematch(Mimick.Networking.INetworkService network) => 0; }
    private class StaticMethod { private static void LaunchRematch(Mimick.Networking.INetworkService network) { } }
    private class GenericMethod { private void LaunchRematch<T>() { } }
    private class ByRefMethod { private void LaunchRematch(ref Mimick.Networking.INetworkService network) { } }
    private class Ambiguous { private void LaunchRematch() { } private void LaunchRematch(Mimick.Networking.INetworkService network) { } }
    private class Absent { }
    private class Parent { public void LaunchRematch() { } }
    private class Inherited : Parent { }
}
namespace Mimick.Networking { public interface INetworkService { } }
