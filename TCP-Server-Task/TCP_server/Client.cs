using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var server = new TcpListener(IPAddress.Loopback, 8888);
        server.Start();
        
        Console.WriteLine("🚀 TCP Server started on port 8888");
        Console.WriteLine("📍 Waiting for client connections...");
        Console.WriteLine("⏹️  Press Ctrl+C to stop\n");

        try
        {
            while (true)
            {
                // Ждем подключения клиента
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("✅ New client connected!");
                
                // Обрабатываем клиента в отдельной задаче
                _ = Task.Run(async () => await HandleClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Server error: {ex.Message}");
        }
        finally
        {
            server.Stop();
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        var clientId = Guid.NewGuid().ToString()[..8];
        
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buffer = new byte[1024];
                
                while (client.Connected)
                {
                    // Читаем данные от клиента
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    
                    if (string.IsNullOrEmpty(message)) 
                        continue;

                    Console.WriteLine($"📨 [{clientId}]: {message}");
                    Console.WriteLine($"⏰ Time: {DateTime.Now:HH:mm:ss}");

                    // Если клиент отправил "exit" - закрываем соединение
                    if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        var goodbye = "Goodbye! Connection closed.";
                        await stream.WriteAsync(Encoding.UTF8.GetBytes(goodbye));
                        Console.WriteLine($"🚪 Client {clientId} disconnected by request");
                        break;
                    }

                    // Отправляем эхо-ответ
                    var response = $"Echo: {message} (received at {DateTime.Now:HH:mm:ss})";
                    var responseData = Encoding.UTF8.GetBytes(response + Environment.NewLine);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    
                    Console.WriteLine($"📤 Sent response to {clientId}\n");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error with client {clientId}: {ex.Message}");
        }
        
        Console.WriteLine($"❌ Client {clientId} disconnected\n");
    }
}