using System;
using System.Runtime.InteropServices;

namespace NLdr.Framework {
    public sealed class LinuxNativeLoader : NativeLoader {
        private const int RtldGlobal = 2;

        public override void FreeModuleHandle(IntPtr moduleHandle) => dlclose(moduleHandle);

        protected override IntPtr NativeGetFunctionPointer(IntPtr handle, string functionName) => dlsym(handle, functionName);

        protected override IntPtr NativeGetLibraryHandle(string path) => dlopen(path, RtldGlobal);

        [DllImport("libdl.so")]
        public static extern int dlclose(IntPtr hModule);

        [DllImport("libdl.so", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);
    }
}