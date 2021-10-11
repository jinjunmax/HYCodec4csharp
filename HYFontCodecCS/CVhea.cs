using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CVhea
    {
        public HYFIXED	version = new HYFIXED();
        public Int16    ascent {get;set;}
        public Int16    descent { get; set; }
        public Int16    lineGap { get; set; }
        public Int16    advanceHeightMax { get; set; }
        public Int16    minTop { get; set; }
        public Int16    minBottom { get; set; }
        public Int16    yMaxExtent { get; set; }
        public Int16    caretSlopeRise { get; set; }
        public Int16    caretSlopeRun { get; set; }
        public Int16    caretOffset { get; set; }
        public Int16    reserved1 { get; set; }
        public Int16    reserved2 { get; set; }
        public Int16    reserved3 { get; set; }
        public Int16    reserved4 { get; set; }
        public Int16    metricDataFormat { get; set; }
        public UInt16   numOfLongVerMetrics { get; set; }
    }
}
