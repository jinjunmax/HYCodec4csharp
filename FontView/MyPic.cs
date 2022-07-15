using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HYFontCodecCS;
using System.Drawing.Drawing2D;
using FontParserEntity;

namespace FontView
{
    public partial class GlpyhWnd : PictureBox
    {        
        public float m_fLeftSide = 0;
        public float m_fRightSide = 0;        
        public float m_fWndBaseLine = 0;

        public float fBaseLine = 0;
        public float fScale = 0;

        public  CharInfo curGlyph;
        public CharsInfo FontGlyph;

        public List<PointF> lstTotalPt;
        public GraphicsPath GlyphGlyphPath;

        public List<GraphicsPath> lstCmpsPath;
        public List<int> lstCmpsIndex;

        public GlpyhWnd()
        {
            InitializeComponent();
            iMouseSel = -1;
        }

        public void SetGlyphInfo(CharInfo Glyph, CharsInfo Glyphs, CHead head, CHhea hhea, FONTTYPE fType)
        {
            curGlyph = Glyph;
            FontGlyph = Glyphs;

            GlyphGlyphPath = new GraphicsPath();
            lstCmpsPath = new List<GraphicsPath>();
            lstCmpsIndex = new List<int>();

            if (head.unitsPerEm> hhea.Ascender-hhea.Descender)
                fScale = (float)this.Size.Height/(float)head.unitsPerEm;
            else
                fScale = (float)this.Size.Height / (float)(hhea.Ascender - hhea.Descender);

            fBaseLine = Math.Abs(hhea.Descender);

            m_fWndBaseLine = (float)this.Size.Height - Math.Abs(fBaseLine * fScale);
            if (fType == FONTTYPE.TTF)
            {
                CountTTFInfo(Glyph);
            }

            if (fType == FONTTYPE.CFF)
            {
                CountCFFInfo(Glyph);                
            }            

            this.Invalidate();

	    }   // end of public void SetGlyphInfo()

