﻿using EvoSC.Common.Database.Models.Maps;
using EvoSC.Common.Interfaces.Controllers;
using EvoSC.Common.Interfaces.Models;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Manialinks.Interfaces;
using EvoSC.Modules.Official.NextMapModule.Controllers;
using EvoSC.Modules.Official.NextMapModule.Interfaces;
using EvoSC.Testing.Controllers;
using NSubstitute;
using Xunit;

namespace EvoSC.Modules.Official.NextMapModule.Tests.Controllers;

public class NextMapEventControllerTests : ControllerMock<NextMapEventController, IEventControllerContext>
{
    private const string Template = "NextMapModule.NextMap";

    private readonly INextMapService _nextMapService = Substitute.For<INextMapService>();
    private readonly IManialinkManager _manialinkManager = Substitute.For<IManialinkManager>();

    public NextMapEventControllerTests()
    {
        InitMock(_nextMapService, _manialinkManager);
    }


    [Fact]
    public async Task OnPodiumStart_Shows_Next_Map()
    {
        var map = new DbMap
        {
            AuthorId = 1,
            Enabled = true,
            Id = 123,
            ExternalId = "1337",
            Name = "snippens dream",
            Uid = "Uid"
        };
        _nextMapService.GetNextMapAsync().Returns(Task.FromResult((IMap) map));

        await Controller.ShowNextMapOnPodiumStartAsync(new(), new PodiumEventArgs
        {
            Time = 0
        });

        await _manialinkManager.Received(1).SendManialinkAsync(Template, Arg.Any<object>());
    }

    [Fact]
    public async Task OnPodiumEnd_Hides_Next_Map()
    {
        var map = new DbMap
        {
            AuthorId = 1,
            Enabled = true,
            Id = 123,
            ExternalId = "1337",
            Name = "snippens dream",
            Uid = "Uid"
        };
        _nextMapService.GetNextMapAsync().Returns(Task.FromResult((IMap) map));
        
        await Controller.HideNextMapOnPodiumEndAsync(new(), null);
        await _manialinkManager.Received(1).HideManialinkAsync(Template);
    }
}
