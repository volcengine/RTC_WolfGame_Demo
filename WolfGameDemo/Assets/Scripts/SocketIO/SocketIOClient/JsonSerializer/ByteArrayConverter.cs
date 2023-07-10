/* MIT License
 *
 * Copyright (c) 2019 HeroWong
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocketIOClient.JsonSerializer
{
    class ByteArrayConverter : JsonConverter<byte[]>
    {
        public ByteArrayConverter()
        {
            Bytes = new List<byte[]>();
        }


        public List<byte[]> Bytes { get; }

        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            byte[] bytes = null;
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "_placeholder")
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.True && reader.GetBoolean())
                    {
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "num")
                        {
                            reader.Read();
                            int num = reader.GetInt32();
                            bytes = Bytes[num];
                            reader.Read();
                        }
                    }
                }
            }
            return bytes;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            Bytes.Add(value);
            writer.WriteStartObject();
            writer.WritePropertyName("_placeholder");
            writer.WriteBooleanValue(true);
            writer.WritePropertyName("num");
            writer.WriteNumberValue(Bytes.Count - 1);
            writer.WriteEndObject();
        }
    }
}
