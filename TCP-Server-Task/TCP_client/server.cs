using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("🚀 TCP Client started");
        Console.WriteLine("📝 Type messages and press Enter to send");
        Console.WriteLine("❌ Type 'exit' to quit\n");

        try
        {
            // Подключаемся к серверу
            using var client = new TcpClient();
            await client.ConnectAsync("localhost", 8888);
            
            Console.WriteLine("✅ Connected to server!");
            Console.WriteLine("💡 Start typing messages...\n");

            var stream = client.GetStream();

            // Задача для чтения ответов от сервера
            var readTask = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                while (client.Connected)
                {
                    try
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        
                        var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"\n📨 Server response: {response}");
                        Console.Write("> "); // Перерисовываем приглашение
                    }
                    catch
                    {
                        break;
                    }
                }
            });

            // Главный цикл для отправки сообщений
            while (client.Connected)
            {
                Console.Write("> ");
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(message)) 
                    continue;
                    
                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase)) 
                    break;

                // Отправляем сообщение + символ новой строки
                var data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                await stream.WriteAsync(data, 0, data.Length);
            }

            Console.WriteLine("👋 Disconnected from server");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("💡 Make sure the server is running on localhost:8888");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}