using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebServiceTester.QAServiceTest;


namespace WebServiceTester
{
    public class ServiceTest
    {

        static void Main(string[] args)
        {
            //Create instance of service
            //TICPivotalQAServiceSvcSoapClient proxy = new TICPivotalQAServiceSvcSoapClient();
            TICPivotalQAServiceSvc proxy = new TICPivotalQAServiceSvc();
            //Get Digested Hash to pass for authentication
            string strDigestedPass = proxy.GetPivotalMD5MessageDigest("password");

            //Authenticate user
            try
            {
                //If user name or password is invalid a soap exception is thrown with reason
                InspectorObj inspObj = proxy.AuthenticateUserLogin("amaldonado@irvinecompany.com", strDigestedPass);
                Console.WriteLine("Hello " + inspObj.First_Name + ", welcome to the Quality Assurance Application!");

               

            }
            catch (Exception e)
            {
                Console.WriteLine("Authentication Failed : " + e.Message);
            }
           

            
           
        }
    }
}
