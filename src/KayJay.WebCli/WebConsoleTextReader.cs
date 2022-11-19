using System.Net.WebSockets;
using System.Text;

namespace KayJay.WebCli
{

    public class WebConsoleTextReader : TextReader
    {
        String BufferString = ""; // TOOD : Stream or Span<>
        public override int Peek()
        {
            if (String.IsNullOrEmpty(BufferString))
                return -1;
            return (int)BufferString[0];
        }

        public override int Read()
        {
            if (String.IsNullOrEmpty(BufferString))
            {
                var lineText = WebConsole.ReadLine();
                if (String.IsNullOrEmpty(lineText) == false)
                    BufferString += lineText + Environment.NewLine;
            }
            if (String.IsNullOrEmpty(BufferString))
                return -1;
            var ret = (int)BufferString[0];
            BufferString = BufferString.Substring(1);
            return ret;
        }
    }
}