        public void CountTTFInfo(CharInfo Glyph)
        {
            float fIndent = (Size.Width - Glyph.AdWidth * fScale) / 2.0f;

            if (Glyph.IsComponent == 0)
            {
                int EndposIndex = 0;
                PointF pfAdapter = new PointF();
                List<byte> lstF = new List<byte>();
                List<PointF> lstDrawPt = new List<PointF>();
                lstTotalPt = new List<PointF>();

                float xoffset = (float)(this.Size.Width - Glyph.AdWidth * fScale) / 2.0f;

                for (int i = 0; i < Glyph.ContourCount; i++)
                {
                    ContourInfo cnurInf = Glyph.ContourInfo[i];
                    for (int j = 0; j < cnurInf.PointCount; j++)
                    {
                        PtInfo pt = cnurInf.PtInfo[j];
                        pfAdapter.X = pt.X * fScale + xoffset;
                        pfAdapter.Y = (float)this.Size.Height - (pt.Y + fBaseLine) * fScale;

                        lstDrawPt.Add(pfAdapter);
                        lstF.Add((byte)pt.PtType);
                    }

                }

                int iPtIndex = 0;
                for (int i = 0; i < Glyph.ContourCount; i++)
                {
                    ContourInfo cnurInf = Glyph.ContourInfo[i];

                    int szPtNum = cnurInf.PointCount;
                    List<PointF> vtTmpPT = new List<PointF>();
                    List<int> vtTmpFlag = new List<int>();

                    for (int j = 0; j < szPtNum; j++)
                    {
                        vtTmpPT.Add(lstDrawPt[iPtIndex]);
                        vtTmpFlag.Add(lstF[iPtIndex]);

                        iPtIndex++;
                    }

                    if (vtTmpFlag[0] == 0x0000)//POINT_FLG_CONTROL
                        TrimPointlist(ref vtTmpPT,ref vtTmpFlag);


                    GraphicsPath cnturPath = new GraphicsPath();
                    PointF f1 = new PointF();
                    PointF f2 = new PointF();
                    int x = 0;
                    while (x < cnurInf.PointCount)
                    {
                        if (x == 0)
                        {
                            f1 = vtTmpPT[x];
                            x++;
                        }
                        else
                        {
                            if (vtTmpFlag[x] == 0x0001)//POINT_FLG_ANCHOR)
                            {
                                cnturPath.AddLine(f1, vtTmpPT[x]);
                                f1 = vtTmpPT[x];
                                x++;

                            }
                            else if (vtTmpFlag[x] == 0x0000)//POINT_FLG_CONTROL
                            {
                                if (x + 1 < cnurInf.PointCount)
                                {
                                    if (vtTmpFlag[x + 1] == 0x0001)//POINT_FLG_ANCHOR
                                    {
                                        CountBezierPath(f1, vtTmpPT[x], vtTmpPT[x + 1], cnturPath);

                                        f1 = vtTmpPT[x + 1];
                                        x += 2;
                                    }
                                    else if (vtTmpFlag[x + 1] == 0x0000)//POINT_FLG_CONTROL
                                    {
                                        f2.X = vtTmpPT[x].X + (vtTmpPT[x + 1].X - vtTmpPT[x].X) / 2.0f;
                                        f2.Y = vtTmpPT[x].Y + (vtTmpPT[x + 1].Y - vtTmpPT[x].Y) / 2.0f;

                                        CountBezierPath(f1, vtTmpPT[x], f2, cnturPath);
                                        f1 = f2;
                                        x++;
                                    }
                                }
                                else if (x + 1 == cnurInf.PointCount)
                                {
                                    CountBezierPath(f1, vtTmpPT[x], vtTmpPT[0], cnturPath);
                                    break;
                                }
                            }
                        }
                    }
                    cnturPath.CloseFigure();
                    GlyphGlyphPath.AddPath(cnturPath, false);
                }
            }
            else
            { 
                // 针对组合轮廓转换
		        short sValue = 0;
		        float fXScale = 1.0f, fYScale = 1.0f, fValue = 0.0f, fFraction = 0.0f;
                int iCmpnnts = Glyph.CmpInf.Count;

		        for (int j=0; j<iCmpnnts; j++)
		        {
			        fXScale = 1.0f; 
			        fYScale = 1.0f;
                    CmpInf hyCmpst = Glyph.CmpInf[j];
                    GraphicsPath CmpGlyphPath = new GraphicsPath();

                    int sGlyphIndex = hyCmpst.Gid;
			        if ((hyCmpst.Flag&0x0002)>0)//GLYF_CMPST_ARGS_ARE_XY_VALUES
			        {
                        if ((hyCmpst.Flag&0x0008)>0)//GLYF_CMPST_WE_HAVE_A_SCALE
				        {
					        fYScale = fXScale = HYBase.HY_F2DOT14_to_float((short)hyCmpst.Scale);
				        }

				        if ((hyCmpst.Flag&0x0040)>0)//GLYF_CMPST_WE_HAVE_AN_X_AND_Y_SCALE
				        {
                            fXScale = HYBase.HY_F2DOT14_to_float((short)hyCmpst.ScaleX);
                            fYScale = HYBase.HY_F2DOT14_to_float((short)hyCmpst.ScaleY);
				        }

				        // 计算路径
				        PointF		pfAdapter = new PointF();
                        if (sGlyphIndex > FontGlyph.CharInfo.Count) continue;
				        if (sGlyphIndex<0)	continue;

                        CharInfo cmpGlyph = FontGlyph.CharInfo[sGlyphIndex];					

				        int		szCntunNum = cmpGlyph.ContourCount;						
				        for (int i=0; i<szCntunNum; i++)
				        {
					        ContourInfo				hyCntur = cmpGlyph.ContourInfo[i];
                            int                     szPtNum = hyCntur.PointCount;
                         
					        List<PointF>		    vtTmpPT = new List<PointF>();
                            List<int>               vtTmpFlag = new List<int>();

					        GraphicsPath	cnturPath = new GraphicsPath();
					        for(int x=0; x<szPtNum; x++)
					        {							
						        PtInfo htPt = hyCntur.PtInfo[x];

                                if ((hyCmpst.Flag & 0x0002)>0)//GLYF_CMPST_ARGS_ARE_XY_VALUES
						        {							
							        pfAdapter.X = htPt.X*fXScale;
							        pfAdapter.Y = htPt.Y*fYScale;
							
							        // 后偏移
							        pfAdapter.X = (float)pfAdapter.X+hyCmpst.Arg[0];
							        pfAdapter.Y = (float)pfAdapter.Y+hyCmpst.Arg[1];														
						        }
						        else 
						        {
							        // 先缩放
							        pfAdapter.X = htPt.X*fXScale;
							        pfAdapter.Y = htPt.Y*fYScale;

							        // 后偏移
							        pfAdapter.X = (float)htPt.X+hyCmpst.Arg[0];
							        pfAdapter.Y = (float)htPt.Y+hyCmpst.Arg[1];
						        }                              

						        // 适配到windows坐标系左上角
						        PointF wndPt = new PointF();
                                wndPt.X = pfAdapter.X * fScale + fIndent;
                                wndPt.Y = (float)this.Size.Height - (pfAdapter.Y + fBaseLine) * fScale;

                                vtTmpPT.Add(wndPt);
						        vtTmpFlag.Add(htPt.PtType);
					        }

					        if (vtTmpFlag.Count>0)
					        {
                                if (vtTmpFlag[0] == 0x0000)//POINT_FLG_CONTROL
							        TrimPointlist(ref vtTmpPT, ref vtTmpFlag);

                                GraphicsPath CmpCntunPath = new GraphicsPath();
                                PointF f1 = new PointF();
                                PointF f2 = new PointF();
                                int x = 0;
                                while (x < vtTmpPT.Count)
                                {
                                    if (x == 0)
                                    {
                                        f1 = vtTmpPT[x];
                                        x++;
                                    }
                                    else
                                    {
                                        if (vtTmpFlag[x] == 0x0001)//POINT_FLG_ANCHOR)
                                        {
                                            cnturPath.AddLine(f1, vtTmpPT[x]);
                                            f1 = vtTmpPT[x];
                                            x++;

                                        }
                                        else if (vtTmpFlag[x] == 0x0000)//POINT_FLG_CONTROL
                                        {
                                            if (x + 1 < vtTmpPT.Count)
                                            {
                                                if (vtTmpFlag[x + 1] == 0x0001)//POINT_FLG_ANCHOR
                                                {
                                                    CountBezierPath(f1, vtTmpPT[x], vtTmpPT[x + 1], CmpCntunPath);

                                                    f1 = vtTmpPT[x + 1];
                                                    x += 2;
                                                }
                                                else if (vtTmpFlag[x + 1] == 0x0000)//POINT_FLG_CONTROL
                                                {
                                                    f2.X = vtTmpPT[x].X + (vtTmpPT[x + 1].X - vtTmpPT[x].X) / 2.0f;
                                                    f2.Y = vtTmpPT[x].Y + (vtTmpPT[x + 1].Y - vtTmpPT[x].Y) / 2.0f;

                                                    CountBezierPath(f1, vtTmpPT[x], f2, CmpCntunPath);
                                                    f1 = f2;
                                                    x++;
                                                }
                                            }
                                            else if (x + 1 == vtTmpPT.Count)
                                            {
                                                CountBezierPath(f1, vtTmpPT[x], vtTmpPT[0], CmpCntunPath);
                                                break;
                                            }
                                        }
                                    }
                                }
                                CmpCntunPath.CloseFigure();
                                CmpGlyphPath.AddPath(CmpCntunPath, false);
					        }
				        }
                        GlyphGlyphPath.AddPath(CmpGlyphPath, false);
                        lstCmpsPath.Add(CmpGlyphPath);
                        lstCmpsIndex.Add(sGlyphIndex);
			        }				
		        }
            }

        }    // end of  public void CountTTFInfo()

