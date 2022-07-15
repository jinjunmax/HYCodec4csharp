using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Web;
using System.Web.UI;
using FontParserEntity;
using System.Drawing;
using System.Windows.Media;
using System.Windows;

namespace HYFontCodecCS
{
    public class HYEncode : HYEncodeBase
    {
        public HYRESULT EncodeFont(string strFontPath, string strProFile, UInt32 TableFlag, ref CharsInfo encdChars)
        {
            GlyphChars = encdChars;
            FileStrm = new FileStream(strFontPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            LoadFontFeature(strProFile);
            if (TableFlag == 0)
            {
                TableFlag = GLYF_FLG | OS2_FLG | CMAP_FLG | HEAD_FLG | HHEA_FLG | HMTX_FLG | LOCA_FLG | MAXP_FLG | NAME_FLG | POST_FLG | PREP_FLG | GASP_FLG | DSIG_FLG | VHEA_FLG | VMTX_FLG;
            }

            if ((TableFlag & GLYF_FLG) > 0)
            {
                FontType = FONTTYPE.TTF;
            }
            if ((TableFlag & CFF_FLG) > 0)
            {
                FontType = FONTTYPE.CFF;
            }

            try
            {
                for (int i = 0; i < encdChars.CharInfo.Count; i++)
                {
                    HYFontBase.CountBoundBox(i, ref encdChars, FontType);
                }
            }
            catch (Exception t)
            {
                return HYRESULT.NOERROR;
            }

            GetFontBox(GlyphChars);
            MakeTableDirectory(TableFlag);
            MakeCodeMap();
            MakeCmap();
            MakePost();
            EncodeTableDirectory();

            if ((TableFlag & DSIG_FLG) > 0)
            {
                EncodeDSIG();
            }

            if ((TableFlag & GASP_FLG) > 0)
            {
                MakeGasp_ClearType();
                EncodeGasp();
            }

            if ((TableFlag & HEAD_FLG) > 0)
            {
                Encodehead();
            }

            if ((TableFlag & HHEA_FLG) > 0)
            {
                Encodehhea();
            }

            if ((TableFlag & MAXP_FLG) > 0)
            {
                Encodemaxp();
            }

            if ((TableFlag & OS2_FLG) > 0)
            {
                EncodeOS2(codemap.lstCodeMap, TableFlag);
            }

            if ((TableFlag & NAME_FLG) > 0)
            {
                Encodename();
            }

            if ((TableFlag & CMAP_FLG) > 0)
            {
                Encodecmap(codemap.lstCodeMap);
            }

            if ((TableFlag & POST_FLG) > 0)
            {
                Encodepost();
            }

            if (FontType == FONTTYPE.CFF)
            {
                EncodeCFF();
            }

            if ((TableFlag & HMTX_FLG) > 0)
            {
                Encodehmtx();
            }

            if ((TableFlag & PREP_FLG) > 0)
            {
                EndcodePrep();
            }

            if (FontType == FONTTYPE.TTF)
            {
                EncodeGlyph();
                Encodeloca();
            }

            /*
            if ((TableFlag & GSUB_FLG) > 0)
            {
                EncodeGSUB();
            }
            */

            if ((TableFlag & VHEA_FLG) > 0)
            {
                Encodevhea();
            }

            if ((TableFlag & VMTX_FLG) > 0)
            {
                Encodevmtx();
            }

            FileStrm.Flush();
            for (int i = 0; i < TableDirectorty.numTables; i++)
            {
                CTableEntry HYEntry = TableDirectorty.vtTableEntry[i];
                uint CheckBufSz = (HYEntry.length + 3) / 4 * 4;
                byte[] pCheckBuf = new byte[CheckBufSz];
                FileStrm.Seek(HYEntry.offset, SeekOrigin.Begin);
                FileStrm.Read(pCheckBuf, 0, (int)CheckBufSz);

                HYEntry.checkSum = CalcFontTableChecksum(pCheckBuf);
            }
            EncodeTableDirectory();
            FileStrm.Flush();
            FileStrm.Close();

            SetCheckSumAdjustment(strFontPath);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeFont()

        public void GetFontBox(CharsInfo charsInf)
        {
            if (charsInf.CharInfo.Count == 0) return;

            int xmin = 0, ymin = 0, xmax = 0, ymax = 0;

            bool b = false;
            for (int i = 0; i < charsInf.CharInfo.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(charsInf.CharInfo[i].Section)) continue;

                if (BoundStringToInt(charsInf.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax))
                {
                    if (!b)
                    {
                        Head.xMin = (short)xmin;
                        Head.xMax = (short)xmax;
                        Head.yMin = (short)ymin;
                        Head.yMax = (short)ymax;
                        b = true;
                    }
                    else
                    {
                        if (xmin < Head.xMin) Head.xMin = (short)xmin;
                        if (xmax > Head.xMax) Head.xMax = (short)xmax;
                        if (ymin < Head.yMin) Head.yMin = (short)ymin;
                        if (ymax > Head.yMax) Head.yMax = (short)ymax;
                    }
                }
            }

        }   // end of void GetFontBox()

        protected HYRESULT MakeTableDirectory(UInt32 ulFlag)
        {
            TableDirectorty = new CTableDirectory();
            if (FontType == FONTTYPE.TTF)
            {
                TableDirectorty.version.value = 1;
                TableDirectorty.version.fract = 0;
            }
            else if (FontType == FONTTYPE.CFF)
            {
                TableDirectorty.version.value = 0x4f54;
                TableDirectorty.version.fract = 0x544f;
            }

            //BASE 
            if ((ulFlag & BASE_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.BASE_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //CFF
            if (FontType == FONTTYPE.CFF)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.CFF_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //DSIG
            if ((ulFlag & DSIG_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.DSIG_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //GDEF
            if ((ulFlag & GDEF_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.GDEF_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //GPOS
            if ((ulFlag & GPOS_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.GPOS_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //GSUB
            if ((ulFlag & GSUB_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.GSUB_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //OS2
            if ((ulFlag & OS2_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.OS2_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //VORG
            if ((ulFlag & VORG_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.VORG_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //CMAP
            if ((ulFlag & CMAP_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.CMAP_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //CVT
            if ((ulFlag & CVT_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.CVT_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //prgm
            if ((ulFlag & FPGM_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.FPGM_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //GASP
            if ((ulFlag & GASP_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.GASP_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //GLYF
            if (FontType == FONTTYPE.TTF)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.GLYF_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //Head
            if ((ulFlag & HEAD_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.HEAD_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //HHEA
            if ((ulFlag & HHEA_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.HHEA_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //HMTX
            if ((ulFlag & HMTX_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.HMTX_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //LOCA
            if (FontType == FONTTYPE.TTF)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.LOCA_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //MAXP
            if ((ulFlag & MAXP_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.MAXP_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //NAME
            if ((ulFlag & NAME_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.NAME_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //POST
            if ((ulFlag & POST_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.POST_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //PREP
            if ((ulFlag & PREP_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.PREP_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //VHEA
            if ((ulFlag & VHEA_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.VHEA_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }
            //VMTX
            if ((ulFlag & VMTX_FLG) > 0)
            {
                CTableEntry HYEntry = new CTableEntry();
                HYEntry.tag = (UInt32)TABLETAG.VMTX_TAG;
                TableDirectorty.vtTableEntry.Add(HYEntry);
            }

            return HYRESULT.NOERROR;

        }   // end of public int MakeTableDirectory()

        public HYRESULT MakeCodeMap()
        {
            codemap = new HYCodeMap();
            for (int i = 0; i < GlyphChars.CharInfo.Count; i++)
            {
                CharInfo chrInf = GlyphChars.CharInfo[i];
                List<uint> lstUnicode = new List<uint>();
                UnicodeStringToList(chrInf.Unicode, ref lstUnicode);

                for (int j = 0; j < lstUnicode.Count; j++)
                {
                    HYCodeMapItem codemapItem = new HYCodeMapItem();
                    codemapItem.GID = i;
                    codemapItem.Unicode = lstUnicode[j];
                    codemap.lstCodeMap.Add(codemapItem);
                }
            }

            codemap.QuickSortbyUnicode();

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT MakeCodeMap()

        public HYRESULT MakeCmap()
        {
            Cmap = new CCmap();
            CMAP_TABLE_ENTRY entry = new CMAP_TABLE_ENTRY();

            entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 0;
            entry.plat_encod_ID = 3;
            entry.format = (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_4;
            Cmap.vtCamp_tb_entry.Add(entry);

            entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 0;
            entry.plat_encod_ID = 4;
            entry.format = (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_12;
            Cmap.vtCamp_tb_entry.Add(entry);

            entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 3;
            entry.plat_encod_ID = 1;
            entry.format = (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_4;
            Cmap.vtCamp_tb_entry.Add(entry);

            entry = new CMAP_TABLE_ENTRY();
            entry.plat_ID = 3;
            entry.plat_encod_ID = 10;
            entry.format = (UInt16)CMAPENCODEFORMAT.CMAP_ENCODE_FT_12;
            Cmap.vtCamp_tb_entry.Add(entry);

            Cmap.version = 0;
            Cmap.numSubTable = (UInt16)Cmap.vtCamp_tb_entry.Count;

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT MakeCmap()
           
        protected HYRESULT LoadFontFeature(string ProFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(ProFile, settings);
            xmlDoc.Load(reader);

            // 得到Name
            XmlNode xNameRoot = xmlDoc.SelectSingleNode("FontProfile/NAME");
            if (xNameRoot != null)
            {
                Name = new CName();
                foreach (XmlNode xn in xNameRoot.ChildNodes)
                {
                    NAMERECORD nameRecode = new NAMERECORD();
                    nameRecode.platformID = UInt16.Parse(xn.Attributes["platformID"].Value);
                    nameRecode.encodingID = UInt16.Parse(xn.Attributes["encodingID"].Value);
                    nameRecode.languageID = UInt16.Parse(xn.Attributes["languageID"].Value);
                    nameRecode.nameID = UInt16.Parse(xn.Attributes["nameID"].Value);
                    nameRecode.strContent = xn.InnerText.Trim();

                    Name.vtNameRecord.Add(nameRecode);
                }
            }

            // 得到OS2
            OS2 = new COS2();
            XmlNode xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/WeightClass");
            OS2.usWeightClass = UInt16.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/WidthClass");
            OS2.usWidthClass = UInt16.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/FsType");
            OS2.fsType = Int16.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/FamilyType");
            OS2.panose.FamilyType = Convert.ToByte(xOS2Node.InnerText.Trim()); ;

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/SerifStyle");
            OS2.panose.SerifStyle = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/Weight");
            OS2.panose.Weight = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/Proportion");
            OS2.panose.Proportion = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/Contrast");
            OS2.panose.Contrast = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/StrokeVariation");
            OS2.panose.StrokeVariation = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/ArmStyle");
            OS2.panose.ArmStyle = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/Midline");
            OS2.panose.Midline = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/Panose/XHeight");
            OS2.panose.XHeight = Byte.Parse(xOS2Node.InnerText.Trim());

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/UnicodeRange1");
            OS2.ulUnicodeRange1 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/UnicodeRange2");
            OS2.ulUnicodeRange2 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/UnicodeRange3");
            OS2.ulUnicodeRange3 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/UnicodeRange4");
            OS2.ulUnicodeRange4 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);

            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/achVendID");
            byte[] byTmp = System.Text.Encoding.Default.GetBytes(xOS2Node.InnerText.Trim());
            OS2.vtachVendID = byTmp.ToList();
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/fsSelection");
            OS2.fsSelection = Convert.ToUInt16(xOS2Node.InnerText.Trim(), 16);
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/TypoAscender");
            OS2.sTypoAscender = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/TypoDscender");
            OS2.sTypoDescender = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/TypoLineGap");
            OS2.sTypoLineGap = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/WinAscent");
            OS2.usWinAscent = UInt16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/WinDescent");
            OS2.usWinDescent = UInt16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/CodePageRange1");
            OS2.ulCodePageRange1 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/CodePageRange2");
            OS2.ulCodePageRange2 = Convert.ToUInt32(xOS2Node.InnerText.Trim(), 16);
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SubscriptXSize");
            OS2.ySubscriptXSize = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SubscriptYSize");
            OS2.ySubscriptYSize = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SubscriptXOffset");
            OS2.ySubscriptXOffset = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SubscriptYOffset");
            OS2.ySubscriptYOffset = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SuperscriptXSize");
            OS2.ySuperscriptXSize = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SuperscriptYSize");
            OS2.ySubscriptYSize = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SuperscriptXOffset");
            OS2.ySuperscriptXOffset = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/SuperscriptYOffset");
            OS2.ySuperscriptYOffset = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/StrikeoutSize");
            OS2.yStrikeoutSize = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/StrikeoutPosition");
            OS2.yStrikeoutPosition = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/xHeight");
            OS2.sxHeight = Int16.Parse(xOS2Node.InnerText.Trim());
            xOS2Node = xmlDoc.SelectSingleNode("FontProfile/OS2/CapHeight");
            OS2.sCapHeight = Int16.Parse(xOS2Node.InnerText.Trim());
            //POST
            Post = new CPost();
            XmlNode xPOSTNode = xmlDoc.SelectSingleNode("FontProfile/POST/ItalicAngle");
            double italicAngle = Double.Parse(xPOSTNode.InnerText.Trim());
            Post.italicAngle.value = (Int16)italicAngle;
            Post.italicAngle.fract = (UInt16)((italicAngle - (Int16)italicAngle) / 1000.0f * 65536.0f);
            xPOSTNode = xmlDoc.SelectSingleNode("FontProfile/POST/UnderlinePosition");
            Post.underlinePosition = Int16.Parse(xPOSTNode.InnerText.Trim());
            xPOSTNode = xmlDoc.SelectSingleNode("FontProfile/POST/UnderlineThickness");
            Post.underlineThickness = Int16.Parse(xPOSTNode.InnerText.Trim());
            xPOSTNode = xmlDoc.SelectSingleNode("FontProfile/POST/IsFixedPitch");
            Post.isFixedPitch = UInt16.Parse(xPOSTNode.InnerText.Trim());
            //HHEA
            Hhea = new CHhea();
            XmlNode xHheaNode = xmlDoc.SelectSingleNode("FontProfile/HHEA/Ascender");
            Hhea.Ascender = Int16.Parse(xHheaNode.InnerText.Trim());
            xHheaNode = xmlDoc.SelectSingleNode("FontProfile/HHEA/Descender");
            Hhea.Descender = Int16.Parse(xHheaNode.InnerText.Trim());
            xHheaNode = xmlDoc.SelectSingleNode("FontProfile/HHEA/LineGap");
            Hhea.LineGap = Int16.Parse(xHheaNode.InnerText.Trim());
            //HEAD
            Head = new CHead();
            XmlNode xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/fontversion");
            double version = Double.Parse(xHeadNode.InnerText.Trim());
            Head.fontRevision.value = (Int16)version;
            Head.fontRevision.fract = (UInt16)((version - (Int16)version) / 1000.0f * 65536.0f);
            xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/Flag");
            Head.flags = UInt16.Parse(xHeadNode.InnerText.Trim());
            xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/UnitsPerEM");
            Head.unitsPerEm = UInt16.Parse(xHeadNode.InnerText.Trim());
            xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/Macstyle");
            Head.macStyle = UInt16.Parse(xHeadNode.InnerText.Trim());
            xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/lowestRecPPEM");
            Head.lowestRecPPEM = UInt16.Parse(xHeadNode.InnerText.Trim());
            xHeadNode = xmlDoc.SelectSingleNode("FontProfile/HEAD/FontDirectionHint");
            Head.fontDirectionHint = Int16.Parse(xHeadNode.InnerText.Trim());

            //CFF
            CFFInfo = new CCFFInfo();
            XmlNode xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/CID_Font");
            CFFInfo.TopDICT.IsCIDFont = int.Parse(xCFFNode.InnerText.Trim());
            if (CFFInfo.TopDICT.IsCIDFont == 1)
            {
                xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/ROS/Registroy");
                CFFInfo.TopDICT.Ros.strRegistry = xCFFNode.InnerText.Trim();
                xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/ROS/Ordering");
                CFFInfo.TopDICT.Ros.strOrdering = xCFFNode.InnerText.Trim();
                xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/ROS/Supplement");
                CFFInfo.TopDICT.Ros.Supplement = long.Parse(xCFFNode.InnerText.Trim());
                xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/CID_FontVersion");
                CFFInfo.TopDICT.CIDFontVersion = double.Parse(xCFFNode.InnerText.Trim());

                xCFFNode = xmlDoc.SelectSingleNode("FontProfile/CFF/XUID");
                int itmp = int.Parse(xCFFNode.Attributes["setFlag"].Value.Trim());
                if (itmp == 1)
                {
                    XmlNodeList NodeLst = xCFFNode.ChildNodes;
                    for (int i = 0; i < NodeLst.Count; i++)
                    {
                        XmlNode xNode = NodeLst.Item(i);
                        if (xNode.Name == "UID")
                        {
                            CFFInfo.TopDICT.vtXUID.Add(int.Parse(xNode.InnerText.Trim()));
                        }
                    }
                }
            }

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT LoadFontFeature()            
    }
}
