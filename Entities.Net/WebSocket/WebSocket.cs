using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace Entities.Net
{

    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException() { }
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, Exception inner) : base(message, inner) { }
        protected BadRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class WebSocket : IDisposable
    {
        readonly static System.Text.RegularExpressions.Regex regexGet = new System.Text.RegularExpressions.Regex("^GET");
        readonly static System.Text.RegularExpressions.Regex regexKey = new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)");
        readonly static System.Security.Cryptography.SHA1 sh1 = System.Security.Cryptography.SHA1.Create();
        private MessageRecever recever;
        private Socket socket;
        private bool isDisposed = false;
        public event Action<WebSocket> onClose;
        /// <summary>
        /// use for client
        /// </summary>
        public WebSocket():this(new IPEndPoint(IPAddress.Any, 0)) { }
        /// <summary>
        /// use for server
        /// </summary>
        /// <param name="localEndPoint"></param>
        public WebSocket(IPEndPoint localEndPoint)
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
        }
        private WebSocket(Socket socket)
        {
            this.socket = socket;
            recever = new MessageRecever(socket);
            recever.onOver += () =>
            {
                onClose?.Invoke(this);
                Dispose();
            };
        }
        public void Listen(int count=10)
        {
            socket.Listen(count);
        }
        public Task<WebSocket> AcceptAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                Socket s = socket.Accept();
                byte[] recBuffer = new byte[512];
                int n = s.Receive(recBuffer);
                string data = Encoding.UTF8.GetString(recBuffer, 0, n);
                if (regexGet.IsMatch(data)&&regexKey.IsMatch(data))
                {
                    const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                    byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                        + "Connection: Upgrade" + eol
                        + "Upgrade: websocket" + eol
                        + $"Sec-WebSocket-Accept:{TakeResponseKey(data)}"+ eol
                        + eol);
                    s.Send(response);
                    return new WebSocket(s);
                }
                else
                {
                    throw new BadRequestException();
                }           
            });
        }
        private string TakeResponseKey(string data)
        {
            return Convert.ToBase64String(sh1.ComputeHash(Encoding.UTF8.GetBytes(regexKey.Match(data).Groups[1].Value.Trim()
                + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        }
        public Task<byte[]> ReceiveAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(WebSocket));
           return recever.Take();
        }
        public Task Connect(EndPoint endPoint)
        {
            return Task.Factory.StartNew(() =>
            {
                socket.Connect(endPoint);
                socket.Send(Encoding.UTF8.GetBytes("GET Http1.1\r\nSec-WebSocket-Key: -----------------"));
                byte[] buffer = new byte[1];
                socket.Receive(buffer);
            });
        }
        public Task<int> SendAsync(byte[]buffer,int offset,int size)
        {
           return Task.Factory.StartNew(() => socket.Send(MaskHelper.Mask(buffer,offset,size,OpCode.Binary)));
        }
        public Task<int> SendAsync(string msg)
        {
            byte[] data= Encoding.Default.GetBytes(msg);
            return Task.Factory.StartNew(() => socket.Send(MaskHelper.Mask(data, 0, data.Length,OpCode.Text)));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                //socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
                socket.Close();
            }
            isDisposed = true;
        }
    }
}
