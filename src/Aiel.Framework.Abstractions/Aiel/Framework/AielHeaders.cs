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

public static class AielHeaders
{
    public const String Prefix = "X-Aiel-";
    public const String AccountsInstanceHeader = Prefix + "Accounts-Instance";
    public const String AccountsVersionHeader = Prefix + "Accounts-Version";
    public const String ApiInstanceHeader = Prefix + "API-Instance";
    public const String ApiVersionHeader = Prefix + "API-Version";
    public const String ApplicationName = Prefix + "Application-Name";
    public const String ApplicationVersion = Prefix + "Application-Version";
    public const String ClientInstanceHeader = Prefix + "Client-Instance";
    public const String ClientVersionHeader = Prefix + "Client-Version";
    public const String CurrentUserHeader = Prefix + "Current-User";
    public const String TenantIdHeader = Prefix + "Tenant-Id";
}
