using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    public partial class MainWnd : Form
    {
        HYDecode FontDec;
        int iCunGlyphInx = 0;

        public MainWnd()
        {
            InitializeComponent();
            edt_ThknssX.Text = "50";
            edt_ThknssY.Text = "50";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.InitialDirectory = "d:\\";
            OFD.Filter = "True Type files (*.ttf)|*.ttf|Open Type files (*.otf)|*.otf|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;

            if (OFD.ShowDialog() == DialogResult.OK)
            {
                using (myStream)
                {
                    FontDec = new HYDecode();
                    FontDec.FontDecode(OFD.FileName);
                    iCunGlyphInx = 0;
                    CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                    myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
                }
            }
        }

        private void PREBTN_Click(object sender, EventArgs e)
        {
            if (FontDec == null) return;

            if (iCunGlyphInx > 0)
            {
                iCunGlyphInx--;
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);

            }
        }   // end of private void PREBTN_Click()

        private void Nextbtn_Click(object sender, EventArgs e)
        {
            if (FontDec == null) return;

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
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (FindText1.TextLength == 0) return;
            iCunGlyphInx = Convert.ToInt32(FindText1.Text, 10);
            if (iCunGlyphInx > -1)
            {
                CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
                myPic1.SetGlyphInfo(GlyfItem, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
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
                UInt32 TableFlag = HYEncode.CFF_FLG | HYEncode.OS2_FLG | HYEncode.CMAP_FLG | 
                                    HYEncode.HEAD_FLG | HYEncode.HHEA_FLG | HYEncode.HMTX_FLG |
                                    HYEncode.MAXP_FLG | HYEncode.NAME_FLG | HYEncode.POST_FLG | 
                                    HYEncode.GASP_FLG | HYEncode.DSIG_FLG | HYEncode.VHEA_FLG | 
                                    HYEncode.VMTX_FLG;

                CharsInfo chars = FontDec.GlyphChars;
                encode.EncodeFont("d:\\testCSharp.otf", "D:\\WorkZone\\Font\\CSharp\\HYFontCSharp\\HYFontCodecCS\\bin\\Profile.xml", TableFlag, ref chars);
            }
            if (FontDec.FntType == FONTTYPE.TTF)
            {
                UInt32 TableFlag = HYEncode.GLYF_FLG | HYEncode.OS2_FLG | 
                    HYEncode.CMAP_FLG | HYEncode.HEAD_FLG |
                    HYEncode.HHEA_FLG | HYEncode.HMTX_FLG |
                    HYEncode.LOCA_FLG | HYEncode.MAXP_FLG | 
                    HYEncode.NAME_FLG | HYEncode.POST_FLG | 
                    HYEncode.PREP_FLG | HYEncode.DSIG_FLG | 
                    HYEncode.GASP_FLG | HYEncode.VHEA_FLG |
                    HYEncode.VMTX_FLG;

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
                            HYRESULT hr = HYRESULT.NOERROR;
                            string strSubsetName = SubSetDlg.FileName;
                            List<UInt32> lstMssUni = new List<UInt32>();

                            //DateTime tm1 = DateTime.Now;
                            hr = HYFontAPI.ExtractFont(strFontFile, strSubsetName, ref lstUnicode, ref lstMssUni);
                            //DateTime tm2 = DateTime.Now;
                            //double dbspan = (tm2 - tm1).TotalSeconds;
                            //MessageBox.Show(dbspan.ToString());

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
        }   // end of private void SubSetFont_Click()

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
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|Opentype files (*.otf)|*.otf";
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
        }  // end of button7_Click
        private void WOFF2TTF_Click(object sender, EventArgs e)
        {
            string strwoffFile;
            OpenFileDialog wffDlg = new OpenFileDialog();
            wffDlg.Filter = "woff files (*.woff)|*.woff";
            if (wffDlg.ShowDialog() == DialogResult.OK)
            {
                strwoffFile = wffDlg.FileName;
                try
                {
                    SaveFileDialog fontDlg = new SaveFileDialog();
                    fontDlg.Filter = "truetype files (*.ttf)|*.ttf|Opentype files (*.otf)|*.otf";
                    if (fontDlg.ShowDialog() == DialogResult.OK)
                    {
                        HYRESULT hr;
                        string strfntName = fontDlg.FileName;
                        //hr = HYFontAPI.;

                        
                    }
                }
                catch (Exception ext)
                {
                    MessageBox.Show(ext.ToString());
                }
            }


        }   // end of private void WOFF2TTF_Click()

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

            String strBaseFile;
            if (FntfleDlg.ShowDialog() != DialogResult.OK) return;
            strBaseFile = FntfleDlg.FileName;


            FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "truetype files (*.ttf)|*.ttf|opentype files (*.otf)|*.otf";

            // Allow the user to select multiple images.
            FntfleDlg.Multiselect = false;
            FntfleDlg.Title = "Font Browser";

            String strCmprFile;
            if (FntfleDlg.ShowDialog() != DialogResult.OK) return;
            strCmprFile = FntfleDlg.FileName;

        }   // end of private void button10_Click(object sender, EventArgs e)

        [DllImport("FT_Glyphs.dll", EntryPoint = "SetOutlineThickness", CallingConvention = CallingConvention.Cdecl)]
        extern static IntPtr SetOutlineThickness(byte[] pOutline, ref int length, int xstrength, int ystrength);
        [DllImport("FT_Glyphs.dll", EntryPoint = "ReleaseOutlineBuffer", CallingConvention = CallingConvention.Cdecl)]
        extern static void ReleaseOutlineBuffer();

        private void btn_Thickness_Click(object sender, EventArgs e)
        {
            if (edt_ThknssX.TextLength == 0) return;
            int iXStrength = Convert.ToInt32(edt_ThknssX.Text, 10);
            if (edt_ThknssY.TextLength == 0) return;
            int iYStrength = Convert.ToInt32(edt_ThknssY.Text, 10);

            string StringInf = "";
            CharInfo GlyfItem = FontDec.GlyphChars.CharInfo[iCunGlyphInx];
            if (GlyfItem.ContourCount == 0) return;

            CharInfoToStringInfo(GlyfItem, ref StringInf);

            byte[] inChar = System.Text.Encoding.ASCII.GetBytes(StringInf);
            int iLength = inChar.Length;
            IntPtr pRet = SetOutlineThickness(inChar, ref iLength, iXStrength, iYStrength);
            if (pRet != null)
            {
                string strRet = Marshal.PtrToStringAnsi(pRet);

                CharInfo charInf = new CharInfo();
                charInf.AdHeight = GlyfItem.AdHeight;
                charInf.AdWidth = GlyfItem.AdWidth;
                charInf.Name = GlyfItem.Name;
                charInf.Unicode = GlyfItem.Unicode;
                charInf.Section = GlyfItem.Section;
                StringInfoToChars(ref charInf, strRet);
                myPic2.SetGlyphInfo(charInf, FontDec.GlyphChars, FontDec.tbHead, FontDec.tbHhea, FontDec.FntType);
            }

        }   // end of private void btn_Thickness_Click()

        private void CharInfoToStringInfo(CharInfo GlyfItem, ref string StringInf)
        {
            string strPt = "";
            string strFlag = "";
            string strendPts = "";

            int iPtIndx = 0;

            for (int i = 0; i < GlyfItem.ContourCount; i++)
            {
                for (int j = 0; j < GlyfItem.ContourInfo[i].PointCount; j++)
                {
                    strPt += GlyfItem.ContourInfo[i].PtInfo[j].X.ToString();
                    strPt += ".";
                    strPt += GlyfItem.ContourInfo[i].PtInfo[j].Y.ToString();
                    strPt += "|";

                    strFlag += GlyfItem.ContourInfo[i].PtInfo[j].PtType.ToString();
                    strFlag += ",";

                    iPtIndx++;
                }
                strendPts += (iPtIndx - 1).ToString();
                strendPts += ",";
            }

            strPt = strPt.Substring(0, strPt.Length - 1);
            strPt += ";";
            strFlag = strFlag.Substring(0, strFlag.Length - 1);
            strFlag += ";";
            strendPts = strendPts.Substring(0, strendPts.Length - 1);
            strendPts += ";";

            strPt += strFlag;
            strPt += strendPts;

            StringInf = strPt;

        }   // end of private void CharInfoToStringInfo()

        private void StringInfoToChars(ref CharInfo GlyfItem, string StringInf)
        {
            string[] strSZ = StringInf.Split(';');
            if (strSZ.Length < 3) return;

            //拆分点;            
            string[] szPoints = strSZ[0].Split('|');
            string[] szflgs = strSZ[1].Split(',');
            string[] szEndpts = strSZ[2].Split(',');

            GlyfItem.ContourCount = szEndpts.Length;

            int iContouIndx = 0;

            ContourInfo cnturinf = new ContourInfo();
            for (int i = 0; i < szPoints.Length; i++)
            {
                string[] szpt = szPoints[i].Split('.');
                PtInfo pt = new PtInfo();
                pt.X = int.Parse(szpt[0]);
                pt.Y = int.Parse(szpt[1]);
                pt.PtType = int.Parse(szflgs[i]);

                cnturinf.PtInfo.Add(pt);
                if (szEndpts.Length != iContouIndx)
                {
                    if (i == int.Parse(szEndpts[iContouIndx]))
                    {
                        if (iContouIndx == 0)
                        {
                            cnturinf.PointCount = i + 1;
                        }
                        else
                        {
                            cnturinf.PointCount = int.Parse(szEndpts[iContouIndx]) - int.Parse(szEndpts[iContouIndx - 1]);
                        }

                        GlyfItem.ContourInfo.Add(cnturinf);
                        cnturinf = new ContourInfo();
                        iContouIndx++;
                    }
                }
            }

        }   // end of private void StringInfoToChars()

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseOutlineBuffer();

        }   // end of private void Form1_FormClosed()

        private void FontInf_Click(object sender, EventArgs e)
        {
            FntInfWnd wndFntInf = new FntInfWnd(FontDec);
            wndFntInf.Show();

        }   // end of private void FontInf_Click()

        private void btnChngeCode_Click(object sender, EventArgs e)
        {
            UpdateFontCodeWnd wndChang = new UpdateFontCodeWnd();
            wndChang.Show();

        }   //end of private void btnChngeCode_Click()

        private void btnMgrFnt_Click(object sender, EventArgs e)
        {
            MergeFontWnd wndMergeFont = new MergeFontWnd();
            wndMergeFont.Show();

        }   // end of private void btnMgrFnt_Click()

        private void btnCodeRmRepeat_Click(object sender, EventArgs e)
        {
            string strCodeFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                strCodeFile = FntfleDlg.FileName;
                try {
                    SaveFileDialog nwCodeDlg = new SaveFileDialog();
                    nwCodeDlg.Filter = "码表文件 (*.txt)|*.txt";
                    if (nwCodeDlg.ShowDialog() == DialogResult.OK)
                    {   
                        string strNewCodeTB = nwCodeDlg.FileName;

                        List<uint> lstUni = new List<uint>();
                        CBase.ReadCodeFile(strCodeFile, ref lstUni);
                        CBase.UnicodeUnrepeat(ref lstUni);

                        CBase.SaveCodeFile(strNewCodeTB, lstUni);

                        MessageBox.Show("ok");
                    }
                }
                catch (Exception ext)
                {
                    MessageBox.Show(ext.ToString());
                }
            }

        }   // end of private void btnCodeRmRepeat_Click()
        
    }
}
