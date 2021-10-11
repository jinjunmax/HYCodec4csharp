using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class HMTX_LONGHORMERTRIC
	{
        public UInt16 advanceWidth { get; set; }
        public Int16 lsb { get; set; }
	}

    public class CHmtx
    {
        public List<HMTX_LONGHORMERTRIC> lstLonghormetric = new List<HMTX_LONGHORMERTRIC>();
    }
}
