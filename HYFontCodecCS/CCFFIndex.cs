using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CCFFIndex
    {
        public UInt16   Count;
        public byte     Offsize;
        public List<UInt32> vtOffset = new List<UInt32>();
        public List<byte> vtData = new List<byte>();
    }
}
