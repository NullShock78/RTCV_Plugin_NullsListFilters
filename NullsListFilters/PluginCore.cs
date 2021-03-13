using RTCV.Common;
using RTCV.PluginHost;
using RTCV.UI;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Diagnostics;

namespace NullsListFilters
{
    [Export(typeof(IPlugin))]
    public class PluginCore : IPlugin, IDisposable
    {
        public static RTCSide CurrentSide = RTCSide.Both;

        public string Name => "NullsListFilters";
        public string Description => "A list filter pack";

        public string Author => "NullShock78";

        public Version Version => new Version(1, 0, 0);

        //Must be loaded on both sides
        public RTCSide SupportedSide => RTCSide.Both;

        public void Dispose()
        {
        }

        public bool Start(RTCSide side)
        {
            CurrentSide = side;
            return true;
        }

        public bool StopPlugin()
        {
            return true;
        }
    }
}
