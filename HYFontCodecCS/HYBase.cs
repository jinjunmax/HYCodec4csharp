using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using FontParserEntity;
using System.Windows;

namespace HYFontCodecCS
{
    public class HYBase
    {
        public static UInt16 hy_cdr_int16_to(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);

        }   // end of public static UInt16 hy_cdr_int16_to()

        public static UInt32 hy_cdr_int32_to(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;

        }   // end of protected UInt32 hy_cdr_int32_to()
        public static UInt64 hy_cdr_int64_to(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;

        }   // end of protected UInt64 hy_cdr_int64_to()
        public static float HY_F2DOT14_to_float(short F2DOT14)
        {
            short sValue;
            if ((0x8000&F2DOT14)>1)
                sValue = (short)(-((~(F2DOT14 >> 14)) + 1));
            else
                sValue = (short)(F2DOT14 >> 14);

            float fFraction = (F2DOT14 & 0x3FFF) / 16384.0f;
            float fValue = sValue + fFraction;

            return fValue;

        }	// end of float HY_F2DOT14_to_float()

        public static short HY_float_to_F2DOT14(float f)
        {
	        if (f>1.999939 || f<-2.0) return 0;

	        short   sValue = (short)f;
	        //记录扩大10的n次方倍
	        int a =0;
	        float ftmp = f;
	        while(((ftmp-(int)ftmp)<0.00001) && ((ftmp-(int)ftmp)>-0.00001)) //用标准的float型判断为零的方式更好：((x-(int)x)<0.00001)&&((x-(int)x)>-0.00001)
	        {
		        ftmp= ftmp*10;
		        a++;	
	        }

            ushort fraction = (ushort)Math.Abs(((f-sValue) * (int)Math.Pow(10.0, (double)a)));

	        return (short)((sValue<<14)|fraction);

        }	// end of short HY_float_to_F2DOT14()

        public static int HY_RealRount(double db)
        {
            int iRtn = 0;
            if (db > 0)
            {
                iRtn = (int)(db + 0.5f);
            }
            else 
            {
                iRtn = (int)(db - 0.5f);
            }

            return iRtn;

        }	// end of BOOL int HY_RealRount()

        /// <summary>
        /// 通过unicode确定GID
        /// </summary>
        /// <param name="chars">字符轮廓集合</param>
        /// <param name="unicode">UniCode编码</param>
        /// <returns></returns>
        public static int GetGlyphsID(CharsInfo chars, UInt32 unicode)
        {
            for (int i=0; i< chars.CharInfo.Count; i++)
            {
               UInt32 tmpUni = Convert.ToUInt32(chars.CharInfo[i].Unicode);
                if (unicode == tmpUni)
                    return i;
            }

            return -1;

        }   // end of public static int GetGlyphsID()

        /// <summary>
        /// 对齐函数
        /// </summary>
        /// <param name="n"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public static uint HY_calc_align(uint n, uint align)
        {
            return ((n + align - 1) & (~(align - 1)));

        }  // end of uint HY_calc_align()

        /// <summary>
        /// 检查是否有重叠点
        /// </summary>
        /// <param name="ptLst"></param>
        /// <returns>ture 有重点，false 无重点</returns>
        public static bool CheckCoincide(List<PtInfo> ptLst)
        {
            for (int i=0; i< ptLst.Count; i++)
            {
                PtInfo pt1 = ptLst[i];
                Rect rt1 = new Rect(new Point(pt1.X - 1, pt1.Y - 1),
                    new Point(pt1.X + 1, pt1.Y + 1));
                for (int j = i+1; j < ptLst.Count; j++)
                {
                    PtInfo pt2 = ptLst[j];
                    if (rt1.Contains(new Point(pt2.X,pt2.Y)))
                    {
                        return true;
                    }
                }
            }

            return false;

        }   // end of public static bool CheckCoincide()

