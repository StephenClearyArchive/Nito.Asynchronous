using System;

namespace Nito.Communication
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Async;

    internal static class SocketHelpers
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

        public static Exception GetError(this SocketAsyncEventArgs @this)
        {
#if DESKTOP4 || SILVERLIGHT3 || SILVERLIGHT4
            if (@this.ConnectByNameError != null)
            {
                return @this.ConnectByNameError;
            }
#endif

            if (@this.SocketError != SocketError.Success)
            {
                return new SocketException((int)@this.SocketError);
            }

            return null;
        }
    }
}
