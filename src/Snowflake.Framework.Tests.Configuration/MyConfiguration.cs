﻿using Snowflake.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowflake.Framework.Tests.Configuration
{
    [ConfigurationSection("myconfig", "myconfig")]
    public partial interface MyConfiguration
    {
        [ConfigurationOption("Hello", true)]
        bool MyBoolean { get; set; }

        [ConfigurationOption("myenum", MyEnum.World)]
        MyEnum MyEnum { get; set; }

        [ConfigurationOption("ss")]
        Guid MyResource { get; set; }

    }

    public enum MyEnum
    {
        [SelectionOption("hello")]
        Hello,
        [SelectionOption("world")]
        World
    }
}
