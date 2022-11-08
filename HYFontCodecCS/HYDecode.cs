using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using FontParserEntity;


namespace HYFontCodecCS
{

    public class HYDecode : HYFontBase
    {
        public FileStream DecodeStream
        {            
            get { return FileStrm; }
        }        

        public HYRESULT FontOpen(string strFileName)
        {
            try
            {
                FileStrm = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                DecodeTableDirectory();
            }
            catch
            {
                throw;
            }

            return HYRESULT.NOERROR;
        
        }   // end of public HYRESULT FontOpen()

        public HYRESULT FontClose()
        {
            if (FileStrm != null)
            {
                FileStrm.Close();            
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontOpen()

        // get Maxp Head Hmtx 
        public HYRESULT GetTable(string strFileName)
        {
            try
            {
                FileStrm = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch
            {
                throw;
            }
            Chars = new FontParserEntity.CharsInfo();
            HYRESULT rtResult;
            // 获取字典目录
            DecodeTableDirectory();
            rtResult = DecodeMaxp();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            // 解析Head表
            rtResult = DecodeHead();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            FontClose();

            return HYRESULT.NOERROR;

        }   //end of public HYRESULT GetTable()
        public HYRESULT GetTableData(UInt32 tag, ref CTableEntry tableData)
        {
            if(FileStrm !=null)
            {

                int iTbInx = TableDirectorty.FindTableEntry(tag);
                if (iTbInx !=-1)
                {
                    CTableEntry tbEntry = TableDirectorty.vtTableEntry[iTbInx];
                    FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

                    tableData.aryTableData = new byte[tbEntry.length];
                    FileStrm.Read(tableData.aryTableData,0,(int)tbEntry.length);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT GetTableData()

        //解码字库指定表,其它表不做处理
        public HYRESULT FontDecode(string strFileName)
        {
            try
            {
                FileStrm = new FileStream(strFileName, FileMode.Open, FileAccess.Read,FileShare.Read);
            }
            catch
            {
                throw;            
            }
            Chars = new FontParserEntity.CharsInfo();            

            HYRESULT rtResult;
            // 获取字典目录
            DecodeTableDirectory();
            
            rtResult = DecodeMaxp();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            // 解析Head表
            rtResult = DecodeHead();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            // 解析CMAP表
            rtResult = DecodeCmap();
            if (rtResult != HYRESULT.NOERROR) return rtResult;

            // 解析POST表
            rtResult = DecodePost();
            if (rtResult != HYRESULT.NOERROR) return rtResult;

            if (FontType == FONTTYPE.TTF)
            {                
                Chars.Type = "TrueType";
                // 解析LOCA表
                rtResult = DecodeLoca();
                if (rtResult != HYRESULT.NOERROR) return rtResult;
                // 解析GLYPH表
                rtResult = DecodeGlyph();
                if (rtResult != HYRESULT.NOERROR) return rtResult;
            }
            if (FontType == FONTTYPE.CFF)
            {
                Chars.Type = "OpenType";
                rtResult = DecodeCFF();
                if (rtResult != HYRESULT.NOERROR) return rtResult;
            }

            // 解析Name表
            rtResult = DecodeName();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            // 解析HHEA表
            rtResult = DecodeHhea();
            if (rtResult != HYRESULT.NOERROR) return rtResult;
            // 解析HMTX表
            rtResult = DecodeHmtx();
            if (rtResult != HYRESULT.NOERROR) return rtResult;

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.COLR_TAG);
            if (iEntryIndex != -1)
            {
                DecodeCOLR();
            }
             
            if (FileStrm != null)
            {
                FileStrm.Close();
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontDecode()

        public HYRESULT DecodeTableDirectory()
	    {            
            byte[] array = new byte[4];
            TableDirectorty = new CTableDirectory();

			// sfnt version
            FileStrm.Read(array,0,2);
            TableDirectorty.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
            FileStrm.Read(array,0,2);
			TableDirectorty.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
			if (TableDirectorty.version.value==0x4f54&&TableDirectorty.version.fract==0x544f)
			{
			   FontType=FONTTYPE.CFF;
			}

			if (TableDirectorty.version.value==0x0001&&TableDirectorty.version.fract==0x0000)
			{
			   FontType=FONTTYPE.TTF;
			}
            
            //numTables
            FileStrm.Read(array,0,2);
            TableDirectorty.numTables = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
	        //searchRange
            FileStrm.Read(array,0,2);
            TableDirectorty.searchRange = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));			
			//entrySelector
            FileStrm.Read(array,0,2);
            TableDirectorty.entrySelector = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
			//rangeShift
            FileStrm.Read(array,0,2);
            TableDirectorty.rangeShift = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
			
            for (UInt16 i=0; i<TableDirectorty.numTables; i++)
			{
                CTableEntry HYEntry = new CTableEntry();
				//tag		
                FileStrm.Read(array,0,4);
                HYEntry.tag = hy_cdr_int32_to(BitConverter.ToUInt32(array,0));
				// checkSum
                FileStrm.Read(array, 0, 4);
                HYEntry.checkSum = hy_cdr_int32_to(BitConverter.ToUInt32(array,0));
				//offset
                FileStrm.Read(array, 0, 4);
                HYEntry.offset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
				//length
                FileStrm.Read(array, 0, 4);
                HYEntry.length = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

                TableDirectorty.vtTableEntry.Add(HYEntry);
			}

			return HYRESULT.NOERROR;		    

	    }	// end of int CHYFontCodec::DecodeTableDirectory()

        public HYRESULT DecodeMaxp()
        {        
            Maxp = new CMaxp();
            
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.MAXP_TAG);
            if (iEntryIndex == -1) return HYRESULT.MAXP_DECODE;

            CTableEntry  tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            if (FontType == FONTTYPE.TTF)
            {
                FileStrm.Read(array, 0, 2);
                Maxp.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.numGlyphs = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxPoints = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxContours = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxCompositePoints = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxCompositeContours = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxZones = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxTwilightPoints = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxStorage = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxFunctionDefs = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxInstructionDefs = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxStackElements = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxSizeOfInstructions = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxComponentElements = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.maxComponentDepth = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));               
            }

            if (FontType == FONTTYPE.CFF)
            {
                FileStrm.Read(array, 0, 2);
                Maxp.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Maxp.numGlyphs = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeMaxp()

        public HYRESULT DecodeHead()
        {
            Head = new CHead();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.HEAD_TAG);
            if (iEntryIndex == -1) return HYRESULT.HEAD_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            //Table version number
            FileStrm.Read(array, 0, 2);
            Head.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Head.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //fontRevision
            FileStrm.Read(array, 0, 2);
            Head.fontRevision.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Head.fontRevision.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //checkSumAdjustment
            FileStrm.Read(array, 0, 4);           
            Head.checkSumAdjustment = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));         
            // magicNumber
            FileStrm.Read(array, 0, 4);
            Head.magicNumber = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //flags
            array = new byte[4];
            FileStrm.Read(array, 0, 2);
            Head.flags = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //unitsPerEm
            FileStrm.Read(array, 0, 2);
            Head.unitsPerEm = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //created
            Head.created = new byte[8];
            FileStrm.Read(Head.created, 0, 8);
            //modified
            Head.modified = new byte[8];
            FileStrm.Read(Head.modified, 0, 8);

            //xMin
            FileStrm.Read(array, 0, 2);
            Head.xMin = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // yMin
            FileStrm.Read(array, 0, 2);
            Head.yMin = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //xMax
            FileStrm.Read(array, 0, 2);
            Head.xMax = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // yMax
            FileStrm.Read(array, 0, 2);
            Head.yMax = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // macStyle
            FileStrm.Read(array, 0, 2);
            Head.macStyle = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //lowestRecPPEM
            FileStrm.Read(array, 0, 2);
            Head.lowestRecPPEM = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //fontDirectionHint
            FileStrm.Read(array, 0, 2);
            Head.fontDirectionHint = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //indexToLocFormat
            FileStrm.Read(array, 0, 2);
            Head.indexToLocFormat = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //glyphDataFormat
            FileStrm.Read(array, 0, 2);
            Head.glyphDataFormat = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            string strRect;
            strRect = Head.xMin.ToString()+",";
            strRect += Head.yMin.ToString()+",";
            strRect += (Head.xMax - Head.xMin).ToString()+",";
            strRect += (Head.yMax - Head.yMin).ToString();

            if (Chars != null)
            {
                Chars.HeadInfo.Em = Head.unitsPerEm;
                Chars.HeadInfo.MaxRect = strRect;
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeHead()

        public HYRESULT DecodeCmap()
        {
            Cmap = new CCmap();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.CMAP_TAG);
            if (iEntryIndex == -1) return HYRESULT.CMAP_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            long TableStartPt = FileStrm.Position;

            FileStrm.Read(array, 0, 2);
            Cmap.version = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Cmap.numSubTable = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            for (UInt16 i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbMAPEntry = new CMAP_TABLE_ENTRY();
                FileStrm.Read(array, 0, 2);
                tbMAPEntry.plat_ID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                tbMAPEntry.plat_encod_ID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 4);
                tbMAPEntry.offset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
                Cmap.vtCamp_tb_entry.Add(tbMAPEntry);
            }

            for (UInt16 i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbMAPEntry = Cmap.vtCamp_tb_entry[i];
                FileStrm.Seek(TableStartPt + tbMAPEntry.offset, SeekOrigin.Begin);

                FileStrm.Read(array, 0, 2);
                tbMAPEntry.format = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                switch (tbMAPEntry.format)
                {
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_0:
                        DecodeCmapFmt0(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_2:
                        DecodeCmapFmt2(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_4:
                        DecodeCmapFmt4(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_6:
                        DecodeCmapFmt6(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_8:
                        DecodeCmapFmt8(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_10:
                        DecodeCmapFmt10(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_12:
                        DecodeCmapFmt12(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_13:
                        DecodeCmapFmt13(tbMAPEntry);
                        break;
                    case (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_14:
                        DecodeCmapFmt14(tbMAPEntry);
                        break;
                    default:
                        break;
                }            
            }

            return HYRESULT.NOERROR;

        }   //end of protected HYRESULT DecodeCmap()

        public HYRESULT DecodeOS2()
        {
            OS2 = new COS2();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.OS2_TAG);
            if (iEntryIndex == -1) return HYRESULT.OS2_DECODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            //Table version number
            FileStrm.Read(array, 0, 2);
            OS2.version = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // Average weighted 
            FileStrm.Read(array, 0, 2);
            OS2.xAvgCharWidth= (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // Indicates the visual weight (degree of blackness or thickness of strokes) of the characters in the font
            FileStrm.Read(array, 0, 2);
            OS2.usWeightClass = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // usWidthClass
            FileStrm.Read(array, 0, 2);
            OS2.usWidthClass = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // Indicates font embedding licensing rights for the font. Embeddable fonts may be stored in a document
            FileStrm.Read(array, 0, 2);
            OS2.fsType = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // Subscript horizontal font size.            
            FileStrm.Read(array, 0, 2);
            OS2.ySubscriptXSize = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // Subscript vertical font size..
            FileStrm.Read(array, 0, 2);
            OS2.ySubscriptYSize = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Subscript x offset
            FileStrm.Read(array, 0, 2);
            OS2.ySubscriptXOffset = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Subscript y offset
            FileStrm.Read(array, 0, 2);
            OS2.ySubscriptYOffset = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Subscript y offset
            FileStrm.Read(array, 0, 2);
            OS2.ySuperscriptXSize = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Superscript vertical font size
            FileStrm.Read(array, 0, 2);
            OS2.ySuperscriptYSize = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Superscript x offset
            FileStrm.Read(array, 0, 2);
            OS2.ySuperscriptXOffset = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Superscript y offset
            FileStrm.Read(array, 0, 2);
            OS2.ySuperscriptYOffset = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Strikeout size
            FileStrm.Read(array, 0, 2);
            OS2.yStrikeoutSize = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Strikeout position.
            FileStrm.Read(array, 0, 2);
            OS2.yStrikeoutPosition = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //Font-family class and subclass.  Also see section 3.4
            FileStrm.Read(array, 0, 2);
            OS2.sFamilyClass = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //PANOSE classification number		
            OS2.panose.FamilyType = (byte)FileStrm.ReadByte();
            OS2.panose.SerifStyle = (byte)FileStrm.ReadByte();
            OS2.panose.Weight = (byte)FileStrm.ReadByte();
            OS2.panose.Proportion = (byte)FileStrm.ReadByte();
            OS2.panose.Contrast = (byte)FileStrm.ReadByte();
            OS2.panose.StrokeVariation = (byte)FileStrm.ReadByte();
            OS2.panose.ArmStyle = (byte)FileStrm.ReadByte();
            OS2.panose.Letterform = (byte)FileStrm.ReadByte();
            OS2.panose.Midline = (byte)FileStrm.ReadByte();
            OS2.panose.XHeight = (byte)FileStrm.ReadByte();
            // ulUnicodeRange1
            FileStrm.Read(array, 0, 4);
            OS2.ulUnicodeRange1 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            // ulUnicodeRange2
            FileStrm.Read(array, 0, 4);
            OS2.ulUnicodeRange2 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            // ulUnicodeRange3
            FileStrm.Read(array, 0, 4);
            OS2.ulUnicodeRange3 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            // ulUnicodeRange4
            FileStrm.Read(array, 0, 4);
            OS2.ulUnicodeRange4 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            // Font Vendor Identification
            FileStrm.Read(array, 0, 4);
            OS2.vtachVendID = array.ToList<byte>();
            //.Add(array[0]);
            //OS2.vtachVendID.Add(array[1]);
            //OS2.vtachVendID.Add(array[2]);
            //OS2.vtachVendID.Add(array[3]);
            // Font selection flags
            FileStrm.Read(array, 0, 2);
            OS2.fsSelection = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The minimum Unicode index (character code) in this font
            FileStrm.Read(array, 0, 2);
            OS2.usFirstCharIndex = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The maximum Unicode index (character code) in this font,
            FileStrm.Read(array, 0, 2);
            OS2.usLastCharIndex = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));            
            // The typographic ascender for this font. 
            FileStrm.Read(array, 0, 2);
            OS2.sTypoAscender = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The typographic descender for this font. 
            FileStrm.Read(array, 0, 2);
            OS2.sTypoDescender = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The typographic line gap for this font
            FileStrm.Read(array, 0, 2);
            OS2.sTypoLineGap = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The ascender metric for Windows
            FileStrm.Read(array, 0, 2);
            OS2.usWinAscent = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            // The descender metric for Windows
            FileStrm.Read(array, 0, 2);
            OS2.usWinDescent = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            if (OS2.version > 0)
            {
                // ulCodePageRange1
                FileStrm.Read(array, 0, 4);
                OS2.ulCodePageRange1 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
                // ulCodePageRange2
                FileStrm.Read(array, 0, 4);
                OS2.ulCodePageRange2 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            }

            if (OS2.version > 1)
            {
                // xHeight
                FileStrm.Read(array, 0, 2);
                OS2.sxHeight = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)); 
                // sCapHeight
                FileStrm.Read(array, 0, 2);
                OS2.sCapHeight = (short)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                // usDefaultChar
                FileStrm.Read(array, 0, 2);
                OS2.usDefaultChar = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                // usBreakChar
                FileStrm.Read(array, 0, 2);
                OS2.usBreakChar = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                // usBreakChar
                FileStrm.Read(array, 0, 2);
                OS2.usMaxContext = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeOS2()

        protected HYRESULT DecodeCmapFmt0(CMAP_TABLE_ENTRY entry)
	    {
            CMAP_ENCODE_FORMAT_0		CEF0 = entry.Format0;
			CEF0.format = 0;

			// length
            byte[] array = new byte[4];
            FileStrm.Read(array, 0, 2);
			CEF0.length = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

			// language
            FileStrm.Read(array, 0, 2);			
			CEF0.language =  hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));		

			// glyphIdArray			
			for (int i=0; i<256; i++)
			{				
                FileStrm.Read(array, 0, 1);
                CEF0.vtGlyphId.Add(array[0]);			
			}

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt0()        

        protected HYRESULT DecodeCmapFmt2(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt2()

        protected HYRESULT DecodeCmapFmt4(CMAP_TABLE_ENTRY entry)
        {
            byte[] array = new byte[4];

            CMAP_ENCODE_FORMAT_4  CEF4 = entry.Format4;
            CEF4.format = 4;

            long ulStart = FileStrm.Position - 2;
            FileStrm.Read(array, 0, 2);
            CEF4.length = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            CEF4.language = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            CEF4.segCountX2 = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            CEF4.searchRange = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            CEF4.entrySelector = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            CEF4.rangeShift = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            
            for (UInt16 i = 0; i<CEF4.segCountX2 >> 1; i++)
		    {
                FileStrm.Read(array, 0, 2);
                CEF4.vtEndCount.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));
		    }

            FileStrm.Read(array, 0, 2);
            CEF4.reservedPad = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            for (UInt16 i = 0; i < CEF4.segCountX2 >> 1; i++)
            {
                FileStrm.Read(array, 0, 2);
                CEF4.vtstartCount.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));
            }

            for (UInt16 i = 0; i < CEF4.segCountX2 >> 1; i++)
            {
                FileStrm.Read(array, 0, 2);
                CEF4.vtidDelta.Add((Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));
            }

            for (UInt16 i = 0; i < CEF4.segCountX2 >> 1; i++)
            {
                FileStrm.Read(array, 0, 2);
                CEF4.vtidRangeOffset.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));
            }

            long ulCurrent = FileStrm.Position;
            long ipglyphIdArrayLen = (CEF4.length - (UInt16)(ulCurrent - ulStart))/2;	
		    for (long i=0; i<ipglyphIdArrayLen; i++)
		    {
                FileStrm.Read(array, 0, 2);
                CEF4.vtglyphIdArray.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));			  
		    }

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt4()

