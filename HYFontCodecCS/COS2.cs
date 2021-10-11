using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class HYPANOSE
	{	        
		public byte			FamilyType;
		public byte			SerifStyle;
		public byte			Weight;
		public byte			Proportion;
		public byte			Contrast;
		public byte			StrokeVariation;
		public byte			ArmStyle;
		public byte         Letterform;
		public byte         Midline;
		public byte			XHeight;
	} 

    public class COS2
    {
        public COS2()
        {
            panose = new HYPANOSE();        
        }

		public UInt16					version {get; set;}
		public Int16                    xAvgCharWidth{get;set;}
		public UInt16					usWeightClass{get;set;}
        public UInt16					usWidthClass{get;set;}
		public Int16                    fsType{get;set;}
		public Int16                    ySubscriptXSize{get;set;}
		public Int16                    ySubscriptYSize{get;set;}
		public Int16                    ySubscriptXOffset{get;set;}
		public Int16                    ySubscriptYOffset{get;set;}		
        public Int16                    ySuperscriptXSize{get;set;}
		public Int16                    ySuperscriptYSize{get;set;}
		public Int16                    ySuperscriptXOffset{get;set;}
		public Int16                    ySuperscriptYOffset{get;set;}
		public Int16                    yStrikeoutSize{get;set;}
		public Int16                    yStrikeoutPosition{get;set;}
		public Int16                    sFamilyClass{get;set;}

		public HYPANOSE					panose{get;set;}
        public UInt32                   ulUnicodeRange1;
        public UInt32                   ulUnicodeRange2;
        public UInt32                   ulUnicodeRange3;
        public UInt32                   ulUnicodeRange4;

		public List<byte>				vtachVendID {get;set;}
		public UInt16					fsSelection{get;set;}
		public UInt16					usFirstCharIndex{get;set;}
		public UInt16					usLastCharIndex{get;set;}
		public Int16					sTypoAscender{get;set;}
		public Int16					sTypoDescender{get;set;}
		public Int16					sTypoLineGap{get;set;}
		public UInt16					usWinAscent{get;set;}		
        public UInt16                   usWinDescent{get;set;}
		public UInt32                   ulCodePageRange1{get;set;}
		public UInt32                   ulCodePageRange2{get;set;}
		public Int16					sxHeight{get;set;}
		public Int16                    sCapHeight{get;set;}
		public UInt16                   usDefaultChar{get;set;}
        public UInt16                   usBreakChar{get;set;}
        public UInt16                   usMaxContext{get;set;}

    }
}
