using System;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NLdr.Framework;

namespace NLdr {
    /// <summary>
    ///     Provides a wrapper around native loaders.
    /// </summary>
    public abstract class NativeLibrary {
        private readonly NativeLoader _nativeLoader;
        private IntPtr _moduleHandle;

        protected NativeLibrary() {
            // Implement Linux's dynamic load methods / NativeLoader
            _nativeLoader = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? (NativeLoader) new LinuxNativeLoader()
                : new WindowsNativeLoader();
        }

        public T GetDelegate<T>([NotNull] string functionName) => _nativeLoader.GetDelegate<T>(functionName);

        /// <summary>
        ///     Loads an unmanaged library from the specified path and maps marked delegates to corresponding unmanaged functions.
        /// </summary>
        /// <param name="path">The path to the unmanaged library.</param>
        public void Load([NotNull] string path) {
            _nativeLoader.LoadUnmanagedLibrary(path);
            foreach (var field in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                // Filter all delegate instances
                if (!typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                    continue;
                }

                // Filter those instances marked with the UnmanagedFunction attribute
                var unmanagedFunctionAttribute = field.FieldType.GetCustomAttribute<UnmanagedFunctionAttribute>();
                if (unmanagedFunctionAttribute == null) {
                    continue;
                }

                field.SetValue(this, _nativeLoader.GetDelegate(unmanagedFunctionAttribute.Name, field.FieldType));
            }
        }
    }
}