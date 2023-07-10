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

namespace SocketIOClient.Messages
{
    public static class MessageFactory
    {
        private static IMessage CreateMessage(MessageType type)
        {
            switch (type)
            {
                case MessageType.Opened:
                    return new OpenedMessage();
                case MessageType.Ping:
                    return new PingMessage();
                case MessageType.Pong:
                    return new PongMessage();
                case MessageType.Connected:
                    return new ConnectedMessage();
                case MessageType.Disconnected:
                    return new DisconnectedMessage();
                case MessageType.EventMessage:
                    return new EventMessage();
                case MessageType.AckMessage:
                    return new ClientAckMessage();
                case MessageType.ErrorMessage:
                    return new ErrorMessage();
                case MessageType.BinaryMessage:
                    return new BinaryMessage();
                case MessageType.BinaryAckMessage:
                    return new ClientBinaryAckMessage();
            }
            return null;
        }

        public static IMessage CreateMessage(int eio, string msg)
        {
            var enums = Enum.GetValues(typeof(MessageType));
            foreach (MessageType item in enums)
            {
                string prefix = ((int)item).ToString();
                if (msg.StartsWith(prefix))
                {
                    IMessage result = CreateMessage(item);
                    if (result != null)
                    {
                        result.Eio = eio;
                        result.Read(msg.Substring(prefix.Length));
                        return result;
                    }
                }
            }
            return null;
        }

        public static OpenedMessage CreateOpenedMessage(string msg)
        {
            var openedMessage = new OpenedMessage();
            if (msg[0] == '0')
            {
                openedMessage.Eio = 4;
                openedMessage.Read(msg.Substring(1));
            }
            else
            {
                openedMessage.Eio = 3;
                int index = msg.IndexOf(':');
                openedMessage.Read(msg.Substring(index + 2));
            }
            return openedMessage;
        }
    }
}
