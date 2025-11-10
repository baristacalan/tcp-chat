using System.Net;
using System.Net.Sockets;
using System.Text;


namespace TcpChat
{
    internal class Client
    {

        private Socket? _socket;
        private int _port;
        private IPAddress? _address;
        private string? _userName;

        public bool IsConnected { get; private set; }


        public Client(string ipAddress, int port, string? userName)
        {
            if(IPAddress.TryParse(ipAddress, out IPAddress? address)) _address = address;
            _port = port;

            if(string.IsNullOrWhiteSpace(userName)) _userName = "Annonymous Client";

            _userName = userName!;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ArgumentNullException.ThrowIfNull(nameof(ipAddress));
            ArgumentNullException.ThrowIfNull(nameof(port));

            IsConnected = false;
        }

        public async Task ConnectAsync()
        {
            var endpoint = new IPEndPoint(_address!, _port);
            await _socket!.ConnectAsync(endpoint);
            _socket.NoDelay = true;
            IsConnected = true;

        }

        public async Task WriteMessageAsync()
        {
            while (true)
            {
                Console.Write("> ");
                string message = await Console.In.ReadLineAsync() ?? "";

                string fullMessage = $"[{_userName}]: {message}";

                var data = Encoding.UTF8.GetBytes(fullMessage);

                await _socket!.SendAsync(data);

            }
        }

        public async Task ReadMessageAsync()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {

                int bytesRead = await _socket!.ReceiveAsync(buffer);

                if (bytesRead == 0) break;

                var server_response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"\r {new string(' ', Console.WindowWidth - 1)} \r");
                Console.WriteLine(server_response);
                Console.Write("> ");

            }
        }

        public void Disconnect()
        {
            if(IsConnected)
                _socket?.Dispose();
            IsConnected = false;
        }

    }
}
