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

using System.Net.Http.Json;
using System.Text.Json;

namespace Aiel.Results;

/// <summary>
/// Provides extension methods for <see cref="HttpClient"/> to send requests and receive results as <see cref="Result"/> or <see cref="Result{TValue}"/>.
/// </summary>
public static class ResultHttpClientExtensions
{
    /// <summary>
    /// Sends a GET request to the specified URI and returns the result.
    /// </summary>
    /// <param name="client">The HTTP client to send the request.</param>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result.</returns>
    public static async Task<Result> GetResultAsync(this HttpClient client, String requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetAsync(requestUri, cancellationToken);
            return await response.AsResultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a GET request to the specified URI and returns the deserialized result.
    /// </summary>
    /// <typeparam name="TDto">The type of the expected result.</typeparam>
    /// <param name="client">The HTTP client to send the request.</param>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized result.</returns>
    public static async Task<Result<TDto>> GetResultAsync<TDto>(this HttpClient client, String requestUri, CancellationToken cancellationToken = default)
        where TDto : notnull
    {
        try
        {
            var response = await client.GetAsync(requestUri, cancellationToken);
            return await response.AsResultAsync<TDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a POST request with JSON content to the specified URI and deserializes the response into a <see cref="Result"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result> PostAndGetResultAsync(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a POST request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TDto">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TDto>> PostAndGetResultAsync<TDto>(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
        where TDto : notnull
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync<TDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PUT request with JSON content to the specified URI and deserializes the response into a <see cref="Result"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result> PutAndReturnResultAsync(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PUT request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TDto">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TDto>> PutAndReturnResultAsync<TDto>(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
        where TDto : notnull
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync<TDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PATCH request with JSON content to the specified URI and deserializes the response into a <see cref="Result"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result> PatchAndReturnResultAsync(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PatchAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PATCH request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TDto">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TDto>> PatchAndReturnResultAsync<TDto>(
        this HttpClient httpClient,
        String requestUri,
        Object content,
        CancellationToken cancellationToken = default)
        where TDto : notnull
    {
        try
        {
            var response = await httpClient.PatchAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.AsResultAsync<TDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<T>> DeleteAndReturnResultAsync<T>(
        this HttpClient httpClient,
        String requestUri,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        try
        {
            var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
            return await response.AsResultAsync<T>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Always returns a <see cref="Result"/> from the HttpResponseMessage. Does not throw.
    /// </summary>
    /// <param name="response">The HTTP response to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized <see cref="Result"/>.</returns>
    public static async Task<Result> AsResultAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var utf8Json = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<Result>(utf8Json, Results.JSO, cancellationToken);
            return result ?? await ErrorAsync(response);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Always returns a <see cref="Result{TValue}"/> from the HttpResponseMessage. Does not throw.
    /// </summary>
    /// <typeparam name="T">The type of value expected in the successful result.</typeparam>
    /// <param name="response">The HTTP response to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized result.</returns>
    public static async Task<Result<T>> AsResultAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        where T : notnull
    {
        try
        {
            await using var utf8Json = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<Result<T>>(utf8Json, Results.JSO, cancellationToken);
            return result ?? (Result<T>)await ErrorAsync(response);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    [SuppressMessage("Roslynator", "RCS1173:Use coalesce expression instead of 'if'", Justification = "I hate casts.")]
    private static async Task<Result> ErrorAsync(HttpResponseMessage response)
    {
        await using var utf8Json = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<Result>(utf8Json, Results.JSO);
        if (result is null)
        {
            return new ApiError(FormatErrorMessage(response));
        }

        return result;
    }

    private static String FormatErrorMessage(HttpResponseMessage response)
        => $"Request to {response.RequestMessage?.RequestUri} failed with {response.StatusCode}: {response.RequestMessage}.";
}
