using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CGasp_RangeRecord
    { 
        public ushort rangeMaxPPEM;
		public ushort rangeGaspBehavior;
    }

    public class CGasp_Head
    {     
        public ushort version;
		public ushort numRanges;
    }

    public class CGasp
    {
        public CGasp_Head Head;
        public List<CGasp_RangeRecord> lstRangeRecord;
    }
}
