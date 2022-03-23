using Ceras;
using RTCV.CorruptCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//Made by NullShock78

namespace NullsListFilters
{
    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    [Export(typeof(IListFilter))]
    public class FloatTiltListFilter : IListFilter
    {
        List<TiltEntry> entries = new List<TiltEntry>();
        int repeatAmt = 0;
        static Regex entryRegex = new Regex(@"(^|(?<type>\((ADD|SUB|DIV|MUL|SET|ABS|NEG|SQRT|RND|SIN|SINADD|COS|COSADD|LOG|LOGADD|TAN|TANADD|POW)\))) *((?<range>([\-\d\.]+)->([\-\d\.]+))|(?<single>([\-\d\.]+)))");

        //Needed because apparently float.parse works differently in different OS languages
        static CultureInfo culture = new CultureInfo("en-US");
        public string Initialize(string filePath, string[] dataLines, bool flipBytes, bool syncListViaNetcore)
        {
            //Ignore flipbytes, as this list works differently
            int line = 2;
            foreach (string s in dataLines)
            {
                if (s.StartsWith("@"))
                {
                    if (s.Substring(1).StartsWith("REPEAT_"))
                        repeatAmt = Convert.ToInt32(s.Substring(8));
                    if (repeatAmt < 0)
                    {
                        repeatAmt = 0;
                    }
                }
                string workingString = s.Replace("f","").Replace("F", "").Replace(" ", "").ToUpper();                
                string origLine = workingString;

                var m = entryRegex.Match(workingString);
                if(m == null || !m.Success)
                {
                    throw new Exception($"Error loading {filePath}: Unable to parse line {line}, parse failed");
                }
                TiltType type = TiltType.ADD;

                try
                {
                    //Type
                    if (m.Groups["type"].Success)
                    {
                        switch (m.Groups[2].Value)
                        {
                            case "ADD":
                                break;
                            case "SUB":
                                type = TiltType.SUB;
                                break;
                            case "MUL":
                                type = TiltType.MUL;
                                break;
                            case "DIV":
                                type = TiltType.DIV;
                                break;
                            case "SET":
                                type = TiltType.SET;
                                break;
                            case "ABS":
                                type = TiltType.ABS;
                                break;
                            case "NEG":
                                type = TiltType.NEG;
                                break;
                            case "SQRT":
                                type = TiltType.SQRT;
                                break;
                            case "RND":
                                type = TiltType.RND;
                                break;
                            case "SIN":
                                type = TiltType.SIN;
                                break;
                            case "COS":
                                type = TiltType.COS;
                                break;
                            case "TAN":
                                type = TiltType.TAN;
                                break;
                            case "LOG":
                                type = TiltType.LOG;
                                break;
                            case "LOGADD":
                                type = TiltType.LOG2;
                                break;
                            case "SINADD":
                                type = TiltType.SIN2;
                                break;
                            case "COSADD":
                                type = TiltType.COS2;
                                break;
                            case "TANADD":
                                type = TiltType.TAN2;
                                break;
                            case "POW":
                                type = TiltType.POW;
                                break;
                            default:
                                break;
                        }
                    }

                    bool range = false;
                    float min = 0;
                    float max = 0;

                    if (m.Groups["range"].Success)
                    {
                        range = true;
                        //6/7
                        min = float.Parse(m.Groups[4].Value, culture);
                        max = float.Parse(m.Groups[5].Value, culture);
                    }
                    else
                    {
                        //9
                        min = float.Parse(m.Groups[9].Value, culture);
                    }

                    TiltEntry entry = new TiltEntry(type, origLine, min, max, range);

                    entries.Add(entry);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error loading {filePath}: Unable to parse line {line}, float parse failed. Exception:\r\n{ex}");
                }
                line++;
            }

            if (entries.Count == 0)
            {
                throw new Exception($"Error loading {filePath}: List empty");
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            string hash = Filtering.RegisterList(this, name, syncListViaNetcore);
            return hash;
        }

        public bool ContainsValue(byte[] bytes)
        {
            //Not to be used as a limiter, will never match
            return false;
        }

        public string GetHash()
        {
            List<byte> bList = new List<byte>();
            byte[] prefix = new byte[] { (byte)'F', (byte)'l', (byte)'o', (byte)'a', (byte)'t', (byte)'T', (byte)'F' };
            foreach (var e in entries)
            {           
                bList.AddRange(prefix);
                bList.AddRange(BitConverter.GetBytes((int)e.tiltType));
                bList.AddRange(BitConverter.GetBytes(e.range));
                bList.AddRange(BitConverter.GetBytes(e.min));
                bList.AddRange(BitConverter.GetBytes(e.max));
            }
            MD5 hash = MD5.Create();
            hash.ComputeHash(bList.ToArray());
            string hashStr = Convert.ToBase64String(hash.Hash);
            return hashStr;
        }

        public int GetPrecision()
        {
            return sizeof(float);
        }

        public byte[] GetRandomValue(string hash, int precision, byte[] passthrough = null)
        {
            if (passthrough == null)
            {
                return new byte[precision]; //better than null I guess
            }

            if(precision != 4 || passthrough.Length != 4)
            {
                return passthrough;
            }
            var origValue = BitConverter.ToSingle(passthrough, 0);
            var ret = entries[RtcCore.RND.Next(entries.Count)].Tilt(origValue);
            if (repeatAmt != 0)
            {
                for (int i = 0; i < repeatAmt; i++)
                {
                    ret = entries[RtcCore.RND.Next(entries.Count)].Tilt(ret);
                }
            }
            return BitConverter.GetBytes(ret);
        }

        public List<string> GetStringList()
        {
            List<string> res = new List<string>();
            res.Add("@" + nameof(FloatTiltListFilter)); //Add top line to specify class for reflection
            foreach (var e in entries)
            {
                res.Add(e.ToString());
            }
            return res;
        }
    }

