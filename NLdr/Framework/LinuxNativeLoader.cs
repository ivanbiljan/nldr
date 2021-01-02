using System;
using System.Runtime.InteropServices;

namespace NLdr.Framework {
    public sealed class LinuxNativeLoader : INativeLoader {
        private const int RtldGlobal = 2;

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr hModule);

        [DllImport("libdl.so", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

        public void FreeModuleHandle(IntPtr moduleHandle) {
            dlclose(moduleHandle);
        }

        public IntPtr LoadUnmanagedLibrary(string path) {
            return dlopen(path, RtldGlobal);
        }

        public IntPtr GetSymbol(IntPtr handle, string name) => dlsym(handle, name);
    }
}