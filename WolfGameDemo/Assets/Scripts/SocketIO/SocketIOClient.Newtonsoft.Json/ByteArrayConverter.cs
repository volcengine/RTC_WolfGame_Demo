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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketIOClient.Newtonsoft.Json
{
    class ByteArrayConverter : JsonConverter
    {
        public ByteArrayConverter()
        {
            Bytes = new List<byte[]>();
        }

        internal List<byte[]> Bytes { get; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, global::Newtonsoft.Json.JsonSerializer serializer)
        {
            byte[] bytes = null;
            if (reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == "_placeholder")
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.Boolean && (bool)reader.Value)
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == "num")
                        {
                            reader.Read();
                            if (reader.Value != null)
                            {
                                if (int.TryParse(reader.Value.ToString(), out int num))
                                {
                                    bytes = Bytes[num];
                                    reader.Read();
                                }
                            }
                        }
                    }
                }
            }
            return bytes;
        }

        public override void WriteJson(JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer serializer)
        {
            var source = value as byte[];
            Bytes.Add(source.ToArray());
            writer.WriteStartObject();
            writer.WritePropertyName("_placeholder");
            writer.WriteValue(true);
            writer.WritePropertyName("num");
            writer.WriteValue(Bytes.Count - 1);
            writer.WriteEndObject();
        }
    }
}
