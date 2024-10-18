﻿using EvoSC.Manialinks.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace EvoSC.Manialinks.Tests;

public class ManialinkActionManagerControllerRegistrationTests
{
    public class SimpleManialinkController : ManialinkController
    {
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    public class SimpleManialinkWithoutPostfixes : ManialinkController
    {
        public Task TestAction() => Task.CompletedTask;
    }
    
    public class SimpleInvalidManialinkController
    {
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    [ManialinkRoute(Route = "MyRoute")]
    public class CustomClassRouteController : ManialinkController
    {
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    public class CustomActionRouteController : ManialinkController
    {
        [ManialinkRoute(Route = "MyAction")]
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    [ManialinkRoute(Route = "MyRoute")]
    public class CustomFullRouteController : ManialinkController
    {
        [ManialinkRoute(Route = "MyAction")]
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    public class ActionWithParamsController : ManialinkController
    {
        public Task TestActionAsync(int a, int b) => Task.CompletedTask;
    }
    
    public class ActionWithPermissionsController : ManialinkController
    {
        [ManialinkRoute(Permission = "MyPermission")]
        public Task TestActionAsync() => Task.CompletedTask;
    }

    public enum MyPermissions
    {
        MyPermission1
    }
    
    public class ActionWithEnumIdentPermissionsController : ManialinkController
    {
        [ManialinkRoute(Permission = MyPermissions.MyPermission1)]
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    [ManialinkRoute(Permission = "MyPermission")]
    public class ActionWithPermissionsFromClassController : ManialinkController
    {
        public Task TestActionAsync() => Task.CompletedTask;
    }
    
    [ManialinkRoute(Permission = "MyPermission")]
    public class ActionWithOverridingPermissionsController : ManialinkController
    {
        [ManialinkRoute(Permission = "MyOverriddenPermission")]
        public Task TestActionAsync() => Task.CompletedTask;
    }

    [Fact]
    public void Test_Route_Registration_Without_Custom_Components()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(SimpleManialinkController));

        var (action, route) = actionManager.FindAction("SimpleManialink/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
    }
    
    [Fact]
    public void Test_Route_Registration_Without_Custom_Components_Without_Postfixes()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(SimpleManialinkWithoutPostfixes));

        var (action, route) = actionManager.FindAction("SimpleManialinkWithoutPostfixes/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
    }

    [Fact]
    public void Test_Route_Unregistration()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(SimpleManialinkController));
        actionManager.UnregisterForController(typeof(SimpleManialinkController));

        Assert.Throws<InvalidOperationException>(() =>
        {
            var (action, route) = actionManager.FindAction("SimpleManialink/TestAction");
        });
    }

    [Fact]
    public void Test_Route_Registration_With_Invalid_Controller()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());

        Assert.Throws<InvalidOperationException>(() =>
        {
            actionManager.RegisterForController(typeof(SimpleInvalidManialinkController));
        });
    }

    [Fact]
    public void Test_Route_Registration_With_Custom_Class_Route()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(CustomClassRouteController));

        var (action, route) = actionManager.FindAction("MyRoute/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Custom_Action_Route()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(CustomActionRouteController));

        var (action, route) = actionManager.FindAction("CustomActionRoute/MyAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Custom_Controller_And_Action_Route()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(CustomFullRouteController));

        var (action, route) = actionManager.FindAction("MyRoute/MyAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Action_Parameters()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(ActionWithParamsController));

        var (action, route) = actionManager.FindAction("ActionWithParams/TestAction/1/2");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
        
        Assert.NotNull(route.Children?.Values.FirstOrDefault()?.Children?.Values.FirstOrDefault()?.Children?.Values.FirstOrDefault());
        
        Assert.True(route.Children.Values.First().Children.Values.First().IsParameter);
        Assert.True(route.Children.Values.First().Children.Values.First().Children.Values.First().IsParameter);
        Assert.Equal("1", route.Children.Values.First().Children.Values.First().Name);
        Assert.Equal("2", route.Children.Values.First().Children.Values.First().Children.Values.First().Name);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Action_Permission()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(ActionWithPermissionsController));

        var (action, route) = actionManager.FindAction("ActionWithPermissions/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
        Assert.Equal("MyPermission", action.Permission);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Action_Permission_Using_Enum_Identifier()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(ActionWithEnumIdentPermissionsController));

        var (action, route) = actionManager.FindAction("ActionWithEnumIdentPermissions/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
        Assert.Equal("MyPermissions.MyPermission1", action.Permission);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Action_Permission_From_Class()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(ActionWithPermissionsFromClassController));

        var (action, route) = actionManager.FindAction("ActionWithPermissionsFromClass/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
        Assert.Equal("MyPermission", action.Permission);
    }
    
    [Fact]
    public void Test_Route_Registration_With_Action_Overridden_Permission()
    {
        var actionManager = new ManialinkActionManager(new NullLogger<ManialinkActionManager>());
        
        actionManager.RegisterForController(typeof(ActionWithOverridingPermissionsController));

        var (action, route) = actionManager.FindAction("ActionWithOverridingPermissions/TestAction");
        
        Assert.NotNull(action);
        Assert.NotNull(route);
        Assert.Equal("MyOverriddenPermission", action.Permission);
    }
}
