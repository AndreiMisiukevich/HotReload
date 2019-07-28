using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xamarin.Forms
{
    public interface IHotCompiler
    {
        Type Compile(string code, string className);
        bool TryLoadAssembly(Assembly assembly);
    }

    internal static class HotCompiler
    {
        private static readonly Lazy<IHotCompiler> _lazyHotCompiler;

        static HotCompiler()
        {
            _lazyHotCompiler = new Lazy<IHotCompiler>(() => new MicrosoftHotCompiler());
            AppDomain.CurrentDomain.AssemblyLoad += (_, e) => Current.TryLoadAssembly(e.LoadedAssembly);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Current.TryLoadAssembly(a);
            }
            Current.TryLoadAssembly(HotReloader.Current.App.GetType().Assembly);
        }

        public static IHotCompiler Current => _lazyHotCompiler.Value;

        internal static bool? IsSupported { get; set; }

        #region Microsoft
        private class MicrosoftHotCompiler : IHotCompiler
        {
            private HashSet<MetadataReference> _references = new HashSet<MetadataReference>();

            public Type Compile(string code, string className)
            {
                if(IsSupported == false)
                {
                    return null;
                }
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
                _references.Add(MetadataReference.CreateFromFile(assembly.Location));
                return true;
            }
        }
        #endregion

        //#region Mono
        //private sealed class MonoHotCompiler : IHotCompiler
        //{
        //    private readonly Evaluator _evaluator;
        //    private readonly ConsoleReportPrinter _reporter;
        //    private readonly StringWriter _writer;

        //    internal MonoHotCompiler()
        //    {
        //        _writer = new StringWriter();
        //        _reporter = new ConsoleReportPrinter(_writer);
        //        var context = new CompilerContext(new CompilerSettings
        //        {
        //            GenerateDebugInfo = false,
        //            WarningsAreErrors = false,
        //            LoadDefaultReferences = true,
        //            Optimize = true,
        //        }, _reporter);
        //        _evaluator = new Evaluator(context);
        //    }

        //    public Type Compile(string code, string className)
        //    {
        //        //var oldClassName = className.Split('.').LastOrDefault();
        //        //className = oldClassName + "GENERATED_POSTFIX" + Path.GetRandomFileName();
        //        //code = code.Replace(oldClassName, className);

        //        var writerStringBuilder = _writer.GetStringBuilder();
        //        writerStringBuilder.Remove(0, writerStringBuilder.Length);

        //        try
        //        {
        //            if (!_evaluator.Run(code) || _reporter.ErrorsCount > 0)
        //            {
        //                throw new Exception();
        //            }
        //            var getTypeMethod = _evaluator.Compile($"typeof({className})");

        //            if (getTypeMethod == null)
        //            {
        //                throw new Exception();
        //            }

        //            object typeObject = null;
        //            getTypeMethod(ref typeObject);
        //            var type = typeObject as Type;
        //            return type;
        //        }
        //        catch
        //        {
        //            Console.WriteLine($"HOTRELOADER ERROR: {_writer.ToString()}");
        //            return null;
        //        }
        //    }

        //    public bool TryLoadAssembly(Assembly assembly)
        //    {
        //        var name = assembly.GetName().Name;
        //        if (name == "mscorlib" ||
        //            name == "System" ||
        //            name == "System.Core" ||
        //            name.StartsWith("Mono.CSharp", StringComparison.CurrentCultureIgnoreCase) ||
        //            name.StartsWith("Microsoft.CodeAnalysis", StringComparison.CurrentCultureIgnoreCase) ||
        //            name.StartsWith("HotReload.HotCompile", StringComparison.CurrentCultureIgnoreCase) ||
        //            name.StartsWith("eval-", StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            return false;
        //        }

        //        _evaluator?.ReferenceAssembly(assembly);
        //        return true;
        //    }
        //}
        //#endregion
    }
}
