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

namespace Aiel.Gps.HP.Sentences;

/// <summary>
/// Represents the quality of a GPS position fix.
/// </summary>
public enum FixQuality : Int32
{
    /// <summary>Fix not available or invalid.</summary>
    Invalid = 0,

    /// <summary>Standard GPS SPS (Standard Positioning Service) mode, no correction.</summary>
    GpsFix = 1,

    /// <summary>Differential GPS fix (corrections from reference station).</summary>
    DgpsFix = 2,

    /// <summary>PPS (Precise Positioning Service) fix.</summary>
    PpsFix = 3,

    /// <summary>Real Time Kinematic fix (centimeter-level accuracy).</summary>
    Rtk = 4,

    /// <summary>Float RTK (less precise than full RTK).</summary>
    FloatRtk = 5,

    /// <summary>Dead reckoning/estimated fix.</summary>
    Estimated = 6,

    /// <summary>Manual input mode.</summary>
    ManualInput = 7,

    /// <summary>Simulation mode.</summary>
    Simulation = 8
}
