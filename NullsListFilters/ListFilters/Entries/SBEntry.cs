using Ceras;
using NullsListFilters.ListFilters.Data;
using RTCV.CorruptCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullsListFilters.ListFilters.Entries
{
    /// <summary>
    /// Represents an entry for bit filter list.
    /// </summary>
    [Serializable]
    [MemberConfig(TargetMember.All)]
    public class SBEntry
    {

        ulong template;
        ulong wildcard;
        ulong passthrough;
        ulong reserved;
        ulong unreserved;

        public string FormFactor { get; private set; }
        public int Precision { get; private set; }
        public string OriginalLine { get; set; }

        //Current random, slow, replace eventually
        static ulong NextULong()
        {
            byte[] byteBuffer = new byte[8];
            RtcCore.RND.NextBytes(byteBuffer);
            return BitConverter.ToUInt64(byteBuffer, 0);
        }

        public SBEntry(string formFactor, ulong template, ulong wildcard, ulong passthrough, ulong reserved, int precision)
        {
            this.FormFactor = formFactor.ToUpper();
            this.template = template;
            this.wildcard = wildcard;
            this.passthrough = passthrough;
            this.reserved = reserved;
            this.unreserved = ~reserved; //Opposite of reserved for efficiency
            this.Precision = precision;
        }

        //Gotta do this to satisfy Ceras
        public SBEntry()
        {
            this.template = 0;
            this.wildcard = 0;
            this.passthrough = 0;
            this.reserved = 0;
            this.unreserved = 0;
            Precision = 0;
        }

        public bool Matches(ulong data)
        {
            //template == data and reserved mask
            return template == (data & reserved);
        }

        public ulong GetRandom(ulong data)
        {
            //When passthrough is implemented, uncomment this line and remove the other
            return (NextULong() & wildcard) | (data & passthrough) | template;
            //return (NextULong() & unreserved) | template;
        }

        public ulong GetRandomLegacy()
        {
            return (NextULong() & unreserved) | template;
        }

        /// <summary>
        /// Gets bytes for hashing
        /// </summary>
        public byte[] GetBytesForHash()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(template));
            bytes.AddRange(BitConverter.GetBytes(wildcard));
            bytes.AddRange(BitConverter.GetBytes(passthrough));
            bytes.AddRange(BitConverter.GetBytes(reserved));
            //Don't need unreserved, it's just reserved flipped
            return bytes.ToArray();
        }
    }
}