        protected HYRESULT DecodeCmapFmt6(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt6()

        protected HYRESULT DecodeCmapFmt8(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt8()

        protected HYRESULT DecodeCmapFmt10(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt10()

        protected HYRESULT DecodeCmapFmt12(CMAP_TABLE_ENTRY entry)
        {
            byte[] array = new byte[4];

            CMAP_ENCODE_FORMAT_12 CEF12 = entry.Format12;
            CEF12.format = 12;

            FileStrm.Read(array, 0, 2);

            FileStrm.Read(array, 0, 4);
            CEF12.length = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            FileStrm.Read(array, 0, 4);
            CEF12.language = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            FileStrm.Read(array, 0, 4);
            CEF12.nGroups = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            for (UInt32 i=0; i<CEF12.nGroups; i++)
		    {
			    CMAP_ENCODE_FORMAT_12_GROUP  group = new CMAP_ENCODE_FORMAT_12_GROUP();

                FileStrm.Read(array, 0, 4);
                group.startCharCode = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

                FileStrm.Read(array, 0, 4);
                group.endCharCode = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

                FileStrm.Read(array, 0, 4);
                group.startGlyphID = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

			    CEF12.vtGroup.Add(group);
		    }

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt12()

        protected HYRESULT DecodeCmapFmt13(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt12()

        protected HYRESULT DecodeCmapFmt14(CMAP_TABLE_ENTRY entry)
        {

            return HYRESULT.NOERROR;

        }	// end of HYRESULT DecodeCmapFmt12()

        public HYRESULT DecodeName()
        {
            Name = new CName();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.NAME_TAG);
            if (iEntryIndex == -1) return HYRESULT.NAME_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            long TbStart = FileStrm.Position;
            
            FileStrm.Read(array, 0, 2);
            Name.Format = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Name.count = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Name.offset = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            for (UInt16 i = 0; i < Name.count; i++)
            {
                NAMERECORD NameRecord = new NAMERECORD();
                FileStrm.Read(array, 0, 2);
                NameRecord.platformID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                NameRecord.encodingID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                NameRecord.languageID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                NameRecord.nameID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                NameRecord.length = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                NameRecord.offset = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                
                long CurrentPos = FileStrm.Position;
                FileStrm.Seek(TbStart + Name.offset + NameRecord.offset,SeekOrigin.Begin);

                Byte[] Content = new Byte[NameRecord.length];
                FileStrm.Read(Content, 0, NameRecord.length);

                if (
                    (NameRecord.platformID == 3 && NameRecord.encodingID == 1) ||
                    (NameRecord.platformID == 0 && NameRecord.encodingID == 3) ||
                    (NameRecord.platformID == 0 && NameRecord.encodingID == 4) ||
                    (NameRecord.platformID == 0 && NameRecord.encodingID == 6)	
                    )                    
                    NameRecord.strContent = System.Text.Encoding.BigEndianUnicode.GetString(Content);               
                else
                    NameRecord.strContent = System.Text.Encoding.Default.GetString(Content);

                if (Chars != null)
                {
                    if (NameRecord.platformID == 3 && NameRecord.encodingID == 1 && NameRecord.nameID == 5)
                    {
                        Chars.Version = NameRecord.strContent;
                    }               
                }

                Name.vtNameRecord.Add(NameRecord);
                FileStrm.Seek(CurrentPos, SeekOrigin.Begin);         
            }

            if (Name.Format == 1)
            {
                FileStrm.Read(array, 0, 2);
                UInt16 langTagCount = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                for (UInt16 i = 0; i < langTagCount; i++)
                {
                    LANGTAGRECORD langTagRecord = new LANGTAGRECORD();

                    FileStrm.Read(array, 0, 2);
                    langTagRecord.length = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    FileStrm.Read(array, 0, 2);
                    langTagRecord.offset = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    FileStrm.Read(array, 0, 2);
                    langTagRecord.offset = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    long CurrentPos = FileStrm.Position;
                    FileStrm.Seek(TbStart + Name.offset + langTagRecord.offset,SeekOrigin.Begin);

                    Byte[] langTagContent = new Byte[langTagRecord.length];
                    FileStrm.Read(langTagContent, 0, langTagRecord.length);

                    Name.vtLangTargeRecord.Add(langTagRecord);

                    FileStrm.Seek(CurrentPos, SeekOrigin.Begin);                    
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeName()

        public HYRESULT DecodeLoca()
        {
            Loca = new CLoca();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.LOCA_TAG);
            if (iEntryIndex == -1) return HYRESULT.LOCA_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            Int16 sIndexToLocFormat = Head.indexToLocFormat;
			UInt32 usArraySize = Maxp.numGlyphs;
            usArraySize++;

            if (sIndexToLocFormat == 0)
			{
                UInt32 uTmp = 0;
				for (UInt32 i = 0; i<usArraySize; i++)
				{				
                    FileStrm.Read(array, 0, 2);
                    uTmp = (UInt32)(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)) * 2);
                    Loca.vtLoca.Add(uTmp);
				}				
			}	

            if (sIndexToLocFormat == 1)
            {
                for (UInt32 i = 0; i<usArraySize; i++)
				{
                    FileStrm.Read(array, 0, 4);
                    Loca.vtLoca.Add(hy_cdr_int32_to(BitConverter.ToUInt32(array, 0)));
				}
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeLoca()

        public HYRESULT DecodeGlyph()
        {            
            byte[] array = new byte[4];

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.GLYF_TAG);
            if (iEntryIndex == -1) return HYRESULT.GLYF_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            long TableStartPoint = FileStrm.Position;           
            for (UInt16 i = 0; i < Maxp.numGlyphs; i++)
            {                
                CharInfo  charinf = new CharInfo();
                charinf.Id = i;
                charinf.Name = Post.FindNameByGID(i);

                List<uint> lstUnicode = new List<uint>();
                FindGryphUncidoByIndex(i, ref lstUnicode);

                charinf.Unicode = "";
                for (int itmp = 0; itmp < lstUnicode.Count; itmp++)
                {
                    charinf.Unicode += Convert.ToString(lstUnicode[itmp], 10);
                    if (itmp != lstUnicode.Count - 1)
                        charinf.Unicode += "|";
                }
                
                if (Loca.vtLoca[i] < Loca.vtLoca[i + 1])
                {
                    FileStrm.Seek(TableStartPoint + Loca.vtLoca[i], SeekOrigin.Begin);

                    FileStrm.Read(array, 0, 2);
                    charinf.ContourCount = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    FileStrm.Read(array, 0, 2);                    
                    Int16 xMin = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    FileStrm.Read(array, 0, 2);
                    Int16 yMin = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    FileStrm.Read(array, 0, 2);
                    Int16 xMax = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    FileStrm.Read(array, 0, 2);
                    Int16 yMax = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    charinf.Section = xMin.ToString() + ",";
                    charinf.Section += yMin.ToString() + ",";
                    charinf.Section += (xMax - xMin).ToString() + ",";
                    charinf.Section += (yMax - yMin).ToString();

                    charinf.AdHeight = GetGlyfAdvancHeight(i);
                    charinf.AdWidth = GetGlyfAdvancWidth(i);

                    if (charinf.ContourCount > 0)
                    {
                        charinf.IsComponent = 0;
                        DecodeGLYF_SIMPLE(charinf);
                    }
                    else
                    {
                        charinf.IsComponent = 1;
                        DecodeGLYF_COMPOS(charinf);
                    }
                }
                if (Chars != null)
                {
                    Chars.CharInfo.Add(charinf);
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeGlyph()

        public HYRESULT DecodeHhea()
        {
            Hhea = new CHhea();
            byte[] array = new byte[4];

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.HHEA_TAG);
            if (iEntryIndex == -1) return HYRESULT.HHEA_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            // version 
            FileStrm.Read(array, 0, 2);
			Hhea.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
			Hhea.version.fract= hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
			
            // Typographic ascent
            FileStrm.Read(array, 0, 2);
            Hhea.Ascender = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.Descender = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
			Hhea.LineGap = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.advanceWidthMax = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.minLeftSideBearing  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.minRightSideBearing  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.xMaxExtent  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.caretSlopeRise  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.caretSlopeRun  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.caretOffset  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            Hhea.reserved1  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.reserved2  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.reserved3  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.reserved4  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.metricDataFormat  = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Hhea.numberOfHMetrics  = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
        
            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeHhea()

        public HYRESULT DecodeHmtx1(List<uint> lstUni)
        {
            Hmtx = new CHmtx();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.HMTX_TAG);
            if (iEntryIndex == -1) return HYRESULT.HHEA_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);
            
            for (UInt16 i = 0; i < Hhea.numberOfHMetrics; i++)
            {
                HMTX_LONGHORMERTRIC Longhormetric = new HMTX_LONGHORMERTRIC();

                FileStrm.Read(array, 0, 2);
                Longhormetric.advanceWidth = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                FileStrm.Read(array, 0, 2);
                Longhormetric.lsb = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                Hmtx.lstLonghormetric.Add(Longhormetric);
            }

            UInt16 uTmp = Hmtx.lstLonghormetric[Hhea.numberOfHMetrics - 1].advanceWidth;
            int LeftSideBearingNum = Maxp.numGlyphs - Hhea.numberOfHMetrics;
            for (int i = 0; i < LeftSideBearingNum; i++)
            {
                HMTX_LONGHORMERTRIC Longhormetric = new HMTX_LONGHORMERTRIC();
                FileStrm.Read(array, 0, 2);

                Longhormetric.lsb = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                Longhormetric.advanceWidth = uTmp;
                Hmtx.lstLonghormetric.Add(Longhormetric);
            }

            for (int i=0; i< lstUni.Count; i++)
            {
                int iIndex = FindGryphIndexByUnciode(lstUni[i]);
                if (iIndex !=-1)
                {
                    if (Chars != null)
                    {
                        Chars.CharInfo[i].AdWidth = Hmtx.lstLonghormetric[iIndex].advanceWidth;
                    }
                }

            }

            return HYRESULT.NOERROR;
        }

        public HYRESULT DecodeHmtx()
        {
            Hmtx = new CHmtx();     
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.HMTX_TAG);
            if (iEntryIndex == -1) return HYRESULT.HHEA_DECODE;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            int iCharIndex = 0;
			for (UInt16 i=0; i<Hhea.numberOfHMetrics; i++)
			{
				HMTX_LONGHORMERTRIC  Longhormetric = new HMTX_LONGHORMERTRIC();

                FileStrm.Read(array, 0, 2);
				Longhormetric.advanceWidth = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

				FileStrm.Read(array, 0, 2);
				Longhormetric.lsb = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

				Hmtx.lstLonghormetric.Add(Longhormetric);

                if (Chars != null)
                {
                    if (Chars.CharInfo.Count>0)
                        Chars.CharInfo[iCharIndex++].AdWidth = Longhormetric.advanceWidth;
                }                
			}

            UInt16 uTmp = Hmtx.lstLonghormetric[Hhea.numberOfHMetrics-1].advanceWidth;
			int  LeftSideBearingNum = Maxp.numGlyphs-Hhea.numberOfHMetrics;			
			for (int i=0; i<LeftSideBearingNum; i++)
			{
                HMTX_LONGHORMERTRIC Longhormetric = new HMTX_LONGHORMERTRIC();
                FileStrm.Read(array, 0, 2);

                Longhormetric.lsb = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                Longhormetric.advanceWidth = uTmp;

                Hmtx.lstLonghormetric.Add(Longhormetric);

                if (Chars != null)
                {
                    if (Chars.CharInfo.Count > 0)
                        Chars.CharInfo[iCharIndex++].AdWidth = uTmp;
                }
                
			}	

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeHmtx()

        public HYRESULT DecodeVhea()
        {
            Vhea = new CVhea();
            byte[] array = new byte[4];

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.VHEA_TAG);
            if (iEntryIndex == -1) return HYRESULT.VHEA_NOEXIST;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            // version 
            FileStrm.Read(array, 0, 2);
            Vhea.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            // Typographic ascent
            FileStrm.Read(array, 0, 2);
            Vhea.ascent = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.descent = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.lineGap = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.advanceHeightMax = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.minTop = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.minBottom = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.yMaxExtent = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.caretSlopeRise = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.caretSlopeRun = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.caretOffset = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 2);
            Vhea.reserved1 = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.reserved2 = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.reserved3 = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.reserved4 = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.metricDataFormat = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Vhea.numOfLongVerMetrics = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeVhea()

        public HYRESULT DecodeVmtx()
        {
            Vmtx = new CVmtx();
            byte[] array = new byte[4];
            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.VMTX_TAG);
            if (iEntryIndex == -1) return HYRESULT.VMTX_NOEXIST;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            for (UInt16 i = 0; i < Vhea.numOfLongVerMetrics; i++)
            {
                VMTX_LONGHORMETRIC Longhormetric = new VMTX_LONGHORMETRIC();
                FileStrm.Read(array, 0, 2);
                Longhormetric.advanceHeight = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                Longhormetric.tsb= (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                Vmtx.vtLongHormetric.Add(Longhormetric);
            }

            UInt16 uTmp = Vmtx.vtLongHormetric[Vhea.numOfLongVerMetrics- 1].advanceHeight;
            int TopSideBearingNum = Maxp.numGlyphs - Vhea.numOfLongVerMetrics;
            for (int i = 0; i < TopSideBearingNum; i++)
            {
                VMTX_LONGHORMETRIC Longhormetric = new VMTX_LONGHORMETRIC();
                FileStrm.Read(array, 0, 2);

                Longhormetric.tsb = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                Longhormetric.advanceHeight = uTmp;

                Vmtx.vtLongHormetric.Add(Longhormetric);
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeVmtx()

        public HYRESULT DecodePost()
        {
            Post = new CPost();
            byte[] array = new byte[4];

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.POST_TAG);
            if (iEntryIndex == -1) return HYRESULT.POST_NOEXIST;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            // version 
            FileStrm.Read(array, 0, 2);
            Post.version.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Post.version.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            // italicAngle
            FileStrm.Read(array, 0, 2);
            Post.italicAngle.value = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            FileStrm.Read(array, 0, 2);
            Post.italicAngle.fract = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            //underlinePosition
            FileStrm.Read(array, 0, 2);
            Post.underlinePosition= (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            //underlineThickness
            FileStrm.Read(array, 0, 2);
            Post.underlineThickness = (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            //isFixedPitch
            FileStrm.Read(array, 0, 4);
            Post.isFixedPitch = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            //minMemType42
            FileStrm.Read(array, 0, 4);
            Post.minMemType42 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            //maxMemType42
            FileStrm.Read(array, 0, 4);
            Post.maxMemType42 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            //minMemType1
            FileStrm.Read(array, 0, 4);
            Post.minMemType1 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            //maxMemType1
            FileStrm.Read(array, 0, 4);
            Post.maxMemType1 = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            if (Post.version.value == 2 && Post.version.fract == 0)
            {
                FileStrm.Read(array, 0, 2);
                Post.PostFormat2.usNumberOfGlyphs = (ushort)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                int iOwnNameNumber = 0;
                for (int i = 0; i < Post.PostFormat2.usNumberOfGlyphs; i++)
                {
                    FileStrm.Read(array, 0, 2);
                    ushort usNameIdx = (ushort)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                    Post.PostFormat2.lstGlyphNameIndex.Add(usNameIdx);
                    if (usNameIdx > 257) {
                        iOwnNameNumber++;
                    }                
                }

                long lEndPos = tbEntry.offset+tbEntry.length;
				long tmp = FileStrm.Position;
				while (FileStrm.Position<lEndPos)
				{
                    int iStringLen = FileStrm.ReadByte();					
					Post.PostFormat2.lstNameLength.Add((char)iStringLen);
					//string  strTmp;
                    byte[] strTmp = new byte[iStringLen];					
                    FileStrm.Read(strTmp,0,iStringLen);
                    string str = System.Text.Encoding.Default.GetString(strTmp); 
                    Post.lstStandString.Add(str);
				}
            }

            // 在opentypeV1.3标准后已经被弃用，本程序目前暂不支持Format == 2.5的格式。

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodePost()

        public HYRESULT DecodeCOLR()
        {
            Colr = new CHYCOLR();
            byte[] array = new byte[4];

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.COLR_TAG);
            if (iEntryIndex == -1) return HYRESULT.COLR_NOEXIST;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset, SeekOrigin.Begin);

            // version 
            FileStrm.Read(array, 0, 2);
            Colr.version = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));            

            FileStrm.Read(array, 0, 2);
            Colr.numBaseGlyphRecords = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Read(array, 0, 4);
            Colr.baseGlyphRecordsOffset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            FileStrm.Read(array, 0, 4);
            Colr.layerRecordsOffset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            FileStrm.Read(array, 0, 2);
            Colr.numLayerRecords = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Seek(tbEntry.offset + Colr.baseGlyphRecordsOffset,SeekOrigin.Begin);
          
            for (ushort i = 0; i < Colr.numBaseGlyphRecords; i++)
            {
                CBaseGlyphRecord BaseGlyphRecord = new CBaseGlyphRecord();

                FileStrm.Read(array, 0, 2);
                BaseGlyphRecord.GID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                BaseGlyphRecord.firstLayerIndex = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                BaseGlyphRecord.numLayers = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                Colr.lstBaseGlyphRecord.Add(BaseGlyphRecord);
            }
            
            FileStrm.Seek(tbEntry.offset + Colr.layerRecordsOffset, SeekOrigin.Begin);
            for (long i = 0; i < Colr.numLayerRecords; i++)
            {
                CLayerRecord LayerRecord = new CLayerRecord();

                FileStrm.Read(array, 0, 2);
                LayerRecord.GID = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Read(array, 0, 2);
                LayerRecord.paletteIndex = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                Colr.lstLayerRecord.Add(LayerRecord);
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeCOLR()

        protected void DecodeGLYF_SIMPLE(CharInfo charInf)
        {
            byte[] array = new byte[4];
            List <UInt16>EndPotArray = new List<UInt16>();
            for (Int16 i = 0; i < charInf.ContourCount; i++)
            {
                FileStrm.Read(array, 0, 2);
                UInt16 PtNum = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                EndPotArray.Add(PtNum);
            }

            FileStrm.Read(array, 0, 2);
            UInt16 instructionLength = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

            FileStrm.Seek(instructionLength,SeekOrigin.Current);                  

            List<byte>	vtflags = new List<byte>();
            int nPts = EndPotArray[charInf.ContourCount-1] + 1;
         
    	    int j;	
		    // 获取标记	
		    for (int i=0;i<nPts;i++)
		    {				    
                FileStrm.Read(array, 0, 1);
                vtflags.Add(array[0]);

                if ((array[0]&0x0008)>0)
			    {
                    j = FileStrm.ReadByte();                   
				    while (j-->0) 
				    {
					    i++;
                        vtflags.Add(array[0]);
				    }
			    }
		    }
            
            // 获取X坐标		    
            List<short>	xCoordinates = new List<short>();
            List<short> yCoordinates = new List<short>();
            List<byte> PtFlag = new List<byte>();
            List<int> EndPos = new List<int>();            

            short Ptx=0, Pty=0;            
		    for (int i=0; i<nPts;i++)
		    {
			    byte flag = vtflags[i];   

                if ((flag&0x0002)>0)
			    {				
                    j=FileStrm.ReadByte();
                    if ((flag & 0x0010)>0)
				    {
					    // 正值      
                        Ptx+=(short)j;
                        xCoordinates.Add(Ptx);
				    }
				    else 
				    {
					    // 负值
                        Ptx-=(short)j;
                        xCoordinates.Add(Ptx);
				    }
			    }
			    else 
			    {
				    // 无偏移
                    if ((flag&0x0010)>0)
				    {
                        xCoordinates.Add(Ptx);
				    }
				    else
				    {						
                        FileStrm.Read(array, 0, 2);
                        Ptx += (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                        xCoordinates.Add(Ptx);
				    }
			    }                           
		    }
 
             // 获取Y轴坐标	
		    for (int i=0; i<nPts;i++)
		    {
                byte flag = vtflags[i];
                if ((flag&0x0004)>0)
			    {		
                    j=FileStrm.ReadByte();
                    if ((flag&0x0020)>0)
				    {
					    // 正值				
                        Pty += (short)j;
                        yCoordinates.Add(Pty);
				    }
				    else 
				    {
					    // 负值
                        Pty -= (short)j;
                        yCoordinates.Add(Pty);
				    }
			    }
			    else 
			    {
				    // 无偏移
                    if ((flag&0x0020)>0)
				    {
                        yCoordinates.Add(Pty);
				    }
				    else
				    {
					    // SHORT 类型偏移
                        FileStrm.Read(array, 0, 2);
                        Pty += (Int16)hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                        yCoordinates.Add(Pty);	
				    }
			    }
		    }

            ContourInfo cntuinf = new ContourInfo();
            int CntIndex = 0;
            for (int i = 0; i < nPts; i++)
            {
                if ((vtflags[i]&0x0001) > 0)
                {
                    PtFlag.Add(0x0001);
                }
                else
                {
                    PtFlag.Add(0x0000);
                }

                PtInfo hyPt = new PtInfo();
                hyPt.Y = yCoordinates[i];
                hyPt.X = xCoordinates[i];
                hyPt.PtType = PtFlag[i];

                cntuinf.PtInfo.Add(hyPt);

                if (i == EndPotArray[CntIndex])
                {
                    cntuinf.PointCount = cntuinf.PtInfo.Count;
                    charInf.ContourInfo.Add(cntuinf);
                    if (CntIndex < charInf.ContourCount - 1)
                    {
                        cntuinf = new ContourInfo();
                        CntIndex++;
                    }                    
                }
            }

        }   // end of protected void DecodeGLYF_SIMPLE()

        protected void DecodeGLYF_COMPOS(CharInfo charinf)
        {
            UInt16	flag=0;            
            byte[]  array = new byte[4];
		    do {

                CmpInf  cmpst = new CmpInf();
                cmpst.Arg = new List<int>();

                FileStrm.Read(array, 0, 2);
                flag = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                cmpst.Flag = flag;
			    
                FileStrm.Read(array, 0, 2);
                cmpst.Gid  = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));	    		    
			    

			    //flags&GLYF_CMPST_ARGS_ARE_XY_VALUES;		
			    //flags&GLYF_CMPST_ROUND_XY_TO_GRID;
			    //flags&GLYF_CMPST_USE_MY_METRICS;			

			    if ((cmpst.Flag&0x0001)>0)  //GLYF_CMPST_ARG_1_AND_2_ARE_WORDS
			    {   
                    FileStrm.Read(array, 0, 2);
                    cmpst.Arg.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));

                    FileStrm.Read(array, 0, 2);
                    cmpst.Arg.Add(hy_cdr_int16_to(BitConverter.ToUInt16(array, 0)));
			    }
			    else
			    {
				    ushort uArg1and2 = 0; 
                    FileStrm.Read(array, 0, 2);
                    uArg1and2 = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));				    
				
				    cmpst.Arg.Add((char)(uArg1and2 >> 8));
				    cmpst.Arg.Add((char)(uArg1and2 & 0x00FF));
			    }

			    if ((cmpst.Flag&0x0008)>0) //GLYF_CMPST_WE_HAVE_A_SCALE
			    {	
                    FileStrm.Read(array, 0, 2);
                    cmpst.Scale = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

			    }
			    else if ((cmpst.Flag&0x0040)>0)    //GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE
			    {
                    FileStrm.Read(array, 0, 2);
                    cmpst.ScaleX = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
		
                    FileStrm.Read(array, 0, 2);
                    cmpst.ScaleY = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
			    }
			    else if ((cmpst.Flag&0x0080)>0) //GLYF_CMPST_WE_HAVE_A_TWO_BY_TWO
			    {				    
                    FileStrm.Read(array, 0, 2);
                    cmpst.ScaleX = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    FileStrm.Read(array, 0, 2);
                    cmpst.Scale01= hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));

                    FileStrm.Read(array, 0, 2);
                    cmpst.Scale10= hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
				 
                    FileStrm.Read(array, 0, 2);
                    cmpst.ScaleY= hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
			    }

                charinf.CmpInf.Add(cmpst);			    

		    } while ((flag&0x0020)>0); 	//GLYF_CMPST_MORE_COMPONENT

            // 不去读取组件的指令集

        }   // end of protected void DecodeGLYF_COMPOS()

