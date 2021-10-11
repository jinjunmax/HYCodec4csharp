using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CTableEntry
    { 
    	public UInt32   tag {get;set;}
		public UInt32   checkSum{get;set;}	
		public UInt32   offset{get;set;}
        public UInt32   length{get;set;}
    
    }   // end of public class CTableEntry


    public class CTableDirectory
    {        
        public HYFIXED          version = new HYFIXED();
		public UInt16			numTables        {get;set;}
		public UInt16			searchRange      {get;set;}
		public UInt16			entrySelector    {get;set;}
		public UInt16			rangeShift       {get;set;}
     
		public List<CTableEntry>  vtTableEntry = new List<CTableEntry>();

        public Int32 FindTableEntry(UInt32 tag)
        {
            for (Int32 i=0; i<vtTableEntry.Count;i++)
            {
                if (vtTableEntry[i].tag == tag)
                    return i;
            }

            return -1;

        }   //public Int32 FindTableEntry

    }   // end of public class CTableDirectory
}