        public static void CountBoundBox(int iGID, ref CharsInfo charsinf, FONTTYPE fntType)
        {
            CharInfo charinf = charsinf.CharInfo[iGID];
            if (charinf.ContourCount == 0)
            {               
                return;
            }           
            
            if (fntType == FONTTYPE.CFF)
            {
                for (int i = 0; i<charinf.ContourCount; i++)
                {
                    double ax = 0.0, ay = 0.0, bx = 0.0, by = 0.0, cx = 0.0, cy = 0.0, dx = 0.0, dy = 0.0;

                    ContourInfo cnutinf = charinf.ContourInfo[i];
                    for (int j = 0; j < cnutinf.PointCount; j++)
                    {
                        PtInfo ptinf = cnutinf.PtInfo[j];
                        if (ptinf.PtType == 0x01)
                        {
                            ReportBounds(ref charinf, ptinf.X, ptinf.Y);
                        }
                        else
                        {
                            if (j - 1 < 0) break;
                            if (j + 2 >= cnutinf.PointCount) break;

                            ax = cnutinf.PtInfo[j-1].X;
                            ay = cnutinf.PtInfo[j-1].Y;
                            bx = cnutinf.PtInfo[j].X;
                            by = cnutinf.PtInfo[j].Y;
                            cx = cnutinf.PtInfo[++j].X;
                            cy = cnutinf.PtInfo[j].Y;
                            dx = cnutinf.PtInfo[++j].X;
                            dy = cnutinf.PtInfo[j].Y;

                            Report3Bezier(ref charinf, ax, ay, bx, by, cx, cy, dx, dy);
                            ReportBounds(ref charinf, dx, dy);
                        }
                    
                    }
                }
            }
            else if (fntType == FONTTYPE.TTF)
            {
                if (charinf.IsComponent == 0)
                {
                    for (int i = 0; i < charinf.ContourCount; i++)
                    {
                        ContourInfo cnutinf = charinf.ContourInfo[i];
                        for (int j = 0; j < cnutinf.PointCount; j++)
                        {
                            PtInfo ptinf = cnutinf.PtInfo[j];
                            ReportBounds(ref charinf, ptinf.X, ptinf.Y);
                        }
                    }
                }

                if (charinf.IsComponent == 1)
                {               
                    // 针对组合轮廓转换			       
                    float fXScale = 1.0f, fYScale = 1.0f;
                    int CharXmin = 0, CharYmin = 0, CharXMax = 0, CharYMax=0;
			        for (int i=0; i<charinf.CmpInf.Count; i++)
			        {				
				        fXScale = 1.0f; 
				        fYScale = 1.0f;				
				        CmpInf cmppntInf = charinf.CmpInf[i];

				        int sGlyphIndex  = cmppntInf.Gid;
				        if ((cmppntInf.Flag&0x0002)>0)//GLYF_CMPST_ARGS_ARE_XY_VALUES
				        {
					        if ((cmppntInf.Flag&0x0008)>0)//GLYF_CMPST_WE_HAVE_A_SCALE
					        {
						        fXScale = HY_F2DOT14_to_float((short)cmppntInf.Scale);
						        fYScale = fXScale;
					        }

					        if ((cmppntInf.Flag&0x0040)>0)//GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE
					        {
                                fXScale = HY_F2DOT14_to_float((short)cmppntInf.ScaleX);
                                fYScale = HY_F2DOT14_to_float((short)cmppntInf.ScaleY);
					        }
				        }
							
				        // 计算路径				
                        if (sGlyphIndex >= charsinf.CharInfo.Count) continue;
				        if (sGlyphIndex<0)	continue;

                        CharInfo cmpCharInfo = charsinf.CharInfo[sGlyphIndex];

                        string[] values = cmpCharInfo.Section.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        float xcmpMin = Convert.ToInt32(values[0], 10);
                        float YcmpMin = Convert.ToInt32(values[1], 10);
                        float xcmpMax = xcmpMin + Convert.ToInt32(values[2], 10);
                        float YcmpMax = YcmpMin + Convert.ToInt32(values[3], 10);
                        

				        // 先缩放
                        float XminTmp = xcmpMin * fXScale;
                        float XmaxTmp = xcmpMax * fXScale;
                        float YminTmp = YcmpMin * fYScale;
                        float YmaxTmp = YcmpMax * fYScale;

				        // 后偏移
                        xcmpMin = XminTmp + cmppntInf.Arg[0];
                        xcmpMax = XmaxTmp + cmppntInf.Arg[0];
                        YcmpMin = YminTmp + cmppntInf.Arg[1];
                        YcmpMax = YmaxTmp + cmppntInf.Arg[1];													
					
                        
				        if (i==0)
				        {
                            CharXmin = HY_RealRount((double)xcmpMin);
                            CharYmin = HY_RealRount((double)YcmpMin);
                            CharXMax = HY_RealRount((double)xcmpMax);
                            CharYMax = HY_RealRount((double)YcmpMax);
				        }
				        else 
				        {
                            if (CharXmin > HY_RealRount(xcmpMin)) CharXmin = HY_RealRount(xcmpMin);
                            if (CharXMax < HY_RealRount(xcmpMax)) CharXMax = HY_RealRount(xcmpMax);
                            if (CharYmin > HY_RealRount(YcmpMin)) CharYmin = HY_RealRount(YcmpMin);
                            if (CharYMax < HY_RealRount(YcmpMax)) CharYMax = HY_RealRount(YcmpMax);
				        }								
			        }
                    charinf.Section = CharXmin.ToString() + "," + CharYmin.ToString() + "," + (CharXMax - CharXmin).ToString() + "," + (CharYMax - CharYmin).ToString();
                }
            }

        }	// end of void	CHYGlyph::CountBoundBox()

