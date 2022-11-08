using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HYFontCodecCS;

namespace FontView
{
    public partial class MergeFontWnd : Form
    {
        public MergeFontWnd()
        {
            InitializeComponent();

        }   // end of public MergeFontWnd()

        private void btnLTBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                tbxLatin.Text = OFD.FileName;
            }

        }   // end of private void btnLTBtn_Click()

        private void btnHansBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                tbxHans.Text = OFD.FileName;
            }

        }   // end of private void btnHansBtn_Click()
        private void btnLCMerge_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            string strMergeFont="";
            if (OFD.ShowDialog() != DialogResult.OK) return;
            
            strMergeFont = OFD.FileName;

            HYDecode dcd1 = new HYDecode();
            GetFontAllTable(ref dcd1, tbxLatin.Text);
            HYDecode dcd2 = new HYDecode();
            GetFontTables(ref dcd2, tbxHans.Text);

            //cbxCover.Checked;

        }   // end of private void btnLCMerge_Click()        
        private HYRESULT GetFontAllTable(ref HYDecode dcd, string strFName)
        {
            HYRESULT hr;

            hr = dcd.FontOpen(strFName);
            if (hr != HYRESULT.NOERROR) return hr;

            dcd.DecodeMaxp();
            dcd.DecodeHead();
            dcd.DecodeCmap();
            dcd.DecodePost();

            for (int i = 0; i < dcd.tbDirectory.numTables; i++) {
                CTableEntry tbEntrt = dcd.tbDirectory.vtTableEntry[i];
                if (tbEntrt.tag == (int)TABLETAG.MAXP_TAG ||
                   tbEntrt.tag == (int)TABLETAG.HEAD_TAG ||
                   tbEntrt.tag == (int)TABLETAG.CMAP_TAG ||
                   tbEntrt.tag == (int)TABLETAG.POST_TAG)  {

                    continue;
                }
                else if (tbEntrt.tag == (int)TABLETAG.GLYF_TAG) {
                    dcd.DecodeLoca();
                    dcd.DecodeGlyph();
                }
                else if (tbEntrt.tag == (int)TABLETAG.CFF_TAG) {
                    dcd.DecodeCFF();
                }
                else {
                    dcd.GetTableData(tbEntrt.tag, ref tbEntrt);
                }
            }

            return hr;

        }   // end of private void GetFontTabel()
             
        private HYRESULT GetFontTables(ref HYDecode dcd, string strFName)
        {
            HYRESULT hr;

            hr = dcd.FontOpen(strFName);
            if (hr != HYRESULT.NOERROR) return hr;

            dcd.DecodeMaxp();
            dcd.DecodeHead();
            dcd.DecodeCmap();
            dcd.DecodePost();

            for (int i = 0; i < dcd.tbDirectory.numTables; i++)
            {
                CTableEntry tbEntrt = dcd.tbDirectory.vtTableEntry[i];
                if (tbEntrt.tag == (int)TABLETAG.GLYF_TAG){
                    dcd.DecodeLoca();
                    dcd.DecodeGlyph();
                }
                else if (tbEntrt.tag == (int)TABLETAG.CFF_TAG){
                    dcd.DecodeCFF();
                }
            }

            return hr;

        }   // end of private HYRESULT GetFontAllTable()

        private void MergeFont(HYDecode dcd1, HYDecode dcd2, HYEncode ecd)
        {
            for (int i = 0; i < dcd2.GlyphChars.CharInfo.Count; i++)
            {
               // dcd2.GlyphChars.CharInfo[i].Unicode;


            }


                for (int i=0; i<dcd1.GlyphChars.CharInfo.Count;i++)
            {





            }


        }   // end of private void MergeFont()

        private void MergeGlyphs(HYDecode dcd1, HYDecode dcd2, HYEncode ecd)
        {
            


        }


    }
}
