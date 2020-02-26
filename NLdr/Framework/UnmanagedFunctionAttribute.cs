using System;
using JetBrains.Annotations;

namespace NLdr.Framework {
    /// <summary>
    ///     Represents the UnmanagedFunction attribute. Marks the parent delegate as an unmanaged function with the specified
    ///     name.
    /// </summary>
    /// <remarks>This attribute has no effect on delegates declared outside of a <see cref="NativeModule" />.</remarks>
    [AttributeUsage(AttributeTargets.Delegate)]
    public sealed class UnmanagedFunctionAttribute : Attribute {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmangedFunctionAttribute" /> with the specified name.
        /// </summary>
        /// <param name="name">The name, which must not be <c>null</c></param>
        public UnmanagedFunctionAttribute([NotNull] string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        ///     Gets the function's name.
        /// </summary>
        public string Name { get; }
    }
}