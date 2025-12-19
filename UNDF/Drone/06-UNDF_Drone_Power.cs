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
