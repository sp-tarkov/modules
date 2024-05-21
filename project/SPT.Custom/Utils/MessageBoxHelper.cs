using System;
using System.Runtime.InteropServices;

namespace SPT.Custom.Utils
{
    public class MessageBoxHelper
    {
        public enum MessageBoxType : uint
        {
            ABORTRETRYIGNORE = (uint)(0x00000002L | 0x00000010L),
            CANCELTRYCONTINUE = (uint)(0x00000006L | 0x00000030L),
            HELP = (uint)(0x00004000L | 0x00000040L),
            OK = (uint)(0x00000000L | 0x00000040L),
            OKCANCEL = (uint)(0x00000001L | 0x00000040L),
            RETRYCANCEL = (uint)0x00000005L,
            YESNO = (uint)(0x00000004L | 0x00000040L),
            YESNOCANCEL = (uint)(0x00000003L | 0x00000040L),
            DEFAULT = (uint)(0x00000000L | 0x00000010L)
        }

        public enum MessageBoxResult
        {
            ERROR = -1,
            OK = 1,
            CANCEL = 2,
            ABORT = 3,
            RETRY = 4,
            IGNORE = 5,
            YES = 6,
            NO = 7,
            TRY_AGAIN = 10
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBox(IntPtr hwnd, String lpText, String lpCaption, uint uType);

        public static IntPtr GetWindowHandle()
        {
            return GetActiveWindow();
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxType type = MessageBoxType.DEFAULT)
        {
            try
            {
                return (MessageBoxResult)MessageBox(GetWindowHandle(), text, caption, (uint)type); ;
            }
            catch (Exception)
            {
                return MessageBoxResult.ERROR;
            }
        }
    }
}