    [Serializable]
    public enum TiltType
    {
        ADD = 0,
        SUB = 1,
        MUL = 2,
        DIV = 3,
        SET = 4,
        ABS,
        NEG,
        SQRT,
        RND,
        SIN, SIN2,
        COS, COS2,
        LOG, LOG2,
        TAN, TAN2,
        POW
    }

    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    public class TiltEntry
    {
        public bool range = false;
        public float min;
        public float max;
        public TiltType tiltType;

        string origLine;
        //Needed for ceras
        public TiltEntry() { }

        public TiltEntry(TiltType type, string origLine, float min, float max, bool range = true)
        {
            this.origLine = origLine;
            this.tiltType = type;
            this.min = min;
            this.max = max;
            this.range = range;
        }

        public float Tilt(float val)
        {
            float tAmt = (range ? ((float)RtcCore.RND.NextDouble() * (max - min) + min) : min);
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
                case TiltType.ABS:
                    return Math.Abs(val);
                case TiltType.NEG:
                    return -Math.Abs(val);
                case TiltType.SQRT:
                    return (float)Math.Sqrt(val);
                case TiltType.RND:
                    return (float)Math.Ceiling(val);
                case TiltType.SIN:
                    return (float)Math.Sin(val);
                case TiltType.COS:
                    return (float)Math.Cos(val);
                case TiltType.TAN:
                    return (float)Math.Tan(val);
                case TiltType.LOG:
                    if (val <= 0) return val;
                    return (float)Math.Log(val);
                case TiltType.LOG2:
                    if (val <= 0 || tAmt <= 0) return val;
                    return (float)Math.Log(val) + (float)Math.Log(tAmt);
                case TiltType.SIN2:
                    return (float)Math.Sin(val) + (float)Math.Sin(tAmt);
                case TiltType.COS2:
                    return (float)Math.Cos(val) + (float)Math.Cos(tAmt);
                case TiltType.TAN2:
                    return (float)Math.Tan(val) + (float)Math.Tan(tAmt);
                case TiltType.POW:
                    return (float)Math.Pow(val, tAmt);
                default:
                    return val;
            }
        }
    }

}
