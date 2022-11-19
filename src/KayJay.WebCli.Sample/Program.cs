using KayJay.WebCli;

WebConsole.Init(args, WebMain);

static async Task WebMain(string[] args)
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
            return;
        }
        Console.WriteLine("You entered : " + message);
        await Task.Delay(1000);
    }
}
