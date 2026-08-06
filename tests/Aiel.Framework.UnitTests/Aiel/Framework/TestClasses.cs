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

public sealed class DependencyAInitializer : IInitializer
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyBInitializer : IInitializer
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyA;
public sealed class DependencyB;
public sealed class DependencyC;
public sealed class DependencyD;

public sealed class DependencyAConfigurator : IConfigurator
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyBConfigurator : IConfigurator
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyCConfigurator : IConfigurator
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyDConfigurator : IConfigurator
{
    public static Int32 InvokeCount { get; private set; }

    public static void Reset() => InvokeCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        InvokeCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyAPreConfigurator : IConfigurator
{
    public static Int32 PreConfigureCount { get; private set; }

    public static void Reset() => PreConfigureCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PreConfigureCount++;
        return Task.CompletedTask;
    }

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class DependencyBPreConfigurator : IConfigurator
{
    public static Int32 PreConfigureCount { get; private set; }

    public static void Reset() => PreConfigureCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PreConfigureCount++;
        return Task.CompletedTask;
    }

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class DependencyCPreConfigurator : IConfigurator
{
    public static Int32 PreConfigureCount { get; private set; }

    public static void Reset() => PreConfigureCount = 0;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PreConfigureCount++;
        return Task.CompletedTask;
    }

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class DependencyDPreConfigurator : IConfigurator
{
    public static Int32 PreConfigureCount { get; private set; }

    public static void Reset() => PreConfigureCount = 0;

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PreConfigureCount++;
        return Task.CompletedTask;
    }
}

public sealed class DependencyAPhaseConfigurator : PhaseLogCollector, IConfigurator
{
    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("A:Pre");
        return Task.CompletedTask;
    }

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("A:Configure");
        return Task.CompletedTask;
    }
}

public sealed class DependencyBPhaseConfigurator : PhaseLogCollector, IConfigurator
{
    public Task PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("B:Pre");
        return Task.CompletedTask;
    }

    public Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        PhaseLog.Add("B:Configure");
        return Task.CompletedTask;
    }
}

public class PhaseLogCollector
{
    public static readonly List<String> PhaseLog = [];
}
