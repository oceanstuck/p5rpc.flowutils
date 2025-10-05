using p5rpc.CustomSaveDataFramework.Interfaces;
using p5rpc.CustomSaveDataFramework.Nodes;
using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.logging;
using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.flowutils.customsavedata;

internal class FlowFunctions
{
    private IFlowFramework _flowFramework;
    private Logger _logger;
    private IModLoader _modLoader;
    private ICustomSaveDataFramework _customSaveDataFramework;

    public FlowFunctions(IFlowFramework flowFramework, Logger logger, IModLoader modLoader, ICustomSaveDataFramework customSaveDataFramework)
    {
        _flowFramework = flowFramework;
        _logger = logger;
        _modLoader = modLoader;
        _customSaveDataFramework = customSaveDataFramework;
    }

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
            }
            flowApi.SetReturnValue((int)value!);

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
            }
            flowApi.SetReturnValue((float)value!);

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

            value = isFloat ? ((SavedFloat)entry!).value : ((SavedInt)entry!).value;
            return true;
        }
        else { return false; }
    }

    private bool TrySetCustomSaveDataValue(string modId, string key, object value)
    {
        if (!_modLoader.GetAppConfig().EnabledMods.Contains(modId))
        {
            _logger.WriteLog(LogLevel.ERROR, $"{modId} is not enabled, aborting...");
            return false;
        }

        if (_customSaveDataFramework.TryGetEntry(modId, key, out var entry))
        {
            switch (entry!.GetType().ToString())
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
}
