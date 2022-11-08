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
    public partial class CodeProcessWnd : Form
    {
        string m_strCodeFile1;
        string m_strCodeFile2;
        string m_strNwCodeFile;
        List<uint> m_lstUni1 = new List<uint>();
        List<uint> m_lstUni2 = new List<uint>();

        public CodeProcessWnd()
        {
            InitializeComponent();

        }   // end of CodeProcessWnd()

        private void btnSrc_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strCodeFile1 = FntfleDlg.FileName;
            }

            CBase.ReadCodeFile(m_strCodeFile1, ref m_lstUni1);

        }   // end of private void btnSrc_Click()

        private void btnSrc2_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strCodeFile2 = FntfleDlg.FileName;
            }

            CBase.ReadCodeFile(m_strCodeFile2, ref m_lstUni2);

        }   // end of private void btnSrc2_Click()

        private void btnNew_Click(object sender, EventArgs e)
        {
            

        }  // end of private void btnNew_Click()

        private void btnUnrepeate_Click(object sender, EventArgs e)
        {
            string strCodeFile;
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() != DialogResult.OK) return;

            strCodeFile = FntfleDlg.FileName;
            try
            {
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

        }   // end of private void btnUnrepeate_Click()

        /// <summary>
        /// 码表转字表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCode2Char_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strNwCodeFile = FntfleDlg.FileName;
            }

        }   // end of private void btnCode2Char_Click()

        /// <summary>
        /// 码表1-2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn1sub2_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strNwCodeFile = FntfleDlg.FileName;
            }

            List<uint> lstUni3 = new List<uint>();
            CBase.Unicode1sub2(ref m_lstUni1, ref m_lstUni2, ref lstUni3);
            CBase.SaveCodeFile(m_strNwCodeFile, lstUni3);

            MessageBox.Show("处理完成");

        }   // end of  private void btn1sub2_Click()

        /// <summary>
        /// 码表2-1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn2sub1_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strNwCodeFile = FntfleDlg.FileName;
            }

            List<uint> lstUni3 = new List<uint>();
            CBase.Unicode1sub2(ref m_lstUni2, ref m_lstUni1, ref lstUni3);
            CBase.SaveCodeFile(m_strNwCodeFile, lstUni3);
            MessageBox.Show("处理完成");

        }   // end of private void btn2sub1_Click()

        /// <summary>
        /// 码表1+2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn1sum2_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strNwCodeFile = FntfleDlg.FileName;
            }

            List<uint> lstUni3 = new List<uint>();
            CBase.Unicode1sum2(ref m_lstUni1, ref m_lstUni2, ref lstUni3);
            CBase.SaveCodeFile(m_strNwCodeFile, lstUni3);

            MessageBox.Show("处理完成");

        }   // end of  private void btn1sum2_Click()

        /// <summary>
        /// 码表1&2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn1and2_Click(object sender, EventArgs e)
        {
            OpenFileDialog FntfleDlg = new OpenFileDialog();
            FntfleDlg.Filter = "码表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            if (FntfleDlg.ShowDialog() == DialogResult.OK)
            {
                m_strNwCodeFile = FntfleDlg.FileName;
            }

            List<uint> lstUni3 = new List<uint>();
            CBase.Unicode1and2(ref m_lstUni1, ref m_lstUni2, ref lstUni3);
            CBase.SaveCodeFile(m_strNwCodeFile, lstUni3);

            MessageBox.Show("处理完成");

        }   // end of private void btn1and2_Click()
    }
}
