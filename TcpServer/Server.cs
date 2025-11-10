using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChat
{
    internal class Server
    {
        private readonly int _port;
        private readonly IPAddress? _ipAddress;
        private readonly List<TcpClient> _clients = [];
        private TcpListener? _listener;

        public bool IsRunning { get; private set; }

        public Server(string ipAddress, int port)
        {
            _= IPAddress.TryParse(ipAddress, out _ipAddress);
            _port = port;

            ArgumentNullException.ThrowIfNullOrWhiteSpace(ipAddress);
            ArgumentNullException.ThrowIfNull(port);

        }

        public void Start()
        {
            try
            {
                _listener = new TcpListener(_ipAddress!, _port);
                _listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server failed to start: {ex.Message}");
            }
            IsRunning = true;

        }

        public TcpClient AcceptClientAsync()
        {
            TcpClient newClient = _listener!.AcceptTcpClient();
            newClient.Client.NoDelay = true;
            lock (_clients) _clients.Add(newClient);
            return newClient;

        }

        public async Task HandleClientAsync(TcpClient? client)
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

        public void Stop()
        {
            IsRunning = false;
            foreach (var c in _clients)
            {
                c.Close();
            }
            _clients.Clear();

            _listener!.Stop();

            _listener.Dispose();
        }
        

        //Helper function
        private async Task SendAsync(TcpClient client, byte[] data, CancellationToken ct=default)
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

       public async Task BroadcastAsync(byte[] data, TcpClient? exclude = null, CancellationToken ct = default)
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


        public async Task ServerCommandLoop()
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
                        Stop();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(command)) continue;

                    string displayedCommand = $"[Server] {command}";

                    byte[] data = Encoding.UTF8.GetBytes(displayedCommand);

                    //List<TcpClient> snapshot;
                    //lock (_clients) snapshot = _clients.ToList();
                    //var command_tasks = new List<Task>();
                    //foreach (var c in snapshot)
                    //{
                    //    command_tasks.Add(SendAsync(c, data));
                    //}
                    //await Task.WhenAll(command_tasks);

                    await BroadcastAsync(data, null);

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
    }
}
