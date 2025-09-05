using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QUIKSharp
{
    public sealed class ProtectedSocket : IDisposable
    {
        private TcpClient _client;
        private int _refCount;
        private readonly object _lock = new object();

        public ProtectedSocket(TcpClient client)
        {
            _client = client;
            _refCount = 1;
        }

        public TcpClient GetClient()
        {
            lock (_lock)
            {
                if (_refCount <= 0) return null;
                _refCount++;
                return _client;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                if (_refCount > 0) _refCount--;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_refCount > 0) return;

                try
                {
                    if (_client?.Connected == true)
                        _client.Client.Shutdown(SocketShutdown.Both);
                }
                catch { }

                try { _client?.Close(); } catch { }
                try { _client?.Dispose(); } catch { }
                _client = null;
            }
        }
    }
}
