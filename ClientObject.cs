using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    TcpClient client;
    ServerObject server; // Объект сервера

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        // Получаем NetworkStream для взаимодействия с сервером
        var stream = client.GetStream();
        // Создаем StreamReader для чтения данных
        Reader = new StreamReader(stream);
        // Создаем StreamWriter для отправки данных
        Writer = new StreamWriter(stream);
    }

    public async Task ProcessAsync()
    {
        try
        {
            // Получаем имя пользователя
            string? userName = await Reader.ReadLineAsync();
            string? message = $"{userName} вошел в чат";
            // Посылаем сообщение о входе в чат всем подключенным пользователям
            await server.BroadcastMessageAsync(message, Id);
            Console.WriteLine(message);
            // В бесконечном цикле получаем сообщения от клиента
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;

                    // Проверяем, сообщение (например, "Плохое слово")
                    if (message.Contains("Мат"))
                    {
                        // Заменяем слово на другое (например, "Хорошее слово")
                        message = message.Replace("Мат", "Фу-фу-фу, нельзя так писать!");
                    }
                    message = $"{userName}: {message}";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                }
                catch
                {
                    message = $"{userName} покинул чат";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            // В случае выхода из цикла закрываем ресурсы
            server.RemoveConnection(Id);
        }
    }

    // Закрытие подключения
    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}
