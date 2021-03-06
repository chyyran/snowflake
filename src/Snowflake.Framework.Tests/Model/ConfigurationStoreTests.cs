﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Snowflake.Configuration;
using Snowflake.Configuration.Internal;
using Snowflake.Configuration.Tests;
using Snowflake.Model.Database;
using Snowflake.Model.Database.Models;
using Xunit;

namespace Snowflake.Model.Tests
{
    interface NotASectionTemplate
    {

    }

    [ConfigurationSection("templateName", "displayName")]
    partial interface SectionTemplate
    {

    }

    //public class Test
    //{
    //    public void TestAnalyzer()
    //    {
    //        ConfigurationSection<SectionTemplate> ok = new();
    //        ConfigurationSection<NotASectionTemplate> notOk = new();
    //        var x = new ConfigurationSection<NotASectionTemplate>();

    //        void AcceptsNoTemplate(IConfigurationSection<NotASectionTemplate> x)
    //        {

    //        }
    //    }
    //}

    public class ConfigurationStoreTests
    {
        [Fact]
        public void ConfigurationStore_CreateAndRetrieve_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var gameGuid = Guid.NewGuid();
  
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
     
            var retrieved = configStore.GetConfiguration<ExampleConfigurationCollection>
                (config.ValueCollection.Guid);
        }

        [Fact]
        public void ConfigurationStore_CreateAndRetrieveEnsure_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var gameGuid = Guid.NewGuid();
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            // trigger an ensure of the ExampleConfiguration
            var res = config.Configuration.ExampleConfiguration.FullscreenResolution;
            configStore.UpdateConfiguration(config);
            var retrieved = configStore.GetConfiguration<ExampleConfigurationCollection>
                (config.ValueCollection.Guid);
        }

        [Fact]
        public void ConfigurationStore_CreateAndRetrieveEnsureUpdate_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var gameGuid = Guid.NewGuid();
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            // trigger an ensure of the ExampleConfiguration
            config.Configuration.ExampleConfiguration.FullscreenResolution
                = Configuration.FullscreenResolution.Resolution3840X2160;
            configStore.UpdateConfiguration(config);
            var retrieved = configStore.GetConfiguration<ExampleConfigurationCollection>
                (config.ValueCollection.Guid);
            Assert.Equal(Configuration.FullscreenResolution.Resolution3840X2160, retrieved
                .Configuration.ExampleConfiguration.FullscreenResolution);
        }

        [Fact]
        public void ConfigurationStore_ValueConsistency_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            
            var setValue = config.ValueCollection[config.GetSection(e => e.ExampleConfiguration).Descriptor,
                nameof(config.Configuration.ExampleConfiguration.FullscreenResolution)];
            config.Configuration.ExampleConfiguration.FullscreenResolution
                = Configuration.FullscreenResolution.Resolution3840X2160;
            configStore.UpdateConfiguration(config);
            // trigger an ensure of the ExampleConfiguration
            var getConfig = configStore.GetConfiguration<ExampleConfigurationCollection>(config.ValueCollection.Guid);
            Assert.Equal(setValue.Value, getConfig.ValueCollection[setValue.Guid].value.Value);
        }

        [Fact]
        public void ConfigurationStore_GetValue_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            var setValue = config.ValueCollection[config.GetSection(e => e.ExampleConfiguration).Descriptor,
                nameof(config.Configuration.ExampleConfiguration.FullscreenResolution)];
            config.Configuration.ExampleConfiguration.FullscreenResolution
                = Configuration.FullscreenResolution.Resolution3840X2160;
            configStore.UpdateConfiguration(config);
            var getValue = configStore.GetValue(setValue.Guid);
            Assert.Equal(setValue.Value, (Configuration.FullscreenResolution)getValue.Value);
        }

        [Fact]
        public async Task ConfigurationStore_GetValueAsync_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            var setValue = config.ValueCollection[config.GetSection(e => e.ExampleConfiguration).Descriptor,
                nameof(config.Configuration.ExampleConfiguration.FullscreenResolution)];
            config.Configuration.ExampleConfiguration.FullscreenResolution
                = Configuration.FullscreenResolution.Resolution3840X2160;
            configStore.UpdateConfiguration(config);
            var getValue = await configStore.GetValueAsync(setValue.Guid);
            Assert.Equal(setValue.Value, (Configuration.FullscreenResolution)getValue.Value);
        }

        [Fact]
        public void ConfigurationStore_Delete_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            // trigger an ensure of the ExampleConfiguration
            var getConfig = configStore.GetConfiguration<ExampleConfigurationCollection>(config.ValueCollection.Guid);
            Assert.NotNull(getConfig);
            configStore.DeleteConfiguration(config.ValueCollection.Guid);
            Assert.Null(configStore.GetConfiguration<ExampleConfigurationCollection>(config.ValueCollection.Guid));
        }

        [Fact]
        public async Task ConfigurationStore_DeleteAsync_Test()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.GetTempFileName()}");
            var configStore = new ConfigurationCollectionStore(optionsBuilder);
            var x = new ConfigurationCollection<ExampleConfigurationCollection>();
            var config = configStore
                .CreateConfiguration<ExampleConfigurationCollection>("TestConfiguration");
            // trigger an ensure of the ExampleConfiguration
            var getConfig = await configStore.GetConfigurationAsync<ExampleConfigurationCollection>(config.ValueCollection.Guid);
            Assert.NotNull(getConfig);
            await configStore.DeleteConfigurationAsync(config.ValueCollection.Guid);
            Assert.Null(await configStore.GetConfigurationAsync<ExampleConfigurationCollection>(config.ValueCollection.Guid));
        }
    }
}
