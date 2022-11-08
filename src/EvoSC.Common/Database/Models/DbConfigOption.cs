﻿using Dapper.Contrib.Extensions;

namespace EvoSC.Common.Database.Models;

[Table("configoptions")]
public class DbConfigOption
{
    [Key]
    public string Key { get; set; }
    public string Value { get; set; }
}