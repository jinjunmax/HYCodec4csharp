using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CCFFPrivteDict
    {
        public string   strFontName;
        public int      iFontNameID;
        public List<double> vtBlueValues = new List<double>();
        public List<double> vtOtherBlues = new List<double>();
        public List<double> vtFamilyBlues = new List<double>();
        public List<double> vtFamilyOtherBlues = new List<double>();
        public double fBlueScale = 0.039625;
        public double fBlueShift=7.0;
        public double fBlueFuzz=1.0;
        public double fStdHW=0.0;
        public double fStdVW=0.0;
        public List<double> vtStemSnapH = new List<double>();
        public List<double> vtStemSnapV = new List<double>();
        public long lForceBold=0;
        public long lLanguageGroup=0;
        public double fExpansionFactor = 0.06;
        public double finitialRandomSeed=0;
        public long lSubrsOffset;
        public long ldefaultWidthX;
        public long lnominalWidthX;
        public CCFFIndex SubIndex = new CCFFIndex();	
    }
}
