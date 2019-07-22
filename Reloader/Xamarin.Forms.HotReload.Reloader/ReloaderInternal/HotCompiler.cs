using System;
using System.IO;
using System.Reflection;
using Mono.CSharp;

namespace Xamarin.Forms
{
    internal sealed class HotCompiler
    {
        private readonly string _code;
        private readonly Assembly[] _referencedAssemblies;
        private readonly string _mainClassName;

        public HotCompiler(string code, string mainClassName, params Assembly[] referencedAssemblies)
        {
            _code = code;
            _mainClassName = mainClassName;
            _referencedAssemblies = referencedAssemblies;
        }

        public object Compile()
        {
            var writer = new StringWriter();
            var reporter = new ConsoleReportPrinter(writer);
            try
            {
                var context = new CompilerContext(new CompilerSettings
                {
                    GenerateDebugInfo = false,
                    WarningsAreErrors = false,
                    LoadDefaultReferences = true,
                    Optimize = true,
                }, reporter);
                var evaluator = new Evaluator(context);

                evaluator.ReferenceAssembly(typeof(Label).Assembly);
                evaluator.ReferenceAssembly(typeof(Xaml.StaticExtension).Assembly);
                evaluator.ReferenceAssembly(typeof(Uri).Assembly);
                evaluator.ReferenceAssembly(typeof(Action).Assembly);
                foreach(var assembly in _referencedAssemblies)
                {
                    evaluator.ReferenceAssembly(assembly);
                }

                evaluator.Run(@"using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;");

                if (!evaluator.Run(_code) || reporter.ErrorsCount > 0)
                {
                    throw new Exception();
                }

                var createMethod = evaluator.Compile("new " + _mainClassName + "()");

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
                Console.WriteLine($"HOTRELOADER ERROR: {writer.ToString()}");
                return null;
            }
        }
    }
}