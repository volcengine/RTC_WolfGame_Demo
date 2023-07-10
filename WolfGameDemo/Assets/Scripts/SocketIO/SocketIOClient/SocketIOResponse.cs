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

using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SocketIOClient
{
    public class SocketIOResponse
    {
        public SocketIOResponse(IList<JsonElement> array, SocketIO socket)
        {
            _array = array;
            InComingBytes = new List<byte[]>();
            SocketIO = socket;
            PacketId = -1;
        }

        readonly IList<JsonElement> _array;

        public List<byte[]> InComingBytes { get; }
        public SocketIO SocketIO { get; }
        public int PacketId { get; set; }

        public T GetValue<T>(int index = 0)
        {
            var element = GetValue(index);
            string json = element.GetRawText();
            return SocketIO.JsonSerializer.Deserialize<T>(json, InComingBytes);
        }

        public JsonElement GetValue(int index = 0) => _array[index];

        public int Count => _array.Count;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[');
            foreach (var item in _array)
            {
                builder.Append(item.GetRawText());
                if (_array.IndexOf(item) < _array.Count - 1)
                {
                    builder.Append(',');
                }
            }
            builder.Append(']');
            return builder.ToString();
        }

        public void CallbackAsync(params object[] data)
        {
            SocketIO.ClientAckAsync(PacketId, data);
        }
    }
}
