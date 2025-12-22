using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;

namespace UNDF_Common
{
    public static class IGCHelper
    {
        public static string FormatMessage(string header, string body)

        {
            return $"{header}|{body}";
        }
    }
}