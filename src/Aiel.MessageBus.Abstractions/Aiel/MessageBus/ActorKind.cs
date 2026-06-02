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

namespace Aiel.MessageBus;

/// <summary>
/// Transport-safe category for the actor that produced a message.
/// Uses a plain <see cref="String"/> rather than a StrongId so message metadata remains
/// transport-safe and does not expose Aiel-specific identifier types to consuming processes.
/// Use the well-known instances (<see cref="User"/>, <see cref="Service"/>, <see cref="System"/>)
/// where they apply; define additional static instances for custom actor kinds.
/// </summary>
public readonly record struct ActorKind(String Value)
{
    public static readonly ActorKind User    = new("user");
    public static readonly ActorKind Service = new("service");
    public static readonly ActorKind System  = new("system");
}
