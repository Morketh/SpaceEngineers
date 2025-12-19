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
