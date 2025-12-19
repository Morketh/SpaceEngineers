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
