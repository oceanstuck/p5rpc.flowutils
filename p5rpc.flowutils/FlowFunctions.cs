using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.logging;
using p5rpc.lib.interfaces;
using Reloaded.Mod.Interfaces;
using RemixToolkit.Core.Configs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace p5rpc.flowutils;

internal class FlowFunctions
{
    private IFlowFramework _flowFramework;
    private Logger _logger;
    private IModLoader _modLoader;

    private IFlowCaller _flowCaller;
    private List<(int, int)> confidantIds;

    /* private enum ItemSection
    {
        Melee,
        Armor = 0x1000,
        Accessory = 0x2000,
        Consumable = 0x3000,
        KeyItem = 0x4000,
        Loot = 0x5000,
        SkillCard = 0x6000,
        Outfit = 0x7000,
        Gun = 0x8000
    } */

    public FlowFunctions(IFlowFramework flowFramework, IModLoader modLoader, ref Logger logger, IFlowCaller flowCaller)
    {
        _flowFramework = flowFramework;
        _logger = logger;
        _modLoader = modLoader;
        _flowCaller = flowCaller;

        confidantIds = new List<(int, int)>()
        {
            (3, 23),
            (4, 24),
            (7, 25),
            (10, 26),
            (11, 27),
            (14, 28),
            (15, 29),
            (16, 30),
            (18, 31),
            // (21, 32), sae altid is never used
            (33, 34),
            (36, 37)
        };

        RegisterConfigReaders();
        RegisterMiscFunctions();
    }

    private void RegisterConfigReaders()
    { 
        _flowFramework.Register("IS_MOD_ENABLED", 1, () =>
        {
            _logger.WriteLog(LogLevel.DEBUG, $"Calling IS_MOD_ENABLED...");

            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            _logger.WriteLog(LogLevel.DEBUG, $"Mod id passed to IS_MOD_ENABLED: {modId}");

            var isEnabled = _modLoader.GetAppConfig().EnabledMods.Contains(modId) ? 1 : 0;
            flowApi.SetReturnValue(isEnabled);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CONFIG_INT_VALUE", 2, () =>
        {
            _logger.WriteLog(LogLevel.DEBUG, $"Calling GET_CONFIG_INT_VALUE...");

            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            _logger.WriteLog(LogLevel.DEBUG, $"Mod id passed to GET_CONFIG_INT_VALUE: {modId}");

            var configId = flowApi.GetStringArg(1);
            _logger.WriteLog(LogLevel.DEBUG, $"Config id passed to GET_CONFIG_INT_VALUE: {configId}");

            flowApi.SetReturnValue((int)GetConfigValue(modId, configId)!);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CONFIG_FLOAT_VALUE", 2, () =>
        {
            _logger.WriteLog(LogLevel.DEBUG, $"Calling GET_CONFIG_FLOAT_VALUE...");

            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            _logger.WriteLog(LogLevel.DEBUG, $"Mod id passed to GET_CONFIG_FLOAT_VALUE: {modId}");

            var configId = flowApi.GetStringArg(1);
            _logger.WriteLog(LogLevel.DEBUG, $"Config id passed to GET_CONFIG_FLOAT_VALUE: {configId}");

            flowApi.SetReturnValue((float)GetConfigValue(modId, configId, true)!);

            return FlowStatus.SUCCESS;

        });
    }

    private object GetConfigValue(string modId, string configId, bool isFloat = false)
    {
        object? value = null;

        var r2ConfigPath = Path.Combine(_modLoader.GetModConfigDirectory(modId), "Config.json");
        if (File.Exists(r2ConfigPath))
        {
            _logger.WriteLog(LogLevel.DEBUG, $"R2 config file found for {modId}");
            if (!TryGetR2ConfigValue(modId, configId, r2ConfigPath, out value, isFloat))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to read R2 config file for {modId}.");
                throw new ArgumentException($"Failed to read R2 config file for {modId}.");
            }
        }
        else
        {
            var remixConfigPath = Path.Combine(_modLoader.GetModConfigDirectory(modId), "ReMIX", "Config", "data.yaml");
            if (File.Exists(remixConfigPath))
            {
                _logger.WriteLog(LogLevel.DEBUG, $"ReMIX config file found for {modId}");
                var remixSchemaPath = Path.Combine(_modLoader.GetDirectoryForModId(modId), "ReMIX", "Config", "config.yaml");
                if (File.Exists(remixSchemaPath))
                {
                    _logger.WriteLog(LogLevel.DEBUG, $"ReMIX schema file found for {modId}");
                    if (!TryGetRemixConfigValue(modId, configId, remixConfigPath, remixSchemaPath, out value, isFloat))
                    {
                        _logger.WriteLog(LogLevel.ERROR, $"Failed to read ReMIX config file for {modId}");
                        throw new ArgumentException($"Failed to read ReMIX config file for {modId}");
                    }
                }
                else
                {
                    _logger.WriteLog(LogLevel.ERROR, $"Failed to find schema file for {modId}");
                    throw new FileNotFoundException($"Missing schema file for {modId}");
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.WARNING, $"Failed to find config file for {modId} (is this mod installed and enabled?)");
                if (_modLoader.GetAppConfig().EnabledMods.Contains(modId)) { throw new FileNotFoundException($"Missing config file for {modId}"); }
                else { return 0; }
            }
        }

        return value!;
    }

    private void RegisterMiscFunctions()
    {
        _flowFramework.Register("CMM_GET_IN_USE_ID", 1, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var inputId = flowApi.GetIntArg(0);

            var idPair = confidantIds.Exists(x => x.Item1 == inputId || x.Item2 == inputId) ? confidantIds.Find(x => x.Item1 == inputId || x.Item2 == inputId) : (inputId, inputId);

            if (idPair.Item1 == 33 || idPair.Item1 == 36) // hardcoded sumire check bc both halves are 2 ids each
            {
                for (int i = 37; i > 32; i--)
                {
                    if (i == 35) { continue; } // bypass maruki id
                    if (_flowCaller.CMM_EXIST(i) == 1)
                    {
                        flowApi.SetReturnValue(i);
                        break;
                    }
                    if (i == 33) { flowApi.SetReturnValue(-1); }
                }
            }
            else if (idPair.Item1 != idPair.Item2 && _flowCaller.CMM_EXIST(idPair.Item2) == 1) { flowApi.SetReturnValue(idPair.Item2); }
            else { flowApi.SetReturnValue(_flowCaller.CMM_EXIST(idPair.Item1) == 1 ? idPair.Item1 : -1); }

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SUM_ITEM_ID", 2, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();

            var section = flowApi.GetIntArg(0) * 0x1000;
            var idInSection = flowApi.GetIntArg(1);
            flowApi.SetReturnValue(section + idInSection);

            return FlowStatus.SUCCESS;
        });
    }

