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

public static class ResultHttpClientExtensions
{
    public static async Task<Result> GetResultAsync(this HttpClient client, String requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await client.GetFromJsonAsync<Result>(requestUri, Results.JSO, cancellationToken);
            return result ?? Result.Failure(new ApiError("Failed to retrieve data from the server."));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a GET request to the specified URI and returns the deserialized result.
    /// </summary>
    /// <typeparam name="T">The type of the expected result.</typeparam>
    /// <param name="client">The HTTP client to send the request.</param>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized result.</returns>
    public static async Task<Result<T>> GetResultAsync<T>(this HttpClient client, String requestUri, CancellationToken cancellationToken = default)
    {
        var result = await client.GetFromJsonAsync<Result<T>>(requestUri, Results.JSO, cancellationToken);
        return result ?? Result<T>.Failure(new ApiError("Failed to retrieve data from the server."));
    }

    /// <summary>
    /// Sends a POST request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body to serialize.</typeparam>
    /// <typeparam name="TResponse">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TResponse>> PostAndReturnResultAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        String requestUri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.ResultAsync<TResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a POST request with JSON content to the specified URI and deserializes the response into a
    /// <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body to serialize.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized result.</returns>
    public static async Task<Result> PostAndReturnResultAsync<TRequest>(
        this HttpClient httpClient,
        String requestUri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.ResultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PUT request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body to serialize.</typeparam>
    /// <typeparam name="TResponse">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TResponse>> PutAndReturnResultAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        String requestUri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.ResultAsync<TResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Sends a PATCH request with JSON content to the specified URI and deserializes the response into a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body to serialize.</typeparam>
    /// <typeparam name="TResponse">The type of value expected in the successful result.</typeparam>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="content">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
    /// deserialized value if successful; otherwise, a failure result containing error information.</returns>
    public static async Task<Result<TResponse>> PatchAndReturnResultAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        String requestUri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PatchAsJsonAsync(requestUri, content, Results.JSO, cancellationToken);
            return await response.ResultAsync<TResponse>(cancellationToken);
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
    {
        try
        {
            var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
            return await response.ResultAsync<T>(cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Returns a <see cref="Result{TValue}"/> with a single value from an HttpResponseMessage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ResultAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<Result<T>>(Results.JSO, cancellationToken)
                ?? await ErrorAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    /// <summary>
    /// Returns a <see cref="Result"/> from an <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="response">The HTTP response to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized result.</returns>
    public static async Task<Result> ResultAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<Result>(Results.JSO, cancellationToken)
                ?? Result.Failure(await ErrorAsync(response, cancellationToken));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private static async Task<Error> ErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var result = JsonSerializer.Deserialize<Result>(json);
            if (result is not null)
            {
                return result.Error;
            }

            return new ResultError(FormatErrorMessage(response));
        }
        catch (Exception)
        {
            return new ResultError(FormatErrorMessage(response));
        }
    }

    private static String FormatErrorMessage(HttpResponseMessage response)
    {
        return $"Request to {response.RequestMessage?.RequestUri} failed with {response.StatusCode}: {response.RequestMessage}.";
    }
}
