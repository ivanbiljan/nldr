using System;
using JetBrains.Annotations;

namespace NLdr.Framework {
    /// <summary>
    ///     Defines a contract that describes which functions an unmanaged library loader must implement.
    /// </summary>
    public interface INativeLoader {
        /// <summary>
        ///     Frees the handle returned by the <see cref="LoadUnmanagedLibrary" /> method.
        /// </summary>
        /// <param name="moduleHandle">The module handle, which must not be <c>null</c>.</param>
        void FreeModuleHandle([NotNull] IntPtr moduleHandle);

        // /// <summary>
        // ///     Returns a delegate from the specified function pointer.
        // /// </summary>
        // /// <param name="functionName">The function name.</param>
        // /// <param name="delegateType">The type of delegate.</param>
        // /// <returns>The delegate.</returns>
        // Delegate GetDelegate([NotNull] string functionName, Type delegateType);
        //
        // /// <summary>
        // ///     Returns a delegate of the specified type from the specified function pointer.
        // /// </summary>
        // /// <param name="functionName">The function name.</param>
        // /// <typeparam name="T">The type of delegate.</typeparam>
        // /// <returns>The delegate.</returns>
        // T GetDelegate<T>([NotNull] string functionName);

        /// <summary>
        ///     Loads an unmanaged library from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        IntPtr LoadUnmanagedLibrary([NotNull] string path);

        /// <summary>
        /// Gets the location of a symbol that's been loaded into memory as a null-terminated string <paramref name="name"/>.
        /// </summary>
        /// <param name="handle">The handle of a library loaded via <see cref="LoadUnmanagedLibrary"/>.</param>
        /// <param name="name">The name, which must not be <c>null</c>.</param>
        /// <returns>The memory location of the specified symbol.</returns>
        IntPtr GetSymbol(IntPtr handle, string name);
    }
}