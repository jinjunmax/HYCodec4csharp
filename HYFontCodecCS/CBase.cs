using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace HYFontCodecCS
{
    public class CBase
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        public static string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public static bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ReadCharFileToUnicode
        /// </summary>
        /// <param name="CharFile"></param>
        /// <param name="lstUnicode"></param>
        /// <returns></returns>
        public static HYRESULT ReadCharFileToUnicode(string CharFile, ref List<UInt32> lstUnicode)
        {
            FileInfo flinfo = new FileInfo(CharFile);
            if (!flinfo.Exists) return HYRESULT.FILE_NOEXIST;

            string strunicode = File.ReadAllText(CharFile, Encoding.Unicode);
            foreach (char element in strunicode)
            {
                if (element == '\n' || element == '\r') continue;

                UInt32 unicode = Convert.ToUInt32(element);
                lstUnicode.Add(unicode);
            }

            return HYRESULT.NOERROR;

        }   // end of public static HYRESULT ReadCharFileToUnicode()

        /// <summary>
        /// 读取码表文件
        /// </summary>
        /// <param name="codeFile"></param>
        /// <param name="lstUnicode"></param>
        /// <returns></returns>
        public static HYRESULT ReadCodeFile(string codeFile, ref List<UInt32> lstUnicode)
        {
            lstUnicode.Clear();

            FileInfo flinfo = new FileInfo(codeFile);
            if (!flinfo.Exists) return HYRESULT.FILE_NOEXIST;

            foreach (string strunicode in File.ReadLines(codeFile))
            {
                string trim = strunicode.Trim();
                if (trim.Length == 0) continue;
                UInt32 unicode = Convert.ToUInt32(trim, 16);
                lstUnicode.Add(unicode);
            }

            return HYRESULT.NOERROR;

        }   // end of public static HYRESULT ReadCharFileToUnicode()

        /// <summary>
        /// 保存码表
        /// </summary>
        /// <param name="codeFile"></param>
        /// <param name="lstUnicode"></param>
        /// <returns></returns>
        public static void SaveCodeFile(string codeFile, List<UInt32> lstUnicode)
        {
            if (!File.Exists(codeFile))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(codeFile))
                {
                    foreach (UInt32 uni in lstUnicode)
                    {
                        sw.WriteLine(uni.ToString("X"));
                    }
                }
            }

        }   // end of public static void SaveCodeFile()

        /// <summary>
        /// unicode 去重
        /// </summary>
        /// <param name="lstUnicode"></param>
        public static void UnicodeUnrepeat(ref List<UInt32> lstUnicode)
        {
            if (lstUnicode.Count == 0) return;

            UInt32[] Tmp = new UInt32[lstUnicode.Count];
            lstUnicode.CopyTo(Tmp);
            lstUnicode.Clear();

            foreach (UInt32 uni in Tmp)
            {
                if (lstUnicode.Count == 0)
                {
                    lstUnicode.Add(uni);
                }
                else
                {
                    bool b = true;
                    foreach (UInt32 uni1 in lstUnicode)
                    {
                        if (uni1 == uni)
                        {
                            b = false;
                            break;
                        }
                    }

                    if (b)
                    {
                        lstUnicode.Add(uni);
                    }
                }
            }

        }   // end of public static void UnicodeUnrepeat()

        public static void Unicode1sub2(ref List<UInt32> lstUnicode1, ref List<UInt32> lstUnicode2, ref List<UInt32> outlstUnicode)
        {
            outlstUnicode.Clear();

            foreach (UInt32 uni1 in lstUnicode1)
            {
                bool b = true;
                foreach (UInt32 uni2 in lstUnicode2)
                {
                    if (uni1 == uni2)
                    {
                        b = false;
                        break;
                    }
                }

                if (b)
                {
                    outlstUnicode.Add(uni1);
                }
            }

        }   // end of public static void Unicode1sub2()

        public static void Unicode1sum2(ref List<UInt32> lstUnicode1, ref List<UInt32> lstUnicode2, ref List<UInt32> outlstUnicode)
        {
            outlstUnicode.Clear();
            outlstUnicode.AddRange(lstUnicode1);

            foreach (uint code2 in lstUnicode2)
            {
                bool b = true;
                foreach (uint code1 in lstUnicode1)
                {
                    if (code2 == code1)
                    {
                        b = false;
                    }
                }

                if (b)
                {
                    outlstUnicode.Add(code2);
                }
            }

        }   // end of public static void Unicode1sum2()

        public static void Unicode1and2(ref List<UInt32> lstUnicode1, ref List<UInt32> lstUnicode2, ref List<UInt32> outlstUnicode)
        {
            outlstUnicode.Clear();
            foreach (uint code1 in lstUnicode1)
            {
                bool b = true;
                foreach (uint code2 in lstUnicode2)
                {
                    if (code1 == code2)
                    {
                        b = false;
                    }
                }
                if (!b)
                {
                    outlstUnicode.Add(code1);
                }
            }

        }// end of public static void Unicode1and2()
    }
}