        void TrimPointlist(ref List <PointF> vtPoints,ref List <int> vtPointsFlg)
        {
            List<PointF> vtTmpPT = new List<PointF>(vtPoints);
            List<int> vtTmpFlag = new List<int>(vtPointsFlg);

            vtPoints.Clear();
            vtPointsFlg.Clear();
	
	        int vtSize = vtTmpPT.Count;
	        int  vtTmpIndex = -1;
	        for (int i=0; i<vtSize; i++)
	        {
                if (vtTmpFlag[i] == 0x0001)//POINT_FLG_ANCHOR
		        {
			        vtTmpIndex = i;
			        break;
		        }		
	        }
	
	        if (vtTmpIndex!=-1)
	        {
		        for (int i=vtTmpIndex; i<vtSize; i++)
		        {
			        vtPoints.Add(vtTmpPT[i]);
			        vtPointsFlg.Add(vtTmpFlag[i]);
		        }

		        for (int i=0; i<vtTmpIndex; i++)
		        {
			        vtPoints.Add(vtTmpPT[i]);
			        vtPointsFlg.Add(vtTmpFlag[i]);
		        }
	        }
	        else // 如果整个轮廓没有ONLine点，将起点设为Online点
	        {
		        PointF		Pt1 = vtTmpPT[0];
		        PointF		Pt2 = vtTmpPT[vtSize-1];		
		        PointF		PtStart = new PointF();
		        PtStart.X = Pt1.X+((Pt2.X-Pt1.X)/2);
		        PtStart.Y = Pt1.Y+((Pt2.Y-Pt1.Y)/2);	

		        vtPoints.Add(PtStart);
                vtPointsFlg.Add(0x0001);//POINT_FLG_ANCHOR

		        for (int i=0; i<vtSize; i++)
		        {			
			        vtPoints.Add(vtTmpPT[i]);			 
			        vtPointsFlg.Add(vtTmpFlag[i]);
		        }		
	        }	

        }	// end of void TrimPointlist()

