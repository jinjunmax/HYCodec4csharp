using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FontParserEntity;

namespace HYFontCodecCS
{
    public class HYFontBase : HYBase
    {
        protected FONTTYPE FontType;
        protected FileStream FRStrm;
        protected CTableDirectory TableDirectorty;        
        protected CLoca Loca;        
        protected CMaxp Maxp;
        protected CHead Head;        
        protected CName Name;
        protected CCmap Cmap;
        protected CHmtx Hmtx;
        protected CHhea Hhea;
        protected CGSUB Gsub;
        protected COS2 OS2;
        protected CPost Post;        
        protected CVhea Vhea;
        protected CVmtx Vmtx;
        protected CCFFInfo CFFInfo;
        protected CharsInfo Chars;
        protected CHYCOLR Colr;
        protected CDSIG DSIG;
        protected CGasp Gasp;

        public HYCodeMap codemap;
        public FONTTYPE FntType
        {
            get { return FontType; }
            set { FontType = value; }
        }

        public CTableDirectory tbDirectory
        {
            get { return TableDirectorty; }
            set { TableDirectorty = value; }
        }

        public CLoca tbLoca
        {
            get { return Loca; }
            set { Loca = value; }
        }

        public CMaxp tbMaxp
        {
            get { return Maxp; }
            set { Maxp = value; }
        }

        public CHead tbHead
        {
            get { return Head; }
            set { Head = value; }
        }

        public CName tbName
        {
            get { return Name; }
            set { Name = value; }
        }

        public CCmap tbCmap
        {
            get { return Cmap; }
            set { Cmap = value; }
        }

        public CHmtx tbHmtx
        {
            get { return Hmtx; }
            set { Hmtx = value; }
        }

        public CHhea tbHhea
        {
            get { return Hhea; }
            set { Hhea = value; }
        }

        public CGSUB tbGsub
        {
            get { return Gsub; }
            set { Gsub = value; }
        }

        public COS2 tbOS2
        {
            get { return OS2; }
            set { OS2 = value; }
        }

        public CPost tbPost
        {
            get { return Post; }
            set { Post = value; }
        }

        public CVhea tbVhea
        {
            get { return Vhea; }
            set { Vhea = value; }
        }

        public CVmtx tbVmtx
        {
            get { return Vmtx; }
            set { Vmtx = value; }
        }

        public CCFFInfo tbCFF
        {
            get { return CFFInfo; }
            set { CFFInfo = value; }
        }

        public CharsInfo GlyphChars
        {
            get { return Chars; }
            set { Chars = value; }        
        }

        public CHYCOLR tbCOLR
        {
            get { return Colr; }
            set { Colr = value; }
        }

        public CDSIG  tbDSIG
        {
            get { return DSIG; }
            set { DSIG = value; }        
        }

        public CGasp tbGasp
        {
            get { return Gasp;}
            set { Gasp = value;}        
        }

        public void UnicodeStringToList(string strUnicode, ref List<uint> lstUnicode)
        {
            if (strUnicode == "")
            {
                lstUnicode.Clear();
                return;
            }

            string delimStr = "|";
            char[] delimiter = delimStr.ToCharArray();
            string[] split = strUnicode.Split(delimiter);
            for (int i = 0; i<split.Length; i++)
            {
                string strTmp = split[i];                
                lstUnicode.Add(Convert.ToUInt32(strTmp));
            }

        }   // end of public void UnicodeStringToList()

        public void UnicodeListToString(ref string strUnicode, List<uint> lstUnicode)
        {
            for (int i = 0; i < lstUnicode.Count; i++)
            {
                strUnicode += lstUnicode[i].ToString();
                if (i != lstUnicode.Count - 1)
                {
                    strUnicode += "|";        
                }
            }

        }   // end of public void UnicodeListToString()

        public bool BoundStringToInt(string strBound, out int xmin, out int ymin, out int xmax,out int ymax)
        {
            if (string.IsNullOrWhiteSpace(strBound))
            {
                xmin = 0;
                ymin = 0;
                xmax = 0;
                ymax = 0;

                return false;
            }

            string delimStr = ",";
            char[] delimiter = delimStr.ToCharArray();
            string[] split = strBound.Split(delimiter,4);

            xmin = Convert.ToInt32(split[0],10);
            ymin = Convert.ToInt32(split[1],10);
            xmax = xmin + Convert.ToInt32(split[2],10);
            ymax = ymin + Convert.ToInt32(split[3],10);

            return true;

        }   // end of public void BoundStringToList()

        public void BoundIntToString(ref string strBound, int xmin, int ymin, int xmax, int ymax)
        {
            strBound = "";
            strBound += xmin.ToString();
            strBound += ",";

            strBound += ymin.ToString();
            strBound += ",";

            strBound += (xmax - xmin).ToString();
            strBound += ",";

            strBound += (ymax - ymin).ToString();

        }   // end of public void BoundIntToString()
    }
}
