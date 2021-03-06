﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EasyPay2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string returnUrl, string statusUrl, string orderGuid, decimal Amount)
        {
            var merchantId = ConfigurationSettings.AppSettings["MerchantId"];
            var currency = ConfigurationSettings.AppSettings["Currency"];
            var paymentUrl = ConfigurationSettings.AppSettings["PaymentUrl"];
            //var SECURITY_SEQ = ConfigurationSettings.AppSettings["SECURITY_SEQ"];
            var SECURITY_KEY = ConfigurationSettings.AppSettings["SECURITY_KEY"];

            /* Amount : numeric and 2 decimal places, e.g. 9.15, 25.50, 5.00 */
            var amt = Math.Round(Amount, 2).ToString();

            /*
             * rcard=04 display last 4 digits of card no, ex. card number is 4111-1111-1111-1111,
             * EasyPay2 will return TM_CardNum = XXXX-XXXX-XXXX-1111
             */
            var rcard = "04";

            /*
             * A unique transaction reference generated by merchant.
               This number cannot be recycled even though the previous transaction could be unsuccessful.
             */
            var uniqueRef = orderGuid;
            
            /*
             * SECURITY_SEQ : amt,ref,cur,mid,transtype (this is my SECURITY_SEQ)
             * signature = sha512("amt + ref + cur + mid + transtype + SECURITY_KEY")
             */
            SHA512 shaM = new SHA512Managed();
            var source = Encoding.Default.GetBytes(string.Format("{0}{1}{2}{3}{4}{5}", amt, uniqueRef, currency, merchantId, "", SECURITY_KEY));
            var hashValue = shaM.ComputeHash(source);
            var signature = string.Join("", hashValue.Select(h => h.ToString("x2"))).ToUpper();
            string requestUrl = paymentUrl + "mid={0}&ref={1}&amt={2}&cur={3}&rcard={4}&returnurl={5}&version=2&signature={6}&statusurl={7}";

            requestUrl = string.Format(requestUrl, merchantId, uniqueRef, amt, currency, rcard, returnUrl, signature, statusUrl);

            return Redirect(requestUrl);
        }


        public ActionResult EasyPayResponse(string orderguid)
        {
            return View();
        }

        /* It is recommended that the merchant sends EasyPay2 an acknowledgement once they have
            received the updates via statusurl.This is a back-end operation and the acknowledgement is sent via the same connection. Once
            EasyPay2 receives the acknowledgement message from the merchant, EasyPay2 will redirect the
            customer to the merchant’s returnurl. 
            */

        [HttpPost]
        public ActionResult EasyPayStatusResponse()
        {
            var request = Request;

            var TM_RefNo = Request["TM_RefNo"];
            var TM_Signature = Request["TM_Signature"];
            var TM_Currency = Request["TM_Currency"];
            var TM_Error = Request["TM_Error"];
            var TM_DebitAmt = Request["TM_DebitAmt"];
            var TM_Status = Request["TM_Status"];
            var TM_TrnType = Request["TM_TrnType"];
            var merchantId = ConfigurationSettings.AppSettings["MerchantId"];
            var SECURITY_KEY = ConfigurationSettings.AppSettings["SECURITY_KEY"];

            SHA512 shaM1 = new SHA512Managed();
            var source1 = Encoding.Default.GetBytes(string.Format("{0}{1}{2}{3}{4}{5}", TM_DebitAmt, TM_RefNo, TM_Currency, merchantId, TM_TrnType, SECURITY_KEY));
            var hashValue1 = shaM1.ComputeHash(source1);
            var signature1 = string.Join("", hashValue1.Select(h => h.ToString("x2"))).ToUpper();

            SHA512 shaM = new SHA512Managed();
            var source = Encoding.Default.GetBytes(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", TM_DebitAmt, TM_RefNo, TM_Currency, merchantId, TM_TrnType, TM_Status, TM_Error, SECURITY_KEY));
            var hashValue = shaM.ComputeHash(source);
            var signature = string.Join("", hashValue.Select(h => h.ToString("x2"))).ToUpper();


            var paymentUrl = ConfigurationSettings.AppSettings["PaymentUrl"];
            //var merchantId = ConfigurationSettings.AppSettings["MerchantId"];

            var requestUrl = string.Format(paymentUrl + "mid={0}&ref={1}&ack=YES", merchantId, TM_RefNo);

            return View();
            //return Redirect(requestUrl);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {           
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}