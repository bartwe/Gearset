﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Microsoft.Xna.Framework;

namespace Gearset {
    class ReflectionHelper {
        static readonly List<String> UsingNamespaces;

        /// <summary>
        /// The provider used to compile C# Code.
        /// </summary>
        public static CodeDomProvider CodeProvider;

        /// <summary>
        /// The parameters to be passed to the compiler.
        /// </summary>
        public static CompilerParameters CompilerParameters;

        static ReflectionHelper() {
            UsingNamespaces = new List<String>();
            ClearUsings();

            // Create the C# Code Provider to compile C# Code
            IDictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");
            CodeProvider = new CSharpCodeProvider(providerOptions);
            CompilerParameters = new CompilerParameters();

            // Generate aclass library instead of an executable
            CompilerParameters.GenerateExecutable = false;

            // Specify the assembly file name to generate.
            CompilerParameters.OutputAssembly = "C:\\DummyAssembly.dll";

            // Save the assembly as a physical file.
            CompilerParameters.GenerateInMemory = false;

            // Set whether to treat all warnings as errors.
            CompilerParameters.TreatWarningsAsErrors = false;

            // Add a reference to the game and System.dll and XNA
            //CompilerParameters.ReferencedAssemblies.Add(typeof(Game).Assembly.FullName);
            ClearReferencedAsseblies();
        }

        public static void AddUsingNamespace(String name) {
            name = name.Trim();
            // Check if we don't already have this namespace.
            foreach (var n in UsingNamespaces)
                if (name.Equals(n))
                    return;
            UsingNamespaces.Add(name);
        }

        public static void ClearUsings() {
            UsingNamespaces.Clear();
            UsingNamespaces.AddRange(new[] {
                "System"
            });
        }

        public static void AddReferencedAssembly(String assemblyFullName) {
            assemblyFullName = assemblyFullName.Trim();
            // Check if we don't already have this namespace.
            foreach (var n in CompilerParameters.ReferencedAssemblies)
                if (assemblyFullName.Equals(n))
                    return;

            // In CLR 4.0 we must not add mscorlib.dll
            if (typeof(Object).Assembly.FullName.Equals(assemblyFullName) || assemblyFullName.Contains("mscorlib.dll"))
                return;

            CompilerParameters.ReferencedAssemblies.Add(assemblyFullName);
        }

        public static void ClearReferencedAsseblies() {
            CompilerParameters.ReferencedAssemblies.Clear();
            CompilerParameters.ReferencedAssemblies.AddRange(new[] {
                typeof(Game).Assembly.Location,
                typeof(Vector3).Assembly.Location,
                typeof(ReflectionHelper).Assembly.Location
            });
        }

        #region Compile C# Method

