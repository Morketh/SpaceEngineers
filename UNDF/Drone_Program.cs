using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

using UNDF_Common;

namespace UNDF_Drone
{
    public class Program : MyGridProgram
    {
        // =====================================================
        // UNDF – Unified Navigation & Docking Framework
        // Module: Drone / Cargo
        // Version: 0.5.0-alpha
        // =====================================================

        // ===== CONFIG =====
        const string UNDF_DRONE_ANTENNA_TAG = "[UNDF][DRONE]";
        const string UNDF_DOCK_CONNECTOR_NAME = "Drone Connector";

        const double UNDF_APPROACH_SPEED = 5.0;
        const double UNDF_DOCK_SPEED     = 0.5;
        const double UNDF_ARRIVAL_DIST   = 2.0;

        // ===== STATE =====
        enum UNDF_DroneState
        {
            Boot,
            Registering,
            Idle,
            RequestingDock,
            Approaching,
            Docking,
            Docked,
            Waiting,
            Undocking,
            Refueling,
            Emergency,
            Error
        }

        UNDF_DroneState DroneState = UNDF_DroneState.Boot;
        UNDF_DroneState ResumeState  = UNDF_DroneState.Idle;

        // ===== BLOCKS =====
        IMyRemoteControl Remote;
        IMyShipConnector Connector;
        IMyRadioAntenna  Antenna;

        // ===== IDENTITY =====
        string DroneUUID;
        string ATCUUID = "";

        // ===== DOCKING =====
        Vector3D AssignedDockPosition;
        bool HasDockAssignment = false;

        // ===== CONSTRUCTOR =====
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            UNDF_Init();
        }

        public void Save()
        {
            // Reserved for persistence expansion
        }

        // ===== MAIN LOOP =====
        public void Main(string argument, UpdateType updateSource)
        {
            if(!string.IsNullOrWhiteSpace(argument))
                HandleRunArgument(argument);
            UNDF_HandleMessages();
            UNDF_UpdateStateMachine();
        }

        // ===== INIT =====
        void UNDF_Init()
        {
            Remote   = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
            Connector = GridTerminalSystem.GetBlockWithName(UNDF_DOCK_CONNECTOR_NAME) as IMyShipConnector;
            Antenna  = UNDF_FindAntenna(UNDF_DRONE_ANTENNA_TAG);

            DroneUUID = UNDF_LoadOrCreateUUID();

            UNDF_InitIGC(); // Initialize IGC listeners

            Echo("UNDF Cargo Drone Ready");
            Echo("UUID: " + DroneUUID);
        }

        // ===== MESSAGING =====
        void HandleRunArgument(string argument)
        {
            // Normalize to the same format used by IGC
            HandleUNDFEvent(new string[] { "UNDF_EVENT", argument });
        }

        // Listeners
        IMyBroadcastListener broadcastListener;
        IMyUnicastListener unicastListener;

        void UNDF_InitIGC()
        {
            broadcastListener = IGC.RegisterBroadcastListener("UNDF_CMD");
            broadcastListener.SetMessageCallback("UNDF_CMD");

            unicastListener = IGC.UnicastListener;
            unicastListener.SetMessageCallback("UNDF_UNI");
        }

        void UNDF_HandleMessages()
        {
            while (broadcastListener.HasPendingMessage)
            {
                var msg = broadcastListener.AcceptMessage();
                // Only handle strings
                if (msg.Data is string)
                    UNDF_ParseMessage((string)msg.Data);
            }

            // Process unicast messages (e.g., ACKs)
            while (unicastListener.HasPendingMessage)
            {
                var msg = unicastListener.AcceptMessage();
                if (msg.Data is string)
                    UNDF_ParseMessage((string)msg.Data);
            }
        }

        void UNDF_BroadcastMessage(string msg)
        {
            IGC.SendBroadcastMessage("UNDF_DRONE", msg);
        }

        void UNDF_UnicastMessage(long targetID, string msg)
        {
            IGC.SendUnicastMessage(targetID, "UNDF_DRONE", msg);
        }

        void UNDF_SendMessage(string msg)
        {
            UNDF_BroadcastMessage(msg);
        }

        void UNDF_ParseMessage(string msg)
        {
            var p = msg.Split('|');
            if (p.Length == 0) return;

            switch (p[0])
            {
                case "ACK":
                    ATCUUID = p[1];
                    DroneState = UNDF_DroneState.Idle;
                    break;

                case "DOCK_CLEARANCE":
                    AssignedDockPosition = UNDF_ParseVector(p[2]);
                    HasDockAssignment = true;
                    DroneState = UNDF_DroneState.Approaching;
                    break;

                case "UNDOCK":
                    DroneState = UNDF_DroneState.Undocking;
                    break;

                case "HOLD":
                    DroneState = UNDF_DroneState.Waiting;
                    break;

                case "UNDF_EVENT":
                    HandleUNDFEvent(p);
                    break;
            }
        }

