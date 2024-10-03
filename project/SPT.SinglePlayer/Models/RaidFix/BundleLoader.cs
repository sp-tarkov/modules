using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;

namespace SPT.SinglePlayer.Models.RaidFix
{
    public struct BundleLoader
    {
        private Profile _profile;
        TaskScheduler TaskScheduler { get; }

        public BundleLoader(TaskScheduler taskScheduler)
        {
            _profile = null;
            TaskScheduler = taskScheduler;
        }

        public Task<Profile> LoadBundles(Task<Profile> task)
        {
            _profile = task.Result;

            var loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                PoolManager.PoolsCategory.Raid,
                PoolManager.AssemblyType.Local,
                _profile.GetAllPrefabPaths(false).Where(x => !x.IsNullOrEmpty()).ToArray(),
                JobPriority.General,
                null,
                default(CancellationToken));

            return loadTask.ContinueWith(GetProfile, TaskScheduler);
        }

        private Profile GetProfile(Task task)
        {
            return _profile;
        }
    }
}