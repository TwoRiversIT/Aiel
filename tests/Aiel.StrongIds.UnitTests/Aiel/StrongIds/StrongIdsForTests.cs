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

namespace Aiel.StrongIds;

[StrongId<Guid>]
public readonly partial record struct GuidAllowDefaultFalseId;

[StrongId<Int16>(AllowDefault = false)]
public readonly partial record struct Int16AllowDefaultFalseId;

[StrongId<UInt16>(AllowDefault = true)]
public readonly partial record struct UInt16AllowDefaultTrueId;

[StrongId<Int32>(AllowDefault = true)]
public readonly partial record struct Int32AllowDefaultTrueId;

[StrongId<UInt32>(AllowDefault = false)]
public readonly partial record struct UInt32AllowDefaultFalseId;

[StrongId<Int64>(AllowDefault = true)]
public readonly partial record struct Int64AllowDefaultTrueId;

[StrongId<UInt64>(AllowDefault = true)]
public readonly partial record struct UInt64AllowDefaultTrueId;

[StrongId<String>]
public readonly partial record struct StringAllowDefaultFalseId;

[StrongId<String>(AllowDefault = true)]
public readonly partial record struct StringAllowDefaultTrueId;

// Just want to see if these compile and work with the source generator.
[StrongId<Guid>(GenerateTryFrom = false)]
public readonly partial record struct GuidNoTryFromId;

[StrongId<Guid>(GenerateTryParse = false)]
public readonly partial record struct GuidNoTryParseId;

[StrongId<Guid>(GenerateTryFrom = false, GenerateTryParse = false)]
public readonly partial record struct GuidNoTryFromNoTryParseId;
