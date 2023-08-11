﻿using EvoSC.Common.Models;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Modules.Official.MatchManagerModule.Config;
using EvoSC.Modules.Official.MatchManagerModule.Controllers;
using EvoSC.Modules.Official.MatchManagerModule.Interfaces;
using EvoSC.Testing.Controllers;
using Moq;

namespace MatchManagerModule.Tests.Controllers;

public class MatchTrackerEventControllerTests : EventControllerTestBase<MatchTrackerEventController>
{
    private Mock<ITrackerSettings> _settings = new();
    private Mock<IMatchTracker> _tracker = new();

    public MatchTrackerEventControllerTests()
    {
        InitMock(_settings, _tracker);
    }

    [Fact]
    public async Task Scores_Report_Is_Tracked()
    {
        var scoresArgs = new ScoresEventArgs
        {
            Section = ModeScriptSection.Undefined,
            UseTeams = false,
            WinnerTeam = 0,
            WinnerPlayer = null,
            Teams = null,
            Players = null
        };

        await Controller.OnScoresAsync(null, scoresArgs);
        
        _tracker.Verify(m => m.TrackScoresAsync(scoresArgs), Times.Once);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Begin_Match_Is_Tracked_Depending_On_Settings(bool automaticTracking, bool isTracked)
    {
        _settings.Setup(m => m.AutomaticTracking).Returns(automaticTracking);

        await Controller.OnBeginMatchAsync(null, EventArgs.Empty);

        var timesCalled = isTracked ? Times.Once() : Times.Never();
        
        _tracker.Verify(m => m.BeginMatchAsync(), timesCalled);
    }
}