        // 通过Unicode编码获取Gryph Index
	    public int	FindGryphIndexByUnciode(UInt32 ulUnicode)
	    {		
		    int iGlyphIndex = -1;
		    int ulloop = 0;
		    int iEntryIndex = Cmap.FindSpecificEntryIndex(CMAPENCODEFORMAT.CMAP_ENCODE_FT_12);
		    if (iEntryIndex!=-1)
		    {
			    CMAP_TABLE_ENTRY TbEntry = Cmap.vtCamp_tb_entry[iEntryIndex];
			    ulloop = (int)TbEntry.Format12.nGroups;
			    for (int i=0; i<ulloop; i++)
			    {                   
				    CMAP_ENCODE_FORMAT_12_GROUP	tagF12_GROUP = TbEntry.Format12.vtGroup[i];
				    if (ulUnicode>=tagF12_GROUP.startCharCode && ulUnicode<= tagF12_GROUP.endCharCode)
				    {
                        iGlyphIndex = (int)tagF12_GROUP.startGlyphID + (int)(ulUnicode - tagF12_GROUP.startCharCode);
				    }
			    }
		    }
		    else 
		    {                
			    iEntryIndex = Cmap.FindSpecificEntryIndex(CMAPENCODEFORMAT.CMAP_ENCODE_FT_4);
			    if (iEntryIndex!=-1)
			    {
				    CMAP_TABLE_ENTRY        TbEntry =  Cmap.vtCamp_tb_entry[iEntryIndex];
				    CMAP_ENCODE_FORMAT_4    Format4 = TbEntry.Format4;

				    ulloop = Format4.segCountX2>>1;
				    for (int i=0; i<ulloop; i++)
				    {
					    // 确定这个unicode是处于哪个segment中。
					    if (ulUnicode>=Format4.vtstartCount[i] && ulUnicode<=Format4.vtEndCount[i])
					    {
                            int iRang = (int)(ulUnicode - Format4.vtstartCount[i]);
                            
						    if (Format4.vtidRangeOffset[i] != 0)
						    {
							    int iID = (Format4.vtidRangeOffset[i]-Format4.segCountX2)/2+i+iRang;
                                if (iID< Format4.vtglyphIdArray.Count)
                                {
                                    iGlyphIndex = Format4.vtglyphIdArray[iID];
                                }
						    }
                            else
						    {
							    iGlyphIndex = (Format4.vtstartCount[i]+iRang+(Format4.vtidDelta[i]))%65536;
						    }
					    }
				    }
			    }                 
		    }

            return iGlyphIndex;

	    }	// end of int	CHYFontCodec::FindGryphIndexByUnciode()
        
