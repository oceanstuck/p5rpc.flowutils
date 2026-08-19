using p5rpc.CustomSaveDataFramework.Interfaces;
using p5rpc.CustomSaveDataFramework.Nodes;
using p5rpc.flowscriptframework.interfaces;
using p5rpc.flowutils.customsavedata.logging;
using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnusedDataPolicy = p5rpc.CustomSaveDataFramework.Nodes.Node.UnusedDataPolicy;

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

        var flowApi = _flowFramework.GetFlowApi();

        _flowFramework.Register("CUSTOM_SAVE_ENTRY_EXISTS", 2, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            _logger.WriteLog(LogLevel.DEBUG, $"Calling CUSTOM_SAVE_ENTRY_EXISTS with mod id {modId} and key {key}.");
            flowApi.SetReturnValue(_customSaveDataFramework.TryGetEntry(modId, key, out var value) ? 1 : 0);
            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("CUSTOM_SAVE_MOD_KEY_EXISTS", 1, () =>
        {
            var modId = flowApi.GetStringArg(0);
            _logger.WriteLog(LogLevel.DEBUG, $"Calling CUSTOM_SAVE_MOD_KEY_EXISTS with mod id {modId}.");
            flowApi.SetReturnValue(_customSaveDataFramework.ContainsModKey(modId) ? 1 : 0);
            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CUSTOM_SAVE_DATA_INT", 2, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            _logger.WriteLog(LogLevel.DEBUG, $"Calling GET_CUSTOM_SAVE_DATA_INT with mod id {modId} and key {key}.");
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new ArgumentException($"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }

            _logger.WriteLog(LogLevel.DEBUG, $"{modId}.{key} is of type {node.GetType()}");

            int value;
            if (node is SavedInt) { value = ((SavedInt)node!).value; }
            else if (node is SavedLong)
            {
                var unsizedValue = ((SavedLong)node!).value;
                try
                {
                    value = (int)unsizedValue;
                }
                catch (OverflowException overflow)
                {
                    _logger.WriteLog(LogLevel.ERROR, $"{unsizedValue} was outside the range of type int");
                    throw new OverflowException(overflow.Message);
                }
            }
            else if (node is SavedByte) { value = ((SavedByte)node!).value; }
            else if (node is SavedShort) { value = ((SavedShort)node!).value; }
            else
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not integral saved type");
                throw new ArgumentException($"{modId}.{key} was not integral saved type");
            }
            flowApi.SetReturnValue(value);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_INT", 3, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetIntArg(2);

            _logger.WriteLog(LogLevel.DEBUG, $"Setting {modId}.{key} to {value} via SET_CUSTOM_SAVE_DATA_INT");
            int operationSucceeded = 0;
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new ArgumentException($"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }

            _logger.WriteLog(LogLevel.DEBUG, $"{modId}.{key} is of type {node.GetType()}");
            if (node is SavedInt) { ((SavedInt)node!).value = value; }
            else if (node is SavedLong) { ((SavedLong)node!).value = value; }
            else if (node is SavedByte)
            {
                try
                {
                    byte resizedValue = (byte)value;
                    ((SavedByte)node!).value = resizedValue;
                }
                catch (OverflowException overflow)
                {
                    _logger.WriteLog(LogLevel.ERROR, $"{value} was outside the range of type SavedByte");
                    throw new OverflowException(overflow.Message);
                }
            }
            else if (node is SavedShort)
            {
                try
                {
                    short resizedValue = (short)value;
                    ((SavedShort)node!).value = resizedValue;
                }
                catch (OverflowException overflow)
                {
                    _logger.WriteLog(LogLevel.ERROR, $"{value} was outside the range of type SavedByte");
                    throw new OverflowException(overflow.Message);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not integral saved type");
                throw new ArgumentException($"{modId}.{key} was not integral saved type");
            }

            operationSucceeded = 1;
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("GET_CUSTOM_SAVE_DATA_FLOAT", 2, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            _logger.WriteLog(LogLevel.DEBUG, $"Calling GET_CUSTOM_SAVE_DATA_FLOAT with mod id {modId} and key {key}.");
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new ArgumentException($"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }

            _logger.WriteLog(LogLevel.DEBUG, $"{modId}.{key} is of type {node.GetType()}");

            float value;
            if (node is SavedFloat) { value = ((SavedFloat)node!).value; }
            else if (node is SavedDouble)
            {
                var unsizedValue = ((SavedDouble)node!).value;
                try
                {
                    value = (float)unsizedValue;
                }
                catch (OverflowException overflow)
                {
                    _logger.WriteLog(LogLevel.ERROR, $"{unsizedValue} was outside the range of type float");
                    throw new OverflowException(overflow.Message);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not type SavedFloat nor SavedDouble");
                throw new ArgumentException($"{modId}.{key} was not type SavedFloat nor SavedDouble");
            }
            flowApi.SetReturnValue(value);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_FLOAT", 3, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetFloatArg(2);

            _logger.WriteLog(LogLevel.DEBUG, $"Setting {modId}.{key} to {value} via SET_CUSTOM_SAVE_DATA_FLOAT");
            int operationSucceeded = 0;
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new ArgumentException($"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }

            _logger.WriteLog(LogLevel.DEBUG, $"{modId}.{key} is of type {node.GetType()}");
            if (node is SavedFloat) { ((SavedFloat)node!).value = value; }
            else if (node is SavedDouble) { ((SavedDouble)node!).value = value; }
            else
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not type SavedFloat nor SavedDouble");
                throw new ArgumentException($"{modId}.{key} was not type SavedFloat nor SavedDouble");
            }

            operationSucceeded = 1;
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("PRINT_CUSTOM_SAVE_DATA_STRING", 2, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);

            _logger.WriteLog(LogLevel.DEBUG, $"Calling PRINT_CUSTOM_SAVE_DATA_STRING with mod id {modId} and key {key}.");
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
                throw new ArgumentException($"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }
            if (node is not SavedString)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not type SavedString");
                throw new ArgumentException($"{modId}.{key} was not type SavedString");
            }
            var value = ((SavedString)node!).value;
            _logger._logger.WriteLine(value!, System.Drawing.Color.White);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("SET_CUSTOM_SAVE_DATA_STRING", 3, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetStringArg(2);

            _logger.WriteLog(LogLevel.DEBUG, $"Setting {modId}.{key} to {value} via SET_CUSTOM_SAVE_DATA_STRING");
            int operationSucceeded = 0;
            if (!_customSaveDataFramework.TryGetEntry(modId, key, out var node))
            {
                _logger.WriteLog(LogLevel.ERROR, $"Failed to get custom save item {key} from {modId}.");
            }
            if (node == null)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was null");
                throw new NullReferenceException($"{modId}.{key} was null");
            }
            if (node is not SavedString)
            {
                _logger.WriteLog(LogLevel.ERROR, $"{modId}.{key} was not type SavedString");
                throw new ArgumentException($"{modId}.{key} was not type SavedString");
            }
            ((SavedString)node!).value = value;

            operationSucceeded = 1;
            flowApi.SetReturnValue(operationSucceeded);

            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("CREATE_CUSTOM_SAVE_DATA_INT", 4, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetIntArg(2);
            UnusedDataPolicy unusedDataPolicy = flowApi.GetIntArg(3) == 1 ? UnusedDataPolicy.Discard : UnusedDataPolicy.Keep;

            _logger.WriteLog(LogLevel.DEBUG, $"Creating {modId}.{key} via CREATE_CUSTOM_SAVE_DATA_INT with value {value} and {unusedDataPolicy} policy.");
            _customSaveDataFramework.AddEntry(modId, key, new SavedInt(value, unusedDataPolicy));
            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("CREATE_CUSTOM_SAVE_DATA_FLOAT", 4, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetFloatArg(2);
            UnusedDataPolicy unusedDataPolicy = flowApi.GetIntArg(3) == 1 ? UnusedDataPolicy.Discard : UnusedDataPolicy.Keep;

            _logger.WriteLog(LogLevel.DEBUG, $"Creating {modId}.{key} via CREATE_CUSTOM_SAVE_DATA_FLOAT with value {value} and {unusedDataPolicy} policy.");
            _customSaveDataFramework.AddEntry(modId, key, new SavedFloat(value, unusedDataPolicy));
            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("CREATE_CUSTOM_SAVE_DATA_STRING", 4, () =>
        {
            var modId = flowApi.GetStringArg(0);
            var key = flowApi.GetStringArg(1);
            var value = flowApi.GetStringArg(2);
            UnusedDataPolicy unusedDataPolicy = flowApi.GetIntArg(3) == 1 ? UnusedDataPolicy.Discard : UnusedDataPolicy.Keep;

            _logger.WriteLog(LogLevel.DEBUG, $"Creating {modId}.{key} via CREATE_CUSTOM_SAVE_DATA_STRING with value {value} and {unusedDataPolicy} policy.");
            _customSaveDataFramework.AddEntry(modId, key, new SavedString(value, unusedDataPolicy));
            return FlowStatus.SUCCESS;
        });

        _flowFramework.Register("TRY_REMOVE_CUSTOM_SAVE_ENTRY", 2, () =>
        {
            string modId = flowApi.GetStringArg(0);
            string key = flowApi.GetStringArg(1);

            _logger.WriteLog(LogLevel.DEBUG, $"Calling TRY_REMOVE_CUSTOM_SAVE_ENTRY with mod id {modId} and key {key}.");
            flowApi.SetReturnValue(_customSaveDataFramework.RemoveEntry(modId, key) ? 1 : 0);
            return FlowStatus.SUCCESS;
        });
    }

    /*private bool TryGetCustomSaveDataValue(string modId, string key, out object? value, Type expectedType)
    {
        _logger.WriteLog(LogLevel.DEBUG, $"Attempting to read value {modId}.{key}");
        value = null;

        var entryFound = _customSaveDataFramework.TryGetEntry(modId, key, out value);
        if (value == null)
        {
            _logger.WriteLog(LogLevel.WARNING, $"Entry {modId}.{key} was null");
            return false;
        }

        if (entryFound)
        {
            Type entryType = value!.GetType();
            _logger.WriteLog(LogLevel.DEBUG, $"{modId}.{key} is of type {entryType}");

            if (value is SavedString && expectedType != typeof(string))
            {
                _logger.WriteLog(LogLevel.ERROR, "Unexpected string value.");
                return false;
            }
            if (value is SavedDouble) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save double value."); }
            if (expectedType != typeof(float) && (value is SavedFloat || value is SavedDouble)) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save float/double value as non-float."); }
            if (value is SavedLong) { _logger.WriteLog(LogLevel.WARNING, "Attempted to read custom save long value."); }

            _logger.WriteLog(LogLevel.DEBUG, "Attempting to read value...");

            value = entryType.GetField("value")!.GetValue(value);
            if (value == null)
            {
                _logger.WriteLog(LogLevel.ERROR, "Value returned null");
                return false;
            }

            return true;
        }
        else
        {
            _logger.WriteLog(LogLevel.ERROR, $"Failed to find entry {modId}.{key}");
            return false;
        }
    }

    private bool TrySetCustomSaveDataValue(string modId, string key, object value)
    {
        _logger.WriteLog(LogLevel.DEBUG, $"Attempting to write value {value} to {modId}.{key}");

        if (!_modLoader.GetAppConfig().EnabledMods.Contains(modId))
        {
            _logger.WriteLog(LogLevel.ERROR, $"{modId} is not enabled, aborting...");
            return false;
        }

        var entryFound = _customSaveDataFramework.TryGetEntry(modId, key, out var entry);
        if (entry == null)
        {
            _logger.WriteLog(LogLevel.WARNING, $"Entry {modId}.{key} was null");
            return false;
        }

        if (entryFound)
        {
            Type entryType = entry!.GetType();
            _logger.WriteLog(LogLevel.DEBUG, $"Entry {modId}.{key} is of type {entryType}");

            var fieldInfo = entryType.GetField("value");
            fieldInfo!.SetValue(entry, Convert.ChangeType(value, fieldInfo.FieldType)); // explicit conversion to avoid trying to implicit cast int to byte/short
            Debug.Assert(fieldInfo.GetValue(entry) == Convert.ChangeType(value, fieldInfo.FieldType));

            return true;
        }
        else
        {
            _logger.WriteLog(LogLevel.ERROR, $"Failed to find entry {modId}.{key}");
            return false;
        }
    }*/
}
