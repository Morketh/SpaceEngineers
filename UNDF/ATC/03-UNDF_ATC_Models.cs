public partial class Program
{
    class UNDF_DroneInfo
    {
        public string DroneUUID;
        public long IGCAddress;
        public bool Docked;
        public string AssignedDockId;
    }

    class UNDF_DockInfo
    {
        public string DockId;
        public IMyShipConnector Connector;
        public bool Occupied;
        public string OccupantDroneUUID;
    }
}
