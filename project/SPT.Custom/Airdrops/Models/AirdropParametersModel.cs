namespace SPT.Custom.Airdrops.Models
{
    public class AirdropParametersModel
    {
        public AirdropConfigModel Config;

        public bool AirdropAvailable;
        public float DistanceTraveled;
        public float DistanceToTravel;
        public float DistanceToDrop;
        public float Timer;
        public bool PlaneSpawned;
        public bool BoxSpawned;

        public int DropHeight;
        public int TimeToStart;

        public UnityEngine.Vector3 RandomAirdropPoint;
    }
}