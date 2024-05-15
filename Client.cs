using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Client
{
    private string host;
    private int port;
    private TcpClient? client;
    private StreamReader? reader;
    private StreamWriter? writer;
    private string? userName;

    public Client(string host, int port)
    {
        this.host = host;
        this.port = port;
        this.client = new TcpClient();
    }

    public async Task StartAsync()
    {
        Console.Write("Введите свое имя: ");
        userName = Console.ReadLine();
        Console.WriteLine($"Добро пожаловать, {userName}");

        try
        {
            await client.ConnectAsync(host, port); // Подключение клиента
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            if (writer is null || reader is null) return;

            // Запускаем новый поток для получения данных
            Task.Run(() => ReceiveMessageAsync(reader));

            // Запускаем ввод сообщений
            await SendMessageAsync(writer);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            writer?.Close();
            reader?.Close();
            client?.Close();
        }
    }

    // Отправка сообщений
    private async Task SendMessageAsync(StreamWriter writer)
    {
        // Сначала отправляем имя
        await writer.WriteLineAsync(userName);
        await writer.FlushAsync();
        Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");

        while (true)
        {
            string? message = Console.ReadLine();
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }

    // Получение сообщений
    private async Task ReceiveMessageAsync(StreamReader reader)
    {
        while (true)
        {
            try
            {
                // Считываем ответ в виде строки
                string? message = await reader.ReadLineAsync();
                // Если пустой ответ, ничего не выводим на консоль
                if (string.IsNullOrEmpty(message)) continue;
                Print(message); // Вывод сообщения
            }
            catch
            {
                break;
            }
        }
    }

    // Чтобы полученное сообщение не накладывалось на ввод нового сообщения
    private void Print(string message)
    {
        if (OperatingSystem.IsWindows()) // Если ОС Windows
        {
            var position = Console.GetCursorPosition(); // Получаем текущую позицию курсора
            int left = position.Left;   // Смещение в символах относительно левого края
            int top = position.Top;     // Смещение в строках относительно верха
            // Копируем ранее введенные символы в строке на следующую строку
            Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
            // Устанавливаем курсор в начало текущей строки
            Console.SetCursorPosition(0, top);
            // В текущей строке выводит полученное сообщение
            Console.WriteLine(message);
            // Переносим курсор на следующую строку
            // И пользователь продолжает ввод уже на следующей строке
            Console.SetCursorPosition(left, top + 1);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
