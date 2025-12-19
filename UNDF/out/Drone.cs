// UNDF – Unified Navigation & Docking Framework
// Module: Drone / Cargo
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
        UNDF_Init();
    }

    public void Main(string argument, UpdateType updateSource)
    {
        UNDF_HandleMessages();
        UNDF_UpdateStateMachine();
    }
}


public partial class Program
{
    // ===== UNDF DRONE CONFIG =====

    const string UNDF_DRONE_ANTENNA_TAG = "[UNDF][DRONE]";
    const string UNDF_DOCK_CONNECTOR_NAME = "Drone Connector";

    const double UNDF_APPROACH_SPEED = 5.0;   // m/s
    const double UNDF_DOCK_SPEED     = 0.5;   // m/s
    const double UNDF_ARRIVAL_DIST   = 2.0;   // meters
}


public partial class Program
{
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

    UNDF_DroneState CurrentState = UNDF_DroneState.Boot;
    UNDF_DroneState ResumeState = UNDF_DroneState.Idle;
}


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


public partial class Program
{
    void UNDF_HandleMessages()
    {
        var listener = IGC.UnicastListener;
        while (listener.HasPendingMessage)
        {
            var msg = listener.AcceptMessage();
            UNDF_ParseMessage(msg.Data.ToString());
        }
    }

    void UNDF_SendMessage(string msg)
    {
        Antenna.TransmitMessage(msg);
    }

    void UNDF_ParseMessage(string msg)
    {
        var p = msg.Split('|');

        switch (p[0])
        {
            case "ACK":
                ATCUUID = p[1];
                CurrentState = UNDF_DroneState.Idle;
                break;

            case "DOCK_CLEARANCE":
                AssignedDockPosition = UNDF_ParseVector(p[2]);
                HasDockAssignment = true;
                CurrentState = UNDF_DroneState.Approaching;
                break;

            case "UNDOCK":
                CurrentState = UNDF_DroneState.Undocking;
                break;

            case "HOLD":
                CurrentState = UNDF_DroneState.Waiting;
                break;
            
            case "UNDF_EVENT":

                switch (p[1])
                {
                    case "LOW_POWER":
                        HandleLowPowerEvent();
                        break;

                    case "POWER_OK":
                        HandlePowerRestoredEvent();
                        break;
                }
                break;
        }
    }

    void UNDF_EnterRegistering()
    {
        UNDF_SendMessage($"REGISTER|{DroneUUID}");
        CurrentState = UNDF_DroneState.Registering;
    }

    void UNDF_SendDockRequest()
    {
        UNDF_SendMessage($"REQUEST_DOCK|{DroneUUID}");
    }

    void UNDF_NotifyDocked()
    {
        UNDF_SendMessage($"DOCKED|{DroneUUID}");
        CurrentState = UNDF_DroneState.Waiting;
    }
}


public partial class Program
{
    // ============================================================
    // UNDF – Power / Refuel Event Handling
    // Driven entirely by ISY timer → PB Run arguments
    // ============================================================

    void HandleLowPowerEvent()
    {
        // Prevent loops / spam
        if (CurrentState == UNDF_DroneState.Emergency ||
            CurrentState == UNDF_DroneState.Refueling)
            return;

        Echo("⚠ UNDF: LOW POWER EVENT RECEIVED");

        // Remember what we were doing (optional future use)
        ResumeState = CurrentState;

        // Enter emergency mode
        CurrentState = UNDF_DroneState.Emergency;

        // Notify ATC immediately for priority handling
        UNDF_SendMessage($"LOW_POWER|{DroneUUID}");
    }

    void HandlePowerRestoredEvent()
    {
        Echo("✓ UNDF: POWER RESTORED EVENT RECEIVED");

        // Only meaningful if we were refueling or emergency
        if (CurrentState != UNDF_DroneState.Refueling &&
            CurrentState != UNDF_DroneState.Emergency)
            return;

        // UNDF does NOT auto-resume missions
        // ATC or operator decides what happens next
        CurrentState = UNDF_DroneState.Waiting;

        UNDF_SendMessage($"POWER_OK|{DroneUUID}");
    }

    void EnterRefuelingState()
    {
        Echo("⛽ UNDF: ENTERING REFUELING STATE");

        CurrentState = UNDF_DroneState.Refueling;
    }
}


public partial class Program
{
    void UNDF_UpdateStateMachine()
    {
        switch (CurrentState)
        {
            case UNDF_DroneState.Boot:
                UNDF_EnterRegistering();
                break;

            case UNDF_DroneState.Registering:
                break;

            case UNDF_DroneState.Idle:
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

            case UNDF_DroneState.Waiting:
                break;

            case UNDF_DroneState.Undocking:
                UNDF_UpdateUndocking();
                break;

            case UNDF_DroneState.Refueling:
                break;

            case UNDF_DroneState.Emergency:
                break;

            case UNDF_DroneState.Error:
                break;
        }
    }
}


public partial class Program
{
    void UNDF_UpdateApproach()
    {
        if (!HasDockAssignment) return;

        Remote.ClearWaypoints();
        Remote.AddWaypoint(AssignedDockPosition, "UNDF Dock Approach");
        Remote.SetAutoPilotEnabled(true);
        Remote.SetAutoPilotSpeedLimit((float)UNDF_APPROACH_SPEED);

        double distance = Vector3D.Distance(Remote.GetPosition(), AssignedDockPosition);

        if (distance <= UNDF_ARRIVAL_DIST)
        {
            Remote.SetAutoPilotEnabled(false);
            CurrentState = UNDF_DroneState.Docking;
        }
    }
}


public partial class Program
{
    void UNDF_UpdateDocking()
    {
        if (Connector.Status == MyShipConnectorStatus.Connectable)
        {
            Connector.Connect();
            CurrentState = UNDF_DroneState.Docked;
        }
    }

    void UNDF_UpdateUndocking()
    {
        if (Connector.Status == MyShipConnectorStatus.Connected)
            Connector.Disconnect();

        CurrentState = UNDF_DroneState.Idle;
    }
}


public partial class Program
{
    IMyRadioAntenna UNDF_FindAntenna(string tag)
    {
        var ants = new List<IMyRadioAntenna>();
        GridTerminalSystem.GetBlocksOfType(ants, a => a.CustomName.Contains(tag));
        return ants.Count > 0 ? ants[0] : null;
    }

    string UNDF_LoadOrCreateUUID()
    {
        if (!string.IsNullOrWhiteSpace(Storage))
            return Storage;

        string uuid = Guid.NewGuid().ToString();
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


