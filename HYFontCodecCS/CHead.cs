﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CHead
    {
        public CHead()
        {
            version = new HYFIXED();
            fontRevision = new HYFIXED();
        }

        public HYFIXED				version;
		public HYFIXED				fontRevision;

		public UInt32		        checkSumAdjustment {get;set;}
		public UInt32		        magicNumber{get;set;}
		public UInt16		        flags{get;set;}	
		public UInt16		        unitsPerEm{get;set;}
		public Byte[]				created;	
		public Byte[]				modified;
        public Int16                xMin { get; set; }
        public Int16                yMin { get; set; }
        public Int16                xMax { get; set; }
        public Int16                yMax { get; set; }
        public UInt16               macStyle { get; set; }
        public UInt16               lowestRecPPEM{get;set;}
        public Int16                fontDirectionHint { get; set; }
        public Int16                indexToLocFormat { get; set; }
        public Int16                glyphDataFormat { get; set; }	
    }
}
