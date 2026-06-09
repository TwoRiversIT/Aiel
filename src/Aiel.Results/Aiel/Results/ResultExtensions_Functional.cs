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

namespace Aiel.Results;

/// <summary>
/// Extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/> types.
/// </summary>
[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Extension methods intentionally await Task<Result<T>> returned by external code to enable composable async operations. This is fundamental to the Result pattern's design for chaining async operations across library and application boundaries.")]
public static partial class ResultExtensions
{
    /// <summary>
    /// Transforms the value of a successful result using the specified mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapper">A function to transform the value.</param>
    /// <returns>A successful result with the transformed value, or a failed result with the original error.</returns>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Transforms the value of a successful result using the specified asynchronous mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapper">An asynchronous function to transform the value.</param>
    /// <returns>A task containing a successful result with the transformed value, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper)
    {
        return result.IsSuccess
            ? Result<TOut>.Success(await mapper(result.Value))
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Transforms the value of a successful result task using the specified mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task containing the result to transform.</param>
    /// <param name="mapper">A function to transform the value.</param>
    /// <returns>A task containing a successful result with the transformed value, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Transforms the value of a successful result task using the specified asynchronous mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task containing the result to transform.</param>
    /// <param name="mapper">An asynchronous function to transform the value.</param>
    /// <returns>A task containing a successful result with the transformed value, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    /// <summary>
    /// Chains operations that return results, allowing for sequential composition of result-returning functions.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">A function that takes a value and returns a result.</param>
    /// <returns>The result returned by the binder function, or a failed result with the original error.</returns>
    /// <example>
    /// <code>
    /// public Result&lt;OrderConfirmation&gt; ProcessOrder(CreateOrderRequest request)
    /// {
    ///     return ValidateOrder(request)
    ///         .Bind(order =&gt; CheckInventory(order))
    ///         .Bind(order =&gt; ProcessPayment(order))
    ///         .Bind(order =&gt; CreateShipment(order))
    ///         .Map(shipment =&gt; new OrderConfirmation(shipment.TrackingNumber));
    /// }
    /// </code>
    /// </example>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Chains operations that return results asynchronously, allowing for sequential composition of async result-returning functions.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">An asynchronous function that takes a value and returns a result.</param>
    /// <returns>A task containing the result returned by the binder function, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.IsSuccess
            ? await binder(result.Value)
            : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    /// Chains operations that return results, allowing for sequential composition of result-returning functions on a result task.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task containing the result to bind.</param>
    /// <param name="binder">A function that takes a value and returns a result.</param>
    /// <returns>A task containing the result returned by the binder function, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Chains operations that return results asynchronously on a result task.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the output result value.</typeparam>
    /// <param name="resultTask">A task containing the result to bind.</param>
    /// <param name="binder">An asynchronous function that takes a value and returns a result.</param>
    /// <returns>A task containing the result returned by the binder function, or a failed result with the original error.</returns>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    /// <summary>
    /// Handles both success and failure cases of a result, returning a single value.
    /// </summary>
    /// <typeparam name="TIn">The type of the result value.</typeparam>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="result">The result to match on.</param>
    /// <param name="onSuccess">A function to handle the success case.</param>
    /// <param name="onFailure">A function to handle the failure case.</param>
    /// <returns>The value returned by either the success or failure handler.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Handles both success and failure cases of a result asynchronously, returning a single value.
    /// </summary>
    /// <typeparam name="TIn">The type of the result value.</typeparam>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="result">The result to match on.</param>
    /// <param name="onSuccess">An asynchronous function to handle the success case.</param>
    /// <param name="onFailure">An asynchronous function to handle the failure case.</param>
    /// <returns>A task containing the value returned by either the success or failure handler.</returns>
    public static Task<TOut> MatchAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Handles both success and failure cases of a result task, returning a single value.
    /// </summary>
    /// <typeparam name="TIn">The type of the result value.</typeparam>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="resultTask">A task containing the result to match on.</param>
    /// <param name="onSuccess">A function to handle the success case.</param>
    /// <param name="onFailure">A function to handle the failure case.</param>
    /// <returns>A task containing the value returned by either the success or failure handler.</returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    /// Handles both success and failure cases of a result task asynchronously, returning a single value.
    /// </summary>
    /// <typeparam name="TIn">The type of the result value.</typeparam>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="resultTask">A task containing the result to match on.</param>
    /// <param name="onSuccess">An asynchronous function to handle the success case.</param>
    /// <param name="onFailure">An asynchronous function to handle the failure case.</param>
    /// <returns>A task containing the value returned by either the success or failure handler.</returns>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return await result.MatchAsync(onSuccess, onFailure);
    }

    /// <summary>
    /// Executes an action on a successful result without transforming the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to tap into.</param>
    /// <param name="action">An action to execute if the result is successful.</param>
    /// <returns>The original result, allowing for method chaining.</returns>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Executes an asynchronous action on a successful result without transforming the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to tap into.</param>
    /// <param name="action">An asynchronous action to execute if the result is successful.</param>
    /// <returns>A task containing the original result, allowing for method chaining.</returns>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
        {
            await action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Executes an action on a successful result task without transforming the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task containing the result to tap into.</param>
    /// <param name="action">An action to execute if the result is successful.</param>
    /// <returns>A task containing the original result, allowing for method chaining.</returns>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Executes an asynchronous action on a successful result task without transforming the value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="resultTask">A task containing the result to tap into.</param>
    /// <param name="action">An asynchronous action to execute if the result is successful.</param>
    /// <returns>A task containing the original result, allowing for method chaining.</returns>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
    {
        var result = await resultTask;
        return await result.TapAsync(action);
    }
}
