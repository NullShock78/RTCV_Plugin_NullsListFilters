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
using NullsListFilters.ListFilters.Entries;
//Made by NullShock78

namespace NullsListFilters.ListFilters
{
    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    [Export(typeof(IListFilter))]
    public class FloatManipListFilter : IListFilter
    {
        List<FloatManipEntry> entries = new List<FloatManipEntry>();
        List<FloatManipEntry> limiterEntries = new List<FloatManipEntry>();
        static Regex entryRegex = new Regex(@"(^|(?<type>\((ADD|SUB|DIV|MUL|SET|LIM)\))) *((?<range>([\-\d\.]+)->([\-\d\.]+))|(?<single>([\-\d\.]+)))");

        //Needed because apparently float.parse works differently in different OS languages
        static CultureInfo culture = new CultureInfo("en-US");

        
        public string Initialize(string filePath, string[] dataLines, bool flipBytes, bool syncListViaNetcore)
        {
            //Ignore flipbytes, as this list works differently
            int line = 2;
            foreach (string s in dataLines)
            {
                
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
                            case "LIM":
                                type = TiltType.LIM;
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

                    FloatManipEntry entry = new FloatManipEntry(type, origLine, min, max, range);

                    entries.Add(entry);
                    if (entry.IsLimiter)
                    {
                        limiterEntries.Add(entry);
                    }
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
                //Not to be used as a limiter, will never match
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
            MD5 hash = MD5.Create();
            hash.ComputeHash(bList.ToArray());
            string hashStr = Convert.ToBase64String(hash.Hash);
            return hashStr;
        }

        public int GetPrecision()
        {
            return 4;
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

            return BitConverter.GetBytes(entries[RtcCore.RND.Next(entries.Count)].Tilt(BitConverter.ToSingle(passthrough, 0)));
        }

        public List<string> GetStringList()
        {
            List<string> res = new List<string>();
            res.Add("@" + nameof(FloatManipListFilter)); //Add top line to specify class for reflection
            foreach (var e in entries)
            {
                res.Add(e.ToString());
            }
            return res;
        }
    }

}
