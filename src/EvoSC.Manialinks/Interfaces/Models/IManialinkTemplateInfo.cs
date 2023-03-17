﻿using System.Reflection;

namespace EvoSC.Manialinks.Interfaces.Models;

public interface IManialinkTemplateInfo
{
    /// <summary>
    /// Assemblies required to render this template.
    /// </summary>
    public IEnumerable<Assembly> Assemblies { get; }
    
    /// <summary>
    /// The full qualified name of this template.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The content of this template.
    /// </summary>
    public string Content { get; }
}
