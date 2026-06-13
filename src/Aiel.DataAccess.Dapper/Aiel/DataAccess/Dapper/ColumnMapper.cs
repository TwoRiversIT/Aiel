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

using Dapper;
using System.Reflection;

namespace Aiel.DataAccess.Dapper;

/// <summary>
/// Provides methods to configure Dapper column-to-property mappings using <see cref="ColumnNameAttribute"/> decorations.
/// </summary>
public static class ColumnMapper
{
    /// <summary>
    /// Searches for all types decorated with <see cref="HasColumnMapsAttribute"/> in the calling assembly
    /// and maps the properties that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <remarks>The search does not include inherited attributes; only attributes applied directly to the
    /// discovered types are matched. If an assembly cannot load all of its types, only the successfully
    /// loaded types are mapped.</remarks>
    public static void MapTypesFromCallingAssembly()
    {
        MapTypesFromAssemblies(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Searches for all types decorated with <see cref="HasColumnMapsAttribute"/> in the executing assembly
    /// and maps the properties that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <remarks>
    /// It always returns the assembly that contains the currently executing method. Not the
    /// caller. Not the entry point.Not the project referencing it. If you want the caller’s
    /// assembly, you use <see cref="Assembly.GetCallingAssembly"/>. If you want the entry
    /// assembly, you use <see cref="Assembly.GetExecutingAssembly"/>.
    /// </remarks>
    public static void MapTypesFromExecutingAssembly()
    {
        MapTypesFromAssemblies(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Searches for all types decorated with <see cref="HasColumnMapsAttribute"/> in the assembly that contains the specified <typeparamref name="T"/>
    /// and maps the properties that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <remarks>The search does not include inherited attributes; only attributes applied directly to the
    /// discovered types are matched. If an assembly cannot load all of its types, only the successfully
    /// loaded types are mapped.</remarks>
    /// <typeparam name="T">The type whose containing assembly will be scanned for types to map.</typeparam>
    public static void MapTypesFromAssemblyContaining<T>()
    {
        MapTypesFromAssemblies(typeof(T).Assembly);
    }

    /// <summary>
    /// Searches for all types decorated with <see cref="HasColumnMapsAttribute"/> in the provided <paramref name="assemblies"/>
    /// and maps the properties that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <remarks>The search does not include inherited attributes; only attributes applied directly to the
    /// discovered types are matched. If an assembly cannot load all of its types, only the successfully
    /// loaded types are mapped.</remarks>
    /// <param name="assemblies">The assemblies to search for types with the specified attribute. At least one assembly must be provided.</param>
    public static void MapTypesFromAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        var list = assemblies.SelectMany(a =>
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(static t => t != null);
            }
        }).Where(t => t?.IsDefined(typeof(HasColumnMapsAttribute), inherit: false) == true)
          .ToArray();

        foreach (var type in list)
        {
            if (type == null)
            {
                continue;
            }

            MapColumns(type);
        }
    }

    /// <summary>
    /// Maps the properties of <typeparamref name="T"/> that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The type to configure column mappings for.</typeparam>
    public static void Map<T>()
    {
        var type = typeof(T);

        CustomPropertyTypeMap map = new(type, (_, columnName) => type.GetProperties().FirstOrDefault(prop => prop.GetCustomAttribute<ColumnNameAttribute>(false)?.Name == columnName)!);

        SqlMapper.SetTypeMap(type, map);
    }

    /// <summary>
    /// Maps the properties <paramref name="type"/> that are decorated with <see cref="ColumnNameAttribute"/>.
    /// </summary>
    /// <param name="type">The type to configure column mappings for.</param>
    public static void MapColumns(this Type type)
    {
        SqlMapper.SetTypeMap(
            type,
            new CustomPropertyTypeMap(
                type,
                (_, columnName) => type.GetProperties()
                    .FirstOrDefault(prop =>
                        prop.GetCustomAttribute<ColumnNameAttribute>(false)?.Name == columnName)!
            )
        );
    }
}
