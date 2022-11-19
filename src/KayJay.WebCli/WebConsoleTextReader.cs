namespace KayJay.WebCli
{

    public class WebConsoleTextReader : TextReader
    {
        public override int Peek()
        {
            WebConsole.old_out?.WriteLine("Peek() called.");
            return WebConsole.old_in?.Peek() ?? 0;
        }

        public override int Read()
        {
            WebConsole.old_out?.WriteLine($"Read() called.");
            var ret = WebConsole.old_in?.Read() ?? 0;
            WebConsole.old_out?.WriteLine($"value = {ret}");
            return ret;
        }
    }
}
