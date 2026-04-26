using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DFApp.Web.Infrastructure
{
    /// <summary>
    /// 整数与字符串 JSON 转换器
    /// 读取 JSON 时将字符串值转为整数，写入 JSON 时将整数值转为字符串
    /// </summary>
    public class IntToStringConverter : JsonConverter<int>
    {
        /// <summary>
        /// 读取 JSON 值并转换为整数
        /// 支持 JSON 中以字符串或数字形式表示的整数
        /// </summary>
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int result))
                {
                    return result;
                }

                throw new JsonException($"无法将字符串 \"{stringValue}\" 转换为整数。");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            throw new JsonException($"意外的 JSON 令牌类型: {reader.TokenType}，期望字符串或数字。");
        }

        /// <summary>
        /// 将整数值写入 JSON 时转为字符串
        /// </summary>
        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
