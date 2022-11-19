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

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        return WebConsole.Init(args, WebMain);
    }

    static async Task<int> WebMain(string[] args)
    {
        Console.WriteLine("WebCli Sample");
        Console.Write("Please input any sentence: ");
        string message = Console.ReadLine();
        if (message != null)
            Console.WriteLine("You entered : " + message);
        return 0;
    }
}
```

You'll get an interactive browser app!



# Roadmap

- Various Method
 - WebConsole.WriteFile()
 - WebConsole.WriteImage()
 - WebConsole.WriteTable()
 - WebConsole.ReadPassword()
 - WebConsole.ReadMultiline()
 - WebConsole.ReadFile()
 ...
	