using System.Net;
using System.Net.Sockets;
using System.Text;



namespace Server
{
    internal class Program
    {
        static readonly int _port = 4040;
        static readonly IPAddress _ipAddress = IPAddress.Any;
        static readonly List<TcpClient> _clients = [];
        
        static async Task Main(string[] args)
        {

            TcpListener listener = new(_ipAddress, _port);
            try
            {
                listener.Start();
                Console.WriteLine($"Server listening on {_ipAddress}:{_port}");

                _= Task.Run(ServerCommandLoop);
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    lock(_clients) _clients.Add(client);
                    client.Client.NoDelay = true;
                    Console.WriteLine($"Connection Accepted: {client.Client.RemoteEndPoint}");
                    

                    _ = Task.Run(() => HandleClientAsync(client));

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally 
            { 
                listener.Stop();
            
            }

        }

        private static async Task HandleClientAsync(TcpClient? client)
        {

            await using NetworkStream stream = client!.GetStream();
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while (true)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string cmd = text.Trim();

                    if (cmd.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

                    Console.WriteLine($"{bytesRead} bytes recieved from [{client.Client.RemoteEndPoint}]: {text}");


                    await stream.WriteAsync(buffer ,0, buffer.Length);
                    Console.WriteLine($"{bytesRead} bytes sent back to [{client.Client.RemoteEndPoint}]: {text}");

                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error: [{client.Client.RemoteEndPoint}] lost connection. {ioEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: [{client.Client.RemoteEndPoint}] {ex.Message}");
            }
            finally
            {
                client.Close();
                lock (_clients) _clients.Remove(client);
                Console.WriteLine($"Connection terminated: {client.Client.RemoteEndPoint}");
            }
        }

        private static async Task ServerCommandLoop()
        {
            while (true)
            {
                Console.Write(">");
                string command = await Console.In.ReadLineAsync() ?? "";

                if (string.IsNullOrWhiteSpace(command)) continue;

                byte[] data = Encoding.UTF8.GetBytes(command);

                lock (_clients)
                {
                   foreach(var c in _clients.ToArray())
                    {

                        if(!c.Connected) continue;
                        _= c.GetStream().WriteAsync(data, 0, data.Length);
                        
                    }

                }
            }
        }

    }
}
