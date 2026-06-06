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

// -----------------------------------------------------------------------
// AielEventIds.cs — Template / sample EventIds enum
//
// Range allocation convention:
//   1000–1999  Service lifecycle
//   2000–2999  Request pipeline
//   3000–3999  Data / persistence
//   4000–4999  Integration / external calls
//   5000–5999  Security / auth
//
// Each Aiel module owns a distinct sub-range to guarantee uniqueness
// across the system.  The range is documented in the team wiki.
// -----------------------------------------------------------------------

namespace Aiel.Logging.Examples;

/// <summary>
/// Central registry of structured-logging event identifiers for the Aiel framework.
/// </summary>
/// <remarks>
/// <para>
/// Every <see cref="Microsoft.Extensions.Logging.LoggerMessageAttribute"/>-decorated
/// method <b>must</b> use an <c>(int)AielEventIds.XXX</c> cast for its
/// <c>EventId</c> argument (enforced by AIEL001).
/// </para>
/// <para>
/// The matching optional parameter
/// <c>AielEventIds eventId = AielEventIds.XXX</c> must also be present
/// (enforced by AIEL002) and must agree with the attribute value (AIEL005).
/// </para>
/// </remarks>
public enum AielEventIds
{
    // ── Service lifecycle ──────────────────────────────────────────────
    ServiceStart = 1000,
    ServiceStop = 1001,
    ServiceRestart = 1002,
    ServiceHealthy = 1003,
    ServiceDegraded = 1004,

    // ── Request pipeline ──────────────────────────────────────────────
    RequestStart = 2000,
    RequestEnd = 2001,
    RequestError = 2002,
    RequestTimeout = 2003,
    RequestRetry = 2004,

    // ── Data / persistence ────────────────────────────────────────────
    DbQueryStart = 3000,
    DbQueryEnd = 3001,
    DbQueryError = 3002,
    DbMigration = 3003,

    // ── Integration / external calls ──────────────────────────────────
    ExternalCallStart = 4000,
    ExternalCallEnd = 4001,
    ExternalCallError = 4002,

    // ── Security / auth ───────────────────────────────────────────────
    AuthSuccess = 5000,
    AuthFailure = 5001,
    AuthTokenExpired = 5002,
}
