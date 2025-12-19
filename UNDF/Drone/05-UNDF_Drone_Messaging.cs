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
