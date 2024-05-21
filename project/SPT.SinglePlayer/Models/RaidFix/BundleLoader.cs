using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;

namespace SPT.SinglePlayer.Models.RaidFix
{
    public struct BundleLoader
    {
        Profile Profile;
        TaskScheduler TaskScheduler { get; }

        public BundleLoader(TaskScheduler taskScheduler)
        {
            Profile = null;
            TaskScheduler = taskScheduler;
        }

        public Task<Profile> LoadBundles(Task<Profile> task)
        {
            Profile = task.Result;

            var loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                PoolManager.PoolsCategory.Raid,
                PoolManager.AssemblyType.Local,
                Profile.GetAllPrefabPaths(false).Where(x => !x.IsNullOrEmpty()).ToArray(),
                JobPriority.General,
                null,
                default(CancellationToken));

            return loadTask.ContinueWith(GetProfile, TaskScheduler);
        }

        private Profile GetProfile(Task task)
        {
            return Profile;
        }
    }
}