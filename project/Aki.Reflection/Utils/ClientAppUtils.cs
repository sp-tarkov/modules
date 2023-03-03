using Comfort.Common;
using EFT;

namespace Aki.Reflection.Utils
{
    public static class ClientAppUtils
    {
        public static ClientApplication<ISession> GetClientApp()
        {
            return Singleton<ClientApplication<ISession>>.Instance;
        }

        public static TarkovApplication GetMainApp()
        {
            return GetClientApp() as TarkovApplication;
        }
    }
}
