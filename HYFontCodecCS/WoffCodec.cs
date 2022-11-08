using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ZLibNet;


namespace HYFontCodecCS
{
    public class WOFFTableDictEntry
	{
		public uint		tag;			//4-byte sfnt table identifier.
		public uint     offset;			//Offset to the data, from beginning of WOFF file.
		public uint		complength;		//Length of the compressed data, excluding padding.
		public uint		origlength;		//Length of the uncompressed table, excluding padding.
		public uint		origchecksum;	//Checksum of the uncompressed table.
	};

    public class WoffCodec : HYBase
    {
        //private byte[] SfntBuf;        
        private byte[] PrivteBuf = null;
        private byte[] OrigMetaBuf = null;
        private byte[] CmprsMetaBuf = null;
        private List<byte[]> lstSfntOrigtbBuf;
        private List<byte[]> lstSfntcmprstbBuf;

        private List<WOFFTableDictEntry> lstWoffTblDict;
        private HYDecode FontDecode;

        public HYRESULT Woff2Sfnt(string strWoffFile, string strSfntFile)
        {            
            HYEncode FntEncode = new HYEncode();

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT Woff2Sfnt()

        public HYRESULT DecodeWoff(string WoffFile, ref HYEncode Encode)
        {
            
            FileInfo wofffile = new FileInfo(WoffFile);
            FileStream strmWoff = wofffile.OpenRead();
            HYRESULT hr = DecodeWoffHeader(strmWoff, ref Encode);
            if (hr != HYRESULT.NOERROR) return hr;
            hr = DecodeWoffTableDictr(strmWoff, ref Encode);



            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeWoff()

        public HYRESULT DecodeWoffHeader(FileStream strmWoff, ref HYEncode Encode)
        {            
            byte[] array = new byte[4];

            //WOFFHeader
            strmWoff.Read(array, 0, 4);
            UInt32 iSign = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            if (iSign != 0x774F4646) return HYRESULT.WOFF_DECODE;
            //sfnt version
            strmWoff.Read(array, 0, 4);
            UInt32 iVersin = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            Encode.tbDirectory.version.value = (short)(iVersin >> 16);
            Encode.tbDirectory.version.fract = (ushort)(iVersin & 0x0000ffff);
            //wofflength
            strmWoff.Read(array, 0, 4);
            UInt32 iwoffLength = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //numTables
            strmWoff.Read(array, 0, 2);
            Encode.tbDirectory.numTables = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //reserved
            strmWoff.Seek(2, SeekOrigin.Current);
            //totalSfntSize            
            UInt32 iFntLngth = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            byte[] Fntbuf = new byte[iFntLngth];

            //majorVersion
            strmWoff.Read(array, 0, 2);
            ushort sWfmjrVersion = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //minorVersion
            strmWoff.Read(array, 0, 2);
            ushort sWfmnrVersion = hy_cdr_int16_to(BitConverter.ToUInt16(array, 0));
            //metaOffset
            strmWoff.Read(array, 0, 4);
            UInt32 uMetaOffset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //metaLength
            UInt32 uMetaLength = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //metaOrigLength
            UInt32 uMetaOrigLength = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //privOffset
            UInt32 uprivOffset = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));
            //privLength
            UInt32 uprivLength = hy_cdr_int32_to(BitConverter.ToUInt32(array, 0));

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeWoffHeader()

        public HYRESULT DecodeWoffTableDictr(FileStream strmWoff, ref HYEncode Encode)
        {



            return HYRESULT.NOERROR;

        }   // end of public HYRESULT DecodeWoffTableDictr()

        public HYRESULT Sfnt2Woff(string SfntFile, string WoffFile, string MetadataFile)
        {
            HYRESULT result;
            try
            {
                if (MetadataFile.Length > 0)
                {
                    FileInfo fileInf = new FileInfo(MetadataFile);
                    if (MetadataFile.Length > 0)
                    {
                        OrigMetaBuf = new byte[MetadataFile.Length];
                        FileStream MetaFile = new FileStream(MetadataFile, FileMode.Open);
                        MetaFile.Read(OrigMetaBuf, 0, OrigMetaBuf.Length);
                        MetaFile.Close();

                        CmprsMetaBuf = ZLibCompressor.Compress(OrigMetaBuf);
                    }
                } 

                result = DecodeSfntFont(SfntFile);

                if (result == HYRESULT.NOERROR)
                {
                    result = CompresssnftTableBuf();
                    if (result == HYRESULT.NOERROR)
                    {
                        result = EncodeWoff(WoffFile);
                    }
                }
                      
            }
            catch(Exception exc)
            {
                exc.ToString();
                throw;
            }            

            return result;

        }   // end of public HYRESULT Sfnt2Woff()

        public HYRESULT	DecodeSfntFont(string SfntFile)
        {          
            FileInfo fileInf = new FileInfo(SfntFile);
            if (fileInf.Length > 0)
            {
                try 
                {
                    FontDecode  = new HYDecode();                    
                    HYRESULT rst = FontDecode.FontOpen(SfntFile);
                    if (rst != HYRESULT.NOERROR)  return rst;

                    //rst = FontDecode.DecodeTableDirectory();
                    //if (rst != HYRESULT.NOERROR)  return rst;

                    lstSfntOrigtbBuf = new List<byte[]>();
                    for (ushort i = 0; i < FontDecode.tbDirectory.numTables; i++)
		            {
                        CTableEntry tbleEntry = FontDecode.tbDirectory.vtTableEntry[i];
                        FontDecode.DecodeStream.Seek(tbleEntry.offset,SeekOrigin.Begin);

                        byte[] tbbuf = new byte[tbleEntry.length];
                        FontDecode.DecodeStream.Read(tbbuf, 0, (int)tbleEntry.length);
                        lstSfntOrigtbBuf.Add(tbbuf);
		            }
                    FontDecode.FontClose();
                }
                catch(Exception ect)
                {
                    ect.ToString();
                    throw;
                }                
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT	DecodeSfntFont()

        public HYRESULT CompresssnftTableBuf()
        {
            try 
            {
                lstWoffTblDict = new List<WOFFTableDictEntry>();
                lstSfntcmprstbBuf = new List<byte[]>();
                for (ushort i = 0; i < FontDecode.tbDirectory.numTables; i++)
                {
                    CTableEntry tableEntry = FontDecode.tbDirectory.vtTableEntry[i];
                    WOFFTableDictEntry woffEntry = new WOFFTableDictEntry();

                    woffEntry.tag = tableEntry.tag;
                    woffEntry.origchecksum = tableEntry.checkSum;
                    woffEntry.origlength = tableEntry.length;

                    byte[] CmprstbBuf = ZLibCompressor.Compress(lstSfntOrigtbBuf[i]);
                    if (CmprstbBuf.Length < woffEntry.origlength)
                    {
                        woffEntry.complength = (uint)CmprstbBuf.Length;
                        lstSfntcmprstbBuf.Add(CmprstbBuf);
                    }
                    else
                    {
                        woffEntry.complength = woffEntry.origlength;
                        lstSfntcmprstbBuf.Add(lstSfntOrigtbBuf[i]);
                    }

                    lstWoffTblDict.Add(woffEntry);
                }
            }
            catch(Exception ext)
            {
                ext.ToString();
                throw;
            }
        
            return HYRESULT.NOERROR;

        }   // end of public void	CompresssnftTableBuf()

        public HYRESULT EncodeWoff(string WoffFile)
        {
            try
            {
                FileStream woffFile = new FileStream(WoffFile, FileMode.Create, FileAccess.ReadWrite,FileShare.None);

                EncodeWoffHeader(woffFile);

                // 获取woffTableDiecrtory的数据起始点
                long WoffTableDiecrtoryPos = woffFile.Position;

                EncodeWoffTableDiecrtory(woffFile);
                EncodeWoffTableData(woffFile);

                if (CmprsMetaBuf != null)
                {
                    if (CmprsMetaBuf.Length > 0)
                    {
                        woffFile.Write(CmprsMetaBuf, 0, CmprsMetaBuf.Length);
                    }                
                }


                if (PrivteBuf != null)
                {
                    if (PrivteBuf.Length > 0)
                    {
                        woffFile.Write(PrivteBuf, 0, PrivteBuf.Length);
                    }
                }

                woffFile.Seek(WoffTableDiecrtoryPos,SeekOrigin.Begin);                
                EncodeWoffTableDiecrtory(woffFile);
                woffFile.Flush();
                woffFile.Close();

            }
            catch (Exception ext)
            {
                ext.ToString();
                throw;
            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeWoff()

        public HYRESULT EncodeWoffHeader(FileStream woffFile)
        {
            uint    ulTmp = 0;
		    ushort  usTmp = 0;
            byte[]  btTmp;

            woffFile.Seek(0, SeekOrigin.Begin);

            //signature 0x774F4646             
            ulTmp = hy_cdr_int32_to(0x774F4646);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //flavor        
            usTmp = hy_cdr_int16_to((ushort)FontDecode.tbDirectory.version.value);
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);            
            
            usTmp = hy_cdr_int16_to((ushort)FontDecode.tbDirectory.version.fract);
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            uint uiNumTable = (uint)FontDecode.tbDirectory.vtTableEntry.Count;
            uint iTotalSfntDataSize = 12;
            iTotalSfntDataSize += 16 * uiNumTable;
            for (int i = 0; i < uiNumTable; i++)
            {
                CTableEntry tblEntry = FontDecode.tbDirectory.vtTableEntry[i];
                iTotalSfntDataSize += ((tblEntry.length + 3) & 0xFFFFFFFC);
            }

            uint uiWoffsize = 44;
            uiWoffsize += 20 * uiNumTable;
            for (int i = 0; i < (int)uiNumTable; i++)
		    {
                WOFFTableDictEntry entry = lstWoffTblDict[i];
                uiWoffsize += (entry.complength + 3) & 0xFFFFFFFC;
		    }
            
            uint ulMetaOffset = 0;
            if (CmprsMetaBuf != null)
            {
                if (CmprsMetaBuf.Length > 0)
                {
                    ulMetaOffset = uiWoffsize;
                    uiWoffsize += (uint)(CmprsMetaBuf.Length + 3) & 0xFFFFFFFC;
                }            
            }

		    uint ulPrivteOffset = 0;
            if (PrivteBuf != null)
            {
                if (PrivteBuf.Length > 0)
                {
                    ulPrivteOffset = uiWoffsize;
                    uiWoffsize += (uint)(PrivteBuf.Length + 3) & 0xFFFFFFFC;
                }
            }            

            //length
            ulTmp = hy_cdr_int32_to(uiWoffsize);            
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //numTables
            usTmp = hy_cdr_int16_to((ushort)uiNumTable);
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //reserved
            usTmp = 0;
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //totalSfntSize
            ulTmp = hy_cdr_int32_to(iTotalSfntDataSize);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //majorVersion
            usTmp = hy_cdr_int16_to(1);
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //minorVersion
            usTmp = 0;
            btTmp = BitConverter.GetBytes(usTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //metaOffset
            ulTmp = hy_cdr_int32_to(ulMetaOffset);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //metaLength
            ulTmp = CmprsMetaBuf == null ? 0 : hy_cdr_int32_to((uint)CmprsMetaBuf.Length);
            //ulTmp = hy_cdr_int32_to((uint)CmprsMetaBuf.Length);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //metaOrigLength
            ulTmp = OrigMetaBuf == null ? 0 : hy_cdr_int32_to((uint)OrigMetaBuf.Length);
            //ulTmp = hy_cdr_int32_to((uint)OrigMetaBuf.Length);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //privOffset
            ulTmp = hy_cdr_int32_to(ulPrivteOffset);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            //privLength
            ulTmp = PrivteBuf == null ? 0 : hy_cdr_int32_to((uint)PrivteBuf.Length);
            //ulTmp = hy_cdr_int32_to((uint)PrivteBuf.Length);
            btTmp = BitConverter.GetBytes(ulTmp);
            woffFile.Write(btTmp, 0, btTmp.Length);

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeWoffHeader()

        public HYRESULT EncodeWoffTableDiecrtory(FileStream woffFile)
        {
            uint ulTmp = 0;            
            byte[] btTmp;
            for (int i = 0; i < lstWoffTblDict.Count; i++)
		    {
                WOFFTableDictEntry  entry = lstWoffTblDict[i];

			    //tag
			    ulTmp = hy_cdr_int32_to(entry.tag);
                btTmp = BitConverter.GetBytes(ulTmp);
                woffFile.Write(btTmp, 0, btTmp.Length);			    

			    //offset
			    ulTmp = hy_cdr_int32_to(entry.offset);
                btTmp = BitConverter.GetBytes(ulTmp);
                woffFile.Write(btTmp, 0, btTmp.Length);

			    //compLength
			    ulTmp = hy_cdr_int32_to(entry.complength);
                btTmp = BitConverter.GetBytes(ulTmp);
                woffFile.Write(btTmp, 0, btTmp.Length);

			    //origLength
			    ulTmp = hy_cdr_int32_to(entry.origlength);
                btTmp = BitConverter.GetBytes(ulTmp);
                woffFile.Write(btTmp, 0, btTmp.Length);

			    //origChecksum
			    ulTmp = hy_cdr_int32_to(entry.origchecksum);
                btTmp = BitConverter.GetBytes(ulTmp);
                woffFile.Write(btTmp, 0, btTmp.Length);
		    }	

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeWoffTableDiecrtory()

        public HYRESULT EncodeWoffTableData(FileStream woffFile)
        {
            uint ulTmp = 0;
            for (int i = 0; i < lstWoffTblDict.Count; i++)
		    {
                WOFFTableDictEntry  entry = lstWoffTblDict[i];
                entry.offset = (uint)woffFile.Position;
                byte[] tabledata = lstSfntcmprstbBuf[i];

			    ulTmp =	(entry.complength+3)&0xFFFFFFFC;
                woffFile.Write(tabledata, 0, (int)entry.complength);

                uint iTail = ulTmp - entry.complength;
                if (iTail > 0)
                {
                    byte[] bttmp = new byte[iTail];
                    woffFile.Write(bttmp, 0, bttmp.Length);               
                }
		    }	

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT EncodeWoffTableData()



    }
}
