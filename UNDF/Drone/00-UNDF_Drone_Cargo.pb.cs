// UNDF – Unified Navigation & Docking Framework
// Module: Drone / Cargo
// Version: 0.5.0-alpha

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using System;
using System.Collections.Generic;

public partial class Program : MyGridProgram
{
    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
        UNDF_Init();
    }

    public void Main(string argument, UpdateType updateSource)
    {
        UNDF_HandleMessages();
        UNDF_UpdateStateMachine();
    }
}
