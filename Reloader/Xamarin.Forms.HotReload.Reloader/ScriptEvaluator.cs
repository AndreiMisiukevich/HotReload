using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.CodeDom.Compiler;

namespace SimpleHelpers
{
    class DeleteMe
    {
        public static void CompileExecutable()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            var compilerParameters = new CompilerParameters
            {
                IncludeDebugInformation = false,
                GenerateExecutable = false,
                OutputAssembly = "ASSEMBLY_NAME",
                GenerateInMemory = true,
                TreatWarningsAsErrors = false
            };
            compilerParameters.ReferencedAssemblies.Add("ASSEMBLY_NAME");
            var compilerResults = provider.CompileAssemblyFromSource(compilerParameters, "SOURCE_CODE");
        }
    }

    public class ScriptEvaluator
    {
        public string Script { get; set; }

        public bool HasError { get; set; }

        public string Message { get; set; }

        public string MainClassName { get; set; }

        private Mono.CSharp.CompiledMethod _createMethod;
        private HashSet<Assembly> _assemblies = new HashSet<Assembly>();

        public Mono.CSharp.CompiledMethod CreateMethod
        {
            get { return _createMethod; }
        }

        public ScriptEvaluator(string csharpCode, string mainClassName)
            : this(csharpCode, mainClassName, null)
        {
        }

        public ScriptEvaluator(string csharpCode, string mainClassName, Type baseType)

        {
            Script = csharpCode;
            MainClassName = mainClassName;
            HasError = false;
            if (baseType != null)
                AddReference(baseType);
        }

        public static ScriptEvaluator Create(string csharpCode, string mainClassName)
        {
            return (new ScriptEvaluator(csharpCode, mainClassName)).Compile();
        }

        public ScriptEvaluator Compile()
        {
            Message = null;
            HasError = false;
            var reportWriter = new System.IO.StringWriter();

            try
            {
                var settings = new Mono.CSharp.CompilerSettings();
                settings.GenerateDebugInfo = false;
                settings.LoadDefaultReferences = true;
                settings.Optimize = true;
                settings.WarningsAreErrors = false;

                var reporter = new Mono.CSharp.ConsoleReportPrinter(reportWriter);
                var ctx = new Mono.CSharp.CompilerContext(settings, reporter);
                var scriptEngine = new Mono.CSharp.Evaluator(ctx);
                AddDefaultReferences();
                // add assemblies
                foreach (var i in _assemblies)
                {
                    scriptEngine.ReferenceAssembly(i);
                }

                if (String.IsNullOrWhiteSpace(Script))
                    throw new ArgumentNullException("Expression");

                if (!scriptEngine.Run(Script))
                    throw new Exception(reportWriter.ToString());

                if (reporter.ErrorsCount > 0)
                {
                    throw new Exception(reportWriter.ToString());
                }

                _createMethod = scriptEngine.Compile("new " + MainClassName + "()");

                if (reporter.ErrorsCount > 0)
                {
                    throw new Exception(reportWriter.ToString());
                }
                if (_createMethod == null)
                {
                    throw new Exception("script method could be created");
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                HasError = true;
            }
            return this;
        }

        private void AddDefaultReferences()
        {
            AddReference(GetType());
            AddReference(typeof(Uri));
            AddReference(typeof(Action));
        }

        public ScriptEvaluator AddReference(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public ScriptEvaluator AddReference(Type type) => AddReference(type.Assembly);

        public T CreateInstance<T>() where T : class => CreateInstance() as T;

        public object CreateInstance()
        {
            if (!HasError)
            {
                if (_createMethod == null)
                {
                    Compile();
                }

                if (_createMethod != null)
                {
                    object result = null;
                    _createMethod(ref result);
                    return result;
                }
            }
            return null;
        }
    }
}