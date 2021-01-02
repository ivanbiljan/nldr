using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Dynamitey;
using ImpromptuInterface;
using ImpromptuInterface.Build;
using JetBrains.Annotations;
using NLdr.Framework;

namespace NLdr {
    /// <summary>
    ///     Provides a wrapper around native loaders.
    /// </summary>
    public sealed class NativeLibrary : IDisposable {
        // private readonly NativeLoader _nativeLoader;
        // private IntPtr _moduleHandle;
        //
        // protected NativeLibrary() {
        //     // Implement Linux's dynamic load methods / NativeLoader
        //     _nativeLoader = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        //         ? (NativeLoader) new LinuxNativeLoader()
        //         : new WindowsNativeLoader();
        // }
        //
        // public T GetDelegate<T>([NotNull] string functionName) => _nativeLoader.GetDelegate<T>(functionName);
        //
        // /// <summary>
        // ///     Loads an unmanaged library from the specified path and maps marked delegates to corresponding unmanaged functions.
        // /// </summary>
        // /// <param name="path">The path to the unmanaged library.</param>
        // public void Load([NotNull] string path) {
        //     // _nativeLoader.LoadUnmanagedLibrary(path);
        //     // foreach (var field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
        //     //     // Filter all delegate instances
        //     //     if (!typeof(Delegate).IsAssignableFrom(field.FieldType)) {
        //     //         continue;
        //     //     }
        //     //
        //     //     // Filter those instances marked with the UnmanagedFunction attribute
        //     //     var unmanagedFunctionAttribute = field.FieldType.GetCustomAttribute<UnmanagedFunctionAttribute>();
        //     //     if (unmanagedFunctionAttribute == null) {
        //     //         continue;
        //     //     }
        //     //
        //     //     field.SetValue(this, _nativeLoader.GetDelegate(unmanagedFunctionAttribute.Name, field.FieldType));
        //     // }
        //     
        //     _nativeLoader.LoadUnmanagedLibrary(path);
        // }

        private static readonly Func<Type[],Type> MakeNewCustomDelegate = (Func<Type[],Type>)Delegate.CreateDelegate(typeof(Func<Type[],Type>), typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.DelegateHelpers").GetMethod("MakeNewCustomDelegate", BindingFlags.NonPublic | BindingFlags.Static));

        public static Type NewDelegateType(Type ret, params Type[] parameters)
        {
            Type[] args = new Type[parameters.Length + 1];
            parameters.CopyTo(args, 0);
            args[args.Length-1] = ret;
            return MakeNewCustomDelegate(args);
        }

        private readonly INativeLoader _loader;
        private IntPtr _moduleHandle;

        public NativeLibrary(INativeLoader loader) {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public static NativeLibrary Default =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new NativeLibrary(new LinuxNativeLoader())
                : new NativeLibrary(new WindowsNativeLoader());

        public T Bind<T>(string path) where T : class {
            var type = typeof(T);
            if (!type.IsInterface) {
                throw new ArgumentException($"{nameof(T)} must be an interface.", nameof(T));
            }

            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }

            _moduleHandle = _loader.LoadUnmanagedLibrary(path);
            if (_moduleHandle == IntPtr.Zero) {
                throw new BadImageFormatException(path);
            }
            
            var expandoObject = new ExpandoObject();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var delegateType = NewDelegateType(method.ReturnType, parameterTypes);
                var marshaledDelegate = GetDelegate(method.Name, delegateType);
                
                var dynamicMethod = new DynamicMethod(method.Name, method.ReturnType, parameterTypes, Impromptu.ActLike<T>(expandoObject).GetType());
                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0); // Load the object
                for (var i = 0; i < parameterTypes.Length; ++i) {
                    // Load the remaining arguments
                    ilGenerator.Emit(OpCodes.Ldarg, i + 1);
                }

                ilGenerator.Emit(OpCodes.Call, marshaledDelegate.Method);
                ilGenerator.Emit(OpCodes.Ret);
                SetMember(expandoObject, method.Name, dynamicMethod.CreateDelegate(typeof(lua_state)));
            }

            return Impromptu.ActLike<T>(expandoObject);

            void SetMember(ExpandoObject expando, string name, object value) {
                var expandoDict = expando as IDictionary<string, object>;
                expandoDict[name] = value;
            }
            
            Type NewDelegateType(Type returnType, params Type[] parameters)  {
                var args = returnType == typeof(void) ? new Type[parameters.Length] : new Type[parameters.Length + 1];
                parameters.CopyTo(args, 0);
                args[args.Length - 1] = returnType;
                return MakeNewCustomDelegate(args);
            }
        }
        
        public delegate IntPtr lua_state();

        private T GetDelegate<T>(string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            Debug.Assert(_moduleHandle != default, $"{_moduleHandle} != default");

            var functionHandle = _loader.GetSymbol(_moduleHandle, name);
            if (functionHandle == default) {
                throw new Exception($"Function '{name}' does not exist.");
            }

            return Marshal.GetDelegateForFunctionPointer<T>(functionHandle);
        }
        
        private Delegate GetDelegate(string name, Type delegateType) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            Debug.Assert(_moduleHandle != default, $"{_moduleHandle} != default");

            var functionHandle = _loader.GetSymbol(_moduleHandle, name);
            if (functionHandle == default) {
                throw new Exception($"Function '{name}' does not exist.");
            }
            
            Debug.WriteLine(delegateType);
            return Marshal.GetDelegateForFunctionPointer(functionHandle, delegateType);
        }

        private void ReleaseUnmanagedResources() {
            _loader.FreeModuleHandle(_moduleHandle);
        }

        public void Dispose() {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~NativeLibrary() {
            ReleaseUnmanagedResources();
        }
    }
}