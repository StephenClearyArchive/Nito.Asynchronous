using System;

namespace Nito.Communication
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Async;

    public static class SocketHelpers
    {
        public static IList<ArraySegment<byte>> RemainingBuffers(IList<ArraySegment<byte>> buffers, int bytesWritten)
        {
            var ret = new List<ArraySegment<byte>>();
            int index;
            for (index = 0; index != buffers.Count && bytesWritten != 0; ++index)
            {
                var buffer = buffers[index];
                if (bytesWritten >= buffer.Count)
                {
                    bytesWritten -= buffer.Count;
                }
                else
                {
                    ret.Add(new ArraySegment<byte>(buffer.Array, buffer.Offset + bytesWritten, buffer.Count - bytesWritten));
                    bytesWritten = 0;
                }
            }

            ret.AddRange(buffers.Skip(index));
            return ret;
        }
    }
}
