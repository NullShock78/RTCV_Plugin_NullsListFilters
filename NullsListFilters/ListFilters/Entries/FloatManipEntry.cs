using Ceras;
using RTCV.CorruptCore;
using System;

namespace NullsListFilters.ListFilters.Entries
{
    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    public class FloatManipEntry
    {
        public bool IsLimiter { get; private set; } = false;
        public bool IsRange { get; private set; } = false;
        public float min;
        public float max;
        public TiltType tiltType;

        string origLine;
        //Needed for ceras
        public FloatManipEntry() { }

        public FloatManipEntry(TiltType type, string origLine, float min, float max, bool range = true)
        {
            this.origLine = origLine;
            this.tiltType = type;
            this.min = min;
            this.max = max;
            this.IsRange = range;
            this.IsLimiter = type == TiltType.LIM;
        }

        public bool LimitValue(float val)
        {
            if (IsRange)
            {
                return !float.IsNaN(val) && !float.IsInfinity(val) && val >= min && val <= max;
            }
            else
            {
                return !float.IsNaN(val) && !float.IsInfinity(val) && val == min;
            }
        }

        public float Tilt(float val)
        {
            float tAmt = (IsRange ? ((float)RtcCore.RND.NextDouble() * (max - min) + min) : min);
            switch (tiltType)
            {
                case TiltType.ADD:
                    return val + tAmt;
                case TiltType.SUB:
                    return val - tAmt;
                case TiltType.MUL:
                    return val * tAmt;
                case TiltType.DIV:
                    if (tAmt == 0) return val;
                    return val / tAmt;
                case TiltType.SET:
                    return tAmt;
                default:
                    return val;
            }
        }
    }
}
