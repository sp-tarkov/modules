namespace Aki.Common.Models.Logging
{
    public class ServerLogBackgroundColor
    {
        public string Value { get; private set; }

        private ServerLogBackgroundColor(string value) { Value = value; }

        public static ServerLogBackgroundColor Default { get { return new ServerLogBackgroundColor(""); } }
        public static ServerLogBackgroundColor Black { get { return new ServerLogBackgroundColor("blackBG"); } }
        public static ServerLogBackgroundColor Red { get { return new ServerLogBackgroundColor("redBG"); } }
        public static ServerLogBackgroundColor Green { get { return new ServerLogBackgroundColor("greenBG"); } }
        public static ServerLogBackgroundColor Yellow { get { return new ServerLogBackgroundColor("yellowBG"); } }
        public static ServerLogBackgroundColor Blue { get { return new ServerLogBackgroundColor("blueBG"); } }
        public static ServerLogBackgroundColor Magenta { get { return new ServerLogBackgroundColor("magentaBG"); } }
        public static ServerLogBackgroundColor Cyan { get { return new ServerLogBackgroundColor("cyanBG"); } }
        public static ServerLogBackgroundColor White { get { return new ServerLogBackgroundColor("whiteBG"); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
