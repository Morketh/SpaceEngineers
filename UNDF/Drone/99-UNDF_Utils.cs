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
