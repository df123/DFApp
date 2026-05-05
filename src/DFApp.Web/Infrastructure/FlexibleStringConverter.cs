using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DFApp.Web.Infrastructure
{
    /// <summary>
    /// 灵活的字符串转换器，能将 JSON 中的数字和字符串都转换为 C# string
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString();
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32().ToString();
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            throw new JsonException($"意外的 JSON 令牌类型: {reader.TokenType}，期望字符串、数字或null。");
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