    private bool TryGetR2ConfigValue(string modId, string configId, string configPath, out object? configValue, bool isFloat = false)
    {
        configValue = null;
        _logger.WriteLog(LogLevel.INFO, $"Attempting to read {(isFloat ? "float" : "int")} value from R2 config");

        try
        {
            using (JsonDocument r2Config = JsonDocument.Parse(File.ReadAllText(configPath)))
            {
                var configItem = r2Config.RootElement.GetProperty(configId);
                _logger.WriteLog(LogLevel.DEBUG, $"{configId} in {modId} is of type {configItem.ValueKind}");

                switch (configItem.ValueKind)
                {
                    case JsonValueKind.True:
                        configValue = 1;
                        break;
                    case JsonValueKind.False:
                        configValue = 0;
                        break;
                    case JsonValueKind.Number:
                        if (isFloat && configItem.TryGetDouble(out var floatVal)) { configValue = floatVal; }
                        else if (configItem.TryGetInt32(out var intVal)) { configValue = intVal; }
                        else
                        {
                            _logger.WriteLog(LogLevel.ERROR, $"Failed to parse config value {configId} in {modId}.");
                            return false;
                        }
                        break;
                    default:
                        _logger.WriteLog(LogLevel.ERROR, $"Config value {configId} in {modId} is unsupported type: {configItem.ValueKind.ToString()}.");
                        return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.WriteLog(LogLevel.ERROR, ex.ToString());
            return false;
        }
    }

    private bool TryGetRemixConfigValue(string modId, string configId, string configPath, string schemaPath, out object? configValue, bool isFloat = false)
    {
        configValue = null;
        _logger.WriteLog(LogLevel.INFO, $"Attempting to read {(isFloat ? "float" : "int")} value from ReMIX config");

        try
        {
            var yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

            _logger.WriteLog(LogLevel.DEBUG, $"Deserializing config file for {modId}");
            var remixConfig = yamlDeserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(configPath));
            if (remixConfig == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to deserialize ReMIX config for {modId}.");
                return false;
            }

            var schemaParser = new Parser(new StringReader(schemaPath));
            schemaParser.Consume<StreamStart>();
            schemaParser.Consume<DocumentStart>();
            schemaParser.Consume<MappingStart>();

            ConfigSetting? configSchema = null;
            while (schemaParser.TryConsume<Scalar>(out var section))
            {
                if (section.Value == "settings")
                {
                    _logger.WriteLog(LogLevel.DEBUG, $"Deserializing schema settings for {modId}");
                    configSchema = yamlDeserializer.Deserialize<ConfigSetting[]>(schemaParser).Where(x => x.Id == configId).First();
                    break;
                }
                else
                {
                    _logger.WriteLog(LogLevel.DEBUG, $"Skipping schema {section.Value} section...");
                    schemaParser.SkipThisAndNestedEvents();
                }
            }

            if (configSchema == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to deserialize ReMIX schema for {modId}.");
                return false;
            }

            if (remixConfig.TryGetValue(configSchema.Id, out configValue))
            {
                if (configValue == null)
                {
                    _logger.WriteLog(LogLevel.WARNING, $"Config value {configSchema.Id} for {modId} returned null. Returning default value...");
                    configValue = configSchema.GetDefaultValue();
                }

                var configType = configSchema.GetPropertyType();
                _logger.WriteLog(LogLevel.DEBUG, $"{configId} in {modId} is of type {nameof(configType)}");

                if (configType == typeof(string))
                {
                    _logger.WriteLog(LogLevel.ERROR, $"Config value {configId} in {modId} is unsupported type: {configType.ToString()}.");
                    return false;
                }
                else if (isFloat)
                {
                    if (configType == typeof(double)) { _logger.WriteLog(LogLevel.WARNING, $"Attempted to read double value {configId} in {modId}."); }
                    configValue = Convert.ChangeType(configValue, configType);
                }
                else
                {
                    configValue = configType.IsEnum ? Convert.ToInt32(configValue) : Convert.ChangeType(configValue, configType);
                    if (configType == typeof(bool)) { configValue = (bool)configValue! ? 1 : 0; }
                }

                _logger.WriteLog(LogLevel.DEBUG, $"Config value {configId} in {modId} is equal to {configValue!.ToString()}");
                return true;
            }
            else
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get config value {configId} in {modId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.WriteLog(LogLevel.ERROR, ex.ToString());
            return false;
        }
    }
}
