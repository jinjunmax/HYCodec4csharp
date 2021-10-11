﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public enum HYRESULT
    {         
        NOERROR	            =				0,        
        FILE_NOEXIST        =				1001,
        FILE_OPEN 	        =				1002,
        FILE_READ 	        =				1003,
        FILE_WRITE          =  				1004,
        FILE_EXIST          = 				1005,        
        CMAP_DECODE			=				2001,
        CMAP_ENCODE			=				2002,
        GLYF_DECODE			=				2003,
        GLYF_ENCODE			=				2004,
        HEAD_DECODE			=				2005,
        HEAD_ENCODE			=				2006,
        HHEA_DECODE			=				2007,
        HHEA_ENCODE			=				2008,
        HMTX_DECODE			=				2009,
        HMTX_ENCODE			=				2010,
        LOCA_DECODE			=				2011,
        LOCA_ENCODE			=				2012,
        MAXP_DECODE			=				2013,
        MAXP_ENCODE			=				2014,
        NAME_DECODE			=				2015,
        NAME_ENCODE			=				2016,
        POST_DECODE			=				2017,
        POST_ENCODE			=				2018,
        OS2_DECODE			=				2019,
        OS2_ENCODE			=				2020,
        LTSH_DECODE			=				2021,
        LTSH_ENCODE			=				2022,
        VDMX_DECODE			=				2023,
        VDMX_ENCODE			=				2024,
        CVT_DECODE			=				2025,
        CVT_ENCODE			=				2026,
        FPGM_DECODE			=				2027,
        FPGM_ENCODE			=				2028,
        GASP_DECODE			=				2029,
        GASP_ENCODE			=				2030,
        HDMX_DECODE			=				2031,
        HDMX_ENCODE			=				2032,
        PREP_DECODE			=				2033,
        PREP_ENCODE         =				2034,
        DSIG_DECODE         =				2035,
        DSIG_ENCODE         =				2036,
        VHEA_DECODE         =				2037,
        VHEA_ENCODE         =				2038,
        VMTX_DECODE         =				2039,
        VMTX_ENCODE         =				2040,
        CFF_DECODE	        =				2041,
        CFF_ENCODE	        =				2042,
        GSUB_DECODE         =				2043,
        GSUB_ENCODE         =				2044,
        DIRECTORY_DECODE    =			    2045,
        DIRECTORY_ENCODE    =			    2046,
        CMAP_NOEXIST        =               2047,
        GLYF_NOEXIST        =               2048,
        HEAD_NOEXIST        =               2049,
        HHEA_NOEXIST        =               2050,   
        HMTX_NOEXIST        =               2051,
        LOCA_NOEXIST        =               2052,
        MAXP_NOEXIST        =               2053,   
        NAME_NOEXIST        =               2054,
        POST_NOEXIST        =               2055,
        OS2_NOEXIST         =               2056,
        LTSH_NOEXIST        =               2057,
        VDMX_NOEXIST        =               2058,    
        CVT_NOEXIST         =               2059,
        FPGM_NOEXIST        =               2060,
        GASP_NOEXIST        =               2061,
        HDMX_NOEXIST        =               2062,
        PREP_NOEXIST        =               2063,
        DSIG_NOEXIST        =               2064,
        VHEA_NOEXIST        =               2065,
        VMTX_NOEXIST        =               2066,
        CFF_NOEXIST         =               2067,
        GSUB_NOEXIST        =               2068,
        DIRECTORY_NOEXIST   =               2069,
        COLR_NOEXIST        =               2070,
        COLR_DECODE         =               2071,
        COLR_ENCODE         =               2072,
        NO_FONT             =               2200,
        NO_TTF	            =				2201,
        NO_CFF	            =				2202,
        NO_TTC              =               2203,
        TTC_TO_FONT         =               2204,
        FUNC_PARA	        = 			    3000,
        EXTRACT_ZERO        =               3100
    }
}
