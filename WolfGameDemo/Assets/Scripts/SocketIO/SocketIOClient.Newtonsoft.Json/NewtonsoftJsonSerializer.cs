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
using Newtonsoft.Json;
using SocketIOClient.JsonSerializer;
using System.Collections.Generic;

namespace SocketIOClient.Newtonsoft.Json
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public Func<JsonSerializerSettings> JsonSerializerOptions { get; }

        public JsonSerializeResult Serialize(object[] data)
        {
            var converter = new ByteArrayConverter();
            var settings = GetOptions();
            settings.Converters.Add(converter);
            string json = JsonConvert.SerializeObject(data, settings);
            return new JsonSerializeResult
            {
                Json = json,
                Bytes = converter.Bytes
            };
        }

        public T Deserialize<T>(string json)
        {
            var settings = GetOptions();
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public T Deserialize<T>(string json, IList<byte[]> bytes)
        {
            var converter = new ByteArrayConverter();
            converter.Bytes.AddRange(bytes);
            var settings = GetOptions();
            settings.Converters.Add(converter);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        private JsonSerializerSettings GetOptions()
        {
            JsonSerializerSettings options;
            if (OptionsProvider != null)
            {
                options = OptionsProvider();
            }
            else
            {
                options = CreateOptions();
            }
            if (options == null)
            {
                options = new JsonSerializerSettings();
            }
            return options;
        }

        [Obsolete("Use Options instead.")]
        public virtual JsonSerializerSettings CreateOptions()
        {
            return new JsonSerializerSettings();
        }

        public Func<JsonSerializerSettings> OptionsProvider { get; set; }
    }
}