        public void CountCFFInfo(CharInfo Glyph)
        {            
            PointF pfAdapter = new PointF();

            float xoffset = (float)(this.Size.Width - Glyph.AdWidth * fScale) / 2.0f;
            for (int i = 0; i < Glyph.ContourCount; i++)
            {
                List<byte> lstF = new List<byte>();
                List<PointF> lstDrawPt = new List<PointF>();            

                ContourInfo ctrInf = Glyph.ContourInfo[i];
                
                for (int j=0; j<ctrInf.PointCount; j++)
                {
                    PtInfo pt = ctrInf.PtInfo[j];

                    pfAdapter.X = pt.X*fScale + xoffset;
                    pfAdapter.Y = (float)this.Size.Height - (pt.Y + fBaseLine) * fScale;

                    lstDrawPt.Add(pfAdapter);                    
                    lstF.Add((byte)pt.PtType);
                }

                GraphicsPath grphPath = new GraphicsPath();
                int x = 0;
                PointF f1 = new PointF();
                while (x < ctrInf.PointCount)
                {
                    if (x == 0)
                    {
                        f1 = lstDrawPt[x];
                        x++;
                    }
                    else
                    {
                        if (lstF[x] == 0x0001)//lstF
                        {
                            grphPath.AddLine(f1, lstDrawPt[x]);
                            f1 = lstDrawPt[x];
                            x++;
                        }
                        else if (lstF[x] == 0x0000) //POINT_FLG_CONTROL
                        {
                            if (x + 1 == ctrInf.PointCount)
                            {
                                grphPath.Reset();
                                break;
                            }

                            if (x + 2 < ctrInf.PointCount)
                            {
                                grphPath.AddBezier(f1, lstDrawPt[x], lstDrawPt[x + 1], lstDrawPt[x + 2]);
                                f1 = lstDrawPt[x + 2];
                                x += 3;
                            }
                            else if (x + 2 == ctrInf.PointCount)
                            {
                                grphPath.AddBezier(f1, lstDrawPt[x], lstDrawPt[x + 1], lstDrawPt[0]);
                                break;
                            }
                        }
                    }
                }
                grphPath.CloseFigure();
                GlyphGlyphPath.AddPath(grphPath, false);
            }

        }   // end of public void CountCFFInfo()

        public void CountBezierPath(PointF pt0, PointF pt1, PointF pt2, GraphicsPath path)
        {
	        int		    iCurvePSize = 10;
	        PointF[]	Curve = new System.Drawing.PointF[10];

	        CountBezierCurvePos(pt0, pt1, pt2,ref Curve, ref iCurvePSize);

	        path.AddCurve(Curve);

        }	// end of void CFontShowWnd::CountBezierPath()

