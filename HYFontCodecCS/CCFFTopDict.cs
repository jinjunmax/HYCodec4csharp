using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CCFFTopDict
    {
        public string						strVersion;
		public int							iVersionSID;
		public string						strNotice;
		public int							iNoticeSID;
		public string						strCopyright;
		public int							iCopyrghtSID;
		public string 					    strFullName;	
		public int							iFullNameSID;
		public string						strFamilyName;	
		public int							iFamilyNameSID;
		public string 					    strWeight;	
		public int							iWeightSID;
		public long							isFixedPitch;
		public double						ItalicAngle;
		public double						UnderlinePosition = -100.0;
		public double						UnderlineThickness = 50.0;
		public long							PaintType;
		public long							CharStringType = 2;
		public List<double> 			    vtFontMatrix = new List<double>();
		public long							UniqueID = -1;	
		public List<int>				    vtFontBOX = new List<int>();
		public double						strokeWidth;
		public List<int>				    vtXUID = new List<int>();
		public long							charsetOffset;
		public long							encodingOffset;
		public long							charStringOffset;
		public long							PrivteDictSize;
		public long							PrivteOffset;
		public long							SyntheticBaseIndex;
		public string						strPostSript;	
		public int							PostSriptSID;
		public string						strBaseFontName;	
		public int							BaseFontNameSID;
		public List<double>				    vtBaseFontBlend = new List<double>();
		public int							IsCIDFont = 0;
		// cid font 特有
		public CCFFROS						Ros = new CCFFROS();
		public double						CIDFontVersion;
		public double						CIDFontRevision;
		public double						CIDFontType;
		public long 						CIDCount;
		public long							UIDBase;
		public long							FDArryIndexOffset;
		public long							FDSelectOffset;
		public string						strCIDFontName;
		public int							CIDFontNameSID;	
    }
}
