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

            string strunicode = File.ReadAllText(CharFile,Encoding.Unicode);            
            foreach (char element in strunicode)
            {
                if (element == '\n' || element == '\r') continue;
                
                UInt32 unicode = Convert.ToUInt32(element);
                lstUnicode.Add(unicode);            
            }

            return HYRESULT.NOERROR;

        }   // end of public static HYRESULT ReadCharFileToUnicode()

        
    }
}
