using System;
using System.Runtime.InteropServices;
using NLdr.Framework;

namespace NLdr {
    /// <summary>
    ///     Represents the base class of a native loader.
    /// </summary>
    public abstract class NativeLoader : INativeLoader {
        private IntPtr _moduleHandle;

        public void Dispose() {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void LoadUnmanagedLibrary(string path) {
            _moduleHandle = NativeGetLibraryHandle(path ?? throw new ArgumentNullException(nameof(path)));
            if (_moduleHandle == IntPtr.Zero) {
                throw new BadImageFormatException(path);
            }
        }

        public abstract void FreeModuleHandle(IntPtr moduleHandle);

        public Delegate GetDelegate(string functionName, Type delegateType) {
            if (functionName == null) {
                throw new ArgumentNullException(nameof(functionName));
            }

            var functionPointer = NativeGetFunctionPointer(_moduleHandle, functionName);
            if (functionPointer == IntPtr.Zero) {
                throw new ArgumentException($"Invalid function '{functionName}'", nameof(functionName));
            }

            return Marshal.GetDelegateForFunctionPointer(functionPointer, delegateType);
        }

        public T GetDelegate<T>(string functionName) {
            if (functionName == null) {
                throw new ArgumentNullException(nameof(functionName));
            }

            var functionPointer = NativeGetFunctionPointer(_moduleHandle, functionName);
            if (functionPointer == IntPtr.Zero) {
                throw new ArgumentException($"Invalid function '{functionName}'", nameof(functionName));
            }

            return Marshal.GetDelegateForFunctionPointer<T>(functionPointer);
        }

        ~NativeLoader() {
            ReleaseUnmanagedResources();
        }

        protected abstract IntPtr NativeGetFunctionPointer(IntPtr handle, string functionName);

        protected abstract IntPtr NativeGetLibraryHandle(string path);

        private void ReleaseUnmanagedResources() {
            FreeModuleHandle(_moduleHandle);
        }
    }
}