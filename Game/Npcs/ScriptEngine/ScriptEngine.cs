using System;
using System.Text;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

namespace MTA.Game.Npcs.ScriptEngine
{
    /// <summary>
    /// Description of ScriptEngine.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of ScriptEngine.
    /// </remarks>
    /// <param name="Settings">The settings associated to the script engine.</param>
    /// <param name="scriptcheckinterval">The interval between each script update.</param>
    public class ScriptEngine(ScriptSettings Settings, int scriptcheckinterval = 10000)
    {
        /// <summary>
        /// The settings associated with the script engine.
        /// </summary>
        private ScriptSettings Settings = Settings;

        /// <summary>
        /// The thread checking for script updates.
        /// </summary>
        //	private Threading.BaseThread scriptCheckerThread;

        /// <summary>
        /// The interval between each script update.
        /// </summary>
        private int checkInterval = scriptcheckinterval;

        public static void SetNamespaces(ScriptSettings settings)
        {
            Content = Content.Replace("__namespace__", getns(settings));
            Content2 = Content2.Replace("__namespace__", getns2(settings));
        }

        /// <summary>
        /// The c# code content.
        /// </summary>
        private static string Content = @"__namespace__

namespace scriptnamespace
{
	class scriptclass
	{
		__method__
	}
}";

        /// <summary>
        /// The vb code content.
        /// </summary>
        private static string Content2 = @"__namespace__

Namespace scriptnamespace
	Class scriptclass
	
		__method__
		
	End Class
End Namespace";

        /// <summary>
        /// Gets the namespace code for c#.
        /// </summary>
        /// <returns>Returns the namespace code.</returns>
        private static string getns(ScriptSettings Settings)
        {
            StringBuilder namespaceBuilder = new StringBuilder();
            foreach (string _namespace in Settings._namespaces.Values)
            {
                namespaceBuilder.Append("using ").Append(_namespace).Append(";").Append(Environment.NewLine);
            }

            return namespaceBuilder.ToString();
        }

        /// <summary>
        /// Gets the namespace code for vb.
        /// </summary>
        /// <returns>Returns the namespace code.</returns>
        private static string getns2(ScriptSettings Settings)
        {
            StringBuilder namespaceBuilder = new StringBuilder();
            foreach (string _namespace in Settings._namespaces.Values)
            {
                namespaceBuilder.Append("Imports ").Append(_namespace).Append(Environment.NewLine);
            }

            return namespaceBuilder.ToString();
        }

        private string? currentcompilefile;

