using VRageMath;

namespace UNDF_Common
{
    public static class Utils
    {
        public static Vector3D ParseVector(string data)
        {
            var p = data.Split(',');
            return new Vector3D(
                double.Parse(p[0]),
                double.Parse(p[1]),
                double.Parse(p[2])
            );
        }

        public static string FormatVector(Vector3D v)
        {
            return v.X + "," + v.Y + "," + v.Z;
        }
    }
}
