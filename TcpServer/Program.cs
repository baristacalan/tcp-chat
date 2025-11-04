using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Program
    {
        static readonly int PORT = 4040;
        static readonly IPAddress _ipAddress = IPAddress.Any;
        static readonly TcpListener _listener = new(_ipAddress, PORT);
        
        static async Task Main(string[] args)
        {

            try
            {
                _listener.Start();
                Console.WriteLine($"Server listening on {_ipAddress}:{PORT}");

                while (true)
                {
                    Console.WriteLine("Waiting for new connections...");
                    var client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"Connection Accepted: {client.Client.RemoteEndPoint}");
                    

                    _ = Task.Run(async () => await HandleClientAsync(client));


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally 
            { 
                _listener.Stop();
            
            }

        }

        private static async Task HandleClientAsync(TcpClient? client)
        {

            await using NetworkStream stream = client!.GetStream();
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    Console.WriteLine($"{bytesRead} bytes recieved from [{client.Client.RemoteEndPoint}]: {text}");


                    await stream.WriteAsync(buffer, 0, bytesRead);
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
                Console.WriteLine($"Connection terminated: {client.Client.RemoteEndPoint}");
            }
        }

    }
}
