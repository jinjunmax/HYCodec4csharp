using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CCFFInfo
    {
        public CCFFHeader Header = new CCFFHeader();
        public CCFFTopDict TopDICT = new CCFFTopDict();
        public List<CCFFPrivteDict> vtFDArry = new List<CCFFPrivteDict>();
        public List<string> vtFontName = new List<string>();
        public CCFFStandString stnStrings = new CCFFStandString();
        public CCFFIndex globalSubIndex = new CCFFIndex();
        public CCFFPrivteDict PrivteDict = new CCFFPrivteDict();
        public CCFFFDSelect FDSelect = new CCFFFDSelect();
        public CCFFCharset Charset = new CCFFCharset();	

        protected ushort	FindSIDbyGlyphID(ushort usGID)
        {
            if (usGID==0) return 0;
            if (Charset.format == 0)
            {
	            return Charset.format0.vtSID[usGID-1];
            }

            if (Charset.format == 1 || Charset.format == 2)
            {
	            int t = Charset.format1.vtRang.Count;
	            int iGIDIndex = 1;
	            for (int i=0; i<t; i++) 
	            {
                    CCFFCSFormatRang1  FormatRang = Charset.format1.vtRang[i];
		            if (usGID == iGIDIndex)
		            {
			            return FormatRang.first;
		            }

		            if (usGID>iGIDIndex && usGID<=iGIDIndex+FormatRang.left)
		            {					
			            return (ushort)(FormatRang.first+usGID-iGIDIndex);
		            }
		            else
		            {
			            iGIDIndex += FormatRang.left+1;
		            }				
	            }
            }		

            return 0;

        }	// end of unsigned short CHYCFFInfo::FindSIDbyGlyph()
	    public string	FindStringbyGlyphID(ushort usGID)
	    {
            ushort usSID = FindSIDbyGlyphID(usGID);
            return stnStrings.szStandString[usSID];

	    }	// end of unsigned short CHYCFFInfo::FindNamebyGlyphID()
    }
}
