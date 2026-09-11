using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

// Development tool only. Never scans user folders and never uploads game files.
// load performs managed assembly loading/reflection; it never calls game methods.
static class Program
{
    static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    static void Json(object obj) => Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2) throw new ArgumentException("inspect <dll> | rename-exact <input> <output> | load <dll> [typename] | dump <dll> <typename-fragment>");
            string path = Path.GetFullPath(args[1]);
            if (args[0] == "load")
            {
                Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
                Assembly asm = Assembly.LoadFile(path);
                Console.WriteLine("ASSEMBLY_LOAD_OK: " + asm.FullName);
                if (args.Length > 2)
                {
                    Type type = asm.GetType(args[2], true)!;
                    Console.WriteLine("TYPE_LOAD_OK: " + type.FullName);
                }
                return 0;
            }
            using var module = ModuleDefMD.Load(path);
            var types = module.GetTypes().ToArray();
            var groups = types.GroupBy(t => t.FullName, StringComparer.Ordinal).Where(g => g.Count() > 1).ToArray();
            if (args[0] == "inspect")
            {
                Json(new {
                    File = Path.GetFileName(path), SHA256 = Hash(path), Assembly = module.Assembly.FullName,
                    Mvid = module.Mvid, TypeCount = types.Length, MethodCount = types.Sum(t => t.Methods.Count),
                    FieldCount = types.Sum(t => t.Fields.Count),
                    Duplicates = groups.Select(g => new { Name = g.Key, Entries = g.Select(t => new {
                        Token = t.MDToken.Raw.ToString("X8"), Flags = t.Attributes.ToString(),
                        Parent = t.DeclaringType?.FullName, Fields = t.Fields.Count, Methods = t.Methods.Count,
                        Properties = t.Properties.Count, Events = t.Events.Count, Nested = t.NestedTypes.Count,
                        CustomAttributes = t.CustomAttributes.Count
                    })}),
                    References = module.GetAssemblyRefs().Select(a => a.FullName)
                });
                return 0;
            }
            if (args[0] == "dump")
            {
                foreach (var t in types.Where(t => t.FullName.Contains(args[2], StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine(t.MDToken + " " + t.FullName);
                    foreach (var f in t.Fields) Console.WriteLine("  FIELD " + f.FullName);
                    foreach (var m in t.Methods)
                    {
                        Console.WriteLine("  METHOD " + m.MDToken + " " + m.FullName);
                        if (m.HasBody) foreach (var i in m.Body.Instructions) Console.WriteLine("    " + i);
                    }
                }
                return 0;
            }
            if (args[0] != "rename-exact" || args.Length != 3) throw new ArgumentException("Unknown operation");
            const string expectedHash = "d9a5bd86b75ac44a188e41ef67cdf2834714417a44444e33b159715ae880e7f6";
            if (Hash(path) != expectedHash) throw new InvalidOperationException("Unrecognized input; refusing to modify it.");
            if (groups.Length != 2 || groups.Sum(g => g.Count() - 1) != 3) throw new InvalidOperationException("Unexpected duplicate layout");
            var plan = new Dictionary<uint, string> { [0x02000322] = "<>O", [0x020003c3] = "<>O", [0x020003e3] = "<>c" };
            foreach (var pair in plan)
            {
                var t = types.Single(t => t.MDToken.Raw == pair.Key);
                if (t.FullName != pair.Value || t.Fields.Count != 0 || t.Methods.Count != 0 || t.NestedTypes.Count != 0 || t.GenericParameters.Count != 0)
                    throw new InvalidOperationException("Target no longer matches the inspected empty helper");
                string name = pair.Value + "_arribbaaFix5_" + pair.Key.ToString("X8");
                Console.WriteLine($"RENAME {pair.Key:X8} {t.FullName} -> {name}; no type/member deletion");
                t.Name = name;
            }
            var options = new ModuleWriterOptions(module);
            options.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            string output = Path.GetFullPath(args[2]);
            if (File.Exists(output)) throw new IOException("Output already exists");
            module.Write(output, options);
            using var check = ModuleDefMD.Load(output);
            var after = check.GetTypes().ToArray();
            if (after.GroupBy(t => t.FullName, StringComparer.Ordinal).Any(g => g.Count() > 1)) throw new InvalidOperationException("Duplicate remains");
            if (after.Length != types.Length || after.Sum(t => t.Methods.Count) != types.Sum(t => t.Methods.Count) || after.Sum(t => t.Fields.Count) != types.Sum(t => t.Fields.Count))
                throw new InvalidOperationException("Definition counts changed");
            Json(new { Result = "STRUCTURAL_REPAIR_OK_NOT_GAME_TEST", InputSHA256 = expectedHash, OutputSHA256 = Hash(output), RenamedTypes = plan.Count, DeletedTypes = 0, DeletedMethods = 0, DeletedFields = 0 });
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            return 1;
        }
    }
}
