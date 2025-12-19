public partial class Program
{
    void UNDF_Init()
    {
        Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
        Connector = GridTerminalSystem.GetBlockWithName(UNDF_DOCK_CONNECTOR_NAME) as IMyShipConnector;
        Antenna = UNDF_FindAntenna(UNDF_DRONE_ANTENNA_TAG);

        DroneUUID = UNDF_LoadOrCreateUUID();

        Echo($"UNDF Cargo Drone Ready\nUUID: {DroneUUID}");
    }
}
