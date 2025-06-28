using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Docker.DotNet;

// Adapted from https://github.com/dotnet/runtime/issues/74385#issuecomment-1705083109
internal sealed class JsonEnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    static Dictionary<string, string> nameCompareMap = new(StringComparer.OrdinalIgnoreCase);

    static Dictionary<int, string> valueCompareMap = new();

    static JsonEnumMemberConverter()
    {
        var members = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => (Value: (int)f.GetValue(null), f.Name, AttributeName: f.GetCustomAttribute<EnumMemberAttribute>()?.Value))
                .ToArray();

        nameCompareMap = members
            .Where(pair => pair.AttributeName != null)
            .ToDictionary(e => e.Name, e => e.AttributeName);

        valueCompareMap = members
            .ToDictionary(e => e.Value, e => e.AttributeName);

    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt64(out var value))
        {
            return (TEnum)((object)value);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            return Enum.Parse<TEnum>(reader.GetString(), true);
        }

        throw new JsonException($"Cannot convert {reader.TokenType} to {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var name = Enum.GetName(value);

        if (nameCompareMap.TryGetValue(name, out var v))
        {
            writer.WriteStringValue(v);
        }
        else if (valueCompareMap.TryGetValue(Convert.ToInt32(value), out var intValue))
        {
            writer.WriteStringValue(intValue);
        }
        else
        {
            // Fallback to the default enum name if no EnumMember attribute is found
            writer.WriteStringValue(name);
        }
    }
}