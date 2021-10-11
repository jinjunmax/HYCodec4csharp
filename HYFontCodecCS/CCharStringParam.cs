using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HYFontCodecCS
{
    public class CCharStringParam
    {
        public double[]	            transient = new double[32];
	    public List<double>			arguments = new List<double>();		
        public int  bufStart;
        public Rectangle bounds = new Rectangle();
        public int hstemPos;		// position of the hstem or hstemhm operator
        public int hstemArgs;		// count of hstem arguments
        public int injectPos;		// Position in the charstring of the first move or path operator
        public int lastOp;
        public int hintCount;
        public bool haveWidth;
        public bool hstem;
        public bool vstem;
        public bool movePath;
        public PointF position  = new PointF();

        public int TakeWidth(ref double width, int paramCount)
		{
			int result = 0;

			if (!haveWidth)
			{
				if (arguments.Count > paramCount)
				{
					width = arguments[0];
					++result;
				}

				haveWidth = true;
			}
			return result;		
		}

	};    
}
