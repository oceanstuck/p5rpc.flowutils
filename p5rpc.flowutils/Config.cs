using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using p5rpc.flowutils.logging;
using p5rpc.flowutils.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;

namespace p5rpc.flowutils.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Log Level")]
        [DefaultValue(LogLevel.WARNING)]
        [Display(Order = 0)]
        [Category("Logging")]
        public LogLevel loglevel { get; set; } = LogLevel.WARNING;
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
