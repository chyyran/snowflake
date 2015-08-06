﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using SharpYaml.Serialization;
using System.IO;
using Snowflake.Plugin.Configuration;

namespace Snowflake.Plugin
{
    public class YamlConfiguration : IPluginConfiguration
    {
        public string ConfigurationFileName { get; }
        public IDictionary<string, dynamic> Configuration { get; private set; }

        private string DefaultValues { get; }
        public YamlConfiguration(string configFileName, string defaultValues)
        {
            this.ConfigurationFileName = configFileName;
            this.Configuration = new Dictionary<string, dynamic>();
            this.DefaultValues = defaultValues;
        }
        public void LoadConfiguration()
        {
            var serializer = new Serializer(new SerializerSettings()
            {
                EmitTags = false
            });
            if (!File.Exists(this.ConfigurationFileName))
            {
                File.Create(this.ConfigurationFileName).Close();
                File.WriteAllText(this.ConfigurationFileName, this.DefaultValues);
            }
            string serializedYaml = File.ReadAllText(this.ConfigurationFileName);
            this.Configuration = serializer.Deserialize<IDictionary<string, dynamic>>(serializedYaml);
        }

        public void SaveConfiguration()
        {
            var serializer = new Serializer(new SerializerSettings()
            {
                EmitTags = false
            });
            string serializedYaml = serializer.Serialize(this.Configuration);
            File.WriteAllText(this.ConfigurationFileName, serializedYaml);
        }
    }
}