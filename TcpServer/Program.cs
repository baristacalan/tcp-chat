using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Encodings;
using System.Text;
namespace TcpServer
{
    internal class Program
    {
        const int PORT = 4040;
        static readonly IPAddress ipAddress = IPAddress.Any;
        static readonly TcpListener listener = new(ipAddress, PORT);
        
        static async Task Main(string[] args)
        {

            try
            {
                listener.Start();
                Console.WriteLine($"Server listening on {ipAddress}:{PORT}");

                while (true)
                {
                    Console.WriteLine("Waiting for new connections...");
                    var client = await listener.AcceptTcpClientAsync();
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
                listener.Stop();
            
            }

        }

        private static async Task HandleClientAsync(TcpClient? client)
        {
            if (client == null) return;

            await using NetworkStream stream = client!.GetStream();
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    Console.WriteLine($"[{client.Client.RemoteEndPoint}] -> {bytesRead} bytes sent.");

                    string text = Encoding.UTF8.GetString(buffer);

                    await stream.WriteAsync(buffer, 0, bytesRead);
                    Console.WriteLine(text);
                    Console.WriteLine($"[{client.Client.RemoteEndPoint}] <- {bytesRead} bytes recieved");
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
