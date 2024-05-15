
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Вы хотите запустить сервер или клиент? (s/c)");
        string? choice = Console.ReadLine();

        if (choice?.ToLower() == "s")
        {
            ServerObject server = new ServerObject(); // Создаем сервер
            await server.ListenAsync(); // Запускаем сервер
        }
        else if (choice?.ToLower() == "c")
        {
            Client client = new Client("127.0.0.1", 8888);
            await client.StartAsync();
        }
        else
        {
            Console.WriteLine("Неверный выбор. Пожалуйста, выберите 's' для сервера или 'c' для клиента.");
        }
    }
}
