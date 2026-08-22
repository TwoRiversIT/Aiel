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

using System.Text.Json.Serialization;

namespace Aiel.Results;

/// <summary>
/// Represents the result of an operation, either successful or failed.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public Boolean IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public Boolean IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed result.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error associated with the result. Must be <see cref="NoError"/> for successful results.</param>
    /// <exception cref="ArgumentException">Thrown when the success state and error state are inconsistent.</exception>
    [JsonConstructor]
    protected internal Result(Boolean isSuccess, Error error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException("A Success Result must not have an error.", nameof(error));
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentException("A Failure Result must have an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error ?? NoError.Instance;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(isSuccess: true, error: null!);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the result.</typeparam>
    /// <param name="value">The value to include in the successful result. Must not be <see langword="null"/>.</param>
    /// <returns>A <see cref="Result{TValue}"/> representing a successful operation with the provided value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static Result<T> Success<T>(T value)
        where T : notnull
        => Result<T>.Success(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error for the failed result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(isSuccess: false, error);

    /// <summary>
    /// Implicit conversion from <see cref="Error"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation, either successful or failed, with a value of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// A value is present if and only if <see cref="Result.IsSuccess"/> is <see langword="true"/>. There is no
/// third state: a successful result always carries a non-<see langword="null"/> value, and a failed result
/// carries none.
/// </para>
/// <para>
/// To model "the operation succeeded and the answer is legitimately nothing", use
/// <c>Result&lt;Maybe&lt;T&gt;&gt;</c> rather than a nullable <typeparamref name="T"/>. See <see cref="Maybe{T}"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of value returned by a successful operation. Must not be nullable.</typeparam>
public sealed class Result<T> : Result
    where T : notnull
{
    /// <summary>
    /// Gets or sets the value for JSON serialization which is why it is a property instead of a field.
    /// </summary>
    private T? ValueStorage { get; }

    /// <summary>
    /// Gets the value returned by a successful operation.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="TryGetValue(out T)"/> when failure is an expected outcome. This property is
    /// intended for call sites that have already established <see cref="Result.IsSuccess"/> is
    /// <see langword="true"/>.
    /// </remarks>
    /// <exception cref="ResultException">
    /// Thrown when <see cref="Result.IsFailure"/> is <see langword="true"/>. The thrown exception carries
    /// the <see cref="Result.Error"/> that caused the failure.
    /// </exception>
    /// <remarks>
    /// This property is deliberately hidden from reflection-based serialization. Because the getter throws,
    /// it must never sit in the path of a serializer, logger, or object mapper that walks public properties.
    /// <see cref="Result{T}"/> is serialized by its dedicated converter, which reads the backing storage
    /// directly. Call <c>ConfigureForResults()</c> on your <c>JsonSerializerOptions</c> to install it.
    /// </remarks>
    [JsonIgnore]
    public T Value => IsSuccess
        ? ValueStorage!
        : throw new ResultException(
            $"Cannot read Value when IsSuccess == false. If you need to return a value when the operation fails, consider adding a property to {Error.GetType().Name}.",
            Error);

    /// <summary>
    /// Initializes a new successful instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    private Result(T value) : base(isSuccess: true, error: null!)
    {
        ValueStorage = value;
    }

    /// <summary>
    /// Initializes a new failed instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="error">The error of the failed result.</param>
    private Result(Error error) : base(isSuccess: false, error)
    {
        ValueStorage = default;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful result. Must not be <see langword="null"/>.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new(value);
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error for the failed result.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static new Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Gets the value when the operation succeeded.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the value; otherwise <see langword="default" />.
    /// </param>
    /// <returns><see langword="true"/> when the operation succeeded; otherwise <see langword="false"/>.</returns>

    // NOTE: In a previous version the signature was `public Boolean TryGetValue(out T? value)`, but that was a
    // regression from the original design. The intent is that a successful result always has a non-null value,
    // so the out parameter should be non-nullable. The `[NotNullWhen(true)]` attribute communicates this to
    // static analysis tools.
    public Boolean TryGetValue([NotNullWhen(true)] out T value)
    {
        value = IsSuccess ? ValueStorage! : default!;
        return IsSuccess;
    }

    /// <summary>
    /// Implicit conversion from a value to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from an <see cref="Error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
