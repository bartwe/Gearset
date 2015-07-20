using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Delegate used for methods that return the value of a variable.
    /// </summary>
    public delegate Object Getter(params Object[] o);

    /// <summary>
    /// Delegate used for methods that sets the value of a variable.
    /// </summary>
    public delegate void Setter(params Object[] o);

    static class InspectorReflectionHelper {
        /// <summary>
        /// Used to store the setters and getters for types we've
        /// already generated.
        /// </summary>
        static readonly Dictionary<String, Dictionary<MemberInfo, SetterGetterPair>> SetterGetterCache;

        static InspectorReflectionHelper() {
            SetterGetterCache = new Dictionary<String, Dictionary<MemberInfo, SetterGetterPair>>();
        }

        /// <summary>
        /// Method that creates a Diccionary of methods to set/get the value of
        /// each member of an object of the specified type. But this object only
        /// reachable by the path specified. For example, to get the Position (Vector3)
        /// from a player, the parameters must be: path="Position.", t=Vector3.
        /// If the object being ispected is the class World which contains a
        /// player then parameters must be: path="Player.Position.", t=Vector3.
        /// 
        /// This method is ~10X faster than the first one which created C# code.
        /// </summary>
        internal static Dictionary<MemberInfo, SetterGetterPair> GetSetterGetterDict3(InspectorNode node) {
            var nodeType = node.Type;
            var targetType = node.Target.GetType();
            var expandingObjectTypeName = CreateCSharpTypeString(nodeType);
            var baseObjectTypeName = CreateCSharpTypeString(node.Target.GetType());
            var dictionaryKey = node.Root.Name + node.GetPath();

            // Check if we haven't already generated methods for this node.
            if (SetterGetterCache.ContainsKey(dictionaryKey)) {
                return SetterGetterCache[dictionaryKey];
            }
            // Get all instance fields and properties.
            var members = nodeType.GetFieldsAndProperties(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Code and return types for the methods.
            var codes = new List<string>();
            var types = new List<Type>();

            // We haven't created code to set the fields/properties for this kind of node
            // we have to generated code now and compile it generating methods.
            var setterGetterDict = new Dictionary<MemberInfo, SetterGetterPair>();

            // Defines wether a field or property can be set/get because of a parent being
            // read only.

            var iter = node;
            // Create the path to this node, path will always contain at least
            // two elements because the root is always there.
            var path = new List<InspectorNode>(5);
            while (iter != null) {
                path.Add(iter);
                iter = iter.Parent;
            }

            // Add the reference to the Type of the node, and the target type assemblies.
            ReflectionHelper.AddReferencedAssembly(nodeType.Assembly.Location);
            ReflectionHelper.AddReferencedAssembly(node.Target.GetType().Assembly.Location);

            AddReferencedAssemblies(nodeType);
            AddReferencedAssemblies(node.Target.GetType());

            // Find out which is the last parent that we need to asign.
            // values must be reassigned on the path as long as the parent
            // is a read-only struct.
            // If there's no object or writable valuetype in the chain,
            // we can write to the field/property (if the field/property
            // is writable also.
            var breakIndex = 0;
            if (path.Count > 1) {
                // Assume we can write until we prove we can't
                for (breakIndex = 0; breakIndex < path.Count; ++breakIndex) {
                    // if parent is a read-only value type, continue.
                    if (path[breakIndex].Type.IsValueType)
                        if (!path[breakIndex].CanWrite) {
                            // Sorry, we can't write.
                        }
                        else
                            continue;
                    break;
                }
                //breakIndex--;
            }
            else {
                breakIndex = 0;
            }

            // Create an assembly.
            // TODO: Reuse assembly, possible?
            //AssemblyName myAssemblyName = new AssemblyName();
            //myAssemblyName.Name = "Inspector";

            //AssemblyBuilder myAssembly = Thread.GetDomain().DefineDynamicAssembly(myAssemblyName,  AssemblyBuilderAccess.Run, "c:\\");

            //// Create a module. For a single-file assembly the module
            //// name is usually the same as the assembly name.
            //ModuleBuilder myModule = myAssembly.DefineDynamicModule(myAssemblyName.Name, true);

            //// Define a public class 'Example'.
            //TypeBuilder myTypeBuilder = myModule.DefineType("Example", TypeAttributes.Public);

            // Create GET methods for every field
            foreach (var memberInfo in members) {
                var propertyInfo = memberInfo as PropertyInfo;
                var fieldInfo = memberInfo as FieldInfo;
                var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                setterGetterDict.Add(memberInfo, new SetterGetterPair(null, null));

                // Create the 'Function1' public method, which takes an integer
                // and returns a string.

                var myMethod = new DynamicMethod("XdtkGet_" + memberInfo.Name, typeof(Object), new[] { typeof(Object[]) }, typeof(Inspector), true);
                //MethodBuilder myMethod = myTypeBuilder.DefineMethod("XdtkGet_" + memberInfo.Name,
                //   MethodAttributes.Public | MethodAttributes.Static,
                //   typeof(Object), new Type[] { typeof(Object[]) });

                var ilGenerator = myMethod.GetILGenerator();

                // Load the target object
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldc_I4_0);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                ilGenerator.Emit(OpCodes.Castclass, targetType);

                for (var i = path.Count - 2; i >= 0; --i) {
                    var parent = path[i];
                    // Local variable (aka v#) to store this section of the path.
                    if (parent.IsProperty) {
                        ilGenerator.Emit(OpCodes.Callvirt, path[i + 1].Type.GetProperty(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true));
                        if (parent.Type.IsValueType) {
                            var tempLocal = ilGenerator.DeclareLocal(parent.Type);
                            ilGenerator.Emit(OpCodes.Stloc_S, tempLocal);
                            ilGenerator.Emit(OpCodes.Ldloca_S, tempLocal);
                        }
                    }
                    else {
                        if (parent.Type.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldflda, path[i + 1].Type.GetField(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                        else
                            ilGenerator.Emit(OpCodes.Ldfld, path[i + 1].Type.GetField(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    }
                }

                // Get the value now!
                if (propertyInfo != null) {
                    var getMethod = propertyInfo.GetGetMethod(true);
                    if (getMethod == null)
                        continue;
                    ilGenerator.Emit(OpCodes.Callvirt, getMethod);
                }
                else
                    ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);

                if (memberType.IsValueType)
                    ilGenerator.Emit(OpCodes.Box, memberType);

                // Return
                ilGenerator.Emit(OpCodes.Ret);

                //if (myMethod.InitLocals == false)
                //    System.Diagnostics.Debugger.Break();

                setterGetterDict[memberInfo].Getter = (Getter)myMethod.CreateDelegate(typeof(Getter));
            }


            // Create SET methods for every field
            foreach (var memberInfo in members) {
                var propertyInfo = memberInfo as PropertyInfo;
                var fieldInfo = memberInfo as FieldInfo;

                // If it's a read-only property, continue.
                if (propertyInfo != null && (propertyInfo.GetSetMethod(true) == null || !propertyInfo.CanWrite))
                    continue;

                // Create the 'Function1' public method, which takes an integer
                // and returns a string.
                var myMethod = new DynamicMethod("XdtkSet_" + memberInfo.Name, null, new[] { typeof(Object[]) }, typeof(Inspector), true);
                myMethod.InitLocals = true;
                //MethodBuilder myMethod = myTypeBuilder.DefineMethod("XdtkSet_" + memberInfo.Name,
                //   MethodAttributes.Public | MethodAttributes.Static,
                //   typeof(void), new Type[] { typeof(Object[]) });

                var ilGenerator = myMethod.GetILGenerator();

                // This boolean used to be how we controlled the licensing of Gearset, but now
                // is hard-wired to true.
                var willGenerateSetter = true;
                if (node.Type.Assembly == typeof(GearConsole).Assembly ||
                    (node.Parent != null && node.Parent.Type.Assembly == typeof(GearConsole).Assembly))
                    willGenerateSetter = true;
                if (!willGenerateSetter) {
                    ilGenerator.Emit(OpCodes.Ldc_R4, 4f);
                    ilGenerator.Emit(OpCodes.Stsfld, typeof(GearConsole).GetField("LiteVersionNoticeAlpha", BindingFlags.Public | BindingFlags.Static));
                }
                else {
                    var locals = new Stack<LocalBuilder>();

                    // Load the target object into local 0.
                    var targetLocal = ilGenerator.DeclareLocal(targetType);
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    ilGenerator.Emit(OpCodes.Castclass, targetType);
                    ilGenerator.Emit(OpCodes.Stloc_S, targetLocal);
                    locals.Push(targetLocal);

                    for (var i = path.Count - 2; i >= 0; --i) {
                        var parent = path[i];

                        // Load a reference to the previous variable.
                        if (path[i + 1].Type.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldloca, locals.Peek());
                        else
                            ilGenerator.Emit(OpCodes.Ldloc, locals.Peek());

                        // Read the new variable.
                        if (parent.IsProperty)
                            ilGenerator.Emit(OpCodes.Callvirt, path[i + 1].Type.GetProperty(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true));
                        else
                            ilGenerator.Emit(OpCodes.Ldfld, path[i + 1].Type.GetField(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

                        // Store it in a new local.
                        var l = ilGenerator.DeclareLocal(parent.Type);
                        locals.Push(l);
                        ilGenerator.Emit(OpCodes.Stloc_S, l);
                    }

                    var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                    // Load the object or struct to set the property to.
                    if (path[0].Type.IsValueType)
                        ilGenerator.Emit(OpCodes.Ldloca, locals.Peek());
                    else
                        ilGenerator.Emit(OpCodes.Ldloc, locals.Peek());

                    // Load the value to be set (as object) and cast/unbox it.
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (memberType.IsValueType)
                        ilGenerator.Emit(OpCodes.Unbox_Any, memberType);
                    else
                        ilGenerator.Emit(OpCodes.Castclass, memberType);

                    // Set the value now!
                    if (propertyInfo != null)
                        ilGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod(true));
                    else
                        ilGenerator.Emit(OpCodes.Stfld, fieldInfo);

                    // Now set all fields/properties in reverse order
                    for (var i = 0; i < path.Count - 1; i++) {
                        var parent = path[i];

                        // Push to object to set the value to.
                        var valueToSet = locals.Pop();
                        if (locals.Peek().LocalType.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldloca_S, locals.Peek());
                        else
                            ilGenerator.Emit(OpCodes.Ldloc_S, locals.Peek());

                        // Push the value to set
                        ilGenerator.Emit(OpCodes.Ldloc, valueToSet);

                        //ilGenerator.Emit(OpCodes.Ldloc, local);
                        // Local variable (aka v#) to store this section of the path.
                        if (parent.IsProperty) {
                            var methodInfo = path[i + 1].Type.GetProperty(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
                            if (methodInfo == null) {
                                ilGenerator.Emit(OpCodes.Pop);
                                ilGenerator.Emit(OpCodes.Pop);
                                break;
                            }
                            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
                        }
                        else {
                            ilGenerator.Emit(OpCodes.Stfld, path[i + 1].Type.GetField(parent.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                        }
                    }
                }
                // Return
                ilGenerator.Emit(OpCodes.Ret);

                //if (myMethod.InitLocals == false)
                //    System.Diagnostics.Debugger.Break();

                setterGetterDict[memberInfo].Setter = (Setter)myMethod.CreateDelegate(typeof(Setter));
            }


            //Type finalType = myTypeBuilder.CreateType();
            //myAssembly.Save("SetterGetterMethods.dll");


            // Build the dictionary
            //foreach (MemberInfo memberInfo in members)
            //{
            //    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            //    if (propertyInfo != null && propertyInfo.GetIndexParameters().Length > 0)
            //        continue;
            //    MethodInfo[] finalMethods = finalType.GetMethods();

            //    // HACK: If there are two methods with the same name (i.e. new'd in a derived class)
            //    MethodInfo setterInfo = finalMethods.FirstOrDefault((m) => m.Name == "XdtkSet_" + memberInfo.Name);
            //    MethodInfo getterInfo = finalMethods.FirstOrDefault((m) => m.Name == "XdtkGet_" + memberInfo.Name);

            //    if (setterInfo != null)
            //        setterGetterDict[memberInfo].Setter = (Setter)Delegate.CreateDelegate(typeof(Setter), setterInfo);
            //    if (getterInfo != null)
            //        setterGetterDict[memberInfo].Getter = (Getter)Delegate.CreateDelegate(typeof(Getter), getterInfo); ;
            //}
            return setterGetterDict;
        }

        /// <summary>
        /// Method that creates a Diccionary of methods to set/get the value of
        /// each member of an object of the specified type. But this object only
        /// reachable by the path specified. For example, to get the Position (Vector3)
        /// from a player, the parameters must be: path="Position.", t=Vector3.
        /// If the object being ispected is the class World which contains a
        /// player then parameters must be: path="Player.Position.", t=Vector3.
        /// 
        /// This method is ~10X faster than the previous one that creaated C# code.
        /// </summary>
        internal static Dictionary<MemberInfo, SetterGetterPair> GetSetterGetterDict2(InspectorNode node) {
            var nodeType = node.Type;
            var targetType = node.Target.GetType();
            var expandingObjectTypeName = CreateCSharpTypeString(nodeType);
            var baseObjectTypeName = CreateCSharpTypeString(node.Target.GetType());
            var dictionaryKey = node.Root.Name + node.GetPath();

            // Check if we haven't already generated methods for this node.
            if (SetterGetterCache.ContainsKey(dictionaryKey)) {
                return SetterGetterCache[dictionaryKey];
            }
            // Get all instance fields and properties.
            var members = nodeType.GetFieldsAndProperties(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Code and return types for the methods.
            var codes = new List<string>();
            var types = new List<Type>();

            // We haven't created code to set the fields/properties for this kind of node
            // we have to generated code now and compile it generating methods.
            var setterGetterDict = new Dictionary<MemberInfo, SetterGetterPair>();

            // Defines wether a field or property can be set/get because of a parent being
            // read only.

            var iter = node;
            // Create the path to this node, path will always contain at least
            // two elements because the root is always there.
            var path = new List<InspectorNode>(5);
            while (iter != null) {
                path.Add(iter);
                iter = iter.Parent;
            }

            // Add the reference to the Type of the node, and the target type assemblies.
            ReflectionHelper.AddReferencedAssembly(nodeType.Assembly.Location);
            ReflectionHelper.AddReferencedAssembly(node.Target.GetType().Assembly.Location);

            AddReferencedAssemblies(nodeType);
            AddReferencedAssemblies(node.Target.GetType());

            // Find out which is the last parent that we need to asign.
            // values must be reassigned on the path as long as the parent
            // is a read-only struct.
            // If there's no object or writable valuetype in the chain,
            // we can write to the field/property (if the field/property
            // is writable also.
            var breakIndex = 0;
            if (path.Count > 1) {
                // Assume we can write until we prove we can't
                for (breakIndex = 0; breakIndex < path.Count; ++breakIndex) {
                    // if parent is a read-only value type, continue.
                    if (path[breakIndex].Type.IsValueType)
                        if (!path[breakIndex].CanWrite) {
                            // Sorry, we can't write.
                        }
                        else
                            continue;
                    break;
                }
                //breakIndex--;
            }
            else {
                breakIndex = 0;
            }

            // Create an assembly.
            // TODO: Reuse assembly, possible?
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = "Inspector";

            var myAssembly = Thread.GetDomain().DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.Run, "c:\\");

            // Create a module. For a single-file assembly the module
            // name is usually the same as the assembly name.
            var myModule = myAssembly.DefineDynamicModule(myAssemblyName.Name, true);

            // Define a public class 'Example'.
            var myTypeBuilder = myModule.DefineType("Example", TypeAttributes.Public);

            // Create GET methods for every field
            foreach (var memberInfo in members) {
                var propertyInfo = memberInfo as PropertyInfo;
                var fieldInfo = memberInfo as FieldInfo;
                var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                setterGetterDict.Add(memberInfo, new SetterGetterPair(null, null));

                // Create the 'Function1' public method, which takes an integer
                // and returns a string.
                var myMethod = myTypeBuilder.DefineMethod("XdtkGet_" + memberInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(Object), new[] { typeof(Object[]) });

                var ilGenerator = myMethod.GetILGenerator();

                // Load the target object
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldc_I4_0);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                ilGenerator.Emit(OpCodes.Castclass, targetType);

                for (var i = path.Count - 2; i >= 0; --i) {
                    var parent = path[i];
                    // Local variable (aka v#) to store this section of the path.
                    if (parent.IsProperty) {
                        ilGenerator.Emit(OpCodes.Callvirt, path[i + 1].Type.GetProperty(parent.Name).GetGetMethod(true));
                        if (parent.Type.IsValueType) {
                            var tempLocal = ilGenerator.DeclareLocal(parent.Type);
                            ilGenerator.Emit(OpCodes.Stloc_S, tempLocal);
                            ilGenerator.Emit(OpCodes.Ldloca_S, tempLocal);
                        }
                    }
                    else {
                        if (parent.Type.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldflda, path[i + 1].Type.GetField(parent.Name));
                        else
                            ilGenerator.Emit(OpCodes.Ldfld, path[i + 1].Type.GetField(parent.Name));
                    }
                }

                // Get the value now!
                if (propertyInfo != null) {
                    var getMethod = propertyInfo.GetGetMethod();
                    if (getMethod == null)
                        continue;
                    ilGenerator.Emit(OpCodes.Callvirt, getMethod);
                }
                else
                    ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);

                if (memberType.IsValueType)
                    ilGenerator.Emit(OpCodes.Box, memberType);

                // Return
                ilGenerator.Emit(OpCodes.Ret);
            }


            // Create SET methods for every field
            foreach (var memberInfo in members) {
                var propertyInfo = memberInfo as PropertyInfo;
                var fieldInfo = memberInfo as FieldInfo;

                // If it's a read-only property, continue.
                if (propertyInfo != null && (propertyInfo.GetSetMethod() == null || !propertyInfo.CanWrite))
                    continue;

                // Create the 'Function1' public method, which takes an integer
                // and returns a string.
                var myMethod = myTypeBuilder.DefineMethod("XdtkSet_" + memberInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(void), new[] { typeof(Object[]) });

                var ilGenerator = myMethod.GetILGenerator();

                // This boolean used to be how we controlled the licensing of Gearset, but now
                // is hard-wired to true.
                var willGenerateSetter = true;
                if (node.Type.Assembly == typeof(GearConsole).Assembly ||
                    (node.Parent != null && node.Parent.Type.Assembly == typeof(GearConsole).Assembly))
                    willGenerateSetter = true;
                if (!willGenerateSetter) {
                    ilGenerator.Emit(OpCodes.Ldc_R4, 4f);
                    ilGenerator.Emit(OpCodes.Stsfld, typeof(GearConsole).GetField("LiteVersionNoticeAlpha", BindingFlags.Public | BindingFlags.Static));
                }
                else {
                    var locals = new Stack<LocalBuilder>();

                    // Load the target object into local 0.
                    var targetLocal = ilGenerator.DeclareLocal(targetType);
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    ilGenerator.Emit(OpCodes.Castclass, targetType);
                    ilGenerator.Emit(OpCodes.Stloc_S, targetLocal);
                    locals.Push(targetLocal);

                    for (var i = path.Count - 2; i >= 0; --i) {
                        var parent = path[i];

                        // Load a reference to the previous variable.
                        if (path[i + 1].Type.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldloca, locals.Peek());
                        else
                            ilGenerator.Emit(OpCodes.Ldloc, locals.Peek());

                        // Read the new variable.
                        if (parent.IsProperty)
                            ilGenerator.Emit(OpCodes.Callvirt, path[i + 1].Type.GetProperty(parent.Name).GetGetMethod(true));
                        else
                            ilGenerator.Emit(OpCodes.Ldfld, path[i + 1].Type.GetField(parent.Name));

                        // Store it in a new local.
                        var l = ilGenerator.DeclareLocal(parent.Type);
                        locals.Push(l);
                        ilGenerator.Emit(OpCodes.Stloc_S, l);
                    }

                    var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                    // Load the object or struct to set the property to.
                    if (path[0].Type.IsValueType)
                        ilGenerator.Emit(OpCodes.Ldloca, locals.Peek());
                    else
                        ilGenerator.Emit(OpCodes.Ldloc, locals.Peek());

                    // Load the value to be set (as object) and cast/unbox it.
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (memberType.IsValueType)
                        ilGenerator.Emit(OpCodes.Unbox_Any, memberType);
                    else
                        ilGenerator.Emit(OpCodes.Castclass, memberType);

                    // Set the value now!
                    if (propertyInfo != null)
                        ilGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                    else
                        ilGenerator.Emit(OpCodes.Stfld, fieldInfo);

                    // Now set all fields/properties in reverse order
                    for (var i = 0; i < path.Count - 1; i++) {
                        var parent = path[i];

                        // Push to object to set the value to.
                        var valueToSet = locals.Pop();
                        if (locals.Peek().LocalType.IsValueType)
                            ilGenerator.Emit(OpCodes.Ldloca_S, locals.Peek());
                        else
                            ilGenerator.Emit(OpCodes.Ldloc_S, locals.Peek());

                        // Push the value to set
                        ilGenerator.Emit(OpCodes.Ldloc, valueToSet);

                        //ilGenerator.Emit(OpCodes.Ldloc, local);
                        // Local variable (aka v#) to store this section of the path.
                        if (parent.IsProperty) {
                            var methodInfo = path[i + 1].Type.GetProperty(parent.Name).GetSetMethod(false);
                            if (methodInfo == null) {
                                ilGenerator.Emit(OpCodes.Pop);
                                ilGenerator.Emit(OpCodes.Pop);
                                break;
                            }
                            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
                        }
                        else {
                            ilGenerator.Emit(OpCodes.Stfld, path[i + 1].Type.GetField(parent.Name));
                        }
                    }
                }
                // Return
                ilGenerator.Emit(OpCodes.Ret);
            }


            var finalType = myTypeBuilder.CreateType();
            //myAssembly.Save("SetterGetterMethods.dll");


            // Build the dictionary
            foreach (var memberInfo in members) {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null && propertyInfo.GetIndexParameters().Length > 0)
                    continue;
                var finalMethods = finalType.GetMethods();

                // HACK: If there are two methods with the same name (i.e. new'd in a derived class)
                var setterInfo = finalMethods.FirstOrDefault(m => m.Name == "XdtkSet_" + memberInfo.Name);
                var getterInfo = finalMethods.FirstOrDefault(m => m.Name == "XdtkGet_" + memberInfo.Name);

                if (setterInfo != null)
                    setterGetterDict[memberInfo].Setter = (Setter)Delegate.CreateDelegate(typeof(Setter), setterInfo);
                if (getterInfo != null)
                    setterGetterDict[memberInfo].Getter = (Getter)Delegate.CreateDelegate(typeof(Getter), getterInfo);
                ;
            }
            return setterGetterDict;
        }

        /// <summary>
        /// Method that creates a Diccionary of methods to set/get the value of
        /// each member of an object of the specified type. But this object only
        /// reachable by the path specified. For example, to get the Position (Vector3)
        /// from a player, the parameters must be: path="Position.", t=Vector3.
        /// If the object being ispected is the class World which contains a
        /// player then parameters must be: path="Player.Position.", t=Vector3.
        /// </summary>
        /// <param name="path">Path to get to object of type t, including the 
        /// name of the object itself and a point at the end.</param>
        /// <param name="o">Type of the object to get methods for.</param>
        /// <returns></returns>
        internal static Dictionary<MemberInfo, SetterGetterPair> GetSetterGetterDict(InspectorNode node) {
            throw new Exception("This method is obsolete, use GetSetterGetterDict2 instead which is a lot faster.");
        }

        /// <summary>
        /// Adds all the needed assemblies to deal with a specified type.
        /// Add references to all types needed, including generic parameters and
        /// implemented interfaces.
        /// We go up the hierarchy, baseType could be null if the type is 
        /// a Interface.
        /// </summary>
        static void AddReferencedAssemblies(Type t) {
            var baseType = t;
            while (baseType != typeof(Object) && baseType != null) {
                ReflectionHelper.AddReferencedAssembly(baseType.Assembly.Location);
                baseType = baseType.BaseType;
            }
            foreach (var genericType in t.GetGenericArguments()) {
                baseType = genericType;
                while (baseType != typeof(Object) && baseType != null) {
                    ReflectionHelper.AddReferencedAssembly(baseType.Assembly.Location);
                    baseType = baseType.BaseType;
                }
            }
            foreach (var interfaceType in t.GetInterfaces()) {
                baseType = interfaceType;
                while (baseType != null) {
                    ReflectionHelper.AddReferencedAssembly(baseType.Assembly.Location);
                    baseType = baseType.BaseType;
                }
            }
            // If this type is nested, the parent types should be added to the
            // reference assembly list.
            if (t.IsNested) {
                var parentTypeName = t.AssemblyQualifiedName.Replace("+" + t.Name + ",", ",");
                AddReferencedAssemblies(Type.GetType(parentTypeName));
            }
        }

        static String CreateCSharpTypeString(Type t) {
            var result = new StringBuilder(t.FullName);
            if (t.IsGenericType) {
                // Remove generic parameters, becuase the notation returned
                // by FullName does not work in C#.
                var usableLength = t.FullName.IndexOf('`');
                result = result.Remove(usableLength, result.Length - usableLength);
                result.Append("<");
                foreach (var genericParam in t.GetGenericArguments()) {
                    result.Append(CreateCSharpTypeString(genericParam));
                    result.Append(",");
                }
                result.Remove(result.Length - 1, 1); // Remove the last comma
                result.Append(">");
            }
            if (t.IsNested)
                result.Replace('+', '.');
            return result.ToString();
        }

        /// <summary>
        /// Helper class to store a setter and a getter for a specified
        /// FieldInfo in the setterGetterCache dictionary.
        /// </summary>
        internal class SetterGetterPair {
            internal Setter Setter;
            internal Getter Getter;

            internal SetterGetterPair(Setter setter, Getter getter) {
                Setter = setter;
                Getter = getter;
            }
        }
    }
}
