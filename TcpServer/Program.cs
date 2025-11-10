using System.Net;

namespace TcpChat
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Server? server = null;

            string ipAddress = "0.0.0.0";
            int port = 4040;

            if (args.Length > 0)
            {
                ipAddress = args[0];
                port = int.Parse(args[1]);

            }

            try
            {

                server = new(ipAddress, port);

                server.Start();
                Console.WriteLine($"Server listening on {ipAddress}:{port}...");

                _ = Task.Run(server.ServerCommandLoop); //Server commands thread
                while (true)
                {
                    if (!server.IsRunning) break;
                    var client = server.AcceptClientAsync();
                    Console.WriteLine($"Connection Accepted: {client.Client.RemoteEndPoint}");


                    _ = Task.Run(() => server.HandleClientAsync(client)); //New thread for the client.

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                server!.Stop();
            }

        }

    }
}
