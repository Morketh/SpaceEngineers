public partial class Program
{
    enum UNDF_ATCState
    {
        Online,
        Holding,
        Emergency
    }

    UNDF_ATCState CurrentState = UNDF_ATCState.Online;
}
