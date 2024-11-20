using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace SPT.Custom.Utils;

public class MemoryManager : MonoBehaviour
{
    private WaitForSecondsRealtime _gcCollectionTime = new(30f);

    public void Awake()
    {
        StartCoroutine(MemoryManagerCoroutine());
    }

    private IEnumerator MemoryManagerCoroutine()
    {
        yield return _gcCollectionTime;
 
        // SPTCustomPlugin.Log.LogDebug($"Allocated Mananged Memory {GC.GetTotalMemory(false) / 1024f / 1024f} MB");

        Task.Run(CollectMemory);

        StartCoroutine(MemoryManagerCoroutine());
    }

    private Task CollectMemory()
    {
        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        GC.Collect(2, GCCollectionMode.Optimized, false, true);

        return Task.CompletedTask;
    }
}