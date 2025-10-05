using p5rpc.CustomSaveDataFramework.Interfaces;
using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.customsavedata.Configuration;
using p5rpc.flowutils.customsavedata.Template;
using p5rpc.flowutils.logging;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

#if DEBUG
using System.Diagnostics;
#endif

namespace p5rpc.flowutils.customsavedata
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

#if DEBUG
            // Attaches debugger in debug mode; ignored in release.
            Debugger.Launch();
#endif

            var logger = new Logger(_logger);
            var flowFrameworkController = _modLoader.GetController<IFlowFramework>();
            if (flowFrameworkController == null || !flowFrameworkController.TryGetTarget(out var flowFramework))
            {
                throw new Exception("Failed to get IFlowFramework Controller");
            }

            var customSaveDataController = _modLoader.GetController<ICustomSaveDataFramework>();
            if (customSaveDataController == null || !customSaveDataController.TryGetTarget(out var customSaveDataFramework))
            {
                throw new Exception("Failed to get ICustomSaveDataFramework Controller");
            }

            var functions = new FlowFunctions(flowFramework, logger, _modLoader, customSaveDataFramework);
            functions.RegisterCustomSaveDataHandlers();
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