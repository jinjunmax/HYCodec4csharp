using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CSEffect
    {
        public bool SetOutLineThickness(ref CSGlyph outline, long XStrength, long YStrength)
        {
            if (XStrength == 0 && YStrength == 0) return false;

            XStrength /= 2;
            YStrength /= 2;

            Outline_Orientation outOrnttn =  Get_Orientation(ref outline);
            if (outOrnttn == Outline_Orientation.FT_ORIENTATION_NONE)
                return false;

            int  c=0, first=0, last=0;
            List<CSPoint> points = outline.points;
            for (c = 0; c < outline.n_contours; c++)
            {
                CSPoint inPt = new CSPoint();
                CSPoint outPt = new CSPoint();
                CSPoint anchor = new CSPoint();
                CSPoint shift = new CSPoint();

                long l_in, l_out, l_anchor = 0, l, q, d;
                int  i, j, k;

                l_in = 0;
                last = outline.endContous[c];

                /* pacify compiler */
                inPt.X = inPt.Y = anchor.X = anchor.Y = 0;

                /* Counter j cycles though the points; counter i advances only  */
                /* when points are moved; anchor k marks the first moved point. */

                /*
                for (i = last, j = first, k = -1; j != i && i != k; j = j < last ? j + 1 : first)
                {
                    if (j != k)
                    {
                        outPt.X = points[j].X - points[i].X;
                        outPt.Y = points[j].Y - points[i].Y;
                        l_out = (FT_Fixed)FT_Vector_NormLen(&out );

                        if (l_out == 0)
                            continue;
                    }
                    else
                    {
                        out   = anchor;
                        l_out = l_anchor;
                    }
                }*/
            }

            return true;

        }   // end of public void SetOutLineThickness()

        int Vector_NormLen(ref CSPoint vector)
        {   
            return 0;
        }   // end of int Vector_NormLen()

        public Outline_Orientation Get_Orientation(ref CSGlyph outline)
        {
            CSBox box = new CSBox();
            int xshift=0, yshift=0;           
            CSPoint v_prev = new CSPoint();
            CSPoint v_cur = new CSPoint();
            int  c=0, n=0;
            long area = 0;

            if (outline.n_points <= 0)
                return Outline_Orientation.FT_ORIENTATION_TRUETYPE;

            Outline_Get_CBox(ref outline);

            /* Handle collapsed outlines to avoid undefined FT_MSB. */
            if (outline.box.xMin == outline.box.xMax || outline.box.yMin == outline.box.yMax)
                return Outline_Orientation.FT_ORIENTATION_NONE;


            xshift = FT_MSB((UInt32)(Math.Abs(outline.box.xMax)|Math.Abs(outline.box.xMin))) - 14;
            xshift = Math.Max(xshift, 0);

            yshift = FT_MSB((UInt32)(outline.box.yMax - outline.box.yMin)) - 14;
            yshift = Math.Max(yshift, 0);

            List<CSPoint> points = outline.points;
            
            int first = 0,last = 0;
            for (c=0;c< outline.n_contours; c++)
            {
                last = outline.endContous[c];

                v_prev.X = points[last].X >> xshift;
                v_prev.Y = points[last].Y >> yshift;
                for (n = first; n <= last; n++)
                {
                    v_cur.X = points[n].X >> xshift;
                    v_cur.Y = points[n].Y >> yshift;

                    area = ADD_LONG(area,
                        MUL_LONG(v_cur.Y - v_prev.Y,
                                  v_cur.X + v_prev.X));

                    v_prev.X = v_cur.X;
                    v_prev.Y = v_cur.Y;

                }
                first = last + 1;
            }

            if (area > 0)
                return Outline_Orientation.FT_ORIENTATION_POSTSCRIPT;
            else if (area < 0)
                return Outline_Orientation.FT_ORIENTATION_TRUETYPE;
            else
                return Outline_Orientation.FT_ORIENTATION_NONE;

        }   // end of public Outline_Orientation Get_Orientation()

        long ADD_LONG(long a, long b)
        {
            return (long)((ulong)(a) + (ulong)(b));
        }
        
        long SUB_LONG(long a, long b)                                    
        {
            return (long)((ulong)(a) - (ulong)(b));
        }
          
        long MUL_LONG(long a,long b)
        {
            return (long)((ulong)(a) * (ulong)(b));
        }

        long NEG_LONG(long a) 
        {
            return -(long)(a);
        }

        public Int32 FT_MSB(UInt32 z)
        {
            Int32 shift = 0;

            /* determine msb bit index in `shift' */
            if ((z&0xFFFF0000UL)>0)
            {
                z >>= 16;
                shift += 16;
            }
            if ((z&0x0000FF00UL)>0)
            {
                z >>= 8;
                shift += 8;
            }
            if ((z&0x000000F0UL)>0)
            {
                z >>= 4;
                shift += 4;
            }
            if ((z&0x0000000CUL)>0)
            {
                z >>= 2;
                shift += 2;
            }
            if ((z&0x00000002UL)>0)
            {
                /* z     >>= 1; */
                shift += 1;
            }

            return shift;
        }   // end of 


        public void Outline_Get_CBox(ref CSGlyph outline)
        {
            long xMin=0, yMin=0, xMax=0, yMax=0;            
            if (outline.n_points == 0)
            {
                xMin = 0;
                yMin = 0;
                xMax = 0;
                yMax = 0;
            }
            else
            {                
                List<CSPoint> vec = outline.points;                
                xMin = xMax = vec[0].X;
                yMin = yMax = vec[0].Y;               

                for (int i=1; i < outline.n_points; i++)
                {
                    long x, y;

                    x = vec[i].X;
                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;

                    y = vec[i].Y;
                    if (y < yMin) yMin = y;
                    if (y > yMax) yMax = y;
                }
            }

            outline.box.xMin = xMin;
            outline.box.xMax = xMax;
            outline.box.yMin = yMin;
            outline.box.yMax = yMax;

        }   // end of public void Outline_Get_CBox()

    }
}
