// UNDF - Unified Navigation & Docking Framework
// Module: ATC
// Version: 0.5.0-alpha

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using System;
using System.Collections.Generic;

public partial class Program : MyGridProgram
{
    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
        UNDF_ATC_Init();
    }

    public void Main(string argument, UpdateType updateSource)
    {
        UNDF_ATC_HandleMessages();
        UNDF_ATC_Update();
    }
}


public partial class Program
{
    // ===== UNDF ATC CONFIG =====
    const string UNDF_DOCK_TAG = "[UNDF][DOCK]";
    const double UNDF_DOCK_APPROACH_OFFSET = 20.0; // meters
}


public partial class Program
{
    enum UNDF_ATCState
    {
        Online,
        Holding,
        Emergency
    }

    UNDF_ATCState CurrentState = UNDF_ATCState.Online;
}


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


public partial class Program
{
    Dictionary<string, UNDF_DroneInfo> Drones =
        new Dictionary<string, UNDF_DroneInfo>();

    Dictionary<string, UNDF_DockInfo> Docks =
        new Dictionary<string, UNDF_DockInfo>();
}


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


public partial class Program
{
    void UNDF_ATC_HandleMessages()
    {
        var listener = IGC.UnicastListener;

        while (listener.HasPendingMessage)
        {
            var msg = listener.AcceptMessage();
            UNDF_ATC_ParseMessage(msg.Data.ToString(), msg.Source);
        }
    }

    void UNDF_ATC_ParseMessage(string msg, long source)
    {
        var p = msg.Split('|');

        switch (p[0])
        {
            case "REGISTER":
                UNDF_ATC_RegisterDrone(p[1], source);
                break;

            case "REQUEST_DOCK":
                UNDF_ATC_HandleDockRequest(p[1]);
                break;

            case "DOCKED":
                UNDF_ATC_MarkDocked(p[1]);
                break;
        }
    }
}


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


public partial class Program
{
    void UNDF_ATC_Update()
    {
        // Phase 0.5: nothing active here yet
        // Later: carrier motion, emergencies, jump prep, etc.
    }
}


public partial class Program
{
    string UNDF_FormatVector(Vector3D v)
    {
        return v.X + "," + v.Y + "," + v.Z;
    }
}


