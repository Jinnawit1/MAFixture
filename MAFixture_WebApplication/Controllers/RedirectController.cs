using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MAFixture_WebApplication.Controllers
{
    public class RedirectController : Controller
    {
        // หน้าหลักที่เช็คเงื่อนไข
        public ActionResult Index()
        {
            var userAgent = Request.UserAgent.ToLower();
            var requestUrl = Request.Url.ToString();
            var isLocalhost = requestUrl.Contains("localhost") || requestUrl.Contains("127.0.0.1");

            // Case 1: If the user is on localhost, redirect to the Home page
            if (isLocalhost)
            {
                return RedirectToAction("Index", "Global");
            }

            // Case 2: If the device is mobile or tablet (Android, iPhone, etc.)
            if (userAgent.Contains("mobi") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            {
                return Redirect("https://10.52.60.227/MATesting/Global");
            }
            // Case 3: If the device is a Windows desktop
            else if (userAgent.Contains("windows"))
            {
                return Redirect("https://rdreporters/MATesting/Global");
            }

            // Default case for unknown devices (smart TVs, etc.)
            else
            {
                return Redirect("https://10.52.60.227/MATesting/Global");
            }
        }
	}
}