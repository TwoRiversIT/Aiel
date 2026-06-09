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

using Aiel.Internal;
using Microsoft.CodeAnalysis;

namespace Aiel.Gps.HP.Generators.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00019 is raised when the generator finds no structs annotated with [NmeaMessage].
    /// </summary>
    public static readonly DiagnosticDescriptor NoNmeaMessageTypesDiscovered = new(
        id: DiagnosticRuleIDs.AIEL00019_NoNmeaMessageTypesDiscoveredId,
        title: "No NMEA message types discovered",
        messageFormat: "NmeaMessageUnionGenerator found no structs annotated with [NmeaMessage]. The required NmeaMessage union cannot be emitted without at least one decorated struct.",
        category: "GPS",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
