﻿using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Controllers.Context;
using EvoSC.Common.Events.Attributes;
using EvoSC.Common.Remote;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Modules.Official.FastestCp.Interfaces;
using GbxRemoteNet.Events;

namespace EvoSC.Modules.Official.FastestCp.Controllers;

[Controller]
public class FastestCpController : EvoScController<EventControllerContext>
{
    private readonly IFastestCpService _fastestCpService;

    public FastestCpController(IFastestCpService fastestCpService)
    {
        _fastestCpService = fastestCpService;
    }

    [Subscribe(ModeScriptEvent.WayPoint)]
    public Task RegisterCpTime(object sender, WayPointEventArgs args)
    {
        return _fastestCpService.RegisterCpTime(args);
    }

    [Subscribe(GbxRemoteEvent.EndMap)]
    public Task ResetCpTimes(object sender, MapGbxEventArgs args)
    {
        return _fastestCpService.ResetCpTimes();
    }
}
