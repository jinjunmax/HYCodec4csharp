using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FontParserEntity;

namespace HYFontCodecCS
{
    public class HYFontInfo
    {
        public short HHEA_Asc = 0;
        public short HHEA_Des = 0;
        public short OS_TypoAsc = 0;
        public short OS_TypoDes = 0;
        public ushort OS_WinAsc = 0;
        public ushort OS_WinDes = 0;
        public short Head_XMin = 0;
        public short Head_YMin = 0;
        public short Head_XMax = 0;
        public short Head_YMax = 0;

    }   // end of public class HYFontInfo

    public class  HYFontAPI
    {        
        public static HYRESULT FONTToWoff(string FontName, string WoffFile)
        {
            try
            {
                WoffCodec woff = new WoffCodec();
                return woff.Sfnt2Woff(FontName, WoffFile, "");
            }
            catch (Exception ext)
            {
                ext.ToString();
                throw;
            }
           

        }   // end of int FONTToWoff()

        public static  HYRESULT FontToEOT(string FontName, string EotFile)
        {
            try
            {   
                HYRESULT ret;

                FileInfo fileInf = new FileInfo(FontName);
                byte[] fntData = new byte[fileInf.Length];

                HYDecodeC FontDecode = new HYDecodeC();
                ret = FontDecode.FontOpen(FontName);
                if (ret != HYRESULT.NOERROR) return ret;

                FontDecode.DecodeTableDirectory();
                FontDecode.DecodeOS2();
                FontDecode.DecodeMaxp();
                FontDecode.DecodeHead();
                FontDecode.DecodeName();

                FontDecode.DecodeStream.Seek(0,SeekOrigin.Begin);
                FontDecode.DecodeStream.Read(fntData,0,(int)fileInf.Length);

                FontDecode.FontClose();

                FileStream eotSave = new FileStream(EotFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

                UInt32	lTmp = 0;
		        UInt16  sTmp = 0;
                byte[] aryTmp;

                //eotsize
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp,0,aryTmp.Length);
                //fontdatasize
                aryTmp = BitConverter.GetBytes((UInt32)fileInf.Length);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //vesion
                lTmp = 0x00020001;
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //Flags
                lTmp = 0;
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //FontPANOSE
                eotSave.WriteByte(FontDecode.tbOS2.panose.FamilyType);
                eotSave.WriteByte(FontDecode.tbOS2.panose.SerifStyle);
                eotSave.WriteByte(FontDecode.tbOS2.panose.Weight);
                eotSave.WriteByte(FontDecode.tbOS2.panose.Proportion);
                eotSave.WriteByte(FontDecode.tbOS2.panose.Contrast);
                eotSave.WriteByte(FontDecode.tbOS2.panose.StrokeVariation);
                eotSave.WriteByte(FontDecode.tbOS2.panose.ArmStyle);
                eotSave.WriteByte(FontDecode.tbOS2.panose.Letterform);
                eotSave.WriteByte(FontDecode.tbOS2.panose.Midline);
                eotSave.WriteByte(FontDecode.tbOS2.panose.XHeight);
                //Charset
                byte bTmp = 0x01;
                eotSave.WriteByte(bTmp);
                //Italic
                bTmp = (byte)(FontDecode.tbOS2.fsSelection&0x0001);
                eotSave.WriteByte(bTmp);
                //Weight
                lTmp = FontDecode.tbOS2.usWeightClass;
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //fstype                
                aryTmp = BitConverter.GetBytes((ushort)FontDecode.tbOS2.fsType);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //MagicNumber
                sTmp = 0x504C;
                aryTmp = BitConverter.GetBytes(sTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //UnicodeRange1		
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulUnicodeRange1);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //UnicodeRange2		
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulUnicodeRange2);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //UnicodeRange3		
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulUnicodeRange3);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //UnicodeRange4		
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulUnicodeRange4);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //CodePageRange1
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulCodePageRange1);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //CodePageRange2
                aryTmp = BitConverter.GetBytes(FontDecode.tbOS2.ulCodePageRange2);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //CheckSumAdjustment
                aryTmp = BitConverter.GetBytes(FontDecode.tbHead.checkSumAdjustment);
                eotSave.Write(aryTmp, 0, aryTmp.Length);               
                //Reserved1
                lTmp = 0;
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);                
                //Reserved2
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //Reserved3
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                //Reserved4
                aryTmp = BitConverter.GetBytes(lTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
                
                //aryTmp = BitConverter.GetBytes(eotSave.Length);

                int iNameNum = FontDecode.tbName.vtNameRecord.Count;
		        for(int i=0; i<iNameNum; i++)
		        {			
			        NAMERECORD record = FontDecode.tbName.vtNameRecord[i];
			        if (record.platformID == 3 && record.encodingID == 1&&record.languageID==1033&&record.nameID==1)
			        {				       
				        //Padding1
				        sTmp = 0;				        
                        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
                        
				        //FamilyNameSize
				        sTmp = record.length;
                        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        
				        //FamilyName
                        aryTmp = System.Text.Encoding.Unicode.GetBytes(record.strContent);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);                        
				        break;
			        }
		        }

		        for(int i=0; i<iNameNum; i++)
		        {
			        NAMERECORD record = FontDecode.tbName.vtNameRecord[i];
			        if (record.platformID == 3 && record.encodingID == 1&&record.languageID==1033&&record.nameID==2)
                    {		    
				        //Padding2
				        sTmp = 0;
                        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        
				        //StyleNameSize
				        sTmp = record.length;
				        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        //StyleName
                        aryTmp = System.Text.Encoding.Unicode.GetBytes(record.strContent);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);		
				        break;
			        }
		        }

		        for(int i=0; i<iNameNum; i++)
		        {
			        NAMERECORD record = FontDecode.tbName.vtNameRecord[i];
			        if (record.platformID == 3 && record.encodingID == 1&&record.languageID==1033&&record.nameID==5)			
			        {				        
				        //Padding3
				        sTmp = 0;
				        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        //VersionNameSize
				        sTmp = record.length;
				        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);

				        //VersionName
                        aryTmp = System.Text.Encoding.Unicode.GetBytes(record.strContent);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);		
				        break;
			        }
		        }

		        for(int i=0; i<iNameNum; i++)
		        {
			        NAMERECORD record = FontDecode.tbName.vtNameRecord[i];
			        if (record.platformID == 3 && record.encodingID == 1&&record.languageID==1033&&record.nameID==4)
			        {				        
				        //Padding4
				        sTmp = 0;
				        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        //FullNameSize
				        sTmp = record.length;
				        aryTmp = BitConverter.GetBytes(sTmp);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
				        //FullName
                        aryTmp = System.Text.Encoding.Unicode.GetBytes(record.strContent);
                        eotSave.Write(aryTmp, 0, aryTmp.Length);
                        break;
			        }			
		        }

                //Padding5
		        sTmp = 0;
		        aryTmp = BitConverter.GetBytes(sTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);
		        //RootStringSize
                aryTmp = BitConverter.GetBytes(sTmp);
                eotSave.Write(aryTmp, 0, aryTmp.Length);

                //FontData
                eotSave.Write(fntData,0,(int)fileInf.Length);
                eotSave.Flush();
                
                //Seek to EOT length  
                eotSave.Seek(0,SeekOrigin.Begin);                
                aryTmp = BitConverter.GetBytes((UInt32)eotSave.Length);
                eotSave.Write(aryTmp, 0, aryTmp.Length);

                eotSave.Close();
            }
            catch (Exception expt)
            {
                expt.ToString();
                throw;
            }

            return HYRESULT.NOERROR;

        }   // end of int FontToEOT()

        public static HYRESULT ExtractFont(string FontName, string SubName, ref List<UInt32> lstUnicode, ref List<UInt32> lstMssUni)
        {
            CFontManager Extract = new CFontManager();
            //return Extract.ExtractFont(FontName, SubName, lstUnicode, 1, ref lstMssUni);

            return Extract.GetSubset(FontName, SubName, lstUnicode, 1, ref lstMssUni);

        }   // end of public int ExtractFont()
        public static HYRESULT ModifyFontInfo(string strFileName, string strNewFileName, HYFontInfo Fntinf)
        {
            CFontManager FntMng = new CFontManager();
            return FntMng.ModifyFontInfo(strFileName,strNewFileName,Fntinf);

        }   // end of public static HYRESULT ModifyFontInfo()

        public static HYRESULT FontsToTTC(List<string> lstFontName)
        {
            CTTC ttc = new CTTC();
            return ttc.FontToTTC(lstFontName);            

        }   // end of public static HYRESULT FontsToTTC()

        public static HYRESULT FontToTTCForAFDK_C(List<string> lstFontName)
        {
            CTTC ttc = new CTTC();
            return ttc.FontToTTCForAFDK_C(lstFontName);

        }   // end of public static HYRESULT FontToTTCForAFDK_C()

        public static HYRESULT FontToTTCForAFDK_P(List<string> lstFontName)
        {
            CTTC ttc = new CTTC();
            return ttc.FontToTTCForAFDK_P(lstFontName);

        }   // end of public static HYRESULT FontToTTCForAFDK_P()        
        public static HYRESULT TTCToFonts(string strTTCName, ref List<string> lstTTFs)
        {
           CTTC ttc = new CTTC();

           return ttc.TTCToFonts(strTTCName, ref lstTTFs);
            
        }   // end of public static HYRESULT TTCToFonts()

        // 检查两个字形的兼容性
         public static int CheckCmptbl(CharInfo baseChar, CharInfo cmprChar, ref String strInfo)
        {
            if ((baseChar.IsComponent == 1) || cmprChar.IsComponent == 1) {
                strInfo = "组件字形不做兼容性检查";
                return 0;
            }

            if (baseChar.ContourCount != cmprChar.ContourCount) {
                strInfo = "两个字库轮廓数量不一致";
                return 1;
            }

            for (int i = 0; i < baseChar.ContourCount; i++)
            {
                if (HYBase.CheckCoincide(baseChar.ContourInfo[i].PtInfo))
                {
                    strInfo = "Base字形轮廓轮廓有重合点";
                    return 2;

                }
            }

            for (int i = 0; i < cmprChar.ContourCount; i++)
            {
                if (HYBase.CheckCoincide(cmprChar.ContourInfo[i].PtInfo))
                {
                    strInfo = "compare字形轮廓轮廓有重合点";
                    return 2;
                }
            }

            for (int i=0; i<baseChar.ContourCount; i++) {
                if (baseChar.ContourInfo[i].PointCount != cmprChar.ContourInfo[i].PointCount) {
                    strInfo = "第" + (i + 1).ToString()+"轮廓,点数量不一致。";
                    return 3;
                }

                for (int j=0; j< baseChar.ContourInfo[i].PointCount; j++){

                    if (baseChar.ContourInfo[i].PtInfo[j].PtType !=
                        cmprChar.ContourInfo[i].PtInfo[j].PtType){

                        strInfo = "第"+(i + 1).ToString() +"轮廓,第"+(j+i).ToString()+"点类型不一致。";
                        return 4;
                    }
                }
            }

            return 0;

        }   // end of public static int CheckCmptbl()

    }
}
