﻿using System;
using System.Threading.Tasks;
using EvoSC.Common.Events;
using EvoSC.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleInjector;
using Xunit;

namespace EvoSC.Common.Tests.Events;

public class EventManagerTests
{
    class HandlerRanException : Exception {}
    class HandlerRanException2 : Exception {}

    private readonly ILogger<EventManager> _logger;
    private readonly IServiceProvider _services;
    private readonly Mock<IEvoSCApplication> _app;

    public EventManagerTests()
    {
        _logger = LoggerFactory.Create(c => { }).CreateLogger<EventManager>();
        _services = new ServiceCollection().BuildServiceProvider();
        _app = new Mock<IEvoSCApplication>();
        _app.Setup(a => a.Services).Returns(new Container());
    }
    
    [Fact]
    public async Task Event_Added_AndFired_Dont_Throw_Exception()
    {
        IEventManager manager = new EventManager(_logger, _app.Object, null);

        manager.Subscribe("test", (object sender, EventArgs args) =>
        {
            Console.WriteLine();
            return Task.CompletedTask;
        });
        var ex = await Record.ExceptionAsync(() => manager.RaiseAsync("test", EventArgs.Empty));
        
        Assert.Null(ex);
    }

    [Fact]
    public void Event_Added_And_Fired()
    {
        IEventManager manager = new EventManager(_logger, _app.Object, null);

        manager.Subscribe<EventArgs>("test", (sender, args) => throw new HandlerRanException());

        Assert.ThrowsAsync<HandlerRanException>(async () =>
        {
            await manager.RaiseAsync("test", EventArgs.Empty);
        });
    }

    [Fact]
    public async Task Event_Added_And_Removed()
    {
        IEventManager manager = new EventManager(_logger, _app.Object, null);

        var handler = new AsyncEventHandler<EventArgs>((sender, args) => throw new HandlerRanException());
        
        manager.Subscribe("test", handler);
        manager.Unsubscribe("test", handler);

        var ex = await Record.ExceptionAsync(() => manager.RaiseAsync("test", EventArgs.Empty));
        
        Assert.Null(ex);
    }

    [Fact]
    public void Only_Equal_Event_Handler_Removed()
    {
        IEventManager manager = new EventManager(_logger, _app.Object, null);

        var handler = new AsyncEventHandler<EventArgs>((sender, args) => throw new HandlerRanException());
        var handler2 = new AsyncEventHandler<EventArgs>((sender, args) => throw new HandlerRanException2());
        
        manager.Subscribe("test", handler);
        manager.Subscribe("test", handler2);
        manager.Unsubscribe("test", handler);

        Assert.ThrowsAsync<HandlerRanException2>(async () =>
        {
            await manager.RaiseAsync("test", EventArgs.Empty);
        });
    }
}