        public void  CountBezierCurvePos(PointF inP0, PointF inP1, PointF inP2, ref PointF[] otP, ref int iioPtNum)
        {
	        float   dt = 0; 
	        float   X = 0.0f, Y = 0.0f;
	        int     Z = 0;    
            
            for ( int i = 0; i < iioPtNum ; i++ )
	        {	
		        dt =  i / (float)(iioPtNum - 1);

		        if (i == 0 )
		        {
			        otP[i].X = (float)inP0.X * ( 1.0f - dt ) * ( 1.0f - dt ) + 2 * dt * ( 1.0f - dt) * (float)inP1.X  + dt * dt * (float)inP2.X;	
			        otP[i].Y = (float)inP0.Y * ( 1.0f - dt ) * ( 1.0f - dt ) + 2 * dt * ( 1.0f - dt) * (float)inP1.Y  + dt * dt * (float)inP2.Y;				
		        }
		        else
		        {
			        X = (float)inP0.X * ( 1.0f - dt ) * ( 1.0f - dt ) + 2 * dt * ( 1.0f - dt) * (float)inP1.X  + dt * dt * (float)inP2.X;
			        Y =(float) inP0.Y * ( 1.0f - dt ) * ( 1.0f - dt ) + 2 * dt * ( 1.0f - dt) * (float)inP1.Y  + dt * dt *(float) inP2.Y;

			        if ((otP[i - 1].X == X) && (otP[i - 1].Y == Y) ) 
			        {
				        continue;
			        }
			        else 
			        {
				        otP[Z].X = X;
				        otP[Z].Y = Y;
			        }
		        }
		        Z++;
	        }		

	        iioPtNum = Z;

        }	// end of void  CFontShowWnd::CountBezierCurvePos()

        public int iMouseSel { get; set; }
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics bmpGraphics = pe.Graphics;
            bmpGraphics.SmoothingMode = SmoothingMode.HighQuality;

            Pen		pnBk = new Pen(Color.Red,2);
            Pen pnSelF = new Pen(Color.Blue, 2);

            Brush   bsh = new SolidBrush(Color.Black);
            Brush   bshSel = new SolidBrush(Color.Blue);

            if (GlyphGlyphPath != null)
            {
                bmpGraphics.DrawPath(pnBk, GlyphGlyphPath);
                /*
                Font drawFont = new Font("Arial", 10);                
                for (int i = 0; i < Glyph.GlyfTableItemBody.numberOfPoints; i++)
                {
                    if (i == iMouseSel)
                    {
                        bmpGraphics.DrawEllipse(pnSelF, lstTotalPt[i].X - 4, lstTotalPt[i].Y - 4, 8, 8);
                        bmpGraphics.DrawString(i.ToString(), drawFont, bshSel, lstTotalPt[i]);                    
                    }
                    else
                    {
                        bmpGraphics.DrawEllipse(pnBk, lstTotalPt[i].X - 4, lstTotalPt[i].Y - 4, 8, 8);
                        bmpGraphics.DrawString(i.ToString(), drawFont, bsh, lstTotalPt[i]);                    
                    }
                }
                 * */
            }

            base.OnPaint(pe);

        }   // end of protected override void OnPaint()

        public delegate void OnMyClickEventHandle(object sender, string  strPt);
        public OnMyClickEventHandle OnMyClickEvent;

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            /*
            for (int i = 0; i < curGlyph.ContourCount; i++)
            {
                for (int )
                RectangleF rt = new RectangleF(lstTotalPt[i].X - 4, lstTotalPt[i].Y - 4, 8, 8);

                if (rt.Contains(e.X,e.Y))
                {
                    int Tmp = this.Size.Height - ((int)lstTotalPt[i].Y - (int)m_fWndBaseLine);
                    string strPt = lstTotalPt[i].X.ToString() + " : " + Tmp.ToString();
                    OnMyClickEvent(this, strPt);
                    iMouseSel = i;
                    Invalidate();
                    return;
                }
            }
             * */

        }   // end of protected override void OnMouseClick()
         
    }
}