        static void ReportBounds(ref CharInfo chrInf, double X, double Y)
        {
            string[] values = chrInf.Section.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length == 0)
            {
                chrInf.Section = X.ToString() + "," + Y.ToString() + "," + "0" + "," + "0";
            }
            else
            {
                int xMin = Convert.ToInt32(values[0], 10);
                int yMin = Convert.ToInt32(values[1], 10);
                int xMax = xMin + Convert.ToInt32(values[2], 10);
                int yMax = yMin + Convert.ToInt32(values[3], 10);

                if (X < xMin)
                    xMin = HY_RealRount(X);
                if (X > xMax)
                    xMax = HY_RealRount(X);
                if (Y < yMin)
                    yMin = HY_RealRount(Y);
                if (Y > yMax)
                    yMax = HY_RealRount(Y);

                chrInf.Section = xMin.ToString() + "," + yMin.ToString() + "," + (xMax - xMin).ToString() + "," + (yMax - yMin).ToString();
            }

        }	// end of void ReportBounds()

        static void Report3Bezier(ref CharInfo charinf, double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy)
        {
            string[] values = charinf.Section.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int xMin = Convert.ToInt32(values[0],10);
            int yMin = Convert.ToInt32(values[1],10);
            int xMax = xMin + Convert.ToInt32(values[2],10);
            int yMax = yMin + Convert.ToInt32(values[3],10);

            Pair xs = BezierInflection(ax, bx, cx, dx);
            Pair ys = BezierInflection(ay, by, cy, dy);

            if ((double)xs.First < 0.0 || (double)xs.First > 1.0)
                xs.First = Nan();
            if ((double)xs.Second < 0.0 || (double)xs.Second > 1.0)
                xs.Second = Nan();
            if ((double)ys.First < 0.0 || (double)ys.First > 1.0)
                ys.First = Nan();
            if ((double)ys.Second < 0.0 || (double)ys.Second > 1.0)
                ys.Second = Nan();

            double a, b;
            a = BezierEval(ax, bx, cx, dx, (double)xs.First);
            b = BezierEval(ax, bx, cx, dx, (double)xs.Second);

            xMin = (short)HY_RealRount(Less(xMin, a));
            xMin = (short)HY_RealRount(Less(xMin, b));
            xMax = (short)HY_RealRount(Greater(xMax, a));
            xMax = (short)HY_RealRount(Greater(xMax, b));

            a = BezierEval(ay, by, cy, dy, (double)ys.First);
            b = BezierEval(ay, by, cy, dy, (double)ys.Second);

            yMin = (short)HY_RealRount(Less(yMin, a));
            yMin = (short)HY_RealRount(Less(yMin, b));
            yMax = (short)HY_RealRount(Greater(yMax, a));
            yMax = (short)HY_RealRount(Greater(yMax, b));

            charinf.Section = xMin.ToString() + "," + yMin.ToString() + "," + (xMax - xMin).ToString() + "," + (yMax - yMin).ToString();

        }	// end of void ReportBezier()

        static double BezierEval(double a0, double a1, double a2, double a3, double t)
        {
            if (IsNan(t))
                return Nan();

            if (t < 0.0 || t > 1.0) return -1.0;

            double rt = 1.0 - t;
            return rt * ((rt * (rt * a0 + 3.0 * a1 * t)) + 3.0 * a2 * t * t) + t * t * t * a3;

        }	// end of double BezierEval ()

        static double Less(double original, double x)
        {
            if (IsNan(x))
                return original;

            if (x < original)
                return x;
            else
                return original;

        }	// end of double Less()
        static double Greater(double original, double x)
        {
            if (IsNan(x))
                return original;

            if (x > original)
                return x;
            else
                return original;

        }	// end of double Greater()
        static Pair BezierInflection(double a0, double a1, double a2, double a3)
        {
            double a = -a0 + 3.0 * a1 - 3.0 * a2 + a3;
            double b = 2.0 * a0 - 4.0 * a1 + 2.0 * a2;
            double c = -a0 + a1;
            double root1 = 0.0;
            double root2 = 0.0;

            Quadratic(a, b, c, ref root1, ref root2);
            return new Pair(root1, root2);

        }	// end of std::pair <double, double> CHYGlyph::BezierInflection ()
        static void Quadratic(double a, double b, double c, ref double root1, ref double root2)
        {
            double determinant, y, factor;
            if (a == 0.0)
            {
                if (b == 0.0)
                {
                    root1 = Nan();
                    root2 = Nan();
                }
                else
                {
                    root1 = -c / b;
                    root2 = root1;
                }
            }
            else if (b == 0.0)
            {
                root1 = Math.Sqrt(-a * c) / a;
                root2 = -root1;
            }
            else
            {
                factor = 0.5 * b / a;
                y = -4.0 * a * c / (b * b);

                if (Math.Abs(y) < 0.05)
                {
                    determinant = Binomial(y, 0.5);
                    root1 = determinant;
                    root2 = -2.0 - determinant;
                }
                else
                {
                    determinant = 1.0 + y;

                    if (determinant < -100.0 * granularity())
                        determinant = Nan();
                    else if (determinant < 0.0)
                        determinant = 0.0;
                    else
                        determinant = Math.Sqrt(determinant);

                    root1 = -1.0 + determinant;
                    root2 = -1.0 - determinant;
                }

                root1 *= factor;
                root2 *= factor;
            }

        }	// end of void	CHYGlyph::Quadratic ()
        static double Nan()
        {
            return Math.Sqrt(-1.0);

        }	// end of double const CHYGlyph::Nan()
        static double Binomial(double x, double n)
        {
            /* Compute (1+x)^n - 1 for x near 0 by binomial theorem */

            double iterator,
                factorial,
                nfact,
                niterator,
                xpower,
                sum,
                lastsum,
                sign,
                nsign,
                toggle,
                delta,
                lastDelta;

            if (IsNan(x + n) || x < -1.0)
                sum = Nan();
            else if (x * x > 0.025)
                sum = Math.Exp(n * Math.Log(1.0 + x)) - 1.0;
            else
            {
                nsign = SignX(n);
                sign = nsign;
                toggle = sign;
                factorial = 1.0;
                iterator = 1.0;
                lastsum = 0.0;
                xpower = x;
                nfact = n;
                niterator = n;
                sum = toggle * x * n;
                lastDelta = Infinity();

                while (lastsum != (sum += (delta = ((toggle *= sign) * (xpower *= x) * (nfact *= (niterator -= nsign)) /
                    (factorial *= (iterator += 1.0))))))
                {
                    lastsum = sum;

                    if (delta * delta >= lastDelta * lastDelta)
                        break;

                    lastDelta = delta;
                }
            }

            return sum;

        }	// end of   double	CHYGlyph::Binomial ()
        static bool IsNan(double x)
        {
            return !(x == x);

        }	// end of bool IsNan()
        static double SignX(double x)
        {
            return x < 0.0 ? -1.0 : 1.0;

        }	// end of double SignX()
        static double Infinity()
        {
            return Math.Tan(3.1415926535897932384626 / 2.0);

        }	// end of double const Infinity()
        static  double granularity()
        {
            return 2.2204460493e-16;

        }	// end of double const granularity()

        public static UInt32 CMAP_FLG =	0x00000001;
        public static UInt32 HEAD_FLG = 0x00000002;
        public static UInt32 HHEA_FLG = 0x00000004;
        public static UInt32 HMTX_FLG = 0x00000008;
        public static UInt32 MAXP_FLG = 0x00000010;
        public static UInt32 NAME_FLG = 0x00000020;
        public static UInt32 OS2_FLG = 0x00000040;
        public static UInt32 POST_FLG = 0x00000080;
        public static UInt32 CVT_FLG = 0x00000100;
        public static UInt32 FPGM_FLG = 0x00000200;
        public static UInt32 GLYF_FLG = 0x00000400;
        public static UInt32 LOCA_FLG = 0x00000800;
        public static UInt32 PREP_FLG = 0x00001000;
        public static UInt32 CFF_FLG = 0x00002000;
        public static UInt32 VORG_FLG = 0x00004000;
        public static UInt32 EBDT_FLG = 0x00008000;
        public static UInt32 EBLC_FLG = 0x00010000;
        public static UInt32 EBSC_FLG = 0x00020000;
        public static UInt32 BASE_FLG = 0x00040000;
        public static UInt32 GDEF_FLG = 0x00080000;
        public static UInt32 GPOS_FLG = 0x00100000;
        public static UInt32 GSUB_FLG = 0x00200000;
        public static UInt32 JSTF_FLG = 0x00400000;
        public static UInt32 DSIG_FLG = 0x00800000;
        public static UInt32 GASP_FLG = 0x01000000;
        public static UInt32 HDMX_FLG = 0x02000000;
        public static UInt32 KERN_FLG = 0x04000000;
        public static UInt32 LTSH_FLG = 0x08000000;
        public static UInt32 PCLT_FLG = 0x10000000;
        public static UInt32 VDMX_FLG = 0x20000000;
        public static UInt32 VHEA_FLG = 0x40000000;
        public static UInt32 VMTX_FLG = 0x80000000;

    }   // end of public class HYBase

     public class HYFIXED
     {
        public Int16	value {get;set;}
        public UInt16   fract {get;set;}    

     }  // end of public class HYFIXED

     public enum FONTTYPE
     {
         CFF = 0x0001,
         TTF = 0x0002,

     }  // end if public enum FONTTYPE
     
     public enum CMAPENCODEFORMAT
     {
        CMAP_ENCODE_FT_0    =   0,
        CMAP_ENCODE_FT_2	=   2,
        CMAP_ENCODE_FT_4    =   4,
        CMAP_ENCODE_FT_6    =   6,
        CMAP_ENCODE_FT_8    =   8,
        CMAP_ENCODE_FT_10   =   10,
        CMAP_ENCODE_FT_12   =   12,
        CMAP_ENCODE_FT_13   =   13,
        CMAP_ENCODE_FT_14   =   14,

     }  // end of public enum CMAPENCODEFORMAT

     public enum TABLETAG
     {      
        BASE_TAG    =	0x42415345,
        CFF_TAG		=	0x43464620,
        DSIG_TAG	=	0x44534947,
        COLR_TAG    =   0x434F4C52,
        CPAL_TAG    =   0x4350414C,    
        EBDT_TAG	=	0x45424454,
        EBLC_TAG	=	0x45424C43,
        EBSC_TAG	=	0x45425343,
        GDEF_TAG	=	0x47444546,
        GPOS_TAG	=	0x47504F53,
        GSUB_TAG	=	0x47535542,
        JSTF_TAG	=	0x4A535446,
        LTSH_TAG	=	0x4C545348,
        OS2_TAG 	=	0x4F532F32,
        PCLT_TAG	=	0x50434C54,
        VDMX_TAG	=	0x56444D58,
        VORG_TAG	=	0x564F5247,
        CMAP_TAG	=	0x636D6170,
        CVT_TAG		=	0x63767420,
        FPGM_TAG	=	0x6670676D,
        GASP_TAG	=	0x67617370,
        GLYF_TAG	=	0x676C7966,
        HDMX_TAG	=	0x68646D78,
        HEAD_TAG	=	0x68656164,
        HHEA_TAG	=	0x68686561,
        HMTX_TAG	=	0x686D7478,
        KERN_TAG	=	0x6B65726E,
        LOCA_TAG	=	0x6C6F6361,
        MAXP_TAG	=	0x6D617870,
        NAME_TAG	=	0x6E616D65,
        POST_TAG	=	0x706F7374,
        PREP_TAG	=	0x70726570,
        VHEA_TAG	=	0x76686561,
        VMTX_TAG	=	0x766D7478,
    }

    public class CMPSTFLAG
    {
        public static ushort GLYF_CMPST_ARG_1_AND_2_ARE_WORDS = 0x0001;
        public static ushort GLYF_CMPST_ARGS_ARE_XY_VALUES = 0x0002;
        public static ushort GLYF_CMPST_ROUND_XY_TO_GRID = 0x0004;
        public static ushort GLYF_CMPST_WE_HAVE_A_SCALE = 0x0008;
        public static ushort GLYF_CMPST_RESERVE = 0x0010;
        public static ushort GLYF_CMPST_MORE_COMPONENT = 0x0020;
        public static ushort GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE = 0x0040;
        public static ushort GLYF_CMPST_WE_HAVE_A_TWO_BY_TWO = 0x0080;
        public static ushort GLYF_CMPST_WE_HAVE_INSTRUCTIONS = 0x0100;
        public static ushort GLYF_CMPST_USE_MY_METRICS = 0x0200;
        public static ushort GLYF_CMPST_OVERLAP_COMPOUND = 0x0400;
        public static ushort GLYF_CMPST_SCALED_COMPONENT_OFFSET = 0x0800;
        public static ushort GLYF_CMPST_UNSCALED_COMPONENT_OFFSET = 0x1000;
    }

}   // end of 
