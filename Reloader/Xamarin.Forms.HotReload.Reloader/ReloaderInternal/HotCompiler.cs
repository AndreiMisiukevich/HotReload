using System;
using System.IO;
using System.Reflection;
using Mono.CSharp;

namespace Xamarin.Forms
{
    internal sealed class HotCompiler
    {
        private static readonly Lazy<HotCompiler> _lazyHotReloader;

        static HotCompiler() => _lazyHotReloader = new Lazy<HotCompiler>(() => new HotCompiler());

        public static HotCompiler Current => _lazyHotReloader.Value;

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

            AppDomain.CurrentDomain.AssemblyLoad += (_, e) => LoadAssembly(e.LoadedAssembly);
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadAssembly(a);
            }
        }

        private void LoadAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (name == "mscorlib" ||
                name == "System" ||
                name == "System.Core" ||
                name.StartsWith("eval-", StringComparison.CurrentCultureIgnoreCase))
                return;
            _evaluator?.ReferenceAssembly(assembly);
        }

        private readonly string _code;
        private readonly Assembly[] _referencedAssemblies;
        private readonly string _mainClassName;

        public HotCompiler(string code, string mainClassName, params Assembly[] referencedAssemblies)
        {
            _code = code;
            _mainClassName = mainClassName;
            _referencedAssemblies = referencedAssemblies;
        }

        public object Compile(string code, string className)
        {
            var writerStringBuilder = _writer.GetStringBuilder();
            writerStringBuilder.Remove(0, writerStringBuilder.Length);

            try
            {
//                _evaluator.Run(@"
//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.Net.Http;
//using System.Collections.Concurrent;
//using System.Linq;
//using Xamarin.Forms.Internals;
//using Xamarin.Forms.Xaml;
//using System.Text.RegularExpressions;
//using System.Net.NetworkInformation;
//using System.CodeDom.Compiler;
//using System.Reflection;
//using System.IO;
//using System.Xml;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using System.ComponentModel;");

                if (!_evaluator.Run(code) || _reporter.ErrorsCount > 0)
                {
                    throw new Exception();
                }
                var createMethod = _evaluator.Compile("new " + className + "()");

                if (createMethod == null)
                {
                    throw new Exception();
                }

                object instance = null;
                createMethod(ref instance);
                return instance;
            }
            catch
            {
                Console.WriteLine($"HOTRELOADER ERROR: {_writer.ToString()}");
                return null;
            }
        }
    }
}
