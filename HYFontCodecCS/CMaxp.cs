using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CMaxp
    {
        public HYFIXED  version = new HYFIXED();
        public UInt16   numGlyphs               {get;set;}
        public UInt16   maxPoints               {get;set;}
        public UInt16   maxContours             {get;set;}
        public UInt16   maxCompositePoints      {get;set;}
        public UInt16   maxCompositeContours    {get;set;}
        public UInt16   maxZones                {get;set;}
        public UInt16   maxTwilightPoints       {get;set;}
        public UInt16   maxStorage              {get;set;}
        public UInt16   maxFunctionDefs         {get;set;}
        public UInt16   maxInstructionDefs      {get;set;}
        public UInt16   maxStackElements        {get;set;}
        public UInt16   maxSizeOfInstructions   {get;set;}
        public UInt16   maxComponentElements    {get;set;}
        public UInt16   maxComponentDepth       {get;set;}

    }   // end of public class CMaxp

}   
