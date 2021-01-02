using System;
using System.Runtime.InteropServices;
using NLdr.Framework;

namespace NLdr {
    /// <summary>
    ///     Represents the base class of a native loader.
    /// </summary>
    public abstract class NativeLoader {
        private IntPtr _moduleHandle;
        private bool _isDisposed;

        public static INativeLoader Default =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? (INativeLoader) new LinuxNativeLoader()
                : new WindowsNativeLoader();

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        public void LoadUnmanagedLibrary(string path) {
            _moduleHandle = NativeGetLibraryHandle(path ?? throw new ArgumentNullException(nameof(path)));
            if (_moduleHandle == IntPtr.Zero) {
                throw new BadImageFormatException(path);
            }
        }

        public void FreeModuleHandle(IntPtr moduleHandle) => NativeFreeHandle(_moduleHandle);

        public Delegate GetDelegate(string functionName, Type delegateType) {
            ThrowIfDisposed();
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
            ThrowIfDisposed();
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
        protected abstract void NativeFreeHandle(IntPtr handle);

        private void ReleaseUnmanagedResources() {
            FreeModuleHandle(_moduleHandle);
        }

        private void ThrowIfDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(nameof(NativeLoader));
            }
        }
    }
}