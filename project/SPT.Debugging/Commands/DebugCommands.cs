using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using SPT.Debugging.Scripts;

namespace SPT.Debugging.Commands
{
    public class DebugCommands
    {
        [ConsoleCommand("debug_extract", "", null, "keys: Survived, Killed, Runner, MissingInAction, Transit", new string[] { })]
        public static void DebugExtract(ExitStatus status)
        {
            bool isGameActive = Singleton<AbstractGame>.Instantiated;
            if (!isGameActive)
            {
                ConsoleScreen.LogError("Game is not active");
                return;
            }

            LocalGame game = Singleton<AbstractGame>.Instance as LocalGame;
            if (game == null)
            {
                ConsoleScreen.LogError("Game is not a local game");
                return;
            }

            string profileId = GamePlayerOwner.MyPlayer.ProfileId;
            string exitName = Singleton<GameWorld>.Instance.ExfiltrationController.ExfiltrationPoints.FirstOrDefault().name;
            game.Stop(profileId, status, exitName);
        }

        [ConsoleCommand("botmon", "", null, "botmon 0 - off; botmon 1 - on", new string[] { })]
        public static void Botmon(int enabled)
        {
            bool isGameActive = Singleton<AbstractGame>.Instantiated;
            if (!isGameActive)
            {
                ConsoleScreen.LogError("Game is not active");
                return;
            }

            LocalGame game = Singleton<AbstractGame>.Instance as LocalGame;
            if (game == null)
            {
                ConsoleScreen.LogError("Game is not a local game");
                return;
            }

            if (enabled == 0)
            {
                ConsoleScreen.Log("Botmonitor set to disabled");
                var script = game.gameObject.GetComponent<BotmonitorScript>();

                if (script == null)
                {
                    return;
                }

                script.DestorySelf();
            }
            else
            {
                ConsoleScreen.Log("Botmonitor set to enabled");
                var script = game.gameObject.GetComponent<BotmonitorScript>();

                if (script != null)
                {
                    return;
                }

                game.gameObject.AddComponent<BotmonitorScript>();
            }
        }
    }
}
