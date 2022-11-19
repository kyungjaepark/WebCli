# WebCli

WebCli is a library that handles the output and input of console app in a web browser.

# Sample Code

```
using KayJay.WebCli;

WebConsole.Init(args, WebMain);

static async Task WebMain(string[] args)
{
    WebConsole.WriteLine("WebCli Sample");
    WebConsole.WriteLine("=============");
    for (int i = 3; i > 0; i--)
    {
        WebConsole.WriteLine($"System will begin in {i} seconds..");
        await Task.Delay(1000);
    }

    while (true)
    {
        WebConsole.WriteLine("--------------------------------------------");
        WebConsole.WriteLine("Please input any sentence: I'll echo you it.");
        WebConsole.WriteLine("enter 'exit' to exit.");
        WebConsole.Write("input : ");
        string message = WebConsole.ReadLine();
        if (message.Trim() == "exit")
        {
            WebConsole.WriteLine("Exit...");
            return;
        }
        await Task.Delay(1000);
        WebConsole.WriteLine("You entered : " + message);
    }
}

```

The default browser screen with your application will appear.


 