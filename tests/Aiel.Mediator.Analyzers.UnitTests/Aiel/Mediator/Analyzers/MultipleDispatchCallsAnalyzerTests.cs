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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Mediator.Analyzers;

public sealed class MultipleDispatchCallsAnalyzerTests
{
    // Minimal in-memory stub of the Aiel.Dispatch interfaces.
    // The analyzer matches by name, so this is sufficient without shipping an assembly reference.
    private const String MediatorStub = """
        using System.Threading;
        using System.Threading.Tasks;

        namespace Aiel.Dispatch
        {
            public interface IAction { }
            public interface ICommand : IAction { }
            public interface IQuery<TResult> : IAction { }
            public interface INotification { }

            public interface ISender
            {
                Task ExecuteAsync(ICommand command, CancellationToken ct = default);
                Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
            }

            public interface IPublisher
            {
                Task PublishAsync<TNotification>(TNotification notification, CancellationToken ct = default)
                    where TNotification : INotification;
            }
        }
        """;

    // -------------------------------------------------------------------------
    // Positive cases — diagnostic MUST fire
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Reports_WhenMethodCallsExecuteAsyncTwice()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record CommandA : ICommand;
            public record CommandB : ICommand;

            public class Service(ISender sender)
            {
                public async Task DoWorkAsync(CancellationToken ct)
                {
                    await sender.ExecuteAsync(new CommandA(), ct);
                    await sender.ExecuteAsync(new CommandB(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var d = Assert.Single(diagnostics);
        Assert.Equal("TRMD0001", d.Id);
        Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
        Assert.Contains("DoWorkAsync", d.GetMessage());
        Assert.Contains("2", d.GetMessage());
    }

    [Fact]
    public async Task Reports_WhenMethodMixesExecuteAsyncAndQueryAsync()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record SomeCommand : ICommand;
            public record SomeQuery : IQuery<int>;

            public class Service(ISender sender)
            {
                public async Task<int> DoWorkAsync(CancellationToken ct)
                {
                    await sender.ExecuteAsync(new SomeCommand(), ct);
                    return await sender.QueryAsync(new SomeQuery(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var d = Assert.Single(diagnostics);
        Assert.Equal("TRMD0001", d.Id);
    }

    [Fact]
    public async Task Reports_WhenPublisherCallsCombineWithSenderCalls()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record SomeCommand : ICommand;
            public record SomeEvent : INotification;

            public class Service(ISender sender, IPublisher publisher)
            {
                public async Task DoWorkAsync(CancellationToken ct)
                {
                    await sender.ExecuteAsync(new SomeCommand(), ct);
                    await publisher.PublishAsync(new SomeEvent(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var d = Assert.Single(diagnostics);
        Assert.Equal("TRMD0001", d.Id);
        Assert.Contains("2", d.GetMessage());
    }

    [Fact]
    public async Task Reports_CountReflectsAllCallSites()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record Cmd : ICommand;

            public class Service(ISender sender)
            {
                public async Task DoWorkAsync(CancellationToken ct)
                {
                    await sender.ExecuteAsync(new Cmd(), ct);
                    await sender.ExecuteAsync(new Cmd(), ct);
                    await sender.ExecuteAsync(new Cmd(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        var d = Assert.Single(diagnostics);
        Assert.Contains("3", d.GetMessage());
    }

    // -------------------------------------------------------------------------
    // Negative cases — diagnostic must NOT fire
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DoesNotReport_WhenMethodCallsDispatchOnce()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record SomeCommand : ICommand;

            public class Service(ISender sender)
            {
                public async Task DoWorkAsync(CancellationToken ct)
                {
                    await sender.ExecuteAsync(new SomeCommand(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenTwoMethodsEachCallDispatchOnce()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record CommandA : ICommand;
            public record CommandB : ICommand;

            public class Service(ISender sender)
            {
                public async Task FirstAsync(CancellationToken ct)
                    => await sender.ExecuteAsync(new CommandA(), ct);

                public async Task SecondAsync(CancellationToken ct)
                    => await sender.ExecuteAsync(new CommandB(), ct);
            }
            """;

        var diagnostics = await AnalyzeAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenUnrelatedMethodSharesSameName()
    {
        const String source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public record SomeCommand : ICommand;

            public interface INotISender
            {
                Task ExecuteAsync(SomeCommand cmd, CancellationToken ct);
                Task ExecuteAsync(SomeCommand cmd2, CancellationToken ct2);
            }

            public class Service(INotISender notSender)
            {
                public async Task DoWorkAsync(CancellationToken ct)
                {
                    await notSender.ExecuteAsync(new SomeCommand(), ct);
                    await notSender.ExecuteAsync(new SomeCommand(), ct);
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task DoesNotReport_WhenMethodBodyIsEmpty()
    {
        const String source = """
            using System.Threading.Tasks;
            using Aiel.Dispatch;

            public class Service(ISender sender)
            {
                public Task NothingAsync() => Task.CompletedTask;
            }
            """;

        var diagnostics = await AnalyzeAsync(source);
        Assert.Empty(diagnostics);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(String source)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
        };

        // Add System.Runtime so ValueTask and other BCL types resolve inside the stub.
        var runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Runtime");
        if (runtimeAssembly is not null)
        {
            references.Add(MetadataReference.CreateFromFile(runtimeAssembly.Location));
        }

        // Emit the mediator stub into memory so the analyzer can resolve ISender/IPublisher symbols.
        var stubTree = CSharpSyntaxTree.ParseText(MediatorStub);
        var stubCompilation = CSharpCompilation.Create(
            "Aiel.Dispatch.Stub",
            [stubTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        await using var stubStream = new MemoryStream();
        var emitResult = stubCompilation.Emit(stubStream);
        if (!emitResult.Success)
        {
            var errors = String.Join(", ", emitResult.Diagnostics.Select(d => d.ToString()));
            throw new InvalidOperationException($"Failed to emit mediator stub: {errors}");
        }

        stubStream.Position = 0;
        references.Add(MetadataReference.CreateFromImage(stubStream.ToArray()));

        var testTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "MultipleDispatchCallsAnalyzerTests",
            [testTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new MultipleDispatchCallsAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
