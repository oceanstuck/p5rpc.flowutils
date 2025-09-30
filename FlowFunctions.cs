using p5rpc.CustomSaveDataFramework.Interfaces;
using p5rpc.CustomSaveDataFramework.Nodes;
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

    // private ICustomSaveDataFramework? _customSaveDataFramework;

    public FlowFunctions(IFlowFramework flowFramework, IModLoader modLoader, ref Logger logger, IFlowCaller flowCaller/*, ref ICustomSaveDataFramework? customSaveDataFramework*/)
    {
        _flowFramework = flowFramework;
        _logger = logger;
        _modLoader = modLoader;
        _flowCaller = flowCaller;
        // _customSaveDataFramework = customSaveDataFramework;
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

        _flowFramework.Register("CMM_GET_IN_USE_ID", 1, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var inputId = flowApi.GetIntArg(0);

            var idPair = confidantIds.Exists(x => x.Item1 == inputId || x.Item2 == inputId) ? confidantIds.Find(x => x.Item1 == inputId || x.Item2 == inputId) : (inputId, inputId);

            if (idPair.Item1 == 33 || idPair.Item1 == 36) // hardcoded sumire check bc both halves are 2 ids each (34 should never be used though...)
            {
                for (int i = 37; i > 32; i--)
                {
                    if (i == 35 || i == 34) { i = 33; } // bypass maruki id and unused "kasumi" altid
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
    }

    /*
    public void RegisterCustomSaveDataHandlers()
    {
        if (_customSaveDataFramework == null) { return; }

        _flowFramework.Register("GET_CUSTOM_SAVE_DATA_INT", 2, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            if (!TryGetCustomSaveDataValue(modId, key, out var value))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new Exception($"Failed to read custom save item {key} from {modId}.");
            }
            flowApi.SetReturnValue((int)value);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_INT", 3, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();

            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetIntArg(2);

            var operationSucceeded = TrySetCustomSaveDataValue(modId, key, value) ? 1 : 0;
            if (operationSucceeded == 0) { _logger.WriteLog(LogLevel.ERROR, $"Failed to set custom save iten {key} in {modId}. Value: {value.ToString()}"); }
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CUSTOM_SAVE_DATA_FLOAT", 2, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            if (!TryGetCustomSaveDataValue(modId, key, out var value, true))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new Exception($"Failed to read custom save item {key} from {modId}.");
            }
            flowApi.SetReturnValue((float)value);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_FLOAT", 3, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();

            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetFloatArg(2);

            var operationSucceeded = TrySetCustomSaveDataValue(modId, key, value) ? 1 : 0;
            if (operationSucceeded == 0) { _logger.WriteLog(LogLevel.ERROR, $"Failed to set custom save iten {key} in {modId}. Value: {value.ToString()}"); }
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_STRING", 3, () =>
        {
            var flowApi = _flowFramework.GetFlowApi();

            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetStringArg(2);

            var operationSucceeded = TrySetCustomSaveDataValue(modId, key, value) ? 1 : 0;
            if (operationSucceeded == 0) { _logger.WriteLog(LogLevel.ERROR, $"Failed to set custom save iten {key} in {modId}. Value: {value.ToString()}"); }
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });
    }

    private bool TryGetCustomSaveDataValue(string modId, string key, out object? value, bool isFloat = false)
    {
        value = null;

        if (_customSaveDataFramework.TryGetEntry(modId, key, out var entry))
        {
            if (entry is SavedString)
            {
                _logger.WriteLog(LogLevel.ERROR, "Attempted to read custom save string value.");
                return false;
            }
            if (entry is SavedDouble) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save double value."); }
            if (!isFloat && (entry is SavedFloat || entry is SavedDouble)) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save float/double value as int."); }
            if (entry is SavedLong) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save long value."); }

            value = isFloat ? ((SavedFloat)entry).value : ((SavedInt)entry).value;
            return true;
        }
        else { return false; }
    }

    private bool TrySetCustomSaveDataValue(string modId, string key, object value)
    {
        if (_customSaveDataFramework.TryGetEntry(modId, key, out var entry))
        {
            switch (entry.GetType().ToString())
            {
                case nameof(SavedString):
                    ((SavedString)entry).value = (string)value;
                    return true;
                case nameof(SavedDouble):
                    ((SavedDouble)entry).value = (double)value;
                    return true;
                case nameof(SavedFloat):
                    ((SavedFloat)entry).value = (float)value;
                    return true;
                case nameof(SavedLong):
                    ((SavedLong)entry).value = (long)value;
                    return true;
                case nameof(SavedInt):
                    ((SavedInt)entry).value = (int)value;
                    return true;
                case nameof(SavedShort):
                    ((SavedShort)entry).value = (short)value;
                    return true;
                case nameof(SavedByte):
                    ((SavedByte)entry).value = (byte)value;
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
    */

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
                if (configType == typeof(double)) { _logger.WriteLog(LogLevel.WARNING, $"Attempted to read double value {configId} in {modId}."); }
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
