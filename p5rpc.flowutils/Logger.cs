using Reloaded.Mod.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p5rpc.flowutils.logging;

internal class Logger
{
    private ILogger _logger;

    public Logger(ILogger logger, LogLevel level)
    {
        _logger = logger;
        _level = level;
    }

    private LogLevel _level;

    public void WriteLog(LogLevel lvl, string msg)
    {
        var color = new System.Drawing.Color();
        
        switch (lvl)
        {
            case LogLevel.DEBUG:
                color = System.Drawing.Color.Gray;
                break;
            case LogLevel.INFO:
                color = System.Drawing.Color.White;
                break;
            case LogLevel.WARNING:
                color = System.Drawing.Color.Yellow;
                break;
            case LogLevel.ERROR:
                color = System.Drawing.Color.Red;
                break;
            case LogLevel.FATAL:
                color = System.Drawing.Color.Purple;
                break;
        }

        if (_level <= lvl) { _logger.WriteLine($"[Flow Utils][{lvl.ToString()}] {msg}", color); }
    }
}

public enum LogLevel
{
    DEBUG,
    INFO,
    WARNING,
    ERROR,
    FATAL
}
