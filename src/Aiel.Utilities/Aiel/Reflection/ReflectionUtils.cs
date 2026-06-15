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

using System.Reflection;

namespace Aiel.Reflection;

public static class ReflectionUtils
{
    public static String[] GetConstants(this Type type)
    {
        const Int32 maxDepth = 5;

        var list = new List<String>();

        Recurse(list, type, 1);

        return list.ToArray();

        static void Recurse(List<String> constants, Type targetType, Int32 depth)
        {
            if (depth > maxDepth)
            {
                return;
            }

            constants.AddRange(targetType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => x.IsLiteral && !x.IsInitOnly)
                .Select(x => x.GetValue(null)!.ToString()!));

            var nestedTypes = targetType.GetNestedTypes(BindingFlags.Public);
            foreach (var nestedType in nestedTypes)
            {
                Recurse(constants, nestedType, depth + 1);
            }
        }
    }
}
