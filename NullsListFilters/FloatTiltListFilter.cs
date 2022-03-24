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
        List<FloatTiltEntry> entries = new List<FloatTiltEntry>();
        List<FloatTiltEntry> limiterEntries = new List<FloatTiltEntry>();
        int repeatAmt = 0;
        static Regex entryRegex = new Regex(@"(^|(?<type>\((ADD|SUB|DIV|MUL|SET|ABS|NEG|SQRT|RND|SIN|SINADD|COS|COSADD|LOG|LOGADD|TAN|TANADD|POW|LIM)\))) *((?<range>([\-\d\.]+)->([\-\d\.]+))|(?<single>([\-\d\.]+)))");
        public FloatTiltListFilter() {  }

        //Needed because apparently float.parse works differently in different OS languages
        static CultureInfo culture = new CultureInfo("en-US");
        public string Initialize(string filePath, string[] dataLines, bool flipBytes, bool syncListViaNetcore)
        {
            //Ignore flipbytes, as this list works differently
            int line = 1;
            foreach (string str in dataLines)
            {
                string s = str.Trim();
                if (s.StartsWith("#") || s.StartsWith("//"))
                {
                    if (s.StartsWith("#"))
                    {
                        if (s.Substring(1).StartsWith("REPEAT_"))
                            if (s.Substring(1).StartsWith("REPEAT:"))
                                repeatAmt = Convert.ToInt32(s.Substring(8));
                        if (repeatAmt < 0)
                        {
                            repeatAmt = 0;
                        }
                        continue;
                    }
                }
                string workingString = s.Replace("f","").Replace("F", "").Replace(" ", "").ToUpper();                
                string origLine = workingString;

                var m = entryRegex.Match(workingString);
                if(m == null || !m.Success)
                {
                    throw new Exception($"Error loading {filePath}: Unable to parse line {line}, parse failed");
                }
                FloatTiltType type = FloatTiltType.ADD;

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
                                type = FloatTiltType.SUB;
                                break;
                            case "MUL":
                                type = FloatTiltType.MUL;
                                break;
                            case "DIV":
                                type = FloatTiltType.DIV;
                                break;
                            case "SET":
                                type = FloatTiltType.SET;
                                break;
                            case "ABS":
                                type = FloatTiltType.ABS;
                                break;
                            case "NEG":
                                type = FloatTiltType.NEG;
                                break;
                            case "SQRT":
                                type = FloatTiltType.SQRT;
                                break;
                            case "RND":
                                type = FloatTiltType.RND;
                                break;
                            case "SIN":
                                type = FloatTiltType.SIN;
                                break;
                            case "COS":
                                type = FloatTiltType.COS;
                                break;
                            case "TAN":
                                type = FloatTiltType.TAN;
                                break;
                            case "LOG":
                                type = FloatTiltType.LOG;
                                break;
                            case "LOGADD":
                                type = FloatTiltType.LOG2;
                                break;
                            case "SINADD":
                                type = FloatTiltType.SIN2;
                                break;
                            case "COSADD":
                                type = FloatTiltType.COS2;
                                break;
                            case "TANADD":
                                type = FloatTiltType.TAN2;
                                break;
                            case "POW":
                                type = FloatTiltType.POW;
                                break;
                            case "LIM":
                                type = FloatTiltType.LIM;
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
                        min = float.Parse(m.Groups[4].Value, culture);
                        max = float.Parse(m.Groups[5].Value, culture);
                    }
                    else
                    {
                        //9
                        min = float.Parse(m.Groups[9].Value, culture);
                    }

                    FloatTiltEntry entry = new FloatTiltEntry(type, origLine, min, max, range);
                    if (type == FloatTiltType.LIM)
                    {
                        limiterEntries.Add(entry);
                    }
                    else
                    {
                        entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error loading {filePath}: Unable to parse line {line}, float parse failed. Exception:\r\n{ex}");
                }
                line++;
            }

            if (entries.Count + limiterEntries.Count == 0)
            {
                throw new Exception($"Error loading {filePath}: List empty");
            }

            var name = Path.GetFileNameWithoutExtension(filePath);
            string hash = Filtering.RegisterList(this, name, syncListViaNetcore);
            return hash;
        }

        public bool ContainsValue(byte[] bytes)
        {
            if (limiterEntries.Count > 0)
            {
                float val = BitConverter.ToSingle(bytes, 0);

                int ct = limiterEntries.Count;
                for (int i = 0; i < ct; i++)
                {
                    if (limiterEntries[i].LimitValue(val))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                //No limiter available
                return false;
            }
        }

        public string GetHash()
        {
            List<byte> bList = new List<byte>();
            byte[] prefix = new byte[] { (byte)'F', (byte)'l', (byte)'o', (byte)'a', (byte)'t', (byte)'T', (byte)'F' };
            foreach (var e in entries)
            {           
                bList.AddRange(prefix);
                bList.AddRange(BitConverter.GetBytes((int)e.tiltType));
                bList.AddRange(BitConverter.GetBytes(e.IsRange));
                bList.AddRange(BitConverter.GetBytes(e.min));
                bList.AddRange(BitConverter.GetBytes(e.max));
            }
            byte[] prefix2 = new byte[] { (byte)'F', (byte)'l', (byte)'o', (byte)'a', (byte)'t', (byte)'T', (byte)'L', (byte)'I', (byte)'M' };

            foreach (var e in limiterEntries)
            {
                bList.AddRange(prefix);
                bList.AddRange(BitConverter.GetBytes((int)e.tiltType));
                bList.AddRange(BitConverter.GetBytes(e.IsRange));
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

            if (entries.Count == 0) return passthrough; //No entries

            if(precision != sizeof(float) || passthrough.Length != sizeof(float))
            {
                return passthrough;
            }
            var origValue = BitConverter.ToSingle(passthrough, 0);
            var ret = entries[RtcCore.RND.Next(entries.Count)].Modify(origValue);
            if (repeatAmt != 0)
            {
                for (int i = 0; i < repeatAmt; i++)
                {
                    ret = entries[RtcCore.RND.Next(entries.Count)].Modify(ret);
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
            foreach (var e in limiterEntries)
            {
                res.Add(e.ToString());
            }
            return res;
        }
    }

}
