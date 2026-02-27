using p5rpc.flowutils.customsavedata.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using p5rpc.flowutils.customsavedata.logging;

namespace p5rpc.flowutils.customsavedata.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.
    
            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs
    
            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [DisplayName("Log Level")]
        [DefaultValue(LogLevel.INFO)]
        [Display(Order = 0)]
        [Category("Logging")]
        public LogLevel loglevel { get; set; } = LogLevel.INFO;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
