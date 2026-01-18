using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace UNDF_ATC
{
    public class Program : MyGridProgram
    {
        // =====================
        // UNDF – ATC CONTROLLER
        // Version: 0.5.0-alpha
        // =====================

        // ===== CONFIG =====
        const string UNDF_DOCK_TAG = "[UNDF][DOCK]";
        const double UNDF_DOCK_APPROACH_OFFSET = 20.0;

        // ===== STATE =====
        enum UNDF_ATCState
        {
            Online,
            Holding,
            Emergency
        }

        UNDF_ATCState CurrentState = UNDF_ATCState.Online;

        // ===== MODELS =====
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

        // ===== STORAGE =====
        Dictionary<string, UNDF_DroneInfo> Drones =
            new Dictionary<string, UNDF_DroneInfo>();

        Dictionary<string, UNDF_DockInfo> Docks =
            new Dictionary<string, UNDF_DockInfo>();

        // ===== CONSTRUCTOR =====
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            UNDF_ATC_Init();
        }

        public void Save()
        {
            // Reserved for persistence if needed later
        }

        // ===== MAIN LOOP =====
        public void Main(string argument, UpdateType updateSource)
        {
            UNDF_ATC_HandleMessages();
            UNDF_ATC_Update();
        }

        // ===== INIT =====
        void UNDF_ATC_Init()
        {
            UNDF_DiscoverDocks();

            Echo("UNDF ATC ONLINE");
            Echo("Docks found: " + Docks.Count);
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

        // ===== MESSAGING =====
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
            if (p.Length == 0) return;

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

        // ===== LOGIC =====
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

                Echo("Drone registered: " + droneUUID);
            }

            IGC.SendUnicastMessage(source, "UNDF", "ACK|ATC");
        }

        void UNDF_ATC_HandleDockRequest(string droneUUID)
        {
            if (!Drones.ContainsKey(droneUUID))
            {
                Echo("Dock request from unknown drone: " + droneUUID);
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
                "DOCK_CLEARANCE|" + dock.DockId + "|" + UNDF_FormatVector(approach)
            );

            Echo("Assigned dock " + dock.DockId + " -> " + droneUUID);
        }

        void UNDF_ATC_MarkDocked(string droneUUID)
        {
            if (!Drones.ContainsKey(droneUUID)) return;

            Drones[droneUUID].Docked = true;
            Echo("Drone docked: " + droneUUID);
        }

        // ===== UPDATE =====
        void UNDF_ATC_Update()
        {
            // Reserved for future ATC behaviors
        }

        // ===== UTILS =====
        string UNDF_FormatVector(Vector3D v)
        {
            return v.ToString();
        }
    }
}
