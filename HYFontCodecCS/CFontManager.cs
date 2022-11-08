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
            public List<byte> lstData = new List<byte>();
            public uint DataLen;
            public List<uint> lstUnicode = new List<uint>();
            public ushort usAdvWidth;
            public short sLsb;
            public ushort usAdvHeight;
            public short sTsb;
            public string name;
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
        public HYRESULT ExtractFont(string strTTFFile, string strNewTTFFile, List<UInt32> lstunicode, int CMAP, ref List<UInt32> lstMssUni)
        {
            HYDecode FontDecode = new HYDecode();
            HYRESULT sult = PrepareExtractFont(strTTFFile, ref FontDecode);
            if (sult != HYRESULT.NOERROR) return sult;

            List<byte> lstGlyphData = new List<byte>();
            List<uint> vtTableFlag = new List<uint>();

            HYEncodeBase FontEncode = new HYEncodeBase();
          
            sult = Extrace(FontDecode, ref FontEncode, lstunicode, ref lstGlyphData, ref lstMssUni);
                

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

            FontDecode.FontClose();

            return sult;

        }   // end of HYRESULT ExtractFont()

        public HYRESULT GetSubset(string strTTFFile, string strNewTTFFile, List<UInt32> lstunicode, int CMAP, ref List<UInt32> lstMssUni)
        {   
            HYDecode FontDecode = new HYDecode();
            HYRESULT sult = PrepareExtractFont(strTTFFile, ref FontDecode);
            if (sult != HYRESULT.NOERROR) return sult;

            FontDecode.codemap = new HYCodeMap();
            ushort maxGID = 0;
            byte[] pOutGlyphs = FilterGlyphs(ref FontDecode, ref lstunicode, ref maxGID);
            MakeFont(ref FontDecode, pOutGlyphs, strNewTTFFile, maxGID);

            FontDecode.FontClose();

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT GetSubset()

        public void MakeFont(ref HYDecode FontDecode, byte[] pGlpyData, string strNewTTFFile, ushort maxGID)
        {   
            try {
                if (File.Exists(strNewTTFFile)){
                    File.Delete(strNewTTFFile);
                }
            }
            catch (Exception ext) {
                ext.ToString();
                throw;
            }

            HYEncode FontEncode = new HYEncode();
            FontEncode.FontOpen(strNewTTFFile);

            FontEncode.FntType = FontDecode.FntType;
            FontEncode.tbDirectory = FontDecode.tbDirectory;
            FontEncode.codemap = FontDecode.codemap;
            FontEncode.tbMaxp = FontDecode.tbMaxp;
            FontEncode.tbMaxp.numGlyphs = (ushort)(maxGID+1);
            FontEncode.EncodeTableDirectory();

            for (int i = 0; i != FontEncode.tbDirectory.vtTableEntry.Count; i++)
            {
                CTableEntry HYEntry = FontEncode.tbDirectory.vtTableEntry[i];
                if (HYEntry.tag == (uint)TABLETAG.LOCA_TAG){
                    BulidLOCAL(ref FontEncode.FileStrm, ref HYEntry, FontDecode);
                }
                else if (HYEntry.tag == (uint)TABLETAG.GLYF_TAG){
                    BulidGlyph(ref FontEncode.FileStrm, ref HYEntry, pGlpyData);                    
                }
                else if (HYEntry.tag == (uint)TABLETAG.HEAD_TAG)
                {
                   BulidHead(ref FontEncode.FileStrm, ref HYEntry, FontDecode);
                }
                else if (HYEntry.tag == (uint)TABLETAG.CMAP_TAG)
                {                    
                    MakeCMAP(ref FontEncode);                    
                    Encodecmap(ref FontEncode, ref HYEntry, FontEncode.codemap.lstCodeMap);
                }
                else if (HYEntry.tag == (uint)TABLETAG.POST_TAG)
                {
                    FontDecode.tbPost = new CPost();
                    FontDecode.tbPost.version.value = 3;
                    FontDecode.tbPost.version.fract = 0;
                    FontDecode.tbPost.italicAngle.value = 0;
                    FontDecode.tbPost.italicAngle.fract = 0;
                    FontDecode.tbPost.underlinePosition = -75;
                    FontDecode.tbPost.underlineThickness = 50;
                    FontDecode.tbPost.isFixedPitch = 0;
                    FontDecode.tbPost.minMemType42 = 0;
                    FontDecode.tbPost.maxMemType42 = 0;
                    FontDecode.tbPost.minMemType1 = 0;
                    FontDecode.tbPost.maxMemType1 = 0;
                    Bulidepost(ref FontEncode.FileStrm, ref HYEntry, FontDecode);
                }
                else if(HYEntry.tag == (uint)TABLETAG.HHEA_TAG)
                {
                    Bulidhhea(ref FontEncode.FileStrm, ref HYEntry, FontDecode, maxGID);
                }
                else if (HYEntry.tag == (uint)TABLETAG.HMTX_TAG)
                {
                    Buildhmtx(ref FontEncode.FileStrm, ref HYEntry, FontDecode, maxGID);
                }
                else if (HYEntry.tag == (uint)TABLETAG.MAXP_TAG)
                {
                    Buildmaxp(ref FontEncode.FileStrm, ref HYEntry, FontEncode);
                }
                else
                {   
                    uint iLength = (HYEntry.length + 3) / 4 * 4;
                    byte[] TabelData = new byte[iLength];
                    FontDecode.DecodeStream.Seek(HYEntry.offset, SeekOrigin.Begin);
                    FontDecode.DecodeStream.Read(TabelData, 0, (int)iLength);

                    int iEncodeEntryIndex = FontEncode.tbDirectory.FindTableEntry((UInt32)HYEntry.tag);
                    CTableEntry tbEncodeEntry = FontEncode.tbDirectory.vtTableEntry[iEncodeEntryIndex];

                    tbEncodeEntry.offset = (uint)FontEncode.EncodeStream.Position;
                    tbEncodeEntry.length = HYEntry.length;
                    FontEncode.EncodeStream.Write(TabelData, 0, TabelData.Length);
                }
		    }

            FontEncode.EncodeStream.Flush();
            for (int i = 0; i < FontEncode.tbDirectory.numTables; i++)
            {
                CTableEntry HYEntry = FontEncode.tbDirectory.vtTableEntry[i];
                uint CheckBufSz = (HYEntry.length + 3) / 4 * 4;
                byte[] pCheckBuf = new byte[CheckBufSz];
                FontEncode.FileStrm.Seek(HYEntry.offset, SeekOrigin.Begin);
                FontEncode.FileStrm.Read(pCheckBuf, 0, (int)CheckBufSz);

                HYEntry.checkSum = CalcFontTableChecksum(pCheckBuf);
            }
            FontEncode.EncodeTableDirectory();
            FontEncode.FileStrm.Flush();
            FontEncode.FileStrm.Close();

            FontEncode.SetCheckSumAdjustment(strNewTTFFile);

        }   // end of int MakeFont()

        UInt32 CalcFontTableChecksum(byte[] btFile)
        {
            UInt32 Sum = 0;
            Int32 Index = 0;
            Int32 length = ((btFile.Length + 3) & ~3) / sizeof(Int32);
            if (length % 4 != 0) return 0;

            for (int i = 0; i < length; i++)
            {
                byte[] btCnv = new byte[4] { 0, 0, 0, 0 };
                btCnv[0] = btFile[Index++];
                btCnv[1] = btFile[Index++];
                btCnv[2] = btFile[Index++];
                btCnv[3] = btFile[Index++];
                btCnv = btCnv.Reverse().ToArray();
                Sum += BitConverter.ToUInt32(btCnv, 0);
            }

            return Sum;

        }	// end of unsigned long CalcFontTableChecksum()

        void BulidLOCAL(ref FileStream FWStrm, ref CTableEntry HYEntry, HYDecode FontDecode)
        {
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;
            if (FontDecode.tbHead.indexToLocFormat == 0){
                foreach (uint iloca in FontDecode.tbLoca.vtLoca){
                    usTmp = hy_cdr_int16_to((UInt16)iloca);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);
                }
            }
            else if (FontDecode.tbHead.indexToLocFormat == 1){
                foreach (uint iloca in FontDecode.tbLoca.vtLoca){
                    ulTmp = hy_cdr_int32_to(iloca);
                    btTmp = BitConverter.GetBytes(ulTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);
                }
            }

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0){
                byte c = 0;
                for (int i = 0; i < iTail; i++) {
                    FWStrm.WriteByte(c);
                }
            }

        }	// end of void BulidLOCAL()

        public HYRESULT Bulidepost(ref FileStream FWStrm, ref CTableEntry HYEntry, HYDecode FontDecode)
        {
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp = 0;
            UInt32 ulTmp = 0;
            byte[] btTmp;

            //version            
            usTmp = hy_cdr_int16_to((ushort)FontDecode.tbPost.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = 0;
            //usTmp = hy_cdr_int16_to(FontDecode.tbPost.Format.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            // italicAngle
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbPost.italicAngle.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to(FontDecode.tbPost.italicAngle.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //underlinePosition
            if (FontDecode.tbPost.underlinePosition == 0)
            {
                FontDecode.tbPost.underlinePosition = Head.yMin;
            }
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbPost.underlinePosition);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //UnderlineThickness
            if (FontDecode.tbPost.underlineThickness == 0)
            {
                FontDecode.tbPost.underlineThickness = (Int16)(Head.unitsPerEm / 20);
            }
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbPost.underlineThickness);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //isFixedPitch
            ulTmp = hy_cdr_int16_to((UInt16)FontDecode.tbPost.isFixedPitch);
            btTmp = BitConverter.GetBytes(ulTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            ulTmp = 0;
            btTmp = BitConverter.GetBytes(ulTmp);
            //minMemType42
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //maxMemType42
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //minMemType1
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //maxMemType1
            FWStrm.Write(btTmp, 0, btTmp.Length);

            if (FontDecode.tbPost.version.value == 2 && FontDecode.tbPost.version.fract == 0)
            {
                // numberOfGlyphs
                usTmp = hy_cdr_int16_to(FontDecode.tbPost.PostFormat2.usNumberOfGlyphs);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);

                //glyphNameIndex
                for (int i = 0; i < FontDecode.tbPost.PostFormat2.usNumberOfGlyphs; i++)
                {
                    usTmp = hy_cdr_int16_to(FontDecode.tbPost.PostFormat2.lstGlyphNameIndex[i]);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);
                }
                //names
                int st = FontDecode.tbPost.lstStandString.Count;
                for (int i = 258; i < st; i++)
                {
                    byte[] szString = System.Text.Encoding.Default.GetBytes(FontDecode.tbPost.lstStandString[i]);
                    byte strlen = (byte)szString.Length;

                    FWStrm.WriteByte(strlen);
                    FWStrm.Write(szString, 0, strlen);
                }
            }

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FWStrm.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Bulidepost()


        void BulidGlyph(ref FileStream FWStrm, ref CTableEntry HYEntry, byte[] pGlpyData)
        {            
            //pGlpyData.Length 已经是4字节对齐了;
            // 偏移
            HYEntry.offset = (uint)FWStrm.Position;
            FWStrm.Write(pGlpyData, 0, pGlpyData.Length);
            HYEntry.length = (uint)pGlpyData.Length;
            HYEntry.checkSum = CalcFontTableChecksum(pGlpyData);

        }	// end of void BulidGlyph()

        public HYRESULT Buildmaxp(ref FileStream FWStrm, ref CTableEntry HYEntry, HYEncode FntEcd)
        {
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            byte[] btTmp;
            
            //Table version number
            usTmp = hy_cdr_int16_to((ushort)FntEcd.tbMaxp.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);                
            usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //numGlyphs                
            usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.numGlyphs);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            
            if (FntEcd.FntType == FONTTYPE.TTF)
            {                
                //maxPoints          
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxPoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxContours                
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxContours);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxCompositePoints				
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxCompositePoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxCompositeContours                
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxCompositeContours);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxZones
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxZones);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxTwilightPoints
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxTwilightPoints);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxStorage
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxStorage);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxFunctionDefs
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxFunctionDefs);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxInstructionDefs
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxInstructionDefs);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxStackElements
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxStackElements);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxSizeOfInstructions
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxSizeOfInstructions);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxComponentElements
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxComponentElements);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                //maxComponentDepth                
                usTmp = hy_cdr_int16_to(FntEcd.tbMaxp.maxComponentDepth);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
            }

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FWStrm.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Buildmaxp()

        public HYRESULT Bulidhhea(ref FileStream FWStrm, ref CTableEntry HYEntry, HYDecode FontDecode, ushort imaxGID)
        {
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            byte[] btTmp;
            
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //Ascender
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.Ascender);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //Descender
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.Descender);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //LineGap			
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.LineGap);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //advanceWidthMax			
            usTmp = hy_cdr_int16_to(FontDecode.tbHhea.advanceWidthMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //minLeftSideBearing            
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.minLeftSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //minRightSideBearing            
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.minRightSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //xMaxExtent            
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHhea.xMaxExtent);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRise		
            usTmp = hy_cdr_int16_to(1);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRun
            usTmp = 0;
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //caretOffset            
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //reserved1			
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //reserved2			
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //reserved3			
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //reserved4			
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //metricDataFormat
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //numberOfHMetrics
            
            int GID = imaxGID;
            FontDecode.tbHhea.numberOfHMetrics = 0;
            int iBaseadvanceWidth = FontDecode.tbHmtx.lstLonghormetric[GID--].advanceWidth;
            while (GID >= 0)
            {
                if (FontDecode.tbHmtx.lstLonghormetric[GID].advanceWidth == iBaseadvanceWidth)
                    FontDecode.tbHhea.numberOfHMetrics++;
                else
                    break;

                GID--;
            }
            FontDecode.tbHhea.numberOfHMetrics = (ushort)(imaxGID+1- FontDecode.tbHhea.numberOfHMetrics);
            usTmp = hy_cdr_int16_to(FontDecode.tbHhea.numberOfHMetrics);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FWStrm.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT Bulidhhea()

        public HYRESULT Buildhmtx(ref FileStream FWStrm, ref CTableEntry HYEntry, HYDecode FontDecode,ushort imaxGID)
        {          
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            byte[] btTmp;

            
            UInt16 longhormetricNums = FontDecode.tbHhea.numberOfHMetrics;
            UInt16 lefsidebearNums = (UInt16)(imaxGID+1 - FontDecode.tbHhea.numberOfHMetrics);

            for (int i = 0; i < imaxGID+1; i++)
            {
                if (i < longhormetricNums)
                {
                    usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHmtx.lstLonghormetric[i].advanceWidth);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);

                    usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHmtx.lstLonghormetric[i].lsb);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);
                }
                else
                {
                    usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHmtx.lstLonghormetric[i].lsb);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FWStrm.Write(btTmp, 0, btTmp.Length);
                }
            }

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FWStrm.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Buildhmtx()

        void BulidHead(ref FileStream FWStrm, ref CTableEntry HYEntry, HYDecode FontDecode)
        {            
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;
            
            //Table version number
            FontDecode.tbHead.version.value = 1;
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            FontDecode.tbHead.version.fract = 0;
            usTmp = hy_cdr_int16_to(FontDecode.tbHead.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //fontRevision			
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.fontRevision.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to(FontDecode.tbHead.fontRevision.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            // checkSumAdjustment		
            ulTmp = hy_cdr_int32_to(FontDecode.tbHead.checkSumAdjustment);
            btTmp = BitConverter.GetBytes(ulTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //magicNumber
            FontDecode.tbHead.magicNumber = 0x5F0F3CF5;
            ulTmp = hy_cdr_int32_to(FontDecode.tbHead.magicNumber);
            btTmp = BitConverter.GetBytes(ulTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //flags
            usTmp = hy_cdr_int16_to(FontDecode.tbHead.flags);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //unitsPerEm
            usTmp = hy_cdr_int16_to(FontDecode.tbHead.unitsPerEm);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //create            
            FWStrm.Write(FontDecode.tbHead.created, 0, 8);
            //modified
            FWStrm.Write(FontDecode.tbHead.modified, 0, 8);
            //xMin
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.xMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //yMin
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.yMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //xMax
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.xMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //yMax
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.yMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //macStyle
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.macStyle);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //lowestRecPPEM
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.lowestRecPPEM);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //fontDirectionHint
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.fontDirectionHint);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //indexToLocFormat
            FontDecode.tbHead.indexToLocFormat = 1;
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.indexToLocFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);
            //glyphDataFormat
            FontDecode.tbHead.glyphDataFormat = 0;
            usTmp = hy_cdr_int16_to((UInt16)FontDecode.tbHead.glyphDataFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0) {
                byte c = 0;
                for (int i = 0; i < iTail; i++)  {
                    FWStrm.WriteByte(c);
                }
            }

        }	// end of void CFontExtract::BulidHead()

        void FilterByUnicode(ulong ulUnicode, ref List<HYCodeMapItem> lstCodeMap)
        {
            List<HYCodeMapItem> vtTmpHYCodeMap = new List<HYCodeMapItem>();
            vtTmpHYCodeMap.AddRange(lstCodeMap);
                
            lstCodeMap.Clear();
            
            for (int i = 0; i < vtTmpHYCodeMap.Count; i++) {
                if (vtTmpHYCodeMap[i].Unicode != ulUnicode){
                    lstCodeMap.Add(vtTmpHYCodeMap[i]);
                }
            }

        }	// end of void CHYCodeMap::FilterByUnicode()

        private  int  HYCodeMapSortPredicate(HYCodeMapItem d1, HYCodeMapItem d2)
        {
            int ireturn = 0;
            if (d1.Unicode < d2.Unicode) ireturn=-1;
            if (d1.Unicode == d2.Unicode) ireturn=0;
            if (d1.Unicode > d2.Unicode) ireturn =1;

            return ireturn;

        }	// end of BOOL HYCodeMapSortPredicate()

        void MakeCMAP(ref HYEncode FontEncode)
        {            
            CCmap cmap = new CCmap();

            CMAP_TABLE_ENTRY entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 3;
            entry.plat_encod_ID = 1;
            entry.format = (ushort)CMAPENCODEFORMAT.CMAP_ENCODE_FT_4;
            cmap.vtCamp_tb_entry.Add(entry);

            FilterByUnicode(0xffffffff, ref FontEncode.codemap.lstCodeMap);
            FontEncode.codemap.lstCodeMap.Sort(HYCodeMapSortPredicate);
            
            if (FontEncode.codemap.lstCodeMap[FontEncode.codemap.lstCodeMap.Count - 1].Unicode > 0xFFFF)
            {
                entry = new CMAP_TABLE_ENTRY();
                entry.plat_ID = 3;
                entry.plat_encod_ID = 10;
                entry.format = (ushort)CMAPENCODEFORMAT.CMAP_ENCODE_FT_12;
                cmap.vtCamp_tb_entry.Add(entry);
            }
            /*
            int iIndx = FontCodec.m_HYCmap.FindSpecificEntryIndex(CMAP_ENCODE_FT_14);
            if (iIndx != -1)
            {
                CMAP_TABLE_ENTRY Entry = FontCodec.m_HYCmap.vtCamp_tb_entry[iIndx];
                cmap.vtCamp_tb_entry.push_back(Entry);
            }
            */
            cmap.numSubTable = (ushort)cmap.vtCamp_tb_entry.Count;
            FontEncode.tbCmap = cmap;

        }   // end of void MakeCMAP()

        public HYRESULT Encodecmap(ref HYEncode encode, ref CTableEntry HYEntry,List<HYCodeMapItem> lstCodeMap)
        {
            FileStream FWStrm = encode.FileStrm;
            CCmap Cmap = encode.tbCmap;
            HYEntry.offset = (uint)FWStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to(Cmap.version);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            //numSubTable
            usTmp = hy_cdr_int16_to(Cmap.numSubTable);
            btTmp = BitConverter.GetBytes(usTmp);
            FWStrm.Write(btTmp, 0, btTmp.Length);

            int i = 0;
            for (i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbCmapEntry = Cmap.vtCamp_tb_entry[i];

                // platformID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                // encodingID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_encod_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                // offset				
                tbCmapEntry.offset = HYEntry.offset;
                ulTmp = hy_cdr_int32_to(HYEntry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
            }

            for (i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbCmapEntry = Cmap.vtCamp_tb_entry[i];
                switch (tbCmapEntry.format)
                {
                     case 4:
                        {   
                           EncodeCmapFmt4(ref FWStrm,ref tbCmapEntry, lstCodeMap);
                        }
                        break;
                    case 12:
                        {                            
                            EncodeCmapFmt12(ref FWStrm,ref tbCmapEntry,lstCodeMap);
                        }
                        break;
                    default:
                        break;
                }
            }

            long ulCmapEndPos = FWStrm.Position;
            FWStrm.Seek(HYEntry.offset + 4, SeekOrigin.Begin);

            for (i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbCampEntry = Cmap.vtCamp_tb_entry[i];

                // platformID
                usTmp = hy_cdr_int16_to(tbCampEntry.plat_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                // encodingID
                usTmp = hy_cdr_int16_to(tbCampEntry.plat_encod_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
                // offset
                ulTmp = hy_cdr_int32_to(tbCampEntry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                FWStrm.Write(btTmp, 0, btTmp.Length);
            }

            FWStrm.Flush();
            FWStrm.Seek(ulCmapEndPos, SeekOrigin.Begin);
            HYEntry.length = (uint)FWStrm.Position - HYEntry.offset;
            uint iTail = 4 - HYEntry.length % 4;
            if (HYEntry.length % 4 > 0)
            {
                byte c = 0;
                for (int j = 0; j < iTail; j++)
                {
                    FWStrm.WriteByte(c);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodecmap()

        public int NextGroup(int ix, int eov, ref GroupRecord group, List<HYCodeMapItem> lstCodeMap)
        {
            int entryCount = 0;
            bool haveContiguous = false;		// Neither contiguous nor discontiguous yet
            bool contiguous = true;			// Default to true for single glyph range.

            if (ix != eov)
            {
                UInt32 startCharCode = lstCodeMap[ix].Unicode;
                int startGID = lstCodeMap[ix].GID;
                int hold = ix;
                ++ix;
                ++entryCount;

                group.startCode = startCharCode;
                group.startGID = startGID;
                group.endCode = startCharCode;

                while (ix != eov && lstCodeMap[ix].Unicode == startCharCode + 1)
                {
                    int gid = lstCodeMap[ix].GID;
                    bool successive = gid == startGID + 1;

                    startCharCode = lstCodeMap[ix].Unicode;
                    ++group.endCode;

                    if (haveContiguous)
                    {
                        if (successive != contiguous)
                        {
                            ix = hold;
                            entryCount -= 1;
                            group.endCode -= 1;
                            break;
                        }
                    }
                    else
                        contiguous = successive;

                    haveContiguous = true;
                    startGID = gid;

                    hold = ++ix;
                    ++entryCount;
                }
            }
            group.contiguous = contiguous;
            return ix;

        }   // end of protected HYRESULT	NextGroup()
        public int FindGryphIndexByUnciodeEx(UInt32 ulUnicode, List<HYCodeMapItem> lstCodeMap)
        {
            foreach (HYCodeMapItem item in lstCodeMap)
            {
                if (item.Unicode == ulUnicode)
                {
                    return (UInt16)item.GID;
                }
            }

            return -1;

        }	// end of int	FindGryphIndexByUnciodeEx()
        public int EncodeCmapFmt4(ref FileStream FWStrm, ref CMAP_TABLE_ENTRY entry, List<HYCodeMapItem> lstCodeMap)
        {
            List<byte> vtCmap = new List<byte>();
            UInt16 usTmp;
            byte[] btTmp;

            CMAP_ENCODE_FORMAT_4 CEF4 = entry.Format4;
            CEF4.format = 0;

            List<GroupRecord> groupList = new List<GroupRecord>();
            int eov = lstCodeMap.Count;
            int ix = 0;

            while (ix != eov && lstCodeMap[ix].Unicode <= 0xffff)
            {
                GroupRecord group = new GroupRecord();
                int nextIx = NextGroup(ix, eov, ref group, lstCodeMap);
                if (group.startCode < 0xffff)
                {	// This format does not code above 0xfffe
                    if (group.endCode > 0xfffe)
                        group.endCode = 0xfffe;

                    groupList.Add(group);
                }
                ix = nextIx;
            }

            UInt32 lastCode = 0;
            UInt16 entryCount = 0;
            UInt16 groupCount = 0;

            GroupRecord gix = groupList[0];
            GroupRecord gend = groupList[groupList.Count - 1];

            int iPoint = 0;
            //if (gix != groupList[groupList.Count - 1])
            //   ++iPoint;

            while (true)
            {
                if (!gix.contiguous)
                    entryCount += (UInt16)(gix.endCode - gix.startCode + 1);

                if (gix == gend)
                {
                    lastCode = gend.endCode;
                    groupCount += 1;
                    break;
                }

                lastCode = gix.endCode;
                groupCount += 1;
                gix = groupList[++iPoint];
            }

            if (lastCode != 0xffff)
                groupCount += 1;

            int length = 2 * (8 + groupCount * 4 + entryCount);
            if (length > 65535)
                return -1;

            UInt16[] startTable = new UInt16[groupCount];
            UInt16[] endTable = new UInt16[groupCount];
            Int16[] idDelta = new Int16[groupCount];
            UInt16[] idRangeOffset = new UInt16[groupCount];
            List<UInt16> glyphIdArray = new List<UInt16>();

            int groupIx = 0;
            entryCount = 0;
            iPoint = 0;
            // gix = groupList[iPoint];
            while (iPoint < groupList.Count)
            {
                gix = groupList[iPoint];
                uint entries = gix.endCode - gix.startCode + 1;

                startTable[groupIx] = (UInt16)gix.startCode;
                endTable[groupIx] = (UInt16)gix.endCode;

                if (gix.contiguous)
                {	// If the glyphIDs in the range are consecutive then we don't need entries in the GlyphIdArray.
                    idDelta[groupIx] = (short)(gix.startGID - gix.startCode);
                    idRangeOffset[groupIx] = 0xffff;
                }
                else
                {	//Theoretically the table can be compressed by finding multiple ranges with the same differences between each
                    // code point in the range and the corresponding glyphID, and then adjusting the bias via idDelta. We're not going to
                    // bother. It's computationally expensive and probably doesn't yield much savings for most circumstances.
                    idDelta[groupIx] = 0;
                    idRangeOffset[groupIx] = (UInt16)entryCount;

                    uint code = gix.startCode;
                    for (uint i = 0; i < entries; ++i)
                    {
                        int iGID = FindGryphIndexByUnciodeEx(code++, lstCodeMap);
                        if (iGID != -1)
                            glyphIdArray.Add((UInt16)iGID);
                        else
                            glyphIdArray.Add(0);
                    }

                    if (entryCount + entries > 0xffff)
                        return -1;

                    entryCount += (UInt16)entries;
                }
                ++groupIx;
                //gix = groupList[++iPoint];
                iPoint++;
            }

            if (lastCode != 0xffff)
            {
                startTable[groupIx] = 0xffff;
                endTable[groupIx] = 0xffff;
                idDelta[groupIx] = 1;
                idRangeOffset[groupIx] = 0xffff;
            }

            //format
            entry.Format4.format = 4;
            usTmp = hy_cdr_int16_to(entry.Format4.format);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);
            //length
            entry.Format4.length = (ushort)length;
            usTmp = hy_cdr_int16_to(entry.Format4.length);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //language
            entry.Format4.language = 0;
            usTmp = hy_cdr_int16_to(entry.Format4.language);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //segCountX2
            entry.Format4.segCountX2 = (ushort)(groupCount * 2);
            usTmp = hy_cdr_int16_to(entry.Format4.segCountX2);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //searchRange
            entry.Format4.searchRange = (ushort)(2 * Math.Pow(2, Math.Floor((Math.Log((double)groupCount) / Math.Log(2.0)))));
            usTmp = hy_cdr_int16_to(entry.Format4.searchRange);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //entrySelector
            entry.Format4.entrySelector = (ushort)(Math.Log(entry.Format4.searchRange / 2.0) / Math.Log(2.0));
            usTmp = hy_cdr_int16_to(entry.Format4.entrySelector);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //rangeShift
            entry.Format4.rangeShift = (ushort)(entry.Format4.segCountX2 - entry.Format4.searchRange);
            usTmp = hy_cdr_int16_to(entry.Format4.rangeShift);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //endCount			
            for (int i = 0; i < groupCount; i++)
            {
                usTmp = endTable[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            //reservedPad
            entry.Format4.reservedPad = 0;
            usTmp = hy_cdr_int16_to(entry.Format4.reservedPad);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //startCount
            for (int i = 0; i < groupCount; i++)
            {
                usTmp = startTable[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            //idDelta
            for (int i = 0; i < groupCount; i++)
            {
                usTmp = (ushort)idDelta[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            //idRangeOffset
            for (int i = 0; i < groupCount; i++)
            {
                if (idRangeOffset[i] == 0xffff)
                {
                    usTmp = 0;
                }
                else
                {
                    usTmp = (ushort)(2 * (idRangeOffset[i] + groupCount - i));
                }

                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            //glyphIdArray
            for (int i = 0; i < glyphIdArray.Count; i++)
            {
                usTmp = glyphIdArray[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            entry.offset = (uint)(FWStrm.Position - entry.offset);
            FWStrm.Write(vtCmap.ToArray(), 0, vtCmap.Count);

            return 0;

        }   // end of protected HYRESULT	EncodeCmapFmt4()

        public void EncodeCmapFmt12(ref FileStream FWStrm, ref CMAP_TABLE_ENTRY entry, List<HYCodeMapItem> lstCodeMap)
        {
            List<byte> vtCmap = new List<byte>();

            UInt16 usTmp;
            uint ulTmp;
            byte[] btTmp;

            int iGlyphIndex = -1, iExpectGlyphIndex = -1;
            ulong ulUnicode = 0, ulExpectUnicode = 0;
            int i = 0, j = 0;

            int iCharNumber = lstCodeMap.Count;
            while (i < iCharNumber)
            {
                CMAP_ENCODE_FORMAT_12_GROUP tagF12Group = new CMAP_ENCODE_FORMAT_12_GROUP();
                HYCodeMapItem HYMapItem = lstCodeMap[i];

                iGlyphIndex = HYMapItem.GID;
                ulUnicode = HYMapItem.Unicode;

                if (iGlyphIndex == 0)
                {
                    i++;
                    continue;
                }

                tagF12Group.startCharCode = (uint)ulUnicode;
                tagF12Group.startGlyphID = (uint)iGlyphIndex;

                for (j = i + 1; j < iCharNumber; j++)
                {
                    HYCodeMapItem HYMapItem1 = lstCodeMap[j];
                    iExpectGlyphIndex = HYMapItem1.GID;
                    ulExpectUnicode = HYMapItem1.Unicode;

                    if (iExpectGlyphIndex == 0) continue;

                    iGlyphIndex++;
                    ulUnicode++;

                    // Index和unicode都必须是连续的,否则段结束
                    if (iExpectGlyphIndex != iGlyphIndex || ulExpectUnicode != ulUnicode)
                    {
                        tagF12Group.endCharCode = (uint)(ulUnicode - 1);
                        entry.Format12.vtGroup.Add(tagF12Group);
                        i = j;
                        break;
                    }
                }

                // 数组末尾
                if (j == iCharNumber)
                {
                    if (tagF12Group.endCharCode == 0)
                    {
                        if (lstCodeMap[j - 1].GID == 0)
                            tagF12Group.endCharCode = lstCodeMap[j - 2].Unicode;
                        else
                            tagF12Group.endCharCode = lstCodeMap[j - 1].Unicode;

                        entry.Format12.vtGroup.Add(tagF12Group);
                    }
                    break;
                }
            }

            // 向CMAP_ENCODE_F12赋值	
            entry.Format12.format = 12;
            usTmp = hy_cdr_int16_to(entry.Format12.format);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

            entry.Format12.reserved = 0;
            usTmp = hy_cdr_int16_to(entry.Format12.reserved);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);


            // 16 == format + reserved + length + language + nGroups
            // 12 == startCharCode + endCharCode + startGlyphID
            entry.Format12.length = (uint)(16 + entry.Format12.vtGroup.Count * 12);
            ulTmp = hy_cdr_int32_to(entry.Format12.length);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

            entry.Format12.language = 0;
            ulTmp = hy_cdr_int32_to(entry.Format12.language);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

            entry.Format12.nGroups = (uint)entry.Format12.vtGroup.Count;
            ulTmp = hy_cdr_int32_to(entry.Format12.nGroups);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

            ulTmp = 0;
            for (int x = 0; x < entry.Format12.vtGroup.Count; x++)
            {
                List<CMAP_ENCODE_FORMAT_12_GROUP> vtGroups = entry.Format12.vtGroup;
                ulTmp = hy_cdr_int32_to(vtGroups[x].startCharCode);
                btTmp = BitConverter.GetBytes(ulTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

                ulTmp = hy_cdr_int32_to(vtGroups[x].endCharCode);
                btTmp = BitConverter.GetBytes(ulTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

                ulTmp = hy_cdr_int32_to(vtGroups[x].startGlyphID);
                btTmp = BitConverter.GetBytes(ulTmp);
                for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);
            }

            entry.offset = (uint)(FileStrm.Position - entry.offset);
            for (int y = 0; y < vtCmap.Count; y++)
            {
                FileStrm.WriteByte(vtCmap[y]);
            }

        }   // end of protected HYRESULT	EncodeCmapFmt12()
        public void WriteTableEntry(ref FileStream WrtStrm,  CTableDirectory TbDirectory)
        {
            ushort searchRange = 0;
            searchRange = (ushort)(Math.Log((double)TbDirectory.numTables) / Math.Log(2.0));
            searchRange = (ushort)(Math.Pow(2.0, searchRange));
            searchRange = (ushort)(searchRange * 16);

            TbDirectory.searchRange = searchRange;
            TbDirectory.entrySelector = (ushort)(Math.Log((float)(TbDirectory.searchRange / 16)) / Math.Log(2.0));
            TbDirectory.rangeShift = (ushort)(TbDirectory.numTables * 16 - TbDirectory.searchRange);

            WrtStrm.Seek(0,SeekOrigin.Begin);            

            ushort usTmp;
            ulong ulTmp;
            // sfnt version
            usTmp = hy_cdr_int16_to((ushort)TbDirectory.version.value);
            byte[] btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to((ushort)TbDirectory.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);
            //numTables
            usTmp = hy_cdr_int16_to(TbDirectory.numTables);
            btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);
            // searchRange
            usTmp = hy_cdr_int16_to(TbDirectory.searchRange);            
            btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);

            // entrySelector
            usTmp = hy_cdr_int16_to(TbDirectory.entrySelector);
            btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);
            //rangeShift
            usTmp = hy_cdr_int16_to(TbDirectory.rangeShift);
            btTmp = BitConverter.GetBytes(usTmp);
            WrtStrm.Write(btTmp, 0, btTmp.Length);

            for (int i = 0; i < TbDirectory.numTables; i++) {
                CTableEntry HYEntry = TbDirectory.vtTableEntry[i];

                //tag		
                ulTmp = hy_cdr_int32_to(HYEntry.tag);
                btTmp = BitConverter.GetBytes(usTmp);
                WrtStrm.Write(btTmp, 0, btTmp.Length);
                // checkSum
                ulTmp = hy_cdr_int32_to(HYEntry.checkSum);
                btTmp = BitConverter.GetBytes(usTmp);
                WrtStrm.Write(btTmp, 0, btTmp.Length);
                //offset
                ulTmp = hy_cdr_int32_to(HYEntry.offset);
                btTmp = BitConverter.GetBytes(usTmp);
                WrtStrm.Write(btTmp, 0, btTmp.Length);
                //length
                ulTmp = hy_cdr_int32_to(HYEntry.length);
                btTmp = BitConverter.GetBytes(usTmp);
                WrtStrm.Write(btTmp, 0, btTmp.Length);
            }

        }	// end of void CFontExtract::WriteTableEntry()

        public HYRESULT PrepareExtractFont(string strTTFFile, ref HYDecode FontDecode)
        {
            try{
                FontDecode.FontOpen(strTTFFile);
            }
            catch (Exception ext){
                ext.ToString();
                return HYRESULT.FILE_OPEN;
            }
            
            if (FontDecode.tbDirectory.version.value != 1 && FontDecode.tbDirectory.version.fract != 0)
                return HYRESULT.NO_TTF; // 不是truetype
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.CMAP_TAG) == -1) return HYRESULT.CMAP_DECODE;
            FontDecode.DecodeCmap();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.MAXP_TAG) == -1) return HYRESULT.MAXP_DECODE;
            FontDecode.DecodeMaxp();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.HEAD_TAG) == -1) return HYRESULT.HEAD_DECODE;
            FontDecode.DecodeHead();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.LOCA_TAG) == -1) return HYRESULT.LOCA_DECODE;
            FontDecode.DecodeLoca();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.HHEA_TAG) == -1) return HYRESULT.HHEA_DECODE;
            FontDecode.DecodeHhea();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.HMTX_TAG) == -1) return HYRESULT.HMTX_DECODE;
            FontDecode.DecodeHmtx();
            if (FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.COLR_TAG) != -1) {
                FontDecode.DecodeCOLR();
            }
            
            // subset多为web应用，所以vmtx和vhea也可以删除掉
            int TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VHEA_TAG);
            if (TbIndex != -1){
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VMTX_TAG);
            if (TbIndex != -1){
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            //为尽量缩小subset字库的体积，需要去掉非必须表。
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.VDMX_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.HDMX_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.LTSH_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.CVT_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.FPGM_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }            
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.JSTF_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GDEF_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.BASE_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }            
            TbIndex = FontDecode.tbDirectory.FindTableEntry((uint)TABLETAG.GSUB_TAG);
            if (TbIndex != -1)
            {
                FontDecode.tbDirectory.vtTableEntry.RemoveAt(TbIndex);
                FontDecode.tbDirectory.numTables--;
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT PrepareExtractFont()

        byte[] FilterGlyphs(ref HYDecode Decode, ref List<UInt32> lstunicode,ref ushort maxGID)
        {
            int stUni = lstunicode.Count;
            int iEntryIndex = Decode.tbDirectory.FindTableEntry((int)TABLETAG.GLYF_TAG);
            CTableEntry entry = Decode.tbDirectory.vtTableEntry[iEntryIndex];
            Decode.DecodeStream.Seek(entry.offset, SeekOrigin.Begin);

            Decode.codemap.lstCodeMap.Clear();
            // 第一步确定miss char
            HYCodeMapItem mapItm = new HYCodeMapItem();
            mapItm.Unicode = 0xffff;
            mapItm.GID = 0;
            Decode.codemap.lstCodeMap.Add(mapItm);

            uint ulGlyphsLen = Decode.tbLoca.vtLoca[1];
            for (int i = 0; i < stUni; i++)
            {
                int iIndex = Decode.FindGryphIndexByUnciode(lstunicode[i]);
                if (iIndex == -1) continue;

                if (iIndex > maxGID)
                    maxGID = (ushort)iIndex;
                
                ulGlyphsLen += GetGlyphBufLength(ref Decode, entry.offset, (ushort)iIndex,ref maxGID);

            }
            // 分配好子集字形内存            
            uint Real = (ulGlyphsLen + 3) / 4 * 4;
            byte[] pOutGlyphs = new byte[Real];

            CLoca local = new CLoca();
            // 第一步抽取miss char数据		
            local.vtLoca.Add(0);
            uint glyphlenth = Decode.tbLoca.vtLoca[1] - Decode.tbLoca.vtLoca[0];
            if (glyphlenth > 0){
                Decode.DecodeStream.Seek(entry.offset + Decode.tbLoca.vtLoca[0], SeekOrigin.Begin);
                Decode.DecodeStream.Read(pOutGlyphs, 0, (int)glyphlenth);
            }

            uint iBuffOffset = glyphlenth;
            for (int i = 1; i < maxGID+1; i++) {
                if (CheckGid(ref Decode.codemap.lstCodeMap, i)){
                    local.vtLoca.Add(iBuffOffset);
                }
                else {
                    local.vtLoca.Add(iBuffOffset);
                    // 获取GLYH的长度
                    glyphlenth = Decode.tbLoca.vtLoca[i + 1] - Decode.tbLoca.vtLoca[i];
                    Decode.DecodeStream.Seek(entry.offset + Decode.tbLoca.vtLoca[i], SeekOrigin.Begin);
                    Decode.DecodeStream.Read(pOutGlyphs, (int)iBuffOffset, (int)glyphlenth);
                    iBuffOffset += glyphlenth;
                }
            }
            local.vtLoca.Add(iBuffOffset);
            Decode.tbLoca = local;

            return pOutGlyphs;

        }	// end of int CFontExtract::FilterGlyphs()

        uint GetGlyphBufLength(ref HYDecode Decode, uint offsetTable, ushort GID, ref ushort maxGID)
        {
            uint ulLength = 0;
            if (CheckGid(ref Decode.codemap.lstCodeMap, GID))
            {
                uint lca0 = Decode.tbLoca.vtLoca[GID];
                uint lca1 = Decode.tbLoca.vtLoca[GID + 1];
                if (lca1 > lca0)
                {
                    Decode.DecodeStream.Seek(offsetTable + lca0, SeekOrigin.Begin);
                    List<int> vtCmpGID = new List<int>();
                    if (IsCompositeGlyph(ref Decode, ref vtCmpGID))
                    {
                        for (int j = 0; j < vtCmpGID.Count; j++)
                        {
                            if (vtCmpGID[j] > maxGID)
                                maxGID = (ushort)vtCmpGID[j];

                            ulLength += GetGlyphBufLength(ref Decode, offsetTable, (ushort)vtCmpGID[j], ref maxGID);
                        }
                    }
                    ulLength += Decode.tbLoca.vtLoca[GID + 1] - Decode.tbLoca.vtLoca[GID];

                    List<uint> szuni = new List<uint>();
                    Decode.FindGryphUncidoByIndex(GID, ref szuni);
                    if (szuni.Count == 0)
                    {
                        HYCodeMapItem item = new HYCodeMapItem();
                        item.GID = GID;
                        item.Unicode = 0xffffffff;
                        Decode.codemap.lstCodeMap.Add(item);
                    }
                    else
                    {
                        foreach (uint uni in szuni)
                        {
                            HYCodeMapItem item = new HYCodeMapItem();
                            item.GID = GID;
                            item.Unicode = uni;
                            Decode.codemap.lstCodeMap.Add(item);
                        }
                    }
                }
            }

            return ulLength;

        }   // end of void GetGlyphBufLength()

        bool IsCompositeGlyph(ref HYDecode Decode, ref List<int> vtCmpGID)
        {
            ushort usTmp = 0;            
            ushort flags = 0;

            byte[] array = new byte[4];
            Decode.DecodeStream.Read(array, 0, 2);
            short sCmp = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            // box
            Decode.DecodeStream.Seek(8, SeekOrigin.Current);
            //numberOfContours
            if (sCmp == -1) {
                do {
                    //flags
                    Decode.DecodeStream.Read(array, 0, 2);
                    flags = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));


                    //glyphIndex
                    Decode.DecodeStream.Read(array, 0, 2);
                    usTmp = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    if (CheckGid(ref vtCmpGID, (int)usTmp))
                        vtCmpGID.Add(usTmp);                        

                    if ((flags&CMPSTFLAG.GLYF_CMPST_ARG_1_AND_2_ARE_WORDS)>0) {
                        Decode.DecodeStream.Seek(4, SeekOrigin.Current);
                    }
                    else  {
                        Decode.DecodeStream.Seek(2, SeekOrigin.Current);
                    }

                    if ((flags&CMPSTFLAG.GLYF_CMPST_WE_HAVE_A_SCALE)>0) {
                        Decode.DecodeStream.Seek(2, SeekOrigin.Current); 
                    }
                    else if ((flags & CMPSTFLAG.GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE)>0) {
                        Decode.DecodeStream.Seek(4, SeekOrigin.Current);
                    }
                    else if ((flags & CMPSTFLAG.GLYF_CMPST_WE_HAVE_A_TWO_BY_TWO)>0){
                        Decode.DecodeStream.Seek(8, SeekOrigin.Current);
                    }

                } while ((flags & CMPSTFLAG.GLYF_CMPST_MORE_COMPONENT)>0);

                return true;
            }

            return false;

        }	// end of int IsCompositeGlyph()
        bool CheckGid(ref List<int> vtGID, int iGid)
        {
            if (iGid == -1) return false;
            
            for (int i = 0; i < vtGID.Count; i++) {
                if (vtGID[i] == iGid) return false;
            }

            return true;

        }	// end of bool CheckGid

        bool CheckGid(ref List<HYCodeMapItem> lstCodeMap, int iGid)
        {
            if (iGid == -1) return false;
            
            for (int i = 0; i < lstCodeMap.Count; i++)  {
                if (lstCodeMap[i].GID == iGid) 
                    return false;
            }

            return true;

        }	// end of BOOL CFontExtract::CheckGid()

        public HYRESULT ModifyFontInfo(string strFileName, string strNewFileName,HYFontInfo Fntinf)
        {
            HYRESULT sult;
            HYDecode FontDecode = new HYDecode();            
        
            try
            {
                FontDecode.FontOpen(strFileName);
            }
            catch (Exception ext)
            {
                ext.ToString();
                return HYRESULT.FILE_OPEN;
            }

            HYEncodeBase FontEncode = new HYEncodeBase();
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
                    FontEncode.Encodehead();
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

        HYRESULT Extrace(HYDecode FontDecode, ref HYEncodeBase FontEncode, List<uint> lstUnicode, ref List<byte> lstGlyphData,ref List<UInt32> lstMssUni)
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

        }   // end of HYRESULT Extrace()

        bool BuildGlyphData(
                            HYDecode FontDecode, 
                            HYEncodeBase FontEncode,
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

        HYRESULT BulidFont(string strFontName, HYDecode FontDecode, HYEncodeBase FontEncode, List<byte> lstGlyphData, List<uint> vtTableFlag,List<uint> lstUnicode)
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
                    tbEncodeEntry.length = HYDecodeEntry.length;
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

        HYRESULT BulidCMAP(HYEncodeBase FontEnCodec, List<uint> lstUnicoe)
        {
            int iEncodeEntryIndex = FontEnCodec.tbDirectory.FindTableEntry((UInt32)TABLETAG.CMAP_TAG);
            CTableEntry tbEncodeEntry = FontEnCodec.tbDirectory.vtTableEntry[iEncodeEntryIndex];
            tbEncodeEntry.offset = (uint)FontEnCodec.EncodeStream.Position;

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

        public HYRESULT Encodemaxp(HYEncodeBase FontEnCodec)
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

        public HYRESULT Encodehmtx(HYEncodeBase FontEnCodec)
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
