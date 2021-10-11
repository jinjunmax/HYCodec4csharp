using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class VMTX_LONGHORMETRIC
	{	
		public UInt16 advanceHeight {get;set;}			
		public Int16  tsb {get;set;}
	};

	public class CVmtx
	{	
		public List<VMTX_LONGHORMETRIC> vtLongHormetric = new List<VMTX_LONGHORMETRIC>();
	};
}
