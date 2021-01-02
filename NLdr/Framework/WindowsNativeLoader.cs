using System;
using System.Runtime.InteropServices;

namespace NLdr.Framework {
    /// <summary>
    ///     Represents the Windows unmanaged library loader.
    /// </summary>
    public sealed class WindowsNativeLoader : INativeLoader {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public void FreeModuleHandle(IntPtr moduleHandle) {
            FreeLibrary(moduleHandle);
        }

        public IntPtr LoadUnmanagedLibrary(string path) => LoadLibrary(path);

        public IntPtr GetSymbol(IntPtr handle, string name) => GetProcAddress(handle, name);
    }
}