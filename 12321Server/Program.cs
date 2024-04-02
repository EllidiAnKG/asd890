using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static TcpListener server;
    static List<TcpClient> clients = new List<TcpClient>();
    static string historyFolderPath = "ChatHistory";

    static async Task Main(string[] args)
    {
        server = new TcpListener(IPAddress.Any, 12345);
        server.Start();

        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            clients.Add(client);
            NetworkStream stream = client.GetStream();
            Console.WriteLine("Новый клиент подключен.");

            Task.Run(async () => await ReceiveAndBroadcastMessages(stream, client));
        }
    }

    static async Task ReceiveAndBroadcastMessages(NetworkStream fromStream, TcpClient fromClient)
    {
        while (true)
        {
            try
            {
                byte[] data = new byte[1024];
                int bytesRead = await fromStream.ReadAsync(data, 0, data.Length);
                string message = Encoding.UTF8.GetString(data, 0, bytesRead);

                Console.WriteLine("Новое сообщение от клиента: " + message);

                foreach (var client in clients)
                {
                    if (client != fromClient && client.Connected)
                    {
                        NetworkStream toStream = client.GetStream();
                        byte[] responseData = Encoding.UTF8.GetBytes(message);
                        await toStream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }

                string historyFilePath = Path.Combine(historyFolderPath, $"{((IPEndPoint)fromClient.Client.RemoteEndPoint).Address}_chat_history.txt");
                SaveMessageToHistory(historyFilePath, ((IPEndPoint)fromClient.Client.RemoteEndPoint).Address.ToString(), message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении/отправке сообщения: " + ex.Message);
                break;
            }
        }
    }

    static void SaveMessageToHistory(string filePath, string ipAddress, string message)
    {
        if (!Directory.Exists(historyFolderPath))
        {
            Directory.CreateDirectory(historyFolderPath);
        }

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine($"{DateTime.Now} - IP: {ipAddress}, Message: {message}");
        }
    }
}

