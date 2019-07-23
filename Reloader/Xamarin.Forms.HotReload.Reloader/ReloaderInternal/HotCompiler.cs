using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.CSharp;

namespace Xamarin.Forms
{
    internal sealed class HotCompiler
    {
        private static readonly Lazy<HotCompiler> _lazyHotCompiler;

        static HotCompiler() => _lazyHotCompiler = new Lazy<HotCompiler>(() => new HotCompiler());

        public static HotCompiler Current => _lazyHotCompiler.Value;

        private readonly Evaluator _evaluator;
        private readonly ConsoleReportPrinter _reporter;
        private readonly StringWriter _writer;

        private HotCompiler()
        {
            _writer = new StringWriter();
            _reporter = new ConsoleReportPrinter(_writer);
            var context = new CompilerContext(new CompilerSettings
            {
                GenerateDebugInfo = false,
                WarningsAreErrors = false,
                LoadDefaultReferences = true,
                Optimize = true,
            }, _reporter);
            _evaluator = new Evaluator(context);

            AppDomain.CurrentDomain.AssemblyLoad += (_, e) => TryLoadAssembly(e.LoadedAssembly);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                TryLoadAssembly(a);
            }

            //TODO: should be ADDED! new types would be created with some suffixes
            //TryLoadAssembly(HotReloader.Current.App.GetType().Assembly);
        }

        public Type Compile(string code, string className)
        {
            //var oldClassName = className.Split('.').LastOrDefault();
            //className = oldClassName + "GENERATED_POSTFIX" + Path.GetRandomFileName();
            //code = code.Replace(oldClassName, className);

            var writerStringBuilder = _writer.GetStringBuilder();
            writerStringBuilder.Remove(0, writerStringBuilder.Length);
            
            try
            {
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

        private bool TryLoadAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (name == "mscorlib" ||
                name == "System" ||
                name == "System.Core" ||
                name.StartsWith("eval-", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            _evaluator?.ReferenceAssembly(assembly);
            return true;
        }
    }
}
