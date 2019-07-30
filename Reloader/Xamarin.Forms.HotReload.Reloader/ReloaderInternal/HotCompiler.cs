using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.CSharp;

namespace Xamarin.Forms
{
    internal interface IHotCompiler
    {
        Type Compile(string code, string className);
        bool TryLoadAssembly(Assembly assembly);
    }

    internal static class HotCompiler
    {
        static HotCompiler()
        {
            try
            {
                Current = Device.RuntimePlatform == Device.Android
                        ? new MonoHotCompiler() as IHotCompiler
                        : new MicrosoftHotCompiler();
            }
            catch
            {
                Current = new StubHotCompiler();
            }
            AppDomain.CurrentDomain.AssemblyLoad += (_, e) => Current.TryLoadAssembly(e.LoadedAssembly);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Current.TryLoadAssembly(a);
            }

            if (!(Current is MonoHotCompiler))
            {
                Current.TryLoadAssembly(HotReloader.Current.App.GetType().Assembly);
            }
        }

        public static IHotCompiler Current { get; }

        internal static bool? IsSupported { get; set; }

        #region Stub
        private class StubHotCompiler : IHotCompiler
        {
            public Type Compile(string code, string className) => null;
            public bool TryLoadAssembly(Assembly assembly) => false;
        }
        #endregion

        #region Microsoft
        private class MicrosoftHotCompiler : IHotCompiler
        {
            private HashSet<MetadataReference> _references = new HashSet<MetadataReference>();

            public Type Compile(string code, string className)
            {
                if (IsSupported == false)
                {
                    return null;
                }
                try
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(code);
                    var assemblyName = $"HotReload.HotCompile.{Path.GetRandomFileName().Replace(".", string.Empty) }";

                    var compilation = CSharpCompilation.Create(
                        assemblyName,
                        new[] { syntaxTree },
                        _references,
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                    using (var stream = new MemoryStream())
                    {
                        var compilationResult = compilation.Emit(stream);

                        if (!compilationResult.Success)
                        {
                            var failures = compilationResult.Diagnostics
                                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                            foreach (var failure in failures)
                            {
                                Console.Error.WriteLine("\t{0}: {1}", failure.Id, failure.GetMessage());
                            }

                            return null;
                        }
                        stream.Seek(0, SeekOrigin.Begin);

                        var bytes = stream.ToArray();
                        var assembly = Assembly.Load(bytes);
                        var type = assembly.GetType(className);
                        return type;
                    }
                }
                catch
                {
                    Console.WriteLine($"HOTRELOADER ERROR: Couldn't compile {code}");
                    return null;
                }
            }

            public bool TryLoadAssembly(Assembly assembly)
            {
                var name = assembly.GetName().Name;
                if (name.StartsWith("Mono.CSharp", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("Microsoft.CodeAnalysis", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("HotReload.HotCompile", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("eval-", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                try
                {
                    _references.Add(MetadataReference.CreateFromFile(assembly.Location));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region Mono
        private sealed class MonoHotCompiler : IHotCompiler
        {
            private readonly Evaluator _evaluator;
            private readonly ConsoleReportPrinter _reporter;
            private readonly StringWriter _writer;

            internal MonoHotCompiler()
            {
                _writer = new StringWriter();
                _reporter = new ConsoleReportPrinter(_writer);
                var context = new CompilerContext(new CompilerSettings
                {
                    GenerateDebugInfo = false,
                    WarningsAreErrors = false,
                    LoadDefaultReferences = true,
                    Optimize = true
                }, _reporter);
                _evaluator = new Evaluator(context);
            }

            public Type Compile(string code, string className)
            {
                if (IsSupported == false)
                {
                    return null;
                }

                try
                {
                    var writerStringBuilder = _writer.GetStringBuilder();
                    writerStringBuilder.Remove(0, writerStringBuilder.Length);

                    if (!_evaluator.Run(code) || _reporter.ErrorsCount > 0)
                    {
                        throw new Exception();
                    }
                    var getTypeMethod = _evaluator.Compile($"typeof({className})");

                    if (getTypeMethod == null)
                    {
                        throw new Exception();
                    }

                    object typeObject = null;
                    getTypeMethod(ref typeObject);
                    var type = typeObject as Type;
                    return type;
                }
                catch
                {
                    Console.WriteLine($"HOTRELOADER ERROR: {_writer.ToString()}");
                    return null;
                }
            }

            public bool TryLoadAssembly(Assembly assembly)
            {
                var name = assembly.GetName().Name;
                if (name == "mscorlib" ||
                    name == "System" ||
                    name == "System.Core" ||
                    name.StartsWith("Mono.CSharp", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("Microsoft.CodeAnalysis", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("HotReload.HotCompile", StringComparison.CurrentCultureIgnoreCase) ||
                    name.StartsWith("eval-", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }

                try
                {
                    _evaluator?.ReferenceAssembly(assembly);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion
    }
}
