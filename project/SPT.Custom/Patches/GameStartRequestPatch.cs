using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Bootstrap;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches
{
    internal class GameStartRequestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var type = typeof(BackendAbstractClass<IBackEndSession>);
            return AccessTools.Method(type, nameof(BackendAbstractClass<IBackEndSession>.RegenerateToken));
        }

        [PatchPostfix]
        public static async Task PatchPostfix(Task __result)
        {
            // Allow the initial request to go through first, in async this happens in Postfix
            await __result;

            List<ClientMod> clientMods = [];

            foreach (var plugin in Chainloader.PluginInfos.Values)
            {
                clientMods.Add(
                    new ClientMod
                    {
                        Name = plugin.Metadata.Name,
                        GUID = plugin.Metadata.GUID,
                        Version = plugin.Metadata.Version,
                    }
                );
            }

            await RequestHandler.PostJsonAsync("/singleplayer/clientmods", Json.Serialize(new ClientModsRequest(clientMods)));
        }
    }
}
