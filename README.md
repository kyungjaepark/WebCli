# WebCli

WebCli is a library that handles the output and input of console app in a web browser.

- [NuGet Package](https://www.nuget.org/packages/KayJay.WebCli)

![](./docs/webcli.webp)

# Sample Code

```
dotnet new console
dotnet add package KayJay.WebCli
```

Program.cs : 
```
using KayJay.WebCli;

WebConsole.Init(args, WebMain);

static async Task WebMain(string[] args)
{
    await WebConsole.WriteLine("WebCli Sample");
    for (int i = 3; i > 0; i--)
    {
        await WebConsole.WriteLine($"System will begin in {i} seconds..");
        await Task.Delay(1000);
    }

    while (true)
    {
        await WebConsole.WriteLine("Please input any sentence: I'll echo you it.");
        await WebConsole.WriteLine("enter 'exit' to exit.");
        await WebConsole.Write("input : ");
        string message = await WebConsole.ReadLine();
        if (message.Trim() == "exit")
        {
            await WebConsole.WriteLine("Exit...");
            return;
        }
        await Task.Delay(1000);
        await WebConsole.WriteLine("You entered : " + message);
    }
}
```

You'll get an interactive browser app!