        /// <summary>
        /// Checks for updates.
        /// </summary>
        public void Check_Updates()
        {
            try
            {
                // Ensure the base scripts directory exists
                if (!System.IO.Directory.Exists(Settings.ScriptLocation))
                {
                    System.IO.Directory.CreateDirectory(Settings.ScriptLocation);
                }

                // Ensure the cmpl subdirectory exists
                string cmplPath = Settings.ScriptLocation + "\\cmpl";
                if (!System.IO.Directory.Exists(cmplPath))
                {
                    System.IO.Directory.CreateDirectory(cmplPath);
                }

                foreach (string file in System.IO.Directory.GetFiles(cmplPath))
                {
                    System.IO.File.Delete(file);
                }

                DateTime now = DateTime.Now;
                currentcompilefile = "\\cmpl\\cmpl_" + now.Month + "-" + now.Day + "-" + now.Hour + "-" + now.Minute + "-" +
                                     now.Second;

                switch (Settings.Language)
                {
                    case ScriptLanguage.CSharp:
                        {
                            StringBuilder scriptBuilder = new StringBuilder();
                            foreach (string file in System.IO.Directory.GetFiles(Settings.ScriptLocation))
                            {
                                if (file.EndsWith(".cs"))
                                {
                                    scriptBuilder.Append(System.IO.File.ReadAllText(file));
                                    scriptBuilder.Append(Environment.NewLine);
                                }
                            }

                            System.IO.File.WriteAllText(Settings.ScriptLocation + currentcompilefile + ".cs",
                                Content.Replace("__method__", scriptBuilder.ToString()));
                            CompileCSScripts();
                            break;
                        }
                    case ScriptLanguage.VisualBasic:
                        {
                            StringBuilder scriptBuilder = new StringBuilder();
                            foreach (string file in System.IO.Directory.GetFiles(Settings.ScriptLocation))
                            {
                                if (file.EndsWith(".vb"))
                                {
                                    scriptBuilder.Append(System.IO.File.ReadAllText(file));
                                    scriptBuilder.Append(Environment.NewLine);
                                }
                            }

                            System.IO.File.WriteAllText(Settings.ScriptLocation + currentcompilefile + ".vb",
                                Content2.Replace("__method__", scriptBuilder.ToString()));
                            CompileCSScripts();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Script loading failed... Exception: " + Environment.NewLine + e.ToString());
            }
        }

        /// <summary>
        /// Compiles all the c# scripts using Roslyn.
        /// </summary>
        private void CompileCSScripts()
        {
            var scriptPath = Settings.ScriptLocation + currentcompilefile + ".cs";
            var code = System.IO.File.ReadAllText(scriptPath);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            foreach (Type type in Settings.types.Values)
            {
                var asm = Assembly.GetAssembly(type);
                if (asm != null && !string.IsNullOrEmpty(asm.Location))
                    references.Add(MetadataReference.CreateFromFile(asm.Location));
            }

            var compilation = CSharpCompilation.Create(
                "ScriptAssembly",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new System.IO.MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                            Console.WriteLine(diagnostic.ToString());
                    }
                    Console.ReadLine();
                    return;
                }
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == "scriptnamespace" && type.IsClass && type.Name == "scriptclass")
                    {
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (method.IsStatic)
                            {
                                if (method.Name.StartsWith("script_"))
                                    scriptCollection.AddOrUpdate(uint.Parse(method.Name.Split('_')[1]), method);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compiles all the vb scripts.
        /// </summary>
        private void CompileVBScripts()
        {
            Dictionary<string, string> dictionary = new()
            {
                { "CompilerVersion", Settings.Framework }
            };
            CompilerParameters compilerParameters = new()
            {
                GenerateInMemory = true
            };

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                compilerParameters.ReferencedAssemblies.Add(assembly.Location);
            }

            foreach (Type type in Settings.types.Values)
            {
                var asm = Assembly.GetAssembly(type);
                if (asm != null && !string.IsNullOrEmpty(asm.Location))
                    compilerParameters.ReferencedAssemblies.Add(asm.Location);
            }
            VBCodeProvider vbCodeProvider = new();
            CompilerResults compilerResults = vbCodeProvider.CompileAssemblyFromFile(compilerParameters,
                Settings.ScriptLocation + currentcompilefile + ".vb");
            if (compilerResults.Errors.Count != 0)
            {
                foreach (CompilerError err in compilerResults.Errors)
                    Console.WriteLine(err.ToString());
            }
            else
            {
                foreach (Type type in compilerResults.CompiledAssembly.GetTypes())
                {
                    if (type.Namespace == "scriptnamespace" && type.IsClass && type.Name == "scriptclass")
                    {
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (method.IsStatic)
                            {
                                if (method.Name.StartsWith("script_"))
                                    scriptCollection.AddOrUpdate(uint.Parse(method.Name.Split('_')[1]), method);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The collection of the scripts.
        /// </summary>
        public ScriptCollection scriptCollection = new ScriptCollection(Settings);

        /// <summary>
        /// Invokes a script.
        /// </summary>
        /// <param name="key">The script key.</param>
        /// <param name="paramters">Parameters associated with the script. [null, if no parameters]</param>
        /// <returns>Returns true if the script exist.</returns>
        public bool Invoke(uint key, object[] paramters)
        {
            return scriptCollection.Invoke(key, paramters);
        }
    }
}

