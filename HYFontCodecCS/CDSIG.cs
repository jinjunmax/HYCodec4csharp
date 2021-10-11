using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CDSIG
    {
        public CDSIG()
        {
            ulVersion = 0x00000001;
            usNumSigs = 0;
            usFlag = 0;        
        }

        public uint ulVersion;
        public ushort usNumSigs;
        public ushort usFlag;
    }
}
