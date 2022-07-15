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
using FontParserEntity;
using HYFontCodecCS;

namespace FontView
{
    public partial class ChangCodeWnd : Form
    {
        public ChangCodeWnd()
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
        }

        private void ConvterCode(ref HYEncode encde, List<UInt32> lstSrcCode, List<UInt32> lstCnvtCode)
        {
            CharsInfo chars = encde.GlyphChars;
            for (int i=0; i< lstSrcCode.Count; i++)
            {
                UInt32 utmp = lstSrcCode[i];
                int iGID = HYBase.GetGlyphsID(chars, lstSrcCode[i]);

                if (i< lstCnvtCode.Count) {
                    encde.GlyphChars.CharInfo[iGID].Unicode = lstCnvtCode[i].ToString();
                }
            }

        }   // end of private void ConvterCode()
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
            ecd.GlyphChars = dcd.GlyphChars;

        }   //end of private void CopyTable()

        private void EncodeFont(HYEncode ecd)
        {
            ecd.MakeCodeMap();
            ecd.MakeCmap();
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
            ConvterCode(ref ecd, lstSrcCode, lstCnvtCode);

            if (!(ecd.FontOpen(strEncodeFnt) == HYRESULT.NOERROR))
            {
                MessageBox.Show("创建字库失败");
                return;
            }

            EncodeFont(ecd);
            ecd.FontClose();
            ecd.SetCheckSumAdjustment(strEncodeFnt);

        }   // end of private void btnConvter_Click()
    }
}
