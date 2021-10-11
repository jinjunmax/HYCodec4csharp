using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HYFontCodecCS
{
    public class CFontManager : HYFontBase
    {
        public class GroupRecord
        {
            public uint startCode;
            public uint endCode;
            public int startGID;
            public bool contiguous;
        };

        public class GlyfData
		{			
			public List<byte>			lstData = new List<byte>();
			public uint					DataLen;
			public List<uint>		    lstUnicode = new List<uint>();
			public ushort				usAdvWidth;
			public short				sLsb;
			public ushort				usAdvHeight;
			public short				sTsb;
            public string               name;
		};      

        Dictionary<ushort, ushort> ModifiedVMTX = new Dictionary<ushort, ushort>();        
        
        /// <summary>
        /// 检测unicode是否在链表中
        /// </summary>
        /// <param name="UInt32 ulUnicod">待检测unicode</param>
        /// <param name="List<UInt32> lstUnicode">unicode链表</param>
        /// <returns></returns>
		public Boolean CheckUniRepeat(uint ulUnicod, List<uint> lstUnicode)
        {
            foreach (uint ulUni in lstUnicode)
		    {                
			    if (ulUni == ulUnicod) return true;
		    }

		    return false;

        }   // end of public int CheckUniRepeat()

        /// <summary>
        /// 提取子字库
        /// </summary>
        /// <param name="string strTTFFile">原始字库名称</param>
        /// <param name="string strNewTTFFile">新字库名称</param>
        /// <param name="List<UInt32> lst">待提取的unicode编码</param>
        /// <param name="int CMAP">CMAP表处理方式 0保持原有CMAP表 1重新编码CMAP表</param>
        /// <returns>HYRESULT</returns> 
        public HYRESULT ExtractFont(string strTTFFile, string strNewTTFFile, List<UInt32> lstunicode, int CMAP,ref List<UInt32> lstMssUni)
        {
            HYRESULT sult;
            HYDecodeC FontDecode = new HYDecodeC();            
            try {
                FontDecode.FontOpen(strTTFFile);
            }
            catch (Exception ext){
                ext.ToString();
                throw;
            }

            HYEncode FontEncode = new HYEncode();
            sult = FontDecode.DecodeTableDirectory();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }

            if (FontDecode.tbDirectory.version.value != 1 && FontDecode.tbDirectory.version.fract != 0)
            {
                FontDecode.FontClose();
                return HYRESULT.NO_TTF;	// 不是truetype            
            }
            sult = FontDecode.DecodeCmap();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }

            FontEncode.tbCmap = FontDecode.tbCmap;
            sult = FontDecode.DecodeMaxp();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbMaxp = FontDecode.tbMaxp;
            sult = FontDecode.DecodeHead();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbHead = FontDecode.tbHead;
            sult = FontDecode.DecodeLoca();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbLoca = FontDecode.tbLoca;
            sult = FontDecode.DecodeHhea();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbHhea = FontDecode.tbHhea;
            sult = FontDecode.DecodeHmtx();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbHmtx = FontDecode.tbHmtx;
            sult = FontDecode.DecodePost();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }

            FontDecode.DecodeCOLR();

            FontEncode.tbPost = FontDecode.tbPost;
            FontEncode.tbPost.version.value = 3;
            FontEncode.tbPost.version.fract = 0;

            // 为了压缩webfont的传输数据量，只保留字库格式强制要求必须保留的八个表
            int TableIndex = -1;            
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VHEA_TAG);
            if (TableIndex != -1)
            {                
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VMTX_TAG);
            if (TableIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            } 
            // 抽取子字库后会出现字符缺失或字序打乱，导致GSUB映射错误，所以这个需要删除
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GSUB_TAG);
            if (TableIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }            
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GPOS_TAG);
            if (TableIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }
            //这个在抽取字库时，如果不删掉会无法在webfont上显示（可能）
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VDMX_TAG);
            if (TableIndex != -1)
            {                
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }
            //这个在抽取字库时，如果不删掉会无法在webfont上显示（必现）
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.HDMX_TAG);
            if (TableIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }
            //在子字库中没有意义
            TableIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.LTSH_TAG);
            if (TableIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TableIndex);
                FontDecode.tbDirectory.numTables--;
            }
            
            List<byte> lstGlyphData = new List<byte>();
            List<uint> vtTableFlag = new List<uint>();

            // 只抽取字形，但是不清除码位，可以保持其它表不变。
            if (CMAP == 0)
            {
                try
                {
                    Extrace(FontDecode, ref FontEncode, lstunicode, ref lstGlyphData);

                    vtTableFlag.Add((uint)TABLETAG.LOCA_TAG);
                    vtTableFlag.Add((uint)TABLETAG.GLYF_TAG);
                    vtTableFlag.Add((uint)TABLETAG.HEAD_TAG);
                    vtTableFlag.Add((uint)TABLETAG.MAXP_TAG);
                    vtTableFlag.Add((uint)TABLETAG.POST_TAG);                    

                    BulidFont(strNewTTFFile, FontDecode, FontEncode, lstGlyphData, vtTableFlag, lstunicode);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    FontDecode.FontClose();
                    throw;
                }
            }
            // 抽取字形，并且清除码位，包括HMTXT等都需要重新处理。
            if (CMAP == 1)
            {
                try
                {
                    sult = Extrace2(FontDecode, ref FontEncode, lstunicode, ref lstGlyphData, ref lstMssUni);
                    if (sult != HYRESULT.NOERROR)
                    {
                        FontDecode.FontClose();
                        return sult;
                    }

                    vtTableFlag.Add((uint)TABLETAG.LOCA_TAG);
                    vtTableFlag.Add((uint)TABLETAG.GLYF_TAG);
                    vtTableFlag.Add((uint)TABLETAG.HEAD_TAG);
                    vtTableFlag.Add((uint)TABLETAG.MAXP_TAG);
                    vtTableFlag.Add((uint)TABLETAG.POST_TAG);
                    vtTableFlag.Add((uint)TABLETAG.HHEA_TAG);
                    vtTableFlag.Add((uint)TABLETAG.HMTX_TAG);
                    vtTableFlag.Add((uint)TABLETAG.CMAP_TAG);

                    if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.COLR_TAG) != -1)
                        vtTableFlag.Add((uint)TABLETAG.COLR_TAG);


                    sult = BulidFont(strNewTTFFile, FontDecode, FontEncode, lstGlyphData, vtTableFlag, lstunicode);
                    if (sult != HYRESULT.NOERROR)
                    {
                        FontDecode.FontClose();
                        return sult;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    FontDecode.FontClose();
                    throw;                
                }
            }

            FontDecode.FontClose();
            return HYRESULT.NOERROR;

        }   // end of HYRESULT ExtractFont()
        public HYRESULT ModifyFontInfo(string strFileName, string strNewFileName,HYFontInfo Fntinf)
        {
            HYRESULT sult;
            HYDecodeC FontDecode = new HYDecodeC();            
        
            try
            {
                FontDecode.FontOpen(strFileName);
            }
            catch (Exception ext)
            {
                ext.ToString();
                return HYRESULT.FILE_OPEN;
            }

            HYEncode FontEncode = new HYEncode();
            sult = FontDecode.DecodeTableDirectory();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }            
            
            sult = FontDecode.DecodeHead();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbHead = FontDecode.tbHead;
            if (Fntinf.Head_XMax != 0) FontEncode.tbHead.xMax = Fntinf.Head_XMax;
            if (Fntinf.Head_XMin!= 0) FontEncode.tbHead.xMin = Fntinf.Head_XMin;
            if (Fntinf.Head_YMax != 0) FontEncode.tbHead.yMax = Fntinf.Head_YMax;
            if (Fntinf.Head_YMin != 0) FontEncode.tbHead.yMin = Fntinf.Head_YMin;

            sult = FontDecode.DecodeHhea();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbHhea = FontDecode.tbHhea;
            if (Fntinf.HHEA_Asc != 0) FontEncode.tbHhea.Ascender = Fntinf.HHEA_Asc;
            if (Fntinf.HHEA_Des != 0) FontEncode.tbHhea.Descender = Fntinf.HHEA_Des;

            sult = FontDecode.DecodeOS2();
            if (sult != HYRESULT.NOERROR)
            {
                FontDecode.FontClose();
                return sult;
            }
            FontEncode.tbOS2 = FontDecode.tbOS2;
            if (Fntinf.OS_TypoAsc != 0) FontEncode.tbOS2.sTypoAscender = Fntinf.OS_TypoAsc;
            if (Fntinf.OS_TypoDes != 0) FontEncode.tbOS2.sTypoDescender = Fntinf.OS_TypoDes;
            if (Fntinf.OS_WinAsc != 0) FontEncode.tbOS2.usWinAscent = Fntinf.OS_WinAsc;
            if (Fntinf.OS_WinDes != 0) FontEncode.tbOS2.usWinDescent = Fntinf.OS_WinDes;

            List<byte[]> vtTableData = new List<byte[]>();
            List<UInt32> vtTableFlag = new List<UInt32>();
            for (int i = 0; i < FontDecode.tbDirectory.numTables; i++)
            {
                CTableEntry tableEntry = FontDecode.tbDirectory.vtTableEntry[i];

                uint iLength = (tableEntry.length + 3) / 4 * 4;                
                byte[] tbData = new byte[iLength];
                
                FontDecode.DecodeStream.Seek(tableEntry.offset, SeekOrigin.Begin);
                FontDecode.DecodeStream.Read(tbData, 0, (int)tableEntry.length);
                vtTableData.Add(tbData);
                vtTableFlag.Add(tableEntry.tag);
            }
            FontDecode.FontClose();

            //写入新文件
            try
            {
                if (File.Exists(strNewFileName))
                {
                    File.Delete(strNewFileName);
                }

                FontEncode.FontOpen(strNewFileName);
            }
            catch (Exception ext)
            {
                ext.ToString();
                return HYRESULT.FILE_OPEN;
            }

            FontEncode.FntType = FontDecode.FntType;
            FontEncode.tbDirectory = FontDecode.tbDirectory;
            FontEncode.EncodeTableDirectory();

            for (int i = 0; i != FontDecode.tbDirectory.vtTableEntry.Count; i++)
            {
                CTableEntry HYDecodeEntry = FontDecode.tbDirectory.vtTableEntry[i];

                if (HYDecodeEntry.tag == (uint)TABLETAG.HEAD_TAG)
                {
                    //FontEncode.tbHead = FontEncode.tbHead;
                    FontEncode.EncodeheadEx();
                }
                else if (HYDecodeEntry.tag == (uint)TABLETAG.HHEA_TAG)
                {
                    FontEncode.EncodehheaEx();
                }
                else if (HYDecodeEntry.tag == (uint)TABLETAG.OS2_TAG)
                {
                    FontEncode.EncodeOS2Ex();
                }
                else
                {
                    byte[] TabelData = vtTableData[i];
                    int iEncodeEntryIndex = FontEncode.tbDirectory.FindTableEntry((UInt32)HYDecodeEntry.tag);
                    CTableEntry tbEncodeEntry = FontEncode.tbDirectory.vtTableEntry[iEncodeEntryIndex];

                    tbEncodeEntry.offset = (uint)FontEncode.EncodeStream.Position;
                    FontEncode.EncodeStream.Write(TabelData, 0, TabelData.Length);
                }
            }
            
            FontEncode.EncodeStream.Flush();
            for (int i = 0; i < FontEncode.tbDirectory.numTables; i++)
            {
                CTableEntry HYEntry = FontEncode.tbDirectory.vtTableEntry[i];
                uint CheckBufSz = (HYEntry.length + 3) / 4 * 4;
                byte[] pCheckBuf = new byte[CheckBufSz];
                FontEncode.EncodeStream.Seek(HYEntry.offset, SeekOrigin.Begin);
                FontEncode.EncodeStream.Read(pCheckBuf, 0, (int)CheckBufSz);

                HYEntry.checkSum = FontEncode.CalcFontTableChecksum(pCheckBuf);
            }
            
            FontEncode.EncodeTableDirectory();
            FontEncode.FontClose();

            return FontEncode.SetCheckSumAdjustment(strNewFileName);

        }   // end of HYRESULT ModifyFontInfo()

        HYRESULT Extrace(HYDecodeC FontDecode, ref HYEncode FontEncode, List<uint> lstUnicode, ref List<byte> lstGlyphData)
        {
            int iEntryIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GLYF_TAG);
            CTableEntry entry = FontDecode.tbDirectory.vtTableEntry[iEntryIndex];
            FontDecode.DecodeStream.Seek(entry.offset, SeekOrigin.Begin);           

            List<int> vtExtraceGid = new List<int>();
            lstGlyphData.Clear();
            int offset = 0;            

            // miss char  必须存在
            FontEncode.tbLoca = new CLoca();
            FontEncode.tbLoca.vtLoca.Add((uint)offset);
            vtExtraceGid.Add(0);

            for (int i = 0; i < lstUnicode.Count; i++)
            {
                int iIndex = FontDecode.FindGryphIndexByUnciode(lstUnicode[i]);
                if (!CheckGidRepeat(vtExtraceGid, iIndex))
                {
                    vtExtraceGid.Add(iIndex);                    
                }
            }

            uint GlyphDataLen = FontDecode.tbLoca.vtLoca[1] - FontDecode.tbLoca.vtLoca[0];
            byte[] btmp = new byte[GlyphDataLen];
            FontDecode.DecodeStream.Seek(entry.offset+ FontDecode.tbLoca.vtLoca[0], SeekOrigin.Begin);
            FontDecode.DecodeStream.Read(btmp,0,(int)GlyphDataLen);
            lstGlyphData.InsertRange(offset, btmp.ToList());
            offset += (int)GlyphDataLen;

            // 拷贝对应的GlyphData
            for (int i = 1; i < FontDecode.tbMaxp.numGlyphs; i++)            
            {
                // 如果i不在提取Indexlist中，不做任何操作
                if (!CheckGidRepeat(vtExtraceGid, i))
                {
                    FontEncode.tbLoca.vtLoca.Add((uint)offset);
                }
                // 如果i在提取Indexlist中，需要把字形数据提取出来
                else                                
                {                    
                    FontEncode.tbLoca.vtLoca.Add((uint)offset);
                    GlyphDataLen = FontDecode.tbLoca.vtLoca[i + 1] - FontDecode.tbLoca.vtLoca[i];

                    btmp = new byte[GlyphDataLen];
                    FontDecode.DecodeStream.Seek(entry.offset + FontDecode.tbLoca.vtLoca[i], SeekOrigin.Begin);
                    FontDecode.DecodeStream.Read(btmp, 0, (int)GlyphDataLen);

                    lstGlyphData.InsertRange(offset, btmp.ToList());
                    offset += (int)GlyphDataLen;
                }
            }
            FontEncode.tbLoca.vtLoca.Add((uint)offset);
            FontEncode.tbMaxp = FontDecode.tbMaxp;
           
            return HYRESULT.NOERROR;

        }   // end of HYRESULT Extrace()

        HYRESULT Extrace1(HYDecodeC FontDecode, ref HYEncode FontEncode, List<uint> lstUnicode, ref List<byte> lstGlyphData)
        {            
            int iEntryIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GLYF_TAG);
            CTableEntry entry = FontDecode.tbDirectory.vtTableEntry[iEntryIndex];
            FontDecode.DecodeStream.Seek(entry.offset, SeekOrigin.Begin);

            List<int> vtExtraceGid = new List<int>();
            List<HMTX_LONGHORMERTRIC>		vtExtraceHMTX = new List<HMTX_LONGHORMERTRIC>();            
            lstGlyphData.Clear();
            int offset = 0;

            // miss char  必须存在
            HMTX_LONGHORMERTRIC tmpHMTXItem;            

            FontEncode.tbLoca = new CLoca();
            FontEncode.tbLoca.vtLoca.Add((uint)offset);
            vtExtraceGid.Add(0);

            tmpHMTXItem = FontDecode.tbHmtx.lstLonghormetric[0];
            vtExtraceHMTX.Add(tmpHMTXItem);           

            // 保存MISSChar的数据
            uint GlyphDataLen = FontDecode.tbLoca.vtLoca[1] - FontDecode.tbLoca.vtLoca[0];
            byte[] btmp = new byte[GlyphDataLen];
            FontDecode.DecodeStream.Seek(entry.offset + FontDecode.tbLoca.vtLoca[0], SeekOrigin.Begin);
            FontDecode.DecodeStream.Read(btmp, 0, (int)GlyphDataLen);
            lstGlyphData.InsertRange(offset, btmp.ToList());
            offset += (int)GlyphDataLen;

            // 提取字形数据
            List<uint> lstUnicodeTmp = new List<uint>();
            for (int i = 0; i < lstUnicode.Count; i++)
            {
                int iIndex = FontDecode.FindGryphIndexByUnciode(lstUnicode[i]);
                if (iIndex != -1)
                {
                    // 过滤掉字库中不存在的unicode
                    lstUnicodeTmp.Add(lstUnicode[i]);                    
                
                    if (!CheckGidRepeat(vtExtraceGid, iIndex))
                    {
                        // 提取HMTX item项
                        vtExtraceGid.Add(iIndex);
                        vtExtraceHMTX.Add(FontDecode.tbHmtx.lstLonghormetric[iIndex]);                       

                        // 计算loce和数据长度
                        FontEncode.tbLoca.vtLoca.Add((uint)offset);
                        GlyphDataLen = FontDecode.tbLoca.vtLoca[iIndex + 1] - FontDecode.tbLoca.vtLoca[iIndex];
                        btmp = new byte[GlyphDataLen];
                        FontDecode.DecodeStream.Seek(entry.offset + FontDecode.tbLoca.vtLoca[iIndex], SeekOrigin.Begin);
                        FontDecode.DecodeStream.Read(btmp, 0, (int)GlyphDataLen);                      

                        lstGlyphData.InsertRange(offset, btmp.ToList());

                        offset += (int)GlyphDataLen;
                    }
                }
            }
            FontEncode.tbLoca.vtLoca.Add((uint)offset);

            if (lstUnicode.Count != lstUnicodeTmp.Count)
            {
                lstUnicode = lstUnicodeTmp;
            }

            if (lstUnicode.Count == 0) return HYRESULT.EXTRACT_ZERO; 
            
            //HHEA HMTX
            FontEncode.tbHhea = FontDecode.tbHhea;
		    int iHMTXNum = vtExtraceHMTX.Count;
		    if (iHMTXNum>0)
		    {
			    FontEncode.tbHhea.numberOfHMetrics = 0;
			    int				iBaseadvanceWidth = vtExtraceHMTX[--iHMTXNum].advanceWidth;		
			    while(--iHMTXNum>=0)
			    {				
				    if(vtExtraceHMTX[iHMTXNum].advanceWidth == iBaseadvanceWidth)
					    FontEncode.tbHhea.numberOfHMetrics++;
				    else 
					    break;
			    }
                FontEncode.tbHhea.numberOfHMetrics = (ushort)(vtExtraceHMTX.Count - FontEncode.tbHhea.numberOfHMetrics);
			    FontEncode.tbHmtx.lstLonghormetric= vtExtraceHMTX;			
		    }
           
            //CMAP
            FontEncode.codemap = new HYCodeMap();
            HYCodeMapItem mapItm = new HYCodeMapItem();
            mapItm.Unicode = 0xffff;
            mapItm.GID = 0;
            FontEncode.codemap.lstCodeMap.Add(mapItm);
            int stUniNum = lstUnicode.Count;
            for (int i = 0; i < stUniNum; i++)
            {
                mapItm = new HYCodeMapItem();
                mapItm.Unicode = lstUnicode[i];
                mapItm.GID = i + 1;
                FontEncode.codemap.lstCodeMap.Add(mapItm);
            }
            FontEncode.codemap.QuickSortbyUnicode();
            FontEncode.tbMaxp.numGlyphs = (ushort)vtExtraceGid.Count;

            return HYRESULT.NOERROR;

        }   // end of HYRESULT Extrace1()


        HYRESULT Extrace2(HYDecodeC FontDecode, ref HYEncode FontEncode, List<uint> lstUnicode, ref List<byte> lstGlyphData,ref List<UInt32> lstMssUni)
        {

            int iEntryIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GLYF_TAG);
            CTableEntry entry = FontDecode.tbDirectory.vtTableEntry[iEntryIndex];
            FontDecode.DecodeStream.Seek(entry.offset, SeekOrigin.Begin);
            
            Dictionary<int, int> cmpsMAP = new Dictionary<int, int>();
            List<HMTX_LONGHORMERTRIC> vtExtraceHMTX = new List<HMTX_LONGHORMERTRIC>();
            lstGlyphData.Clear();
            int offset = 0;

            // miss char  必须存在            
            FontEncode.tbLoca = new CLoca();
            FontEncode.tbLoca.vtLoca.Add((uint)offset);
            cmpsMAP.Add(0,0);

            HMTX_LONGHORMERTRIC tmpHMTXItem;
            tmpHMTXItem = FontDecode.tbHmtx.lstLonghormetric[0];
            vtExtraceHMTX.Add(tmpHMTXItem);

            // 保存MISSChar的数据
            uint GlyphDataLen = FontDecode.tbLoca.vtLoca[1] - FontDecode.tbLoca.vtLoca[0];
            byte[] btmp = new byte[GlyphDataLen];
            FontDecode.DecodeStream.Seek(entry.offset + FontDecode.tbLoca.vtLoca[0], SeekOrigin.Begin);
            FontDecode.DecodeStream.Read(btmp, 0, (int)GlyphDataLen);
            lstGlyphData.InsertRange(offset, btmp.ToList());
            offset += (int)GlyphDataLen;

             // 提取字形数据
            lstMssUni.Clear();
            List<uint> lstUnicodeTmp = new List<uint>();
            CHYCOLR	ExtraceCOLR = new CHYCOLR();
            int iMaxGlypht=0;
            for (int i = 0; i < lstUnicode.Count; i++)
            {                
                int iIndex = FontDecode.FindGryphIndexByUnciode(lstUnicode[i]);
                if (iIndex != -1)
                {
                    iMaxGlypht++;
                    lstUnicodeTmp.Add(lstUnicode[i]);
                    BuildGlyphData( FontDecode, 
                                    FontEncode, 
                                    ref lstGlyphData, 
                                    iIndex,
                                    ref FontEncode.tbLoca.vtLoca,
                                    ref offset,
                                    ref vtExtraceHMTX,
                                    ref cmpsMAP
                                    );

                    // make新的COLR
			        CBaseGlyphRecord	tmpBaseGlyphRecord = new CBaseGlyphRecord();
			        List<CLayerRecord> tmpvtlayerReord = new List<CLayerRecord>();

			        if ( FontDecode.tbCOLR.FindBaseGlyhRecord(iIndex,ref tmpBaseGlyphRecord))
			        {
				        FontDecode.tbCOLR.FindLayerRecord(iIndex,ref tmpvtlayerReord);

                        // GID =0是misschar这里先不考虑彩色misschar的问题
				        tmpBaseGlyphRecord.GID = (ushort)lstUnicodeTmp.Count;	

				        tmpBaseGlyphRecord.firstLayerIndex = (ushort)ExtraceCOLR.lstLayerRecord.Count;
				        for(int j=0; j<tmpvtlayerReord.Count;j++)
				        {
					        CLayerRecord tmpLayerRecord = tmpvtlayerReord[j];

					        tmpLayerRecord.GID = (ushort)lstUnicodeTmp.Count;
					        ExtraceCOLR.lstLayerRecord.Add(tmpLayerRecord);
				        }				
				        ExtraceCOLR.lstBaseGlyphRecord.Add(tmpBaseGlyphRecord);

			        }
                }
                else
                {
                    lstMssUni.Add(lstUnicode[i]);       
                }
            }

            ExtraceCOLR.numBaseGlyphRecords = (ushort)ExtraceCOLR.lstBaseGlyphRecord.Count;
            ExtraceCOLR.numLayerRecords = (ushort)ExtraceCOLR.lstLayerRecord.Count;
            FontEncode.tbCOLR = ExtraceCOLR;

            FontEncode.tbLoca.vtLoca.Add((uint)offset);

            if (lstUnicode.Count != lstUnicodeTmp.Count)
            {
                lstUnicode = lstUnicodeTmp;
            }

            if (lstUnicode.Count == 0) return HYRESULT.EXTRACT_ZERO;

            //HHEA HMTX
            FontEncode.tbHhea = FontDecode.tbHhea;
            int iHMTXNum = vtExtraceHMTX.Count;
            if (iHMTXNum > 0)
            {
                FontEncode.tbHhea.numberOfHMetrics = 0;
                int iBaseadvanceWidth = vtExtraceHMTX[--iHMTXNum].advanceWidth;
                while (--iHMTXNum >= 0)
                {
                    if (vtExtraceHMTX[iHMTXNum].advanceWidth == iBaseadvanceWidth)
                        FontEncode.tbHhea.numberOfHMetrics++;
                    else
                        break;
                }
                FontEncode.tbHhea.numberOfHMetrics = (ushort)(vtExtraceHMTX.Count - FontEncode.tbHhea.numberOfHMetrics);
                FontEncode.tbHmtx.lstLonghormetric = vtExtraceHMTX;
            }

            //CMAP
            FontEncode.codemap = new HYCodeMap();
            int iDst = 0;
            foreach (int srcGID in cmpsMAP.Keys)
            {
                List<uint> uni = new List<uint>();
                FontDecode.FindGryphUncidoByIndex(srcGID, ref uni);

                int desGID = 0;
                cmpsMAP.TryGetValue(srcGID, out desGID);
                for (int i=0; i<uni.Count; i++)
                {
                   HYCodeMapItem mapItm = new HYCodeMapItem();
                   mapItm.Unicode =uni[i];                   
                   mapItm.GID = desGID;
                   FontEncode.codemap.lstCodeMap.Add(mapItm);
                }
                iDst++;
            }

            FontEncode.codemap.QuickSortbyUnicode();
            FontEncode.tbMaxp.numGlyphs = (ushort)cmpsMAP.Count;
           

            return HYRESULT.NOERROR;

        }   // end of HYRESULT Extrace2()

        bool BuildGlyphData(
                            HYDecodeC FontDecode, 
                            HYEncode FontEncode,
                            ref List<byte> lstGlyphData,
                            int GID,
                            ref List<uint> vtloca, 
                            ref int  offset,                            
                            ref List<HMTX_LONGHORMERTRIC> vtExtraceHMTX,
                            ref Dictionary<int,int> cmpsMAP
                            )
        {
            // 判断是否有重复字形
            if (!cmpsMAP.ContainsKey(GID))
            {   
                uint GlyphDataLen = FontDecode.tbLoca.vtLoca[GID + 1] - FontDecode.tbLoca.vtLoca[GID];
                byte[] btmp = new byte[GlyphDataLen];

                int iEntryIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GLYF_TAG);
                CTableEntry entry = FontDecode.tbDirectory.vtTableEntry[iEntryIndex];
                FontDecode.DecodeStream.Seek(entry.offset + FontDecode.tbLoca.vtLoca[GID], SeekOrigin.Begin);
                FontDecode.DecodeStream.Read(btmp, 0, (int)GlyphDataLen);

                if (GlyphDataLen > 0)
                {
                    // 判断是不是空的字
                    short numberContours = (short)(btmp[0] << 8);
                    numberContours |= btmp[1];
                    if (numberContours == -1)
                    {
                        int iDataIndex = 10;
                        int ivalue = 0;
                        int flags = 0;
                        do
                        {
                            //flags 
                            flags = btmp[iDataIndex++] << 8;
                            flags |= btmp[iDataIndex++];

                            // 找到组件中映射的GID
                            int cmpIndex = btmp[iDataIndex++] << 8;
                            cmpIndex |= btmp[iDataIndex];

                            // 如果已经提取了映射中的GID，只需要修改映射的GID
                            if (cmpsMAP.ContainsKey(GID))
                            {
                                cmpsMAP.TryGetValue(GID, out ivalue);
                                btmp[iDataIndex - 1] = (byte)((ivalue & 0x0000ff00) >> 8);
                                btmp[iDataIndex] = (byte)(ivalue);
                            }
                            else
                            {
                                BuildGlyphData(FontDecode,
                                                FontEncode,
                                                ref lstGlyphData,
                                                cmpIndex,
                                                ref vtloca,
                                                ref offset,
                                                ref vtExtraceHMTX,
                                                ref cmpsMAP);


                                cmpsMAP.TryGetValue(cmpIndex, out ivalue);
                                btmp[iDataIndex - 1] = (byte)((ivalue & 0x0000ff00) >> 8);
                                btmp[iDataIndex] = (byte)(ivalue);
                            }                          

                            iDataIndex++;
                            //
                            if ((flags & 0x0001) > 0)//ARG_1_AND_2_ARE_WORDS
                            {
                                iDataIndex += 4;
                            }
                            else
                            {
                                iDataIndex += 2;
                            }

                            if ((flags & 0x0008) > 0)//WE_HAVE_A_SCALE
                            {
                                iDataIndex += 2;
                            }
                            else if ((flags & 0x0040) > 0)//WE_HAVE_AN_X_AND_Y_SCALE
                            {
                                iDataIndex += 4;
                            }
                            else if ((flags & 0x0080) > 0)//WE_HAVE_A_TWO_BY_TWO 
                            {
                                iDataIndex += 8;
                            }
                        } while ((flags & 0x0020) > 0);
                    }
                }
                // 计算loce和数据长度
                FontEncode.tbLoca.vtLoca.Add((uint)offset);
                lstGlyphData.InsertRange(offset, btmp.ToList());

                offset += (int)GlyphDataLen;

                cmpsMAP.Add(GID, FontEncode.tbLoca.vtLoca.Count - 1);
                vtExtraceHMTX.Add(FontDecode.tbHmtx.lstLonghormetric[GID]);
            }

            return true;

        }   // end of bool BuildGlyphData()

        HYRESULT BulidFont(string strFontName, HYDecodeC FontDecode, HYEncode FontEncode, List<byte> lstGlyphData, List<uint> vtTableFlag,List<uint> lstUnicode)
        {            
            try
            {
                if (File.Exists(strFontName))
                {
                    File.Delete(strFontName);
                }
                
                FontEncode.FontOpen(strFontName);
            }
            catch(Exception ext)
            {
                ext.ToString();
                throw;
            }

            FontEncode.FntType = FontDecode.FntType;
            FontEncode.tbDirectory = FontDecode.tbDirectory;
            FontEncode.EncodeTableDirectory();            

            for (int i = 0; i != FontDecode.tbDirectory.vtTableEntry.Count; i++)
            { 
                CTableEntry HYDecodeEntry = FontDecode.tbDirectory.vtTableEntry[i];
                if (HitTable(vtTableFlag, HYDecodeEntry.tag))
                {
                    if (HYDecodeEntry.tag == (uint)TABLETAG.LOCA_TAG)
                    {
                        FontEncode.Encodeloca();
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.GLYF_TAG)
                    {                        
                        int iEncodeEntryIndex = FontEncode.tbDirectory.FindTableEntry((UInt32)TABLETAG.GLYF_TAG);
                        CTableEntry tbEncodeEntry = FontEncode.tbDirectory.vtTableEntry[iEncodeEntryIndex];
                        tbEncodeEntry.offset = (uint)FontEncode.EncodeStream.Position;
                        FontEncode.EncodeStream.Write(lstGlyphData.ToArray(), 0, lstGlyphData.Count);
                        tbEncodeEntry.length = (uint)FontEncode.EncodeStream.Position - tbEncodeEntry.offset;
                        uint iTail = 4 - tbEncodeEntry.length % 4;
                        if (tbEncodeEntry.length % 4 > 0)
                        {
                            byte c = 0;
                            for (uint j = 0; j < iTail; j++)
                            {
                                FontEncode.EncodeStream.WriteByte(c);
                            }
                        }                        
                    }

                    if (HYDecodeEntry.tag == (uint)TABLETAG.HEAD_TAG)
                    {
                        FontEncode.tbHead = FontDecode.tbHead;
                        FontEncode.tbHead.checkSumAdjustment = 0;
                        FontEncode.Encodehead();
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.MAXP_TAG)
                    {
                        Encodemaxp(FontEncode);
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.CMAP_TAG)
                    {
                        BulidCMAP(FontEncode,lstUnicode);
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.HHEA_TAG)
                    {
                        ushort usTmp = FontEncode.tbHhea.numberOfHMetrics;
                        byte[] btTmp;

                        FontEncode.tbHhea = FontDecode.tbHhea;
                        FontEncode.tbHhea.numberOfHMetrics = usTmp;

                        int iEntryIndex = FontEncode.tbDirectory.FindTableEntry((UInt32)TABLETAG.HHEA_TAG);
                        if (iEntryIndex == -1) return HYRESULT.HHEA_ENCODE;

                        CTableEntry tbEntry = FontEncode.tbDirectory.vtTableEntry[iEntryIndex];
                        tbEntry.offset = (uint)FontEncode.EncodeStream.Position;

                        // version
                        usTmp = hy_cdr_int16_to(1);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        btTmp = BitConverter.GetBytes((UInt16)0);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);

                        //Ascender                       
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.Ascender);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);

                        //Descender
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.Descender);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //LineGap			
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.LineGap);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //advanceWidthMax			
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.advanceWidthMax);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //minLeftSideBearing                        
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.minLeftSideBearing);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //minRightSideBearing                        
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.minLeftSideBearing);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //xMaxExtent
                        usTmp = hy_cdr_int16_to((UInt16)FontEncode.tbHhea.xMaxExtent);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //caretSlopeRise		
                        usTmp = hy_cdr_int16_to(1);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //caretSlopeRun
                        usTmp = 0;
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //caretOffset            
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //reserved1			
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //reserved2			
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //reserved3			
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //reserved4			
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //metricDataFormat
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);
                        //numberOfHMetrics
                        usTmp = hy_cdr_int16_to(FontEncode.tbHhea.numberOfHMetrics);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FontEncode.EncodeStream.Write(btTmp, 0, btTmp.Length);

                        tbEntry.length = (uint)FontEncode.EncodeStream.Position - tbEntry.offset;
                        uint iTail = 4 - tbEntry.length % 4;
                        if (tbEntry.length % 4 > 0)
                        {
                            byte c = 0;
                            for (int j = 0; j < iTail; j++)
                            {
                                FontEncode.EncodeStream.WriteByte(c);
                            }
                        }
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.HMTX_TAG)
                    {
                        Encodehmtx(FontEncode);
                    }
                    if (HYDecodeEntry.tag == (uint)TABLETAG.POST_TAG)
                    {                        
                        FontEncode.Encodepost();
                    }

                    if (HYDecodeEntry.tag == (uint)TABLETAG.COLR_TAG)
                    {
                        FontEncode.EncodeCOLR();
                    }
                }
                else
                {
                    uint iLength = (HYDecodeEntry.length+3)/4*4;
                    byte[] TabelData = new byte[iLength];
                    FontDecode.DecodeStream.Seek(HYDecodeEntry.offset, SeekOrigin.Begin);
                    FontDecode.DecodeStream.Read(TabelData, 0, (int)iLength);

                    int iEncodeEntryIndex = FontEncode.tbDirectory.FindTableEntry((UInt32)HYDecodeEntry.tag);
                    CTableEntry tbEncodeEntry = FontEncode.tbDirectory.vtTableEntry[iEncodeEntryIndex];

                    tbEncodeEntry.offset = (uint)FontEncode.EncodeStream.Position;
                    FontEncode.EncodeStream.Write(TabelData, 0, TabelData.Length);                    
                }
            }

            FontEncode.EncodeStream.Flush();
            for (int i = 0; i < FontEncode.tbDirectory.numTables; i++)
            {
                CTableEntry HYEntry = FontEncode.tbDirectory.vtTableEntry[i];
                uint CheckBufSz = (HYEntry.length + 3) / 4 * 4;
                byte[] pCheckBuf = new byte[CheckBufSz];
                FontEncode.EncodeStream.Seek(HYEntry.offset, SeekOrigin.Begin);
                FontEncode.EncodeStream.Read(pCheckBuf, 0, (int)CheckBufSz);

                HYEntry.checkSum = FontEncode.CalcFontTableChecksum(pCheckBuf);
            }   

            FontEncode.EncodeTableDirectory();           
            //FontEncode.EncodeStream.Close();
            FontEncode.FontClose();

            return FontEncode.SetCheckSumAdjustment(strFontName);

        }   //end of HYRESULT BulidFont()

        HYRESULT BulidCMAP(HYEncode FontEnCodec, List<uint> lstUnicoe)
        {
            int iEncodeEntryIndex = FontEnCodec.tbDirectory.FindTableEntry((UInt32)TABLETAG.CMAP_TAG);
            CTableEntry tbEncodeEntry = FontEnCodec.tbDirectory.vtTableEntry[iEncodeEntryIndex];           
            
            FontEnCodec.tbCmap = new CCmap();
            CMAP_TABLE_ENTRY entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 3;
            entry.plat_encod_ID = 1;
            entry.format = 4;
            FontEnCodec.tbCmap.vtCamp_tb_entry.Add(entry);

            if (lstUnicoe[lstUnicoe.Count-1] > 0xffff)
            {
                entry.plat_ID = 3;
                entry.plat_encod_ID = 10;
                entry.format = 12;
                FontEnCodec.tbCmap.vtCamp_tb_entry.Add(entry);
            }

            FontEnCodec.tbCmap.version = 0;
            FontEnCodec.tbCmap.numSubTable = (ushort)FontEnCodec.tbCmap.vtCamp_tb_entry.Count;
            List<byte> tableData = new List<byte>();            
            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to((ushort)0x0000);
            btTmp = BitConverter.GetBytes(usTmp);
            tableData.AddRange(btTmp);           

            //numSubTable
            usTmp = hy_cdr_int16_to(FontEnCodec.tbCmap.numSubTable);
            btTmp = BitConverter.GetBytes(usTmp);            
            tableData.AddRange(btTmp);

            List<int> vtoffset = new List<int>();
            for (int i = 0; i < FontEnCodec.tbCmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbCmapEntry = FontEnCodec.tbCmap.vtCamp_tb_entry[i];

                // platformID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_ID);
                btTmp = BitConverter.GetBytes(usTmp);                
                tableData.AddRange(btTmp);
                // encodingID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_encod_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                tableData.AddRange(btTmp);
                
                // offset			
                vtoffset.Add(tableData.Count);
                tbCmapEntry.offset = 0;
                ulTmp = hy_cdr_int32_to(tbEncodeEntry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                tableData.AddRange(btTmp);
            }
            
            for (int i = 0; i < FontEnCodec.tbCmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY  tbCmapEntry = FontEnCodec.tbCmap.vtCamp_tb_entry[i];
                switch (tbCmapEntry.format){
                    case 4:{
                            List<byte> vtCmp4 = new List<byte>();
                            FontEnCodec.EncodeCmapFmt4(ref tbCmapEntry, ref vtCmp4, FontEnCodec.codemap.lstCodeMap);                          

                            ulTmp = hy_cdr_int32_to((uint)tableData.Count);
                            byte[] aryTmp = BitConverter.GetBytes(ulTmp);

                            tableData[vtoffset[i]] = aryTmp[0];
                            tableData[vtoffset[i]+1] = aryTmp[1];
                            tableData[vtoffset[i]+2] = aryTmp[2];
                            tableData[vtoffset[i]+3] = aryTmp[3];

                            tableData.AddRange(vtCmp4);
                        }
                        break;                    
                    case 12:{
                            List<byte> vtCmp12 = new List<byte>();
                            FontEnCodec.EncodeCmapFmt12(ref tbCmapEntry, ref vtCmp12, FontEnCodec.codemap.lstCodeMap);                            

                            ulTmp = hy_cdr_int32_to((uint)tableData.Count);
                            byte[] aryTmp = BitConverter.GetBytes(ulTmp);

                            tableData[vtoffset[i]] = aryTmp[0];
                            tableData[vtoffset[i] + 1] = aryTmp[1];
                            tableData[vtoffset[i] + 2] = aryTmp[2];
                            tableData[vtoffset[i] + 3] = aryTmp[3];

                            tableData.AddRange(vtCmp12);
                        }
                        break;
                    default:
                        break;
                }           
            }

            tbEncodeEntry.length = (uint)tableData.Count;
            tbEncodeEntry.offset = (uint)FontEnCodec.EncodeStream.Position;            
            FontEnCodec.EncodeStream.Write(tableData.ToArray(), 0, (int)tbEncodeEntry.length);

            uint iTail = 4 - tbEncodeEntry.length % 4;
            if (tbEncodeEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FontEnCodec.EncodeStream.WriteByte(c);
                }
            }            

            return HYRESULT.NOERROR;           

        }   // end of void BulidCMAP()

        public HYRESULT Encodemaxp(HYEncode FontEnCodec)
        {
            int iEntryIndex = FontEnCodec.tbDirectory.FindTableEntry((UInt32)TABLETAG.MAXP_TAG);
            if (iEntryIndex == -1) return HYRESULT.MAXP_ENCODE;

            CTableEntry tbEntry = FontEnCodec.tbDirectory.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FontEnCodec.EncodeStream.Position;

            UInt16 usTmp;
            byte[] btTmp;
            //FontEnCodec.tbMaxp = new CMaxp();
            if (FontEnCodec.FntType == FONTTYPE.TTF)
            {
                //Table version number                
                usTmp = hy_cdr_int16_to((UInt16)FontEnCodec.tbMaxp.version.value);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);                
                btTmp = BitConverter.GetBytes(FontEnCodec.tbMaxp.version.fract);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //numGlyphs         
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.numGlyphs);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);                
                // maxpoints
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxPoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxContours                
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxContours);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxCompositePoints				
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxCompositePoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxCompositeContours
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxCompositeContours); ;
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxZones
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxZones);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxTwilightPoints
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxTwilightPoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxStorage
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxStorage);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxFunctionDefs
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxFunctionDefs);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxInstructionDefs
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxInstructionDefs);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxStackElements
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxStackElements);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxSizeOfInstructions
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxSizeOfInstructions); ;
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxComponentElements
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxComponentElements);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                //maxComponentDepth
                usTmp = hy_cdr_int16_to(FontEnCodec.tbMaxp.maxComponentDepth);
                btTmp = BitConverter.GetBytes(usTmp);
                FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
            }

            tbEntry.length = (uint)FontEnCodec.EncodeStream.Position - tbEntry.offset;
            uint iTail = 4 - tbEntry.length % 4;
            if (tbEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FontEnCodec.EncodeStream.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodemaxp()

        public HYRESULT Encodehmtx(HYEncode FontEnCodec)
        {
            int iEntryIndex = FontEnCodec.tbDirectory.FindTableEntry((UInt32)TABLETAG.HMTX_TAG);
            if (iEntryIndex == -1) return HYRESULT.HMTX_ENCODE;

            CTableEntry tbEntry = FontEnCodec.tbDirectory.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FontEnCodec.EncodeStream.Position;

            UInt16 usTmp;
            byte[] btTmp;

            UInt16 longhormetricNums = FontEnCodec.tbHhea.numberOfHMetrics;
            UInt16 lefsidebearNums = (UInt16)(FontEnCodec.tbMaxp.numGlyphs - FontEnCodec.tbHhea.numberOfHMetrics);
            for (int i = 0; i < FontEnCodec.tbMaxp.numGlyphs; i++)
            {
                if (i < longhormetricNums)
                {
                    usTmp = hy_cdr_int16_to(FontEnCodec.tbHmtx.lstLonghormetric[i].advanceWidth);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);

                    usTmp = hy_cdr_int16_to((UInt16)FontEnCodec.tbHmtx.lstLonghormetric[i].lsb);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                }
                else
                {
                    usTmp = hy_cdr_int16_to((UInt16)FontEnCodec.tbHmtx.lstLonghormetric[i].lsb);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FontEnCodec.EncodeStream.Write(btTmp, 0, btTmp.Length);
                }
            }

            tbEntry.length = (uint)FontEnCodec.EncodeStream.Position - tbEntry.offset;
            uint iTail = 4 - tbEntry.length % 4;
            if (tbEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FontEnCodec.EncodeStream.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;
        
        }   // end of T EncodeHmtx()

        bool HitTable(List<uint> vtTableFlag, uint ulTableFlag)
	    {
		    for (int i=0; i<vtTableFlag.Count; i++)
		    {
			    if (vtTableFlag[i] == ulTableFlag) 
				    return true;
		    }

		    return false;

	    }	// end of void CFontManager::HitTable()

        bool CheckGidRepeat(List<int> vtGid, int iGid)
	    {		    
            for (int i = 0; i < vtGid.Count; i++)
		    {
                if (vtGid[i] == iGid) return true;
		    }

		    return false;

	    }	// end of BOOL CFontManager::CheckGidRepeat()

        //public HYRESULT MergeGlyphs();


    }
}
