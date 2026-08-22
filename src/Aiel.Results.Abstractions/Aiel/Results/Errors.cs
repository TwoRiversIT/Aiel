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

using Aiel.Framework;
using System.Text;

namespace Aiel.Results;

/// <summary>
/// A sentinel error that represents the absence of an error. When the
/// operation is successful <see cref="Result.Error"/> property will be set to
/// an instance of this type instead of <see langword="null" />, thereby avoiding
/// the need for null checks, or worse, <see cref="NullReferenceException"/>.
/// </summary>
public sealed partial class NoError : Error
{
    internal const String DefaultMessage = "No error.";
    public static readonly NoError Instance = new(DefaultMessage);
}

/// <summary>
/// A sentinel error type that indicates that the source generator was not
/// able to generate the error type specified in the <see cref="Error.Description"/>.
/// </summary>
/// <remarks>
/// This is used to indicate a problem at runtime. with the error prototype
/// that prevented source generation. Not ideal, but there
/// are a set of analyzers that warn about this at compile time, so this should be rare in practice.
/// </remarks>
public sealed partial class NoSourceGeneratedError : Error
{
    internal const String DefaultMessage = "The source generator did not generate any code for this type: ";
}

/// <summary>
/// Represents a generic error that occurred during an API call. This should be used to wrap
/// HTTP-related errors, including deserialization issues, transport errors, etc., but not
/// Application, Domain-Specific, or Infrastructure (database, message bus, etc.) errors.
/// </summary>
public sealed partial class ApiError : Error
{
    public static ApiError FromException(Exception ex, String? message = null)
    {
        var sb = new StringBuilder();
        if (!String.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        ex.Visit((iex) => sb.AppendLine($"{iex.GetType().Name}: {iex.Message}"));

        return new ApiError(sb.ToString());
    }
}

public sealed partial class ValidationError : Error;

/// <summary>
/// Represents a generic error that occurred during an operation that returns a Result.
/// You should prefer to use a more specific error type if one is available, but this
/// could be used to wrap Infrastructure (database, message bus, etc.) errors.
/// </summary>
public sealed partial class InfrastructureError : Error
{
    public static InfrastructureError FromException(Exception ex, String? message = null)
    {
        var sb = new StringBuilder();
        if (!String.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        ex.Visit((iex) => sb.AppendLine($"{iex.GetType().Name}: {iex.Message}"));

        return new InfrastructureError(sb.ToString());
    }
}

/// <summary>
/// A placeholder error for use during development to quickly wrap unexpected
/// exceptions before a more specific error type is created.
/// </summary>
// ToDo: Create an analyzer that will warn when PlaceholderError is used instead of a more specific error type.
public sealed partial class PlaceholderError : Error
{
    public static PlaceholderError FromException(Exception ex, String? message = null)
    {
        var sb = new StringBuilder();
        if (!String.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        ex.Visit((iex) => sb.AppendLine($"{iex.GetType().Name}: {iex.Message}"));

        return new PlaceholderError(sb.ToString());
    }
}
