using System;
using VRage.Game.ModAPI.Ingame;

namespace UNDF_Common
{
    public static class DroneID
    {
        public static string LoadOrCreateUUID(string storage, string entityId, string gridName, out string newStorage)
        {
            if (!string.IsNullOrWhiteSpace(storage))
            {
                newStorage = storage;
                return storage;
            }

            int hash = (entityId + gridName).GetHashCode();
            string uuid = "DRONE_" + Math.Abs(hash).ToString("X");

            newStorage = uuid;
            return uuid;
        }
    }
}
