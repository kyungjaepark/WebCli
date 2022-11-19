using KayJay.WebCli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("WebCli Sample");
        Console.WriteLine("=============");
        for (int i = 3; i > 0; i--)
        {
            Console.WriteLine($"System will begin in {i} seconds..");
            await Task.Delay(1000);
        }

        while (true)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Please input any sentence: I'll echo you it.");
            Console.WriteLine("enter 'exit' to exit.");
            Console.Write("input : ");
            string message = Console.ReadLine();
            if (message == null)
                continue;

            if (message.Trim() == "exit")
            {
                Console.WriteLine("Exit...");
                return 0;
            }
            Console.WriteLine("You entered : " + message);
            await Task.Delay(1000);
        }
    }
}