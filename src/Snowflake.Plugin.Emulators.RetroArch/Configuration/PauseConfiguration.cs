using Snowflake.Configuration;
using Snowflake.Configuration.Attributes;

// autogenerated using generate_retroarch.py
namespace Snowflake.Plugin.Emulators.RetroArch.Configuration
{
    [ConfigurationSection("pause", "Pause Options")]
    public partial interface PauseConfiguration
    {
        [ConfigurationOption("pause_nonactive", true, DisplayName = "Pause when not in focus")]
        bool PauseNonactive { get; set; }
    }
}
