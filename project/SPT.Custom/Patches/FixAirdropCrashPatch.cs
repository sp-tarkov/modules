using EFT.SynchronizableObjects;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;

namespace SPT.Custom.Patches
{
	/// <summary>
	/// This patch prevents the crashing that was caused by the airdrop since the mortar event, it fully unloads whatever synchronized objects
	/// Are still loaded before unused resources are cleaned up (Which causes this crash)
	/// </summary>
	public class FixAirdropCrashPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.method_48));
		}

		[PatchPrefix]
		public static void Prefix()
		{
			if (Singleton<GameWorld>.Instantiated)
			{
				GameWorld gameWorld = Singleton<GameWorld>.Instance;

				List<SynchronizableObject> synchronizableObjectList = Traverse.Create(gameWorld.SynchronizableObjectLogicProcessor).Field<List<SynchronizableObject>>("list_0").Value;

				foreach (SynchronizableObject obj in synchronizableObjectList)
				{
					obj.Logic.ReturnToPool();
					obj.ReturnToPool();
				}

				gameWorld.SynchronizableObjectLogicProcessor.Dispose();

			}
		}
	}
}
