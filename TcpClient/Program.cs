using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Client
{
    internal class Program
    {

        static TcpClient? _client;

        static int? _port;

        static IPAddress? _address;

        static async Task Main(string[] args)
        {
            Console.Write("Enter IP: ");
            
            string ip = Console.ReadLine() ?? "127.0.0.1";
            
            if (IPAddress.TryParse(ip, out IPAddress? address))
            {
                _address = address;
            }

            Console.Write("Enter port: ");
            if (int.TryParse(Console.ReadLine(), out int port))
            {
                _port = port;
            }

            if (_address != null && _port.HasValue)
            {
                try
                {
                    _client = new TcpClient();

                    await _client.ConnectAsync(new IPEndPoint(_address, _port.Value));

                    _client.NoDelay = true;

                    NetworkStream stream = _client.GetStream();

                    var writeTask = Task.Run(() => WriteMessageAsync(stream));

                    var readTask = Task.Run(() => ReadMessageAsync(stream));


                    await Task.WhenAny(writeTask, readTask);

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Invalid IP address or port.");
            }
        }
        static async Task WriteMessageAsync(NetworkStream stream)
        {
            while(true)
            {

                string message = await Console.In.ReadLineAsync() ?? "";

                var data = Encoding.UTF8.GetBytes(message, 0, message.Length);

                await stream.WriteAsync(data, 0, data.Length);

            }
        }

        static async Task ReadMessageAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            while(true)
            {

                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0) break;


                var server_response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"\n[Server] {Encoding.UTF8.GetString(buffer, 0, bytesRead)}");
                Console.Write("> ");

            }
        }
    }
}
