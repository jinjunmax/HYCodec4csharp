using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYFontCodecCS
{
    public class CodeComparer : IComparer<uint>
    {
        public int Compare(uint x, uint y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (x > y)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

    }   // end of public class CodeComparer : IComparer<uint>

    public class HYCodeMapItem
    {
        public UInt32 Unicode { get; set; }
        public Int32 GID { get; set; }     
    }

    public class HYCodeMap
    {
        public void FindIndexByUnicode(UInt32 ulUnicode, List<Int32> GIDS)
        {
            for (int i = 0; i < lstCodeMap.Count; i++)
            {
                if (lstCodeMap[i].Unicode == ulUnicode)
                {
                    GIDS.Add(lstCodeMap[i].GID);
                }
            }

        }   // end of void FindIndexByUnicode()

        public Int32 FindIndexByUnicode(UInt32 ulUnicode)
        {
            for (int i = 0; i < lstCodeMap.Count; i++)
            {
                if (lstCodeMap[i].Unicode == ulUnicode)
                {
                    return lstCodeMap[i].GID;
                }
            }

            return -1;

        }   // end of public UInt32 FindIndexByUnicode()

        public void QuickSortbyUnicode()
        {
            lstCodeMap.Sort(delegate(HYCodeMapItem a, HYCodeMapItem b) { return a.Unicode.CompareTo(b.Unicode); });
        
        }   // end of public void QuickSortbyUnicode()

        public ulong FindMaxUnicode()
        {
            ulong tmp = 0x00;
            for (int i = 0; i < lstCodeMap.Count; i++)
            {   
                if (lstCodeMap[i].Unicode > tmp)
                {
                    tmp = lstCodeMap[i].Unicode;
                }
            }

            return tmp;

        }   // end of public void FindMaxUnicode()

        public ulong FindMinUnicode()
        {
            ulong tmp = 0xffffffff;
            for (int i = 0; i < lstCodeMap.Count; i++)
            {   
                if (lstCodeMap[i].Unicode < tmp)
                {
                    tmp= lstCodeMap[i].Unicode;
                }
            }

            return tmp;

        }   // end of public void FindMinUnicode()



        public List<HYCodeMapItem> lstCodeMap = new List<HYCodeMapItem>();    
    }
}
