using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HYFontCodecCS
{
    public class HYFontCodecCS 
    {     

        /************************************************************************/
        /* FontOpen : 创建或访问字库
         * String strFileName:  字库文件名称
         * FileMode FM:         文件打开标记    
         * int TableFlag：       TableFlag具体读取或写入哪几个表
         *
         * return               
        /************************************************************************/
        public HYRESULT FontOpen(string strFileName, FileMode FM, int TableFlag)
        {
            if (FM == FileMode.Open)
            {

            }

            return HYRESULT.NOERROR;

        }   // end of public HYRESULT FontOpen()

        

    }   // end of class HYFontCodecCS
}   // end of namespace HYFontCodecCS
