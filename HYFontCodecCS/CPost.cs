using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CPost
    {
        public class  CPostFormat2
	    {		    	
            public CPostFormat2()
            {        
                lstGlyphNameIndex = new List<ushort>();
                lstNameLength = new List<char>();
                lstNameStrings = new List<char>();
            }

			public   ushort	usNumberOfGlyphs;
			public   List<ushort>	lstGlyphNameIndex;
			public   List<char>		lstNameLength;
            public	 List<char>		lstNameStrings;
	    };

	    public class CPostFormat25
	    {
		    public CPostFormat25()
            {
                lstNameOffset = new List<char>();
                lstNameStrings = new List<char>();
            }

			public ushort usNumberOfGlyphs;
            public List<char> lstNameOffset;
            public List<char> lstNameStrings;
	    };

        public CPost()
        {
            version = new HYFIXED();
            italicAngle = new HYFIXED();
            PostFormat2 = new CPostFormat2();
            PostFormat25 = new CPostFormat25();

            version.fract = 0;
            version.value = 0;
            italicAngle.fract = 0;
            italicAngle.value = 0;
            underlinePosition = 0;
            underlineThickness = 0;
            isFixedPitch = 0;
            minMemType42 = 0;
            maxMemType42 = 0;
            minMemType1 = 0;
            maxMemType1 = 0;
            lstStandString = new List<string>();

            lstStandString.Add(".notdef");
            lstStandString.Add(".null");
            lstStandString.Add("nonmarkingreturn");
            lstStandString.Add("space");
            lstStandString.Add("exclam");
            lstStandString.Add("quotedbl");
            lstStandString.Add("numbersign");
            lstStandString.Add("dollar");
            lstStandString.Add("percent");
            lstStandString.Add("ampersand");
            lstStandString.Add("quotesingle");
            lstStandString.Add("parenleft");
            lstStandString.Add("parenright");
            lstStandString.Add("asterisk");
            lstStandString.Add("plus");
            lstStandString.Add("comma");
            lstStandString.Add("hyphen");
            lstStandString.Add("period");
            lstStandString.Add("slash");
            lstStandString.Add("zero");
            lstStandString.Add("one");
            lstStandString.Add("two");
            lstStandString.Add("three");
            lstStandString.Add("four");
            lstStandString.Add("five");
            lstStandString.Add("six");
            lstStandString.Add("seven");
            lstStandString.Add("eight");
            lstStandString.Add("nine");
            lstStandString.Add("colon");
            lstStandString.Add("semicolon");
            lstStandString.Add("less");
            lstStandString.Add("equal");
            lstStandString.Add("greater");
            lstStandString.Add("question");
            lstStandString.Add("at");
            lstStandString.Add("A");
            lstStandString.Add("B");
            lstStandString.Add("C");
            lstStandString.Add("D");
            lstStandString.Add("E");
            lstStandString.Add("F");
            lstStandString.Add("G");
            lstStandString.Add("H");
            lstStandString.Add("I");
            lstStandString.Add("J");
            lstStandString.Add("K");
            lstStandString.Add("L");
            lstStandString.Add("M");
            lstStandString.Add("N");
            lstStandString.Add("O");
            lstStandString.Add("P");
            lstStandString.Add("Q");
            lstStandString.Add("R");
            lstStandString.Add("S");
            lstStandString.Add("T");
            lstStandString.Add("U");
            lstStandString.Add("V");
            lstStandString.Add("W");
            lstStandString.Add("X");
            lstStandString.Add("Y");
            lstStandString.Add("Z");
            lstStandString.Add("bracketleft");
            lstStandString.Add("backslash");
            lstStandString.Add("bracketright");
            lstStandString.Add("asciicircum");
            lstStandString.Add("underscore");
            lstStandString.Add("grave");
            lstStandString.Add("a");
            lstStandString.Add("b");
            lstStandString.Add("c");
            lstStandString.Add("d");
            lstStandString.Add("e");
            lstStandString.Add("f");
            lstStandString.Add("g");
            lstStandString.Add("h");
            lstStandString.Add("i");
            lstStandString.Add("j");
            lstStandString.Add("k");
            lstStandString.Add("l");
            lstStandString.Add("m");
            lstStandString.Add("n");
            lstStandString.Add("o");
            lstStandString.Add("p");
            lstStandString.Add("q");
            lstStandString.Add("r");
            lstStandString.Add("s");
            lstStandString.Add("t");
            lstStandString.Add("u");
            lstStandString.Add("v");
            lstStandString.Add("w");
            lstStandString.Add("x");
            lstStandString.Add("y");
            lstStandString.Add("z");
            lstStandString.Add("braceleft");
            lstStandString.Add("bar");
            lstStandString.Add("braceright");
            lstStandString.Add("asciitilde");
            lstStandString.Add("Adieresis");
            lstStandString.Add("Aring");
            lstStandString.Add("Ccedilla");
            lstStandString.Add("Eacute");
            lstStandString.Add("Ntilde");
            lstStandString.Add("Odieresis");
            lstStandString.Add("Udieresis");
            lstStandString.Add("aacute");
            lstStandString.Add("agrave");
            lstStandString.Add("acircumflex");
            lstStandString.Add("adieresis");
            lstStandString.Add("atilde");
            lstStandString.Add("aring");
            lstStandString.Add("ccedilla");
            lstStandString.Add("eacute");
            lstStandString.Add("egrave");
            lstStandString.Add("ecircumflex");
            lstStandString.Add("edieresis");
            lstStandString.Add("iacute");
            lstStandString.Add("igrave");
            lstStandString.Add("icircumflex");
            lstStandString.Add("idieresis");
            lstStandString.Add("ntilde");
            lstStandString.Add("oacute");
            lstStandString.Add("ograve");
            lstStandString.Add("ocircumflex");
            lstStandString.Add("odieresis");
            lstStandString.Add("otilde");
            lstStandString.Add("uacute");
            lstStandString.Add("ugrave");
            lstStandString.Add("ucircumflex");
            lstStandString.Add("udieresis");
            lstStandString.Add("dagger");
            lstStandString.Add("degree");
            lstStandString.Add("cent");
            lstStandString.Add("sterling");
            lstStandString.Add("section");
            lstStandString.Add("bullet");
            lstStandString.Add("paragraph");
            lstStandString.Add("germandbls");
            lstStandString.Add("registered");
            lstStandString.Add("copyright");
            lstStandString.Add("trademark");
            lstStandString.Add("acute");
            lstStandString.Add("dieresis");
            lstStandString.Add("notequal");
            lstStandString.Add("AE");
            lstStandString.Add("Oslash");
            lstStandString.Add("infinity");
            lstStandString.Add("plusminus");
            lstStandString.Add("lessequal");
            lstStandString.Add("greaterequal");
            lstStandString.Add("yen");
            lstStandString.Add("mu");
            lstStandString.Add("partialdiff");
            lstStandString.Add("summation");
            lstStandString.Add("product");
            lstStandString.Add("pi");
            lstStandString.Add("integral");
            lstStandString.Add("ordfeminine");
            lstStandString.Add("ordmasculine");
            lstStandString.Add("Omega");
            lstStandString.Add("ae");
            lstStandString.Add("oslash");
            lstStandString.Add("questiondown");
            lstStandString.Add("exclamdown");
            lstStandString.Add("logicalnot");
            lstStandString.Add("radical");
            lstStandString.Add("florin");
            lstStandString.Add("approxequal");
            lstStandString.Add("Delta");
            lstStandString.Add("guillemotleft");
            lstStandString.Add("guillemotright");
            lstStandString.Add("ellipsis");
            lstStandString.Add("nonbreakingspace");
            lstStandString.Add("Agrave");
            lstStandString.Add("Atilde");
            lstStandString.Add("Otilde");
            lstStandString.Add("OE");
            lstStandString.Add("oe");
            lstStandString.Add("endash");
            lstStandString.Add("emdash");
            lstStandString.Add("quotedblleft");
            lstStandString.Add("quotedblright");
            lstStandString.Add("quoteleft");
            lstStandString.Add("quoteright");
            lstStandString.Add("divide");
            lstStandString.Add("lozenge");
            lstStandString.Add("ydieresis");
            lstStandString.Add("Ydieresis");
            lstStandString.Add("fraction");
            lstStandString.Add("currency");
            lstStandString.Add("guilsinglleft");
            lstStandString.Add("guilsinglright");
            lstStandString.Add("fi");
            lstStandString.Add("fl");
            lstStandString.Add("daggerdbl");
            lstStandString.Add("periodcentered");
            lstStandString.Add("quotesinglbase");
            lstStandString.Add("quotedblbase");
            lstStandString.Add("perthousand");
            lstStandString.Add("Acircumflex");
            lstStandString.Add("Ecircumflex");
            lstStandString.Add("Aacute");
            lstStandString.Add("Edieresis");
            lstStandString.Add("Egrave");
            lstStandString.Add("Iacute");
            lstStandString.Add("Icircumflex");
            lstStandString.Add("Idieresis");
            lstStandString.Add("Igrave");
            lstStandString.Add("Oacute");
            lstStandString.Add("Ocircumflex");
            lstStandString.Add("apple");
            lstStandString.Add("Ograve");
            lstStandString.Add("Uacute");
            lstStandString.Add("Ucircumflex");
            lstStandString.Add("Ugrave");
            lstStandString.Add("dotlessi");
            lstStandString.Add("circumflex");
            lstStandString.Add("tilde");
            lstStandString.Add("macron");
            lstStandString.Add("breve");
            lstStandString.Add("dotaccent");
            lstStandString.Add("ring");
            lstStandString.Add("cedilla");
            lstStandString.Add("hungarumlaut");
            lstStandString.Add("ogonek");
            lstStandString.Add("caron");
            lstStandString.Add("Lslash");
            lstStandString.Add("lslash");
            lstStandString.Add("Scaron");
            lstStandString.Add("scaron");
            lstStandString.Add("Zcaron");
            lstStandString.Add("zcaron");
            lstStandString.Add("brokenbar");
            lstStandString.Add("Eth");
            lstStandString.Add("eth");
            lstStandString.Add("Yacute");
            lstStandString.Add("yacute");
            lstStandString.Add("Thorn");
            lstStandString.Add("thorn");
            lstStandString.Add("minus");
            lstStandString.Add("multiply");
            lstStandString.Add("onesuperior");
            lstStandString.Add("twosuperior");
            lstStandString.Add("threesuperior");
            lstStandString.Add("onehalf");
            lstStandString.Add("onequarter");
            lstStandString.Add("threequarters");
            lstStandString.Add("franc");
            lstStandString.Add("Gbreve");
            lstStandString.Add("gbreve");
            lstStandString.Add("Idotaccent");
            lstStandString.Add("Scedilla");
            lstStandString.Add("scedilla");
            lstStandString.Add("Cacute");
            lstStandString.Add("cacute");
            lstStandString.Add("Ccaron");
            lstStandString.Add("ccaron");
            lstStandString.Add("dcroat");
        }

        public string FindNameByGID(ushort usID)
        {
            if (version.value == 1 && version.fract == 0) 
		    {
                return lstStandString[usID];
		    }

            if (version.value == 2 && version.fract == 0) 
		    {
			    int stCount = PostFormat2.lstGlyphNameIndex.Count;
                if (usID < stCount)
			    {
                    ushort usNameIndex = PostFormat2.lstGlyphNameIndex[usID];
				    return lstStandString[usNameIndex];
			    }
            }

            return "";

        }   // end of string FindNameByGID()

        public ushort FindNameIDByGID(ushort usID)
        {
            if (version.value == 1 && version.fract == 0)
            {
                if (usID < 258)
                {
                    return usID;
                }
            }

            if (version.value == 2 && version.fract == 0)
            {
                int stCount = PostFormat2.lstGlyphNameIndex.Count;
                if (usID < stCount)
                {
                    return PostFormat2.lstGlyphNameIndex[usID];
                }
            }

            return 0xffff;
        
        }   // end of ushort FindNameIDByGID()

        // 返回NameID 如果存在同名Name返回同名Name的ID，否则返回新加的ID
        public int InsertName(string strName)
        { 
            int iIndex = -1;
		    int strCount = lstStandString.Count;

		    for (int i=0; i<strCount; i++)
		    {
			    string strTmp = lstStandString[i];

			    if (strTmp == strName)
			    {
				    return iIndex = i;				
			    }
		    }

		    if (iIndex == -1)
		    {
                lstStandString.Add(strName);
                return lstStandString.Count-1;
		    }

		    return 0;

        }   // end of int InsertName()

        public HYFIXED      version;
        public HYFIXED      italicAngle;
        public Int16        underlinePosition;
        public Int16        underlineThickness;
        public UInt32       isFixedPitch;
        public UInt32       minMemType42;
        public UInt32       maxMemType42;
        public UInt32       minMemType1;
        public UInt32       maxMemType1;

        public List<string> lstStandString;
        public CPostFormat2 PostFormat2;
        public CPostFormat25 PostFormat25;

    }
}
