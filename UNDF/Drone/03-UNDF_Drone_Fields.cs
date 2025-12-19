public partial class Program
{
    // Core blocks
    IMyRemoteControl Remote;
    IMyShipConnector Connector;
    IMyRadioAntenna Antenna;

    // Identity
    string DroneUUID;
    string ATCUUID = "";

    // Dock assignment
    Vector3D AssignedDockPosition;
    bool HasDockAssignment = false;
}
