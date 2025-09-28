using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.logging;
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

    private Dictionary<int, int> confidantIds; 

    public FlowFunctions(IFlowFramework flowFramework, IModLoader modLoader, ref Logger logger)
    {
        _flowFramework = flowFramework;
        _logger = logger;
        _modLoader = modLoader;
    }

    public void RegisterConfigReaders()
    { 
        _flowFramework.Register("IS_MOD_ENABLED", 1, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            var isEnabled = _modLoader.GetAppConfig().EnabledMods.Contains(modId) ? 1 : 0;
            flowApi.SetReturnValue(isEnabled);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CONFIG_INT_VALUE", 2, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            var configId = flowApi.GetStringArg(1);

            object? value = null;
            if (!TryGetR2ConfigValue(modId, configId, out value) && TryGetRemixConfigValue(modId, configId, out value))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to read config file for {modId}.");
                throw new Exception($"Failed to read config file for {modId}.");
            }
            flowApi.SetReturnValue((int)value);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CONFIG_FLOAT_VALUE", 2, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            var configId = flowApi.GetStringArg(1);

            object? value = null;
            if (!TryGetR2ConfigValue(modId, configId, out value, true) && TryGetRemixConfigValue(modId, configId, out value, true))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to read config file for {modId}.");
                throw new Exception($"Failed to read config file for {modId}.");
            }
            flowApi.SetReturnValue((float)value);

            return FlowStatus.SUCCESS;

        });
    }

    public void RegisterMiscFunctions()
    {
        confidantIds = new Dictionary<int, int>()
        {
            [11] = 27,
            [14] = 28,
            [15] = 29,
            [16] = 30,
            [18] = 31,
            [33] = 34
        };

        _flowFramework.Register("CMM_GET_IN_USE_ID", 1, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var id = flowApi.GetIntArg(0);


        });
    }

    public void RegisterCustomSaveDataHandlers()
    {

    }

    private bool TryGetR2ConfigValue(string modId, string configId, out object? configValue, bool isFloat = false)
    {
        configValue = null;
        var configPath = Path.Combine(_modLoader.GetModConfigDirectory(modId), "Config.json");
        if (!File.Exists(configPath))
        {
            _logger.WriteLog(LogLevel.INFO, $"Couldn't find R2 config file for {modId}.");
            return false;
        }

        using (JsonDocument r2Config = JsonDocument.Parse(File.ReadAllText(configPath)))
        {
            var configItem = r2Config.RootElement.GetProperty(configId);
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
                        throw new ArgumentException($"Failed to parse config value {configId} in {modId}.");
                    }
                    break;
                default:
                    _logger.WriteLog(LogLevel.ERROR, $"Config value {configId} in {modId} is unsupported type: {configItem.ValueKind.ToString()}.");
                    throw new ArgumentException($"Config value {configId} in {modId} is unsupported type: {configItem.ValueKind.ToString()}.");
            }
        }
        return true;
    }

    private bool TryGetRemixConfigValue(string modId, string configId, out object? configValue, bool isFloat = false)
    {
        configValue = null;

        var configPath = Path.Combine(_modLoader.GetModConfigDirectory(modId), "ReMIX", "Config", "data.yaml");
        string schemaPath = Path.Combine(_modLoader.GetDirectoryForModId(modId), "ReMIX", "Config", "config.yaml");
        if (!File.Exists(schemaPath))
        {
            _logger.WriteLog(LogLevel.ERROR, $"Couldn't find ReMIX schema file for {modId}.");
            return false;
        }
        if (!File.Exists(configPath))
        {
            _logger.WriteLog(LogLevel.ERROR, $"Couldn't find ReMIX config file for {modId}.");
            return false;
        }

        var yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

        var remixConfig = yamlDeserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(configPath));
        if (remixConfig == null)
        {
            _logger.WriteLog(LogLevel.ERROR, $"Failed to deserialize ReMIX config for {modId}.");
            throw new Exception($"Failed to deserialize ReMIX config for {modId}.");
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
                configSchema = yamlDeserializer.Deserialize<ConfigSetting[]>(schemaParser).Where(x => x.Id == configId).First();
                break;
            }
            else { schemaParser.SkipThisAndNestedEvents(); }
        }

        if (configSchema == null)
        {
            _logger.WriteLog(LogLevel.ERROR, $"Failed to deserialize ReMIX schema for {modId}.");
            throw new Exception($"Failed to deserialize ReMIX schema for {modId}.");
        }

        if (remixConfig.TryGetValue(configSchema.Id, out configValue))
        {
            if (configValue == null) { configValue = configSchema.GetDefaultValue(); }

            var configType = configSchema.GetPropertyType();
            if (configType == typeof(string))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Config value {configId} in {modId} is unsupported type: {configType.ToString()}.");
                throw new ArgumentException($"Config value {configId} in {modId} is unsupported type: {configType.ToString()}.");
            }
            else if (isFloat)
            {
                if (configType == typeof(double)) { _logger.WriteLog(LogLevel.WARNING, $"{configId} in {modId} is of type double. Some precision may be lost."); }
                configValue = Convert.ChangeType(configValue, configType);
            }
            else
            {
                configValue = configType.IsEnum ? Convert.ToInt32(configValue) : Convert.ChangeType(configValue, configType);
                if (configType == typeof(bool)) { configValue = (bool)configValue ? 1 : 0; }
            }
        }
        return true;
    }
}
