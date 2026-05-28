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
    /// Gets a special <see cref="NoError"/> instance representing no error.
    /// </summary>
    internal static readonly NoError NoError = new(NoError.DefaultMessage);

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public Boolean IsSuccess { get; }

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
    protected Result(Boolean isSuccess, Error error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException($"Success {GetType().Name} cannot have an error", nameof(error));
        }

        if (!isSuccess && error is null)
        {
            throw new ArgumentException($"Failure {GetType().Name} must have an error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error ?? NoError;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null!);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="TDto">The type of the value to be stored in the result.</typeparam>
    /// <param name="value">The value to include in the successful result. Can be null for reference types.</param>
    /// <returns>A <see cref="Result{TValue}"/> representing a successful operation with the provided value.</returns>
    public static Result<TDto> Success<TDto>(TDto value) => Result<TDto>.Success(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error for the failed result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Implicit conversion from <see cref="Error"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="TDto"/>, either successful or failed.
/// </summary>
/// <typeparam name="TDto">The type of value returned by a successful operation.</typeparam>
public sealed class Result<TDto> : Result
{
    /// <summary>
    /// Gets or sets the value for JSON serialization.
    /// </summary>
    private TDto? ValueStorage { get; }

    /// <summary>
    /// Gets the value returned by a successful operation.
    /// </summary>
    public TDto Value => IsSuccess
        ? ValueStorage!
        : default!;

    /// <summary>
    /// Initializes a new successful instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="value">The value of the successful result.</param>
    /// <param name="error">The error of the failed result. Must be <see cref="NoError"/> for successful results.</param>
    [JsonConstructor]
    private Result(Boolean isSuccess, TDto value, Error error) : base(isSuccess, isSuccess ? null! : error)
    {
        ValueStorage = value;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<TDto> Success(TDto value) => new(true, value, NoError);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error for the failed result.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static new Result<TDto> Failure(Error error) => new(false, default!, error);

    /// <summary>
    /// Implicit conversion from a value to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<TDto>(TDto value) => Success(value);

    /// <summary>
    /// Implicit conversion from an <see cref="Error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<TDto>(Error error) => Failure(error);
}
