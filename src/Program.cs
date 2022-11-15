using WebCli;

WebConsole.Init(args, WebMain);

static async Task WebMain(string[] args)
{
    await WebConsole.WriteLine("WebCli Sample");
    await WebConsole.WriteLine("========");
    for (int i = 3; i > 0; i--)
    {
        await WebConsole.WriteLine($"System will begin in {i} seconds..");
        Sleep(1);
    }

     while (true)
    {
        await WebConsole.WriteLine("-----------------------------------------");
        await WebConsole.WriteLine("Please input any sentence. I'll echo you.");
        await WebConsole.WriteLine("enter 'exit' to exit.");
        await WebConsole.Write("input : ");
        string message = await WebConsole.ReadLine();
        if (message.Trim() == "exit")
        {
            await WebConsole.WriteLine("Exit...");
            return;
        }
        Sleep(1);
        await WebConsole.WriteLine("You entered : " + message);
    }
}

static void Sleep(double seconds)
{
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(seconds));
}