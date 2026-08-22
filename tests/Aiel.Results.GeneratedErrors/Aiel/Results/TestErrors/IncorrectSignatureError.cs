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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.Results.TestErrors;

// We suppress this warning because this is an example of a dynamically
// generated custom error with an incorrect signature. The code generator will
// not generate the derived error class because it has an incorrect signature,
// but we want to include this class in the unit tests to verify that the code
// analyzer correctly identifies and reports the error.
[SuppressMessage("AielUsage", "AIEL00002:Types derived from Error must have a constructor that accepts a single string parameter", Justification = "<Pending>")]
public sealed class IncorrectSignatureError : Error
{
    // This is a workaround to allow the code to compile so that the unit
    // tests can run. Without this workaround, the compiler will generate an
    // error because the base class Error does not have an accessible
    // parameterless constructor.
    public IncorrectSignatureError()
        : base(NoSourceGeneratedError.NoSourceGeneratedErrorCode.Instance, NoSourceGeneratedError.DefaultMessage + nameof(IncorrectSignatureError))
    {
    }
}
