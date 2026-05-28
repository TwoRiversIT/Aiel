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

using Aiel.Actions;

namespace Aiel.Permissions.Testing.Fixtures;

/// <summary>
/// A fixture action type for use in tests only. Not a production contract.
/// </summary>
/// <remarks>Alpha variant — use when you need a single distinguishable action type.</remarks>
public sealed class AlphaTestAction : IAction;

/// <summary>
/// A fixture action type for use in tests only. Not a production contract.
/// </summary>
/// <remarks>Beta variant — use when a second distinct action type is needed alongside <see cref="AlphaTestAction"/>.</remarks>
public sealed class BetaTestAction : IAction;

/// <summary>
/// A fixture action type for use in tests only. Not a production contract.
/// </summary>
/// <remarks>Gamma variant — use when a third distinct action type is needed.</remarks>
public sealed class GammaTestAction : IAction;

/// <summary>
/// A fixture action type for rename migration tests only. Not a production contract.
/// </summary>
public sealed class ChangeAppointmentTestAction : IAction;

/// <summary>
/// A fixture action type for rename migration tests only. Not a production contract.
/// </summary>
public sealed class RescheduleAppointmentTestAction : IAction;
