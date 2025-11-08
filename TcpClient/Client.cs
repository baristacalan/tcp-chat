using System.Net;
using System.Net.Sockets;
using System.Text;


namespace TcpChat
{
    internal class Client
    {


        static TcpClient? _client;

        static int? _port;

        static IPAddress? _address;

        static string? _name;
        

        //private static string? userName;

        static async Task Main(string[] args)
        {


            if (args.Length == 3)
            {

                if (IPAddress.TryParse(args[0], out IPAddress? address))
                {
                    _address = address;
                }

                if (int.TryParse(args[1], out int port))
                {
                    _port = port;
                }

                _name = args[2];

            }

            if (_address != null && _port.HasValue)
            {
                try
                {
                    _client = new TcpClient();

                    await _client.ConnectAsync(new IPEndPoint(_address, _port.Value));

                    _client.NoDelay = true;

                    NetworkStream stream = _client.GetStream();

                    var writeTask = Task.Run(() => WriteMessageAsync(stream)); //Write thread

                    var readTask = Task.Run(() => ReadMessageAsync(stream)); //Read thread


                    await Task.WhenAny(writeTask, readTask); //Main Thread

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
                Console.Write("> ");
                string message = await Console.In.ReadLineAsync() ?? "";

                string fullMessage = $"[{_name}]: {message}";

                var data = Encoding.UTF8.GetBytes(fullMessage);

                await stream.WriteAsync(data);

            }
        }

        static async Task ReadMessageAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            while (true)
            {

                int bytesRead = await stream.ReadAsync(buffer);

                if (bytesRead == 0) break;

                var server_response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                //await Console.Out.WriteLineAsync(server_response);

                Console.WriteLine($"\r {new string(' ', Console.WindowWidth - 1)} \r");
                Console.WriteLine(server_response);
                Console.Write("> ");

            }
        }
    }
}
