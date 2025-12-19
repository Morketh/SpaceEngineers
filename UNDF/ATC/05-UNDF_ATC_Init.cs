public partial class Program
{
    void UNDF_ATC_Init()
    {
        UNDF_DiscoverDocks();

        Echo("UNDF ATC ONLINE");
        Echo($"Docks found: {Docks.Count}");
    }

    void UNDF_DiscoverDocks()
    {
        var connectors = new List<IMyShipConnector>();
        GridTerminalSystem.GetBlocksOfType(connectors,
            c => c.CustomName.Contains(UNDF_DOCK_TAG));

        foreach (var c in connectors)
        {
            var dock = new UNDF_DockInfo
            {
                DockId = c.CustomName,
                Connector = c,
                Occupied = false,
                OccupantDroneUUID = ""
            };

            Docks[dock.DockId] = dock;
        }
    }
}
