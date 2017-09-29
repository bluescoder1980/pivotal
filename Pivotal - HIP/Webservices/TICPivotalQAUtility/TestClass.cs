using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TICPivotalQAUtility
{
    class TestClass
    {

        static void Main(string[] args)
        {
            MD5Helper test = new MD5Helper();
            Console.WriteLine(test.GetMessageDigest("password"));
         

        }
    }
}
