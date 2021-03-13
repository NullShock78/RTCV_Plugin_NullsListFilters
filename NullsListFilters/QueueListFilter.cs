//using Ceras;
//using RTCV.CorruptCore;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace NullsListFilters
//{
//    [Serializable]
//    [Ceras.MemberConfig(TargetMember.All)]
//    [Export(typeof(IListFilter))]
//    public class QueueListFilter : IListFilter
//    {
//        public int Precision = 4;
//        public int threshold = 10;

//        public QueueListFilter() { }

//        Queue<byte[]> queue = new Queue<byte[]>();
//        public string Initialize(string filePath, string[] dataLines, bool flipBytes, bool syncListViaNetcore)
//        {
//            Precision = int.Parse(dataLines[0]);
//            threshold = int.Parse(dataLines[1]);

//            var name = Path.GetFileNameWithoutExtension(filePath);
//            string hash = Filtering.RegisterList(this, name, syncListViaNetcore);
//            return hash;
//        }

//        public bool ContainsValue(byte[] bytes)
//        {
//            queue.Enqueue(bytes);
//            return true;
//        }

//        public byte[] GetRandomValue(string hash, int precision, byte[] passthrough = null)
//        {
//            if (queue.Count < threshold || precision != this.Precision) { return passthrough; }

//            if (passthrough == null)
//            {
//                return new byte[precision];
//            }

//            return queue.Dequeue();
//        }

//        public string GetHash()
//        {
//            List<byte> bList = new List<byte>();
//            byte[] prefix = new byte[] { (byte)'Q', (byte)'u', (byte)'e', (byte)'u', (byte)'e' };
//            bList.AddRange(prefix);
//            bList.AddRange(BitConverter.GetBytes(Precision));
//            bList.AddRange(BitConverter.GetBytes(threshold));
//            MD5 hash = MD5.Create();
//            hash.ComputeHash(bList.ToArray());
//            string hashStr = Convert.ToBase64String(hash.Hash);
//            return hashStr;
//        }

//        public int GetPrecision()
//        {
//            return Precision;
//        }

//        public List<string> GetStringList()
//        {
//            List<string> res = new List<string>();
//            res.Add("@" + nameof(QueueListFilter)); //Add top line to specify class for reflection
//            res.Add(Precision.ToString());
//            res.Add(threshold.ToString());
//            return res;
//        }
//    }
//}