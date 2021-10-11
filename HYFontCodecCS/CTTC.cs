using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace HYFontCodecCS
{
    public class CTTC : CFontManager
    {
        /// <summary>
        /// 基于afdk下的python脚本调用方式
        /// </summary>
        /// <param name="lstFontNames"></param>
        /// <returns></returns>
        public HYRESULT FontToTTCForAFDK_P(List<string> lstFontNames)
        {
            if (lstFontNames.Count < 1) return HYRESULT.FUNC_PARA;

            string strTTC = Path.GetDirectoryName(lstFontNames[0]) + Path.GetFileNameWithoutExtension(lstFontNames[0]) + ".ttc";

            try
            {
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                string strcmd = "otf2otc -o " + strTTC;
                for (int i = 0; i < lstFontNames.Count; i++)
                {
                    strcmd += " ";
                    strcmd += lstFontNames[i];
                }

                //scope.SetVariable("args", strcmd);               

                engine.SetSearchPaths(new[]{"C:\\Program Files (x86)\\IronPython 2.7\\Lib"});
                ScriptSource script = engine.CreateScriptSourceFromFile(@"Test.py");

                var result = script.Execute(scope);
            }
            catch (Exception e)
            {
                e.ToString();
                throw;
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontToTTCForAFDK_P()

        /// <summary>
        /// 基于afdk下的shell命令行调用方式
        /// </summary>
        /// <param name="lstFontNames"></param>
        /// <returns></returns>
        public HYRESULT FontToTTCForAFDK_C(List<string> lstFontNames)
        { 
            if (lstFontNames.Count < 1) return HYRESULT.FUNC_PARA;

            string strTTC = Path.GetDirectoryName(lstFontNames[0]) + Path.GetFileNameWithoutExtension(lstFontNames[0]) + ".ttc";
            string strcmd = "otf2otc -o " + strTTC;

            for (int i = 0; i < lstFontNames.Count; i++)
            {
                strcmd += " ";
                strcmd += lstFontNames[i];
            }

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(strcmd);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontToTTCForAFDK()

        public HYRESULT FontToTTC(List<string> lstFontNames)
        {
            HYRESULT hr;
            List<HYDecodeC> lstFonts = new List<HYDecodeC>();
            foreach (string strfile in  lstFontNames)
            {
                HYDecodeC fntDecode = new HYDecodeC();
                hr = fntDecode.FontDecode(strfile);
                if (hr != HYRESULT.NOERROR) return hr;
            }

            return HYRESULT.NOERROR;

        }   // end of public bool FontToTTC()

        public HYRESULT TTCToFonts(string strTTC)
        {
            try
            {
                UInt32 ulTmp;                
                byte[] btTmp = new byte[4];

                FileStream flOpen = File.Open(strTTC, FileMode.Open, FileAccess.Read);
                //TTCTag
                flOpen.Read(btTmp, 0, 4);
                ulTmp = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(btTmp, 0));
                if (ulTmp != 0x74746366)
                    return HYRESULT.NO_TTC;

                //Version
                flOpen.Read(btTmp, 0, 4);
                ulTmp = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(btTmp, 0));

                //numFonts
                flOpen.Read(btTmp, 0, 4);
                UInt32 uFontnums = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(btTmp, 0));

                long lFilePos = flOpen.Position;
                //OffsetTable
                for (UInt32 i = 0; i < uFontnums; i++)
                {
                    flOpen.Seek(lFilePos, SeekOrigin.Begin);
                    flOpen.Read(btTmp, 0, 4);
                    lFilePos = flOpen.Position;

                    long lTableOffset = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(btTmp, 0));

                    string strFontName = Path.GetDirectoryName(strTTC)
                                        + "\\"
                                        + Path.GetFileNameWithoutExtension(strTTC)
                                        + "_"
                                        + i.ToString();

                    if (!TTCToFont(flOpen, lTableOffset, ref strFontName))
                        return HYRESULT.TTC_TO_FONT;


                }
            }
            catch (Exception ext)
            {
                ext.ToString();
                throw;
            }

            return HYRESULT.NOERROR;


        }   // end of public bool TTCToFont()


        /// <summary>
        /// TTCToFont
        /// </summary>
        /// <param name="ttcFile"></param>
        /// <param name="loffsetTable">the OffsetTable for each font from the beginning of the file</param>
        /// <param name="strFontName"></param>
        /// <returns></returns>
        private bool TTCToFont(FileStream ttcFile, long loffsetTable, ref string strFontName)
        {
            byte[] byTmp = new byte[4];            

            HYEncode FontEncode = new HYEncode();
            FontEncode.tbDirectory = new CTableDirectory();
            
            ttcFile.Seek(loffsetTable, SeekOrigin.Begin);
            //Version;
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.version.value = (Int16)HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.version.fract = (UInt16)HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));

            uint version = (uint)(FontEncode.tbDirectory.version.value << 16 | FontEncode.tbDirectory.version.fract);
            if (version == 0x00010000)
                strFontName += ".ttf";
            else if (version == 0x4F54544F)
                strFontName += ".otf";
            else
                return false;

            //numTables
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.numTables = HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));
            //searchRange
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.searchRange = HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));
            //entrySelector
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.entrySelector = HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));
            //rangeShift
            ttcFile.Read(byTmp, 0, 2);
            FontEncode.tbDirectory.rangeShift = HYBase.hy_cdr_int16_to(BitConverter.ToUInt16(byTmp, 0));
            
            List<byte[]> vtTableData = new List<byte[]>();
            for (UInt32 i = 0; i < FontEncode.tbDirectory.numTables; i++)
            {
                CTableEntry tableEntry = new CTableEntry();
                //tag
                ttcFile.Read(byTmp, 0, 4);
                tableEntry.tag = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(byTmp, 0));
                //checkSum
                ttcFile.Read(byTmp, 0, 4);
                tableEntry.checkSum = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(byTmp, 0));
                //offset
                ttcFile.Read(byTmp, 0, 4);
                tableEntry.offset = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(byTmp, 0));
                //length
                ttcFile.Read(byTmp, 0, 4);
                tableEntry.length = HYBase.hy_cdr_int32_to(BitConverter.ToUInt32(byTmp, 0));

                FontEncode.tbDirectory.vtTableEntry.Add(tableEntry);

                long lCurPos = ttcFile.Position;

                byte[] tbData = new byte[tableEntry.length];
                ttcFile.Seek(tableEntry.offset,SeekOrigin.Begin);
                ttcFile.Read(tbData, 0, (int)tableEntry.length);
                vtTableData.Add(tbData);

                ttcFile.Seek(lCurPos,SeekOrigin.Begin);                
            }

            // 生成单个字体文件
            FontEncode.FontOpen(strFontName);
            FontEncode.EncodeTableDirectory();
            for (ushort i = 0; i < FontEncode.tbDirectory.numTables; i++)
            {
                CTableEntry tableEntry = FontEncode.tbDirectory.vtTableEntry[i];
                tableEntry.offset = (uint)FontEncode.EncodeStream.Position;

                byte[] tbData = vtTableData[i];
                FontEncode.EncodeStream.Write(tbData, 0, tbData.Length);
            
            }
            FontEncode.EncodeTableDirectory();
            FontEncode.FontClose();

                return true;

        }   // end of public static bool TTCToFonts()       

    }
}