        void HandleUNDFEvent(string[] p)
        {
            if (p.Length < 2) return;

            switch (p[1])
            {
                case "LOW_POWER":
                    HandleLowPowerEvent();
                    break;

                case "POWER_OK":
                    HandlePowerRestoredEvent();
                    break;
            }
        }

        // ===== ATC INTERACTION =====
        void UNDF_EnterRegistering()
        {
            UNDF_SendMessage("REGISTER|" + DroneUUID);
            DroneState = UNDF_DroneState.Registering;
        }

        void UNDF_SendDockRequest()
        {
            UNDF_SendMessage("REQUEST_DOCK|" + DroneUUID);
        }

        void UNDF_NotifyDocked()
        {
            UNDF_SendMessage("DOCKED|" + DroneUUID);
            DroneState = UNDF_DroneState.Waiting;
        }

        // ===== POWER / REFUEL EVENTS =====
        void HandleLowPowerEvent()
        {
            if (DroneState == UNDF_DroneState.Emergency ||
                DroneState == UNDF_DroneState.Refueling)
                return;

            Echo("⚠ UNDF: LOW POWER EVENT");

            ResumeState = DroneState;
            DroneState = UNDF_DroneState.Emergency;

            UNDF_SendMessage("LOW_POWER|" + DroneUUID);
        }

        void HandlePowerRestoredEvent()
        {
            Echo("✓ UNDF: POWER RESTORED");

            if (DroneState != UNDF_DroneState.Refueling &&
                DroneState != UNDF_DroneState.Emergency)
                return;

            DroneState = UNDF_DroneState.Waiting;
            UNDF_SendMessage("POWER_OK|" + DroneUUID);
        }

        void EnterRefuelingState()
        {
            Echo("⛽ UNDF: REFUELING");
            DroneState = UNDF_DroneState.Refueling;
        }

        // ===== STATE MACHINE =====
        void UNDF_UpdateStateMachine()
        {
            switch (DroneState)
            {
                case UNDF_DroneState.Boot:
                    UNDF_EnterRegistering();
                    break;

                case UNDF_DroneState.RequestingDock:
                    UNDF_SendDockRequest();
                    break;

                case UNDF_DroneState.Approaching:
                    UNDF_UpdateApproach();
                    break;

                case UNDF_DroneState.Docking:
                    UNDF_UpdateDocking();
                    break;

                case UNDF_DroneState.Docked:
                    UNDF_NotifyDocked();
                    break;

                case UNDF_DroneState.Undocking:
                    UNDF_UpdateUndocking();
                    break;
            }
        }

        // ===== NAVIGATION =====
        void UNDF_UpdateApproach()
        {
            if (!HasDockAssignment) return;

            Remote.ClearWaypoints();
            Remote.AddWaypoint(AssignedDockPosition, "UNDF Dock");
            Remote.SpeedLimit = (float)UNDF_APPROACH_SPEED;
            Remote.SetAutoPilotEnabled(true);

            double distance =
                Vector3D.Distance(Remote.GetPosition(), AssignedDockPosition);

            if (distance <= UNDF_ARRIVAL_DIST)
            {
                Remote.SetAutoPilotEnabled(false);
                DroneState = UNDF_DroneState.Docking;
            }
        }

        void UNDF_UpdateDocking()
        {
            if (Connector.Status == MyShipConnectorStatus.Connectable)
            {
                Connector.Connect();
                DroneState = UNDF_DroneState.Docked;
            }
        }

        void UNDF_UpdateUndocking()
        {
            if (Connector.Status == MyShipConnectorStatus.Connected)
                Connector.Disconnect();

            DroneState = UNDF_DroneState.Idle;
        }

        // ===== UTILS =====
        IMyRadioAntenna UNDF_FindAntenna(string tag)
        {
            var ants = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType(ants,
                a => a.CustomName.Contains(tag));

            return ants.Count > 0 ? ants[0] : null;
        }

        string UNDF_LoadOrCreateUUID()
        {
            if (!string.IsNullOrWhiteSpace(Storage))
                return Storage;

            int hash = (Me.EntityId.ToString()+Me.CubeGrid.DisplayName).GetHashCode();

            string uuid = "DRONE_" + Math.Abs(hash).ToString("X");
            Storage = uuid;
            return uuid;
        }

        Vector3D UNDF_ParseVector(string data)
        {
            var p = data.Split(',');
            return new Vector3D(
                double.Parse(p[0]),
                double.Parse(p[1]),
                double.Parse(p[2])
            );
        }
    }
}
