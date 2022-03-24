using Ceras;
using RTCV.CorruptCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullsListFilters
{
    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    public class FloatTiltEntry
    {
        public bool IsLimiter { get; private set; } = false;
        public bool IsRange { get; private set; } = false;
        public float min;
        public float max;
        public FloatTiltType tiltType;

        string origLine;
        //Needed for ceras
        public FloatTiltEntry() { }

        public FloatTiltEntry(FloatTiltType type, string origLine, float min, float max, bool range = true)
        {
            this.origLine = origLine;
            this.tiltType = type;
            this.min = min;
            this.max = max;
            this.IsRange = range;
            this.IsLimiter = type == FloatTiltType.LIM;
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

        public float Modify(float val)
        {
            float tAmt = (IsRange ? ((float)RtcCore.RND.NextDouble() * (max - min) + min) : min);
            switch (tiltType)
            {
                case FloatTiltType.ADD:
                    return val + tAmt;
                case FloatTiltType.SUB:
                    return val - tAmt;
                case FloatTiltType.MUL:
                    return val * tAmt;
                case FloatTiltType.DIV:
                    if (tAmt == 0) return val;
                    return val / tAmt;
                case FloatTiltType.SET:
                    return tAmt;
                case FloatTiltType.ABS:
                    return Math.Abs(val);
                case FloatTiltType.NEG:
                    return -Math.Abs(val);
                case FloatTiltType.SQRT:
                    return (float)Math.Sqrt(val);
                case FloatTiltType.RND:
                    return (float)Math.Ceiling(val);
                case FloatTiltType.SIN:
                    return (float)Math.Sin(val);
                case FloatTiltType.COS:
                    return (float)Math.Cos(val);
                case FloatTiltType.TAN:
                    return (float)Math.Tan(val);
                case FloatTiltType.LOG:
                    if (val <= 0) return val;
                    return (float)Math.Log(val);
                case FloatTiltType.LOG2:
                    if (val <= 0 || tAmt <= 0) return val;
                    return (float)Math.Log(val) + (float)Math.Log(tAmt);
                case FloatTiltType.SIN2:
                    return (float)Math.Sin(val) + (float)Math.Sin(tAmt);
                case FloatTiltType.COS2:
                    return (float)Math.Cos(val) + (float)Math.Cos(tAmt);
                case FloatTiltType.TAN2:
                    return (float)Math.Tan(val) + (float)Math.Tan(tAmt);
                case FloatTiltType.POW:
                    return (float)Math.Pow(val, tAmt);
                default:
                    return val;
            }
        }

        public override string ToString()
        {
            return origLine;
        }

    }
}
