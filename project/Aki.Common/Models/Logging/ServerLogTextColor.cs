namespace Aki.Common.Models.Logging
{
    public class ServerLogTextColor
    {
        public string Value { get; private set; }

        private ServerLogTextColor(string value) { Value = value; }

        public static ServerLogTextColor Black { get { return new ServerLogTextColor("black"); } }
        public static ServerLogTextColor Red { get { return new ServerLogTextColor("red"); } }
        public static ServerLogTextColor Green { get { return new ServerLogTextColor("green"); } }
        public static ServerLogTextColor Yellow { get { return new ServerLogTextColor("yellow"); } }
        public static ServerLogTextColor Blue { get { return new ServerLogTextColor("blue"); } }
        public static ServerLogTextColor Magenta { get { return new ServerLogTextColor("magenta"); } }
        public static ServerLogTextColor Cyan { get { return new ServerLogTextColor("cyan"); } }
        public static ServerLogTextColor White { get { return new ServerLogTextColor("white"); } }
        public static ServerLogTextColor Gray { get { return new ServerLogTextColor(""); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
