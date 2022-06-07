using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FontParserEntity;
using HYFontCodecCS;

namespace FontView
{
    public partial class Form1 : Form
    {
        HYDecodeC FontDec;
        int iCunGlyphInx = 0;       

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.InitialDirectory = "d:\\" ;
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1 ;
            OFD.RestoreDirectory = true ;

            if(OFD.ShowDialog() == DialogResult.OK)
            {   
                using (myStream)
                {
                    FontDec = new HYDecodeC();
                    FontDec.FontDecode(OFD.FileName);
                    iCunGlyphInx = 0;
                    CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                    myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
                }
            }
        }

        private void PREBTN_Click(object sender, EventArgs e)
        {
            if (iCunGlyphInx > 0)
            {
                iCunGlyphInx--;
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars,FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
                
            }
        }   // end of private void PREBTN_Click()

        private void Nextbtn_Click(object sender, EventArgs e)
        {
            iCunGlyphInx++;
            if (iCunGlyphInx < FontDec.tbMaxp.numGlyphs)
            {
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
            }
            else
            {
                iCunGlyphInx--;
                MessageBox.Show("最后一个字形");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           //  myPic = new MyPic();
            
           // this.Controls.Add(myPic);
            myPic1.OnMyClickEvent += PrintPoint;

        }

        private void PrintPoint(object sender, string strPt)
        {
            richTextBox1.Text = strPt;
        }

        private void myPic1_Click(object sender, EventArgs e)
        {
            
        }

        private void myPic1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void myPic1_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (myPic1.iMouseSel > 0)
            {
                myPic1.iMouseSel--;
                myPic1.Invalidate();
            }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
            /*
            if (myPic1.iMouseSel + 1 < GlyfItem.)
            {
                myPic1.iMouseSel++;
                myPic1.Invalidate();
            }
             * */
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (FindText.TextLength == 0) return;

            uint UNI = Convert.ToUInt32(FindText.Text, 16);
            iCunGlyphInx = FontDec.FindGryphIndexByUnciode(UNI);
            if (iCunGlyphInx > -1)
            {
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars,FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (FindText1.TextLength == 0) return;
            iCunGlyphInx = Convert.ToInt32(FindText1.Text, 10);
            if (iCunGlyphInx > -1)
            {
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars,FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
            }
        }
/*
        private static int CompareCodeMapItem(HYCodeMapItem A, HYCodeMapItem B)
        {
            if (A.GID > B.GID) return 0;
            if (A.GID == B.GID) return 1;
            return -1;
        }
        */
        private void button5_Click(object sender, EventArgs e)
        {
            
            HYEncode encode = new HYEncode();            
            if (FontDec.FntType == FONTTYPE.CFF)
            {
                UInt32 TableFlag = HYEncode.CFF_FLG | HYEncode.OS2_FLG | HYEncode.CMAP_FLG | HYEncode.HEAD_FLG | HYEncode.HHEA_FLG | HYEncode.HMTX_FLG | HYEncode.MAXP_FLG | HYEncode.NAME_FLG | HYEncode.POST_FLG | HYEncode.GASP_FLG | HYEncode.DSIG_FLG| HYEncode.VHEA_FLG | HYEncode.VMTX_FLG;
                CharsInfo chars = FontDec.GlyphChars;
                encode.EncodeFont("d:\\testCSharp.otf", "D:\\WorkZone\\Font\\CSharp\\HYFontCSharp\\HYFontCodecCS\\bin\\Profile.xml", TableFlag, ref chars);
            }
            if (FontDec.FntType == FONTTYPE.TTF)
            {
                UInt32 TableFlag = HYEncode.GLYF_FLG | HYEncode.OS2_FLG | HYEncode.CMAP_FLG | HYEncode.HEAD_FLG | HYEncode.HHEA_FLG | HYEncode.HMTX_FLG | HYEncode.LOCA_FLG | HYEncode.MAXP_FLG | HYEncode.NAME_FLG | HYEncode.POST_FLG | HYEncode.PREP_FLG | HYEncode.DSIG_FLG | HYEncode.GASP_FLG| HYEncode.VHEA_FLG | HYEncode.VMTX_FLG;

                CharsInfo chars = FontDec.GlyphChars;
                encode.EncodeFont("d:\\testCSharp.ttf", "D:\\WorkZone\\Font\\HYFontStudio\\bin\\data\\Profile.xml", TableFlag, ref chars);
            }
            /*
            HYEncode encode = new HYEncode();
            UInt32 TableFlag = HYEncode.CFF_FLG | HYEncode.OS2_FLG | HYEncode.CMAP_FLG | HYEncode.HEAD_FLG | HYEncode.HHEA_FLG | HYEncode.HMTX_FLG | HYEncode.MAXP_FLG | HYEncode.NAME_FLG | HYEncode.POST_FLG | HYEncode.GSUB_FLG | HYEncode.VHEA_FLG | HYEncode.VMTX_FLG;

            GlyfTable GlyTable = GlyfTableGetter.GlyfTableGetter.GetGlyfTable("D:\\test.json");

            encode.EncodeFont("d:\\testCSharp.otf", "D:\\WorkZone\\Font\\HYFontStudio\\bin\\data\\Profile.xml", TableFlag, GlyTable);
            */

        }

        private void SubSetFont_Click(object sender, EventArgs e)
        {
            /*
            string strFontFile, strTxtFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                strFontFile = FntfleDlg.FileName;                
                OpenFileDialog TxtfleDlg = new OpenFileDialog();
                TxtfleDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (TxtfleDlg.ShowDialog() == DialogResult.OK)
                {
                    strTxtFile = TxtfleDlg.FileName;
                    List<UInt32> lstUnicode = new List<UInt32>();
                    CBase.ReadCharFileToUnicode(strTxtFile, ref lstUnicode);                    

                    try
                    {                        
                        SaveFileDialog SubSetDlg = new SaveFileDialog();
                        SubSetDlg.Filter = "truetype files (*.ttf)|*.ttf|All files (*.*)|*.*";
                        if (SubSetDlg.ShowDialog() == DialogResult.OK)
                        {
                            HYRESULT hr;
                            string strSubsetName = SubSetDlg.FileName;
                            List<UInt32> lstMssUni = new List<UInt32>();
                            hr = HYFontAPI.ExtractFont(strFontFile, strSubsetName, ref lstUnicode, ref lstMssUni);

                            if (hr == HYRESULT.NOERROR)
                            {
                                MessageBox.Show("ok");
                            }
                            else
                            {
                                MessageBox.Show(hr.ToString());                                
                            }
                        }
                    }
                    catch (Exception ext)
                    {
                        MessageBox.Show(ext.ToString());
                    }                  
                }               
            }
            */
            
            HYFontInfo inf = new HYFontInfo();
            inf.Head_XMax = 0;
            inf.Head_XMin = 0;
            inf.Head_YMax = 0;
            inf.Head_YMin = 0;
            inf.HHEA_Asc = 0;
            inf.HHEA_Des = 0;
            inf.OS_TypoAsc = 0;
            inf.OS_TypoDes = 0;
            inf.OS_WinAsc = 0;
            inf.OS_WinDes = 0;

            HYRESULT  hr = HYFontAPI.ModifyFontInfo("d:\\CDK-Pracharath.ttf", "d:\\newCDK-Pracharath.ttf", inf);
            hr = HYFontAPI.ModifyFontInfo("d:\\CDK-Pracharath.ttf", "d:\\newCDK-Pracharath-01.ttf", inf);

        }

        /// <summary>
        /// TTF to Eot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            string strFontFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                strFontFile = FntfleDlg.FileName;
                try
                {                    
                    SaveFileDialog EotDlg = new SaveFileDialog();
                    EotDlg.Filter = "Eot files (*.eot)|*.eot|All files (*.*)|*.*";
                    if (EotDlg.ShowDialog() == DialogResult.OK)
                    {
                        HYRESULT hr;
                        string strEotName = EotDlg.FileName;
                        hr = HYFontAPI.FontToEOT(strFontFile, strEotName);

                        if (hr == HYRESULT.NOERROR)
                        {
                            MessageBox.Show("ok");
                        }
                        else
                        {
                            MessageBox.Show(hr.ToString());
                        }
                    }
                }
                catch (Exception ext)
                {
                    MessageBox.Show(ext.ToString());
                }
            }

        }   // end of private void button6_Click()

        /// <summary>
        /// TTF to Woff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            string strFontFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                strFontFile = FntfleDlg.FileName;
                try
                {
                    SaveFileDialog woffDlg = new SaveFileDialog();
                    woffDlg.Filter = "web open Font Formate files (*.woff)|*.woff|All files (*.*)|*.*";
                    if (woffDlg.ShowDialog() == DialogResult.OK)
                    {
                        HYRESULT hr;
                        string strwoffName = woffDlg.FileName;
                        hr = HYFontAPI.FONTToWoff(strFontFile, strwoffName);

                        if (hr == HYRESULT.NOERROR)
                        {
                            MessageBox.Show("ok");
                        }
                        else
                        {
                            MessageBox.Show(hr.ToString());
                        }
                    }
                }
                catch (Exception ext)
                {
                    MessageBox.Show(ext.ToString());
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string strFontFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "The Font Collection File (*.ttc)|*.ttc";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {                
                try
                {
                    strFontFile = FntfleDlg.FileName;

                    List<string> lstTTFs = new List<string>();
                    HYRESULT hr = HYFontAPI.TTCToFonts(strFontFile, ref lstTTFs);
                    if (hr == HYRESULT.NOERROR)
                    {
                        MessageBox.Show("ok");
                    }
                    else
                    {
                        MessageBox.Show(hr.ToString());
                    }                   
                }
                catch (Exception ext)
                {
                    MessageBox.Show(ext.ToString());
                }
            }

        }   // end of private void button8_Click()

        // fonts to ttc 
        private void button9_Click(object sender, EventArgs e)
        {            
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|opentype files (*.otf)|*.otf";

            // Allow the user to select multiple images.
            FntfleDlg.Multiselect = true;
            FntfleDlg.Title = "Font Browser";

            List<string> lstFontNames = new List<string>();
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Read the files
                    foreach (String file in FntfleDlg.FileNames)
                    {
                        lstFontNames.Add(file);
                    }

                    HYRESULT hr = HYFontAPI.FontToTTCForAFDK_C(lstFontNames);
                    //HYRESULT hr = HYFontAPI.FontToTTCForAFDK_P(lstFontNames);
                    
                    if (hr == HYRESULT.NOERROR)
                    {
                        MessageBox.Show("ok");
                    }
                    else
                    {
                        MessageBox.Show(hr.ToString());
                    }        

                }
                catch (Exception ext)
                {
                    // Could not load the image - probably related to Windows file system permissions.
                    MessageBox.Show("Reported error: " + ext.Message);
                }
            }

        }   // end of private void button8_Click()

        private void button10_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|opentype files (*.otf)|*.otf";

            // Allow the user to select multiple images.
            FntfleDlg.Multiselect = false;
            FntfleDlg.Title = "Font Browser";



        }
    }
}
