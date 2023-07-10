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

namespace SocketIOClient.JsonSerializer
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        public JsonSerializeResult Serialize(object[] data)
        {
            var converter = new ByteArrayConverter();
            var options = GetOptions();
            options.Converters.Add(converter);
            string json = System.Text.Json.JsonSerializer.Serialize(data, options);
            return new JsonSerializeResult
            {
                Json = json,
                Bytes = converter.Bytes
            };
        }

        public T Deserialize<T>(string json)
        {
            var options = GetOptions();
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        }

        public T Deserialize<T>(string json, IList<byte[]> bytes)
        {
            var options = GetOptions();
            var converter = new ByteArrayConverter();
            options.Converters.Add(converter);
            converter.Bytes.AddRange(bytes);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        }

        private JsonSerializerOptions GetOptions()
        {
            JsonSerializerOptions options = null;
            if (OptionsProvider != null)
            {
                options = OptionsProvider();
            }
            if (options == null)
            {
                options = new JsonSerializerOptions();
            }
            return options;
        }

        public Func<JsonSerializerOptions> OptionsProvider { get; set; }
    }
}
