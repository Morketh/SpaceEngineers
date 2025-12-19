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
