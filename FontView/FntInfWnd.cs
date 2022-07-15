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
    public partial class FntInfWnd : Form
    {
        private HYDecode m_Font;
        public FntInfWnd()
        {
            InitializeComponent();

        }   // end of public FntInfWnd()

        public FntInfWnd(HYDecode decodeC)
        {
            InitializeComponent();
            m_Font = decodeC;

        }   // end of public FntInfWnd()
        
        private void FntInfWnd_Load(object sender, EventArgs e)
        {
            if (m_Font == null) return;

            int maxyGID = 0, minyGID = 0;
            int Ymax = 0, Ymin = 0,Xmax=0,Xmin=0;
            m_Font.BoundStringToInt(m_Font.GlyphChars.CharInfo[0].Section,
                  out Xmin, out Ymin, out Xmax, out Ymax);

            for (int i=1; i< m_Font.tbMaxp.numGlyphs; i++)
            {
                int xminTmp,yminTmp, xmaxTmp, ymaxTmp;

                m_Font.BoundStringToInt(m_Font.GlyphChars.CharInfo[i].Section,
                  out xminTmp, out yminTmp, out xmaxTmp, out ymaxTmp);
                
                if (yminTmp < Ymin)
                {
                    Ymin = yminTmp;
                    minyGID = i;
                }                
                if (ymaxTmp < Ymax)
                {
                    Ymax = ymaxTmp;
                    maxyGID = i;
                }
            }
            
            rTBFntInf.Text += "Y值最大字形GID = " + maxyGID.ToString() + ",Y = "+Ymin.ToString()+"\n";
            rTBFntInf.Text += "Y值最小字形GID = " + minyGID.ToString() + ",X = "+Xmin.ToString()+"\n";

            rTBFntInf.Text += "Hhea Ascender = " + m_Font.tbHhea.Ascender.ToString() + "\n";
            rTBFntInf.Text += "Hhea Descener = " + m_Font.tbHhea.Descender.ToString() + "\n";

        }   // end of private void FntInfWnd_Load()
    }
}
