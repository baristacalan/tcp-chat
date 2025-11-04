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


            while (true)
            {
                NetworkStream stream = _client!.GetStream();



                Console.Write(">");

                byte[] buffer = new byte[4096];

                string message = Console.ReadLine() ?? "";

                var data = Encoding.UTF8.GetBytes(message);

                await stream.WriteAsync(data, 0, data.Length);

                int bytesRead = await stream.ReadAsync(buffer);

                var response = Encoding.UTF8.GetString(buffer);

                Console.WriteLine(response);

            }
        }
    }
}
