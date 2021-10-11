using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
   public class GroupRecord
    {
       public UInt32	startCode;
       public UInt32    endCode;
       public Int32 	startGID;
       public bool		contiguous;
    }
}
