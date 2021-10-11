using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CLayerRecord
    {
        public ushort 	GID; 		    //Glyph ID of layer glyph (must be in z-order from bottom to top).
		public ushort   paletteIndex;   //		

    }

    public class CBaseGlyphRecord
    { 
        public 	ushort GID;				//Glyph ID of reference glyph. This glyph is for reference only and is not rendered for color.
		public	ushort firstLayerIndex;	//Index (from beginning of the Layer Records) to the layer record. There will be numLayers consecutive entries for this base glyph.
		public	ushort numLayers;		//Number of color layers associated with this glyph.    
    }

    public class CHYCOLR
    {
        public  ushort 	version; 				//	Table version number (starts at 0).
		public  ushort	numBaseGlyphRecords;	//	Number of Base Glyph Records.
		public  uint	baseGlyphRecordsOffset;	// 	Offset (from beginning of COLR table) to Base Glyph records.
		public  uint	layerRecordsOffset;		// 	Offset (from beginning of COLR table) to Layer Records.
        public  ushort numLayerRecords;		// 	Number of Layer Records.

        public  List<CBaseGlyphRecord>  lstBaseGlyphRecord = new List<CBaseGlyphRecord>();
        public List<CLayerRecord>       lstLayerRecord = new List<CLayerRecord>();

        
        public bool FindBaseGlyhRecord(int iGID, ref CBaseGlyphRecord GlyphRecord)
        {            
            for (int i = 0; i < lstBaseGlyphRecord.Count; i++)
            {
                if (lstBaseGlyphRecord[i].GID == iGID)
                {
                    GlyphRecord.GID = lstBaseGlyphRecord[i].GID;
                    GlyphRecord.firstLayerIndex = lstBaseGlyphRecord[i].firstLayerIndex;
                    GlyphRecord.numLayers = lstBaseGlyphRecord[i].numLayers;

                    return true;
                }
            }

            return false;

        }   // end of bool FindBaseGlyhRecord()

        public void FindLayerRecord(int iGID, ref List<CLayerRecord> out_lstLayerRecord)
        {
            int st = lstLayerRecord.Count;
            for (int i = 0; i < st; i++)
            {
                if (lstLayerRecord[i].GID == iGID)
                {
                    CLayerRecord tmpLayerRecord = new CLayerRecord();

                    tmpLayerRecord.GID = lstLayerRecord[i].GID;
                    tmpLayerRecord.paletteIndex = lstLayerRecord[i].paletteIndex;
                    out_lstLayerRecord.Add(tmpLayerRecord);
                }
            }
        }
    }
}
