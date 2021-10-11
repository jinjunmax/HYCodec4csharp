using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CCFFCharset
    {
        public byte  format;
        public CCFFCSFormat0 format0 = new CCFFCSFormat0();
        public CCFFCSFormat1 format1 = new CCFFCSFormat1();
        public CCFFCSFormat2 format2 = new CCFFCSFormat2();
    }
}
