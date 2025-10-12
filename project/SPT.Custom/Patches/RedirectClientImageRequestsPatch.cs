using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.Custom.Patches;

/// <summary>
/// Redirects Trader and quest images to the SPT folder, not the app data
/// </summary>
///
public class RedirectClientImageRequestsPatch : ModulePatch
{
    private static readonly string _sptPath = Path.Combine(Environment.CurrentDirectory, "SPT", "user", "sptappdata");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProfileEndpointFactoryAbstractClass), nameof(ProfileEndpointFactoryAbstractClass.method_28));
    }

    [PatchPrefix]
    public static bool PatchPrefix(string baseUrl, string url, ref Task<Texture2D> __result)
    {
        var texture2D = CacheResourcesPopAbstractClass.Pop<Texture2D>(url.ConvertToResourceLocation());

        if (texture2D != null)
        {
            __result = Task.FromResult(texture2D);
        }
        else
        {
            var path = _sptPath + url;

            if (File.Exists(path))
            {
                __result = GetTexture0(path);
            }
            else
            {
                __result = GetTexture1(baseUrl + url, path);
            }
        }

        return false; // Skip original
    }

    public static async Task<Texture2D> GetTexture0(string path)
    {
        var result = await ProfileEndpointFactoryAbstractClass.smethod_0(path);

        return result.Value;
    }

    public static async Task<Texture2D> GetTexture1(string path, string localPath)
    {
        var result = await ProfileEndpointFactoryAbstractClass.smethod_1(path);

        if (result.Succeed)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                File.WriteAllBytes(localPath, result.Value.EncodeToPNG());
                result.Value.filterMode = FilterMode.Bilinear;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError("Can't get access to the folder: " + ex);
            }
        }

        return result.Value;
    }
}
