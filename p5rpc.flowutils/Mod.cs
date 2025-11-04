using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.Configuration;
using p5rpc.flowutils.logging;
using p5rpc.flowutils.Template;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace p5rpc.flowutils
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;


            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            var logger = new Logger(_logger, _configuration.loglevel);
            var flowFrameworkController = _modLoader.GetController<IFlowFramework>();
            if (flowFrameworkController == null || !flowFrameworkController.TryGetTarget(out var flowFramework))
            {
                throw new Exception("Failed to get IFlowFramework Controller");
            }

            var p5rLibController = _modLoader.GetController<IP5RLib>();
            if (p5rLibController == null || !p5rLibController.TryGetTarget(out var p5rLib))
            {
                throw new Exception("Failed to get IP5RLib Controller");
            }

            var functions = new FlowFunctions(flowFramework, _modLoader, ref logger, p5rLib.FlowCaller);
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}