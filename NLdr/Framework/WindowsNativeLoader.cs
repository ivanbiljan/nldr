using System;
using System.Runtime.InteropServices;

namespace NLdr.Framework
{
    /// <summary>
    ///     Represents the Windows unmanaged library loader.
    /// </summary>
    public sealed class WindowsNativeLoader : NativeLoader
    {
        public override void FreeModuleHandle(IntPtr moduleHandle)
        {
            FreeLibrary(moduleHandle);
        }

        protected override IntPtr NativeGetFunctionPointer(IntPtr handle, string functionName) =>
            GetProcAddress(handle, functionName);

        protected override IntPtr NativeGetLibraryHandle(string path) => LoadLibrary(path);

        /// <summary>
        ///     Frees the file handle returned by the <see cref="LoadLibrary(string)" /> method.
        /// </summary>
        /// <param name="hModule">The file handle.</param>
        /// <returns><c>true</c> if the handle is released successfully; otherwise, <c>false</c>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        ///     Gets the procedure address bound to the specified name and handle.
        /// </summary>
        /// <param name="hModule">The handle.</param>
        /// <param name="procName">The procedure name.</param>
        /// <returns>The handle for the procedure.</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /// <summary>
        ///     Maps the specified DLL file into the address space of the calling process.
        /// </summary>
        /// <param name="lpFileName">The name of the DLL file.</param>
        /// <returns>The handle for the DLL file.</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}