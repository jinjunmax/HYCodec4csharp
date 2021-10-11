using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HYFontCodecCS
{
    /// <summary>
    /// Unicode和汉字转换器
    /// </summary>
    public class UniChaConverter
    {
        /// <summary>
        /// 两字节汉字转unicode
        /// </summary>
        /// <param name="character">两字节汉字</param>
        /// <returns>unicode</returns>
        public static string Character2Unicode(string character)
        {
            string unicode = "";
            byte[] bytes = Encoding.BigEndianUnicode.GetBytes(character);
            for (int j = 0; j < bytes.Length; j++)
            {
                string s = bytes[j].ToString("X");
                if (s.Length == 1) unicode += @"0";
                unicode += s;
                if ((j%2) == 1) unicode += @" ";
            }
            return unicode.Trim();
        }

        /// <summary>
        /// unicode转两字节汉字
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns>两字节汉字</returns>
        public static string Unicode2Character(string unicode)
        {
            if (string.IsNullOrWhiteSpace(unicode) || unicode.Length != 4) return "";

            try
            {
                var bytes = new byte[2];
                string lowCode = unicode.Substring(0, 2); //取出低字节,并以16进制进制转换 
                bytes[1] = Convert.ToByte(lowCode, 16);
                string highCode = unicode.Substring(2, 2); //取出高字节,并以16进制进行转换 
                bytes[0] = Convert.ToByte(highCode, 16);
                return Encoding.Unicode.GetString(bytes);
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// unicode数组转两字节汉字串
        /// </summary>
        /// <param name="unicodes">unicode列表</param>
        /// <returns>两字节汉字串</returns>
        public static string Unicodes2Characters(List<string> unicodes)
        {
            return unicodes.Aggregate("", (current, unicode) => current + Unicode2Character(unicode.Trim()));
        }

        /// <summary>
        /// 两字节汉字串转十六进制Unicode列表
        /// </summary>
        /// <param name="charactars">两字节汉字串</param>
        /// <returns>十六进制Unicode列表</returns>
        public static List<string> Characters2HexUnicodes(string charactars)
        {
            if (string.IsNullOrWhiteSpace(charactars)) return null;
            var unicodes = new List<string>();
            foreach (char cha in charactars.ToList())
            {
                var uni = Character2Unicode(cha.ToString(CultureInfo.InvariantCulture));
                if (!unicodes.Contains(uni)) unicodes.Add(uni);
            }
            return unicodes;
        }

        /// <summary>
        /// 两字节汉字串转十进制Unicode列表
        /// </summary>
        /// <param name="charactars">两字节汉字串</param>
        /// <returns>十进制Unicode列表</returns>
        public static List<int> Characters2DecUnicodes(string charactars)
        {
            if (string.IsNullOrWhiteSpace(charactars)) return null;
            var unicodes = new List<int>();
            foreach (char cha in charactars.ToList())
            {
                var uni = Unicode2Unicode(Character2Unicode(cha.ToString(CultureInfo.InvariantCulture)));
                if (!unicodes.Contains(uni)) unicodes.Add(uni);
            }
            return unicodes;
        }

        /// <summary>
        /// 四字节汉字转unicode
        /// </summary>
        /// <param name="character">四字节汉字</param>
        /// <returns>unicode</returns>
        public static string Character4Bytes2Unicode(string character)
        {
            //Unicode(win)
            string unicodeWin = Character2Unicode(character);
            //Unicode(real)
            var unicodes = Character2Unicode(character).Split(' ');
            var bytes = new ushort[unicodes.Length];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = (ushort) Unicode2Unicode(unicodes[i]);
            uint ucs4;
            UnicodeConverter.UTF16ToUCS4(bytes, 0, out ucs4);
            return ucs4.ToString("X");
        }

        /// <summary>
        /// 四字节汉字串转十六进制Unicode列表
        /// </summary>
        /// <param name="characters">四字节汉字串</param>
        /// <returns>十六进制Unicode列表</returns>
        public static List<string> Character4Bytes2Unicodes(string characters)
        {
            if (string.IsNullOrWhiteSpace(characters)) return null;
            var unicodes = new List<string>();
            for (int i = 0; i < characters.Length; i += 2)
            {
                var uni = Character4Bytes2Unicode(characters.Substring(i, 2));
                if (!unicodes.Contains(uni)) unicodes.Add(uni);
            }
            return unicodes;
        }

        /// <summary>
        /// unicode转四字节汉字
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns>四字节汉字</returns>
        public static string Unicode2Character4Bytes(string unicode)
        {
            //Unicode(win)
            UInt16[] bytes = UnicodeConverter.UCS4ToUTF16(Convert.ToUInt32(unicode, 16));
            string unicode4 = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                byte[] buf = BitConverter.GetBytes(bytes[i]);
                unicode4 += string.Format("{0}{1} ", Convert.ToString(buf[1], 16), Convert.ToString(buf[0], 16));
            }
            string unicodeWin = unicode4.ToUpper();
            //汉字
            uint ucs4;
            UnicodeConverter.UTF16ToUCS4(bytes, 0, out ucs4);
            return UnicodeConverter.UCS4ToString(ucs4);
        }

        /// <summary>
        /// 同时支持两字节和四字节汉字转unicode
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static string AdCharacter2Unicode(string character)
        {
            return Character4Bytes2Unicode(character);
        }

        /// <summary>
        /// 同时支持两字节和四字节unicode转汉字
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns></returns>
        public static string AdUnicode2Character(string unicode)
        {
            if (string.IsNullOrWhiteSpace(unicode)) return "";

            if (unicode.Trim().Length == 4) return Unicode2Character(unicode);
            return Unicode2Character4Bytes(unicode);
        }

        /// <summary>
        /// 两字节和四字节混排文字转unicode数组
        /// </summary>
        /// <param name="characterString"> 两字节和四字节混排字符串</param>
        /// <returns></returns>
        public static List<string> AdCharacters2Unicodes(string characterString)
        {
            string unicodeString = "";
            string[] unicodes = Character2Unicode(characterString).Split(' ');
            for (int i = 0; i < unicodes.Length; i++)
            {
                int uniInt = Unicode2Unicode(unicodes[i]);
                string unicode;
                if (uniInt >= 55296 && uniInt <= 56319)
                {
                    unicode = Character4Bytes2Unicode(characterString.Substring(i, 2));
                    i++;
                }
                else
                {
                    unicode = Character2Unicode(characterString.Substring(i, 1));
                }
                if (!string.IsNullOrWhiteSpace(unicode)) unicodeString += unicode + " ";
            }
            return string.IsNullOrWhiteSpace(unicodeString) ? null : unicodeString.Trim().Split(' ').ToList();
        }

        /// <summary>
        /// 两字节和四字节混合unicode列表转文字
        /// </summary>
        /// <param name="unicodes">unicode列表</param>
        /// <returns>两字节和四字节混排文字</returns>
        public static string AdUnicodes2Characters(List<string> unicodes)
        {
            return unicodes.Aggregate("", (current, unicode) => current + AdUnicode2Character(unicode.Trim()));
        }

        /// <summary>
        /// 十六进制Unicode码转十进制Unicode码
        /// </summary>
        /// <param name="unicode">十六进制Unicode码</param>
        /// <returns>十进制Unicode码</returns>
        public static int Unicode2Unicode(string unicode)
        {
            if (unicode == null) return 0;
            return Int32.Parse(unicode, NumberStyles.HexNumber);
        }

        /// <summary>
        /// 十进制Unicode码转十六进制Unicode码
        /// </summary>
        /// <param name="unicode">十进制Unicode码</param>
        /// <returns>十六进制Unicode码(长度为4)</returns>
        public static string Unicode2Unicode(int unicode)
        {
            try
            {
                return Unicode2Unicode(unicode, 4);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// (overload)十进制Unicode码转十六进制Unicode码
        /// </summary>
        /// <param name="unicode">十进制Unicode码</param>
        /// <param name="length">十六进制Unicode码长度</param>
        /// <returns>十六进制Unicode码(长度为length)</returns>
        public static string Unicode2Unicode(int unicode, int length)
        {
            try
            {
                return Convert.ToString(unicode, 16).ToUpper().PadLeft(length, '0');
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 十六进制Unicode码列表转十进制Unicode码列表
        /// </summary>
        /// <param name="unicodes"></param>
        /// <returns></returns>
        public static List<int> Unicodes2Unicodes(List<string> unicodes)
        {
            return unicodes.Select(Unicode2Unicode).ToList();
        }

        /// <summary>
        /// 十进制Unicode码列表转十六进制Unicode码列表
        /// </summary>
        /// <param name="unicodes"></param>
        /// <returns></returns>
        public static List<string> Unicodes2Unicodes(List<int> unicodes)
        {
            return unicodes.Select(unicode => Unicode2Unicode(unicode)).ToList();
        }
    }
}