        /// <summary>
        /// Creates a C# Method that can be called from the specified code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="returnType">Set it to null if the method should return void.</param>
        /// <returns></returns>
        public static MethodInfo CompileCSharpMethod(String code, Type returnType) {
            var returnTypeName = returnType == null ? "void" : returnType.Name;
            // Create an compilable enviroment where whatever line can
            // live and easily access the usual XNA stuff.
            code = @"using System;
            using System.IO;
            using System.Collections.Generic;
            using System.Collections;
            using Microsoft.Xna.Framework;
            using Microsoft.Xna.Framework.Graphics;

            namespace DummyNamespace___
            {
                public class DummyClass___
                {
                    public static " + returnTypeName + @" DummyMethod(Object[] p)
                    {
                        return " + code +
                   @"}
                }
            }";

            // Invoke compilation of the source file.
            var cs = CodeProvider.CompileAssemblyFromSource(CompilerParameters, code);

            // Check if there were any errors, TODO: print errors.
            if (cs.Errors.HasErrors) {
                GearsetResources.Console.Log("Gearset", "There were errors while trying to compile code:");
                foreach (CompilerError error in cs.Errors) {
                    Console.WriteLine(error.ErrorText);
                }
                throw new CompileErrorException();
            }

            // Get the newly created method and invoke it.
            var dummyclass = cs.CompiledAssembly.GetType("DummyNamespace___.DummyClass___", false, false);
            return dummyclass.GetMethod("DummyMethod");
        }


        /// <summary>
        /// This method is not intended as a generic way to compile CSHARP
        /// code, it is highly coupled with the InspectorManager.
        /// </summary>
        /// <param name="codes"></param>
        /// <param name="returnTypes"></param>
        /// <returns></returns>
        public static List<MethodInfo> CompileCSharpMethodBatch(IEnumerable<String> codes, IEnumerable<Type> returnTypes) {
            var result = new List<MethodInfo>();
            var codeEnum = codes.GetEnumerator();
            var typeEnum = returnTypes.GetEnumerator();

            var completeCode = new StringBuilder();
            var i = 0;

            foreach (var name in UsingNamespaces) {
                completeCode.AppendLine(String.Format("using {0};", name));
            }

            completeCode.Append(@"
            namespace DummyNamespace___
            {
                public class DummyClass___
                {
                    ");

            // Append every method.
            while (codeEnum.MoveNext() && typeEnum.MoveNext()) {
                if (codeEnum.Current == null) {
                    i++;
                    continue;
                }
                var returnTypeName = typeEnum.Current == null ? "void" : "Object"; // typeEnum.Current.Name;
                completeCode.Append(
                    "public static " + returnTypeName + @" DummyMethod" + i + @"(Object[] p)
                        {
                        " + codeEnum.Current +
                    @"}
                      ");
                i++;
            }

            // Close the class and the namespace
            completeCode.Append(@"}
                                }");

            // Invoke compilation of the source file.
            var cs = CodeProvider.CompileAssemblyFromSource(CompilerParameters, completeCode.ToString());

            //BMK Exception Handling 
            //Check if there were any errors.
            if (cs.Errors.HasErrors) {
                GearsetResources.Console.Log("Gearset", "There were errors while trying to compile code:");
                foreach (CompilerError error in cs.Errors) {
                    Console.WriteLine(error.ErrorText);
                }
                throw new CompileErrorException();
            }

            // Get the newly created method and invoke it.
            var dummyclass = cs.CompiledAssembly.GetType("DummyNamespace___.DummyClass___", false, false);
            for (var j = 0; j < i; j++) {
                MethodInfo method;
                try {
                    method = dummyclass.GetMethod("DummyMethod" + j);
                }
                catch {
                    method = null;
                }
                result.Add(method);
            }

            CompilerParameters.OutputAssembly += "L";
            return result;
        }

        #endregion

        #region Run C# Line

        /// <summary>
        /// Compiles and run the passed C# Code without returning results. 
        /// Parameters can be accesed using the notation "(type)p[i]" where 
        /// type is the Type of the passed parameter in the i direction. We
        /// tryied to make this method with a generic parameter so it wouldn't 
        /// generate garbage when dealing with Value Types, but the Invoke method 
        /// will return an Object and garbage would have been still generated.
        /// </summary>
        public static void RunCSharpLine(String line, params Object[] parameters) {
            CompileCSharpMethod(line, null).Invoke(null, new Object[] { parameters });
        }

        /// <summary>
        /// Compiles and run the passed C# Code. Returns the value of the line.
        /// </summary>
        public static Object RunCSharpLineAndReturnValue(String line, params Object[] parameters) {
            return CompileCSharpMethod(line, typeof(Object)).Invoke(null, new Object[] { parameters });
        }

        #endregion
    }

    public static class ReflectionExtensionMethods {
        #region Get Instance Field/Property

        /// <summary>
        /// Return public Instance fields.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static FieldInfo[] GetInstanceFields(this Type t, bool skipPrivate = true) {
            if (skipPrivate)
                return t.GetFields(BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
            return t.GetFields(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static List<MemberInfo> GetFieldsAndProperties(this Type t, BindingFlags bindingAttr) {
            var targetMembers = new List<MemberInfo>();

            targetMembers.AddRange(t.GetFields(bindingAttr));
            targetMembers.AddRange(t.GetProperties(bindingAttr));

            return targetMembers;
        }

        /// <summary>
        /// Return public Instance properties.
        /// </summary>
        public static PropertyInfo[] GetInstanceProperties(this Type t, bool skipPrivate = true) {
            if (skipPrivate)
                return t.GetProperties(BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
            return t.GetProperties(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Return public Instance methods.
        /// </summary>
        public static MethodInfo[] GetInstanceMethods(this Type t) {
            return t.GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Return public Static methods.
        /// </summary>
        public static MethodInfo[] GetStaticMethods(this Type t) {
            return t.GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.Static);
        }

        #endregion
    }
}
