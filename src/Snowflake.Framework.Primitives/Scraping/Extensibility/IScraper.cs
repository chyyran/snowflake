﻿using Snowflake.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowflake.Scraping.Extensibility
{
    public interface IScraper : IPlugin
    {
        AttachTarget AttachPoint { get; }
        string TargetType { get; }
        IEnumerable<IScraperDirective> Directives { get; }
        Task<IEnumerable<SeedTreeAwaitable>> ScrapeAsync(ISeed target,
            ILookup<string, SeedContent> rootSeeds,
            ILookup<string, SeedContent> childSeeds,
            ILookup<string, SeedContent> siblingSeeds);
    }
}