using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using FontParserEntity;
using HYFontCodecCS;

namespace FontView
{
    public partial class UpdateFontCodeWnd : Form
    {
        public UpdateFontCodeWnd()
        {
            InitializeComponent();
        }

        private void btnFnt_Click(object sender, EventArgs e)
        {            
            OpenFileDialog OFD = new OpenFileDialog();            
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {   
                tbxFnt.Text =  OFD.FileName;
            }

        }   // end of private void btnFnt_Click()

        private void btnSrcCode_Click(object sender, EventArgs e)
        {           
            OpenFileDialog OFD = new OpenFileDialog();            
            OFD.Filter = "Text files (*.txt)|*.txt||";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                tbxSrcCode.Text = OFD.FileName;
            }

        }   // end of private void btnSrcCode_Click()

        private void btnCvtCode_Click(object sender, EventArgs e)
        {           
            OpenFileDialog OFD = new OpenFileDialog();            
            OFD.Filter = "Text files (*.txt)|*.txt||";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                tbxCvtCode.Text = OFD.FileName;
            }

        }   // end of private void btnCvtCode_Click()

        public static CharInfo DeepCopy<CharInfo>(CharInfo obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(CharInfo));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (CharInfo)retval;

        }   // end of public static CharInfo DeepCopy<CharInfo>()

        private void ConvterCode(ref HYEncode encde, ref HYDecode decode, List<UInt32> lstSrcCode, List<UInt32> lstCnvtCode)
        {
            CharsInfo dcdchars = decode.GlyphChars;

            // miss char
            encde.GlyphChars.CharInfo = new List<CharInfo>();
            encde.GlyphChars.CharInfo.Add(dcdchars.CharInfo[0]);
            
            for (int i=1; i< decode.tbMaxp.numGlyphs; i++)
            {   
                CharInfo inf = new CharInfo();
                inf = DeepCopy(dcdchars.CharInfo[i]);

                List<UInt32> lstUni = new List<uint>();
                decode.UnicodeStringToList(inf.Unicode, ref lstUni);

                int srcID = -1;
                for (int j = 0; j < lstUni.Count;j++)
                {
                    srcID = FindID(lstSrcCode, lstUni[j]);
                    if (srcID != -1)
                        break;
                }

                if (srcID !=-1)
                {
                    inf.Unicode = lstCnvtCode[srcID].ToString();
                    inf.Name = "uni" + Convert.ToString(Convert.ToInt32(inf.Unicode), 16).ToUpper();
                }
                encde.GlyphChars.CharInfo.Add(inf);
            }

        }   // end of private void ConvterCode()

        private int FindID(List<UInt32> lstSrcCode, UInt32 uni)
        {
            for (int i=0;i<lstSrcCode.Count; i++)  {
                if (lstSrcCode[i] == uni)
                    return i;
            }

            return -1;

        }   // end of private int FindID()

        /// <summary>
        /// 解码字库
        /// </summary>        
        /// <param name="dcd"></param>
        private void DecodeFont(ref HYDecode dcd)
        {
            dcd.DecodeMaxp();
            dcd.DecodeHead();
            dcd.DecodeCmap();
            dcd.DecodePost();

            for (int i=0; i< dcd.tbDirectory.numTables; i++)
            {
                CTableEntry tbEntrt =  dcd.tbDirectory.vtTableEntry[i];
                if(tbEntrt.tag == (int)TABLETAG.MAXP_TAG||
                   tbEntrt.tag == (int)TABLETAG.HEAD_TAG||
                   tbEntrt.tag == (int)TABLETAG.CMAP_TAG||
                   tbEntrt.tag == (int)TABLETAG.POST_TAG)
                {

                    continue;
                }  
                else if(tbEntrt.tag == (int)TABLETAG.GLYF_TAG)
                {
                    dcd.DecodeLoca();
                    dcd.DecodeGlyph();
                }
                else if (tbEntrt.tag == (int)TABLETAG.CFF_TAG)
                {
                    dcd.DecodeCFF();
                }
                else
                {
                    dcd.GetTableData(tbEntrt.tag,ref tbEntrt);
                }
            }

        }   // end of private void DecodeFont()

        private void CopyTable(ref HYEncode ecd, ref HYDecode dcd)
        {
            ecd.FntType = dcd.FntType;
            ecd.tbDirectory = dcd.tbDirectory;
            ecd.tbMaxp = dcd.tbMaxp;
            ecd.tbHead = dcd.tbHead;
            ecd.tbCmap = dcd.tbCmap;
            if (dcd.tbPost.version.value == 3 && dcd.tbPost.version.fract==0)
            {
                ecd.tbPost = dcd.tbPost;
            }

        }   //end of private void CopyTable()
        private void EncodeFont(HYEncode ecd)
        {
            ecd.MakeCodeMap();
            ecd.MakeCmap();

            if (ecd.tbPost.version.value==2&& ecd.tbPost.version.fract==0)
                ecd.MakePost();

            ecd.EncodeTableDirectory();

            for (int i = 0; i < ecd.tbDirectory.numTables; i++)
            {
                CTableEntry tbEntrt = ecd.tbDirectory.vtTableEntry[i];
                if (tbEntrt.tag == (int)TABLETAG.HEAD_TAG)
                {
                    ecd.Encodehead();
                }
                else if (tbEntrt.tag == (int)TABLETAG.MAXP_TAG)
                {
                    ecd.Encodemaxp();
                }
                else if (tbEntrt.tag == (int)TABLETAG.POST_TAG)
                {
                    ecd.Encodepost();
                }
                else if (tbEntrt.tag == (int)TABLETAG.CMAP_TAG)
                {
                    ecd.Encodecmap(ecd.codemap.lstCodeMap);
                }
                else if (tbEntrt.tag == (int)TABLETAG.GLYF_TAG)
                {
                    ecd.EncodeGlyph();                    
                }
                else if (tbEntrt.tag == (int)TABLETAG.LOCA_TAG)
                {
                    ecd.Encodeloca();
                }
                else if (tbEntrt.tag == (int)TABLETAG.CFF_TAG)
                {
                    ecd.EncodeCFF();
                }
                else
                {
                    tbEntrt.offset = (uint)ecd.EncodeStream.Position;
                    ecd.EncodeStream.Write(tbEntrt.aryTableData, 0, (int)tbEntrt.length);
                    ecd.AlignmentData(tbEntrt.length);
                }
            }

            for (int i = 0; i < ecd.tbDirectory.numTables; i++)
            {
                CTableEntry HYEntry = ecd.tbDirectory.vtTableEntry[i];
                ecd.CalcFontTableChecksum(ecd.EncodeStream,ref HYEntry);
            }

            ecd.EncodeTableDirectory();
            ecd.FontClose();          

        }   // end of private void EncodeFont()

        private void btnConvter_Click(object sender, EventArgs e)
        {            
            if (tbxFnt.Text.Length == 0) return;           
            if (tbxSrcCode.Text.Length == 0) return;
            if (tbxCvtCode.Text.Length == 0) return;

            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            SFD.FilterIndex = 1;
            SFD.RestoreDirectory = true;

            string strEncodeFnt;
            if (SFD.ShowDialog() != DialogResult.OK) return;
            strEncodeFnt = SFD.FileName;

            List<uint> lstSrcCode = new List<uint>();
            CBase.ReadCodeFile(tbxSrcCode.Text,ref lstSrcCode);

            List<uint> lstCnvtCode = new List<uint>();
            CBase.ReadCodeFile(tbxCvtCode.Text, ref lstCnvtCode);

            HYEncode ecd = new HYEncode();
            HYDecode dcd = new HYDecode();
            if (!(dcd.FontOpen(tbxFnt.Text) == HYRESULT.NOERROR))
            {
                MessageBox.Show("字库打开失败");
                return;
            }

            DecodeFont(ref dcd);
            CopyTable(ref ecd, ref dcd);
            ConvterCode(ref ecd, ref dcd,lstSrcCode, lstCnvtCode);

            if (!(ecd.FontOpen(strEncodeFnt) == HYRESULT.NOERROR))
            {
                MessageBox.Show("创建字库失败");
                return;
            }

            EncodeFont(ecd);
            ecd.FontClose();
            ecd.SetCheckSumAdjustment(strEncodeFnt);

            MessageBox.Show("操作完成");

        }   // end of private void btnConvter_Click()
    }
}
