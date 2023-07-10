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
using System.Text;

namespace SocketIOClient.UriConverters
{
    public class UriConverter : IUriConverter
    {
        public Uri GetServerUri(bool ws, Uri serverUri, int eio, string path, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            var builder = new StringBuilder();
            if (serverUri.Scheme == "https" || serverUri.Scheme == "wss")
            {
                builder.Append(ws ? "wss://" : "https://");
            }
            else if (serverUri.Scheme == "http" || serverUri.Scheme == "ws")
            {
                builder.Append(ws ? "ws://" : "http://");
            }
            else
            {
                throw new ArgumentException("Only supports 'http, https, ws, wss' protocol");
            }
            builder.Append(serverUri.Host);
            if (!serverUri.IsDefaultPort)
            {
                builder.Append(":").Append(serverUri.Port);
            }
            if (string.IsNullOrEmpty(path))
            {
                builder.Append("/socket.io");
            }
            else
            {
                builder.Append(path);
            }
            builder
                .Append("/?EIO=")
                .Append(eio)
                .Append("&transport=")
                .Append(ws ? "websocket" : "polling");

            if (queryParams != null)
            {
                foreach (var item in queryParams)
                {
                    builder.Append('&').Append(item.Key).Append('=').Append(item.Value);
                }
            }

            return new Uri(builder.ToString());
        }
    }
}
