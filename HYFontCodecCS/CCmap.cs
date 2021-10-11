using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class  CMAP_ENCODE_FORMAT_0
	{
        public UInt16 format;		//Format number is set to 0. 
        public UInt16 length; 		            //This is the length in bytes of the subtable.
        public UInt16 language;		            //Version number (starts at 0).
        public List<Byte> vtGlyphId = new List<byte>();

	} // end of public class  CMAP_ENCODE_FORMAT_0

	public class CMAP_ENCODE_FORMAT_2_SUBHEAD
	{
        public UInt16 firstCode;
        public UInt16 entryCount;
        public Int16 idDelta;
        public UInt16 idRangeOffset;

	}   // end of public class CMAP_ENCODE_FORMAT_2_SUBHEAD

	public class CMAP_ENCODE_FORMAT_2
	{	
		public UInt16	format;  
		public UInt16	length;  
		public UInt16	language;

		List<UInt16>	vtsubHeaderKeys = new List<UInt16>();	
		List<CMAP_ENCODE_FORMAT_2_SUBHEAD> vtSubHead = new List<CMAP_ENCODE_FORMAT_2_SUBHEAD>();
		List<UInt16>	vtglyphIndex = new List<UInt16>();

	}   // end of public class CMAP_ENCODE_FORMAT_2

	public class CMAP_ENCODE_FORMAT_4
	{
        public UInt16 format;
        public UInt16 length;
        public UInt16 language;
        public UInt16 segCountX2;
        public UInt16 searchRange;
        public UInt16 entrySelector;
        public UInt16 rangeShift;
		public List<UInt16>		vtEndCount      = new List<UInt16>();
		public UInt16			reservedPad {get;set;}	
		public List<UInt16>		vtstartCount    = new List<UInt16>();	
		public List<Int16>		vtidDelta       = new List<Int16>();
		public List<UInt16>		vtidRangeOffset = new List<UInt16>();
		public List<UInt16>		vtglyphIdArray  = new List<UInt16>();

	}   // end of public class CMAP_ENCODE_FORMAT_4

	public class CMAP_ENCODE_FORMAT_6
	{
        public UInt16 format;
        public UInt16 length;
        public UInt16 language;
        public UInt16 firstCode;
        public UInt16 entryCount;
		public List<UInt16> vtGlyphId = new List<ushort>();

	} // end of public class CMAP_ENCODE_FORMAT_6

	public class CMAP_ENCODE_FORMAT_12_GROUP
	{
		public UInt32		startCharCode;
		public UInt32		endCharCode;
		public UInt32		startGlyphID;

	}   // end of public class CMAP_ENCODE_FORMAT_12_GROUP

	public class CMAP_ENCODE_FORMAT_12
	{
        public UInt16 format;
        public UInt16 reserved;
        public UInt32 length;
        public UInt32 language;
        public UInt32 nGroups;
		public List<CMAP_ENCODE_FORMAT_12_GROUP>	vtGroup = new List<CMAP_ENCODE_FORMAT_12_GROUP>();

	}   // end of public class CMAP_ENCODE_FORMAT_12

	public class CMAP_TABLE_ENTRY
	{	
		public UInt16						plat_ID {get;set;}
		public UInt16						plat_encod_ID{get;set;}
        //Byte offset from beginning of table to the subtable for this encoding.
		public UInt32						offset{get;set;}
		public UInt16						format{get;set;}	
		public CMAP_ENCODE_FORMAT_0			Format0 = new CMAP_ENCODE_FORMAT_0();
		public CMAP_ENCODE_FORMAT_2			Format2 = new CMAP_ENCODE_FORMAT_2();
		public CMAP_ENCODE_FORMAT_4			Format4 = new CMAP_ENCODE_FORMAT_4();
		public CMAP_ENCODE_FORMAT_6			Format6 = new CMAP_ENCODE_FORMAT_6();
		public CMAP_ENCODE_FORMAT_12		Format12 = new CMAP_ENCODE_FORMAT_12();
	}

	public class CCmap
	{
		public UInt16 version {get;set;}
		public UInt16 numSubTable {get;set;}
        public List<CMAP_TABLE_ENTRY> vtCamp_tb_entry = new List<CMAP_TABLE_ENTRY>();


        public Int32 FindSpecificEntryIndex(CMAPENCODEFORMAT iFormat)
        {
            for (UInt16 i = 0; i<vtCamp_tb_entry.Count; i++)
            {
                if (vtCamp_tb_entry[i].format == (ushort)iFormat)
                    return i;
            }

            return -1;

        }   // end of public int FindSpecificEntryIndex()

	} // end of public class CHYCmap
}   