        // 通过Gryph Index 获取 Unicode,一个GryphIndex 可能会对应多个Uncoide 编码	
        public void  FindGryphUncidoByIndex(int ulGlyphIndex, ref List<UInt32> szUnicode)
        {
            List<UInt32> szTmpUnicode = new List<uint>();

            int ulloop = 0;
            int iEntryIndex = Cmap.FindSpecificEntryIndex(CMAPENCODEFORMAT.CMAP_ENCODE_FT_12);
            if (iEntryIndex!=-1&&ulGlyphIndex!=0)
            {
                CMAP_TABLE_ENTRY TbEntry = Cmap.vtCamp_tb_entry[iEntryIndex];
                CMAP_ENCODE_FORMAT_12 Format12 = TbEntry.Format12;
                ulloop = (int)Format12.nGroups;
                for (int i=0; i<ulloop; i++)
                {
                    CMAP_ENCODE_FORMAT_12_GROUP  tagF12Group = Format12.vtGroup[i];
                    if (ulGlyphIndex >= tagF12Group.startGlyphID)
                    {
                        if (tagF12Group.startCharCode+(ulGlyphIndex-tagF12Group.startGlyphID) <= tagF12Group.endCharCode)
                        {
                            szTmpUnicode.Add((UInt32)(tagF12Group.startCharCode+(ulGlyphIndex-tagF12Group.startGlyphID)));
                        }
                    }			
                }
            }
            else 
            {
                iEntryIndex = Cmap.FindSpecificEntryIndex(CMAPENCODEFORMAT.CMAP_ENCODE_FT_4);
                if (iEntryIndex!=-1)
                {
                    CMAP_TABLE_ENTRY        TbEntry = Cmap.vtCamp_tb_entry[iEntryIndex];
                    CMAP_ENCODE_FORMAT_4    Format4 = TbEntry.Format4;
                    ulloop = Format4.segCountX2>>1;

                    for (int i=0; i<ulloop;i++)
                    {
                        int iRang		= Format4.vtEndCount[i]-Format4.vtstartCount[i];
                        if (Format4.vtidRangeOffset[i]%65535 !=0)
                        {   
                            for (int x = 0; x <= iRang; x++)
                            {
                                short iID = (short)((Format4.vtidRangeOffset[i] - Format4.segCountX2) / 2 + i + x);
                                if (iID < Format4.vtglyphIdArray.Count)
                                {
                                    if (ulGlyphIndex == Format4.vtglyphIdArray[iID])
                                    {
                                        szTmpUnicode.Add((UInt32)(Format4.vtstartCount[i] + x));
                                    }
                                }
                            }
                        }
                        else 
                        {	
                            for (int x=0; x<=iRang; x++)
                            {
                                if (ulGlyphIndex == (Format4.vtstartCount[i]+x+(Format4.vtidDelta[i]))%65536)
                                {
                                    szTmpUnicode.Add((UInt32)(Format4.vtstartCount[i]+x));
                                }
                            }
                        }
                    }
                }
            }

            bool bRepeat = false;
            for (int i=0; i<szTmpUnicode.Count; i++)
            {
                bRepeat = false;
                for (int j=0; j<szUnicode.Count; j++)
                {
                    if (szTmpUnicode[i]== szUnicode[j])
                        bRepeat = true;
                }

                if (!bRepeat)
                {
                    szUnicode.Add(szTmpUnicode[i]);			
                }
            }	

        }	// end of int CHYFontCodec::FindGryphUncidoByIndex()   

        public int GetGlyfAdvancWidth(int iGlyphID)
	    {
		    int iLonghormetricSize = Hmtx.lstLonghormetric.Count();
		    if (iLonghormetricSize== 0) return 0;

		    if (iGlyphID<iLonghormetricSize)
		    {
                return Hmtx.lstLonghormetric[iGlyphID].advanceWidth;
		    }
		    else 
		    {
                return Hmtx.lstLonghormetric[iLonghormetricSize - 1].advanceWidth;
		    }

	    }	// end of void GetGlyfAdvancWidth()

        int GetGlyfAdvancHeight(int iGlyphID)
	    {
		    int iLonghormetricSize = Vmtx.vtLongHormetric.Count;
		    if (iLonghormetricSize== 0) return -1;

		    if (iGlyphID<iLonghormetricSize)
		    {
			    return  Vmtx.vtLongHormetric[iGlyphID].advanceHeight;
		    }
		    else 
		    {
			    return  Vmtx.vtLongHormetric[iLonghormetricSize-1].advanceHeight;
		    }		    

	    }	// end of void GetGlyfAdvancHeight()

