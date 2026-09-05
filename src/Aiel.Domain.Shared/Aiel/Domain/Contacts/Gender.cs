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

namespace Aiel.Domain.Contacts;

/// <summary>
/// Defines the gender of a contact. Even though this is a flags enum, only
/// one gender should be assigned to a contact.
/// </summary>
/// <remarks>
/// This flags enum is generally used for filtering and searching contacts
/// by gender, not for assigning multiple genders to a single contact. For a single contact, only one gender should be assigned.
/// </remarks>
[Flags]
public enum Gender
{
    /// <summary>
    /// The default value indicating that no gender has been specified. This value should not be used for filtering or searching contacts.
    /// </summary>
    None = 0,
    /// <summary>
    /// Indicates that the gender is male.
    /// </summary>
    Male = 1,
    /// <summary>
    /// Indicates that the gender is female.
    /// </summary>
    Female = 1 << 1,
    /// <summary>
    /// Indicates that the gender is non-binary.
    /// </summary>
    NonBinary = 1 << 2,
    /// <summary>
    /// Indicates that the gender is other.
    /// </summary>
    Other = 1 << 3,
    /// <summary>
    /// Indicates that the gender is prefer not to say.
    /// </summary>
    PreferNotToSay = 1 << 4,
}
