﻿using EvoSC.Common.Interfaces.Themes;
using EvoSC.Common.Themes;
using EvoSC.Common.Themes.Attributes;

namespace EvoSC.Modules.Official.ASayModule.Themes;

[Theme(Name = "ASay", Description = "Default theme for ASay")]
public class ASayTheme : Theme<ASayTheme>
{
    private readonly dynamic _theme;
    
    public ASayTheme(IThemeManager theme)
    {
        _theme = theme.Theme;
    }
    
    public override Task ConfigureAsync()
    {
        Set("ASayModule.Announcement.Default.TextSize").To(3);
        Set("ASayModule.Announcement.Default.IconSize").To(4);
        Set("ASayModule.Announcement.Default.Bg").To(_theme.UI_BgPrimary);
        Set("ASayModule.Announcement.Default.BgSecondary").To(_theme.White);
        Set("ASayModule.Announcement.Default.Text").To(_theme.UI_TextPrimary);
        
        return Task.CompletedTask;
    }
}