        public HYRESULT DecodeCFF()
        {
            CFFInfo = new CCFFInfo();            

            int iEntryIndex = TableDirectorty.FindTableEntry((uint)TABLETAG.CFF_TAG);
			if (iEntryIndex == -1) return HYRESULT.CFF_DECODE;

			CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            FileStrm.Seek(tbEntry.offset,SeekOrigin.Begin);

			// head
            CFFInfo.Header.major = (byte)FileStrm.ReadByte();
            CFFInfo.Header.minor = (byte)FileStrm.ReadByte();
            CFFInfo.Header.hdrSize = (byte)FileStrm.ReadByte();
            CFFInfo.Header.offSize = (byte)FileStrm.ReadByte();			
			
			// name string 
			DecodeNameString();
			// TopDict
			DecodeTopDict(ref CFFInfo.TopDICT);
			// string index
			DecodeStringIndex();
			// global sub Index
			DecodeGlobalSubIndex();
            long lCharsetOffset = (long)(tbEntry.offset + CFFInfo.TopDICT.charsetOffset);
            DecodeCharSets(lCharsetOffset);

			if (CFFInfo.TopDICT.FDArryIndexOffset>0)
			{			
				long lFDArrayOffset = tbEntry.offset+CFFInfo.TopDICT.FDArryIndexOffset;
				DecodeFDArray(lFDArrayOffset,tbEntry.offset);
				long lFDSelectOffset = (long)(tbEntry.offset+CFFInfo.TopDICT.FDSelectOffset);
                DecodeFDSelect(lFDSelectOffset);
			}
			else 
			{
				DecodePrivteDict(ref CFFInfo.PrivteDict,CFFInfo.TopDICT.PrivteOffset+tbEntry.offset,CFFInfo.TopDICT.PrivteDictSize);
			}

			long lCharStringOffset = tbEntry.offset+CFFInfo.TopDICT.charStringOffset;
			DecodeCharString(lCharStringOffset);                
            
            for (int i = 0; i < Chars.CharInfo.Count; i++)
            {
                CountBoundBox(i,ref Chars, FontType);
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT DecodeCFF()

        protected void DecodeNameString()
        { 
            CCFFIndex nmInxd = new CCFFIndex();
			DecodeCFFIndex(ref nmInxd);
		
			int  iSize=0;
			for(ushort i=0; i<nmInxd.Count;i++)
			{
				string str;
				iSize = (int)(nmInxd.vtOffset[i+1]-nmInxd.vtOffset[i]);
                Byte[] Content = new Byte[iSize];
                FileStrm.Read(Content, 0, iSize);
                str = BitConverter.ToString(Content);
                CFFInfo.vtFontName.Add(str);
			}
        
        }   // end of protected void DecodeNameString()

        protected void DecodeTopDict(ref CCFFTopDict TopDict)
        { 
            CCFFIndex TopDictIndx = new CCFFIndex();
			DecodeCFFIndex(ref TopDictIndx);

			long					iBPos = FileStrm.Position;
			long					iCPos=0;
			long					loperator	= 0 ;	
			long		            ulTopDictSize=TopDictIndx.vtOffset[1]-TopDictIndx.vtOffset[0];

			List<double>	vtOperand = new List<double>();
			byte		    byFirst;
            byte            bySecond;
			while(iCPos<ulTopDictSize)
			{
                byFirst = (byte)FileStrm.ReadByte();				 
				loperator			= 0;			 

				 if (byFirst>=0 && byFirst<=21)
				 {			
					 if (byFirst==12)
					 {
                        bySecond = (byte)FileStrm.ReadByte();
						loperator = byFirst<<8|bySecond;					
					 }
					 else 				 
						 loperator = byFirst;		 

					 switch(loperator)
					 {
                         case 0x00://CFF_DICT_OPERATOR_VERSION:					 
							TopDict.iVersionSID=(int)vtOperand[0];					 
							break;
                         case 0x01://CFF_DICT_OPERATOR_NOTICE:					 
							TopDict.iNoticeSID=(int)vtOperand[0];
							break;
                         case 0x0c00://CFF_DICT_OPERATOR_COPYRIGHT:					 
							TopDict.iCopyrghtSID=(int)vtOperand[0];					 
							break;
                         case 0x02://CFF_DICT_OPERATOR_FULLNAME:
							TopDict.iFullNameSID=(int)vtOperand[0];					 
							break;
                         case 0x03://CFF_DICT_OPERATOR_FAMILYNAME:
							TopDict.iFamilyNameSID=(int)vtOperand[0];				 
							break;
                         case 0x04://CFF_DICT_OPERATOR_WEIGHT:					 
							TopDict.iWeightSID=(int)vtOperand[0];					 
							break;
                         case 0x0c01://CFF_DICT_OPERATOR_ISFIXEDPITCH:					 
							if (vtOperand.Count>0)						
								TopDict.isFixedPitch = (long)vtOperand[0];
							break;
                         case 0x0c02://CFF_DICT_OPERATOR_ITALICANGLE:					 
							if (vtOperand.Count>0)						
								TopDict.ItalicAngle = vtOperand[0];						
							break;
                         case 0x0c03://CFF_DICT_OPERATOR_UNDERLINEPOSITION:					 
							if (vtOperand.Count>0)						 
								TopDict.UnderlinePosition = vtOperand[0];						 					 
							break;
                         case 0x0c04://CFF_DICT_OPERATOR_UNDERLINETHICKNESS:					 
							if (vtOperand.Count>0)						
								TopDict.UnderlineThickness = vtOperand[0];						
							break;
                         case 0x0c05://CFF_DICT_OPERATOR_PAINTTYPE:					 
							if (vtOperand.Count>0)						
								TopDict.PaintType= (long)vtOperand[0];											 
							break;
                         case 0x0c06://CFF_DICT_OPERATOR_CHARSTRINGTYPE:					 
							if (vtOperand.Count>0)
								TopDict.PaintType= (long)vtOperand[0];
							break;
                         case 0x0c07://CFF_DICT_OPERATOR_FONTMATRIX:					
							TopDict.vtFontMatrix=vtOperand;												
							break;
                         case 0x0d://CFF_DICT_OPERATOR_UNIQUEID:					
							TopDict.UniqueID = (long)vtOperand[0];				 
							break;
                         case 0x05://CFF_DICT_OPERATOR_FONTBBOX:						
							for (int s=0; s<vtOperand.Count; s++)
							{
								TopDict.vtFontBOX.Add((int)vtOperand[s]);
							}
							break;
                         case 0x0c08://CFF_DICT_OPERATOR_STROKEWIDTH:					 
							if (vtOperand.Count>0)						
								TopDict.strokeWidth = vtOperand[0];											 
							break;
                         case 0x0e://CFF_DICT_OPERATOR_XUID:						 
							for (int s=0; s<vtOperand.Count; s++)
							{
								TopDict.vtXUID.Add((int)vtOperand[s]);
							}
							break;
                         case 0x0f://CFF_DICT_OPERATOR_CHARSET:					 
							if (vtOperand.Count>0)						
								TopDict.charsetOffset = (long)vtOperand[0];
							break;
                         case 0x10://CFF_DICT_OPERATOR_ENCODING:					 
							if (vtOperand.Count>0)						
								TopDict.encodingOffset = (long)vtOperand[0];						
							break;
                         case 0x11://CFF_DICT_OPERATOR_CHARSTRINGS:						
								TopDict.charStringOffset= (long)vtOperand[0];						
							break;
                         case 0x12://CFF_DICT_OPERATOR_PRIVATE:					 						 						 
							TopDict.PrivteDictSize = (long)vtOperand[0];
							TopDict.PrivteOffset = (long)vtOperand[1];
							break;
                         case 0x0c14://CFF_DICT_OPERATOR_SYNTHETICBASE:
							TopDict.SyntheticBaseIndex = (long)vtOperand[0];				 
							break;
                         case 0x0c15://CFF_DICT_OPERATOR_POSTSCRIPT:					
							TopDict.PostSriptSID = (int)vtOperand[0];
							break;
                         case 0x0c16://CFF_DICT_OPERATOR_BASEFONTNAME:					
							TopDict.BaseFontNameSID = (int)vtOperand[0];
							break;
                         case 0x0c17://CFF_DICT_OPERATOR_BASEFONTBLEND:
							for(int s=0; s<vtOperand.Count; s++)
							{
								TopDict.vtBaseFontBlend.Add(vtOperand[s]);
							}
							break;
                         case 0x0c1e://CFF_DICT_OPERATOR_ROS:	
							//Registry
							TopDict.Ros.RegistrySID=(int)vtOperand[0];
							//Ordering
							TopDict.Ros.OrderingSID=(int)vtOperand[1];
							//Supplement
							TopDict.Ros.Supplement=(int)vtOperand[2];
							break;
                         case 0x0c1f://CFF_DICT_OPERATOR_CIDFONTVERSION:				 
							if (vtOperand.Count>0)						 
								TopDict.CIDFontVersion = vtOperand[0];
							break;
                         case 0x0c20://CFF_DICT_OPERATOR_CIDFONTREVISION:					 
							if (vtOperand.Count>0)
								TopDict.CIDFontRevision= vtOperand[0];					 
							break;
                         case 0x0c21://CFF_DICT_OPERATOR_CIDFONTTYPE:				 
							if (vtOperand.Count>0)
								TopDict.CIDFontType = vtOperand[0];
							break;
                         case 0x0c22://CFF_DICT_OPERATOR_CIDCOUNT:					 
							if (vtOperand.Count>0)						
								TopDict.CIDCount = (long)vtOperand[0];
							break;
                         case 0x0c23://CFF_DICT_OPERATOR_UIDBASE:					
							TopDict.UIDBase = (long)vtOperand[0];
							break;
                         case 0x0c24://CFF_DICT_OPERATOR_FDARRAY:					 
							TopDict.FDArryIndexOffset =  (long)vtOperand[0];
							break;
                         case 0x0c25://CFF_DICT_OPERATOR_FDSELECT:				
							TopDict.FDSelectOffset = (long)vtOperand[0];
							break;
                         case 0x0c26://CFF_DICT_OPERATOR_FONTNAME:					 
							TopDict.CIDFontNameSID=(int)vtOperand[0];
							break;
						default:
							break;							
					 }
                     vtOperand.Clear();
				}
				else 
				{	
					double f;
					if (byFirst == 0x1e)
					{				 
						f=DecodeDICTReal();
						vtOperand.Add(f);
					}
					else 
					{						
                        FileStrm.Seek(-1,SeekOrigin.Current);
						f=DecodeDICTInteger();
						vtOperand.Add((double)f);
					}
				}				
                iCPos = FileStrm.Position - iBPos;
			}
        
        }   // end of protected void DecodeTopDict()

        protected void DecodeStringIndex()
        {         
            CCFFIndex StringIndx = new CCFFIndex();
			DecodeCFFIndex(ref StringIndx);

			long ulDataBegin = FileStrm.Position;           
			
			long lSize=0;
			for(ushort i=0; i<StringIndx.Count;i++)
			{
				string str;
				lSize = StringIndx.vtOffset[i+1]-StringIndx.vtOffset[i];
                Byte[] Content = new Byte[lSize];
                FileStrm.Read(Content, 0, (int)lSize);
                str = System.Text.Encoding.UTF8.GetString(Content);               

                CFFInfo.stnStrings.szStandString.Add(str);			
			}

        }   // end of protected void DecodeStringIndex()

        protected void DecodeGlobalSubIndex()
        { 
            DecodeCFFIndex(ref CFFInfo.globalSubIndex);	

		    long ulSize = 0;
		    for(int i=0; i<CFFInfo.globalSubIndex.Count;i++)
		    {			
			    ulSize += CFFInfo.globalSubIndex.vtOffset[i+1]-CFFInfo.globalSubIndex.vtOffset[i];		
		    }		
		     byte ch;
		    for (long i=0; i<ulSize; i++)
		    {		    
                ch = (byte)FileStrm.ReadByte();
			    CFFInfo.globalSubIndex.vtData.Add(ch);
		    }
        
        }   // end of protected void DecodeGlobalSubIndex()

        protected void DecodeFDArray(long ulOffset, long lCFFBegPos)
        { 
            FileStrm.Seek(ulOffset,SeekOrigin.Begin);           

			CCFFIndex		PdictIndex = new CCFFIndex();
			DecodeCFFIndex(ref PdictIndex);

			for(ushort i=0; i<PdictIndex.Count;i++)
			{				
				long	iDataSize = PdictIndex.vtOffset[i+1]-PdictIndex.vtOffset[i];

				CCFFPrivteDict				PrvtDict = new CCFFPrivteDict();
				List<double>				vtOperand = new List<double>();
				long						loperator	= 0;			
				long					    iBPos = FileStrm.Position;
				long				        iCPos = 0;
				byte				        byFirst, bySecond;

				while (iCPos<iDataSize)
				{	
                    byFirst = (byte)FileStrm.ReadByte();
					// operator 
					if (byFirst>=0 && byFirst<=21)
					{			
						if (byFirst==12)
						{	
                            bySecond = (byte)FileStrm.ReadByte();
							loperator = byFirst<<8|bySecond;					
						}
						else 			
							loperator = byFirst;				
						
						if (loperator == 0x0c26)
						{
							PrvtDict.iFontNameID = (int)vtOperand[0];
						}
						else 
						{
							long unCurnPin = FileStrm.Position;
                            if (vtOperand.Count>1)
                            {
                                DecodePrivteDict(ref PrvtDict, (long)(vtOperand[1]) + lCFFBegPos, (long)vtOperand[0]);
                                FileStrm.Seek(unCurnPin, SeekOrigin.Begin);
                                CFFInfo.vtFDArry.Add(PrvtDict);
                            }							
						}
						vtOperand.Clear();
					}
					else  
					{
						double f;
						if ( byFirst == 0x1e )
						{						
							f = DecodeDICTReal();
							vtOperand.Add(f);
						}
						else 
						{							
                            FileStrm.Seek(-1, SeekOrigin.Current);
							f = (double)DecodeDICTInteger();
							vtOperand.Add(f);
						}
					}					
                    iCPos = FileStrm.Position - iBPos;
				}
			}
        
        }   // end of protected void DecodeFDArray()

        protected void DecodePrivteDict(ref CCFFPrivteDict PrvtDict, long lOffset, long ulDataSize)
        {            
            FileStrm.Seek(lOffset,SeekOrigin.Begin);
			List<double>				    vtOperand = new List<double>();
			long							loperator	= 0;			
			long					        iBPos = FileStrm.Position;
			long					        iCPos = 0;
			byte					        byFirst, bySecond;
			
			while (iCPos<ulDataSize)
			{	
                byFirst = (byte)FileStrm.ReadByte();

				// operator 
				if (byFirst>=0 && byFirst<=21)
				{			
					if (byFirst == 12)
					{						
                        bySecond = (byte)FileStrm.ReadByte();
						loperator = byFirst<<8|bySecond;					
					}
					else 				
						loperator = byFirst;				
					
					switch(loperator)
					{
						case 0x06://CFF_DICT_OPERATOR_BLUEVALUES:						
							PrvtDict.vtBlueValues.AddRange(vtOperand);
							break;
						case 0x07://CFF_DICT_OPERATOR_OTHERBLUES:
							PrvtDict.vtOtherBlues.AddRange(vtOperand);
							break;
						case 0x08://CFF_DICT_OPERATOR_FAMILYBLUES:
							PrvtDict.vtFamilyBlues.AddRange(vtOperand);
							break;
						case 0x09://CFF_DICT_OPERATOR_FAMILYOTHERBLUES:
							PrvtDict.vtFamilyOtherBlues.AddRange(vtOperand);
							break;
						case 0x0c09://CFF_DICT_OPERATOR_BLUESCALE:						
							if (vtOperand.Count>0)							
								PrvtDict.fBlueScale=vtOperand[0];						
							break;
						case 0x0c0a://CFF_DICT_OPERATOR_BLUESHIFT:						
							if (vtOperand.Count>0)							
								PrvtDict.fBlueShift=vtOperand[0];
							break;
						case 0x0c0b://CFF_DICT_OPERATOR_BLUEFUZZ:
							if (vtOperand.Count>0)							
								PrvtDict.fBlueFuzz=vtOperand[0];
							break;
						case 0x0a://CFF_DICT_OPERATOR_STDHW:						
							PrvtDict.fStdHW=vtOperand[0];
							break;
						case 0x0b://CFF_DICT_OPERATOR_STDVW:							
							PrvtDict.fStdVW=vtOperand[0];
							break;
						case 0x0c0c://CFF_DICT_OPERATOR_STEMSNAPH:
							PrvtDict.vtStemSnapH=vtOperand;
							break;
						case 0x0c0d://CFF_DICT_OPERATOR_STEMSNAPV:
							PrvtDict.vtStemSnapV=vtOperand;
							break;
						case 0x0c0e://CFF_DICT_OPERATOR_FORCEBOLD:
							if (vtOperand.Count>0)						
								PrvtDict.lForceBold=(long)vtOperand[0];						
							break;
						case 0x0c11://CFF_DICT_OPERATOR_LANGUAGEGROUP:
							if (vtOperand.Count>0)						
								PrvtDict.lLanguageGroup=(long)vtOperand[0];						
							break;
						case 0x0c12://CFF_DICT_OPERATOR_EXPANSIONFACTOR:						
							if (vtOperand.Count>0)						
								PrvtDict.fExpansionFactor=vtOperand[0];												
							break;
						case 0x0c13://CFF_DICT_OPERATOR_INITIALRANDOMSEED:						
							if (vtOperand.Count>0)						
								PrvtDict.finitialRandomSeed=vtOperand[0];												
							break;
						case 0x13://CFF_DICT_OPERATOR_SUBRS:
							{
								long lSubOffset = lOffset+(long)vtOperand[0];
								long lCurPos = FileStrm.Position;
                                FileStrm.Seek(lSubOffset,SeekOrigin.Begin);							
								
								DecodeCFFIndex(ref PrvtDict.SubIndex);		
								long lSize = 0;
								for(short i=0; i<PrvtDict.SubIndex.Count;i++)
								{			
									lSize += PrvtDict.SubIndex.vtOffset[i+1]-PrvtDict.SubIndex.vtOffset[i];		
								}
								
								byte ch;
								for (long i=0; i<lSize; i++)
								{									
                                    ch = (byte)FileStrm.ReadByte();
									PrvtDict.SubIndex.vtData.Add(ch);
								}							
                                FileStrm.Seek(lCurPos,SeekOrigin.Begin);
							}	
							break;
						case 0x14://CFF_DICT_OPERATOR_DEFAULTWIDTHX:						
							if (vtOperand.Count>0)
								PrvtDict.ldefaultWidthX=(long)vtOperand[0];
							break;
						case 0x15://CFF_DICT_OPERATOR_NOMINALWIDTHX:						
							if (vtOperand.Count>0)						
								PrvtDict.lnominalWidthX=(long)vtOperand[0];												
							break;
						default:
							break;
					}				
					vtOperand.Clear();
				}
				else 
				{
					double f;
					if (byFirst==0x1e)
					{
						f=DecodeDICTReal();					
						vtOperand.Add(f);
					}
					else 
					{						
                        FileStrm.Seek(-1, SeekOrigin.Current);
						f=(double)DecodeDICTInteger();
						vtOperand.Add(f);
					}			
				}				
                iCPos = FileStrm.Position - iBPos;
			}				

        }   // end of protected void DecodePrivteDict()

        protected void DecodeFDSelect(long lOffset)
        {
            FileStrm.Seek(lOffset, SeekOrigin.Begin);

            byte uch=(byte)FileStrm.ReadByte();			
			int	                nTmp = 0;
			//byte			    btmp = 0;
			CFFInfo.FDSelect.iFormat = uch;
			if (uch == 0)
			{
				for (long i=0; i<CFFInfo.TopDICT.CIDCount; i++)
				{
					uch=(byte)FileStrm.ReadByte();
					CFFInfo.FDSelect.format0.Add(uch);
				}
			}		

			if (uch==3)
			{
				nTmp = 0;
				uch=(byte)FileStrm.ReadByte();			
				nTmp |= uch<<8;
				uch=(byte)FileStrm.ReadByte();
				nTmp |= uch;				

				for(int i=0; i<nTmp; i++)
				{
					CCFFFDSRang3	FDRang3 = new CCFFFDSRang3();
                    uch=(byte)FileStrm.ReadByte();
					FDRang3.first |= (short)(uch<<8);
					uch=(byte)FileStrm.ReadByte();
					FDRang3.first |= uch;

                    FDRang3.fdIndex=(byte)FileStrm.ReadByte();					
					CFFInfo.FDSelect.format3.vtRang3.Add(FDRang3);
				}
				nTmp = 0;
				uch=(byte)FileStrm.ReadByte();	
				nTmp |= uch<<8;
				uch=(byte)FileStrm.ReadByte();	
				nTmp |= uch;
				CFFInfo.FDSelect.format3.sentinel= (ushort)nTmp;
			}	

        }   // end of protected void DecodeFDSelect()

        protected void DecodeCharSets(long lOffset)
        { 
            FileStrm.Seek(lOffset, SeekOrigin.Begin);

			byte uch;
            uch = (byte)FileStrm.ReadByte();

			int			i = 0;
			ushort	    nTmp = 0;
			byte		btmp = 0;
            byte[]      array = new byte[2];

			CFFInfo.FDSelect.iFormat = uch;
			if (uch == 0)
			{
                int st = Maxp.numGlyphs - 1;
				for (i=0; i<st; i++)
				{
                    FileStrm.Read(array, 0, 2);
					nTmp = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
					CFFInfo.Charset.format0.vtSID.Add(nTmp);
				}				
			}

			if (uch == 1)
			{
                int stGlyphNum = Maxp.numGlyphs - 1;
				int CharSetNum = 0;				
				while (CharSetNum<stGlyphNum)
				{
					CCFFCSFormatRang1 csr1=new CCFFCSFormatRang1();
                    FileStrm.Read(array, 0, 2);
                    nTmp = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));								
					
                    btmp = (byte)FileStrm.ReadByte();
					csr1.first = nTmp;
					csr1.left = btmp;
					CFFInfo.Charset.format1.vtRang.Add(csr1);
					CharSetNum+=btmp+1;
				}
			}

			if (uch==2)
			{
                int stGlyphNum = Maxp.numGlyphs - 1;
				int CharSetNum = 0;				
				while (CharSetNum<stGlyphNum)
				{
				    CCFFCSFormatRang2 csr2 = new CCFFCSFormatRang2();					
                    FileStrm.Read(array, 0, 2);
					nTmp = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
					csr2.first = nTmp;
                    FileStrm.Read(array, 0, 2);
                    nTmp = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));					
					csr2.left = nTmp;
					CFFInfo.Charset.format2.vtRang.Add(csr2);
					CharSetNum+=nTmp+1;
				}	
			}	
        
        }   // end of protected void DecodeCharSets()

        protected int GetFDIndex(ref CCFFFDSelect FDSelect, int iGID)
        {
            int FDIndex = -1;
            if (FDSelect.iFormat == 0)
            {
                if (FDSelect.format0.Count >= iGID)
                {
                    FDIndex = FDSelect.format0[iGID];
                }
            }

            if (FDSelect.iFormat == 3)
            {
                int iRange3Size = FDSelect.format3.vtRang3.Count;

                for (int i = 0; i < iRange3Size; i++)
                {
                    if (i == iRange3Size - 1)
                    {
                        if (iGID >= FDSelect.format3.vtRang3[i].first && iGID <= FDSelect.format3.sentinel)
                        {
                            FDIndex = FDSelect.format3.vtRang3[i].fdIndex;
                        }
                    }
                    else if ((iGID >= FDSelect.format3.vtRang3[i].first) && (iGID < FDSelect.format3.vtRang3[i + 1].first))
                    {
                        FDIndex = FDSelect.format3.vtRang3[i].fdIndex;
                        break;
                    }
                }
            }

            return FDIndex;

        }   // end of protected int GetFDIndex()

        protected void DecodeCharString(long lOffset)
        {   
            FileStrm.Seek(lOffset, SeekOrigin.Begin);
			
			CCFFIndex	CharStringIndx = new CCFFIndex();
			DecodeCFFIndex(ref CharStringIndx);
			
            for (int i = 0; i < CharStringIndx.Count; i++)
            {
                CharInfo sChar = new CharInfo();

                double AdbWidht = -32768.0;
                int iDept = 0;

                CCharStringParam CharParam = new CCharStringParam();
                int uCharDataSize = (int)(CharStringIndx.vtOffset[i + 1] - CharStringIndx.vtOffset[i]);
                if (uCharDataSize < 0x80000)
                {
                    byte[] charData = new byte[0x80000];
                    FileStrm.Read(charData, 0, uCharDataSize);

                    ContourInfo cnurInfo = new ContourInfo();
                    DecodeCharData(ref charData, uCharDataSize, ref sChar, i, ref AdbWidht, ref CharParam, iDept, ref cnurInfo);

                    sChar.ContourCount = sChar.ContourInfo.Count;

                    if (Post.version.value != 3)
                    {
                        sChar.Name = CFFInfo.FindStringbyGlyphID((ushort)i);
                    }	

                    List<uint> lstUnicode = new List<uint>();
                    FindGryphUncidoByIndex(i, ref lstUnicode);

                    sChar.Unicode = "";
                    for (int itmp = 0; itmp < lstUnicode.Count; itmp++)
                    {
                        sChar.Unicode += Convert.ToString(lstUnicode[itmp], 10);
                        if (itmp != lstUnicode.Count - 1)
                            sChar.Unicode += "|";
                    }
                    
                    Chars.CharInfo.Add(sChar);
                }                
            }
        
        }   // end of protected void DecodeCharString()

        protected void DecodeCharData(ref byte[] charData, int iDataSize, ref CharInfo GlyphItem, int iGID, ref double AdvWidth,ref CCharStringParam	CharParam,int iDepth,ref ContourInfo cnurInfo)
        { 
            int ibufOffset = 0;		  
		    if (iDepth > 10) return;
		    if (iDepth == 0) CharParam.bufStart = ibufOffset;

		    byte		B0 = 0, B1 = 0, B2 = 0, B3 = 0, B4 = 0;		
		    //CHYPoint	hypoint;
		    //int			iStemNumber = 0;

		    while (ibufOffset < iDataSize)
		    {
                PtInfo pt;

			    //unsigned char const*	opStart = pData;
                int iOpStart = ibufOffset;
			    //B0=*pData++;
                B0=charData[ibufOffset++];

			    // b0 range 32 - 246
			    if (B0>31 && B0<247)
			    {
				    //CharParam.arguments.push(static_cast <double> (static_cast <SHORT> (static_cast <USHORT> (B0))-139));
                    CharParam.arguments.Add((double)(B0-139));
				    continue;
			    }

			    // b0 rang 247 - 250
			    if (B0>246 && B0<251)
			    {				
				    //B1=*pData++;
                    B1=charData[ibufOffset++];
				    //CharParam.arguments.push(static_cast <double> ((B0-247)* 256+B1+108));
                    CharParam.arguments.Add((double)((B0-247)* 256+B1+108));
				    continue;
			    }

			    // b0 rang 251 - 254
			    if (B0>250 && B0<255)
			    {						
				    //B1=*pData++;
                    B1=charData[ibufOffset++];
				    //CharParam.arguments.push(static_cast <double> (-(B0-251)* 256-B1-108));
                    CharParam.arguments.Add((double)(-(B0-251)* 256-B1-108));
				    continue;
			    }

			    // b0 rang 28
			    //if (B0 == TYPE2_CHARSTING_OPERATOR_SHORTINT )
                if (B0 == 0x1c )
			    {
				    B1=charData[ibufOffset++];
				    B2=charData[ibufOffset++];

				    //CharParam.arguments.push(static_cast <double> (sstatic_cast <short>(B1<<8|B2)));
                    CharParam.arguments.Add((double )(B1<<8|B2));
				    continue;
			    }

			    // b0 rang 255
			    if (B0 == 255 )
			    {
				    //B1=*pData++;
				    //B2=*pData++;
				    //B3=*pData++;
				    //B4=*pData++;
                    B1=charData[ibufOffset++];
				    B2=charData[ibufOffset++];
                    B3=charData[ibufOffset++];
                    B4=charData[ibufOffset++];

				    //CharParam.arguments.push(static_cast <double> (static_cast <long>(B1<<24|B2<<16|B3<<8|B4))/65536.0);
                    CharParam.arguments.Add((B1<<24|B2<<16|B3<<8|B4)/65536.0);
				    continue;
			    }

			    switch(B0)
			    {
			        //case TYPE2_CHARSTING_OPERATOR_HSTEM:
                    case    0x01:
				    {
					    int		acount = 2*(CharParam.arguments.Count/2);

					    if (!CharParam.hstem && iDepth == 0)	// We won't modify a subroutine
					    {
						    CharParam.hstemPos = iOpStart-CharParam.bufStart;
						    CharParam.hstemArgs = acount;
					    }

					    CharParam.TakeWidth(ref AdvWidth,acount);
					    CharParam.hintCount += (acount/2);
					    CharParam.arguments.Clear();
					    CharParam.hstem = true;
				    }
				    break;
			        //case TYPE2_CHARSTING_OPERATOR_VSTEM:
                    case  0x03: 
				    {
					    // We don't care about hints
					    int		acount = 2*(CharParam.arguments.Count/2);					
					    CharParam.TakeWidth(ref AdvWidth,acount);
					    CharParam.hintCount += acount/2;
					    CharParam.arguments.Clear();
					    CharParam.vstem = true;
				    }
				    break;			
			        //case TYPE2_CHARSTING_OPERATOR_HSTEMHM:
                    case   0x12:
				    {
					    int		acount = 2*(CharParam.arguments.Count/2);
					    if (!CharParam.hstem && iDepth==0)	// We won't modify a subroutine
					    {
						    CharParam.hstemPos = iOpStart-CharParam.bufStart;
						    CharParam.hstemArgs = acount;
					    }					
					    CharParam.TakeWidth(ref AdvWidth,acount);
					    CharParam.hintCount += (acount/2);
					    CharParam.arguments.Clear();
					    CharParam.hstem = true;
				    }
				    break;

			        //case TYPE2_CHARSTING_OPERATOR_VSTEMHM:
                    case    0x17:
				    {
					    int		acount = 2*(CharParam.arguments.Count/2);					
					    CharParam.TakeWidth(ref AdvWidth,acount);
					    CharParam.hintCount += (acount/2);
					    CharParam.arguments.Clear();
					    CharParam.vstem = true;
				    }
				    break;			
			        //case TYPE2_CHARSTING_OPERATOR_HINTMASK:
			        //case TYPE2_CHARSTING_OPERATOR_CNTRMASK:
                    case 0x13:
                    case 0x14:
				    {
					    if (CharParam.hstem && !CharParam.vstem && (CharParam.lastOp==1||CharParam.lastOp==18))
					    {
						    int		acount = 2*(CharParam.arguments.Count/2);
						    CharParam.hintCount += (acount/2);
						    CharParam.vstem = true;
						    CharParam.arguments.RemoveRange(CharParam.arguments.Count-acount,acount);						
						    CharParam.TakeWidth(ref AdvWidth,acount);
					    }
					    else
					    {
						    int		acount = 2*(CharParam.arguments.Count/2);
						    CharParam.hintCount += (acount/2);						
						    CharParam.TakeWidth(ref AdvWidth,acount);
					    }

					    int		count = HintBytes(CharParam.hintCount);					
					    if (ibufOffset+count>iDataSize) return;
					    ibufOffset += count;
					
					    CharParam.arguments.Clear();
				    }
				    break;
			        //case TYPE2_CHARSTING_OPERATOR_RMOVETO:
                    case 0x15:
				    {
                        if (cnurInfo.PtInfo.Count > 0)
                        {
                            cnurInfo.PointCount = cnurInfo.PtInfo.Count;
                            GlyphItem.ContourInfo.Add(cnurInfo);
                            cnurInfo = new ContourInfo();
                        }

					    SetMovePath (ref CharParam.movePath, iDepth, ref CharParam.injectPos, ref iOpStart);					
					    int		ix = CharParam.TakeWidth(ref AdvWidth,2);

					    CharParam.position.X += (float)CharParam.arguments[ix++];
					    CharParam.position.Y += (float)CharParam.arguments[ix++];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 1;//POINT_FLG_ANCHOR
                        cnurInfo.PtInfo.Add(pt);
                       
                        CharParam.arguments.Clear();
				    }
				    break;		
			        //case TYPE2_CHARSTING_OPERATOR_HMOVETO:
                    case 0x16:
				    {
                        if (cnurInfo.PtInfo.Count > 0)
                        {
                            cnurInfo.PointCount = cnurInfo.PtInfo.Count;
                            GlyphItem.ContourInfo.Add(cnurInfo);
                            cnurInfo = new ContourInfo();
                        }

					    SetMovePath (ref CharParam.movePath, iDepth,ref  CharParam.injectPos, ref iOpStart);					
					    int		ix = CharParam.TakeWidth(ref AdvWidth,1);

					    CharParam.position.X += (float)CharParam.arguments[ix];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 1;//POINT_FLG_ANCHOR
                        cnurInfo.PtInfo.Add(pt);

                        CharParam.arguments.Clear();			    
				    }
				    break;
                    case 0x04://TYPE2_CHARSTING_OPERATOR_VMOVETO:
				    {
                        if (cnurInfo.PtInfo.Count > 0)
                        {
                            cnurInfo.PointCount = cnurInfo.PtInfo.Count;
                            GlyphItem.ContourInfo.Add(cnurInfo);
                            cnurInfo = new ContourInfo();
                        }

					    SetMovePath (ref CharParam.movePath, iDepth, ref CharParam.injectPos, ref iOpStart);
					    int		ix = CharParam.TakeWidth(ref AdvWidth,1);

					    CharParam.position.Y += (float)CharParam.arguments[ix];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 1;//POINT_FLG_ANCHOR
                        cnurInfo.PtInfo.Add(pt);

                        CharParam.arguments.Clear();
				    }
				    break;
                    case 0x05://TYPE2_CHARSTING_OPERATOR_RLINETO:
				    {
					    int		pairCount = CharParam.arguments.Count/2;
					    for (int ix = 0; ix != pairCount; ++ix)
					    {
						    double		dx = CharParam.arguments[2*ix];
						    double		dy = CharParam.arguments[2*ix+1];

						    CharParam.position.X += (float)dx;
						    CharParam.position.Y += (float)dy;					    

                             pt = new PtInfo();
                             pt.X = HY_RealRount(CharParam.position.X);
                             pt.Y = HY_RealRount(CharParam.position.Y);
                             pt.PtType = 1;//POINT_FLG_ANCHOR
                             cnurInfo.PtInfo.Add(pt);
					    }					    
                        CharParam.arguments.Clear();
				    }
				    break;
                    case 0x06://TYPE2_CHARSTING_OPERATOR_HLINETO:
				    {
					    int		count = CharParam.arguments.Count;
					    int		ix = 0;
					    bool	h = true;
					    while (ix!=count)
					    {
						    double		d = CharParam.arguments[ix];
						    if (h)
						    {						
							    CharParam.position.X += (float)d;
							    h = false;
						    }
						    else
						    {						
							    CharParam.position.Y += (float)d;
							    h = true;
						    }

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 1;//POINT_FLG_ANCHOR
                            cnurInfo.PtInfo.Add(pt);

						     ++ix;
					    }					    
                        CharParam.arguments.Clear();
				    }
				    break;		
                    case 0x07://TYPE2_CHARSTING_OPERATOR_VLINETO:
				    {
					    int		count = CharParam.arguments.Count;
					    int		ix = 0;
					    bool	v = true;

					    while (ix!=count)
					    {
						    double d = CharParam.arguments[ix];
						    if (v)
						    {						
							    CharParam.position.Y +=(float)d;
							    v = false;
						    }
						    else
						    {
							    CharParam.position.X +=(float)d;						
							    v = true;
						    }
						    ++ix;
						   

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 1;//POINT_FLG_ANCHOR
                            cnurInfo.PtInfo.Add(pt);

					    }
					    //CharParam.arguments.Clear();
                        CharParam.arguments.Clear();
				    }
				    break;
                    case 0x08://TYPE2_CHARSTING_OPERATOR_RRCURVETO:
				    {
					    int		iStackIndex		= 0; 
					    int		iStackDeep		= CharParam.arguments.Count;

					    while( iStackIndex < iStackDeep )
					    {
                            CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
                            CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

						    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
						    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

						    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
						    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 1;//POINT_FLG_ANCHOR
                            cnurInfo.PtInfo.Add(pt);
					    }
					    //CharParam.arguments.Clear();
                        CharParam.arguments.Clear();
				    }
				    break;
                    case 0x1b://TYPE2_CHARSTING_OPERATOR_HHCURVETO:
				    {
					    int iStackIndex = 0;
					    int iStackDeep = CharParam.arguments.Count;

					    if ((iStackDeep%4)>1) 
					    {
						    CharParam.arguments.Clear();
						    break;
					    }

					    double		dya = 0.0;
					    if( (iStackDeep%4)!= 0 )
					    {
						    dya += CharParam.arguments[iStackIndex++];
					    }

					    while (iStackIndex<iStackDeep)
					    {
						    double		dxa = CharParam.arguments[iStackIndex++];
						    double		dxb = CharParam.arguments[iStackIndex++];
						    double		dyb = CharParam.arguments[iStackIndex++];
						    double		dxc = CharParam.arguments[iStackIndex++];
						    PointF		da = new PointF((float)dxa, (float)dya);
						    PointF		db = new PointF((float)dxb, (float)dyb);
						    PointF[]	curve = new PointF[4];

						    curve [0] = CharParam.position;
						    curve [1].X = curve[0].X + da.X;
                            curve [1].Y = curve[0].Y + da.Y;
						    curve [2].X = curve[1].X + db.X;
                            curve [2].Y = curve[1].Y + db.Y;
						    curve [3].X = curve[2].X+(float)dxc;
						    curve [3].Y = curve[2].Y;

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[1].X);
                            pt.Y = HY_RealRount(curve[1].Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[2].X);
                            pt.Y = HY_RealRount(curve[2].Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[3].X);
                            pt.Y = HY_RealRount(curve[3].Y);
                            pt.PtType = 1;
                            cnurInfo.PtInfo.Add(pt);

						    CharParam.position = curve[3];
						    dya = 0.0;
					    }
					    CharParam.arguments.Clear();
				    }
				    break;
                    case 0x1f://TYPE2_CHARSTING_OPERATOR_HVCURVETO:
				    {
					    bool bHorizontal = true;
					    int iStackIndex = 0; 
					    int iStackDeep = CharParam.arguments.Count;

                       
					    while(iStackIndex<iStackDeep)
					    {
						    if (bHorizontal)
						    {
							    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
                               
                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);

                                CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
                                CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);

							    if (iStackDeep%4!=0)
							    {
								    if (iStackIndex == (iStackDeep-2))
								    {
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);
								    }
								    else
								    {
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];									   

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);

								    }
							    }
							    else 
							    {
								    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];							    

                                    pt = new PtInfo();
                                    pt.X = HY_RealRount(CharParam.position.X);
                                    pt.Y = HY_RealRount(CharParam.position.Y);
                                    pt.PtType = 1;
                                    cnurInfo.PtInfo.Add(pt);
							    }
						    }
						    else 
						    {
							    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];                               

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);
					

							    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
							    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);	

							    if (iStackDeep%4 != 0)
							    {
								    if (iStackIndex == (iStackDeep-2))
								    {
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);									   	
								    }
								    else
								    {
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);								    
								    }
							    }
							    else 
							    {
								    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
                                    pt = new PtInfo();
                                    pt.X = HY_RealRount(CharParam.position.X);
                                    pt.Y = HY_RealRount(CharParam.position.Y);
                                    pt.PtType = 1;
                                    cnurInfo.PtInfo.Add(pt);						
							    }
						    }
						    bHorizontal = !bHorizontal;
					    }
					    CharParam.arguments.Clear();
				    }
				    break;
                    case 0x18://TYPE2_CHARSTING_OPERATOR_RCURVELINE:
				    {
					    int iStackIndex = 0; 
					    int iStackDeep = CharParam.arguments.Count;

					    while(iStackIndex < iStackDeep-2)
					    {
						    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
						    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];                           		    			

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);	

						    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
						    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

						    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
						    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 1;
                            cnurInfo.PtInfo.Add(pt);				   
					    }

					    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
					    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 1;
                        cnurInfo.PtInfo.Add(pt);

					    CharParam.arguments.Clear();
				    }
				    break;
                    case 0x19://TYPE2_CHARSTING_OPERATOR_RLINECURVE:
				    {
					    int iStackIndex = 0; 
					    int iStackDeep = CharParam.arguments.Count;

					    while(iStackIndex < iStackDeep-6)
					    {
                            CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
                            CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                            pt = new PtInfo();
                            pt.X = HY_RealRount(CharParam.position.X);
                            pt.Y = HY_RealRount(CharParam.position.Y);
                            pt.PtType = 1;
                            cnurInfo.PtInfo.Add(pt);	

					    }

					    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
					    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 0;
                        cnurInfo.PtInfo.Add(pt);	
					 
					    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
					    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 0;
                        cnurInfo.PtInfo.Add(pt);				    

					    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
					    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                        pt = new PtInfo();
                        pt.X = HY_RealRount(CharParam.position.X);
                        pt.Y = HY_RealRount(CharParam.position.Y);
                        pt.PtType = 1;
                        cnurInfo.PtInfo.Add(pt);				    

					    CharParam.arguments.Clear();
				    }
				    break;
                    case 0x1e://TYPE2_CHARSTING_OPERATOR_VHCURVETO:
				    {
					    bool bVertical = true;
					    int iStackIndex = 0; 
					    int iStackDeep = CharParam.arguments.Count;

					    while(iStackIndex < iStackDeep)
					    {
						    if (bVertical)
						    {
							    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];                               

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);

							    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
							    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);					    

							    if (iStackDeep%4 != 0)
							    {
								    if ( iStackIndex == (iStackDeep - 2) )
								    {
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);									    
								    }
								    else 
								    {
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];

                                        pt = new PtInfo();
                                        pt.X = HY_RealRount(CharParam.position.X);
                                        pt.Y = HY_RealRount(CharParam.position.Y);
                                        pt.PtType = 1;
                                        cnurInfo.PtInfo.Add(pt);								    
								    }
							    }
							    else 
							    {
								    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];

                                    pt = new PtInfo();
                                    pt.X = HY_RealRount(CharParam.position.X);
                                    pt.Y = HY_RealRount(CharParam.position.Y);
                                    pt.PtType = 1;
                                    cnurInfo.PtInfo.Add(pt);							    
							    }
						    }
						    else 
						    {
							    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);						   

							    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
							    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                pt = new PtInfo();
                                pt.X = HY_RealRount(CharParam.position.X);
                                pt.Y = HY_RealRount(CharParam.position.Y);
                                pt.PtType = 0;
                                cnurInfo.PtInfo.Add(pt);							    

							    if (iStackDeep%4 != 0)
							    {
								    if (iStackIndex == (iStackDeep-2))
								    {
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
									    CharParam.position.X += (float)CharParam.arguments[iStackIndex++];
								    }
								    else
								    {
									    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];
								    }

                                    pt = new PtInfo();
                                    pt.X = HY_RealRount(CharParam.position.X);
                                    pt.Y = HY_RealRount(CharParam.position.Y);
                                    pt.PtType = 1;
                                    cnurInfo.PtInfo.Add(pt);	
							    }
							    else 
							    {
								    CharParam.position.Y += (float)CharParam.arguments[iStackIndex++];

                                    pt = new PtInfo();
                                    pt.X = HY_RealRount(CharParam.position.X);
                                    pt.Y = HY_RealRount(CharParam.position.Y);
                                    pt.PtType = 1;
                                    cnurInfo.PtInfo.Add(pt);
							    }
						    }
						    bVertical = !bVertical;
					    }
					    CharParam.arguments.Clear();
				    }
				    break;
                    case 0x1a://TYPE2_CHARSTING_OPERATOR_VVCURVETO:
				    {
					   // int iStackIndex = 0; 
					    int iStackDeep = CharParam.arguments.Count;

					    if ((CharParam.arguments.Count%4)>1) break;

					    float		dxa = 0.0f;
					    int		ix = 0;
					    if ((iStackDeep%4) != 0)
					    {
						    dxa = (float)CharParam.arguments[ix++];
					    }

					    while (ix<CharParam.arguments.Count)
					    {
						    float		dya = (float)CharParam.arguments[ix++];
						    float		dxb = (float)CharParam.arguments[ix++];
						    float		dyb = (float)CharParam.arguments[ix++];
						    float		dyc = (float)CharParam.arguments[ix++];
						    PointF		da = new PointF(dxa,dya);
						    PointF		db = new PointF(dxb, dyb);
						    PointF[]	curve = new PointF[4];

						    curve [0] = CharParam.position;
						    curve [1].X = curve [0].X+da.X;
                            curve [1].Y = curve [0].Y+da.Y;

						    curve [2].X = curve [1].X+db.X;
                            curve [2].Y = curve [1].Y+db.Y;

						    curve [3].X = curve [2].X;
						    curve [3].Y = curve [2].Y+dyc;

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[1].X);
                            pt.Y = HY_RealRount(curve[1].Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[2].X);
                            pt.Y = HY_RealRount(curve[2].Y);
                            pt.PtType = 0;
                            cnurInfo.PtInfo.Add(pt);

                            pt = new PtInfo();
                            pt.X = HY_RealRount(curve[3].X);
                            pt.Y = HY_RealRount(curve[3].Y);
                            pt.PtType = 1;
                            cnurInfo.PtInfo.Add(pt);			   	   	

						    CharParam.position = curve[3];
						    dxa = 0.0f;
					    }
					    CharParam.arguments.Clear();
				    }
				    break;			
                    case 0x0e://TYPE2_CHARSTING_OPERATOR_ENDCHAR:
				    {
					    int		ix = CharParam.TakeWidth(ref AdvWidth,CharParam.arguments.Count>1?4:0);

					    CharParam.arguments.Clear();
					    CharParam.lastOp = B0;					

					    //if (HYContour.vtHYPoints.size()>0)
					    //{
						//    HYGryph.vtContour.push_back(HYContour);
						//    HYContour.SetDefault();
					    //}

                        if (cnurInfo.PtInfo.Count > 0)
                        {
                            cnurInfo.PointCount = cnurInfo.PtInfo.Count;
                            GlyphItem.ContourInfo.Add(cnurInfo);
                            cnurInfo = new ContourInfo();
                        }

					    if (CharParam.haveWidth)
					    {
                            if (((ulong)CFFInfo.TopDICT.Ros.RegistrySID | (ulong)CFFInfo.TopDICT.Ros.OrderingSID | (ulong)CFFInfo.TopDICT.Ros.Supplement) > 0)
						    {
							    int iFDIndex = GetFDIndex(ref CFFInfo.FDSelect, iGID);
							    if ( iFDIndex > -1 )
							    {
								    if (AdvWidth == -32768.0 )
								    {
									    AdvWidth = CFFInfo.vtFDArry[iFDIndex].ldefaultWidthX;
								    }
								    else 
								    {
									    AdvWidth = CFFInfo.vtFDArry[iFDIndex].lnominalWidthX+AdvWidth;
								    }
							    }
						    }
						    else
						    {
							    if (AdvWidth == -32768.0 )
							    {
								    AdvWidth = CFFInfo.PrivteDict.ldefaultWidthX;
							    }
							    else 
							    {
								    AdvWidth = CFFInfo.PrivteDict.lnominalWidthX+AdvWidth;
							    }
						    }
					    }
					    else 
					    {
						    AdvWidth = CFFInfo.PrivteDict.ldefaultWidthX;
					    }

                        //hmtxLongMrc.advanceWidth = (ushort)Math.Round(AdvWidth,MidpointRounding.AwayFromZero);
                        //hmtxLongMrc.lsb = (short)Math.Round(AdvWidth,MidpointRounding.AwayFromZero);
					    //HYGryph.advanceWidth = HY_RealRount(AdvWidth);
				    }
				    break;
                    case 0x0a://TYPE2_CHARSTING_OPERATOR_CALLSUBR:
				    {
					    int FDIndex = GetFDIndex(ref CFFInfo.FDSelect, iGID);
					    //unsigned char data[10240]	= {0};
					    //int ipos					= 0;
					    int iSubSize		= 0;
					    int iStart		    = 0;
					    int iEnd			= 0;

					    if (FDIndex != -1) 
					    {
						    CCFFIndex CFFIndex = CFFInfo.vtFDArry[FDIndex].SubIndex;
						    ushort					ix = (ushort)CharParam.arguments[CharParam.arguments.Count-1];
                            CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);                            

						    ushort				count = CFFIndex.Count;

						    if (count < 1240)			ix += 107;
						    else if (count < 33900)		ix += 1131;
						    else						ix += 32768;

						    if (ix < CFFIndex.Count)
						    {
							    iSubSize =(int)(CFFIndex.vtOffset[ix+1]-CFFIndex.vtOffset[ix]);
							    iStart = (int)(CFFIndex.vtOffset[ix]-1);
							    iEnd = (int)(CFFIndex.vtOffset[ix+1]-1);

                                byte[] data = new byte[iSubSize];
					            int ipos					= 0;
							
							    for (int i= iStart; i!=iEnd;i++)
							    {
								    data[ipos++] = CFFIndex.vtData[i];
							    }

                                DecodeCharData(ref data, iSubSize, ref GlyphItem, iGID, ref AdvWidth, ref CharParam, iDepth, ref cnurInfo);
						    }
					    }		
					    else 
					    {
						    CCFFIndex CFFIndex	= CFFInfo.PrivteDict.SubIndex;
						    ushort				count = CFFIndex.Count;
						    ushort				ix = (ushort)CharParam.arguments[CharParam.arguments.Count-1];
                            CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

						    if (count < 1240)			ix += 107;
						    else if (count < 33900)		ix += 1131;
						    else						ix += 32768;

						    if (ix < CFFIndex.Count)
						    {
							    iSubSize = (int)(CFFIndex.vtOffset[ix+1]-CFFIndex.vtOffset[ix]);
							    iStart = (int)(CFFIndex.vtOffset[ix]-1);
							    iEnd = (int)(CFFIndex.vtOffset[ix+1]-1);

                                byte[] data = new byte[iSubSize];
					            int ipos					= 0;

							    for (int  i= iStart; i!=iEnd;i++)
							    {
								    data[ipos++] = CFFIndex.vtData[i];
							    }
                                DecodeCharData(ref data, iSubSize, ref GlyphItem, iGID, ref AdvWidth, ref CharParam, iDepth, ref cnurInfo);
						    }					
					    }				
				    }
				    break;
                    case 0x1d://TYPE2_CHARSTING_OPERATOR_CALLGSUBR:
				    {
					    CCFFIndex CFFIndex = CFFInfo.globalSubIndex;
					    ushort					ix = (ushort)CharParam.arguments[CharParam.arguments.Count-1];                        
                        CharParam.arguments.RemoveAt(CharParam.arguments.Count - 1);
                        ushort				    count = CFFIndex.Count;

					    if (count < 1240)			ix += 107;
					    else if (count < 33900)		ix += 1131;
					    else						ix += 32768;

					    if (ix < count)
					    {						
						    int iSubSize =(int)(CFFIndex.vtOffset[ix+1]-CFFIndex.vtOffset[ix]);						    
						    int ulStart = (int)(CFFIndex.vtOffset[ix]-1);
						    int ulEnd = (int)(CFFIndex.vtOffset[ix+1]-1);

                            byte[] data = new byte[iSubSize];
					        int ipos					= 0;

						    for (int i=ulStart; i!=ulEnd;i++)
						    {
							    data[ipos++] = CFFIndex.vtData[i];
						    }
                            DecodeCharData(ref data, iSubSize, ref GlyphItem, iGID, ref AdvWidth, ref CharParam, iDepth, ref cnurInfo);
					    }					
				    }
				    break;
                    case 0x0b://TYPE2_CHARSTING_OPERATOR_RETURN:
				        CharParam.lastOp = B0;
				    return;
                    case 0x0c://TYPE2_CHARSTING_OPERATOR_ESCAPE:
				    {	                  			   
                        B0=charData[ibufOffset++];					    
					    ushort US = (ushort)(0x0C00|B0);
					    switch (US)
					    {					
                            case 0x0c00://TYPE2_CHARSTING_OPERATOR_RESERVED0:		// dotsection (deprecated)
                            case 0x0c01://TYPE2_CHARSTING_OPERATOR_RESERVED1:
                            case 0x0c02://TYPE2_CHARSTING_OPERATOR_RESERVED2:
						        CharParam.arguments.Clear();
						    break;
                            case 0x0c03://TYPE2_CHARSTING_OPERATOR_AND:		// and
                                {
                                    short				iT1 = (short) (CharParam.arguments[CharParam.arguments.Count-1]);
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

                                    short				iT2 = (short) (CharParam.arguments[CharParam.arguments.Count-1]);
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

                                    CharParam.arguments.Add ((iT1&iT2)!=0?1.0:0.0); 
                                }						    
						    break;
                            case 0x0c04://TYPE2_CHARSTING_OPERATOR_OR:		// or
                                {
                                    short				iT1 = (short) (CharParam.arguments[CharParam.arguments.Count-1]);
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
                                    short				iT2 = (short) (CharParam.arguments[CharParam.arguments.Count-1]);
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

                                    CharParam.arguments.Add ((iT1&iT2)!=0?1.0:0.0);    
                                }						    
						    break;
                            case 0x0c05://TYPE2_CHARSTING_OPERATOR_NOT:		// not
                                {
                                    short				iT1 = (short) (CharParam.arguments[CharParam.arguments.Count-1]);
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
                                    CharParam.arguments.Add(iT1!=0? 0.0: 1.0);                                
                                }
						     
						    break;
                            case 0x0c09://TYPE2_CHARSTING_OPERATOR_ABS:		// abs
						    {
							    double				iT1 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    CharParam.arguments.Add (iT1<0.0? -iT1:iT1);
						    }

						    break;
                            case 0x0c0a://TYPE2_CHARSTING_OPERATOR_ADD:		// add
                                {
                                    double				iT1 = CharParam.arguments[CharParam.arguments.Count-1];
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
                                    double				iT2 = CharParam.arguments[CharParam.arguments.Count-1];
                                    CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

						            CharParam.arguments.Add (iT1 + iT2);
                                }
						    break;
                            case 0x0c0b://TYPE2_CHARSTING_OPERATOR_SUB:		// sub
						    {
							    double		y = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

							    CharParam.arguments.Add(x - y);
						    }
						    break;
					        case 0x0c0c://TYPE2_CHARSTING_OPERATOR_DIV:		// div
						    {
							    double		y = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

							    CharParam.arguments.Add(x/y);
						    }

						    break;
					        case 0x0c0e://TYPE2_CHARSTING_OPERATOR_NEG:		// neg
                            {
                                double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
						        CharParam.arguments.Add(-x);
                            }

						    break;
                            case 0x0c0f://TYPE2_CHARSTING_OPERATOR_EG:		// eq
                            {
                                double		y = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
						        CharParam.arguments.Add (y == x? 1.0: 0.0);
                            }

						    break;
                            case 0x0c12://TYPE2_CHARSTING_OPERATOR_DROP:		// drop
                                {
                                    if (CharParam.arguments.Count>0)
                                    {
                                        CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
                                    }						            
                                }
						    break;
                            case 0x0c14://TYPE2_CHARSTING_OPERATOR_PUT:		// put
						    {
                                double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);						    

							    if ((int)x >= 32)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    CharParam.transient [(int)x] = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
						    }

						    break;
                            case 0x0c15://TYPE2_CHARSTING_OPERATOR_GET:		// get
						    {
							    double		x = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);	

							    if ((int)x >= 32)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    CharParam.arguments.Add(CharParam.transient [(int)x]);
						    }

						    break;
                            case 0x0c16://TYPE2_CHARSTING_OPERATOR_IFELSE:		// ifelse
						    {
							    double		v2 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);	
							    double		v1 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    double		s2 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    double		s1 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

							    CharParam.arguments.Add(v1 <= v2? s1: s2);
						    }

						    break;
                            case 0x0c17://TYPE2_CHARSTING_OPERATOR_RANDOM:		// random
						    {
                                Random rand  =new Random();
							    double dbRand = (double)rand.Next();
							    CharParam.arguments.Add(dbRand);
						    }
						    break;
                            case 0x0c18://TYPE2_CHARSTING_OPERATOR_MUL:		// mul
						    {
                                double		v2 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);	
							    double		v1 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

							    CharParam.arguments.Add(v2 * v1);
						    }
						    break;
                            case 0x0c1a://TYPE2_CHARSTING_OPERATOR_SQRT:		// sqrt
                            {
                                double		v2 = CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
						        CharParam.arguments.Add (Math.Sqrt(v2));
                            }
						    break;
                            case 0x0c1b://TYPE2_CHARSTING_OPERATOR_DUP:		// dup
                            {
                                double		v2 = CharParam.arguments[CharParam.arguments.Count-1];
						        CharParam.arguments.Add(v2);
						        break;
                            }
                            case 0x0c1c://TYPE2_CHARSTING_OPERATOR_EXCH:		// exch
                            {
                                double dbTmp =  CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments[CharParam.arguments.Count-1]= CharParam.arguments[CharParam.arguments.Count-2];
                                CharParam.arguments[CharParam.arguments.Count-2] = dbTmp;					        
                            }
						    break;
                            case 0x0c1d://TYPE2_CHARSTING_OPERATOR_INDEX:	// index
                            {
                                int ix = (int)CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

			                    if (ix < 0)
				                ix = 0;

                                CharParam.arguments.Add(CharParam.arguments[CharParam.arguments.Count-1-ix]);			                    
                            }
						    break;
                            case 0x0c1e://TYPE2_CHARSTING_OPERATOR_ROLL:		// roll
                            {
                                int j = (int)CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);

                                int n = (int)CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
                               
                                int		stackend = CharParam.arguments.Count-1;
                                for (int ix = 0; ix != n; ++ix)
                                {
                                    double dbTmp =  CharParam.arguments[stackend-ix];
                                    CharParam.arguments[stackend-ix]= CharParam.arguments[stackend - modulo (ix + j, n)];
                                    CharParam.arguments[stackend - modulo (ix + j, n)] = dbTmp;	                                   
                                }
                            }
						    break;
                            case 0x0c22://TYPE2_CHARSTING_OPERATOR_HFLEX:	// hflex
						    {
							    if (CharParam.arguments.Count < 7)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    float		y = CharParam.position.Y;
							    float		jointy = y + (float)CharParam.arguments [2];

							    PointF[]	curve = new PointF[4];

							    curve [0].X = CharParam.position.X;
							    curve [0].Y = CharParam.position.Y;

							    curve [1].X = curve [0].X + (float)CharParam.arguments[0];
                                curve [1].Y = y;

							    curve [2].X = curve [1].X + (float)CharParam.arguments [1];
                                curve [2].Y = jointy;
							    curve [3] = new PointF (curve [2].X + (float)CharParam.arguments [3], jointy);
                                

							    CharParam.position.X = curve [3].X;
							    CharParam.position.Y = curve [3].Y;

							    curve [0].X = CharParam.position.X;
							    curve [0].Y = CharParam.position.Y;
							    curve [1] = new PointF (curve [0].X + (float)CharParam.arguments [4], jointy);
							    curve [2] = new PointF (curve [1].X + (float)CharParam.arguments [5], y);
							    curve [3] = new PointF (curve [2].X + (float)CharParam.arguments [6], y);

							    CharParam.position.X = curve [3].X;
							    CharParam.position.Y = curve [3].Y;

							    CharParam.arguments.Clear();
							    break;
						    }						    
                            case 0x0c23://TYPE2_CHARSTING_OPERATOR_FLEX:		// flex
						    {
							    if (CharParam.arguments.Count < 13)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    for (int ix = 0; ix != 1; ++ix)
							    {
								    PointF		da = new PointF((float)CharParam.arguments [6 * ix + 0], (float)CharParam.arguments [6 * ix + 1]);
								    PointF		db = new PointF((float)CharParam.arguments [6 * ix + 2], (float)CharParam.arguments [6 * ix + 3]);
								    PointF		dc = new PointF((float)CharParam.arguments [6 * ix + 4], (float)CharParam.arguments [6 * ix + 5]);
								    PointF[]	curve = new PointF[4];

								    curve[0].X = CharParam.position.X;
								    curve[0].Y = CharParam.position.Y;
								    curve[1].X = curve [0].X + da.X;
                                    curve[1].Y = curve [0].Y + da.Y;
								    curve[2].X = curve [1].X + db.X;
                                    curve[2].Y = curve [1].Y + db.Y;
								    curve[3].X = curve [2].X + dc.X;
                                    curve[3].Y = curve [2].Y + dc.Y;

								    CharParam.position.X = curve [3].X;
								    CharParam.position.Y = curve [3].Y;
							    }

							    CharParam.arguments.Clear();
							    break;
						    }						    
                            case 0x0c24://TYPE2_CHARSTING_OPERATOR_HFLEX1:		// hflex1
						    {
							    if (CharParam.arguments.Count < 9)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    float		y = CharParam.position.Y;
							    float		jointy = y + (float)CharParam.arguments[3];

							    PointF[]		curve  = new PointF[4];
							    curve [0].X = CharParam.position.X;
							    curve [0].Y = CharParam.position.Y;

							    curve [1] = new PointF (curve [0].X + (float)CharParam.arguments [0], curve [0].Y + (float)CharParam.arguments [1]);
							    curve [2] = new PointF (curve [1].X + (float)CharParam.arguments [2], curve [1].Y + (float)CharParam.arguments [3]);
							    curve [3] = new PointF (curve [2].X + (float)CharParam.arguments [4], jointy);

							    curve [0].X = CharParam.position.X;
							    curve [0].Y = CharParam.position.Y;
							    curve [1] = new PointF (curve [0].X + (float)CharParam.arguments [5], jointy);
							    curve [2] = new PointF (curve [1].X + (float)CharParam.arguments [6], curve [1].Y + (float)CharParam.arguments [7]);
							    curve [3] = new PointF (curve [2].X + (float)CharParam.arguments [8], y);


							    CharParam.position.X = curve [3].X;
							    CharParam.position.Y = curve [3].Y;

						    }
						    CharParam.arguments.Clear();
						    break;
                            case 0x0c25://TYPE2_CHARSTING_OPERATOR_FLEX1:		// flex1
						    {

							    if (CharParam.arguments.Count != 11)
							    {
								    CharParam.arguments.Clear();
								    break;
							    }

							    float		d6 = (float)CharParam.arguments[CharParam.arguments.Count-1];
                                CharParam.arguments.RemoveAt(CharParam.arguments.Count-1);
							    float		x = CharParam.position.X;
							    float		y = CharParam.position.Y;

							    PointF		da = new PointF((float)CharParam.arguments[0], (float)CharParam.arguments[1]);
							    PointF		db = new PointF((float)CharParam.arguments[2], (float)CharParam.arguments[3]);
							    PointF		dc = new PointF((float)CharParam.arguments[4], (float)CharParam.arguments[5]);
							    PointF[]	curve = new PointF[4];

							    curve[0].X = CharParam.position.X;
							    curve[0].Y = CharParam.position.Y;
							    curve[1].X = curve [0].X + da.X;
                                curve[1].Y = curve[0].Y + da.Y;
							    curve[2].X = curve [1].X + db.X;
                                curve[2].Y = curve[1].Y + db.Y;
							    curve[3].X = curve[2].X + dc.X;
                                curve[3].Y = curve[2].Y + dc.Y;

							    CharParam.position.X = curve [3].X;
							    CharParam.position.Y = curve [3].Y;
							    float		dx = da.X + db.X + dc.X;
							    float		dy = da.Y + db.Y + dc.Y;

                                da = new PointF((float)CharParam.arguments[6], (float)CharParam.arguments[7]);
                                db = new PointF((float)CharParam.arguments[8], (float)CharParam.arguments[9]);

							    dx += da.X + db.X;
							    dy += da.Y + db.Y;

							    curve[0].X = CharParam.position.X;
							    curve[0].Y = CharParam.position.Y;
							    curve[1].X = curve[0].X + da.X;
                                curve[1].Y = curve[0].Y + da.Y;
							    curve[2].X = curve[1].X + db.X;
                                curve[2].Y = curve[1].Y + db.Y;

                                if (Math.Abs(dx) > Math.Abs(dy))
							    {
								    curve [3].X = curve [2].X + d6;
								    curve [3].Y = y;
							    }
							    else
							    {
								    curve [3].X = x;
								    curve [3].Y = curve [2].Y + d6;
							    }

							    CharParam.arguments.Clear();
							    break;
						    }
					    default:
						    break;
					    }
					    CharParam.lastOp = US;
					    break;
				    }			
			    default:
				    break;
			    }
			    CharParam.lastOp = B0;	
		    }
        
        }   // end of protected void DecodeCharData()

        protected int modulo(int x, int mod)
        {
            while (x < 0)
                x += mod;

            while (x >= mod)
                x -= mod;
            return x;

        }	// end of int modulo()

        protected int HintBytes(int hintCount)
	    {
		    return (hintCount+7)/8;

	    }	// end of int HintBytes()

        protected void SetMovePath(ref bool movePath, int depth, ref int injectPos, ref int opstart)
        {
            if (!movePath)
            {
                movePath = true;
                if (depth == 0)
                    injectPos = opstart;
            }
        
        }   // end of protected void SetMovePath()

        protected int HintBytes(UInt16 hintCount)
        {

            return 0;

        }   // end of protected int HintBytes()

        protected string DecodeCFFString(int SID, ref CCFFIndex strIndex)
        {
            string			str="";
		    CCFFStandString	Standstr = new CCFFStandString();

		    ulong iOffset		= 0;
		    ulong iDataLen		= 0;	

		    int iStandStrMaxInx = Standstr.szStandString.Count-1;
		    if (SID>iStandStrMaxInx)
		    {
			    iOffset = strIndex.vtOffset[SID-iStandStrMaxInx]-1;
			    iDataLen = strIndex.vtOffset[SID-iStandStrMaxInx+1] - strIndex.vtOffset[SID-iStandStrMaxInx];		
			
                List<byte> strTmp = new List<byte>();
                for (ulong i = 0; i < iDataLen; i++)
                {
                    strTmp.Add(strIndex.vtData[(int)(iOffset+i)]);
                }

                str = strTmp.ToString();
		    }
		    else 
		    {
                str = Standstr.szStandString[SID];	
		    }

		    return str;            

        }   // end of protected string DecodeCFFString()

        protected void DecodeCFFIndex(ref CCFFIndex CffIndex)
        {            
			ulong	i=0;
			//byte	uchar=0;
			uint	loffset = 0;
			//ulong	DataSize = 0;
            byte[]  array = new byte[2];

            FileStrm.Read(array,0,2);
            CffIndex.Count = hy_cdr_int16_to(BitConverter.ToUInt16(array,0));
			if(CffIndex.Count>0)
			{				
                CffIndex.Offsize = (byte)FileStrm.ReadByte();
				for (i=0; i<CffIndex.Count; i++)
				{				
					loffset = DecodeCFFInteger(CffIndex.Offsize);					
					CffIndex.vtOffset.Add(loffset);				
				}

				loffset = DecodeCFFInteger(CffIndex.Offsize);					
				CffIndex.vtOffset.Add(loffset);		
			}       
        
        }   // end of protected void DecodeCFFIndex()

        protected uint DecodeCFFInteger(byte DataSize)
       {
            uint	Integer = 0xffffffff;
			byte	B0=0, B1=0, B2=0, B3=0;

			for (int i=0; i<DataSize; i++)
			{
				switch(i)
				{
					case 0:						
			            B0 = (byte)FileStrm.ReadByte(); 
						break;
					case 1:
						B1 = (byte)FileStrm.ReadByte(); 		
						break;
					case 2:
						B2 = (byte)FileStrm.ReadByte(); 		
						break;
					case 3:
						B3 = (byte)FileStrm.ReadByte(); 		
						break;
					default:
						break;
				}
			}

			switch(DataSize)
			{
				case 1:
					Integer = B0;
					break;
				case 2:
					Integer = (uint)(B0<<8|B1);
					break;
				case 3:
					Integer = (uint)(B0<<16|B1<<8|B2);
					break;
				case 4:
					Integer = (uint)(B0<<24|B1<<16|B2<<8|B3);
					break;
				default:
					break;
			}

		    return Integer;

        }   // end of protected long DecodeCFFInteger()

        protected long DecodeDICTInteger()
        {
            long	Integer = 0xffffffff;

            int	B0=0, B1=0, B2=0, B3=0, B4=0;	
	        B0 = FileStrm.ReadByte();

			// b0 range 32 - 246
			if (B0>31 && B0<247)
			{			
				return Integer = B0-139;
			}

			// b0 rang 247 - 250
			if (B0>246 && B0<251)
			{					
	            B1 = FileStrm.ReadByte();
				return Integer = (B0-247)* 256+B1+108;
			}

			// b0 rang 251 - 254
			if (B0>250 && B0<255)
			{
				B1 = FileStrm.ReadByte();	
				return Integer = -(B0 - 251)* 256-B1-108;
			}

			// b0 rang 28
			if (B0 == 28 )
			{
                B1 = FileStrm.ReadByte();	
                B2 = FileStrm.ReadByte();						
				return Integer = B1<<8|B2;
			}

			// b0 rang 29
			if (B0 == 29 )
			{	
				B1 = FileStrm.ReadByte();	
                B2 = FileStrm.ReadByte();	
	            B3 = FileStrm.ReadByte();	
                B4 = FileStrm.ReadByte();	

				return Integer = B1<<24|B2<<16|B3<<8|B4;
			}

            return Integer;

        }   // end of protected long DecodeDICTInteger()

        protected double DecodeDICTReal()
        {
            //double		result = 0.0f;

            bool				going = true;
			bool				started = false;
			bool				inFraction = false;
			bool				inExponent = false;
			uint			    ix = 0;
			byte[]				bcd = new byte[23];
			
			while (going)
			{
				byte		a;
                a = (byte)FileStrm.ReadByte();				
				for (uint i=0; i<2; ++i)
				{
					int		nybble= i>0?a&0xf:a>>4;
					switch (nybble)
					{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
							bcd[ix++] = (byte)('0'+(byte)nybble);
							started = true;
							break;
						case 0xa:
							if (inFraction)					
								return 0.0f;

							if (inExponent)					
								return 0.0f;					

							inFraction = true;
							bcd [ix++] = (byte)'.';
							started = true;
							break;
						case 0xb:
						case 0xc:
							if (inExponent)										
								return 0.0f;					

							inExponent = true;
							bcd [ix++] = (byte)'e';

							if (nybble == 0xc)
								bcd [ix++] = (byte)'-';

							started = true;
							break;
						case 0xd:					
							return 0.0f;
						case 0xe:
							if (started)					
								return 0.0f;

							bcd [ix++] = (byte)'-';
							started = true;
							break;
						case 0xf:
							going = false;
                            break;
					}

					if (ix>20)							
						return 0.0f;				
				}
			}

			bcd [ix++] = 0;

            string str = System.Text.Encoding.Default.GetString(bcd);
            return double.Parse(str);

        }   // end of protected double DecodeDICTReal()
         


    }
}
