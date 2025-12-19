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
