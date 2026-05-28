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

namespace Aiel.Dependencies;

/// <summary>
/// Serves as the root node for the Aiel dependency injection framework to
/// identify, configure, and initialize dependencies.
/// </summary>
public abstract class AielApplication : AielDependency
{
    /// <summary>
    /// Gets the name of the application, which is used by the Aiel dependency injection framework
    /// for logging, diagnostics, and other application-specific purposes.
    /// </summary>
    public abstract String ApplicationName { get; }

    /// <summary>
    /// Gets the current version of the application as a string.
    /// </summary>
    /// <remarks>This property is typically used to identify the application's version for display, logging,
    /// or compatibility checks. The format and meaning of the version string may vary depending on the
    /// implementation.</remarks>
    public abstract String ApplicationVersion { get; }
}
