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

namespace Aiel.Framework;

/// <summary>
/// Defines constants for custom HTTP headers used in the Aiel framework to convey metadata about the application, client, and API instances, as well as versioning information.
/// </summary>
public static class AielHeaders
{
    private const String Prefix = "X-Aiel-";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the unique identifier of the client instance making the request.
    /// </summary>
    public const String ClientInstance = Prefix + "Client-Instance";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the version of the client application making the request.
    /// </summary>
    public const String ClientVersion = Prefix + "Client-Version";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the unique identifier of the server instance handling the request.
    /// </summary>
    public const String ServerInstance = Prefix + "Server-Instance";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the version of the server application handling the request.
    /// </summary>
    public const String ServerVersion = Prefix + "Server-Version";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the unique identifier of the tenant associated with the request.
    /// </summary>
    public const String TenantId = Prefix + "Tenant-Id";

    /// <summary>
    /// Gets the name of the HTTP header used to convey the unique identifier of the current user making the request.
    /// </summary>
    public const String UserId = Prefix + "User-Id";
}
