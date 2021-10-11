using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CRangeRecord
	{   
		public	UInt16 Start                {get;set;}
		public	UInt16 End                  {get;set;}
		public	UInt16 StartCoverageIndex   {get;set;}
    }

	public class CCoverage
	{		
	    public UInt16		        CoverageFormat{get;set;}
	    public UInt16		        GlyphCount{get;set;}		
        public UInt16				RangeCount{get;set;}
	    public List<UInt16>		    vtGlyphID =  new 	List<UInt16>();
        public List<CRangeRecord>	vtRangeRecord = new List<CRangeRecord>();
	}

	public class  CLookUpSingleSubstitution1
	{
        public CCoverage					Coverage = new CCoverage();
		public Int16						DeltaGlyphID {get;set;}
	}

	public class  CLookUpSingleSubstitution2
	{		
		public UInt16				GlyphCount;
		public CCoverage			Coverage = new CCoverage();
        public List<UInt16> vtGlyphID = new List<UInt16>();
	}

	public class CLookUpType1
	{	
		public	UInt16					SubFormat {get;set;}	
		public	CLookUpSingleSubstitution1		Substitution1 = new CLookUpSingleSubstitution1();
		public	CLookUpSingleSubstitution2		Substitution2 = new CLookUpSingleSubstitution2();
	}

	public class CLookUp
	{
        public UInt16 LookUpType { get; set; }
        public UInt16 LookUpFlag { get; set; }
        public UInt16 SubTableCount { get; set; }
        public List<CLookUpType1> vtLookupType1 = new List<CLookUpType1>();
	}

    public class  CLangSys
	{        
		public	UInt16				ReqFeatureIndex {get;set;}
		public	UInt16				FeatureCount{get;set;}
		public  List<UInt16>	    vtFeatureIndex = new List<UInt16>() ;	
	}

	public class CLangSysRecord
	{	
		public	byte[]			SysTag = new byte[4];
        public  CLangSys		LangSys = new CLangSys();		
	}

	public class CScript
	{			
		public CLangSys	DefaultLangSys = new CLangSys();			
		public List<CLangSysRecord>	vtLangSysRecord = new List<CLangSysRecord>();
	}

	public class CScriptRecord
	{	
        public	byte[]	Tag = new byte[4];		
		public  CScript	Script = new CScript();		
	}

    public class  CFeatureTable
	{	
	    public List<UInt16>	vtLookupListIndex = new List<UInt16>();
	}

	public class  CFeatureRecord 
	{	    
		public	byte[]	Tag = new byte[4];
		public  CFeatureTable FeatureTable = new CFeatureTable();
	}

    public class CGSUB
    {
        public HYFIXED version = new HYFIXED();
        public List<CLookUp> vtLookupList = new List<CLookUp>();
        public List<CScriptRecord> vtScriptsList = new List<CScriptRecord>();
        public List<CFeatureRecord> vtFeaturesList = new List<CFeatureRecord>();
    }
}
