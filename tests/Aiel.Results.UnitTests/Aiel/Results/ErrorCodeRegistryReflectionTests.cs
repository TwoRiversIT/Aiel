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

using Aiel.Results.TestErrors;
using Aiel.Roslyn;
using System.Reflection;

namespace Aiel.Results;

public class ErrorCodeRegistryReflectionTests
{
    [Fact]
    public void ErrorTypeRegistry_Should_ContainCustomErrors()
    {
        // Assembly initializers should automatically register the errors when the assembly loads.
        // No manual registration needed - the generator creates the assembly initializers and
        // the compiler emits code to call them when the assembly is loaded.

        // But we might have to force it.
        _ = new SimpleError(NotEmpty);

        var errorTypes = ErrorRegistry.LookupByType.Keys.ToList();

        // These should now pass because AssemblyInitializer registered them:
        errorTypes.Should().Contain(typeof(SimpleError));
        errorTypes.Should().Contain(typeof(DatabaseConnectionError));
        errorTypes.Should().Contain(typeof(InventoryInsufficientError));
        errorTypes.Should().Contain(typeof(OrderNotFoundError));
        errorTypes.Should().Contain(typeof(TransactionError));

        // Verify we have the expected count (at least our 5 custom errors)
        errorTypes.Count.Should().BeGreaterThanOrEqualTo(5, because: $"Expected at least 5 error types to be registered, but only found {errorTypes.Count}");
    }

    private static readonly Type ErrorType = typeof(Error);
    private static readonly Type ErrorCodeType = typeof(ErrorCode);
    private const String NotEmpty = "This is not an empty string.";

    [Theory]
    [InlineData(typeof(SimpleError))]
    [InlineData(typeof(DatabaseConnectionError))]
    [InlineData(typeof(InventoryInsufficientError))]
    [InlineData(typeof(OrderNotFoundError))]
    [InlineData(typeof(TransactionError))]
    public void Error_Reflection_WorksAsExpected(Type errorType)
    {
        // Validate that the provided type is a subclass of Error
        ErrorType.IsAssignableFrom(errorType).Should().BeTrue($"Expected {errorType.FullName} to be a subclass of {ErrorType.FullName}");

        // Find the Code property
        var errorCodeProp = errorType.GetProperty(GeneratorConsts.ErrorCode, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        errorCodeProp.Should().NotBeNull($"Expected {errorType.FullName} to have a 'Code' property");

        // Create an instance of the errorType
        var error = Activator.CreateInstance(errorType, NotEmpty);

        // Get the value of the Code property
        var codeValue = errorCodeProp.GetValue(error);
        codeValue.Should().NotBeNull($"Expected {errorType.FullName} to have a non-null 'Code' property value");

        // Get the type of the ErrorCode
        var errorCodeType = codeValue.GetType();
        ErrorCodeType.IsAssignableFrom(errorCodeType).Should().BeTrue($"Expected {errorCodeType.FullName} to be a subclass of {ErrorCodeType.FullName}");
        // Get the Instance property
        var instanceProp = errorCodeType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        instanceProp.Should().NotBeNull($"Expected {errorCodeType.FullName} to have a static 'Instance' field");

        // Get the value of the Instance property
        var value = instanceProp.GetValue(null) as ErrorCode;
        value.Should().NotBeNull(because: $"{errorCodeType.FullName} Instance property should return an instance of the error code.");

        var fallback = Activator.CreateInstance(errorCodeType) as ErrorCode;
        fallback.Should().NotBeNull(because: $"{errorCodeType.FullName} type should have a parameterless constructor that can be used as a fallback if the Instance property is not properly implemented.");

    }
}
