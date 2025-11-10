namespace TcpChat
{
    internal class Program
    {

        static async Task Main(string[] args)
        {

            string? ipAddress;
            int port;
            string? userName;

            Client? client = null;

            if (args.Length == 3)
            {

                ipAddress = args[0];
                port = int.Parse(args[1]);
                userName = args[2];

            }
            else
            {
                Console.Write("Enter IP: ");
                ipAddress = Console.ReadLine();

                Console.Write("Enter Port: ");
                port = int.Parse(Console.ReadLine()!);

                Console.Write("Enter Username: ");
                userName = Console.ReadLine();
            }

            try
            {
                client = new(ipAddress!, port, userName);
                await client.ConnectAsync();

                var writeTask = Task.Run(() => client.WriteMessageAsync()); //Write Thread
                var readTask = Task.Run(() => client.ReadMessageAsync()); //Read Thread
                
                await Task.WhenAny(writeTask, readTask); //Main Thread
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client failed to : {ex.Message}");
            }
            finally { client!.Disconnect(); }

        }
    }
}
