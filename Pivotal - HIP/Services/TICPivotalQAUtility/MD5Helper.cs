using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace TICPivotalQAUtility
{
    public class MD5Helper
    {

        public MD5Helper() 
        { }


        /// <summary>
        /// This method will be used to create the message digest needed to authenticate
        /// against the Contact_Web_Details Password_Encrypt.  See SI Below
        /// SI # 104157: 1/15/2008 8:11:25 AM: (3-71) GMT: Business Server (Lifecycle)
        /// </summary>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        public string GetMessageDigest(string strPassword)
        {
            RMD5Lib.RMD5DigestClass rmd = new RMD5Lib.RMD5DigestClass();
            return rmd.GetDigest(strPassword);            
        }

   


    }
}
