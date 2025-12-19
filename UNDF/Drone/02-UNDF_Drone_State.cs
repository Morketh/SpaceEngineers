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
