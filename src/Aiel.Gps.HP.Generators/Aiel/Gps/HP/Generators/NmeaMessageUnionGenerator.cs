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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Text;

namespace Aiel.Gps.HP.Generators;

/// <summary>
/// Source generator that creates a discriminated union for NMEA message types.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class NmeaMessageUnionGenerator : IIncrementalGenerator
{
    private const String NmeaMessageAttributeName = "Aiel.Gps.HP.NmeaMessageAttribute";
    private const String NmeaParserAttributeName = "Aiel.Gps.HP.NmeaParserAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all structs with [NmeaMessage] attribute
        var messageTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsStructWithAttribute(node),
                transform: static (ctx, _) => GetMessageInfo(ctx))
            .Where(static m => m is not null);

        // Find all structs with [NmeaParser] attribute
        var parserTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsStructWithAttribute(node),
                transform: static (ctx, _) => GetParserInfo(ctx))
            .Where(static p => p is not null);

        // Combine and generate
        var combined = messageTypes.Collect().Combine(parserTypes.Collect());

        context.RegisterSourceOutput(combined, (spc, data) =>
        {
            var messages = data.Left.Where(m => m is not null).Cast<MessageInfo>().ToList();
            var parsers = data.Right.Where(p => p is not null).Cast<ParserInfo>().ToList();

            if (messages.Count > 0)
            {
                GenerateUnion(spc, messages, parsers);
            }
        });
    }

    private static Boolean IsStructWithAttribute(SyntaxNode node)
    {
        return node is StructDeclarationSyntax sds && sds.AttributeLists.Count > 0;
    }

    private static MessageInfo? GetMessageInfo(GeneratorSyntaxContext ctx)
    {
        var sds = (StructDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(sds) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == NmeaMessageAttributeName)
            {
                var identifier = attr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
                return new MessageInfo(
                    symbol.Name,
                    symbol.ContainingNamespace?.ToDisplayString() ?? "",
                    identifier);
            }
        }

        return null;
    }

    private static ParserInfo? GetParserInfo(GeneratorSyntaxContext ctx)
    {
        var sds = (StructDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(sds) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == NmeaParserAttributeName)
            {
                var messageType = attr.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                if (messageType is not null)
                {
                    return new ParserInfo(
                        symbol.Name,
                        symbol.ContainingNamespace?.ToDisplayString() ?? "",
                        messageType.Name,
                        messageType.ContainingNamespace?.ToDisplayString() ?? "");
                }
            }
        }

        return null;
    }

    private static void GenerateUnion(
        SourceProductionContext context,
        List<MessageInfo> messages,
        List<ParserInfo> parsers)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(GenerateHeader());

        // Nullable enable
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Usings
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine();

        // Add usings for all message namespaces
        var namespaces = messages.Select(m => m.Namespace).Distinct().Where(n => !String.IsNullOrEmpty(n));
        foreach (var ns in namespaces)
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member");
        sb.AppendLine();

        sb.AppendLine("namespace Aiel.Gps.HP;");
        sb.AppendLine();

        // Generate enum
        GenerateEnum(sb, messages);
        sb.AppendLine();

        // Generate union struct
        GenerateUnionStruct(sb, messages, parsers);

        context.AddSource("NmeaMessage.g.cs", sb.ToString());
    }

    private static void GenerateEnum(StringBuilder sb, List<MessageInfo> messages)
    {
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Identifies the type of NMEA message stored in an <see cref=\"NmeaMessage\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public enum NmeaMessageType : System.Byte");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>No message or unknown type.</summary>");
        sb.AppendLine("    None = 0,");
        sb.AppendLine();

        for (var i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];
            sb.AppendLine($"    /// <summary>{msg.Name} message ({msg.Identifier}).</summary>");
            sb.AppendLine($"    {msg.Name} = {i + 1},");
        }

        sb.AppendLine("}");
    }

    private static void GenerateUnionStruct(StringBuilder sb, List<MessageInfo> messages, List<ParserInfo> parsers)
    {
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// A discriminated union that can hold any supported NMEA message type.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("/// <remarks>");
        sb.AppendLine("/// <para>");
        sb.AppendLine("/// This struct stores each message type in a separate field. While this uses more memory");
        sb.AppendLine("/// than a true overlapping union, it is necessary because some message types contain");
        sb.AppendLine("/// reference types (strings) which cannot overlap with value types in the CLR.");
        sb.AppendLine("/// </para>");
        sb.AppendLine("/// <para>");
        sb.AppendLine("/// Only one message type is valid at a time, determined by <see cref=\"Type\"/>.");
        sb.AppendLine("/// </para>");
        sb.AppendLine("/// </remarks>");
        sb.AppendLine("public partial struct NmeaMessage");
        sb.AppendLine("{");

        // Type field
        sb.AppendLine("    /// <summary>Gets the type of message stored in this union.</summary>");
        sb.AppendLine("    public readonly NmeaMessageType Type;");
        sb.AppendLine();

        // Message fields (non-overlapping)
        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <summary>Gets the {msg.Name} message. Only valid when <see cref=\"Type\"/> is <see cref=\"NmeaMessageType.{msg.Name}\"/>.</summary>");
            sb.AppendLine($"    public {msg.FullName} {msg.Name};");
            sb.AppendLine();
        }

        // IsXxx properties
        sb.AppendLine("    #region Type Checking Properties");
        sb.AppendLine();

        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <summary>Gets a value indicating whether this message is a {msg.Name}.</summary>");
            sb.AppendLine($"    public readonly Boolean Is{msg.Name} => Type == NmeaMessageType.{msg.Name};");
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
        sb.AppendLine();

        // TryGetXxx methods
        sb.AppendLine("    #region TryGet Methods");
        sb.AppendLine();

        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <summary>Attempts to get the message as a {msg.Name}.</summary>");
            sb.AppendLine($"    /// <param name=\"message\">When this method returns true, contains the {msg.Name} message.</param>");
            sb.AppendLine($"    /// <returns>True if this is a {msg.Name} message; otherwise, false.</returns>");
            sb.AppendLine($"    public readonly Boolean TryGet{msg.Name}(out {msg.FullName} message)");
            sb.AppendLine("    {");
            sb.AppendLine($"        if (Type == NmeaMessageType.{msg.Name})");
            sb.AppendLine("        {");
            sb.AppendLine($"            message = {msg.Name};");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        message = default;");
            sb.AppendLine("        return false;");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
        sb.AppendLine();

        // Private constructor for factory methods
        sb.AppendLine("    private NmeaMessage(NmeaMessageType type)");
        sb.AppendLine("    {");
        sb.AppendLine("        Type = type;");

        // Initialize all message fields to default
        foreach (var msg in messages)
        {
            sb.AppendLine($"        {msg.Name} = default;");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // Static factory methods
        sb.AppendLine("    #region Factory Methods");
        sb.AppendLine();

        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <summary>Creates a new <see cref=\"NmeaMessage\"/> containing a {msg.Name}.</summary>");
            sb.AppendLine($"    public static NmeaMessage From{msg.Name}({msg.FullName} message)");
            sb.AppendLine("    {");
            sb.AppendLine($"        var result = new NmeaMessage(NmeaMessageType.{msg.Name});");
            sb.AppendLine($"        result.{msg.Name} = message;");
            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("    #endregion");
        sb.AppendLine();

        // TryParse static method
        GenerateTryParseMethod(sb, messages, parsers);

        // Match method for exhaustive pattern matching
        GenerateMatchMethod(sb, messages);

        // ToString override
        sb.AppendLine("    /// <inheritdoc/>");
        sb.AppendLine("    public override readonly String ToString()");
        sb.AppendLine("    {");
        sb.AppendLine("        return Type switch");
        sb.AppendLine("        {");

        foreach (var msg in messages)
        {
            sb.AppendLine($"            NmeaMessageType.{msg.Name} => {msg.Name}.ToString(),");
        }

        sb.AppendLine("            _ => $\"NmeaMessage(Type={Type})\"");
        sb.AppendLine("        };");
        sb.AppendLine("    }");

        sb.AppendLine("}");
    }

    private static void GenerateTryParseMethod(StringBuilder sb, List<MessageInfo> messages, List<ParserInfo> parsers)
    {
        sb.AppendLine("    #region Parsing");
        sb.AppendLine();

        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Attempts to parse an NMEA sentence into a message.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"sentence\">The NMEA sentence bytes.</param>");
        sb.AppendLine("    /// <param name=\"message\">When this method returns true, contains the parsed message.</param>");
        sb.AppendLine("    /// <returns>True if the sentence was parsed successfully; otherwise, false.</returns>");
        sb.AppendLine("    public static Boolean TryParse(ReadOnlySpan<Byte> sentence, out NmeaMessage message)");
        sb.AppendLine("    {");
        sb.AppendLine("        var lexer = new Lexer(sentence);");
        sb.AppendLine("        var identifier = lexer.PeekIdentifier();");
        sb.AppendLine();

        // Generate switch based on identifier
        // For performance, we check identifier length first, then compare bytes
        var messagesByIdentifier = messages.ToDictionary(m => m.Identifier, m => m);

        foreach (var msg in messages)
        {
            var parser = parsers.FirstOrDefault(p => p.MessageTypeName == msg.Name);
            var parserName = parser?.FullName ?? $"{msg.Name}Parser";

            sb.AppendLine($"        // Check for {msg.Identifier}");
            sb.AppendLine($"        if (identifier.SequenceEqual(\"{msg.Identifier}\"u8))");
            sb.AppendLine("        {");
            sb.AppendLine($"            var parser = new {parserName}();");
            sb.AppendLine($"            parser.Parse(ref lexer, out {msg.FullName} parsed);");
            sb.AppendLine($"            message = From{msg.Name}(parsed);");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        message = default;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static void GenerateMatchMethod(StringBuilder sb, List<MessageInfo> messages)
    {
        sb.AppendLine("    #region Pattern Matching");
        sb.AppendLine();

        // Generate Match method with delegates for each type
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Invokes the appropriate handler based on the message type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <typeparam name=\"TResult\">The type of result returned by the handlers.</typeparam>");

        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <param name=\"on{msg.Name}\">Handler for {msg.Name} messages.</param>");
        }

        sb.AppendLine("    /// <param name=\"onNone\">Handler for unknown or empty messages.</param>");
        sb.AppendLine("    /// <returns>The result from the invoked handler.</returns>");

        // Build parameter list
        var parameters = new List<String>();
        foreach (var msg in messages)
        {
            parameters.Add($"Func<{msg.FullName}, TResult> on{msg.Name}");
        }

        parameters.Add("Func<TResult>? onNone = null");

        sb.AppendLine("    public readonly TResult Match<TResult>(");
        sb.AppendLine($"        {String.Join(",\r\n        ", parameters)})");
        sb.AppendLine("    {");
        sb.AppendLine("        return Type switch");
        sb.AppendLine("        {");

        foreach (var msg in messages)
        {
            sb.AppendLine($"            NmeaMessageType.{msg.Name} => on{msg.Name}({msg.Name}),");
        }

        sb.AppendLine("            _ => onNone is not null ? onNone() : throw new InvalidOperationException($\"Unhandled message type: {Type}\")");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate void Match variant
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Invokes the appropriate handler based on the message type.");
        sb.AppendLine("    /// </summary>");

        foreach (var msg in messages)
        {
            sb.AppendLine($"    /// <param name=\"on{msg.Name}\">Handler for {msg.Name} messages.</param>");
        }

        sb.AppendLine("    /// <param name=\"onNone\">Handler for unknown or empty messages.</param>");

        var actionParameters = new List<String>();
        foreach (var msg in messages)
        {
            actionParameters.Add($"Action<{msg.FullName}> on{msg.Name}");
        }

        actionParameters.Add("Action? onNone = null");

        sb.AppendLine("    public readonly void Match(");
        sb.AppendLine($"        {String.Join(",\r\n        ", actionParameters)})");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (Type)");
        sb.AppendLine("        {");

        foreach (var msg in messages)
        {
            sb.AppendLine($"            case NmeaMessageType.{msg.Name}:");
            sb.AppendLine($"                on{msg.Name}({msg.Name});");
            sb.AppendLine("                break;");
        }

        sb.AppendLine("            default:");
        sb.AppendLine("                if (onNone is not null) onNone();");
        sb.AppendLine("                else throw new InvalidOperationException($\"Unhandled message type: {Type}\");");
        sb.AppendLine("                break;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    #endregion");
        sb.AppendLine();
    }

    private static String GenerateHeader()
    {
        var assemblyName = typeof(NmeaMessageUnionGenerator).Assembly.GetName().Name;
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var localTime = DateTime.UtcNow.ToLocalTime().ToString("o");

        return $@"// <auto-generated>
//   This file was brought to you by {assemblyName}
//   Version: {version}
//   Generator: NmeaMessageUnionGenerator
//   Generated at: {localTime}
//
//   DO NOT EDIT THIS FILE BY HAND OR THE WORLD MAY END!
//   (Seriously. The generator will overwrite your changes anyway.)
//
// </auto-generated>

";
    }

    private sealed class MessageInfo(String name, String ns, String identifier)
    {
        public String Name { get; } = name;
        public String Namespace { get; } = ns;
        public String Identifier { get; } = identifier;
        public String FullName => String.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
    }

    private sealed class ParserInfo(String name, String ns, String messageTypeName, String messageTypeNamespace)
    {
        public String Name { get; } = name;
        public String Namespace { get; } = ns;
        public String MessageTypeName { get; } = messageTypeName;
        public String MessageTypeNamespace { get; } = messageTypeNamespace;
        public String FullName => String.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
    }
}
