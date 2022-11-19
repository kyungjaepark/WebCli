using System.Text;

namespace KayJay.WebCli
{
    public class WebConsoleTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            WebConsole.old_out?.Write('[');
            WebConsole.old_out?.Write((int)value);
            WebConsole.old_out?.Write(']');
        }
    }


}
