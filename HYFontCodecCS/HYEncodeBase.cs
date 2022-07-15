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
    public class HYEncodeBase : HYFontBase
    {  
        public FileStream EncodeStream
        {            
            get { return FileStrm;}
        }

        public HYRESULT FontOpen(string strFileName)
        {
            try
            {
                FileStrm = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                throw;
            }
            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontOpen()
        public void  FontClose()
        {
            if (FileStrm != null)
            {
                if (FileStrm.CanWrite)
                {
                    FileStrm.Flush();                    
                }

                FileStrm.Close();
            }

        }   // end of public void  FontClose()

        public void AlignmentData(uint length)
        {            
            if (length % 4 > 0)
            {
                uint iTail = 4 - length % 4;
                byte c = 0;
                for (int i = 0; i < iTail; i++)
                {
                    FileStrm.WriteByte(c);
                }
            }

        }   // end of protected void AlignmentData()

        public HYRESULT EncodeTableDirectory()
        {            
			TableDirectorty.numTables = (UInt16)TableDirectorty.vtTableEntry.Count;	

			double searchRange = 0; 
			searchRange = (ushort)(Math.Log((double)TableDirectorty.numTables)/Math.Log(2.0));
			searchRange = (ushort)Math.Pow(2.0, searchRange);
			searchRange = searchRange*16;

			TableDirectorty.searchRange = (UInt16)searchRange;
			TableDirectorty.entrySelector = (UInt16)(Math.Log((float)(TableDirectorty.searchRange/16))/Math.Log(2.0));
			TableDirectorty.rangeShift = (UInt16)(TableDirectorty.numTables*16 - TableDirectorty.searchRange);

            FileStrm.Seek(0,SeekOrigin.Begin);

			UInt16	usTmp;
			UInt32	ulTmp;
            byte[]  btTmp;
			// sfnt version
			usTmp = hy_cdr_int16_to((UInt16)TableDirectorty.version.value);            
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			usTmp = hy_cdr_int16_to(TableDirectorty.version.fract);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			//numTables
			usTmp = hy_cdr_int16_to(TableDirectorty.numTables);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			// searchRange
			usTmp = hy_cdr_int16_to(TableDirectorty.searchRange);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			// entrySelector
			usTmp = hy_cdr_int16_to(TableDirectorty.entrySelector);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			//rangeShift
			usTmp = hy_cdr_int16_to(TableDirectorty.rangeShift);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp,0,btTmp.Length);

			for (int i=0; i<TableDirectorty.numTables; i++)
			{
				CTableEntry HYEntry = TableDirectorty.vtTableEntry[i];
				//tag		
				ulTmp = hy_cdr_int32_to(HYEntry.tag);
                btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp,0,btTmp.Length);
				
				// checkSum
				ulTmp = hy_cdr_int32_to(HYEntry.checkSum);			
				btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp,0,btTmp.Length);
				//offset
				ulTmp = hy_cdr_int32_to(HYEntry.offset);
				btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp,0,btTmp.Length);
				//length
				ulTmp = hy_cdr_int32_to(HYEntry.length);
				btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp,0,btTmp.Length);
			}			

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeTableDirectory()      

        public HYRESULT Encodecmap(List<HYCodeMapItem> lstCodeMap)
        {            
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.CMAP_TAG);

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to(Cmap.version);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //numSubTable
            usTmp = hy_cdr_int16_to(Cmap.numSubTable);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            int i = 0;
            for (i = 0; i<Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY  tbCmapEntry = Cmap.vtCamp_tb_entry[i];

                // platformID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                // encodingID
                usTmp = hy_cdr_int16_to(tbCmapEntry.plat_encod_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                // offset				
                tbCmapEntry.offset = tbEntry.offset;
                ulTmp = hy_cdr_int32_to(tbEntry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);          
            }

            for (i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY  tbCmapEntry = Cmap.vtCamp_tb_entry[i];
                switch (tbCmapEntry.format)
                {
                    case 0:
                        {
                            List<byte> vtCmp0 = new List<byte>();
                            EncodeCmapFmt0(ref tbCmapEntry, ref vtCmp0, lstCodeMap);
                        }
                        break;
                    case 2:
                        {
                            List<byte> vtCmp2 = new List<byte>();
                            EncodeCmapFmt2(ref tbCmapEntry, ref vtCmp2, lstCodeMap);
                        }
                        break;
                    case 4:
                        {
                            List<byte> vtCmp4 = new List<byte>();
                            if (EncodeCmapFmt4(ref tbCmapEntry, ref vtCmp4, lstCodeMap) == -1)
                            {
                                EncodeCmapFmt4forGB_CJK(ref tbCmapEntry, ref vtCmp4, lstCodeMap);
                            }
                        }
                        break;
                    case 6:
                        {
                            List<byte> vtCmp6 = new List<byte>();
                            EncodeCmapFmt6(ref tbCmapEntry, ref vtCmp6, lstCodeMap);
                        }
                        break;
                    case 8:
                        {
                            List<byte> vtCmp8 = new List<byte>();
                            EncodeCmapFmt8(ref tbCmapEntry, ref vtCmp8, lstCodeMap);
                        }
                        break;
                    case 10:
                        {
                            List<byte> vtCmp10 = new List<byte>();
                            EncodeCmapFmt10(ref tbCmapEntry, ref vtCmp10, lstCodeMap);
                        }
                        break;
                    case 12:
                        {
                            List<byte> vtCmp12 = new List<byte>();
                            EncodeCmapFmt12(ref tbCmapEntry, ref vtCmp12, lstCodeMap);
                        }
                        break;
                    case 13:
                        {
                            List<byte> vtCmp13 = new List<byte>();
                            EncodeCmapFmt13(ref tbCmapEntry, ref vtCmp13, lstCodeMap);
                        }
                        break;
                    case 14:
                        {
                            List<byte> vtCmp14 = new List<byte>();
                            EncodeCmapFmt14(ref tbCmapEntry, ref vtCmp14, lstCodeMap);
                        }
                        break;
                    default:
                        break;
                }           
            }

            long ulCmapEndPos = FileStrm.Position;
            FileStrm.Seek(tbEntry.offset+4,SeekOrigin.Begin);

            for (i = 0; i < Cmap.numSubTable; i++)
            {
                CMAP_TABLE_ENTRY tbCampEntry = Cmap.vtCamp_tb_entry[i];

                // platformID
                usTmp = hy_cdr_int16_to(tbCampEntry.plat_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                // encodingID
                usTmp = hy_cdr_int16_to(tbCampEntry.plat_encod_ID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                // offset
                ulTmp = hy_cdr_int32_to(tbCampEntry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
            }

            FileStrm.Flush();
            FileStrm.Seek(ulCmapEndPos,SeekOrigin.Begin);
            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Decodecmap()

        public void EncodeCmapFmt0(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {
            UInt16 usTmp;            
            byte[] btTmp;

            CMAP_ENCODE_FORMAT_0		CEF0 = entry.Format0;
			CEF0.format = 0;

            usTmp = hy_cdr_int16_to(CEF0.format);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

			// length
			CEF0.length = 262;
            usTmp = hy_cdr_int16_to(CEF0.length);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

			// language
			CEF0.language = 0;
            usTmp = hy_cdr_int16_to(CEF0.language);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);		

			// glyphIdArray			
			// 第一个必须是GID == 0;
            vtCmap.Add(0);
			vtCmap.Add(0);
			for (uint i=1; i<256; i++)
			{			
                int iGID = FindGryphIndexByUnciodeEx(i, lstCodeMap);
				if (iGID>0&&iGID<256)
				{
                    usTmp = hy_cdr_int16_to((UInt16)iGID);
                    btTmp = BitConverter.GetBytes(usTmp);
                    for (int j = 0; j < btTmp.Length; j++) vtCmap.Add(btTmp[j]);
				}					
				else 
				{
                    vtCmap.Add(0);
                    vtCmap.Add(0);
				}
			}

            entry.offset = (uint)(FileStrm.Position - entry.offset);
            for (int i = 0; i < vtCmap.Count; i++)
            {
                FileStrm.WriteByte(vtCmap[i]);
            }

        }   // end of protected HYRESULT	EncodeCmapFmt0()

        public void EncodeCmapFmt2(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {

            

        }   // end of protected HYRESULT	EncodeCmapFmt2()

        public int EncodeCmapFmt4(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {
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
                int nextIx = NextGroup(ix, eov,ref group, lstCodeMap);
                if (group.startCode < 0xffff)
                {	// This format does not code above 0xfffe
                    if (group.endCode > 0xfffe)
                        group.endCode = 0xfffe;

                    groupList.Add(group);
                }
                ix = nextIx;                
            }          
			
			UInt32									lastCode = 0;
			UInt16									entryCount = 0;
			UInt16									groupCount = 0;

            GroupRecord gix = groupList[0];
            GroupRecord gend = groupList[groupList.Count-1];

            int iPoint = 0;
            //if (gix != groupList[groupList.Count - 1])
             //   ++iPoint;
            
            while(true)
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
            UInt16[] endTable = new UInt16 [groupCount];
            Int16[] idDelta = new Int16 [groupCount];
            UInt16[] idRangeOffset = new UInt16 [groupCount];
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
			entry.Format4.segCountX2	= (ushort)(groupCount*2);
			usTmp = hy_cdr_int16_to(entry.Format4.segCountX2);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

			//searchRange
			entry.Format4.searchRange	= (ushort)(2* Math.Pow(2,Math.Floor((Math.Log((double)groupCount)/Math.Log(2.0)))));
            usTmp = hy_cdr_int16_to(entry.Format4.searchRange);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);
				
			//entrySelector
			entry.Format4.entrySelector		=	(ushort)(Math.Log(entry.Format4.searchRange/2.0)/Math.Log(2.0));
            usTmp = hy_cdr_int16_to(entry.Format4.entrySelector);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

			//rangeShift
			entry.Format4.rangeShift		=	(ushort)(entry.Format4.segCountX2-entry.Format4.searchRange);
            usTmp = hy_cdr_int16_to(entry.Format4.rangeShift);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int i = 0; i < btTmp.Length; i++) vtCmap.Add(btTmp[i]);

            //endCount			
			for (int i=0; i<groupCount; i++)
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
			for (int i=0; i<groupCount; i++)
			{
				if (idRangeOffset[i] == 0xffff)
				{
					usTmp = 0;
				}
				else 
				{
					usTmp = (ushort)(2*(idRangeOffset[i]+groupCount-i));
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

            entry.offset = (uint)(FileStrm.Position - entry.offset);
            FileStrm.Write(vtCmap.ToArray(), 0, vtCmap.Count);

            return 0;

        }   // end of protected HYRESULT	EncodeCmapFmt4()

        public int EncodeCmapFmt4forGB_CJK(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {
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
                int nextIx = NextGroup(ix, eov,ref group, lstCodeMap);
                if (group.startCode < 0xffff)
                {	// This format does not code above 0xfffe
                    if (group.endCode > 0xfffe)
                        group.endCode = 0xfffe;

                    groupList.Add(group);
                }
                ix = nextIx;                
            }          
			
			UInt32									lastCode = 0;			
			UInt16									groupCount = 0;

            int iPointGix=0;            
            GroupRecord gix = groupList[0];
            GroupRecord gend = groupList[groupList.Count-1];
            GroupRecord glast = groupList[0];

            if (gix != gend)
            {
                gix = groupList[++iPointGix];                
            }
            while (true)
            { 
                	ulong		curCodeCount = gix.endCode - gix.startCode+1;
                    ulong       lastCodeCount = glast.endCode - glast.startCode + 1;

                    if (glast.endCode == 65505)
                    {
                        uint iTest  = gix.endCode;
                    }

                    if (gix.endCode == 65505)
                    {
                        uint iTest = gix.endCode;
                    }

					// These constants are empirical. They give the most compact results for CJK-style encodings.                  
					if (gix.startCode - glast.endCode < 4)
					{
						if (!glast.contiguous && curCodeCount < 5 ||
                            gix.contiguous == glast.contiguous &&
                            (!gix.contiguous ||	gix.startCode-gix.startGID ==
							(int)glast.startCode -
							(int)glast.startGID))
						{
							glast.contiguous = false;
							glast.endCode = gix.endCode;
							groupList.Remove(gix);
                            gix = glast;
                            iPointGix = groupList.FindIndex(GroupRecord => GroupRecord.startCode == glast.startCode);
						}
						else if (glast.contiguous && lastCodeCount < 5)
						{	// Don't bother with the contiguous nature of the last run because it's so short. Consolidate.
							glast.contiguous = false;
							glast.endCode = gix.endCode;
							groupList.Remove(gix);
                            gix = glast;
                            iPointGix = groupList.FindIndex(GroupRecord => GroupRecord.startCode == glast.startCode);
						}
					}

                    if (iPointGix == groupList.Count - 1)
                        break;
					glast = gix;                    
                    gix = groupList[++iPointGix];            
            }

            UInt16 entryCount = 0;
            gend = groupList[groupList.Count - 1];
            int iPoint = 0;
            gix = groupList[0];
            while(true)
            {
                if (!gix.contiguous)
                    entryCount += (UInt16)(gix.endCode - gix.startCode + 1);                

                if (gix == gend)                
                {
                    //if (!gend.contiguous)
                      //  entryCount += (UInt16)(gend.endCode - gend.startCode + 1);

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
            UInt16[] endTable = new UInt16 [groupCount];
            Int16[] idDelta = new Int16 [groupCount];
            UInt16[] idRangeOffset = new UInt16 [groupCount];
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
            vtCmap.AddRange(btTmp.ToList());
            
			//length
			entry.Format4.length = (ushort)length;
			usTmp = hy_cdr_int16_to(entry.Format4.length);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());            

			//language
			entry.Format4.language = 0;
			usTmp = hy_cdr_int16_to(entry.Format4.language);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());           

			//segCountX2
			entry.Format4.segCountX2	= (ushort)(groupCount*2);
			usTmp = hy_cdr_int16_to(entry.Format4.segCountX2);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());

			//searchRange
			entry.Format4.searchRange	= (ushort)(2* Math.Pow(2,Math.Floor((Math.Log((double)groupCount)/Math.Log(2.0)))));
            usTmp = hy_cdr_int16_to(entry.Format4.searchRange);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());
				
			//entrySelector
			entry.Format4.entrySelector		=	(ushort)(Math.Log(entry.Format4.searchRange/2.0)/Math.Log(2.0));
            usTmp = hy_cdr_int16_to(entry.Format4.entrySelector);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());

			//rangeShift
			entry.Format4.rangeShift		=	(ushort)(entry.Format4.segCountX2-entry.Format4.searchRange);
            usTmp = hy_cdr_int16_to(entry.Format4.rangeShift);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());

            //endCount			
			for (int i=0; i<groupCount; i++)
			{
				usTmp = endTable[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                vtCmap.AddRange(btTmp.ToList());	
			}

            //reservedPad
            entry.Format4.reservedPad = 0;
            usTmp = hy_cdr_int16_to(entry.Format4.reservedPad);
            btTmp = BitConverter.GetBytes(usTmp);
            vtCmap.AddRange(btTmp.ToList());

            //startCount
            for (int i = 0; i < groupCount; i++)
            {
                usTmp = startTable[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                vtCmap.AddRange(btTmp.ToList());
            }

            //idDelta
            for (int i = 0; i < groupCount; i++)
            {
                usTmp = (ushort)idDelta[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                vtCmap.AddRange(btTmp.ToList());
            }		

            //idRangeOffset
			for (int i=0; i<groupCount; i++)
			{
				if (idRangeOffset[i] == 0xffff)
				{
					usTmp = 0;
				}
				else 
				{
					usTmp = (ushort)(2*(idRangeOffset[i]+groupCount-i));
				}
                
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                vtCmap.AddRange(btTmp.ToList());
			}

            //glyphIdArray
            for (int i = 0; i < glyphIdArray.Count; i++)
            {
                usTmp = glyphIdArray[i];
                usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                vtCmap.AddRange(btTmp.ToList());
            }

            entry.offset = (uint)(FileStrm.Position - entry.offset);
            FileStrm.Write(vtCmap.ToArray(),0,vtCmap.Count);

            return 0;

        }   // end of protected int EncodeCmapFmt4forGB_CJK()

        public int NextGroup(int ix, int eov, ref GroupRecord group, List<HYCodeMapItem> lstCodeMap)
        {
            int entryCount = 0;
            bool haveContiguous = false;		// Neither contiguous nor discontiguous yet
            bool contiguous = true;			// Default to true for single glyph range.

            if (ix != eov)
            {
                UInt32 startCharCode = lstCodeMap[ix].Unicode;
                int startGID = lstCodeMap[ix].GID;
			    int					hold = ix;
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

        public void EncodeCmapFmt6(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {

            

        }   // end of protected HYRESULT	EncodeCmapFmt6()

        public void EncodeCmapFmt8(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {
            

        }   // end of protected HYRESULT	EncodeCmapFmt8()

        public void EncodeCmapFmt10(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {

            
        }   // end of protected HYRESULT 	EncodeCmapFmt10()

        public void EncodeCmapFmt12(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {
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
			entry.Format12.format =12;
            usTmp = hy_cdr_int16_to(entry.Format12.format);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);				

			entry.Format12.reserved				= 0;
			usTmp = hy_cdr_int16_to(entry.Format12.reserved);
            btTmp = BitConverter.GetBytes(usTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);				

                
			// 16 == format + reserved + length + language + nGroups
			// 12 == startCharCode + endCharCode + startGlyphID
			entry.Format12.length				= (uint)(16+entry.Format12.vtGroup.Count*12);
            ulTmp = hy_cdr_int32_to(entry.Format12.length);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);				

			entry.Format12.language				= 0;
			ulTmp = hy_cdr_int32_to(entry.Format12.language);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);				

			entry.Format12.nGroups				= (uint)entry.Format12.vtGroup.Count;
            ulTmp = hy_cdr_int32_to(entry.Format12.nGroups);
            btTmp = BitConverter.GetBytes(ulTmp);
            for (int w = 0; w < btTmp.Length; w++) vtCmap.Add(btTmp[w]);

            ulTmp = 0;
            for (int x = 0; x<entry.Format12.vtGroup.Count; x++)
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

        public void EncodeCmapFmt13(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {

            

        }   // end of protected HYRESULT	EncodeCmapFmt13()

        public void EncodeCmapFmt14(ref CMAP_TABLE_ENTRY entry, ref List<byte> vtCmap, List<HYCodeMapItem> lstCodeMap)
        {

            

        }   // end of protected HYRESULT	EncodeCmapFmt14()

        public HYRESULT Encodehead()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.HEAD_TAG);
            if (iEntryIndex == -1) return HYRESULT.HEAD_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;

            //Table version number
            Head.version.value = 1;            
            usTmp = hy_cdr_int16_to((UInt16)Head.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            Head.version.fract = 0;
            usTmp = hy_cdr_int16_to(Head.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //fontRevision			
            usTmp = hy_cdr_int16_to((UInt16)Head.fontRevision.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to(Head.fontRevision.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            // checkSumAdjustment		
            ulTmp = hy_cdr_int32_to(Head.checkSumAdjustment);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //magicNumber
            Head.magicNumber = 0x5F0F3CF5;
            ulTmp = hy_cdr_int32_to(Head.magicNumber);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //flags
            usTmp = hy_cdr_int16_to(Head.flags);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //unitsPerEm
            usTmp = hy_cdr_int16_to(Head.unitsPerEm);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);            
            
            //long tspace = 2082844799; // 1904年 1月1日 0点            
            long FileTimeUtc = 0;
            DateTime dt0 = new DateTime(1904, 1, 1);
            DateTime dt1 = System.DateTime.Now.ToUniversalTime();            
            TimeSpan ts = new TimeSpan(dt1.Ticks - dt0.Ticks);
            FileTimeUtc = (long)ts.TotalSeconds;
            btTmp = BitConverter.GetBytes(FileTimeUtc);
            // created
            if (Head.created == null)
            {
                for (int i = 0; i < 8; i++)
                {
                    FileStrm.WriteByte(btTmp[7 - i]);
                }
            }
            else
            {
                FileStrm.Write(Head.created,0,8);
            }

            //modified
            for (int i = 0; i < 8; i++)
            {
                FileStrm.WriteByte(btTmp[7 - i]);
            }

            //xMin
            usTmp = hy_cdr_int16_to((UInt16)Head.xMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //yMin
            usTmp = hy_cdr_int16_to((UInt16)Head.yMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //xMax
            usTmp = hy_cdr_int16_to((UInt16)Head.xMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //yMax
            usTmp = hy_cdr_int16_to((UInt16)Head.yMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //macStyle
            usTmp = hy_cdr_int16_to((UInt16)Head.macStyle);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //lowestRecPPEM
            usTmp = hy_cdr_int16_to((UInt16)Head.lowestRecPPEM);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //fontDirectionHint
            usTmp = hy_cdr_int16_to((UInt16)Head.fontDirectionHint);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //indexToLocFormat
            Head.indexToLocFormat = 1;
            usTmp = hy_cdr_int16_to((UInt16)Head.indexToLocFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //glyphDataFormat
            Head.glyphDataFormat = 0;
            usTmp = hy_cdr_int16_to((UInt16)Head.glyphDataFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodehead()   

        public HYRESULT EncodeheadEx()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.HEAD_TAG);
            if (iEntryIndex == -1) return HYRESULT.HEAD_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;

            //Table version number
            Head.version.value = 1;
            usTmp = hy_cdr_int16_to((UInt16)Head.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            Head.version.fract = 0;
            usTmp = hy_cdr_int16_to(Head.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //fontRevision			
            usTmp = hy_cdr_int16_to((UInt16)Head.fontRevision.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to(Head.fontRevision.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            // checkSumAdjustment		
            ulTmp = hy_cdr_int32_to(Head.checkSumAdjustment);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //magicNumber
            Head.magicNumber = 0x5F0F3CF5;
            ulTmp = hy_cdr_int32_to(Head.magicNumber);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //flags
            usTmp = hy_cdr_int16_to(Head.flags);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //unitsPerEm
            usTmp = hy_cdr_int16_to(Head.unitsPerEm);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //long tspace = 2082844799; // 1904年 1月1日 0点            
            long FileTimeUtc = 0;
            DateTime dt0 = new DateTime(1904, 1, 1);
            DateTime dt1 = System.DateTime.Now.ToUniversalTime();
            TimeSpan ts = new TimeSpan(dt1.Ticks - dt0.Ticks);
            FileTimeUtc = (long)ts.TotalSeconds;
            btTmp = BitConverter.GetBytes(FileTimeUtc);
            // created
            if (Head.created == null)
            {
                for (int i = 0; i < 8; i++)
                {
                    FileStrm.WriteByte(btTmp[7 - i]);
                }
            }
            else
            {
                FileStrm.Write(Head.created, 0, 8);
            }

            //modified
            for (int i = 0; i < 8; i++)
            {
                FileStrm.WriteByte(btTmp[7 - i]);
            }

            //xMin
            usTmp = hy_cdr_int16_to((UInt16)Head.xMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //yMin
            usTmp = hy_cdr_int16_to((UInt16)Head.yMin);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //xMax
            usTmp = hy_cdr_int16_to((UInt16)Head.xMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //yMax
            usTmp = hy_cdr_int16_to((UInt16)Head.yMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //macStyle
            usTmp = hy_cdr_int16_to((UInt16)Head.macStyle);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //lowestRecPPEM
            usTmp = hy_cdr_int16_to((UInt16)Head.lowestRecPPEM);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //fontDirectionHint
            usTmp = hy_cdr_int16_to((UInt16)Head.fontDirectionHint);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //indexToLocFormat            
            usTmp = hy_cdr_int16_to((UInt16)Head.indexToLocFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //glyphDataFormat            
            usTmp = hy_cdr_int16_to((UInt16)Head.glyphDataFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeheadEx()

        public HYRESULT Encodehhea()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.HHEA_TAG);
            if (iEntryIndex == -1) return HYRESULT.HHEA_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;            
            byte[] btTmp;

            Hhea.version.value = 1;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            Hhea.version.fract = 0;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //Ascender
            if (Hhea.Ascender == 0)
                Hhea.Ascender = Head.yMax;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.Ascender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //Descender
            if (Hhea.Descender == 0)
                Hhea.Descender = Head.yMin;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.Descender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //LineGap			
            usTmp = hy_cdr_int16_to((UInt16)Hhea.LineGap);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //advanceWidthMax			
            usTmp = hy_cdr_int16_to(GetAdvancMaxWidth());
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //minLeftSideBearing
            Hhea.minLeftSideBearing = Head.xMin;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.minLeftSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //minRightSideBearing
            Hhea.minRightSideBearing = FindMinRightSideBearing();
            usTmp = hy_cdr_int16_to((UInt16)Hhea.minRightSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //xMaxExtent
            Hhea.xMaxExtent = (Int16)(Hhea.minLeftSideBearing+(Int16)(Head.xMax - Head.xMin));
            usTmp = hy_cdr_int16_to((UInt16)Hhea.xMaxExtent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRise		
            usTmp = hy_cdr_int16_to(1);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRun
            usTmp = 0;
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretOffset            
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved1			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved2			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved3			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved4			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //metricDataFormat
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //numberOfHMetrics
            int iNum = GlyphChars.CharInfo.Count;
			Hhea.numberOfHMetrics = 0;
			if (iNum>0)
			{
				//int				iBaseadvanceWidth = Hmtx.lstLonghormetric[--iNum].advanceWidth;
                int iBaseadvanceWidth = GlyphChars.CharInfo[--iNum].AdWidth;		
				while(--iNum>=0)
				{
                    if (GlyphChars.CharInfo[iNum].AdWidth == iBaseadvanceWidth)
						Hhea.numberOfHMetrics++;
					else 
						break;
				}
                Hhea.numberOfHMetrics = (UInt16)(GlyphChars.CharInfo.Count - Hhea.numberOfHMetrics);
			}
            usTmp = hy_cdr_int16_to(Hhea.numberOfHMetrics);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodehhea()

        // 函数内部不做任何的跨表计算,这様会减少代码关联
        public HYRESULT EncodehheaEx()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.HHEA_TAG);
            if (iEntryIndex == -1) return HYRESULT.HHEA_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;
            byte[] btTmp;

            Hhea.version.value = 1;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            Hhea.version.fract = 0;
            usTmp = hy_cdr_int16_to((UInt16)Hhea.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //Ascender
            usTmp = hy_cdr_int16_to((UInt16)Hhea.Ascender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //Descender
            usTmp = hy_cdr_int16_to((UInt16)Hhea.Descender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //LineGap			
            usTmp = hy_cdr_int16_to((UInt16)Hhea.LineGap);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //advanceWidthMax			
            usTmp = hy_cdr_int16_to(Hhea.advanceWidthMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //minLeftSideBearing            
            usTmp = hy_cdr_int16_to((UInt16)Hhea.minLeftSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //minRightSideBearing            
            usTmp = hy_cdr_int16_to((UInt16)Hhea.minRightSideBearing);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //xMaxExtent            
            usTmp = hy_cdr_int16_to((UInt16)Hhea.xMaxExtent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRise		
            usTmp = hy_cdr_int16_to(1);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretSlopeRun
            usTmp = 0;
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //caretOffset            
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved1			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved2			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved3			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //reserved4			
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //metricDataFormat
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //numberOfHMetrics            
            usTmp = hy_cdr_int16_to(Hhea.numberOfHMetrics);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodehheaEx()

        public short FindMinRightSideBearing()
	    {
            int iGlyphNum = this.GlyphChars.CharInfo.Count;
            if (iGlyphNum == 0) return -1;

            int xmin,ymin,xmax,ymax;
            BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
            short minRSB = (short)(GlyphChars.CharInfo[0].AdWidth - xmin - (xmax - xmin));
            for (int i = 1; i < iGlyphNum; i++)
		    {
                BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
                short tmp = (short)(GlyphChars.CharInfo[i].AdWidth - xmin - (xmax - xmin));
			    if (tmp<minRSB)  
			    {					
				    minRSB = tmp;								
			    }
		    }
		
		    return minRSB;

	    }	// end of int FindMinRightSideBearing()

        public HYRESULT Encodehmtx()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.HMTX_TAG);
            if (iEntryIndex == -1) return HYRESULT.HMTX_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;            
            byte[] btTmp;

            int iNum = GlyphChars.CharInfo.Count;//Hmtx.lstLonghormetric.Count;
			UInt16      longhormetricNums	= Hhea.numberOfHMetrics;
            UInt16      lefsidebearNums = (UInt16)(iNum-Hhea.numberOfHMetrics);

            for (int i=0; i<iNum; i++)
            {
                int xmin=0, ymin=0, xmax=0, ymax=0;
                BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax,out ymax);

                if (i<longhormetricNums)
                {
                    usTmp = hy_cdr_int16_to((UInt16)GlyphChars.CharInfo[i].AdWidth);                    
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    usTmp = hy_cdr_int16_to((UInt16)xmin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                }
                else
                {
                    usTmp = hy_cdr_int16_to((UInt16)xmin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                }
            }

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodehmtx()

        public HYRESULT Encodemaxp()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.MAXP_TAG);
            if (iEntryIndex == -1) return HYRESULT.MAXP_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;            
            byte[] btTmp;

            Maxp = new CMaxp();

            int GlyphNum = GlyphChars.CharInfo.Count;
            if (FontType == FONTTYPE.CFF)
            {
                //Table version number
                Maxp.version.value = 0;                
                btTmp = BitConverter.GetBytes(Maxp.version.value);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                Maxp.version.fract = 0x5000;
                usTmp = hy_cdr_int16_to(Maxp.version.fract);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                
                //numGlyphs
                Maxp.numGlyphs = (UInt16)GlyphNum;
                usTmp = hy_cdr_int16_to(Maxp.numGlyphs);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);           
            }

            if (FontType == FONTTYPE.TTF)
            {
                //Table version number
                Maxp.version.value = 1;
                usTmp = hy_cdr_int16_to((UInt16)Maxp.version.value);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                Maxp.version.fract = 0;
                btTmp = BitConverter.GetBytes(Maxp.version.fract);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //numGlyphs
                Maxp.numGlyphs = (UInt16)GlyphNum;
                usTmp = hy_cdr_int16_to(Maxp.numGlyphs);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);  

                UInt16 MaxPoints=0; 
                Int16 CntursNum,MaxTContours=0;
				for (int i=0; i<GlyphNum; i++)
				{
					ushort TmpPoint = 0;
                    CharInfo glyph = GlyphChars.CharInfo[i];					
					CntursNum = (short)glyph.ContourCount;
					if (CntursNum == 0) continue;

					if (MaxTContours<CntursNum) 
						MaxTContours=CntursNum;

                    for (int j=0; j<glyph.ContourCount; j++)
                    {
					    TmpPoint+=(ushort)glyph.ContourInfo[j].PointCount;
                    }
					if (MaxPoints<TmpPoint)
						MaxPoints=TmpPoint;
				}
                //maxPoints
				Maxp.maxPoints = MaxPoints;
				usTmp = hy_cdr_int16_to(Maxp.maxPoints);
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxContours
				Maxp.maxContours = (UInt16)MaxTContours;
				usTmp = hy_cdr_int16_to(Maxp.maxContours);
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxCompositePoints				
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxCompositeContours
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxZones
				usTmp = hy_cdr_int16_to(2);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxTwilightPoints
				usTmp = hy_cdr_int16_to(4);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxStorage
				usTmp = hy_cdr_int16_to(32);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxFunctionDefs
				usTmp = hy_cdr_int16_to(96);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxInstructionDefs
				usTmp = hy_cdr_int16_to(96);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxStackElements
				usTmp = hy_cdr_int16_to(256);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxSizeOfInstructions
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxComponentElements
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
				//maxComponentDepth
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length); 
            }

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR; 

        }   // end of protected HYRESULT Encodemaxp()

        public HYRESULT Encodename()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.NAME_TAG);
            if (iEntryIndex == -1) return HYRESULT.NAME_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp=0;
            byte[] btTmp;

            //format
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length); 

            //count
			usTmp = hy_cdr_int16_to((UInt16)Name.vtNameRecord.Count);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //stringOffset
			UInt16 usOffset = (UInt16)(6+12*(UInt16)Name.vtNameRecord.Count);
			usTmp = hy_cdr_int16_to(usOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            int usStringSize = 0;
			byte[] StrContent = new byte[260*4*Name.vtNameRecord.Count];

            for (int i = 0; i < Name.vtNameRecord.Count; i++)
            {
                NAMERECORD tagNameRecord = Name.vtNameRecord[i];
                if (
                    (tagNameRecord.platformID == 3 && tagNameRecord.encodingID == 1) ||
                    (tagNameRecord.platformID == 0 && tagNameRecord.encodingID == 3) ||
                    (tagNameRecord.platformID == 0 && tagNameRecord.encodingID == 4) ||
                    (tagNameRecord.platformID == 0 && tagNameRecord.encodingID == 6)
                    )
                {
                    Encoding utf16 = System.Text.Encoding.BigEndianUnicode;
                    byte[] btString = utf16.GetBytes(tagNameRecord.strContent);
                    int strlen = utf16.GetByteCount(tagNameRecord.strContent.ToCharArray());
                    tagNameRecord.length = (UInt16)strlen;

                    btString.CopyTo(StrContent, usStringSize);
                }
                else
                {
                    Encoding AscII = System.Text.Encoding.Default;
                    byte[] btString = AscII.GetBytes(tagNameRecord.strContent);
                    int strlen = AscII.GetByteCount(tagNameRecord.strContent.ToCharArray());
                    tagNameRecord.length = (UInt16)strlen;
                    btString.CopyTo(StrContent, usStringSize);
                }
                usStringSize += tagNameRecord.length;            
            }

            UInt16  Offset = 0;
            for (int i=0; i<Name.vtNameRecord.Count; i++)
            {
                NAMERECORD  tagNameRecord = Name.vtNameRecord[i];

                //platformID
                usTmp = hy_cdr_int16_to(tagNameRecord.platformID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //encodingID
                usTmp = hy_cdr_int16_to(tagNameRecord.encodingID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //languageID
                usTmp = hy_cdr_int16_to(tagNameRecord.languageID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //nameID
                usTmp = hy_cdr_int16_to(tagNameRecord.nameID);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //length				
                usTmp = hy_cdr_int16_to(tagNameRecord.length);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
                //offset				
                usTmp = hy_cdr_int16_to(Offset);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

                Offset += tagNameRecord.length;            
            }
            FileStrm.Write(StrContent, 0, usStringSize);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodename()

        public HYRESULT EncodeOS2(List<HYCodeMapItem> lstCodeMap,UInt32 TableFlag)
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.OS2_TAG);
            if (iEntryIndex == -1) return HYRESULT.OS2_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp = 0;
            UInt32 ulTmp = 0;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to(0x0003);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //xAvgCharWidth
            int iAvg=0, iCount=0;
            for (int i = 0; i < GlyphChars.CharInfo.Count; i++)
			{
                if (GlyphChars.CharInfo[i].AdWidth!= 0)
                {
                    iAvg += GlyphChars.CharInfo[i].AdWidth;
                    iCount++;
                }
			}			
			usTmp = (UInt16)(iAvg/iCount+0.5f);
			usTmp = hy_cdr_int16_to(usTmp);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWeightClass			
            if (OS2.usWeightClass == 0)
            {
                OS2.usWeightClass = 400;
            }
            usTmp = hy_cdr_int16_to(OS2.usWeightClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWidthClass
            if (OS2.usWidthClass == 0)
            {
                float fRatioe = (float)((Head.xMax-Head.xMin)*100)/(float)(Head.yMax-Head.yMin);
                if (fRatioe <= 50.0) OS2.usWidthClass = 1;
                if (fRatioe > 50.0 && fRatioe <= 62.5) OS2.usWidthClass = 2;
                if (fRatioe > 62.5 && fRatioe <= 75.0) OS2.usWidthClass = 3;
                if (fRatioe > 75.0 && fRatioe <= 87.5) OS2.usWidthClass = 4;
                if (fRatioe > 87.5 && fRatioe < 112.5) OS2.usWidthClass = 5;
                if (fRatioe >= 112.5 && fRatioe < 125.0) OS2.usWidthClass = 6;
                if (fRatioe >= 125.0 && fRatioe < 150.0) OS2.usWidthClass = 7;
                if (fRatioe >= 150.0 && fRatioe < 200) OS2.usWidthClass = 8;
                if (fRatioe >= 200.0) OS2.usWidthClass = 9;
            }
            usTmp = hy_cdr_int16_to(OS2.usWidthClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //fsType
            if (OS2.fsType == 0)
            {
                OS2.fsType = 0x0008;
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.fsType);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptXSize
            if (OS2.ySubscriptXSize == 0)
            {
                OS2.ySubscriptXSize = (short)(Head.unitsPerEm/10);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptXSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptYSize
            if (OS2.ySubscriptYSize == 0)
            {
                OS2.ySubscriptYSize = (short)(Head.unitsPerEm/10);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptYSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptXOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptXOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptYOffset			
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptYOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptXSize
            if (OS2.ySuperscriptXSize==0)
            {
                OS2.ySuperscriptXSize = (Int16)(Head.unitsPerEm/10);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptXSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptYSize
            if (OS2.ySuperscriptYSize==0)
            {
                OS2.ySuperscriptYSize = (Int16)(Head.unitsPerEm/10);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptYSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptXOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptXOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptYOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptYOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //yStrikeoutSize
            if (OS2.yStrikeoutSize==0)
            {
               OS2.yStrikeoutSize = (Int16)(Head.unitsPerEm/20);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.yStrikeoutSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //yStrikeoutPosition
            if (OS2.yStrikeoutPosition==0)
            {
                OS2.yStrikeoutPosition = (Int16)(Head.unitsPerEm/5);
            }
            usTmp = hy_cdr_int16_to((UInt16)OS2.yStrikeoutPosition);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sFamilyClass
            usTmp = hy_cdr_int16_to((UInt16)OS2.sFamilyClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //Panose            
            FileStrm.WriteByte(OS2.panose.FamilyType);
            FileStrm.WriteByte(OS2.panose.SerifStyle);
            FileStrm.WriteByte(OS2.panose.Weight);
            FileStrm.WriteByte(OS2.panose.Proportion);
            FileStrm.WriteByte(OS2.panose.Contrast);
            FileStrm.WriteByte(OS2.panose.StrokeVariation);
            FileStrm.WriteByte(OS2.panose.ArmStyle);
            FileStrm.WriteByte(OS2.panose.Letterform);
            FileStrm.WriteByte(OS2.panose.Midline);
            FileStrm.WriteByte(OS2.panose.XHeight);

            CountUnicodeRange(ref OS2.ulUnicodeRange1, ref OS2.ulUnicodeRange2, ref OS2.ulUnicodeRange3, ref OS2.ulUnicodeRange4,lstCodeMap);
            
			//ulUnicodeRange1 
			ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange1);
			btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//ulUnicodeRange2 	
			ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange2);
			btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//ulUnicodeRange3 	
			ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange3);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);		
			//ulUnicodeRange4
			ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange4);
			btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//achVendID
            FileStrm.WriteByte(OS2.vtachVendID[0]);
            FileStrm.WriteByte(OS2.vtachVendID[1]);
            FileStrm.WriteByte(OS2.vtachVendID[2]);
            FileStrm.WriteByte(OS2.vtachVendID[3]);						
			//fsSelection
			usTmp = hy_cdr_int16_to(OS2.fsSelection);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usFirstCharIndex
            if (lstCodeMap.Count>1)
            {
                UInt32 uluni = lstCodeMap[0].Unicode;
                if (uluni>0xffff)
				{
					OS2.usFirstCharIndex = 0xffff;
				}
				else 
				{
                    OS2.usFirstCharIndex = (UInt16)uluni;
                }
              
				usTmp = hy_cdr_int16_to(OS2.usFirstCharIndex);
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);     
            }
            else 
            {
                usTmp = 0x0000;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
            }
			//usLastCharIndex
			if (lstCodeMap.Count>1)
			{
				UInt32 uluni = lstCodeMap[lstCodeMap.Count-2].Unicode;			
				if (uluni>0xffff)
				{
					OS2.usLastCharIndex = 0xffff;
				}
				else 
				{
					OS2.usLastCharIndex = (UInt16)uluni;
				}
				usTmp = hy_cdr_int16_to(OS2.usLastCharIndex);
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
			}
			else 
			{
                usTmp = 0x0000;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
			}
			//sTypoAscender
			if (OS2.sTypoAscender==0)
			{
				OS2.sTypoAscender = Head.yMax;
			}			
			usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoAscender);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//sTypoDescender
			if (OS2.sTypoDescender==0)
			{			
				OS2.sTypoDescender = Head.yMin;
			}			
			usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoDescender);			
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//sTypoLineGap			
			usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoLineGap);			
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usWinAscent
			if (OS2.usWinAscent==0)
			{
				OS2.usWinAscent = (UInt16)Math.Abs(Head.yMax);
			}			
			usTmp = hy_cdr_int16_to(OS2.usWinAscent);			
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usWinDescent
			if (OS2.usWinDescent==0)
			{
				OS2.usWinDescent =  (UInt16)Math.Abs(Head.yMin);
			}			
			usTmp = hy_cdr_int16_to(OS2.usWinDescent);			
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//ulCodePageRange1 
			ulTmp = hy_cdr_int32_to(OS2.ulCodePageRange1);
			btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//ulCodePageRange2 
			ulTmp = hy_cdr_int32_to(OS2.ulCodePageRange2);
			btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//sxHeight
			int iGID=-1;            
			if (OS2.sxHeight==0)
			{
				iGID = FindGryphIndexByUnciodeEx(0x0078,lstCodeMap);				
				if (iGID!=-1)
				{
                    int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                    BoundStringToInt(GlyphChars.CharInfo[iGID].Section,out xmin,out ymin,out xmax,out ymax);
                    OS2.sxHeight = (short)ymax;
				}				
			}		
			usTmp = hy_cdr_int16_to((UInt16)OS2.sxHeight);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//sCapHeight			
			if (OS2.sCapHeight==0)
			{
				iGID = FindGryphIndexByUnciodeEx(0x0048,lstCodeMap);
				if (iGID!=-1)
				{
                    int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                    BoundStringToInt(GlyphChars.CharInfo[iGID].Section, out xmin, out ymin, out xmax, out ymax);
					OS2.sCapHeight = (short)ymax;
				}
			}
			usTmp = hy_cdr_int16_to((UInt16)OS2.sCapHeight);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usDefaultChar		
			usTmp = 0;
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usBreakChar			
            usTmp = 0x2000;
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//usMaxContext

            if ((TableFlag & GSUB_FLG) > 0)
            {
                usTmp = 2;
            }
            else
            {
                usTmp = 0;
            }

            usTmp = hy_cdr_int16_to(usTmp);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeOS2()

        // 不做跨表计算,减少关联
        public HYRESULT EncodeOS2Ex()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.OS2_TAG);
            if (iEntryIndex == -1) return HYRESULT.OS2_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp = 0;
            UInt32 ulTmp = 0;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to(OS2.version);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //xAvgCharWidth            
            usTmp = hy_cdr_int16_to((ushort)OS2.xAvgCharWidth);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWeightClass			            
            usTmp = hy_cdr_int16_to(OS2.usWeightClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWidthClass            
            usTmp = hy_cdr_int16_to(OS2.usWidthClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //fsType            
            usTmp = hy_cdr_int16_to((UInt16)OS2.fsType);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptXSize            
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptXSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptYSize            
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptYSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptXOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptXOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySubscriptYOffset			
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySubscriptYOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptXSize            
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptXSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptYSize            
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptYSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptXOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptXOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ySuperscriptYOffset
            usTmp = hy_cdr_int16_to((UInt16)OS2.ySuperscriptYOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //yStrikeoutSize            
            usTmp = hy_cdr_int16_to((UInt16)OS2.yStrikeoutSize);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //yStrikeoutPosition            
            usTmp = hy_cdr_int16_to((UInt16)OS2.yStrikeoutPosition);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sFamilyClass
            usTmp = hy_cdr_int16_to((UInt16)OS2.sFamilyClass);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //Panose            
            FileStrm.WriteByte(OS2.panose.FamilyType);
            FileStrm.WriteByte(OS2.panose.SerifStyle);
            FileStrm.WriteByte(OS2.panose.Weight);
            FileStrm.WriteByte(OS2.panose.Proportion);
            FileStrm.WriteByte(OS2.panose.Contrast);
            FileStrm.WriteByte(OS2.panose.StrokeVariation);
            FileStrm.WriteByte(OS2.panose.ArmStyle);
            FileStrm.WriteByte(OS2.panose.Letterform);
            FileStrm.WriteByte(OS2.panose.Midline);
            FileStrm.WriteByte(OS2.panose.XHeight);

            //ulUnicodeRange1 
            ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange1);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ulUnicodeRange2 	
            ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange2);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ulUnicodeRange3 	
            ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange3);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ulUnicodeRange4
            ulTmp = hy_cdr_int32_to(OS2.ulUnicodeRange4);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //achVendID
            FileStrm.WriteByte(OS2.vtachVendID[0]);
            FileStrm.WriteByte(OS2.vtachVendID[1]);
            FileStrm.WriteByte(OS2.vtachVendID[2]);
            FileStrm.WriteByte(OS2.vtachVendID[3]);

            //fsSelection
            usTmp = hy_cdr_int16_to(OS2.fsSelection);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usFirstCharIndex
            usTmp = hy_cdr_int16_to(OS2.usFirstCharIndex);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            
            //usLastCharIndex
            usTmp = hy_cdr_int16_to(OS2.usLastCharIndex);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            
            //sTypoAscender
            usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoAscender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sTypoDescender            
            usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoDescender);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sTypoLineGap			
            usTmp = hy_cdr_int16_to((UInt16)OS2.sTypoLineGap);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWinAscent            
            usTmp = hy_cdr_int16_to(OS2.usWinAscent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usWinDescent            
            usTmp = hy_cdr_int16_to(OS2.usWinDescent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ulCodePageRange1 
            ulTmp = hy_cdr_int32_to(OS2.ulCodePageRange1);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //ulCodePageRange2 
            ulTmp = hy_cdr_int32_to(OS2.ulCodePageRange2);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sxHeight            
            usTmp = hy_cdr_int16_to((UInt16)OS2.sxHeight);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //sCapHeight			            
            usTmp = hy_cdr_int16_to((UInt16)OS2.sCapHeight);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usDefaultChar		
            usTmp = hy_cdr_int16_to((UInt16)OS2.usDefaultChar);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usBreakChar			            
            usTmp = hy_cdr_int16_to((UInt16)OS2.usBreakChar);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //usMaxContext            
            usTmp = hy_cdr_int16_to((UInt16)OS2.usMaxContext);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        } // end of public HYRESULT EncodeOS2Ex()

        /// <summary>
        /// 构建Post表
        /// </summary>
        /// <param name="fixVer">版本号</param>
        /// <param name="italic">角度</param>
        /// <param name="underlinePosition">下划线位置</param>
        /// <param name="underlineThickness">下划线粗细</param>
        /// <param name="isFixedPitch">是否是等宽字体</param>
        /// <param name="lstPostName">postName集合，必须与glyph一一对应，包括miss char</param>
        /// <returns>HYRESULT</returns>
        public HYRESULT MakePost()
        {
            Post = new CPost();

            if (FntType == FONTTYPE.CFF)
            {
                Post.version.value = 3;
                Post.version.fract = 0;
            }
            else if (Post.version.value == 0)
            {
                Post.version.value = 2;
                Post.version.fract = 0;    
            }

            if (Post.underlinePosition == 0)
            {
                Post.underlinePosition = Head.yMin;
            }

            if (Post.underlineThickness == 0)
            {
                Post.underlineThickness = (short)(Head.unitsPerEm/10);
            }

            Post.isFixedPitch = 0;

            Post.PostFormat2.usNumberOfGlyphs = (ushort)GlyphChars.CharInfo.Count;
            for (int i = 0; i < Post.PostFormat2.usNumberOfGlyphs; i++)
			{
                ushort usPostNameID = (ushort)Post.InsertName(GlyphChars.CharInfo[i].Name);
                Post.PostFormat2.lstGlyphNameIndex.Add(usPostNameID);
			}

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT MakePost()

        public HYRESULT Encodepost()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.POST_TAG);
            if (iEntryIndex == -1) return HYRESULT.POST_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp = 0;
            UInt32 ulTmp = 0;
            byte[] btTmp;

            //version
            usTmp = hy_cdr_int16_to((ushort)Post.version.value);            
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = 0;
            //usTmp = hy_cdr_int16_to(Post.Format.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            // italicAngle
            usTmp = hy_cdr_int16_to((UInt16)Post.italicAngle.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
            usTmp = hy_cdr_int16_to(Post.italicAngle.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //underlinePosition
            if (Post.underlinePosition == 0)
            {
                Post.underlinePosition = Head.yMin;
            }
            usTmp = hy_cdr_int16_to((UInt16)Post.underlinePosition);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //UnderlineThickness
            if (Post.underlineThickness == 0)
            {
                Post.underlineThickness = (Int16)(Head.unitsPerEm/20);
            }
            usTmp = hy_cdr_int16_to((UInt16)Post.underlineThickness);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //isFixedPitch
            ulTmp = hy_cdr_int16_to((UInt16)Post.isFixedPitch);
            btTmp = BitConverter.GetBytes(ulTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            ulTmp = 0;
            btTmp = BitConverter.GetBytes(ulTmp);
            //minMemType42
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //maxMemType42
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //minMemType1
            FileStrm.Write(btTmp, 0, btTmp.Length);
            //maxMemType1
            FileStrm.Write(btTmp, 0, btTmp.Length);

            if (Post.version.value == 2 && Post.version.fract == 0)
			{
				// numberOfGlyphs
				usTmp = hy_cdr_int16_to(Post.PostFormat2.usNumberOfGlyphs);				                
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

				//glyphNameIndex
				for (int i = 0; i <Post.PostFormat2.usNumberOfGlyphs; i++)
				{
                    usTmp = hy_cdr_int16_to(Post.PostFormat2.lstGlyphNameIndex[i]);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
				}
				//names
				int st = Post.lstStandString.Count;
				for (int i = 258; i <st; i++)
				{
                    byte[] szString = System.Text.Encoding.Default.GetBytes(Post.lstStandString[i]);
                    byte strlen = (byte)szString.Length;

                    FileStrm.WriteByte(strlen);
                    FileStrm.Write(szString, 0, strlen);
				}
			}

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodepost()

        public HYRESULT EncodeCFF()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.CFF_TAG);
            if (iEntryIndex == -1) return HYRESULT.CFF_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            ulong ulTableBegin = tbEntry.offset;
            PrepareCFFInfo();

            List<byte> vtHead = new List<byte>();
            List<byte> vtName = new List<byte>();
            List<byte> vtTopDict = new List<byte>();
            List<byte> vtString = new List<byte>();
            List<byte> vtGolbIndex = new List<byte>();
            List<byte> vtCharset = new List<byte>();
            List<byte> vtFDSelect = new List<byte>();
            List<byte> vtFArrayData = new List<byte>();
            List<byte> vtPrivateData = new List<byte>();
            List<byte> vtCharStrings = new List<byte>();

            // string 
            EncodeStringIndex(ref vtString);

            EncodeCFFHead(ref vtHead);
            EncodeNameIndex(ref vtName);
            EncodeTopDictIndex(ref vtTopDict);
            EncodeGlobalIndex(ref vtGolbIndex);
            EncodeCharSets(ref vtCharset);
            EncodeCharStrings(ref vtCharStrings);	

            long lOffset = 0;
			lOffset = vtHead.Count+vtName.Count+vtTopDict.Count+vtString.Count+vtGolbIndex.Count;
			// charset
			CFFInfo.TopDICT.charsetOffset = lOffset;		
			lOffset += vtCharset.Count;

			if (CFFInfo.TopDICT.IsCIDFont==1)
			{
				// fdselect				
				CFFInfo.TopDICT.FDSelectOffset = lOffset;
				EncodeFDSelect(ref vtFDSelect);
				lOffset+=vtFDSelect.Count;
			}

			CFFInfo.TopDICT.charStringOffset = lOffset;
			lOffset += vtCharStrings.Count;

			if (CFFInfo.TopDICT.IsCIDFont==1)
			{
				//fdarray
				CFFInfo.TopDICT.FDArryIndexOffset= lOffset;				
				EncodeFDArray(ref vtFArrayData,lOffset);
				lOffset+=vtFArrayData.Count;
			}		
			else
			{
				//private Dict
				CFFInfo.TopDICT.PrivteOffset=lOffset;
				EncodePrivateDICTData(ref vtPrivateData,ref CFFInfo.PrivteDict);
				CFFInfo.TopDICT.PrivteDictSize=vtPrivateData.Count;
				lOffset+= vtPrivateData.Count;
			}
			
			vtTopDict = new List<byte>();
			EncodeTopDictIndex(ref vtTopDict);
			
			//write file
			// Head
            byte[] btHead = vtHead.ToArray();
            FileStrm.Write(btHead, 0, btHead.Length);
			
			// Name
            byte[] btName = vtName.ToArray();
            FileStrm.Write(btName, 0, btName.Length);

			// TopDict
            byte[] btTopDict = vtTopDict.ToArray();
            FileStrm.Write(btTopDict, 0, btTopDict.Length);

			// string
            byte[] btString = vtString.ToArray();
            FileStrm.Write(btString, 0, btString.Length);

			// Global
            byte[] btGolbIndex = vtGolbIndex.ToArray();
            FileStrm.Write(btGolbIndex, 0, btGolbIndex.Length);
			
			// charset
            byte[] btCharset = vtCharset.ToArray();
            FileStrm.Write(btCharset, 0, btCharset.Length);

			if (CFFInfo.TopDICT.IsCIDFont==1)
			{
				// FDSelect
                byte[] btFDSelect = vtFDSelect.ToArray();
                FileStrm.Write(btFDSelect, 0, btFDSelect.Length);
			}

            byte[] btCharStrings = vtCharStrings.ToArray();
            FileStrm.Write(btCharStrings, 0, btCharStrings.Length);

            if (CFFInfo.TopDICT.IsCIDFont == 1)
            {
                //fdarray			
                byte[] btFArrayData = vtFArrayData.ToArray();
                FileStrm.Write(btFArrayData, 0, btFArrayData.Length);
            }
            else
            {
                //private Dict
                byte[] btPrivateData = vtPrivateData.ToArray();
                FileStrm.Write(btPrivateData, 0, btPrivateData.Length);
            }

            tbEntry.length = (uint)(FileStrm.Position - tbEntry.offset);
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeCFF()
        
        protected void	PrepareCFFInfo()
        {
            //CFFInfo = new CCFFInfo();
            // top dict data
            for (int i = 0; i < Name.vtNameRecord.Count; i++)
            {
                NAMERECORD tagNameRecord = Name.vtNameRecord[i];
                // notice
                if (tagNameRecord.platformID == 1 && tagNameRecord.encodingID == 0 && tagNameRecord.nameID == 0)
                {
                    CFFInfo.TopDICT.strNotice = tagNameRecord.strContent;
                }
                // FullName
                if (tagNameRecord.platformID == 1 && tagNameRecord.encodingID == 0 && tagNameRecord.nameID == 4)
                {
                    CFFInfo.TopDICT.strFullName = tagNameRecord.strContent;
                }
                // FamilyName
                if (tagNameRecord.platformID == 1 && tagNameRecord.encodingID == 0 && tagNameRecord.nameID == 1)
                {
                    CFFInfo.TopDICT.strFamilyName = tagNameRecord.strContent;
                }
                // CIDFontName
                if (tagNameRecord.platformID == 1 && tagNameRecord.encodingID == 0 && tagNameRecord.nameID == 6)
                {
                    CFFInfo.TopDICT.strCIDFontName = tagNameRecord.strContent;
                }
                // weight
                if (tagNameRecord.platformID == 1 && tagNameRecord.encodingID == 0 && tagNameRecord.nameID == 2)
                {
                    CFFInfo.TopDICT.strWeight = tagNameRecord.strContent;
                }
            }
            /*
            DateTime dateTime = System.DateTime.Now;
            CFFInfo.TopDICT.vtXUID.Add(126);
            CFFInfo.TopDICT.vtXUID.Add(dateTime.Year);
            CFFInfo.TopDICT.vtXUID.Add((int)dateTime.Ticks);
            */

            CFFInfo.TopDICT.vtFontBOX.Add(Head.xMin);
            CFFInfo.TopDICT.vtFontBOX.Add(Head.yMin);
            CFFInfo.TopDICT.vtFontBOX.Add(Head.xMax);
            CFFInfo.TopDICT.vtFontBOX.Add(Head.yMax);

            CCFFPrivteDict privteDict = new CCFFPrivteDict();
            privteDict.vtBlueValues.Add(-250);
            privteDict.vtBlueValues.Add(0);
            privteDict.vtBlueValues.Add(1350);
            privteDict.vtBlueValues.Add(0);
            privteDict.fBlueScale = 0.039625;
            privteDict.fStdHW = 25;
            privteDict.fStdVW = 66;
            privteDict.ldefaultWidthX = 1000;
            privteDict.lnominalWidthX = 607;
            if (CFFInfo.TopDICT.IsCIDFont == 1)
            {
                CFFInfo.TopDICT.CIDCount = GlyphChars.CharInfo.Count;
                CFFInfo.TopDICT.UIDBase = CFFInfo.TopDICT.vtXUID[2];
                CFFInfo.TopDICT.CIDFontVersion = 1.0;
                //FDSelect
                CFFInfo.FDSelect.iFormat = 3;
                CCFFFDSRang3 Rang3 = new CCFFFDSRang3();
                Rang3.first = 0;
                Rang3.fdIndex = 0;
                CFFInfo.FDSelect.format3.vtRang3.Add(Rang3);
                CFFInfo.FDSelect.format3.sentinel = (UInt16)GlyphChars.CharInfo.Count;
                //FDArray			
                privteDict.strFontName = CFFInfo.TopDICT.strCIDFontName;
                CFFInfo.vtFDArry.Add(privteDict);
            }
            else
            {
                CFFInfo.PrivteDict = privteDict;            
            }

            //charset            
            CFFInfo.Charset.format = 2;
            CCFFCSFormatRang2 formatRang2 = new CCFFCSFormatRang2();
            if (CFFInfo.TopDICT.IsCIDFont==1)
                formatRang2.first = 1;
            else
                formatRang2.first = 391;
            //-2 是因为第一位 （0位是.notdef的位置，不能计入Charset中，另外，nleft 并不包括first
            formatRang2.left = (UInt16)(GlyphChars.CharInfo.Count - 2);
            CFFInfo.Charset.format2.vtRang.Add(formatRang2);
        
        }   // end of protected void PrepareCFFInfo

        protected HYRESULT EncodeCFFHead(ref List<byte> vtHead)
        {
            //major
		    CFFInfo.Header.major = (byte)Head.fontRevision.value;
		    vtHead.Add((byte)Head.fontRevision.value);
		    //minor
            CFFInfo.Header.minor = (byte)Head.fontRevision.fract;
            vtHead.Add((byte)CFFInfo.Header.minor);
		    //hdrSize
		    CFFInfo.Header.hdrSize= 4;
            vtHead.Add((byte)CFFInfo.Header.hdrSize);
		    //offSize
		    CFFInfo.Header.offSize= 4;
            vtHead.Add((byte)CFFInfo.Header.offSize);           

            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeCFFHead()
       
		protected HYRESULT	EncodeNameIndex(ref List<byte> vtName)
        {
            List<byte> vtNm = new List<byte>();
            List<int>	vtOffset = new List<int>();
		
            for (int i=0; i<Name.vtNameRecord.Count; i++)			
            {
                NAMERECORD tagNamRecord = Name.vtNameRecord[i];
                if (tagNamRecord.platformID==1 && tagNamRecord.encodingID==0 && tagNamRecord.nameID==6)
                {
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(tagNamRecord.strContent);
                    for (int j = 0; j < byteArray.Length; j++)
                    {
                        vtNm.Add(byteArray[j]);
                    }

                    vtOffset.Add(1);
                    vtOffset.Add(byteArray.Length+1);
                    break;
                }
            }

            EncodeIndexData(ref vtName,ref vtNm, ref vtOffset);

            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeNameIndex()

        protected int PutStringToListByte(ref List<byte> vtbyte, string str)
        {
            if (str == null) return 0;

            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
            for (int j = 0; j < byteArray.Length; j++)
            {
                vtbyte.Add(byteArray[j]);
            }

            return byteArray.Length;        
        
        }   // end of int PutStringToListByte()

		protected HYRESULT	EncodeStringIndex(ref List<byte> vtString)
        {
            List<byte>			vtStringTmp = new List<byte>();
            List<int>	        vtOffset = new List<int>();
            int				    iStrSize = 0;

            //vtOffset.Add(1);
            if (CFFInfo.TopDICT.IsCIDFont == 1)
            {
                //Registry
                if (!string.IsNullOrEmpty(CFFInfo.TopDICT.Ros.strRegistry))
                {
                    CFFInfo.TopDICT.Ros.RegistrySID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.Ros.strRegistry);
                }
                
                //Ordering
                if (!string.IsNullOrEmpty(CFFInfo.TopDICT.Ros.strOrdering))
                {
                    CFFInfo.TopDICT.Ros.OrderingSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.Ros.strOrdering);
                }
                
            }
            else
            {             
			    for (ushort i=1; i<Maxp.numGlyphs; i++)
			    {
                    string strCID;
                    strCID = "CID" + i.ToString();
                    CFFInfo.stnStrings.InsertString(strCID);
			    }
            }

            //version	
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strVersion))
            {
                CFFInfo.TopDICT.iVersionSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strVersion);
            }            

            // Notice
            if (CFFInfo.TopDICT.strNotice != "")
            {
                CFFInfo.TopDICT.iNoticeSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strNotice);
            }
            
            //copyright
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strCopyright))
            {
                CFFInfo.TopDICT.iCopyrghtSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strCopyright);
            }
            
            //FullName
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strFullName))
            {
                CFFInfo.TopDICT.iFullNameSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strFullName);
            }
            
            //FamilyName
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strFamilyName))
            {
                CFFInfo.TopDICT.iFamilyNameSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strFamilyName);
            }
            
            //weight
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strWeight))
            {
                CFFInfo.TopDICT.iWeightSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strWeight);
            }
            
            //postscript
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strPostSript))
            {
                CFFInfo.TopDICT.PostSriptSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strPostSript);
            }
            
            // BaseFontName
            if (!string.IsNullOrEmpty(CFFInfo.TopDICT.strBaseFontName))
            {
                CFFInfo.TopDICT.BaseFontNameSID = CFFInfo.stnStrings.InsertString(CFFInfo.TopDICT.strBaseFontName);
            }

            //FDARRAY
            for (int i = 0; i < CFFInfo.vtFDArry.Count; i++)
            {       
                CFFInfo.vtFDArry[i].iFontNameID = CFFInfo.stnStrings.InsertString(CFFInfo.vtFDArry[i].strFontName);
            }

            vtOffset.Add(1);
            for (int i = 391; i < CFFInfo.stnStrings.szStandString.Count; i++)
            {
                iStrSize = PutStringToListByte(ref vtStringTmp, CFFInfo.stnStrings.szStandString[i]);                
                if (iStrSize > 0)
                {
                    vtOffset.Add(vtStringTmp.Count + 1);
                }
            }

            EncodeIndexData(ref vtString, ref vtStringTmp, ref vtOffset);

            return HYRESULT.NOERROR;

        }   // end of protected int	EncodeStringIndex()

		protected HYRESULT	EncodeGlobalIndex(ref List<byte>  vtGlobal)
        {
            byte cTmp = 0;
		    CCFFIndex Global = CFFInfo.globalSubIndex;

		    //count
		    vtGlobal.Add((byte)(Global.Count>>8));
		    vtGlobal.Add((byte)(Global.Count));
		    if (Global.Count>0)
		    {
			    //Offsize
			    vtGlobal.Add(Global.Offsize);
			    int st = Global.vtOffset.Count;
			    for (int i=0; i<st; i++)
			    {
				    ulong ulTmp = Global.vtOffset[i];
				    if(Global.Offsize==1)
				    {
					    cTmp  = (byte)(ulTmp&0x000000ff);
					    vtGlobal.Add(cTmp);
				    }
				    if (Global.Offsize==2)
				    {
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)(ulTmp&0x000000ff);				
					    vtGlobal.Add(cTmp);
				    }
				    if (Global.Offsize==3)
				    {
					    cTmp = (byte)((ulTmp&0x00ff0000)>>16);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)(ulTmp&0x000000ff);				
					    vtGlobal.Add(cTmp);
				    }
				    if (Global.Offsize==4)
				    {
					    cTmp = (byte)(ulTmp>>24);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x00ff0000)>>16);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);
					    vtGlobal.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x000000ff));
					    vtGlobal.Add(cTmp);
				    }
			    }
			    int szInData = Global.vtData.Count;
			    for (int i=0; i<szInData; i++)
			    {
				    vtGlobal.Add(Global.vtData[i]);
			    }
		    }
        
            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeGlobalIndex()

		protected HYRESULT	EncodeTopDictIndex(ref List<byte>  vtTopDict)
        {
            List<byte> vtTmpData = new List<byte>();
		    CCFFTopDict TopDict = CFFInfo.TopDICT;
		    List<int>	vtOffset = new List<int>();
		    vtOffset.Add(1);

            if (TopDict.IsCIDFont == 1)
            {
                //ROS
                EncodeDICTInteger((long)TopDict.Ros.RegistrySID, ref vtTmpData);
                EncodeDICTInteger((long)TopDict.Ros.OrderingSID, ref vtTmpData);
                EncodeDICTInteger((long)TopDict.Ros.Supplement, ref vtTmpData);
                //CFF_DICT_OPERATOR_ROS
                vtTmpData.Add((byte)0x0c);
                vtTmpData.Add((byte)0x1e);
            }
            //version
            if (TopDict.iVersionSID!=0)
            {
                //CFF_DICT_OPERATOR_VERSION
	            EncodeDICTInteger((long)TopDict.iVersionSID,ref vtTmpData);
	            vtTmpData.Add((byte)0x00);
            }
            //Notice
            if (TopDict.iNoticeSID!=0)
            {
                //CFF_DICT_OPERATOR_NOTICE
                EncodeDICTInteger((long)TopDict.iNoticeSID,ref vtTmpData);
                vtTmpData.Add((byte)(0x01));
            }
            //Copyright
            if (TopDict.iCopyrghtSID!=0)
            {
	            EncodeDICTInteger((long)TopDict.iCopyrghtSID,ref vtTmpData);
                //CFF_DICT_OPERATOR_COPYRIGHT
	            vtTmpData.Add((byte)0xcc);
	            vtTmpData.Add((byte)0x00);
            }
            //FullName
		    if (TopDict.iFullNameSID!=0)
		    {
			    EncodeDICTInteger((long)TopDict.iFullNameSID,ref vtTmpData);			
                //CFF_DICT_OPERATOR_FULLNAME
			    vtTmpData.Add((byte)0x02);
		    }
            //FamilyName
		    if (TopDict.iFamilyNameSID!=0)
		    {
			    EncodeDICTInteger((long)TopDict.iFamilyNameSID,ref vtTmpData);			
                //CFF_DICT_OPERATOR_FAMILYNAME
			    vtTmpData.Add((byte)(0x03));
		    }
            //Weight	
		    EncodeDICTInteger((long)TopDict.iWeightSID, ref vtTmpData);
		    vtTmpData.Add((byte)0x04);
            //isFixedPitch		
		    if (TopDict.isFixedPitch!=0)
		    {
			    EncodeDICTInteger((long)TopDict.isFixedPitch,ref vtTmpData);
                //CFF_DICT_OPERATOR_ISFIXEDPITCH
			    vtTmpData.Add((byte)0x0c);	
			    vtTmpData.Add((byte)0x01);
		    }
            //ItalicAngle		
		    if (TopDict.ItalicAngle!=0)
		    {
			    EncodeDICTInteger((long)TopDict.ItalicAngle,ref vtTmpData);
                //CFF_DICT_OPERATOR_ITALICANGLE
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x02);
		    }
            //UnderlinePosition
		    if (TopDict.UnderlinePosition != (double)-100.0)
		    {
			    EncodeDICTInteger((long)TopDict.UnderlinePosition,ref vtTmpData);
                //CFF_DICT_OPERATOR_UNDERLINEPOSITION
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x03);
		    }
            //UnderlineThickness
		    if (TopDict.UnderlineThickness != 50.0)
		    {	
			    EncodeDICTInteger((long)(TopDict.UnderlineThickness),ref vtTmpData);
                //CFF_DICT_OPERATOR_UNDERLINETHICKNESS
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x04);
		    }	
            //PaintType
		    if (TopDict.PaintType!=0)
		    {
			    EncodeDICTInteger((long)TopDict.PaintType,ref vtTmpData);
                //CFF_DICT_OPERATOR_PAINTTYPE
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x05);
		    }
            // charstringtype
		    if (TopDict.CharStringType != 2)
		    {
			    EncodeDICTInteger((long)TopDict.CharStringType,ref vtTmpData);
                //CFF_DICT_OPERATOR_CHARSTRINGTYPE
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x06);
		    }
            //FontMatrix
		    int st = TopDict.vtFontMatrix.Count;
		    if (st>0)
		    {
			    for (int i=0; i<st; i++)
			    {
				    EncodeDICTReal(TopDict.vtFontMatrix[i],ref vtTmpData);
			    }			
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x07);	
		    }
            //UniqueID
		    if(TopDict.UniqueID!=-1)
		    {
			    EncodeDICTInteger((long)TopDict.UniqueID,ref vtTmpData);
			    vtTmpData.Add((byte)0x0d);
		    }
            //FontBBox
		    st = TopDict.vtFontBOX.Count; 
		    if (st>0)
		    {
			    for (int i=0; i<st; i++)
			    {
				    EncodeDICTInteger((long)TopDict.vtFontBOX[i],ref vtTmpData);
			    }			
			    vtTmpData.Add((byte)0x05);		
		    }
            //StrokeWidth
		    if (TopDict.strokeWidth>0)
		    {
			    EncodeDICTInteger((long)TopDict.strokeWidth,ref vtTmpData);
			    vtTmpData.Add((byte)0x0c);		
			    vtTmpData.Add((byte)0x08);
		    }

            //XUID
		    st=TopDict.vtXUID.Count;
		    if (st>0)
		    {
			    for (int i=0;i<st;i++)
			    {
				    EncodeDICTInteger((long)TopDict.vtXUID[i],ref vtTmpData);
			    }		
				//CFF_DICT_OPERATOR_ENCODING
			    vtTmpData.Add((byte)0x0e);
		    }
            //charset
		    EncodeDICTMaxInteger((long)TopDict.charsetOffset,ref vtTmpData);
		    vtTmpData.Add((byte)0x0f);	

            //CharStrings		
		    EncodeDICTMaxInteger((long)TopDict.charStringOffset,ref vtTmpData);
		    vtTmpData.Add((byte)0x11);

            if (TopDict.IsCIDFont==0)
		    {
			    EncodeDICTMaxInteger(TopDict.PrivteDictSize,ref vtTmpData);	
			    EncodeDICTMaxInteger(TopDict.PrivteOffset,ref vtTmpData);
                //CFF_DICT_OPERATOR_PRIVATE
			    vtTmpData.Add((byte)0x12);
		    }
            //SyntheticBase
		    if (TopDict.SyntheticBaseIndex!=0)
		    {
			    EncodeDICTInteger(TopDict.SyntheticBaseIndex,ref vtTmpData);
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x14);
		    }
            //PostScript
		    if (TopDict.PostSriptSID!=0)
		    {
			    EncodeDICTInteger((long)TopDict.PostSriptSID,ref vtTmpData);
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x15);
		    }
            //BaseFontName
		    if (TopDict.BaseFontNameSID!=0)
		    {
			    EncodeDICTInteger((long)TopDict.PostSriptSID,ref vtTmpData);
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x16);
		    }	
            //BaseFontBlend	
		    st=TopDict.vtBaseFontBlend.Count;
		    if (st>0)
		    {
			    for(int i=0; i<st; i++)
			    {
				    EncodeDICTReal(TopDict.vtBaseFontBlend[i],ref vtTmpData);
			    }
			    vtTmpData.Add((byte)0x0c);
			    vtTmpData.Add((byte)0x17);
		    }

            if (TopDict.IsCIDFont==1)
		    {
			    //ROS
    //			EncodeDICTInteger(TopDict.Ros.RegistrySID,vtTmpData);
    //			EncodeDICTInteger(TopDict.Ros.OrderingSID,vtTmpData);
    //			EncodeDICTInteger(TopDict.Ros.Supplement,vtTmpData);
    //			vtTmpData.Add(byte(CFF_DICT_OPERATOR_ROS>>8));
    //			vtTmpData.Add(byte(CFF_DICT_OPERATOR_ROS));
			    //CIDFontVersion
			    if (TopDict.CIDFontVersion!=0)
			    {
				    EncodeDICTReal(TopDict.CIDFontVersion,ref vtTmpData);
				    vtTmpData.Add((byte)0x0c);
				    vtTmpData.Add((byte)0x1f);
			    }
			    //CIDFontVersion
			    if (TopDict.CIDFontRevision!=0)
			    {
				    EncodeDICTInteger((long)TopDict.CIDFontRevision,ref vtTmpData);
				    vtTmpData.Add((byte) 0x0c);
				    vtTmpData.Add((byte) 0x20);
			    }
			    //CIDFontType
			    if (TopDict.CIDFontType!=0)
			    {
				    EncodeDICTInteger((long)TopDict.CIDFontType,ref vtTmpData);
				    vtTmpData.Add((byte)0x0c);
				    vtTmpData.Add((byte)0x21);
			    }
			    //CIDCount
			    if (TopDict.CIDCount!=8720)
			    {
				    EncodeDICTInteger(TopDict.CIDCount,ref vtTmpData);
				    vtTmpData.Add((byte)0x0c);
				    vtTmpData.Add((byte)0x22);
			    }

			    EncodeDICTMaxInteger(TopDict.FDArryIndexOffset,ref vtTmpData);
			    vtTmpData.Add((byte) 0x0c);
			    vtTmpData.Add((byte) 0x24);
			
			    EncodeDICTMaxInteger(TopDict.FDSelectOffset,ref vtTmpData);
			    vtTmpData.Add((byte) 0x0c);
			    vtTmpData.Add((byte) 0x25);

			    //FontName
			    if (TopDict.CIDFontNameSID!=0)
			    {
				    EncodeDICTInteger((long)TopDict.CIDFontNameSID,ref vtTmpData);
				    vtTmpData.Add((byte)0x0c);
				    vtTmpData.Add((byte)0x26);
			    }
		    }
            vtOffset.Add(vtTmpData.Count+1);
		    EncodeIndexData( ref vtTopDict,ref vtTmpData,ref vtOffset);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT	EncodeTopDictIndex()

		protected HYRESULT	EncodeFDSelect(ref List<byte> vtData)
        {
            CCFFFDSelect fdSelect = CFFInfo.FDSelect;
		
		    // format
		    vtData.Add((byte)(fdSelect.iFormat));
		    if (fdSelect.iFormat == 0)
		    {
			    int szFormat0 = fdSelect.format0.Count;
			    for (int i=0; i<szFormat0 ; i++)
			    {
				    vtData.Add(fdSelect.format0[i]);
			    }
		    }

		    if (fdSelect.iFormat == 3)
		    {
			    // nRanges
			    ushort stRang = (ushort)fdSelect.format3.vtRang3.Count;
			    vtData.Add((byte)(stRang>>8));
			    vtData.Add((byte)(stRang));

			    // Rang3
			    for (ushort i=0; i<stRang; i++)
			    {
				    vtData.Add((byte)(fdSelect.format3.vtRang3[i].first>>8));
				    vtData.Add((byte)(fdSelect.format3.vtRang3[i].first));

				    vtData.Add(fdSelect.format3.vtRang3[i].fdIndex);
			    }

			    //sentinel
			    vtData.Add((byte)(fdSelect.format3.sentinel>>8));
			    vtData.Add((byte)(fdSelect.format3.sentinel));
		    }


            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeFDSelect()

		protected HYRESULT	EncodeFDArray(ref List<byte> vtFDArrayData,long stOffset)
        {
            int stPrivateDICT = CFFInfo.vtFDArry.Count;
		    List<int>		vtOffset = new List<int>();
		    List<byte>		vtTmpData = new List<byte>();
		
		    vtOffset.Add(0);		
		    for (int i=0; i<stPrivateDICT; i++)
		    {
			    CCFFPrivteDict cffPrivteDict = CFFInfo.vtFDArry[i];
			    EncodePrivateDICTData(ref vtTmpData,ref cffPrivteDict);			
			    vtOffset.Add(vtTmpData.Count);
		    }
		
		    List<int>		vtFDArryOffset = new List<int>();
		    List<byte>		vtFDArrayDataTmp = new List<byte>();		
		    vtFDArryOffset.Add(1);
		    for (int i=0; i<stPrivateDICT;i++)
		    {		
			    CCFFPrivteDict cffPrivteDict = CFFInfo.vtFDArry[i];
			    // FontSet Name ID
			    EncodeDICTInteger((long)cffPrivteDict.iFontNameID,ref vtFDArrayDataTmp);
			    vtFDArrayDataTmp.Add((byte)0x0c);
			    vtFDArrayDataTmp.Add((byte)0x26);

			    // PriuvateDict	size			
			    long iSize = vtOffset[i+1]-vtOffset[i];			
			    //EncodeDICTMaxInteger(iSize,vtFDArrayDataTmp);
			    EncodeDICTInteger(iSize,ref vtFDArrayDataTmp);
			    //PriuvateDict	offset
			    EncodeDICTMaxInteger(vtOffset[i],ref vtFDArrayDataTmp);
                //CFF_DICT_OPERATOR_PRIVATE
			    vtFDArrayDataTmp.Add((byte)0x12);
			    vtFDArryOffset.Add(vtFDArrayDataTmp.Count+1);
		    }
		    EncodeIndexData(ref vtFDArrayData,ref vtFDArrayDataTmp,ref vtFDArryOffset);

		    int stFDOffset =(int)(stOffset+vtFDArrayData.Count);
		    vtFDArrayDataTmp = new List<byte>();
		    vtFDArryOffset = new List<int>();
		    vtFDArrayData = new List<byte>();
		    vtFDArryOffset.Add(1);
		    for (int i=0; i<stPrivateDICT;i++)
		    {		
			    CCFFPrivteDict cffPrivteDict = CFFInfo.vtFDArry[i];
			    // FontSet Name ID
			    EncodeDICTInteger((long)cffPrivteDict.iFontNameID,ref vtFDArrayDataTmp);
			    vtFDArrayDataTmp.Add((byte)0x0c);
			    vtFDArrayDataTmp.Add((byte)0x26);

			    // PriuvateDict	size			
			    long iSize = vtOffset[i+1]-vtOffset[i];			
			    EncodeDICTInteger(iSize,ref vtFDArrayDataTmp);
			    // PriuvateDict	offset
			    EncodeDICTMaxInteger(vtOffset[i]+stFDOffset,ref vtFDArrayDataTmp);
                //CFF_DICT_OPERATOR_PRIVATE
			    vtFDArrayDataTmp.Add((byte)0x12);
			    vtFDArryOffset.Add(vtFDArrayDataTmp.Count+1);
		    }
		    EncodeIndexData(ref vtFDArrayData,ref vtFDArrayDataTmp,ref vtFDArryOffset);

		    int szTmpData = vtTmpData.Count;
		    for (int i=0; i<szTmpData;i++)
		    {
			    vtFDArrayData.Add(vtTmpData[i]);
		    }
    
            return HYRESULT.NOERROR;

        } // end of protected int	EncodeFDArray()

		protected HYRESULT	EncodeCharSets(ref List<byte> vtCharSetData)
        {
            vtCharSetData.Add(CFFInfo.Charset.format);
		
		    int sz = CFFInfo.Charset.format0.vtSID.Count;
		    if (CFFInfo.Charset.format==0)
		    {
			    for (int i=0; i<sz; i++)
			    {
				    ushort unTmp = CFFInfo.Charset.format0.vtSID[i];
				    vtCharSetData.Add((byte)(unTmp>>8));
				    vtCharSetData.Add((byte)(unTmp));
			    }
		    }

		    if (CFFInfo.Charset.format==1)
		    {
			    sz = CFFInfo.Charset.format1.vtRang.Count;
			    for (int i=0; i<sz; i++)
			    {
				    ushort unTmp = CFFInfo.Charset.format1.vtRang[i].first;

				    vtCharSetData.Add((byte)(unTmp>>8));
				    vtCharSetData.Add((byte)(unTmp));
				    vtCharSetData.Add((byte)(CFFInfo.Charset.format1.vtRang[i].left));
			    }
		    }

		    if (CFFInfo.Charset.format==2)
		    {
			    sz = CFFInfo.Charset.format2.vtRang.Count;
			    for (int i=0; i<sz; i++)
			    {
				    ushort unTmp = CFFInfo.Charset.format2.vtRang[i].first;
				    vtCharSetData.Add((byte)(unTmp>>8));
				    vtCharSetData.Add((byte)(unTmp));
				    unTmp = CFFInfo.Charset.format2.vtRang[i].left;
				    vtCharSetData.Add((byte)(unTmp>>8));
				    vtCharSetData.Add((byte)(unTmp));				
			    }
		    }

            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeCharSets()

        protected int GetFDIndex(int iGID)
	    {
		    CCFFFDSelect FDSelect = CFFInfo.FDSelect;
		    int FDIndex = -1;
		    if (FDSelect.iFormat==0)
		    {
			    if (FDSelect.format0.Count>iGID)
			    {
				    FDIndex = FDSelect.format0[iGID];
			    }
		    }

		    if (FDSelect.iFormat==3)
		    {
			    int sz=FDSelect.format3.vtRang3.Count;
			    for (int i=0; i<sz; i++)
			    {
				    if (i==sz-1)
				    {
					    if (iGID>=FDSelect.format3.vtRang3[i].first && iGID<=FDSelect.format3.sentinel)
					    {
						    FDIndex = FDSelect.format3.vtRang3[i].fdIndex;
					    }
				    }
				    else if ((iGID>=FDSelect.format3.vtRang3[i].first) && (iGID<FDSelect.format3.vtRang3[i+1].first))
				    {
					    FDIndex = FDSelect.format3.vtRang3[i].fdIndex;
					    break;
				    }
			    }
		    }

		    return FDIndex;

	    }	// end of int GetFDIndex()

		protected HYRESULT	EncodeCharStrings(ref List<byte>  vtCharSetData)
        {
            List<byte>	cCharStringTmp = new List<byte>();
            List<int>	cCharStringOffset = new List<int>();
            int st=Maxp.numGlyphs;

            cCharStringOffset.Add(1);
            for (int i=0; i<st; i++)
            {
                CharInfo Char = GlyphChars.CharInfo[i];

                /*
                int ijinjunTest = 0;
                if (Char.Unicode == "36")
                {
                    ijinjunTest++;
                }
                */ 

                if (CFFInfo.TopDICT.IsCIDFont==1)
                {
	                int iFDIndex = GetFDIndex(i);
	                if (iFDIndex!=-1)
	                {
		                CCFFPrivteDict CFFPrivteDict = CFFInfo.vtFDArry[iFDIndex];
		                EncodeCharString(ref cCharStringTmp,ref CFFPrivteDict,ref Char);
		                cCharStringOffset.Add(cCharStringTmp.Count+1);
	                }
                }
                else 
                {
	                CCFFPrivteDict CFFPrivteDict = CFFInfo.PrivteDict;
	                EncodeCharString(ref cCharStringTmp,ref CFFPrivteDict,ref Char);
	                cCharStringOffset.Add(cCharStringTmp.Count+1);
                }
            }

            EncodeIndexData(ref vtCharSetData,ref cCharStringTmp, ref cCharStringOffset);
            
            return HYRESULT.NOERROR;

        }   // end of protected int		EncodeCharStrings()
        
		protected HYRESULT	EncodeCharString(ref List<byte>  vtCharSetData,ref CCFFPrivteDict PrivteDict, ref CharInfo GlyphChar)
        {
            long lTmp = 0;
            if (GlyphChar.AdWidth != PrivteDict.ldefaultWidthX)
		    {
                lTmp = GlyphChar.AdWidth - PrivteDict.lnominalWidthX;
			    EncodeDICTInteger(lTmp,ref vtCharSetData);
		    }
            
            PtInfo              PrevPt = new PtInfo();
            int                 iDirection = 0;   //TYPE2_LINE_DIRECTION_0
		    int	                iOperatorFlag=0;            

            for (int i = 0; i < GlyphChar.ContourCount; i++)
            {
                ContourInfo cnturInf = GlyphChar.ContourInfo[i];

                int sPtNum = cnturInf.PointCount;
                int x = 0, ArgumentStackSize;
                int sX = 0, sY = 0;

                List<PtInfo> lstPtInf = cnturInf.PtInfo;
                while (x < sPtNum)
                {
                    ArgumentStackSize = 0;
                    iDirection = 0;// TYPE2_CURVE_DIRECTION_0;
                    iOperatorFlag = 0xFFFF;// TYPE2_CHARSTING_OPERATOR_UNKNOW;                     

                    PtInfo ptInf = cnturInf.PtInfo[x];
                    if (x == 0)
                    {
                        sX = ptInf.X - PrevPt.X;
                        sY = ptInf.Y - PrevPt.Y;

                        if (sX != 0 && sY != 0)
                        {
                            // x					
                            EncodeDICTInteger((long)sX, ref vtCharSetData);
                            // y					
                            EncodeDICTInteger((long)sY, ref vtCharSetData);
                            // operator
                            vtCharSetData.Add(0x15); //TYPE2_CHARSTING_OPERATOR_RMOVETO                            
                        }
                        else if (sX == 0)
                        {
                            // y						
                            EncodeDICTInteger((long)sY, ref vtCharSetData);
                            // operator						
                            vtCharSetData.Add(0x04);//TYPE2_CHARSTING_OPERATOR_VMOVETO
                        }
                        else if (sY == 0)
                        {
                            // X					
                            EncodeDICTInteger((long)sX, ref vtCharSetData);
                            // operator
                            vtCharSetData.Add(0x16);//TYPE2_CHARSTING_OPERATOR_HMOVETO
                        }

                        PrevPt.X = ptInf.X;
                        PrevPt.Y = ptInf.Y;
                        PrevPt.PtType = ptInf.PtType;

                        x++;
                    }
                    else if (ptInf.PtType == 0x01)//POINT_FLG_ANCHOR
                    {
                        EncodeLineToData(ref vtCharSetData, ref PrevPt, ref lstPtInf, ref x, ref iOperatorFlag, ref iDirection, ref ArgumentStackSize);
                        vtCharSetData.Add((byte)iOperatorFlag);
                    }
                    else if (ptInf.PtType == 0x00)
                    {
                        EncodeCurveToData(ref vtCharSetData, ref PrevPt, ref lstPtInf, ref x, ref iOperatorFlag, ref iDirection, ref ArgumentStackSize);
                        vtCharSetData.Add((byte)iOperatorFlag);                        
                    }
                    else // 防止出现 flag 随机值的问题出现
                    {
                        x++;
                    }
                } 
            }
            // operator		
            //TYPE2_CHARSTING_OPERATOR_ENDCHAR
		    vtCharSetData.Add(0x0e);

            return 0;

        }   // end of protected HYRESULT EncodeCharString()

        protected int EncodeLineToData(ref List<byte> vtData, ref PtInfo ptPrePoint, ref List<PtInfo> lstPoints, ref int iPointIndex, ref int iOperatorFlag, ref int iDirection, ref int iArgumentStackSize)
        {
            long sX = 0, sY = 0;
            int stPnts = lstPoints.Count;
            if (stPnts <= iPointIndex) return 0;

            PtInfo pt = lstPoints[iPointIndex];
            sX = pt.X - ptPrePoint.X;
            sY = pt.Y - ptPrePoint.Y;

            if (pt.PtType == 0)//POINT_FLG_CONTROL
            {                
                if (iOperatorFlag == 0x05)//TYPE2_CHARSTING_OPERATOR_RLINETO
                {
                    //48 48
                    if (iArgumentStackSize + 6 > 48) return 0;
                    if (iPointIndex >= stPnts - 3) return 0;

                    // 第一个点
                    sX = pt.X - ptPrePoint.X;
                    sY = pt.Y - ptPrePoint.Y;
                    // x			
                    EncodeDICTInteger(sX, ref vtData);
                    // y			
                    EncodeDICTInteger(sY, ref vtData);
                    iPointIndex++;

                    // 第二个点
                    PtInfo Pt2 = lstPoints[iPointIndex];
                    sX = Pt2.X - pt.X;
                    sY = Pt2.Y - pt.Y;
                    // x				
                    EncodeDICTInteger(sX, ref vtData);
                    // y				
                    EncodeDICTInteger(sY, ref vtData);
                    iPointIndex++;

                    // 第三个点
                    PtInfo Pt3 = lstPoints[iPointIndex];
                    sX = Pt3.X - Pt2.X;
                    sY = Pt3.Y - Pt2.Y;
                    // x			
                    EncodeDICTInteger(sX, ref vtData);
                    // y			
                    EncodeDICTInteger(sY, ref vtData);
                    iPointIndex++;
                    // operator
                    iOperatorFlag = 0x19;//TYPE2_CHARSTING_OPERATOR_RLINECURVE;

                    ptPrePoint.X = Pt3.X;
                    ptPrePoint.Y = Pt3.Y;
                    ptPrePoint.PtType = Pt3.PtType;

                    return 1;//TYPE2_LINE_END;
                }
                return 0;
            }
            if (iOperatorFlag == 0xFFFF)//TYPE2_CHARSTING_OPERATOR_UNKNOW
            {
                // RLINTTO
                if (sX != 0 && sY != 0)
                {
                    //48
                    if (iArgumentStackSize + 2 > 48) return 0;

                    // x			
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // y			
                    EncodeDICTInteger(sY, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iArgumentStackSize++;
                    iOperatorFlag = 0x05;// TYPE2_CHARSTING_OPERATOR_RLINETO;
                    iDirection = 0;// TYPE2_LINE_DIRECTION_0;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if (sX == 0) // x轴相同, Y轴方向移动
                {
                    if (iArgumentStackSize + 1 > 48) return 0;//48

                    EncodeDICTInteger(sY, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;

                    iOperatorFlag = 0x07;// TYPE2_CHARSTING_OPERATOR_VLINETO;
                    iDirection = 1;// TYPE2_LINE_DIRECTION_V;
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if (sY == 0) // Y轴相同,X轴方向移动
                {
                    if (iArgumentStackSize + 1 > 48) return 0;//48

                    EncodeDICTInteger(sX, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iOperatorFlag = 0x06;// TYPE2_CHARSTING_OPERATOR_HLINETO;
                    iDirection = 2; //TYPE2_LINE_DIRECTION_H;
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
            }
            else if (iOperatorFlag == 0x05)//TYPE2_CHARSTING_OPERATOR_RLINETO
            {
                if (sX == 0 || sY == 0) return 0;
                if (iArgumentStackSize + 2 > 48) return 0;//48

                // x			
                EncodeDICTInteger(sX,ref vtData);
                iArgumentStackSize++;
                // y			
                EncodeDICTInteger(sY,ref vtData);
                ptPrePoint.X = pt.X;
                ptPrePoint.Y = pt.Y;
                ptPrePoint.PtType = pt.PtType;
                iArgumentStackSize++;

                iOperatorFlag = 0x05;// TYPE2_CHARSTING_OPERATOR_RLINETO;
                iDirection = 0;// TYPE2_LINE_DIRECTION_0;

                iPointIndex++;
                EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex,ref iOperatorFlag,ref iDirection,ref iArgumentStackSize);
            }
            else if (iOperatorFlag == 0x06)//TYPE2_CHARSTING_OPERATOR_HLINETO
            {
                if (iArgumentStackSize + 1 > 48) return 0;//48
                if (iDirection == 2)//TYPE2_LINE_DIRECTION_H
                {
                    if (sX != 0) return 0;

                    EncodeDICTInteger(sY, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iDirection = 1; //TYPE2_LINE_DIRECTION_V;
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if (iDirection == 1)//TYPE2_LINE_DIRECTION_V
                {
                    if (sY != 0) return 0;

                    EncodeDICTInteger(sX, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iDirection = 2;// TYPE2_LINE_DIRECTION_H;
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
            }
            else if (iOperatorFlag == 0x07)//TYPE2_CHARSTING_OPERATOR_VLINETO
            {
                if (iArgumentStackSize + 1 > 48) return 0;//48
                if (iDirection == 1)//TYPE2_LINE_DIRECTION_V
                {
                    if (sY != 0) return 0;

                    EncodeDICTInteger(sX, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iDirection = 2; //TYPE2_LINE_DIRECTION_H
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if (iDirection == 2)//TYPE2_LINE_DIRECTION_H
                {
                    if (sX != 0) return 0;

                    EncodeDICTInteger(sY, ref vtData);
                    ptPrePoint.X = pt.X;
                    ptPrePoint.Y = pt.Y;
                    ptPrePoint.PtType = pt.PtType;
                    iDirection = 1;// TYPE2_LINE_DIRECTION_V;
                    iArgumentStackSize++;

                    iPointIndex++;
                    EncodeLineToData(ref vtData, ref ptPrePoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
            }

            return 0;

        }   // end of protected int EncodeLineToData();

        protected int EncodeCurveToData(ref List<byte> vtData, ref PtInfo PrevPoint, ref List<PtInfo> lstPoints, ref int iPointIndex, ref int iOperatorFlag, ref int iDirection,ref int iArgumentStackSize)
        {            
		    int sX = 0, sY = 0;
            int szPnts = lstPoints.Count;
            if (iPointIndex >= szPnts) return 0;

            PtInfo Pnt1 = lstPoints[iPointIndex];
            if (Pnt1.PtType == 0x01)//POINT_FLG_ANCHOR
            {
                if (iOperatorFlag == 0x08)//TYPE2_CHARSTING_OPERATOR_RRCURVETO
                {
                    if (iArgumentStackSize + 2 > 48) return 0;//48
                    // 如果是控制点代表下边将是一条直线 rcurveline
                    sX = Pnt1.X - PrevPoint.X;
                    sY = Pnt1.Y - PrevPoint.Y;
                    // x				
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // y				
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt1.X;
                    PrevPoint.Y = Pnt1.Y;
                    PrevPoint.PtType = Pnt1.PtType;
                    iPointIndex++;

                    // operator
                    iOperatorFlag = 0x18;// TYPE2_CHARSTING_OPERATOR_RCURVELINE;
                    iDirection = 0;//TYPE2_CURVE_DIRECTION_0;

                    return 1;// TYPE2_CURVE_END;
                }
                return 0;
            }

            //取后两个点
            if (szPnts <= iPointIndex + 2) return 0;
            PtInfo Pnt2 = lstPoints[iPointIndex + 1];
            PtInfo Pnt3 = lstPoints[iPointIndex + 2];

            // 压缩的起点
            if (iOperatorFlag == 0xFFFF)    //TYPE2_CHARSTING_OPERATOR_UNKNOW
            {
                // hvcurveto
                if (((Pnt1.Y - PrevPoint.Y) == 0) && ((Pnt3.X - Pnt2.X) == 0))
                {
                    if (iArgumentStackSize + 4 > 48) return 0;

                    sX = Pnt1.X - PrevPoint.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dxb
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dyb
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dyc
                    sY = Pnt3.Y - Pnt2.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;
                    iDirection = 3;// TYPE2_CURVE_DIRECTION_HV;
                    iOperatorFlag = 0x1f;// TYPE2_CHARSTING_OPERATOR_HVCURVETO;

                    iPointIndex += 3;
                    EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if (((Pnt1.X - PrevPoint.X) == 0) && ((Pnt3.Y - Pnt2.Y) == 0))	// vhcurveto
                {
                    if (iArgumentStackSize + 4 > 48) return 0;

                    sY = Pnt1.Y - PrevPoint.Y;
                    EncodeDICTInteger(sY,ref vtData);
                    iArgumentStackSize++;
                    // dxb
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX,ref vtData);
                    iArgumentStackSize++;
                    // dyb
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dyc
                    sX = Pnt3.X - Pnt2.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;
                    iDirection = 4;// TYPE2_CURVE_DIRECTION_VH;
                    iOperatorFlag = 0x1e;// TYPE2_CHARSTING_OPERATOR_VHCURVETO;
                    iPointIndex += 3;

                    EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else if ((Pnt3.Y - Pnt2.Y) == 0)		// hhcurveto
                {
                    if (iArgumentStackSize + 4 > 48) return 0;

                    // |-dy1? {dxa dxb dyb dxc} + hhcurveto|-
                    if ((Pnt1.Y - PrevPoint.Y) != 0)
                    {
                        if (iArgumentStackSize + 5 > 48) return 0;

                        // dy1
                        sY = Pnt1.Y - PrevPoint.Y;
                        EncodeDICTInteger(sY, ref vtData);
                        iArgumentStackSize++;
                    }
                    // |-{dxa dxb dyb dxc} + hhcurveto|-
                    // dxa
                    sX = Pnt1.X - PrevPoint.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dxb
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dyb
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxc
                    sX = Pnt3.X - Pnt2.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;
                    iDirection = 2;// TYPE2_CURVE_DIRECTION_H;
                    iOperatorFlag = 0x1b;// TYPE2_CHARSTING_OPERATOR_HHCURVETO;
                    iPointIndex += 3;

                    EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                // vvcuveto
                else if ((Pnt3.X - Pnt2.X) == 0)
                {
                    if (iArgumentStackSize + 4 > 48) return 0;

                    // |-dx1? {dya dxb dyb dyc} + hhcurveto|-
                    if ((Pnt1.X - PrevPoint.X) != 0)
                    {
                        if (iArgumentStackSize + 5 > 48) return 0;

                        // dx1
                        sX = Pnt1.X - PrevPoint.X;
                        EncodeDICTInteger(sX, ref vtData);
                        iArgumentStackSize++;
                    }
                    // |-{dya dxb dyb dyc} + hhcurveto|-
                    // dya
                    sY = Pnt1.Y - PrevPoint.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxb
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dyb
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dyc
                    sY = Pnt3.Y - Pnt2.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;
                    iDirection = 1;// TYPE2_CURVE_DIRECTION_V;
                    iOperatorFlag = 0x1a;// TYPE2_CHARSTING_OPERATOR_VVCURVETO;
                    iPointIndex += 3;

                    EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
                else
                {
                    if (iArgumentStackSize + 6 > 48) return 0;

                    // 第一个点
                    sX = Pnt1.X - PrevPoint.X;
                    sY = Pnt1.Y - PrevPoint.Y;
                    // x				
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // y
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // 第二个点
                    sX = Pnt2.X - Pnt1.X;
                    sY = Pnt2.Y - Pnt1.Y;
                    // x				
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // y			
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // 第三个点
                    sX = Pnt3.X - Pnt2.X;
                    sY = Pnt3.Y - Pnt2.Y;
                    // x
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // y			
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;
                    // operator
                    iOperatorFlag = 0x08;// TYPE2_CHARSTING_OPERATOR_RRCURVETO;
                    iDirection = 0;// TYPE2_CURVE_DIRECTION_0;
                    iPointIndex += 3;

                    EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                }
            }
            else if (iOperatorFlag == 0x08)//TYPE2_CHARSTING_OPERATOR_RRCURVETO
            {
                if (iArgumentStackSize + 6 > 48) return 0;
                if ((Pnt1.Y - PrevPoint.Y) == 0) return 0;

                // 第一个点
                sX = Pnt1.X - PrevPoint.X;
                sY = Pnt1.Y - PrevPoint.Y;
                // x		
                EncodeDICTInteger(sX, ref vtData);
                iArgumentStackSize++;
                // y			
                EncodeDICTInteger(sY, ref vtData);
                iArgumentStackSize++;
                // 第二个点
                sX = Pnt2.X - Pnt1.X;
                sY = Pnt2.Y - Pnt1.Y;
                // x			
                EncodeDICTInteger(sX, ref vtData);
                iArgumentStackSize++;
                // y			
                EncodeDICTInteger(sY, ref vtData);
                iArgumentStackSize++;
                // 第三个点
                sX = Pnt3.X - Pnt2.X;
                sY = Pnt3.Y - Pnt2.Y;
                // x
                EncodeDICTInteger(sX, ref vtData);
                iArgumentStackSize++;
                // y			
                EncodeDICTInteger(sY, ref vtData);
                iArgumentStackSize++;

                PrevPoint.X = Pnt3.X;
                PrevPoint.Y = Pnt3.Y;
                PrevPoint.PtType= Pnt3.PtType;
                // operator
                iOperatorFlag = 0x08;// TYPE2_CHARSTING_OPERATOR_RRCURVETO;
                iDirection = 0;// TYPE2_CURVE_DIRECTION_0;
                iPointIndex += 3;

                EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
            }
            else if (iOperatorFlag == 0x1b)//TYPE2_CHARSTING_OPERATOR_HHCURVETO
            {
                if (iArgumentStackSize + 4 > 48) return 0;
                if ((Pnt1.Y - PrevPoint.Y) != 0) return 0;
                if ((Pnt3.Y - Pnt2.Y) != 0) return 0;

                // |-{dxa dxb dyb dxc} + hhcurveto|-
                // dxa
                sX = Pnt1.X - PrevPoint.X;
                EncodeDICTInteger(sX,ref vtData);
                iArgumentStackSize++;
                // dxb
                sX = Pnt2.X - Pnt1.X;
                EncodeDICTInteger(sX,ref vtData);
                iArgumentStackSize++;
                // dyb
                sY = Pnt2.Y - Pnt1.Y;
                EncodeDICTInteger(sY, ref vtData);
                iArgumentStackSize++;
                // dxc
                sX = Pnt3.X - Pnt2.X;
                EncodeDICTInteger(sX, ref vtData);
                iArgumentStackSize++;

                PrevPoint.X = Pnt3.X;
                PrevPoint.Y = Pnt3.Y;
                PrevPoint.PtType = Pnt3.PtType;
                iDirection = 2;// TYPE2_CURVE_DIRECTION_H;
                iOperatorFlag = 0x1b;// TYPE2_CHARSTING_OPERATOR_HHCURVETO;
                iPointIndex += 3;

                EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
            }
            else if (iOperatorFlag == 0x1a)//TYPE2_CHARSTING_OPERATOR_VVCURVETO
            {
                if (iArgumentStackSize + 4 > 48) return 0;
                if ((Pnt1.X - PrevPoint.X) != 0) return 0;
                if ((Pnt3.X - Pnt2.X) != 0) return 0;

                // |-{dya dxb dyb dyc} + hhcurveto|-
                // dya
                sY = Pnt1.Y - PrevPoint.Y;
                EncodeDICTInteger(sY,ref vtData);
                iArgumentStackSize++;
                // dxb
                sX = Pnt2.X - Pnt1.X;
                EncodeDICTInteger(sX,ref vtData);
                iArgumentStackSize++;
                // dyb
                sY = Pnt2.Y - Pnt1.Y;
                EncodeDICTInteger(sY,ref vtData);
                iArgumentStackSize++;
                // dyc
                sY = Pnt3.Y - Pnt2.Y;
                EncodeDICTInteger(sY,ref vtData);
                iArgumentStackSize++;

                PrevPoint.X = Pnt3.X;
                PrevPoint.Y = Pnt3.Y;
                PrevPoint.PtType = Pnt3.PtType;

                iDirection = 2;// TYPE2_CURVE_DIRECTION_H;
                iOperatorFlag = 0x1a;// TYPE2_CHARSTING_OPERATOR_VVCURVETO;
                iPointIndex += 3;

                EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);

            }
            else if (iOperatorFlag == 0x1f)//TYPE2_CHARSTING_OPERATOR_HVCURVETO
            {
                if (iArgumentStackSize + 5 > 48) return 0;
                if (iDirection == 3)//TYPE2_CURVE_DIRECTION_HV
                {
                    if ((Pnt1.X - PrevPoint.X) != 0) return 0;

                    // dyd
                    sY = Pnt1.Y - PrevPoint.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxe
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dye
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxf
                    sX = Pnt3.X - Pnt2.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;

                    iDirection = 4;// TYPE2_CURVE_DIRECTION_VH;
                    iOperatorFlag = 0x1f;// TYPE2_CHARSTING_OPERATOR_HVCURVETO;
                    iPointIndex += 3;

                    //|-{dxa dxb dyb dyc dyd dxe dye dxf} + hvcurveto(31)|-
                    if ((Pnt3.Y - Pnt2.Y) == 0)
                    {
                        EncodeCurveToData(ref vtData, ref PrevPoint, ref  lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                    }
                    else	//|-{dxa dxb dyb dyc dyd dxe dye dxf} + dyf? hvcurveto(31)|-
                    {
                        // dyf
                        sY = Pnt3.Y - Pnt2.Y;
                        EncodeDICTInteger(sY,ref  vtData);
                        iArgumentStackSize++;
                        return 0;
                    }
                }
                else if (iDirection == 4)//TYPE2_CURVE_DIRECTION_VH
                {
                    if ((Pnt1.Y - PrevPoint.Y) != 0) return 0;

                    // dyd
                    sX = Pnt1.X - PrevPoint.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dxe
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dye
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dyf
                    sY = Pnt3.Y - Pnt2.Y;
                    EncodeDICTInteger(sY,ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;

                    iDirection = 3;// TYPE2_CURVE_DIRECTION_HV;
                    iOperatorFlag = 0x1f;// TYPE2_CHARSTING_OPERATOR_HVCURVETO;
                    iPointIndex += 3;

                    //|-{dxa dxb dyb dyc dyd dxe dye dxf} + hvcurveto(31)|-
                    if ((Pnt3.X - Pnt2.X) == 0)
                    {
                        EncodeCurveToData(ref vtData, ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                    }
                    else	//|-{dxa dxb dyb dyc dyd dxe dye dxf} + dyf? hvcurveto(31)|-
                    {
                        // dxf
                        sX = Pnt3.X - Pnt2.X;
                        EncodeDICTInteger(sX, ref vtData);
                        iArgumentStackSize++;

                        return 0;
                    }
                }

            }
            else if (iOperatorFlag == 0x1e)//TYPE2_CHARSTING_OPERATOR_VHCURVETO
            {
                if (iArgumentStackSize + 5 > 48) return 0;
                if (iDirection == 4)//TYPE2_CURVE_DIRECTION_VH
                {
                    if ((Pnt1.Y - PrevPoint.Y) != 0) return 0;
                    // dxd
                    sX = Pnt1.X - PrevPoint.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dxe
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dye
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dyf
                    sY = Pnt3.Y - Pnt2.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;

                    iDirection = 3;// TYPE2_CURVE_DIRECTION_HV;
                    iOperatorFlag = 0x1e;// TYPE2_CHARSTING_OPERATOR_VHCURVETO;
                    iPointIndex += 3;

                    //|-{dya dxb dyb dxc dxd dxe dye dyf} + vhcurveto(30)|-
                    if ((Pnt3.X - Pnt2.X) == 0)
                    {
                        EncodeCurveToData(ref vtData,ref PrevPoint, ref lstPoints,ref iPointIndex,ref iOperatorFlag, ref iDirection,ref iArgumentStackSize);
                    }
                    else	//|-{dya dxb dyb dxc dxd dxe dye dyf} + dxf? vhcurveto(30)|-
                    {
                        // dxf
                        sX = Pnt3.X - Pnt2.X;
                        EncodeDICTInteger(sX, ref vtData);
                        iArgumentStackSize++;

                        return 0;
                    }
                }
                else if (iDirection == 3)//TYPE2_CURVE_DIRECTION_HV
                {
                    if ((Pnt1.X - PrevPoint.X) != 0) return 0;

                    // dyd
                    sY = Pnt1.Y - PrevPoint.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxe
                    sX = Pnt2.X - Pnt1.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;
                    // dye
                    sY = Pnt2.Y - Pnt1.Y;
                    EncodeDICTInteger(sY, ref vtData);
                    iArgumentStackSize++;
                    // dxf
                    sX = Pnt3.X - Pnt2.X;
                    EncodeDICTInteger(sX, ref vtData);
                    iArgumentStackSize++;

                    PrevPoint.X = Pnt3.X;
                    PrevPoint.Y = Pnt3.Y;
                    PrevPoint.PtType = Pnt3.PtType;

                    iDirection = 4;// TYPE2_CURVE_DIRECTION_VH;
                    iOperatorFlag = 0x1e;// TYPE2_CHARSTING_OPERATOR_VHCURVETO;
                    iPointIndex += 3;

                    if ((Pnt3.Y - Pnt2.Y) == 0)
                    {
                        EncodeCurveToData(ref vtData,ref PrevPoint, ref lstPoints, ref iPointIndex, ref iOperatorFlag, ref iDirection, ref iArgumentStackSize);
                    }
                    else	//|-{dya dxb dyb dxc dxd dxe dye dyf} + dxf? vhcurveto(30)|-
                    {
                        // dyf
                        sY = Pnt3.Y - Pnt2.Y;
                        EncodeDICTInteger(sY, ref vtData);
                        iArgumentStackSize++;
                        return 0;
                    }
                }
            }

            return 0;

        }   // end of protected int EncodeCurveToData()
        
		protected void	EncodeIndexData(ref List<byte> vtOutData,ref List<byte> vtData, ref List<int> vtOffset)
        {
            ushort usTmp = 0;
		    long ulTmp = 0;
		    byte cTmp = 0;
		
		    //count
		    usTmp = (ushort)vtOffset.Count;
		    if (usTmp==0) return;

		    usTmp-=1;
		    vtOutData.Add((byte)(usTmp>>8));
		    vtOutData.Add((byte)usTmp);

		    if(usTmp>0)
		    {
			    byte offSize=0;
			    ulTmp = vtOffset[vtOffset.Count-1];
			    if ((ulTmp&0x000000ff) !=0) offSize = 1;
			    if ((ulTmp&0x0000ff00) !=0) offSize = 2;
			    if ((ulTmp&0x00ff0000) !=0) offSize = 3;
			    if ((ulTmp&0xff000000) !=0) offSize = 4;
			    //offSize		
			    vtOutData.Add(offSize);
			    //offset
			    for (int i=0; i<vtOffset.Count; i++)
			    {
				    ulTmp = vtOffset[i];
				    if(offSize==1)
				    {
					    cTmp  = (byte)(ulTmp&0x000000ff);				
					    vtOutData.Add(cTmp);
				    }
				    if (offSize==2)
				    {
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);
					    vtOutData.Add(cTmp);
					    cTmp = (byte)(ulTmp&0x000000ff);				
					    vtOutData.Add(cTmp);
				    }
				    if (offSize==3)
				    {
					    cTmp = (byte)((ulTmp&0x00ff0000)>>16);				
					    vtOutData.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);				
					    vtOutData.Add(cTmp);
					    cTmp = (byte)(ulTmp&0x000000ff);				
					    vtOutData.Add(cTmp);
				    }
				    if (offSize==4)
				    {
					    cTmp = (byte)(ulTmp>>24);
					    vtOutData.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x00ff0000)>>16);
					    vtOutData.Add(cTmp);
					    cTmp = (byte)((ulTmp&0x0000ff00)>>8);
					    vtOutData.Add(cTmp);
					    cTmp = (byte)(ulTmp&0x000000ff);
					    vtOutData.Add(cTmp);
				    }
			    }
                
                for (int i = 0; i < vtData.Count; i++)
			    {
                    vtOutData.Add(vtData[i]);
			    }
		    }
		    
        }   // end of protected void EncodeIndexData()

		protected int EncodePrivateDICTData(ref List<byte> vtData,ref CCFFPrivteDict PrivtDict)
        {
            //BlueValues;
		    int szBlV = PrivtDict.vtBlueValues.Count;
		    if (szBlV>0)
		    {
			    for (int i=0; i<szBlV;i++)
			    {
				    long lInteger = (long)(PrivtDict.vtBlueValues[i]);
				    EncodeDICTInteger(lInteger,ref vtData);				
			    }
                //CFF_DICT_OPERATOR_BLUEVALUES
			    vtData.Add(0x06);
		    }		

		    //OtherBlueValues;
		    int szOB = PrivtDict.vtOtherBlues.Count;
		    if (szOB>0)
		    {
			    for (int i=0; i<szOB; i++)
			    {
				    long lInteger = (long)(PrivtDict.vtOtherBlues[i]);
				    EncodeDICTInteger(lInteger,ref vtData);				
			    }
                //CFF_DICT_OPERATOR_OTHERBLUES
			    vtData.Add(0x07);
		    }		

		    //FamliyBlues;
		    int szFBLU = PrivtDict.vtFamilyBlues.Count;
		    if (szFBLU>0)
		    {
			    for (int i=0; i<szFBLU; i++)
			    {
				    long lInteger = (long)(PrivtDict.vtFamilyBlues[i]);
				    EncodeDICTInteger(lInteger,ref vtData);				
			    }
                //CFF_DICT_OPERATOR_FAMILYBLUES
			    vtData.Add(0x08);
		    }
		
		    //FamliyOtherBulues:
		    int szFOBLU = PrivtDict.vtFamilyOtherBlues.Count;
		    if (szFOBLU>0)
		    {
			    for (int i=0; i<szFOBLU; i++)
			    {
				    long lInteger = (long)(PrivtDict.vtFamilyOtherBlues[i]);
				    EncodeDICTInteger(lInteger,ref vtData);				
			    }
                //CFF_DICT_OPERATOR_FAMILYOTHERBLUES
			    vtData.Add(0x09);
		    }

		    //BlueScale:
		    if (PrivtDict.fBlueScale != 0.039625000)
		    {
			    EncodeDICTReal(PrivtDict.fBlueScale,ref vtData);
			    vtData.Add((byte)(0x0C));
			    vtData.Add((byte)(0x07));
		    }		

		    //BlueShift
		    if (PrivtDict.fBlueShift != 7.0)
		    {
			    EncodeDICTInteger((long)PrivtDict.fBlueShift,ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x0a);
		    }
		    //BlueFuzz
		    if (PrivtDict.fBlueFuzz != 1.0)
		    {
			    EncodeDICTInteger((long)PrivtDict.fBlueFuzz,ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x0b);
		    }
		    //StdHW		
		    if (PrivtDict.fStdHW != -1.0)
		    {
			    EncodeDICTInteger((long)PrivtDict.fStdHW,ref vtData);
			    vtData.Add((byte)0x0a);
		    }
		    //StdVW
		    if (PrivtDict.fStdHW != -1.0)
		    {
			    EncodeDICTInteger((long)PrivtDict.fStdVW, ref vtData);
			    vtData.Add((byte)0x0b);
		    }
		    //stemsnapH		
		    int sztmsnpH = PrivtDict.vtStemSnapH.Count;
		    if (sztmsnpH>0)
		    {
			    for (int i=0; i<sztmsnpH; i++)
			    {
				    long lInteger = (long)(PrivtDict.vtStemSnapH[i]);
				    EncodeDICTInteger(lInteger,ref vtData);					
			    }
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x0c);
		    }
		    //stemsnapV
		    int sztmsnpV = PrivtDict.vtStemSnapV.Count;
		    if (sztmsnpV>0)
		    {
			    for (int i=0; i<sztmsnpV; i++)
			    {
				    long lInteger = (long)(PrivtDict.vtStemSnapV[i]);
				    EncodeDICTInteger(lInteger,ref vtData);			
			    }
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x0d);
		    }		
		    // forcebold
		    if (PrivtDict.lForceBold!=0)
		    {
			    EncodeDICTInteger(PrivtDict.lForceBold,ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x0e);
		    }
		    //languageGroup		
		    if (PrivtDict.lLanguageGroup!=0)
		    {
			    EncodeDICTInteger(PrivtDict.lLanguageGroup,ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x11);
		    }
		    //expansionfactor
		    if (PrivtDict.fExpansionFactor<0.059999||PrivtDict.fExpansionFactor>0.06)
		    {
			    EncodeDICTReal(PrivtDict.fExpansionFactor, ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x12);
		    }		
		    //initialRandomSeed
		    if(PrivtDict.finitialRandomSeed!=0.0)
		    {
			    EncodeDICTReal(PrivtDict.finitialRandomSeed, ref vtData);
			    vtData.Add((byte)0x0c);
			    vtData.Add((byte)0x13);	
		    }	

            //暂时不财通subrs的组件服用方式
		    // subrs()
		    //lSubrPostion = 0;
		    //EncodeDICTInteger(PrivtDict.lSubrsOffset, vtData);
		    //vtData.Add((byte)(CFF_DICT_OPERATOR_SUBRS));   

		    if (PrivtDict.ldefaultWidthX !=0) 
		    {
			    //defaulwidthx
			    EncodeDICTInteger(PrivtDict.ldefaultWidthX,ref vtData);
			    vtData.Add((byte)0x14);
		    }

		    if (PrivtDict.lnominalWidthX!=0)
		    {
			    //nominalwidthx		
			    EncodeDICTInteger(PrivtDict.lnominalWidthX, ref vtData);
			    vtData.Add((byte)0x15);
		    }	

		    return 0;

	    }	// end of int EncodePrivateDICTData()
       
		protected void EncodeDICTMaxInteger(long lsrc, ref List<byte> vtData)
        {
            byte cTmp = 0;
		    vtData.Add(29);
		    cTmp = (byte)((lsrc&0xff000000)>>24);
		    vtData.Add(cTmp);
		    cTmp = (byte)((lsrc&0x00ff0000)>>16);
		    vtData.Add(cTmp);
		    cTmp = (byte)((lsrc&0x0000ff00)>>8);
		    vtData.Add(cTmp);
		    vtData.Add((byte)lsrc);
    
        }   // end of protected void		EncodeDICTMaxInteger()

		protected void	EncodeDICTInteger(long lsrc, ref List<byte> vtData)
        {
            byte bTmp =0;
            short sValue =0;
            if (lsrc>=-107 && lsrc<=107)
            {
                vtData.Add((byte)(lsrc+139));            
            }
            else if (lsrc>=108&&lsrc<=1131)
            {
                sValue = (short)(lsrc-108);

                bTmp = (byte)((sValue>>8)+247);
                vtData.Add(bTmp);
                bTmp = (byte)(sValue&0x00ff);
                vtData.Add(bTmp);            
            }
            else if (lsrc>=-1131 && lsrc<=-108)
            {
                sValue = (short)(-lsrc-108);
                bTmp = (byte)((sValue>>8)+251);
                vtData.Add(bTmp);
                bTmp = (byte)(sValue&0x00ff);
                vtData.Add(bTmp);
            }
            else if (lsrc >= -32768 &&  lsrc <= 32767)
            {
                sValue=(short)lsrc;

                vtData.Add(28);
                bTmp = (byte)(sValue>>8);
                vtData.Add(bTmp);
                bTmp = (byte)(sValue&0x00ff);
                vtData.Add(bTmp);
            }
            else if (lsrc<-32768 ||  lsrc>32767)
		    {
			    vtData.Add(29);
			    bTmp = (byte)((lsrc&0xff000000)>>24);
			    vtData.Add(bTmp);
			    bTmp = (byte)((lsrc&0x00ff0000)>>16);
			    vtData.Add(bTmp);
			    bTmp = (byte)((lsrc&0xff000000)>>8);
			    vtData.Add(bTmp);
			    vtData.Add((byte)lsrc);
		    }          
        
        }   // end of protected void	EncodeDICTInteger()

		protected void	EncodeDICTReal(double dbsrc,ref List<byte> vtData)
        {            
            string tmp;
            tmp = dbsrc.ToString();            
            byte[] bcd = System.Text.Encoding.Default.GetBytes(tmp);

            uint	        ix			= 0;
		    byte			ch;
		    byte			a			= 0;
		    bool			started		= false;
		    bool			inFraction	= false;
		    bool			inExponent	= false;
		    bool			even		= true;

            vtData.Add((byte)30);
            while (ix < bcd.Length)
            {
                ch = bcd[ix++];
                a = (byte)(a << 4);

                if (ch>='0' && ch<='9')
			    {
				    a |= (byte)(ch - '0');
				    started = true;
			    } else if (ch == '-')
			    {
				    if (started)			
					    return;				
				    a |= 0xe;
				    started = true;

			    } else if (ch == 'E')
			    {
				    if (inExponent)				
					    return;

				    if (bcd [ix] == '-')
				    {
					    a |= 0xc;
					    ++ix;
				    }
				    else
				    {
					    a |= 0xb;
				    }

				    inExponent	= true;
				    started		= true;

			    } else if (ch == '.')
			    {
				    if (inFraction)										
					    return;
				    if (inExponent)
					    return;
				    a |= 0xa;
				    inFraction = true;
				    started = true;
			    }
			    even = !even;
			    if (even) 				
				    vtData.Add(a);
            }

            a = (byte)((a<<4)|0xf);
            if (even)
                a = (byte)((a << 4)|0xf);

            vtData.Add(a);

        }   // end of protected void EncodeDICTReal()

        public HYRESULT EncodeGlyph()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.GLYF_TAG);
            if (iEntryIndex == -1) return HYRESULT.GLYF_ENCODE;

            Loca = new CLoca();

            long lTableBegin = FileStrm.Position;
            UInt32 ulGlyphOffset = 0;
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;
            
            UInt16 usTmp;            
            byte[] btTmp;

            int iGlyfNum = GlyphChars.CharInfo.Count;             
            //Table version number
            for (int i = 0; i < GlyphChars.CharInfo.Count; i++)
            {
                CharInfo GlyfItem = GlyphChars.CharInfo[i];

                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyfItem.Section, out xmin, out ymin, out xmax, out ymax);

                Loca.vtLoca.Add(ulGlyphOffset);

                if (GlyfItem.IsComponent == 0 && GlyfItem.ContourCount>0)
                {
                    //numberOfContours                
                    usTmp = hy_cdr_int16_to((UInt16)GlyfItem.ContourCount);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                    //xMin
                    usTmp = hy_cdr_int16_to((UInt16)xmin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                    //yMin
                    usTmp = hy_cdr_int16_to((UInt16)ymin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                    //xMax
                    usTmp = hy_cdr_int16_to((UInt16)xmax);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                    //yMax
                    usTmp = hy_cdr_int16_to((UInt16)ymax);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                    
                    PtInfo prePoint = new PtInfo();
                    List<Int16> vtXCoordinat = new List<short>();
                    List<Int16> vtYCoordinat = new List<short>();
                    List<byte> vtflage = new List<byte>();
                    List<UInt16> vtendPtsOfContour = new List<ushort>();
                    int XOffset = 0, YOffset = 0;

                    int TatlPntNum = 0;
                    try
                    {
                        for (int j = 0; j < GlyfItem.ContourCount; j++)
                        {
                            ContourInfo cntur = GlyfItem.ContourInfo[j];

                            int stCutPntnum = cntur.PointCount;
                            for (int x = 0; x < stCutPntnum; x++)
                            {
                                PtInfo pt = cntur.PtInfo[x];
                                byte flage = 0;

                                if (pt.PtType == 0x01)//POINT_FLG_ANCHOR
                                {
                                    flage |= 0x0001;// GLYF_SMPL_ONCURVE;
                                }

                                // 计算X轴坐标点偏移量
                                XOffset = pt.X - prePoint.X;
                                if (XOffset == 0)
                                {
                                    // 当前点与上一点x位置相同
                                    flage |= 0x0010;// GLYF_SMPL_X_SAME_FLG;
                                }
                                else
                                {
                                    // 如果偏移量的绝对值小于255，按单字节保存
                                    if (Math.Abs(XOffset) <= 255)
                                    {
                                        // x-Short Vector位设置为1
                                        flage |= 0x0002;// GLYF_SMPL_X_VECTOR;
                                                        // 如果offset为正,This x is same表示偏移方向 1为正， 0为负
                                        if (XOffset > 0) flage |= 0x0010;// GLYF_SMPL_X_SAME_FLG;

                                        vtXCoordinat.Add((short)Math.Abs(XOffset));
                                    }
                                    else    // 按双字节保存
                                    {
                                        vtXCoordinat.Add((short)XOffset);
                                    }
                                }

                                // 计算Y轴坐标点偏移量				
                                YOffset = pt.Y - prePoint.Y;
                                if (YOffset == 0)
                                {
                                    // 当前点与上一点偏移量相同
                                    flage |= 0x0020;// GLYF_SMPL_Y_SAME_FLG;
                                }
                                else
                                {
                                    // 如果偏移量的绝对值小于255，按单字节保存
                                    if (Math.Abs(YOffset) <= 255)
                                    {
                                        flage |= 0x0004;// GLYF_SMPL_Y_VECTOR;

                                        // 如果offset为正,This x is same表示偏移方向 1为正， 0为负
                                        if (YOffset > 0) flage |= 0x0020;
                                        vtYCoordinat.Add((short)Math.Abs(YOffset));
                                    }
                                    else    // 按双字节保存
                                    {
                                        vtYCoordinat.Add((short)YOffset);
                                    }
                                }
                                prePoint = pt;
                                vtflage.Add(flage);
                            }

                            TatlPntNum += stCutPntnum;
                            vtendPtsOfContour.Add((UInt16)(TatlPntNum - 1));
                        }
                    }
                    catch(Exception expt)
                    {
                        throw (expt);
                    }
                    //endPtsOfContours
                    int stEndPtsCnturNum = vtendPtsOfContour.Count;
                    for (int y = 0; y < stEndPtsCnturNum; y++)
                    {
                        usTmp = hy_cdr_int16_to(vtendPtsOfContour[y]);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);
                    }

                    //instructionLength					
                    usTmp = 0;//hy_cdr_int16_to((UInt16)GlyfItem.0));
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    //instructions[n]
                    /*
                    size_t stInstrSize = glyph.vtInstruntions.size();
                    for (size_t y=0; y<stInstrSize; y++)
                    {
                    fwrite(&glyph.vtInstruntions[y],1,1,m_pFontFile);
                    }*/

                    // 对Flag做行程压缩
                    int iRepeat = 0;
                    byte btFlag = 0, btNextFlag = 0;
                    List<byte> vtRle = new List<byte>();
                    int stflagsize = vtflage.Count;

                    //int y = 0, z = 0;
                    for (int y = 0; y < stflagsize; y++)
                    {
                        btFlag = vtflage[y];
                        iRepeat = 0;
                        int z = 0;
                        for (z = y + 1; z < stflagsize; z++)
                        {
                            if (iRepeat > 255) break;
                            btNextFlag = vtflage[z];

                            if (btNextFlag != btFlag) break;
                            iRepeat++;
                        }

                        if (iRepeat > 0)
                        {
                            y = z - 1;
                            btFlag |= 0x0008;//GLYF_SMPL_REPEAT;
                            vtRle.Add(btFlag);
                            vtRle.Add((byte)iRepeat);
                        }
                        else
                        {
                            vtRle.Add(btFlag);
                        }
                    }

                    //flags
                    int stRlesz = vtRle.Count;
                    for (int y = 0; y < stRlesz; y++)                    
                    {
                        FileStrm.WriteByte(vtRle[y]);
                    }

                    byte ucTmp = 0;
                    //xCoordinates
                    int stcrdts = vtflage.Count;
                    int stAryIndex = 0;
                    for (int y = 0; y < stcrdts; y++)
                    {
                        if ((vtflage[y] & 0x0002) > 0)//GLYF_SMPL_X_VECTOR
                        {
                            ucTmp = (byte)vtXCoordinat[stAryIndex];
                            FileStrm.WriteByte(ucTmp);
                        }
                        else
                        {
                            if ((vtflage[y] & 0x0010) > 0)//GLYF_SMPL_X_SAME_FLG
                                continue;
                            usTmp = hy_cdr_int16_to((ushort)vtXCoordinat[stAryIndex]);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                        stAryIndex++;
                    }

                    stAryIndex = 0;
                    //yCoordinates
                    stcrdts = vtflage.Count;
                    for (int y = 0; y < stcrdts; y++)
                    {
                        if ((vtflage[y] & 0x0004) > 0)//GLYF_SMPL_Y_VECTOR
                        {
                            ucTmp = (byte)vtYCoordinat[stAryIndex];
                            FileStrm.WriteByte(ucTmp);
                        }
                        else
                        {

                            if ((vtflage[y] & 0x0020) > 0)//GLYF_SMPL_Y_SAME_FLG
                                continue;

                            usTmp = hy_cdr_int16_to((ushort)vtYCoordinat[stAryIndex]);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                        stAryIndex++;
                    }
                }
                else if (GlyfItem.IsComponent == 1) // 组件字形
                {
                    //numberOfContours					
                    usTmp = 0xffff;
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    //xMin
                    usTmp = hy_cdr_int16_to((ushort)xmin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    //yMin
                    usTmp = hy_cdr_int16_to((ushort)ymin);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    //xMax
                    usTmp = hy_cdr_int16_to((ushort)xmax);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    //yMax
                    usTmp = hy_cdr_int16_to((ushort)ymax);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

                    int stCmpsz = GlyfItem.CmpInf.Count;
                    for (int x = 0; x < stCmpsz; x++)
                    {
                        //flags
                        CmpInf Cmps = GlyfItem.CmpInf[x];

                        usTmp = hy_cdr_int16_to((ushort)Cmps.Flag);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);

                        //glyphIndex
                        usTmp = hy_cdr_int16_to((ushort)Cmps.Gid);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);

                        //
                        if ((Cmps.Flag & 0x0001) > 0)//GLYF_CMPST_ARG_1_AND_2_ARE_WORDS
                        {
                            usTmp = hy_cdr_int16_to((ushort)Cmps.Arg[0]);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            usTmp = hy_cdr_int16_to((ushort)Cmps.Arg[1]);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                        else
                        {
                            ushort arg0 = (ushort)Cmps.Arg[0];
                            ushort arg1 = (ushort)Cmps.Arg[1];

                            usTmp = (ushort)(arg0 << 8 | (arg1 & 0xFF));
                            usTmp = hy_cdr_int16_to(usTmp);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }

                        if ((Cmps.Flag & 0x0008) > 0)//GLYF_CMPST_WE_HAVE_A_SCALE
                        {
                            usTmp = hy_cdr_int16_to((ushort)Cmps.Scale);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                        else if ((Cmps.Flag & 0x0040) > 0)//GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE
                        {
                            usTmp = hy_cdr_int16_to((ushort)Cmps.ScaleX);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            usTmp = hy_cdr_int16_to((ushort)Cmps.ScaleY);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                        else if ((Cmps.Flag & 0x0080) > 0)//GLYF_CMPST_WE_HAVE_A_TWO_BY_TWO
                        {
                            usTmp = hy_cdr_int16_to((ushort)Cmps.ScaleX);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            usTmp = hy_cdr_int16_to((ushort)Cmps.Scale01);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            usTmp = hy_cdr_int16_to((ushort)Cmps.Scale10);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            usTmp = hy_cdr_int16_to((ushort)Cmps.ScaleY);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                        }
                    }

                    /* 目前没有指令集
                    if (glyph.vtInstruntions.size() > 0)
                    {
                        size_t stInstSize = glyph.vtInstruntions.size();
                        for (size_t x = 0;x<stInstSize;x++)
                        {
                            fwrite(&glyph.vtInstruntions[x],1,1,m_pFontFile);
                        }						
                    }*/
                }

                ulGlyphOffset = (uint)(FileStrm.Position - lTableBegin);

                int iTail = 4 - (int)ulGlyphOffset % 4;
                if (ulGlyphOffset % 4 > 0)
                {
                    byte c = 0;
                    for (int i0 = 0; i0 < iTail; i0++)
                    {
                        FileStrm.WriteByte(c);
                    }
                    ulGlyphOffset += (uint)iTail;
                }
            }

            // loca number = glyph+1;
            Loca.vtLoca.Add(ulGlyphOffset);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeGlyph()      

        public HYRESULT Encodeloca()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.LOCA_TAG);
            if (iEntryIndex == -1) return HYRESULT.LOCA_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            UInt16 usTmp;
            UInt32 ulTmp;
            byte[] btTmp;
            if (Head.indexToLocFormat == 0)
            {
                foreach (uint iloca in Loca.vtLoca)
                {
                    usTmp = hy_cdr_int16_to((UInt16)iloca);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                }
            }
            else if (Head.indexToLocFormat == 1)
            {
                foreach (uint iloca in Loca.vtLoca)
                {
                    ulTmp = hy_cdr_int32_to(iloca);
                    btTmp = BitConverter.GetBytes(ulTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
                }            
            }

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodeloca()

        public HYRESULT EncodeGSUB()
        {
            UInt16	usTmp = 0;
            byte[] btTmp;

            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.GSUB_TAG);
            if (iEntryIndex == -1) return HYRESULT.GSUB_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;
			long GsubBeginPos = tbEntry.offset;
            long GsubEndPos = 0;
			
			//Table version number
            Gsub.version.value = 1;
            Gsub.version.fract = 0;
            usTmp = hy_cdr_int16_to((UInt16)Gsub.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            usTmp = hy_cdr_int16_to((UInt16)Gsub.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);			

			usTmp = 0;
			//ScriptList offset			
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//FeatureList offset
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//LookupList offset
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            EncodeScriptList(GsubBeginPos,ref GsubEndPos);
            EncodeFeatureList(GsubBeginPos, ref GsubEndPos);
            EncodeLookupList(GsubBeginPos, ref GsubEndPos);

            tbEntry.length = (uint)(FileStrm.Position - tbEntry.offset);
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeGSUB()

        protected HYRESULT EncodeScriptList(long GsubBeginPos, ref long GsubEndPos)
        {
            UInt16				usTmp				= 0;			
            byte[]              btTmp;

			long ulScriptLstBeginPos = FileStrm.Position;

			//write ScriptList offset
            FileStrm.Seek(GsubBeginPos+4,SeekOrigin.Begin);		
			usTmp = hy_cdr_int16_to((UInt16)(ulScriptLstBeginPos-GsubBeginPos));
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

			FileStrm.Seek(ulScriptLstBeginPos,SeekOrigin.Begin);
			int stScriptsList = Gsub.vtScriptsList.Count;

			//ScriptCount			
			usTmp = hy_cdr_int16_to((UInt16)stScriptsList);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);	

			List<long> vtScriptOffset = new List<long>();
			for (int i=0; i<stScriptsList; i++)
			{
				CScriptRecord  ScriptRecord = Gsub.vtScriptsList[i];

				//ScriptTag
                FileStrm.WriteByte(ScriptRecord.Tag[0]);	
				FileStrm.WriteByte(ScriptRecord.Tag[1]);	
                FileStrm.WriteByte(ScriptRecord.Tag[2]);	
                FileStrm.WriteByte(ScriptRecord.Tag[3]);

				//Script offset;
				usTmp = 0;		
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);				
			}

			for (int i=0; i<stScriptsList; i++)
			{
				// save Script Offset
				long ulScriptBegin = FileStrm.Position;
				vtScriptOffset.Add(ulScriptBegin-ulScriptLstBeginPos);

				CScriptRecord  ScriptRecord = Gsub.vtScriptsList[i];
				//DefaultLangSys offset;				
				usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

				//LangSysCount
				int stLangSysRecord = ScriptRecord.Script.vtLangSysRecord.Count;
				usTmp = hy_cdr_int16_to((UInt16)stLangSysRecord);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

				//LangSysRecord
				for (int j=0; j<stLangSysRecord; j++)
				{
					CLangSysRecord LangSysRecord = ScriptRecord.Script.vtLangSysRecord[j];

					//LangSysRecord Tag
                    FileStrm.WriteByte(LangSysRecord.SysTag[0]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[1]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[2]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[3]);
					
					//LangSys Offset
					usTmp = 0;
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);					
				}

				List<long> vtLangSysOffset = new List<long>();
				for (int j=0; j<stLangSysRecord; j++)
				{
					long ulLangSysBegin = FileStrm.Position;
					vtLangSysOffset.Add(ulLangSysBegin-ulScriptBegin);
					
					CLangSysRecord LangSysRecord = ScriptRecord.Script.vtLangSysRecord[j];
					//LangSys table
					//LookupOrder reserved 0
					usTmp = 0;
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
					
					//ReqFeatureIndex
					usTmp = hy_cdr_int16_to(LangSysRecord.LangSys.ReqFeatureIndex);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
					
					//FeatureCount
					int stFeatureCount = LangSysRecord.LangSys.vtFeatureIndex.Count;
					usTmp = hy_cdr_int16_to((UInt16)stFeatureCount);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
					
					for (int k=0; k<stFeatureCount; k++)
					{
						//FeatureIndex
						usTmp = hy_cdr_int16_to(LangSysRecord.LangSys.vtFeatureIndex[k]);                        
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);						
					}
				}
				
				GsubEndPos = FileStrm.Position;

				// write LangSys offset
                FileStrm.Seek(ulScriptBegin+4,SeekOrigin.Begin);

				//LangSysRecord offset
				for (int j=0; j<stLangSysRecord; j++)
				{
					//LangSysTag
					CLangSysRecord LangSysRecord = ScriptRecord.Script.vtLangSysRecord[j];

					//LangSysRecord Tag
                    FileStrm.WriteByte(LangSysRecord.SysTag[0]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[1]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[2]);
                    FileStrm.WriteByte(LangSysRecord.SysTag[3]);				

					//offset
					usTmp = hy_cdr_int16_to((UInt16)vtLangSysOffset[j]);
					btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
				}
                FileStrm.Seek(GsubEndPos,SeekOrigin.Begin);			

				// write DefaultLangSys offset
				ScriptRecord.Script.DefaultLangSys.FeatureCount = (ushort)ScriptRecord.Script.DefaultLangSys.vtFeatureIndex.Count;
				if (ScriptRecord.Script.DefaultLangSys.FeatureCount>0)
				{	
                    FileStrm.Seek(ulScriptBegin,SeekOrigin.Begin);

					usTmp = hy_cdr_int16_to((UInt16)(GsubEndPos-ulScriptBegin));
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
					
                    FileStrm.Seek(GsubEndPos,SeekOrigin.Begin);			

					//LookupOrder eserved for an offset to a reordering table
					usTmp=0;
					btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

					//ReqFeatureIndex
					usTmp = hy_cdr_int16_to(ScriptRecord.Script.DefaultLangSys.ReqFeatureIndex);
					btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
					//FeatureCount
					int stFeatureCount = ScriptRecord.Script.DefaultLangSys.vtFeatureIndex.Count;					
					usTmp = hy_cdr_int16_to((UInt16)stFeatureCount);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);

					for (int k=0; k<stFeatureCount; k++)
					{
						//FeatureIndex
						usTmp = hy_cdr_int16_to(ScriptRecord.Script.DefaultLangSys.vtFeatureIndex[k]);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);						
					}
                    GsubEndPos = FileStrm.Position;
				}
			}						

			// write scriptlist  offset			
            FileStrm.Seek(ulScriptLstBeginPos + 2,SeekOrigin.Begin);
			for (int j=0; j<stScriptsList; j++)
			{
				CScriptRecord  ScriptRecord = Gsub.vtScriptsList[j];
				//ScriptTag
                FileStrm.WriteByte(ScriptRecord.Tag[0]);
                FileStrm.WriteByte(ScriptRecord.Tag[1]);
                FileStrm.WriteByte(ScriptRecord.Tag[2]);
                FileStrm.WriteByte(ScriptRecord.Tag[3]);

				// offset
				usTmp = hy_cdr_int16_to((UInt16)vtScriptOffset[j]);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
			}
            FileStrm.Seek(GsubEndPos + 2, SeekOrigin.Begin);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeScriptList()

        protected HYRESULT EncodeFeatureList(long GsubBeginPos, ref long GsubEndPos)
        {
            UInt16 usTmp = 0;
            byte[] btTmp;
			int stFeatureList = Gsub.vtFeaturesList.Count;
			long ulFeatureLstBeginPos = FileStrm.Position;

			//write FeatureList offset			
            FileStrm.Seek(GsubBeginPos+6,SeekOrigin.Begin);

			usTmp = hy_cdr_int16_to((UInt16)(ulFeatureLstBeginPos-GsubBeginPos));
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			FileStrm.Seek(ulFeatureLstBeginPos,SeekOrigin.Begin);

			//FeatureCount			
			usTmp = hy_cdr_int16_to((UInt16)stFeatureList);
			btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			
			List<long> vtFeatureOffset = new List<long>();
			//FeatureRecord
			usTmp = 0;
			for (int i=0; i<stFeatureList; i++)
			{
				CFeatureRecord FeaturTable = Gsub.vtFeaturesList[i];
				//FeatureTag
                FileStrm.WriteByte(FeaturTable.Tag[0]);
                FileStrm.WriteByte(FeaturTable.Tag[1]);
                FileStrm.WriteByte(FeaturTable.Tag[2]);
                FileStrm.WriteByte(FeaturTable.Tag[3]);

				//Feature  0ffset = 0;
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
			}

			for (int i=0; i<stFeatureList; i++)
			{
				CFeatureRecord FeaturTable = Gsub.vtFeaturesList[i];

				//  Feature offset
				vtFeatureOffset.Add(FileStrm.Position-ulFeatureLstBeginPos);

				//FeatureParams 
				usTmp = 0;				
				btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

				//LookupCount
				usTmp = (UInt16)FeaturTable.FeatureTable.vtLookupListIndex.Count;
				usTmp = hy_cdr_int16_to(usTmp);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
				
				for (int j=0; j<FeaturTable.FeatureTable.vtLookupListIndex.Count; j++)
				{
					usTmp = FeaturTable.FeatureTable.vtLookupListIndex[j];
					usTmp = hy_cdr_int16_to(usTmp);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);					
				}
			}
			GsubEndPos = FileStrm.Position;

			long ltmp = ulFeatureLstBeginPos+2;
			for (int i=0; i<stFeatureList; i++)
			{
				CFeatureRecord FeaturTable = Gsub.vtFeaturesList[i];
                FileStrm.Seek(ltmp,SeekOrigin.Begin);
				
				//FeatureTag
                FileStrm.WriteByte(FeaturTable.Tag[0]);
                FileStrm.WriteByte(FeaturTable.Tag[1]);
                FileStrm.WriteByte(FeaturTable.Tag[2]);
                FileStrm.WriteByte(FeaturTable.Tag[3]);

				usTmp = hy_cdr_int16_to((UInt16)vtFeatureOffset[i]);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);	

			}
            FileStrm.Seek(GsubEndPos,SeekOrigin.Begin);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeFeatureList()

        protected HYRESULT EncodeLookupList(long GsubBeginPos, ref long GsubEndPos)
        {
            UInt16 usTmp = 0;
            byte[] btTmp;
			int szLookUpList = Gsub.vtLookupList.Count;

            List<long> vtLookupOffset = new List<long>();
            long lLookupLstBegin = FileStrm.Position;

            //write FeatureList offset
            FileStrm.Seek(GsubBeginPos+8,SeekOrigin.Begin);
            usTmp = hy_cdr_int16_to((UInt16)(lLookupLstBegin-GsubBeginPos));
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            FileStrm.Seek(lLookupLstBegin, SeekOrigin.Begin);

            //LookupCount
            usTmp = hy_cdr_int16_to((UInt16)(szLookUpList));
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            //Offset	Lookup[LookupCount]			
            for (int i = 0; i < szLookUpList; i++)
            {
                usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
            }

            for (int i = 0; i < szLookUpList; i++)
            {
                // look up begin position
                long lLookUpBegin = FileStrm.Position;
                // save  SubTables offset from beginning of Lookup table 
                vtLookupOffset.Add(lLookUpBegin - lLookupLstBegin);

                CLookUp lookup = Gsub.vtLookupList[i];
                //LookupType
				usTmp = hy_cdr_int16_to((UInt16)(lookup.LookUpType));
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);            

                //LookupFlag
				usTmp = hy_cdr_int16_to((UInt16)(lookup.LookUpFlag));
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

                //SubTableCount
                usTmp = hy_cdr_int16_to((UInt16)(lookup.SubTableCount));
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

                List<long> vtSubTableOffset = new List<long>();
                if (lookup.LookUpType == 1)	//Single 
                {
                    //SubTable Array of offsets to SubTables-from beginning of Lookup table
                    usTmp = 0;
                    for (int j = 0; j < lookup.vtLookupType1.Count; j++)
                    {
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);
                    }
                }

                //MarkFilteringSet 0
                usTmp = 0;
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

                //SubTable Single Substitution
                if (lookup.LookUpType == 1)
                {
                    for (int j = 0; j < lookup.vtLookupType1.Count; j++)
                    {
                        CLookUpType1 LookUpType1 = lookup.vtLookupType1[j];

                        //Single Substitution begin position
                        long lSubstitutionBegin = FileStrm.Position;
                        // save  SubTables offset from beginning of Lookup table 
                        vtSubTableOffset.Add((long)(lSubstitutionBegin-lLookUpBegin));
                        //SubstFormat
                        usTmp = hy_cdr_int16_to(LookUpType1.SubFormat);
                        btTmp = BitConverter.GetBytes(usTmp);
                        FileStrm.Write(btTmp, 0, btTmp.Length);

                        if (LookUpType1.SubFormat == 2)
                        {
                            CLookUpSingleSubstitution2 Substitution2 = LookUpType1.Substitution2;
                            //Coverage	Offset to Coverage table-from beginning of Substitution table
                            usTmp = 0;
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);
                            //GlyphCount
                            usTmp = hy_cdr_int16_to(Substitution2.GlyphCount);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            //Substitute Array of substitute GlyphIDs-ordered by Coverage Index
							for (ushort x=0; x<Substitution2.GlyphCount; x++)
							{
								usTmp = hy_cdr_int16_to(Substitution2.vtGlyphID[x]);
                                btTmp = BitConverter.GetBytes(usTmp);
                                FileStrm.Write(btTmp, 0, btTmp.Length);
							}

                            //write Coverage offset
							GsubEndPos = FileStrm.Position;
                            UInt16 sSubstitutionOffset = (UInt16)(GsubEndPos - lSubstitutionBegin);
                            FileStrm.Seek(lSubstitutionBegin + 2,SeekOrigin.Begin);
							
							usTmp = hy_cdr_int16_to(sSubstitutionOffset);
                            btTmp = BitConverter.GetBytes(usTmp);
                            FileStrm.Write(btTmp, 0, btTmp.Length);

                            // seek to file end                             
                            FileStrm.Seek(GsubEndPos,SeekOrigin.Begin);

                            //Coverage
							if (Substitution2.Coverage.CoverageFormat == 1)
							{
								usTmp = hy_cdr_int16_to(Substitution2.Coverage.CoverageFormat);
								btTmp = BitConverter.GetBytes(usTmp);
                                FileStrm.Write(btTmp, 0, btTmp.Length);

								usTmp = hy_cdr_int16_to(Substitution2.Coverage.GlyphCount);
                                btTmp = BitConverter.GetBytes(usTmp);
                                FileStrm.Write(btTmp, 0, btTmp.Length);
								
								for (UInt16 x=0; x<Substitution2.Coverage.GlyphCount;x++)
								{
									usTmp = hy_cdr_int16_to(Substitution2.Coverage.vtGlyphID[x]);
                                    btTmp = BitConverter.GetBytes(usTmp);
                                    FileStrm.Write(btTmp, 0, btTmp.Length);
								}
							}
                            GsubEndPos = FileStrm.Position;
                        }
                    }
                }

                // write  SubTables offset from beginning of Lookup table 
                FileStrm.Seek(lLookUpBegin+6,SeekOrigin.Begin);
				for (UInt16 us =0; us<lookup.SubTableCount; us++)
				{
					usTmp = hy_cdr_int16_to((UInt16)vtSubTableOffset[us]);
                    btTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(btTmp, 0, btTmp.Length);
				}                
            }

            // write  Lookup  offset from beginning of LookupList -zero based 
            FileStrm.Seek(lLookupLstBegin + 2, SeekOrigin.Begin);
            for (int us = 0; us < szLookUpList; us++)
            {
                usTmp = hy_cdr_int16_to((UInt16)vtLookupOffset[us]);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
            }
            
            FileStrm.Seek(GsubEndPos, SeekOrigin.Begin);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EncodeLookupList()

        protected void CountVheaVmtx()
	    {
		    Vhea = new CVhea();
		    Vmtx = new CVmtx();

		    Vhea.version.value		= 0x0001;
		    Vhea.version.fract		= 0x0000;
		    Vhea.ascent				= (short)(Head.unitsPerEm/2);
		    Vhea.descent			= (short)(0 - Head.unitsPerEm/2);
		    Vhea.lineGap			= 0;
		    Vhea.advanceHeightMax	= (short)Head.unitsPerEm;
		    Vhea.numOfLongVerMetrics = 1;            
            
		    int stGlyphNum = Maxp.numGlyphs;
            ushort usADH = (ushort)Head.unitsPerEm;
		    for (int i=0; i<stGlyphNum; i++)
		    {
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);

                short Tsb = (short)(OS2.sTypoAscender - ymax);
				VMTX_LONGHORMETRIC vmtxLongHormetric = new VMTX_LONGHORMETRIC();
                vmtxLongHormetric.advanceHeight = usADH;
				vmtxLongHormetric.tsb = Tsb;
				Vmtx.vtLongHormetric.Add(vmtxLongHormetric);
			    
		    }

		    Vhea.minTop = FindVertMinTop();
		    Vhea.minBottom = FindVertMinBottom();

		    Vhea.yMaxExtent			= (short)(Vhea.minTop+Head.yMax-Head.yMin);
		    Vhea.caretSlopeRise		= 0;
		    Vhea.caretSlopeRun		= 1;
		    Vhea.caretOffset		= 0;	//Set value equal to 0 for nonslanted fonts.
		    Vhea.reserved1			= 0;
		    Vhea.reserved2			= 0;
		    Vhea.reserved3			= 0;
		    Vhea.reserved4			= 0;
		    Vhea.metricDataFormat	= 0;

	    }	// end of void CountVheaVmtx()
        public HYRESULT Encodevhea()
        {
            CountVheaVmtx();
            
			ushort	usTmp = 0;			
            byte[]  btTmp;

            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.VHEA_TAG);
            if (iEntryIndex == -1) return HYRESULT.VHEA_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
			tbEntry.offset = (uint)this.FileStrm.Position;

			//Table version number
			Vhea.version.value = 1;
			Vhea.version.fract = 0;
			usTmp = hy_cdr_int16_to((ushort)Vhea.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);			
			usTmp = hy_cdr_int16_to((ushort)Vhea.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			// ascent
			usTmp = hy_cdr_int16_to((ushort)Vhea.ascent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			// descent
			usTmp = hy_cdr_int16_to((ushort)Vhea.descent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			// lineGap
			usTmp = hy_cdr_int16_to((ushort)Vhea.lineGap);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			// advanceHeightMax
			usTmp = hy_cdr_int16_to((ushort)Vhea.advanceHeightMax);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			// minTopSideBearing
			usTmp = hy_cdr_int16_to((ushort)Vhea.minTop);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//minBottomSideBearing
			usTmp = hy_cdr_int16_to((ushort)Vhea.minBottom);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//yMaxExtent
			usTmp = hy_cdr_int16_to((ushort)Vhea.yMaxExtent);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//caretSlopeRise
			usTmp = hy_cdr_int16_to((ushort)Vhea.caretSlopeRise);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);			
			// caretSlopeRun
			usTmp = hy_cdr_int16_to((ushort)Vhea.caretSlopeRun);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//caretOffset
			usTmp = hy_cdr_int16_to((ushort)Vhea.caretOffset);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//reserved
			usTmp = hy_cdr_int16_to((ushort)Vhea.reserved1);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//reserved
			usTmp = hy_cdr_int16_to((ushort)Vhea.reserved2);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//reserved
			usTmp = hy_cdr_int16_to((ushort)Vhea.reserved3);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//reserved
			usTmp = hy_cdr_int16_to((ushort)Vhea.reserved4);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//metricDataFormat
			usTmp = hy_cdr_int16_to((ushort)Vhea.metricDataFormat);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);
			//numOfLongVerMetrics
			usTmp = hy_cdr_int16_to((ushort)Vhea.numOfLongVerMetrics);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

			tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodevhea()

        public HYRESULT Encodevmtx()
        {
            ushort	usTmp = 0;			
            byte[]  bTmp;

			int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.VMTX_TAG);
			if (iEntryIndex == -1) return HYRESULT.VMTX_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
			tbEntry.offset = (uint)FileStrm.Position;

            for (int i = 0; i < Maxp.numGlyphs; i++)
            {
                if (i < Vhea.numOfLongVerMetrics)
                {
                    usTmp = hy_cdr_int16_to(Vmtx.vtLongHormetric[i].advanceHeight);
                    bTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(bTmp, 0, bTmp.Length);

                    usTmp = hy_cdr_int16_to((ushort)Vmtx.vtLongHormetric[i].tsb);
                    bTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(bTmp, 0, bTmp.Length);
                }
                else
                {
                    usTmp = hy_cdr_int16_to((ushort)Vmtx.vtLongHormetric[i].tsb);
                    bTmp = BitConverter.GetBytes(usTmp);
                    FileStrm.Write(bTmp, 0, bTmp.Length);
                }            
            }

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT Encodevmtx()

        public HYRESULT EndcodePrep()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.PREP_TAG);
            if (iEntryIndex == -1) return HYRESULT.PREP_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            byte[] PreData = { 0xb8, 0x01, 0xff, 0x85, 0xb8, 0x00,0x04,0x8d };

            FileStrm.Write(PreData,0,PreData.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT EndcodePrep()

        public HYRESULT EncodeCOLR()
	    {		    
			UInt16	usTmp = 0;
            UInt32  ultmp = 0;
            byte[]  bTmp;

            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.COLR_TAG);
            if (iEntryIndex == -1) return HYRESULT.COLR_ENCODE;
            
            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;			

            usTmp = hy_cdr_int16_to(Colr.version);
            bTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);			

            usTmp = hy_cdr_int16_to(Colr.numBaseGlyphRecords);
            bTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);

			long bGROffset = FileStrm.Position;
			ultmp = hy_cdr_int32_to(Colr.baseGlyphRecordsOffset);
			bTmp = BitConverter.GetBytes(ultmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);

			long lROffset = FileStrm.Position;
			ultmp = hy_cdr_int32_to(Colr.layerRecordsOffset);
			bTmp = BitConverter.GetBytes(ultmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);

			usTmp = hy_cdr_int16_to(Colr.numLayerRecords);
			bTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);
			 
			long BaseGlyphlth = 0;
			for (int i=0; i<Colr.numBaseGlyphRecords; i++)
			{
				CBaseGlyphRecord BaseGlyphRecord = Colr.lstBaseGlyphRecord[i];

				usTmp = hy_cdr_int16_to(BaseGlyphRecord.GID);
				bTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(bTmp, 0, bTmp.Length);

				usTmp = hy_cdr_int16_to(BaseGlyphRecord.firstLayerIndex);
				bTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(bTmp, 0, bTmp.Length);

				usTmp = hy_cdr_int16_to(BaseGlyphRecord.numLayers);
				bTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(bTmp, 0, bTmp.Length);

				BaseGlyphlth+=6;
			}
			
			for (int i=0; i<Colr.numLayerRecords; i++)
			{
                CLayerRecord LayerRecord = Colr.lstLayerRecord[i];

				usTmp = hy_cdr_int16_to(LayerRecord.GID);
                bTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(bTmp, 0, bTmp.Length);

				usTmp = hy_cdr_int16_to(LayerRecord.paletteIndex);
                bTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(bTmp, 0, bTmp.Length);				
			}

            long lTMP = FileStrm.Position;
			
            FileStrm.Seek(bGROffset,SeekOrigin.Begin);

            Colr.baseGlyphRecordsOffset = 14;
            ultmp = hy_cdr_int32_to(Colr.baseGlyphRecordsOffset);
            bTmp = BitConverter.GetBytes(ultmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);

            Colr.layerRecordsOffset = (uint)(14 + BaseGlyphlth);
            ultmp = hy_cdr_int32_to(Colr.layerRecordsOffset);
            bTmp = BitConverter.GetBytes(ultmp);
            FileStrm.Write(bTmp, 0, bTmp.Length);			

            FileStrm.Seek(lTMP, SeekOrigin.Begin);

            tbEntry.length = (uint)(FileStrm.Position - tbEntry.offset);
            AlignmentData(tbEntry.length);

		    return HYRESULT.NOERROR;

	    }	// end of int CHYFontCodec::EncodeCOLR()

        public HYRESULT EncodeDSIG()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.DSIG_TAG);
            if (iEntryIndex == -1) return HYRESULT.DSIG_ENCODE;

            DSIG = new CDSIG();

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            ushort usTmp = 0;
            uint uiTmp = 0;
            byte[] btTmp;
            
            uiTmp = hy_cdr_int32_to(DSIG.ulVersion);
            btTmp = BitConverter.GetBytes(uiTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            usTmp = hy_cdr_int16_to(DSIG.usNumSigs);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            usTmp = hy_cdr_int16_to(DSIG.usFlag);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);
            
            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeDSIG()

        public HYRESULT EncodeGasp()
        {
            int iEntryIndex = TableDirectorty.FindTableEntry((UInt32)TABLETAG.GASP_TAG);
            if (iEntryIndex == -1) return HYRESULT.GASP_ENCODE;

            CTableEntry tbEntry = TableDirectorty.vtTableEntry[iEntryIndex];
            tbEntry.offset = (uint)FileStrm.Position;

            ushort usTmp = 0;
            uint uiTmp = 0;
            byte[] btTmp;

            uiTmp = hy_cdr_int16_to(Gasp.Head.version);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            usTmp = hy_cdr_int16_to(Gasp.Head.numRanges);
            btTmp = BitConverter.GetBytes(usTmp);
            FileStrm.Write(btTmp, 0, btTmp.Length);

            for (int i = 0; i < Gasp.lstRangeRecord.Count; i++)
            {
                usTmp = hy_cdr_int16_to(Gasp.lstRangeRecord[i].rangeMaxPPEM);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);

                usTmp = hy_cdr_int16_to(Gasp.lstRangeRecord[i].rangeGaspBehavior);
                btTmp = BitConverter.GetBytes(usTmp);
                FileStrm.Write(btTmp, 0, btTmp.Length);
            }

            tbEntry.length = (uint)FileStrm.Position - tbEntry.offset;
            AlignmentData(tbEntry.length);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeGasp()

        public void MakeGasp_ClearType()
	    {
            Gasp = new CGasp();

            Gasp.Head = new CGasp_Head();
            Gasp.lstRangeRecord = new List<CGasp_RangeRecord>();

		    Gasp.Head.version       = 1;
		    Gasp.Head.numRanges     = 1;

		    CGasp_RangeRecord RangeRecord = new CGasp_RangeRecord();
		    RangeRecord.rangeMaxPPEM = 65535;
		    RangeRecord.rangeGaspBehavior = 0x000a;	//GASP_SYMMETRIC_SMOOTHING|GASP_SYMMETRIC_GRIDFIT 

		    Gasp.lstRangeRecord.Add(RangeRecord);

	    }	// end of int MakeGasp_ClearType()

        public UInt16 GetAdvancMaxWidth()
	    {            
		    UInt16 iAVDW= 0;
            for (int i = 0; i < GlyphChars.CharInfo.Count; i++)
            {
                if (GlyphChars.CharInfo[i].AdWidth > iAVDW)
                    iAVDW = (UInt16)GlyphChars.CharInfo[i].AdWidth;
            }
                
		    return iAVDW;

        }	// end of UInt16 GetAdvancMaxWidth()
        public int FindHeadXminGID()
	    {
            if (GlyphChars.CharInfo.Count > 0)
		    {
			    int iGID = 0;
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
			    int iXmin0 = xmin;

                for (int i = 1; i < GlyphChars.CharInfo.Count; i++)
			    {
                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
				    if (xmin<iXmin0)
				    {
                        iXmin0 = xmin;
					    iGID = i;
				    }
			    }

			    return iGID;
		    }
	
		    return -1;

	    }	// end of int FindHeadXminGID()

        public int FindHeadXmaxGID()
        {
            if (GlyphChars.CharInfo.Count > 0)
	        {
		        int iGID = 0;
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
                int iXmax0 = xmax;

                for (int i = 1; i < GlyphChars.CharInfo.Count; i++)
		        {
                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
                    if (xmax > iXmax0)
			        {
                        iXmax0 = xmax;
				        iGID = i;
			        }
		        }

		        return iGID;
	        }
	
	        return -1;

        }	// end of int FindHeadXmaxGID()
        public int FindHeadYminGID()
        {
            if (GlyphChars.CharInfo.Count > 0)
	        {
		        int iGID = 0;
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
		        int Ymin0 = ymin;

                for (int i = 1; i < GlyphChars.CharInfo.Count; i++)
		        {
                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
                    if (ymin < Ymin0)
			        {
                        Ymin0 = ymin;
				        iGID = i;
			        }
		        }
		        return iGID;
	        }
	        return -1;

        }	// end of int FindHeadYminGID()
        public int FindHeadYmaxGID()
	    {
            if (GlyphChars.CharInfo.Count > 0)
		    {
			    int iGID = 0;
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
                int ymax0 = ymax;
                for (int i = 1; i < GlyphChars.CharInfo.Count; i++)
			    {
                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
                    if (ymax > ymax0)
				    {
                        ymax0 = ymax;
					    iGID = i;
				    }
			    }
			    return iGID;
            }	
		    return -1;

	    }	// end of int CHYFontCodec::FindHeadYmaxGID()
        public short FindVertMinTop()
	    {	
		    short minTop = 0;
		    int stVerMetricsNum = Vhea.numOfLongVerMetrics;
		    if (stVerMetricsNum>0)
		    {
			    minTop = Vmtx.vtLongHormetric[0].tsb;
			    for (int i=1; i<stVerMetricsNum; i++)
			    {					
				    if (Vmtx.vtLongHormetric[i].tsb<minTop)  
				    {
					    minTop = Vmtx.vtLongHormetric[i].tsb;	
				    }	
			    }		    
		    }

		    return minTop;

	    }	// end of int FindVertMinTop()
        public short FindVertMinBottom()
	    {
		    short minBottom = 0;
            int stGlyphNum = GlyphChars.CharInfo.Count;
		    if (stGlyphNum>0)
		    {
                int xmin = 0, ymin = 0, xmax = 0, ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);
			    minBottom = (short)(ymin-OS2.sTypoDescender);
			    for (int i=0; i<stGlyphNum; i++)
			    {
                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
				    if (ymin<minBottom)  
				    {					
					    minBottom = (short)ymin;
				    }
			    }
		    }

		    return minBottom;

	    }	// end of int FindVertMinBottom()
        public int FindVertTopSideBearGID()
	    {
		    int iGID = -1;
		    int stVerMetricsNum = Vhea.numOfLongVerMetrics;		
		    if (stVerMetricsNum>0)
		    {
			    short minTop = Vmtx.vtLongHormetric[0].tsb;
			    for (int i=1; i<stVerMetricsNum; i++)
			    {					
				    if (Vmtx.vtLongHormetric[i].tsb<minTop)  
				    {
					    minTop = Vmtx.vtLongHormetric[i].tsb;
					    iGID = i;
				    }	
			    }
		    }

		    return iGID;

	    }	// end of int FindVertTopSideBearGID()
        public int FindVertBottomSideBearGID()
	    {
		    int iGID = -1;
            int stGlyphNum = GlyphChars.CharInfo.Count;
		    if (stGlyphNum>0)
		    {
                int xmin = 0,ymin = 0, xmax = 0,ymax = 0;
                BoundStringToInt(GlyphChars.CharInfo[0].Section, out xmin, out ymin, out xmax, out ymax);

                short minBottom = (short)(ymin - Hhea.Descender);
			    for (int i=1; i<stGlyphNum; i++){

                    BoundStringToInt(GlyphChars.CharInfo[i].Section, out xmin, out ymin, out xmax, out ymax);
                    if (ymin - Hhea.Descender < minBottom)  
				    {
                        minBottom = (short)(ymin - Hhea.Descender);
					    iGID = i;
				    }
			    }
		    }

		    return iGID;

	    }	// end of int CHYFontCodec::FindVertBottomSideBearGID()
        

        public void CountUnicodeRange(ref UInt32 uni1, ref UInt32 uni2, ref UInt32 uni3, ref UInt32 uni4, List<HYCodeMapItem> lstCodeMap)
	    {            
            uni1 = 0;
            uni2 = 0;
            uni3 = 0;
            uni4 = 0;

		    int  stCodeNum = lstCodeMap.Count;
		    for(int i=0; i<stCodeNum; i++)
		    {
			    HYCodeMapItem MapItem = lstCodeMap[i];
			    
				uint unicode = MapItem.Unicode;
				//Basic Latin  0
				if (unicode<=0x007F)
				{
					uni1 |= 0x00000001;
					continue;
				}
				//Latin-1 Supplement	1
				if (unicode>=0x0080 && unicode<=0x00FF)
				{
					uni1 |= 0x00000002;					
					continue;
				}
				//Latin Extended-A		2
				if (unicode>=0x0100 && unicode<=0x017F)
				{
					uni1 |= 0x00000004;
					continue;
				}
				//Latin Extended-B		3
				if (unicode>=0x0180 && unicode<=0x024F)
				{
					uni1 |= 0x00000008;
					continue;
				}				
				//	IPA Extensions		4
				if (unicode>=0x0250 && unicode<=0x02AF)
				{
					uni1 |= 0x00000010;
					continue;
				}
				//	Phonetic Extensions 4
				if (unicode>=0x1D00 && unicode<=0x1D7F)
				{
					uni1 |= 0x00000010;
					continue;
				}
				//	Phonetic Extensions Supplement 4
				if (unicode>=0x1D80 && unicode<=0x1DBF)
				{
					uni1 |= 0x00000010;
					continue;
				}
				//Spacing Modifier Letters 5
				if (unicode>=0x02B0 && unicode<=0x02FF)
				{
					uni1 |= 0x00000020;
					continue;
				}
				//Modifier Tone Letters 5
				if (unicode>=0xA700 && unicode<=0xA71F)
				{
					uni1 |= 0x00000020;
					continue;
				}
				//Combining Diacritical Marks 6
				if (unicode>=0x0300 && unicode<=0x036F)
				{
					uni1 |= 0x00000040;
					continue;
				}
				//Combining Diacritical Marks Supplement 6
				if (unicode>=0x1DC0 && unicode<=0x1DFF)
				{
					uni1 |= 0x00000040;
					continue;
				}
				//Greek and Coptic 7
				if (unicode>=0x0370 && unicode<=0x03FF)
				{
					uni1 |= 0x00000080;
					continue;
				}
				//Coptic 8
				if (unicode>=0x2C80 && unicode<=0x2CFF)
				{
					uni1 |= 0x00000100;
					continue;
				}
				// 	Cyrillic 9
				if (unicode>=0x0400&& unicode<=0x04FF)
				{
					uni1 |= 0x00000200;
					continue;
				}
				// Cyrillic Supplement 9
				if (unicode>=0x0500&& unicode<=0x052F)
				{
					uni1 |= 0x00000200;
					continue;
				}
				//Cyrillic Extended-A 9
				if (unicode>=0xA640&& unicode<=0xA69F)
				{
					uni1 |= 0x00000200;
					continue;
				}
				//Cyrillic Extended-B 9
				if (unicode>=0xA640&& unicode<=0xA69F)
				{
					uni1 |= 0x00000200;
					continue;
				}
				//Armenian	10
				if (unicode>=0x0530&& unicode<=0x058F)
				{
					uni1 |= 0x00000400;
					continue;
				}
				//Hebrew	11
				if (unicode>=0x0590&& unicode<=0x05FF)
				{
					uni1 |= 0x00000800;
					continue;
				}
				// Vai	12
				if (unicode>=0xA500&& unicode<=0xA63F)
				{
					uni1 |= 0x00001000;
					continue;
				}
				// Arabic	13
				if (unicode>=0x0600&& unicode<=0x06FF)
				{
					uni1 |= 0x00002000;
					continue;
				}
				// Arabic  Supplement	13
				if (unicode>=0x0750&& unicode<=0x077F)
				{
					uni1 |= 0x00002000;
					continue;
				}
				// NKo 14
				if (unicode>=0x07C0&& unicode<=0x07FF)
				{
					uni1 |= 0x00004000;
					continue;
				}
				// Devanagari 15
				if (unicode>=0x0900&& unicode<=0x097F)
				{
					uni1 |= 0x00008000;
					continue;
				}
				//Bengali 16
				if (unicode>=0x0980&& unicode<=0x09FF)
				{
					uni1 |= 0x00010000;
					continue;
				}
				//Gurmukhi	17
				if (unicode>=0x0A00&& unicode<=0x0A7F)
				{
					uni1 |= 0x00020000;
					continue;
				}
				//Gujarati 18
				if (unicode>=0x0A80&& unicode<=0x0AFF)
				{
					uni1 |= 0x00040000;
					continue;
				}
				//Oriya 19
				if (unicode>=0x0B00&& unicode<=0x0B7F)
				{
					uni1 |= 0x00080000;
					continue;
				}
				//Tamil 20
				if (unicode>=0x0B80&& unicode<=0x0BFF)
				{
					uni1 |= 0x00100000;
					continue;
				}
				//Telugu 21
				if (unicode>=0x0C00&& unicode<=0x0C7F)
				{
					uni1 |= 0x00200000;
					continue;
				}
				//Kannada 22
				if (unicode>=0x0C80&& unicode<=0x0CFF)
				{
					uni1 |= 0x00400000;
					continue;
				}
				//Malayalam 23
				if (unicode>=0x0D00&& unicode<=0x0D7F)
				{
					uni1 |= 0x00800000;
					continue;
				}
				//Thai 24
				if (unicode>=0x0E00&& unicode<=0x0E7F)
				{
					uni1 |= 0x01000000;
					continue;
				}
				//Lao 25
				if (unicode>=0x0E80&& unicode<=0x0EFF)
				{
					uni1 |= 0x02000000;
					continue;
				}
				//Georgian 26
				if (unicode>=0x10A0&& unicode<=0x10FF)
				{
					uni1 |= 0x04000000;
					continue;
				}
				//Georgian Supplement	26
				if (unicode>=0x2D00&& unicode<=0x2D2F)
				{
					uni1 |= 0x04000000;
					continue;
				}
				//Balinese	27
				if (unicode>=0x2D00&& unicode<=0x2D2F)
				{
					uni1 |= 0x08000000;
					continue;
				}
				//Hangul Jamo	28
				if (unicode>=0x1100&& unicode<=0x11FF)
				{
					uni1 |= 0x10000000;
					continue;
				}
				//Latin Extended Additional		29
				if (unicode>=0x1E00&& unicode<=0x1EFF)
				{
					uni1 |= 0x20000000;
					continue;
				}
				//Latin Extended-C			29
				if (unicode>=0x2C60&& unicode<=0x2C7F)
				{
					uni1 |= 0x20000000;
					continue;
				}
				//Latin Extended-D		29
				if (unicode>=0xA720&& unicode<=0xA7FF)
				{
					uni1 |= 0x20000000;
					continue;
				}
				//Greek Extended		30
				if (unicode>=0x1F00&& unicode<=0x1FFF)
				{
					uni1 |= 0x40000000;
					continue;
				}
				//General Punctuation	31
				if (unicode>=0x2000&& unicode<=0x206F)
				{
					uni1 |= 0x80000000;
					continue;
				}
				//Supplemental Punctuation	31
				if (unicode>=0x2E00&& unicode<=0x2E7F)
				{
					uni1 |= 0x80000000;
					continue;
				}
				//Superscripts And Subscripts 32
				if (unicode>=0x2070&& unicode<=0x209F)
				{
					uni2 |= 0x00000001;
					continue;
				}
				//Currency Symbols	 33
				if (unicode>=0x20A0&& unicode<=0x20CF)
				{
					uni2 |= 0x00000002;
					continue;
				}
				//Combining Diacritical Marks For Symbols	 34
				if (unicode>=0x20D0&& unicode<=0x20FF)
				{
					uni2 |= 0x00000004;
					continue;
				}
				//Letterlike Symbols	35
				if (unicode>=0x2100&& unicode<=0x214F)
				{
					uni2 |= 0x00000008;
					continue;
				}
				//Number Forms	36
				if (unicode>=0x2150&& unicode<=0x218F)
				{
					uni2 |= 0x00000010;
					continue;
				}
				//Arrows	37
				if (unicode>=0x2190&& unicode<=0x21FF)
				{
					uni2 |= 0x00000020;
					continue;
				}
				//	Supplemental Arrows-A 37
				if (unicode>=0x27F0&& unicode<=0x27FF)
				{
					uni2 |= 0x00000020;
					continue;
				}
				//	Supplemental Arrows-B 37
				if (unicode>=0x2900 && unicode<=0x297F)
				{
					uni2 |= 0x00000020;
					continue;
				}
				//	Miscellaneous Symbols and Arrows  37
				if (unicode>=0x2B00 && unicode<=0x2BFF)
				{
					uni2 |= 0x00000020;
					continue;
				}
				//	Mathematical Operators	 38
				if (unicode>=0x2200 && unicode<=0x22FF)
				{
					uni2 |= 0x00000040;
					continue;
				}
				//	Supplemental Mathematical Operators	 38
				if (unicode>=0x2A00 && unicode<=0x2AFF)
				{
					uni2 |= 0x00000040;
					continue;
				}
				//	Miscellaneous Mathematical Symbols-A  38
				if (unicode>=0x27C0 && unicode<=0x27EF)
				{
					uni2 |= 0x00000040;
					continue;
				}
				//	Miscellaneous Mathematical Symbols-B  38
				if (unicode>=0x2980 && unicode<=0x29FF)
				{
					uni2 |= 0x00000040;
					continue;
				}
				//	Miscellaneous Mathematical Symbols-B  38
				if (unicode>=0x2980 && unicode<=0x29FF)
				{
					uni2 |= 0x00000040;
					continue;
				}
				//	Miscellaneous Technical	39
				if (unicode>=0x2300 && unicode<=0x23FF)
				{
					uni2 |= 0x00000080;
					continue;
				}
				//	Control Pictures	40
				if (unicode>=0x2400 && unicode<=0x243F)
				{
					uni2 |= 0x00000100;
					continue;
				}				
				//	Optical Character Recognition	41
				if (unicode>=0x2440 && unicode<=0x245F)
				{
					uni2 |= 0x00000200;
					continue;
				}
				//42	Enclosed Alphanumerics
				if (unicode>=0x2460 && unicode<=0x24FF)
				{
					uni2 |= 0x00000400;
					continue;
				}
				//43	Box Drawing
				if (unicode>=0x2500 && unicode<=0x257F)
				{
					uni2 |= 0x00000800;
					continue;
				}
				//44	Block Elements
				if (unicode>=0x2580 && unicode<=0x259F)
				{
					uni2 |= 0x00001000;
					continue;
				}
				//45	Geometric  Shapes
				if (unicode>=0x25A0 && unicode<=0x25FF)
				{
					uni2 |= 0x00002000;
					continue;
				}
				//46	Miscellaneous Symbols
				if (unicode>=0x2600 && unicode<=0x26FF)
				{
					uni2 |= 0x00004000;
					continue;
				}
				//47	Dingbats
				if (unicode>=0x2700 && unicode<=0x27BF)
				{
					uni2 |= 0x00008000;
					continue;
				}
				//48	CJK Symbols And Punctuation
				if (unicode>=0x3000 && unicode<=0x303F)
				{
					uni2 |= 0x00010000;
					continue;
				}
				//49	Hiragana	3040-309F
				if (unicode>=0x3040 && unicode<=0x309F)
				{
					uni2 |= 0x00020000;
					continue;
				}
				//50	Katakana	
				if (unicode>=0x30A0 && unicode<=0x30FF)
				{
					uni2 |= 0x00040000;
					continue;
				}
				//Katakana Phonetic Extensions
				if (unicode>=0x31F0 && unicode<=0x31FF)
				{
					uni2 |= 0x00040000;
					continue;
				}
				//51	Bopomofo
				if (unicode>=0x3100 && unicode<=0x312F)
				{
					uni2 |= 0x00080000;
					continue;
				}
				//Bopomofo Extended
				if (unicode>=0x31A0 && unicode<=0x31BF)
				{
					uni2 |= 0x00080000;
					continue;
				}
				//52	Hangul Compatibility Jamo
				if (unicode>=0x3130 && unicode<=0x318F)
				{
					uni2 |= 0x00100000;
					continue;
				}
				//53	Phags-pa
				if (unicode>=0xA840 && unicode<=0xA87F)
				{
					uni2 |= 0x00200000;
					continue;
				}
				//54	Enclosed CJK Letters And Months
				if (unicode>=0x3200 && unicode<=0x32FF)
				{
					uni2 |= 0x00400000;
					continue;
				}
				//55	CJK Compatibility
				if (unicode>=0x3300 && unicode<=0x33FF)
				{
					uni2 |= 0x00800000;
					continue;
				}				
				//56	Hangul Syllables	
				if (unicode>=0xAC00 && unicode<=0xD7AF)
				{
					uni2 |= 0x01000000;
					continue;
				}				
				
				//57	Non-Plane 0 *	D800-DFFF
				if (unicode>=0xD800 && unicode<=0xDFFF)
				{
					uni2 |= 0x02000000;
					continue;
				}
				//58	Phoenician	10900-1091F
				if (unicode>=0x10900 && unicode<=0x1091F)
				{
					uni2 |= 0x04000000;
					continue;
				}
				//59	CJK Unified Ideographs	4E00-9FFF
				if (unicode>=0x4E00 && unicode<=0x9FFF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//CJK Radicals Supplement
				if (unicode>=0x2E80 && unicode<=0x2EFF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//Kangxi Radicals	2F00-2FDF
				if (unicode>=0x2F00 && unicode<=0x2FDF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//Ideographic Description Characters	2FF0-2FFF
				if (unicode>=0x2FF0 && unicode<=0x2FFF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//CJK Unified Ideographs Extension A	3400-4DBF
				if (unicode>=0x3400 && unicode<=0x4DBF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//CJK Unified Ideographs Extension B	20000-2A6DF
				if (unicode>=0x20000 && unicode<=0x2A6DF)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//Kanbun	3190-319F
				if (unicode>=0x3190 && unicode<=0x319F)
				{
					uni2 |= 0x08000000;
					continue;
				}
				//60	Private Use Area (plane 0)	E000-F8FF
				if (unicode>=0xE000 && unicode<=0xF8FF)
				{
					uni2 |= 0x10000000;
					continue;
				}
				//61	CJK Strokes	31C0-31EF
				if (unicode>=0x31C0 && unicode<=0x31EF)
				{
					uni2 |= 0x20000000;
					continue;
				}
				//CJK Compatibility Ideographs	F900-FAFF
				if (unicode>=0xF900 && unicode<=0xFAFF)
				{
					uni2 |= 0x20000000;
					continue;
				}
				//CJK Compatibility Ideographs Supplement	2F800-2FA1F
				if (unicode>=0x2F800 && unicode<=0x2FA1F)
				{
					uni2 |= 0x20000000;
					continue;
				}
				//62	Alphabetic Presentation Forms	FB00-FB4F
				if (unicode>=0xFB00 && unicode<=0xFB4F)
				{
					uni2 |= 0x40000000;
					continue;
				}
				//63	Arabic Presentation Forms-A	FB50-FDFF
				if (unicode>=0xFB50 && unicode<=0xFDFF)
				{
					uni2 |= 0x80000000;
					continue;
				}
				//64	Combining Half Marks	FE20-FE2F
				if (unicode>=0xFE20 && unicode<=0xFE2F)
				{
					uni3 |= 0x00000001;
					continue;
				}
				//65	Vertical Forms	FE10-FE1F
				if (unicode>=0xFE10 && unicode<=0xFE1F)
				{
					uni3 |= 0x00000002;
					continue;
				}
				//CJK Compatibility Forms	FE30-FE4F
				if (unicode>=0xFE30 && unicode<=0xFE4F)
				{
					uni3 |= 0x00000002;
					continue;
				}
				//66	Small Form Variants	FE50-FE6F
				if (unicode>=0xFE50 && unicode<=0xFE6F)
				{
					uni3 |= 0x00000004;
					continue;
				}
				//67	Arabic Presentation Forms-B	FE70-FEFF
				if (unicode>=0xFE70 && unicode<=0xFEFF)
				{
					uni3 |= 0x00000008;
					continue;
				}
				//68	Halfwidth And Fullwidth Forms	FF00-FFEF
				if (unicode>=0xFF00 && unicode<=0xFFEF)
				{
					uni3 |= 0x00000010;
					continue;
				}
				//69	Specials	FFF0-FFFF
				if (unicode>=0xFFF0 && unicode<0xFFFF)
				{
					uni3 |= 0x00000020;
					continue;
				}				
				//70	Tibetan	0F00-0FFF
				if (unicode>=0x0F00 && unicode<=0x0FFF)
				{
					uni3 |= 0x00000040;
					continue;
				}
				//71	Syriac	0700-074F
				if (unicode>=0x0700 && unicode<=0x074F)
				{
					uni3 |= 0x00000080;
					continue;
				}
				//72	Thaana	0780-07BF
				if (unicode>=0x0700 && unicode<=0x07BF)
				{
					uni3 |= 0x00000100;
					continue;
				}
				//73	Sinhala	0D80-0DFF
				if (unicode>=0x0D80 && unicode<=0x0DFF)
				{
					uni3 |= 0x00000200;
					continue;
				}
				//74	Myanmar	1000-109F
				if (unicode>=0x1000 && unicode<=0x109F)
				{
					uni3 |= 0x00000400;
					continue;
				}
				//75	Ethiopic	1200-137F
				if (unicode>=0x1200 && unicode<=0x137F)
				{
					uni3 |= 0x00000800;
					continue;
				}
				//Ethiopic Supplement	1380-139F
				if (unicode>=0x1380 && unicode<=0x139F)
				{
					uni3 |= 0x00000800;
					continue;
				}
				//Ethiopic Extended	2D80-2DDF
				if (unicode>=0x2D80 && unicode<=0x2DDF)
				{
					uni3 |= 0x00000800;
					continue;
				}
				//76	Cherokee	13A0-13FF
				if (unicode>=0x13A0 && unicode<=0x13FF)
				{
					uni3 |= 0x00001000;
					continue;
				}
				//77	Unified Canadian Aboriginal Syllabics	1400-167F
				if (unicode>=0x1400 && unicode<=0x167F)
				{
					uni3 |= 0x00002000;
					continue;
				}
				//78	Ogham	1680-169F
				if (unicode>=0x1680 && unicode<=0x169F)
				{
					uni3 |= 0x00004000;
					continue;
				}
				//79	Runic	16A0-16FF
				if (unicode>=0x16A0 && unicode<=0x16FF)
				{
					uni3 |= 0x00008000;
					continue;
				}
				//80	Khmer	1780-17FF
				if (unicode>=0x1780 && unicode<=0x17FF)
				{
					uni3 |= 0x00010000;
					continue;
				}
				//Khmer Symbols	19E0-19FF
				if (unicode>=0x19E0 && unicode<=0x19FF)
				{
					uni3 |= 0x00010000;
					continue;
				}
				//81	Mongolian	1800-18AF
				if (unicode>=0x1800 && unicode<=0x18AF)
				{
					uni3 |= 0x00020000;
					continue;
				}
				//82	Braille Patterns	2800-28FF
				if (unicode>=0x2800 && unicode<=0x28FF)
				{
					uni3 |= 0x00040000;
					continue;
				}
				//83	Yi Syllables	A000-A48F
				if (unicode>=0xA000 && unicode<=0xA48F)
				{
					uni3 |= 0x00080000;
					continue;
				}
				//Yi Radicals	A490-A4CF
				if (unicode>=0xA490 && unicode<=0xA4CF)
				{
					uni3 |= 0x00080000;
					continue;
				}
				//84	Tagalog	1700-171F
				if (unicode>=0x1700 && unicode<=0x171F)
				{
					uni3 |= 0x00100000;
					continue;
				}
				//Hanunoo	1720-173F
				if (unicode>=0x1720 && unicode<=0x173F)
				{
					uni3 |= 0x00100000;
					continue;
				}
				//Buhid	1740-175F
				if (unicode>=0x1740 && unicode<=0x175F)
				{
					uni3 |= 0x00100000;
					continue;
				}
				//Tagbanwa	1760-177F
				if (unicode>=0x1760 && unicode<=0x177F)
				{
					uni3 |= 0x00100000;
					continue;
				}
				//85	Old Italic	10300-1032F
				if (unicode>=0x10300 && unicode<=0x1032F)
				{
					uni3 |= 0x00200000;
					continue;
				}
				//86	Gothic	10330-1034F
				if (unicode>=0x10330 && unicode<=0x1034F)
				{
					uni3 |= 0x00400000;
					continue;
				}
				//87	Deseret	10400-1044F
				if (unicode>=0x10400 && unicode<=0x1044F)
				{
					uni3 |= 0x00800000;
					continue;
				}
				//88	Byzantine Musical Symbols	1D000-1D0FF
				if (unicode>=0x1D000 && unicode<=0x1D0FF)
				{
					uni3 |= 0x01000000;
					continue;
				}
				//Musical Symbols	1D100-1D1FF
				if (unicode>=0x1D100 && unicode<=0x1D1FF)
				{
					uni3 |= 0x01000000;
					continue;
				}
				//Ancient Greek Musical Notation	1D200-1D24F
				if (unicode>=0x1D200 && unicode<=0x1D24F)
				{
					uni3 |= 0x01000000;
					continue;
				}
				//89	Mathematical Alphanumeric Symbols	1D400-1D7FF
				if (unicode>=0x1D400 && unicode<=0x1D7FF)
				{
					uni3 |= 0x02000000;
					continue;
				}
				//90	Private Use (plane 15)	FF000-FFFFD
				if (unicode>=0xFF000 && unicode<=0xFFFFD)
				{
					uni3 |= 0x04000000;
					continue;
				}
				//Private Use (plane 16)	100000-10FFFD
				if (unicode>=0x100000 && unicode<=0x10FFFD)
				{
					uni3 |= 0x04000000;
					continue;
				}
				//91	Variation Selectors	FE00-FE0F
				if (unicode>=0xFE00&& unicode<=0xFE0F)
				{
					uni3 |= 0x08000000;
					continue;
				}
				//Variation Selectors Supplement	E0100-E01EF
				if (unicode>=0xFE00&& unicode<=0xFE0F)
				{
					uni3 |= 0x08000000;
					continue;
				}
				//92	Tags	E0000-E007F
				if (unicode>=0xE0000&& unicode<=0xE007F)
				{
					uni3 |= 0x10000000;
					continue;
				}
				//93	Limbu	1900-194F
				if (unicode>=0x1900&& unicode<=0x194F)
				{
					uni3 |= 0x20000000;
					continue;
				}
				//94	Tai Le	1950-197F
				if (unicode>=0x1950&& unicode<=0x197F)
				{
					uni3 |= 0x40000000;
					continue;
				}
				//95	New Tai Lue	1980-19DF
				if (unicode>=0x1980&& unicode<=0x19DF)
				{
					uni3 |= 0x80000000;
					continue;
				}
				//96	Buginese	1A00-1A1F
				if (unicode>=0x1A00 && unicode<=0x1A1F)
				{
					uni4 |= 0x00000001;
					continue;
				}
				
				//97	Glagolitic	2C00-2C5F
				if (unicode>=0x2C00 && unicode<=0x2C5F)
				{
					uni4 |= 0x00000002;
					continue;
				}
				//98	Tifinagh	2D30-2D7F
				if (unicode>=0x2D30 && unicode<=0x2D7F)
				{
					uni4 |= 0x00000004;
					continue;
				}
				//99	Yijing Hexagram Symbols	4DC0-4DFF
				if (unicode>=0x4DC0 && unicode<=0x4DFF)
				{
					uni4 |= 0x00000008;
					continue;
				}
				//100	Syloti Nagri	A800-A82F
				if (unicode>=0xA800 && unicode<=0xA82F)
				{
					uni4 |= 0x00000010;
					continue;
				}
				//101	Linear B Syllabary	10000-1007F
				if (unicode>=0x10000 && unicode<=0x1007F)
				{
					uni4 |= 0x00000020;
					continue;
				}
				//Linear B Ideograms	10080-100FF
				if (unicode>=0x10080 && unicode<=0x100FF)
				{
					uni4 |= 0x00000020;
					continue;
				}
				//Aegean Numbers	10100-1013F
				if (unicode>=0x10100 && unicode<=0x1013F)
				{
					uni4 |= 0x00000020;
					continue;
				}
				//102	Ancient Greek Numbers	10140-1018F
				if (unicode>=0x10140 && unicode<=0x1018F)
				{
					uni4 |= 0x00000040;
					continue;
				}
				//103	Ugaritic	10380-1039F
				if (unicode>=0x10380 && unicode<=0x1039F)
				{
					uni4 |= 0x00000080;
					continue;
				}
				//104	Old Persian	103A0-103DF
				if (unicode>=0x103A0 && unicode<=0x103DF)
				{
					uni4 |= 0x00000100;
					continue;
				}
				//105	Shavian	10450-1047F
				if (unicode>=0x10450 && unicode<=0x1047F)
				{
					uni4 |= 0x00000200;
					continue;
				}
				//106	Osmanya	10480-104AF
				if (unicode>=0x10480 && unicode<=0x104AF)
				{
					uni4 |= 0x00000400;
					continue;
				}
				//107	Cypriot Syllabary	10800-1083F
				if (unicode>=0x10800 && unicode<=0x1083F)
				{
					uni4 |= 0x00000800;
					continue;
				}
				//108	Kharoshthi	10A00-10A5F
				if (unicode>=0x10A00 && unicode<=0x10A5F)
				{
					uni4 |= 0x00001000;
					continue;
				}
				//109	Tai Xuan Jing Symbols	1D300-1D35F
				if (unicode>=0x1D300 && unicode<=0x1D35F)
				{
					uni4 |= 0x00002000;
					continue;
				}
				//110	Cuneiform	12000-123FF
				if (unicode>=0x12000 && unicode<=0x123FF)
				{
					uni4 |= 0x00004000;
					continue;
				}
				//Cuneiform Numbers and Punctuation	12400-1247F
				if (unicode>=0x12400 && unicode<=0x1247F)
				{
					uni4 |= 0x00004000;
					continue;
				}
				//111	Counting Rod Numerals	1D360-1D37F
				if (unicode>=0x1D360 && unicode<=0x1D37F)
				{
					uni4 |= 0x00008000;
					continue;
				}
				//112	Sundanese	1B80-1BBF
				if (unicode>=0x1B80 && unicode<=0x1BBF)
				{
					uni4 |= 0x00010000;
					continue;
				}
				//113	Lepcha	1C00-1C4F
				if (unicode>=0x1C00 && unicode<=0x1C4F)
				{
					uni4 |= 0x00020000;
					continue;
				}
				//114	Ol Chiki	1C50-1C7F
				if (unicode>=0x1C50 && unicode<=0x1C7F)
				{
					uni4 |= 0x00040000;
					continue;
				}
				//115	Saurashtra	A880-A8DF
				if (unicode>=0xA880 && unicode<=0xA8DF)
				{
					uni4 |= 0x00080000;
					continue;
				}
				//116	Kayah Li	A900-A92F
				if (unicode>=0xA900 && unicode<=0xA92F)
				{
					uni4 |= 0x00100000;
					continue;
				}
				//117	Rejang	A930-A95F
				if (unicode>=0xA930 && unicode<=0xA95F)
				{
					uni4 |= 0x00200000;
					continue;
				}
				//118	Cham	AA00-AA5F
				if (unicode>=0xAA00 && unicode<=0xAA5F)
				{
					uni4 |= 0x00400000;
					continue;
				}
				//119	Ancient Symbols	10190-101CF
				if (unicode>=0x10190 && unicode<=0x101CF)
				{
					uni4 |= 0x00800000;
					continue;
				}
				//120	Phaistos Disc	101D0-101FF
				if (unicode>=0x101D0 && unicode<=0x101FF)
				{
					uni4 |= 0x01000000;
					continue;
				}
				//121	Carian	102A0-102DF
				if (unicode>=0x102A0 && unicode<=0x102DF)
				{
					uni4 |= 0x02000000;
					continue;
				}
				//Lycian	10280-1029F
				if (unicode>=0x10280 && unicode<=0x1029F)
				{
					uni4 |= 0x02000000;
					continue;
				}
				//Lydian	10920-1093F
				if (unicode>=0x10920 && unicode<=0x1093F)
				{
					uni4 |= 0x02000000;
					continue;
				}				
				//122	Domino Tiles	1F030-1F09F
				if (unicode>=0x1F030 && unicode<=0x1F09F)
				{
					uni4 |= 0x04000000;
					continue;
				}
				if (unicode>=0x1F000 && unicode<=0x1F02F)
				{
					uni4 |= 0x04000000;
					continue;
				}
		    }

	    }	// end of void	CHYFontCodec::CountUnicodeRange()

        public UInt32 CalcFontTableChecksum(byte[] btFile)
        {
            UInt32 Sum = 0;
            Int32 Index = 0;
            Int32 length = ((btFile.Length + 3) & ~3) / sizeof(Int32);

            for (int i = 0; i < length; i++ )
            {
                byte[] btCnv = new byte[4] {0,0,0,0};
                btCnv[0] = btFile[Index++];                
                btCnv[1] = btFile[Index++];                
                btCnv[2] = btFile[Index++];
                btCnv[3] = btFile[Index++];
                btCnv = btCnv.Reverse().ToArray();
                Sum += BitConverter.ToUInt32(btCnv, 0);
            }

            return Sum;

        }	// end of unsigned long CalcFontTableChecksum()

        public void CalcFontTableChecksum(FileStream fstm, ref CTableEntry HYEntry)
        {   
            uint CheckBufSz = (HYEntry.length + 3) / 4;
            fstm.Seek(HYEntry.offset, SeekOrigin.Begin);
            UInt32 Sum = 0;
            for (int i = 0; i < CheckBufSz; i++)
            {   
                byte[] btCnv = new byte[4] { 0, 0, 0, 0 };
                fstm.Read(btCnv,0,4);
                Sum += BitConverter.ToUInt32(btCnv, 0);
            }
            HYEntry.checkSum = Sum;

        }   // end of public UInt32 CalcFontTableChecksum()

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

        public HYRESULT SetCheckSumAdjustment(string FontFile)
        {
            try
            {
                FileStrm = new FileStream(FontFile, FileMode.Open, FileAccess.ReadWrite,FileShare.None);

                byte[] btFile = new byte[FileStrm.Length];
                FileStrm.Read(btFile, 0, (int)FileStrm.Length);

                UInt32 ulcheckSum = CalcFontTableChecksum(btFile);
                ulcheckSum = 0xB1B0AFBA - ulcheckSum;
                ulcheckSum = hy_cdr_int32_to(ulcheckSum);

                byte[] array = new byte[4];
                FileStrm.Seek(4, SeekOrigin.Begin);
                //numTables
                FileStrm.Read(array, 0, 2);
                int numTables = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
                FileStrm.Seek(6, SeekOrigin.Current);

                for (UInt16 i = 0; i < numTables; i++)
                {
                    CTableEntry HYEntry = new CTableEntry();
                    //tag		
                    FileStrm.Read(array, 0, 4);
                    HYEntry.tag = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
                    // checkSum
                    FileStrm.Read(array, 0, 4);
                    HYEntry.checkSum = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
                    //offset
                    FileStrm.Read(array, 0, 4);
                    HYEntry.offset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
                    //length
                    FileStrm.Read(array, 0, 4);
                    HYEntry.length = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

                    if (HYEntry.tag.Equals(TABLETAG.HEAD_TAG))
                    {
                        FileStrm.Seek(HYEntry.offset + 8, SeekOrigin.Begin);

                        byte[] btTmp = BitConverter.GetBytes(ulcheckSum);
                        FileStrm.Write(btTmp, 0, btTmp.Length);
                        break;
                    }
                }

                FileStrm.Flush();
                FileStrm.Close();
            }
            catch (Exception ext)
            {
                ext.ToString();
                if (FileStrm != null)
                {
                    FileStrm.Close();                
                }
            
            }
            return HYRESULT.NOERROR;

        }   // end of protected HYRESULT SetCheckSumAdjustment()        
       
    }
}
