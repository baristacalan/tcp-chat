using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChat
{
    internal class Server
    {
        static readonly int _port = 4040;
        static readonly IPAddress _ipAddress = IPAddress.Any;
        static readonly List<TcpClient> _clients = [];

        static bool isRunning = false;
        
        static readonly TcpListener listener = new(_ipAddress, _port);
        static async Task Main(string[] args)
        {

            try
            {
                
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Server listening on {_ipAddress}:{_port}");

                _ = Task.Run(ServerCommandLoop); //Server commands thread
                while (true)
                {
                    if (!isRunning) break;
                    var client = await listener.AcceptTcpClientAsync(); //Main thread.
                    lock(_clients) _clients.Add(client);
                    client.Client.NoDelay = true;
                    Console.WriteLine($"Connection Accepted: {client.Client.RemoteEndPoint}");
                    

                    _ =Task.Run(() => HandleClientAsync(client)); //New thread for the client.

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally 
            { 
                //isRunning = false;
                StopServer();
                //listener.Stop();
            
            }

        }

        private static async Task HandleClientAsync(TcpClient? client)
        {

            using NetworkStream stream = client!.GetStream();
            byte[] buffer = new byte[4096];

            int bytesRead;

            try
            {
                while (true)
                {
                    bytesRead = await stream.ReadAsync(buffer, CancellationToken.None);

                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string cmd = text.Trim();

                    if (cmd.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

                    //string userName = text.Split(":")[0];

                    //Console.WriteLine(userName);

                    string clientEndPointStr = client?.Client.RemoteEndPoint?.ToString()!;

                    var message = $"[{clientEndPointStr}] {text}";
                    
                    
                    Console.WriteLine($"{message}");


                    var broadCastData = Encoding.UTF8.GetBytes(message); 

                    await BroadcastAsync(broadCastData, client);

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

        private static void StopServer()
        {
            isRunning = false;
            foreach (var c in _clients)
            {
                c.Close();
            }
            _clients.Clear();

            listener.Stop();

        }
        private static async Task SendAsync(TcpClient client, byte[] data, CancellationToken ct=default)
        {
            try
            {
                if (!client.Connected) return;
                await client.GetStream().WriteAsync(data, ct);
            }
            catch(Exception ex)
            {
                lock (_clients) _clients.Remove(client);
                Console.WriteLine(ex.Message);
            }

        }


        private static async Task BroadcastAsync(byte[] data, TcpClient? exclude = null, CancellationToken ct = default)
        {
            List<TcpClient> snapshot;
            lock (_clients) snapshot = _clients.ToList();
            var tasks = new List<Task>();

            snapshot.Remove(exclude!);

            foreach (var c in snapshot)
            {

                tasks.Add(SendAsync(c, data, ct));

            }
            await Task.WhenAll(tasks);
        }


        private static async Task ServerCommandLoop()
        {
            while (true)
            {
                try
                {

                    Console.Write("> ");
                    string command = await Console.In.ReadLineAsync() ?? "";
                    string cmd = command.Trim();

                    if (cmd.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        StopServer();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(command)) continue;

                    string displayedCommand = $"[Server] {command}";

                    byte[] data = Encoding.UTF8.GetBytes(displayedCommand);

                    List<TcpClient> snapshot;
                    lock (_clients) snapshot = _clients.ToList();

                    var command_tasks = new List<Task>();
                    foreach (var c in snapshot)
                    {
                        command_tasks.Add(SendAsync(c, data));
                    }
                    await Task.WhenAll(command_tasks);

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
    }
}
