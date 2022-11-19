using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace KayJay.WebCli
{
    public class WebConsole
    {
        public static Encoding utf8Encoding = new UTF8Encoding(false, false);
        public static WebSocket? webSocket;

        public static WebMainDelegate? webMainDelegate;

        public delegate Task WebMainDelegate(string[] args);

        public static void Init(string[] args, WebMainDelegate webMainDelegate, bool redirectConsole = true)
        {

            WebCli.WebConsole.Args = args;
            WebCli.WebConsole.webMainDelegate = webMainDelegate;

            bool wwwRootExists = Directory.Exists("wwwroot");
            if (wwwRootExists == false)
                Directory.CreateDirectory("wwwroot");
            try
            {
                // https://andrewlock.net/how-to-automatically-choose-a-free-port-in-asp-net-core/

                var builder = WebApplication.CreateBuilder(args);
                builder.WebHost.UseKestrel().UseUrls("http://[::1]:0");
                builder.Host.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
                builder.Services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.Zero);
                var app = builder.Build();

                // https://medium.com/geekculture/run-code-once-the-application-starts-in-net6-2e4e965ddcec
                // https://andrewlock.net/exploring-dotnet-6-part-12-upgrading-a-dotnet-5-startup-based-app-to-dotnet-6/
                // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-6.0
                var webSocketOptions = new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromMinutes(2)
                };
                app.UseWebSockets(webSocketOptions);

                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/")
                    {
                        await context.Response.WriteAsync(HtmlFile.IndexHtml);
                        return;
                    }

                    if (context.Request.Path == "/ws")
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            WebConsole.SetWebSocket(webSocket);
                            try
                            {
                                if (redirectConsole)
                                {
                                    RedirectConsole();
                                }
                                await WebConsole.webMainDelegate(WebCli.WebConsole.Args);
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                            }
                            catch
                            {

                            }

                            if (redirectConsole)
                            {
                                FinishRedirectConsole();
                            }

                            await app.StopAsync();
                            if (wwwRootExists == false)
                            {
                                try
                                {
                                    Directory.Delete("wwwroot");
                                }
                                catch { }
                            }

                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        }
                    }
                    else
                    {
                        await next(context);
                    }

                });

                app.Lifetime.ApplicationStarted.Register(OnStarted);
                void OnStarted()
                {
                    var httpServer = app.Urls.Where(r => r.StartsWith("http://")).First();
                    OpenUrl(httpServer);

                }

                app.Run();

                // TODO? https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-7.0#accept-websocket-requests
                // If you're using a background service to write data to a WebSocket, make sure you keep the middleware pipeline running.'
            }
            catch { }


        }
        public static string[] Args = new string[] { };

        public static TextWriter? old_out = null;
        public static TextWriter? old_error = null;
        public static TextReader? old_in = null;

        public static void RedirectConsole()
        {
            old_out = Console.Out;
            old_error = Console.Error;
            old_in = Console.In;
            Console.SetOut(new WebConsoleTextWriter());
            Console.SetError(new WebConsoleTextWriter());
            Console.SetIn(new WebConsoleTextReader());
        }

        public static void FinishRedirectConsole()
        {
            if (old_out != null)
                Console.SetOut(old_out);
            if (old_error != null)
                Console.SetError(old_error);
            if (old_in != null)
                Console.SetIn(old_in);
            old_out = null;
            old_error = null;
            old_in = null;
        }

        public static void SetWebSocket(WebSocket webSocket)
        {
            WebConsole.webSocket = webSocket;
        }

        public static void Write(String s)
        {
            if (webSocket == null)
                return;
            var buffer = utf8Encoding.GetBytes(s);
            Task sendTask = Task.Run(()=>webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, buffer.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None));
            sendTask.Wait();
        }

        public static void WriteLine(String s)
        {
            Write(s);
            WriteLine();
        }
        public static void WriteLine()
        {
            Write("\n");
        }

        public static String ReadLine()
        {

            if (webSocket == null)
                return "";
            var buffer_bin = new byte[] { 0 };
            Task sendTask = Task.Run(() => webSocket.SendAsync(
                    new ArraySegment<byte>(buffer_bin, 0, buffer_bin.Length),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None));
            sendTask.Wait();

            var buffer = new byte[16384];
            
            var receieveTask = Task.Run(()=>webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None));
            receieveTask.Wait();
            var receiveResult = receieveTask.Result;
            return utf8Encoding.GetString(buffer, 0, receiveResult.Count);
        }

        // https://stackoverflow.com/a/43232486
        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private class HtmlFile
        {
            public static string IndexHtml = @"

<!DOCTYPE html>
<html>

<head>
    <meta charset=""utf-8"" />
    <title>WebCli</title>
    <style>
        table {
            border: 0
        }

        .commslog-data {
            font-family: Consolas, Courier New, Courier, monospace;
        }

        .commslog-server {
            background-color: red;
            color: white
        }

        .commslog-client {
            background-color: green;
            color: white
        }
    </style>
</head>

<body>

    <div id=""console-box"" class=""commslog-data"">
        <div id=""messageBox"">

            <div id=""inputBox"" style=""display:none"">
                <input id=""sendMessage"" disabled class='commslog-data' />
                <button id=""sendButton"" type=""submit"" disabled>Send</button>
            </div>
        </div>

    </div>
    <div style=""display:none"">
        <h1>WebSocket Sample Application</h1>
        <p id=""stateLabel"">Ready to connect...</p>
        <div>
            <label for=""connectionUrl"">WebSocket Server URL:</label>
            <input id=""connectionUrl"" />
            <button id=""connectButton"" type=""submit"">Connect</button>
        </div>
        <p></p>
        <div>
            <!--
        <label for=""sendMessage"">Message to send:</label>
        <input id=""sendMessage"" disabled />
        <button id=""sendButton"" type=""submit"" disabled>Send</button>
    -->
            <button id=""closeButton"" disabled>Close Socket</button>
        </div>

        <h2>Communication Log</h2>
        <table style=""width: 800px"">
            <thead>
                <tr>
                    <td style=""width: 100px"">From</td>
                    <td style=""width: 100px"">To</td>
                    <td>Data</td>
                </tr>
            </thead>
            <tbody id=""commsLog"">
            </tbody>
        </table>
    </div>
    <script>
        var connectionUrl = document.getElementById(""connectionUrl"");
        var connectButton = document.getElementById(""connectButton"");
        var stateLabel = document.getElementById(""stateLabel"");
        var sendMessage = document.getElementById(""sendMessage"");
        var sendButton = document.getElementById(""sendButton"");
        var commsLog = document.getElementById(""commsLog"");
        var messageBox = document.getElementById(""messageBox"");
        var inputBox = document.getElementById(""inputBox"");
        var closeButton = document.getElementById(""closeButton"");
        var socket;

        var scheme = document.location.protocol === ""https:"" ? ""wss"" : ""ws"";
        var port = document.location.port ? ("":"" + document.location.port) : """";

        connectionUrl.value = scheme + ""://"" + document.location.hostname + port + ""/ws"";

        function updateState() {
            function disable() {
                sendMessage.disabled = true;
                sendButton.disabled = true;
                closeButton.disabled = true;
            }
            function enable() {
                sendMessage.disabled = false;
                sendButton.disabled = false;
                closeButton.disabled = false;
            }

            connectionUrl.disabled = true;
            connectButton.disabled = true;

            if (!socket) {
                disable();
            } else {
                switch (socket.readyState) {
                    case WebSocket.CLOSED:
                        stateLabel.innerHTML = ""Closed"";
                        disable();
                        connectionUrl.disabled = false;
                        connectButton.disabled = false;
                        break;
                    case WebSocket.CLOSING:
                        stateLabel.innerHTML = ""Closing..."";
                        disable();
                        break;
                    case WebSocket.CONNECTING:
                        stateLabel.innerHTML = ""Connecting..."";
                        disable();
                        break;
                    case WebSocket.OPEN:
                        stateLabel.innerHTML = ""Open"";
                        enable();
                        break;
                    default:
                        stateLabel.innerHTML = ""Unknown WebSocket State: "" + htmlEscape(socket.readyState);
                        disable();
                        break;
                }
            }
        }

        sendMessage.addEventListener(""keyup"", function (event) {
            event.preventDefault();
            if (event.keyCode === 13) {
                doSend();
            }
        });

        closeButton.onclick = function () {
            if (!socket || socket.readyState !== WebSocket.OPEN) {
                alert(""socket not connected"");
            }
            socket.close(1000, ""Closing from client"");
        };

        sendButton.onclick = doSend;
        function doSend() {
            if (!socket || socket.readyState !== WebSocket.OPEN) {
                alert(""socket not connected"");
            }
            var data = sendMessage.value;
            print_msg(data);
            add_br();
            inputBox.style = 'display:none';

            socket.send(data);

            commsLog.innerHTML += '<tr>' +
                '<td class=""commslog-client"">Client</td>' +
                '<td class=""commslog-server"">Server</td>' +
                '<td class=""commslog-data"">' + htmlEscape(data) + '</td></tr>';
        };

        connectButton.onclick = doConnect;
        function doConnect() {
            stateLabel.innerHTML = ""Connecting"";
            socket = new WebSocket(connectionUrl.value);
            socket.onopen = function (event) {
                updateState();
                commsLog.innerHTML += '<tr>' +
                    '<td colspan=""3"" class=""commslog-data"">Connection opened</td>' +
                    '</tr>';
            };
            socket.onclose = function (event) {
                updateState();

                commsLog.innerHTML += '<tr>' +
                    '<td colspan=""3"" class=""commslog-data"">Connection closed. Code: ' + htmlEscape(event.code) + '. Reason: ' + htmlEscape(event.reason) + '</td>' +
                    '</tr>';

                let msg = document.createElement(""p"");
                msg.innerText = ""Done"";
                msg.style = ""background-color:lime;"";
                messageBox.insertBefore(msg, inputBox);

            };
            socket.onerror = updateState;
            socket.onmessage = function (event) {
                if (typeof event.data === 'object') {

                    let fileReader = new FileReader();
                    fileReader.onload = function (e) {
                        let command = new DataView(e.target.result).getInt8(0);
                        switch (command) {
                            case 0:
                                sendMessage.value = '';
                                inputBox.style = 'display:inline-block';
                                sendMessage.focus();
                                break;
                            default:
                                break;

                        }
                    };
                    fileReader.readAsArrayBuffer(event.data);


                }
                else {
                    addMessage(event.data);
                }

            };
        };
        doConnect();

        function addMessage(s) {

            let lines = s.replace('\r', '').split(""\n"");
            for (let i = 0; i < lines.length; i++) {
                if (i != 0)
                    add_br();
                if (lines[i] != '') {
                    print_msg(lines[i]);
                }

            }

            commsLog.innerHTML += '<tr>' +
                '<td class=""commslog-server"">Server</td>' +
                '<td class=""commslog-client"">Client</td>' +
                '<td class=""commslog-data"">' + htmlEscape(s) + '</td></tr>';

        }

        function add_br() {
            messageBox.insertBefore(document.createElement(""br""), inputBox);
        }
        function print_msg(s) {
            let msg = document.createElement(""span"");
            msg.innerText = s;
            messageBox.insertBefore(msg, inputBox);
        }

        function htmlEscape(str) {
            return str.toString()
                .replace(/&/g, '&amp;')
                .replace(/""/g, '&quot;')
                .replace(/'/g, '&#39;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;');
        }
    </script>
</body>

</html>

";
        }
    }
}