using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public enum Outline_Orientation
    {
        FT_ORIENTATION_TRUETYPE = 0,
        FT_ORIENTATION_POSTSCRIPT = 1,
        FT_ORIENTATION_FILL_RIGHT = FT_ORIENTATION_TRUETYPE,
        FT_ORIENTATION_FILL_LEFT = FT_ORIENTATION_POSTSCRIPT,
        FT_ORIENTATION_NONE
    }

    public class CSPoint
    {
        public int X;
        public int Y;        
    }

    public class CSContuor
    {
        public List<CSPoint> lstPts = new List<CSPoint>();
    }

    public class CSComponent
    {
        public List<int> Arg = new List<int>();
        public int Flag;
        public int Gid;
        public short Scale;
        public short Scale01;
        public short Scale10;
        public short ScaleX;
        public short ScaleY;
    }

    public class CSBox
    {
        public long xMin=0, yMin=0;
        public long xMax=0, yMax=0;

    }   //end of public class CSBox

    public class CSGlyph
    {
        /// <summary>
        /// n_contours Glyph包含的轮廓数量,-1代表是组件字库。
        /// </summary>
        public short n_contours=0;
        /// <summary>
        /// n_points Glyph包含的点数量。
        /// </summary>
        public short n_points=0;
        /// <summary>
        /// the outline's points 
        /// </summary>
        public List<CSPoint> points = new List<CSPoint>();
        /// <summary>
        /// the points flags
        /// </summary>
        public List<short> tags = new List<short>();
        /// <summary>
        /// the contour end points
        /// </summary>
        public List<short> endContous = new List<short>();
        /// <summary>
        /// outline masks
        /// </summary>
        public int flags;
        /// <summary>
        /// 字面高度
        /// </summary>
        public int AdHeight;
        /// <summary>
        /// 字面宽度
        /// </summary>
        public int AdWidth;
        /// <summary>
        /// 字符名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 字形边框
        /// </summary>
        public CSBox box = new CSBox();
        /// <summary>
        /// unicode编码,可能为空
        /// </summary>
        public List <int> unicode = new List<int>();
    }

}
