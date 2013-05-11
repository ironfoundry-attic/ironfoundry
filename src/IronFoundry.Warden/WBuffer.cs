namespace IronFoundry.Warden
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IronFoundry.Warden.Protocol;
    using NLog;
    using ProtoBuf;

    public class WBuffer : IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int MemStreamSize = 32768;

        private MemoryStream ms = new MemoryStream(MemStreamSize);

        private bool disposed = false;

        public void Push(byte[] data, int count)
        {
            log.Trace("WBuffer.Push({0}, {1}) (ms.Position:{2})", data.GetHashCode(), count, ms.Position);
            ms.Write(data, 0, count);
        }

        public IEnumerable<Message> GetMessages()
        {
            log.Trace("WBuffer.GetMessages() START");
            var rv = ParseData();
            log.Trace("WBuffer.GetMessages() (rv.Count {0})", rv.Count);
            return rv;
        }

        public void Dispose()
        {
            if (!disposed && ms != null)
            {
                ms.Dispose();
            }
            disposed = true;
        }

        private List<Message> ParseData()
        {
            var messages = new List<Message>();

            int lastDecodedMessagePos = -1;
            byte[] data = ms.ToArray();
            if (data.Length > 0)
            {
                int startPos = 0;
                do
                {
                    int crlfIdx = CrlfIdx(data, startPos);
                    if (crlfIdx < 0)
                    {
                        break;
                    }

                    int skip1stCrlfIdx = crlfIdx + 2;

                    int crlf2Idx = CrlfIdx(data, skip1stCrlfIdx);
                    if (crlf2Idx < 0)
                    {
                        break;
                    }

                    int skip2ndCrlfIdx = crlf2Idx + 2;

                    int countBytesForDataLengthStr = crlfIdx - startPos;
                    int dataLength = Convert.ToInt32(Encoding.ASCII.GetString(data, startPos, countBytesForDataLengthStr));

                    Message m = DecodePayload(data, skip1stCrlfIdx, dataLength);
                    messages.Add(m);

                    startPos = lastDecodedMessagePos = skip2ndCrlfIdx;
                } while (true);
            }

            if (lastDecodedMessagePos != -1 && data != null)
            {
                ResetStreamWith(data, lastDecodedMessagePos);
            }

            return messages;
        }

        private void ResetStreamWith(byte[] data, int startPos)
        {
            int count = data.Length - startPos;
            log.Trace("WBuffer.ResetStreamWith({0}, {1}) (data.Length:{2}, count:{3})",
                data.GetHashCode(), startPos, data.Length, count);
            ms.Dispose();
            ms = null;
            ms = new MemoryStream(MemStreamSize);
            if (count > 0)
            {
                ms.Write(data, startPos, count);
            }
        }

        private static Message DecodePayload(byte[] data, int startPos, int length)
        {
            using (var str = new MemoryStream(data, startPos, length))
            {
                return Serializer.Deserialize<Message>(str);
            }
        }

        private static int CrlfIdx(byte[] data, int startPos)
        {
            int idx = -1,
                i = startPos,
                j = 0,
                len = data.Length;
            for (; i < len; ++i)
            {
                if (data[i] == Constants.CR)
                {
                    j = i + 1;
                    if (j < len)
                    {
                        if (data[j] == Constants.LF)
                        {
                            idx = i;
                            break;
                        }
                    }
                }
            }
            return idx;
        }
    }
}
