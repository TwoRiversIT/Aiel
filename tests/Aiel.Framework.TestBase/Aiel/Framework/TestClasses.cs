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

namespace Aiel.Framework;

public abstract class TrackedConfigurator : AielDependencyConfigurator, IInitializer, IDisposable, IAsyncDisposable
{
    public Int32 PreConfigureCount { get; private set; }
    public Int32 ConfigureCount { get; private set; }
    public Int32 InitializeCount { get; private set; }
    public Boolean IsDisposed { get; private set; }
    public Boolean IsAsyncDisposed { get; private set; }

    public void Reset()
    {
        PreConfigureCount = 0;
        ConfigureCount = 0;
        InitializeCount = 0;
    }

    public override ValueTask PreConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PreConfigureCount++;
        return ValueTask.CompletedTask;
    }

    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        ConfigureCount++;
        return ValueTask.CompletedTask;
    }
    public ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        InitializeCount++;
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        IsAsyncDisposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

[DependsOn(typeof(CircularB))]
public sealed class CircularA : TrackedConfigurator;
[DependsOn(typeof(CircularA))]
public sealed class CircularB : TrackedConfigurator;

[DependsOn(typeof(DiamondB))]
[DependsOn(typeof(DiamondC))]
public sealed class DiamondA : TrackedConfigurator, IApplicationConfigurator
{
    public String ApplicationName => nameof(DiamondA);
    public String ApplicationVersion => "1.0.0";
}

[DependsOn(typeof(DiamondD))]
public sealed class DiamondB : TrackedConfigurator;
[DependsOn(typeof(DiamondD))]
public sealed class DiamondC : TrackedConfigurator;
public sealed class DiamondD : TrackedConfigurator;
[DependsOn(typeof(LinearB))]
public sealed class LinearA : TrackedConfigurator;
[DependsOn(typeof(LinearC))]
public sealed class LinearB : TrackedConfigurator;
public sealed class LinearC : TrackedConfigurator;

public sealed class PhaseA : PhaseLogCollector
{
    public override ValueTask PreConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("A:Pre");
        return base.PreConfigureAsync(context, cancellationToken);
    }

    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("A:Configure");
        return base.ConfigureAsync(context, cancellationToken);
    }
}

public sealed class PhaseB : PhaseLogCollector
{
    public override ValueTask PreConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("B:Pre");
        return base.PreConfigureAsync(context, cancellationToken);
    }

    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("B:Configure");
        return base.ConfigureAsync(context, cancellationToken);
    }
}

public class PhaseLogCollector : TrackedConfigurator
{
    public static readonly List<String> PhaseLog = [];
}
