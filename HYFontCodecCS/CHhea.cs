using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CHhea
    {
        public HYFIXED	version = new HYFIXED();
        public Int16 Ascender { get; set; }
        public Int16 Descender { get; set; }
        public Int16 LineGap { get; set; }
        public UInt16 advanceWidthMax { get; set; }
        public Int16 minLeftSideBearing { get; set; }
        public Int16 minRightSideBearing { get; set; }
        public Int16 xMaxExtent { get; set; }
        public Int16 caretSlopeRise { get; set; }
        public Int16 caretSlopeRun { get; set; }
        public Int16 caretOffset { get; set; }
        public Int16 reserved1 { get; set; }
        public Int16 reserved2 { get; set; }
        public Int16 reserved3 { get; set; }
        public Int16 reserved4 { get; set; }
        public Int16 metricDataFormat { get; set; }
        public UInt16 numberOfHMetrics { get; set; }
    }
}
