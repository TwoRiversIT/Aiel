// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace Aiel.Framework
{
    /// <summary>
    /// Describes a logical application dependency and the startup contributors that participate in
    /// configuring and initializing that dependency.
    /// </summary>
    public sealed class DependencyDescriptor : DisposableBase
    {
        /// <summary>
    	/// Initializes a new instance of the <see cref="DependencyDescriptor"/> class.
        /// </summary>
    	/// <param name="name">The logical name of the dependency.</param>
    	/// <param name="dependencyType">The <see cref="Type"/> that represents the dependency.</param>
    	/// <param name="dependencies">The dependency types this dependency depends on.</param>
        public DependencyDescriptor(
            String name,
            Type dependencyType,
            IConfigurator instance,
            IEnumerable<Type> dependencies)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Dependency name must not be null or whitespace.", nameof(name));
            }

            ArgumentNullException.ThrowIfNull(dependencyType);
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(dependencies);

            Name = name;
            DependencyType = dependencyType;
            Instance = instance;
            Dependencies = dependencies.ToArray();
        }

        /// <summary>
        /// Gets the logical name of the dependency.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> that represents the dependency.
        /// </summary>
        public Type DependencyType { get; }

        public IConfigurator Instance { get; internal set; }

        /// <summary>
        /// Gets the collection of dependency types that this dependency depends on.
        /// </summary>
        public IReadOnlyCollection<Type> Dependencies { get; }

        protected override async ValueTask DisposeAsyncCore()
        {
            await Instance.SafelyDisposeAsync();
        }
    }
}
