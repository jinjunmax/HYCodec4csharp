using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class LANGTAGRECORD
	{
		public UInt16	    length {get;set;}
        public UInt16       offset  {get;set;}			
		public string		strContent {get;set;}
	}

	public class NAMERECORD
	{			
		public	UInt16	platformID  {get;set;}			
		public	UInt16  encodingID  {get;set;}	
		public	UInt16  languageID  {get;set;}  			
		public	UInt16  nameID      {get;set;}				
		public	UInt16  length      {get;set;}			
		public	UInt16  offset      {get;set;}
		public 	string	strContent  {get;set;}
	}

    public class CName
    {
        public UInt16                   Format          {get;set;}
		public UInt16					count           {get;set;}
		public UInt16					offset          {get;set;}
		public List<NAMERECORD>			    vtNameRecord = new List<NAMERECORD>();
        public List<LANGTAGRECORD> vtLangTargeRecord = new List<LANGTAGRECORD>();
		string						    strData         {get;set;}
    }
}
