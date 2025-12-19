public partial class Program
{
    void UNDF_ATC_RegisterDrone(string droneUUID, long source)
    {
        if (!Drones.ContainsKey(droneUUID))
        {
            Drones[droneUUID] = new UNDF_DroneInfo
            {
                DroneUUID = droneUUID,
                IGCAddress = source,
                Docked = false,
                AssignedDockId = ""
            };

            Echo($"Drone registered: {droneUUID}");
        }

        IGC.SendUnicastMessage(source, "UNDF", "ACK|ATC");
    }

    void UNDF_ATC_HandleDockRequest(string droneUUID)
    {
        if (!Drones.ContainsKey(droneUUID))
        {
            Echo($"Dock request from unknown drone: {droneUUID}");
            return;
        }

        foreach (var dock in Docks.Values)
        {
            if (!dock.Occupied)
            {
                AssignDock(droneUUID, dock);
                return;
            }
        }

        IGC.SendUnicastMessage(
            Drones[droneUUID].IGCAddress,
            "UNDF",
            "HOLD"
        );
    }

    void AssignDock(string droneUUID, UNDF_DockInfo dock)
    {
        dock.Occupied = true;
        dock.OccupantDroneUUID = droneUUID;

        Drones[droneUUID].AssignedDockId = dock.DockId;

        Vector3D approach =
            dock.Connector.GetPosition() +
            dock.Connector.WorldMatrix.Forward * UNDF_DOCK_APPROACH_OFFSET;

        IGC.SendUnicastMessage(
            Drones[droneUUID].IGCAddress,
            "UNDF",
            $"DOCK_CLEARANCE|{dock.DockId}|{UNDF_FormatVector(approach)}"
        );

        Echo($"Assigned dock {dock.DockId} -> {droneUUID}");
    }

    void UNDF_ATC_MarkDocked(string droneUUID)
    {
        var drone = Drones[droneUUID];
        drone.Docked = true;

        Echo($"Drone docked: {droneUUID}");
    }
}
