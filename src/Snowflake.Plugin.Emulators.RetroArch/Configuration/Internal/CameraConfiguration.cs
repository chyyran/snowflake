using Snowflake.Configuration;
using Snowflake.Configuration.Attributes;

// autogenerated using generate_retroarch.py
namespace Snowflake.Plugin.Emulators.RetroArch.Configuration.Internal
{
    /// <summary>
    ///     Not applicable on Desktop OS
    /// </summary>
    [ConfigurationSection("camera", "Camera Options")]
    public partial interface CameraConfiguration
    {
        [ConfigurationOption("camera_allow", false, DisplayName = "Camera Allow", Private = true)]
        bool CameraAllow { get; set; }

        [ConfigurationOption("camera_device", "", DisplayName = "Camera Device", Private = true)]
        string CameraDevice { get; set; }

        // this is enum but null is th eonly possible value
        [ConfigurationOption("camera_driver", "null", DisplayName = "Camera Driver", Private = true)]
        string CameraDriver { get; set; }
    }
}
