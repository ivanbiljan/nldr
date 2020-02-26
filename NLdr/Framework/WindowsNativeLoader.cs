using System;
using System.Runtime.InteropServices;

namespace NLdr.Framework {
    /// <summary>
    ///     Represents the Windows unmanaged library loader.
    /// </summary>
    public sealed class WindowsNativeLoader : NativeLoader {
        public override void FreeModuleHandle(IntPtr moduleHandle) => FreeLibrary(moduleHandle);

        protected override IntPtr NativeGetFunctionPointer(IntPtr handle, string functionName) => GetProcAddress(handle, functionName);

        protected override IntPtr NativeGetLibraryHandle(string path) => LoadLibrary(path);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}