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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aiel.Framework;

public class AddValidatedOptionsTests
{
    [Fact]
    public void AddValidatedOptions_ShouldAddOptionsAndValidator()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddValidatedOptions<TestOptions, TestOptionsValidator>(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<TestOptions>>();
        var validator = serviceProvider.GetService<IValidateOptions<TestOptions>>();

        options.Should().NotBeNull();
        validator.Should().NotBeNull();
    }

    [Fact]
    public void GettingOptionsValue_WhenOptionsAreValid_ShouldReturnOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<String, String?>
            {
                { "TestOptions:StringOption", "ValidValue" }
            })
            .Build();
        var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration);
        services.AddValidatedOptions<TestOptions, TestOptionsValidator>(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();

        // Act
        var optionsValue = options.Value;

        // Assert
        optionsValue.Should().NotBeNull();
        optionsValue.StringOption.Should().Be("ValidValue");
    }

    [Fact]
    public void GettingOptionsValue_WhenOptionsAreInvalid_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration);
        services.AddValidatedOptions<TestOptions, TestOptionsValidator>(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>();

        // Act
        Action act = () => { var _ = options.Value; };

        // Assert
        act.Should().Throw<OptionsValidationException>().WithMessage("StringOption must not be null or empty.");
    }

    private class TestOptions
    {
        public String StringOption { get; set; } = String.Empty;
    }

    private class TestOptionsValidator : IValidateOptions<TestOptions>
    {
        public ValidateOptionsResult Validate(String? name, TestOptions options)
        {
            if (String.IsNullOrWhiteSpace(options.StringOption))
            {
                return ValidateOptionsResult.Fail("StringOption must not be null or empty.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
