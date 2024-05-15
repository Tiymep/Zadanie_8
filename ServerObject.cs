using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;

public class ServerObject
{
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); // Сервер для прослушивания
    List<ClientObject> clients = new List<ClientObject>(); // Все подключения

    protected internal void RemoveConnection(string id)
    {
        // Получаем по id закрытое подключение
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
        // И удаляем его из списка подключений
        if (client != null) clients.Remove(client);
        client?.Close();
    }

    // Прослушивание входящих подключений
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ClientObject clientObject = new ClientObject(tcpClient, this);
                clients.Add(clientObject);
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    // Трансляция сообщения подключенным клиентам
    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] : {message}";
        foreach (var client in clients)
        {
            if (client.Id != id) // Если id клиента не равно id отправителя
            {
                await client.Writer.WriteLineAsync(message); // Передача данных
                await client.Writer.FlushAsync();
            }
        }
        LogMessage(formattedMessage);
    }
    private void LogMessage(string message)
    {
        string filePath = "messages.log";
        try
        {
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи сообщения в файл: {ex.Message}");
        }
    }

    // Отключение всех клиентов
    protected internal void Disconnect()
    {
        foreach (var client in clients)
        {
            client.Close(); // Отключение клиента
        }
        tcpListener.Stop(); // Остановка сервера
    }

}
