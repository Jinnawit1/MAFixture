using MAFixture_WebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Microsoft.Owin.Security;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.Threading;
using System.Drawing;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace MAFixture_WebApplication.Controllers
{
    public class GlobalController : Controller
    {
        #region Database
        static bool IsDebugMode = false;

        public GlobalController()
        {
            if (String.IsNullOrEmpty(_ConnectionString_Traningdatabase))
            {
                SetConnectionString_Traningdatabase();
            }
            if (String.IsNullOrEmpty(_ConnectionString_MAFixture))
            {
                SetConnectionString_MAFixture();
            }
            if (String.IsNullOrEmpty(_ConnectionString_WIPstatus))
            {
                SetConnectionString_WIPstatus();
            }
        }

        private static string _ConnectionString_MAFixture;
        private static string _ConnectionString_Traningdatabase;
        private static string _ConnectionString_WIPstatus;

        public static MAFixtureEntities SettingAccountMAFixture()
        {
            MAFixtureEntities Entities = new MAFixtureEntities();
            if (IsDebugMode)
            {
                Entities.Database.Connection.ConnectionString = @"Server=127.0.0.1;Uid=sa;PASSWORD=Ubuntu2010@gpv;database=MAFixture;Max Pool Size=400;Connect Timeout=600;MultipleActiveResultSets=True;App=EntityFramework;";
            }
            else
            {
                Entities.Database.Connection.ConnectionString = _ConnectionString_MAFixture;
            }

            return Entities;
        }
        public static TraningdatabaseEntities SettingAccountTraningdatabase()
        {
            TraningdatabaseEntities Entities = new TraningdatabaseEntities();
            if (IsDebugMode)
            {
                Entities.Database.Connection.ConnectionString = @"Server=127.0.0.1;Uid=sa;PASSWORD=Ubuntu2010@gpv;database=Traningdatabase;Max Pool Size=400;Connect Timeout=600;MultipleActiveResultSets=True;App=EntityFramework;";
            }
            else
            {
                Entities.Database.Connection.ConnectionString = _ConnectionString_Traningdatabase;
            }

            return Entities;
        }
        public static WipEntities SettingAccountWIPstatus()
        {
            WipEntities Entities = new WipEntities();
            if (IsDebugMode)
            {
                Entities.Database.Connection.ConnectionString = @"Server=127.0.0.1;Uid=sa;PASSWORD=Ubuntu2010@gpv;database=WIP_status;Max Pool Size=400;Connect Timeout=600;MultipleActiveResultSets=True;App=EntityFramework;";
            }
            else
            {
                Entities.Database.Connection.ConnectionString = _ConnectionString_WIPstatus;
            }

            return Entities;
        }

        private void SetConnectionString_MAFixture()
        {
            using (WCFService_225.Service1Client WCF225 = new WCFService_225.Service1Client("BasicHttpBinding_IService11"))
            {
                _ConnectionString_MAFixture = WCF225.GetConnectMAFixture();
            }

        }
        private void SetConnectionString_Traningdatabase()
        {
            using (WCFService.Service1Client WCF227 = new WCFService.Service1Client())
            {
                _ConnectionString_Traningdatabase = WCF227.ConnectTrainingdatabase();
            }
        }
        private void SetConnectionString_WIPstatus()
        {
            using (WCFService.Service1Client WCF227 = new WCFService.Service1Client())
            {
                _ConnectionString_WIPstatus = WCF227.ConnectWIPstatus();
            }
        }
        #endregion
        #region Cookie

        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public bool CheckLogin() { if (Request.IsAuthenticated) { return false; } return true; }

        public ActionResult LogIn(String Login, String Password, String GUID)
        {
            String User = Login; bool Success = false;

            TraningdatabaseEntities Entity = new TraningdatabaseEntities();

            Cookie cook = new Cookie();

            if (string.IsNullOrEmpty(Login) && string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, data = User, url = Url.Action("Index", "Global", null, Request.Url.Scheme, null) }, JsonRequestBehavior.AllowGet);
            }

            var Emp = Entity.Tbl_employee.Where(a => a.Login == Login && a.Password == Password).ToList();
            if (Emp.Any())
            {
                var identity = new ClaimsIdentity(
                    new[] {
                        new Claim("EMPLOYEE_ACCOUNT", Emp.FirstOrDefault().Accountname ?? ""), 
                        new Claim("EMPLOYEE_ID", Emp.FirstOrDefault().EmpID ?? ""), 
                        new Claim("EMPLOYEE_NAME", Emp.FirstOrDefault().Name ?? ""), 
                        new Claim("EMPLOYEE_EMAIL", Emp.FirstOrDefault().Email ?? ""),
                        new Claim("EMPLOYEE_DEPARTMENT", Emp.FirstOrDefault().Depart ?? "")
                    },
                    DefaultAuthenticationTypes.ApplicationCookie);

                Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                }, identity);

                var accountName = Emp.FirstOrDefault().Accountname ?? "";
                var department = Emp.FirstOrDefault().Depart ?? "";
                User = accountName + ", " + department;

                Success = true;
            }
            else
            {
                Success = false;
            }

            Entity.Dispose();

            return Json(new { success = Success, data = User, url = Url.Action("Index", "Global", null, Request.Url.Scheme, null) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCookie()
        {
            if (!Request.IsAuthenticated) { return Json(new { success = false }, JsonRequestBehavior.AllowGet); }

            string EMPLOYEE_ACCOUNT = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ACCOUNT").Value;
            string EMPLOYEE_ID = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            string EMPLOYEE_NAME = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_NAME").Value;
            string EMPLOYEE_EMAIL = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_EMAIL").Value;
            string EMPLOYEE_DEPARTMENT = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_DEPARTMENT").Value;

            if (String.IsNullOrEmpty(EMPLOYEE_ID))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data = (EMPLOYEE_ACCOUNT + ", " + EMPLOYEE_DEPARTMENT) }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DelCookie() { Authentication.SignOut(); return Json(new { success = true }, JsonRequestBehavior.AllowGet); }

        #endregion
        //
        // GET: /Global/
        public ActionResult Index()
        {
            try
            {
                ViewData["EMPLOYEE_ID"] = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            }
            catch
            {
                ViewData["EMPLOYEE_ID"] = "";
            }
            return View();
        }
        public ActionResult MaintenanceHistory(string id)
        {
            try
            {
                ViewData["MA_ID"] = id;
            }
            catch
            {
                ViewData["MA_ID"] = "";
            }
            return View();
        }
        public ActionResult NewEquipment(string id)
        {
            try
            {
                ViewData["MA_ID"] = id;
                ViewData["EMPLOYEE_ID"] = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            }
            catch
            {
                ViewData["MA_ID"] = "";
                ViewData["EMPLOYEE_ID"] = "";
            }
            return View();
        }
        public ActionResult FixtureMaintenance()
        {
            try
            {
                ViewData["EMPLOYEE_ID"] = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            }
            catch
            {
                ViewData["EMPLOYEE_ID"] = "";
            }
            return View();
        }
        public ActionResult ObsoleteFixture()
        {
            return View();
        }
        public ActionResult Customer()
        {
            return View();
        }
        public ActionResult EmailList()
        {
            return View();
        }
        public ActionResult MasterMAList()
        {
            return View();
        }
        public ActionResult MasterToolsList()
        {
            try
            {
                ViewData["EMPLOYEE_ID"] = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            }
            catch
            {
                ViewData["EMPLOYEE_ID"] = "";
            }

            bool isIndustrial = false;
            string dept = "";
            string userEmail = "";

            try
            {
                var emailClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_EMAIL");
                if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                {
                    userEmail = emailClaim.Value.Trim();
                }
            }
            catch { }

            try
            {
                var deptClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_DEPARTMENT");
                if (deptClaim != null && !string.IsNullOrEmpty(deptClaim.Value))
                {
                    dept = deptClaim.Value;
                    if (dept.IndexOf("Industrial", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isIndustrial = true;
                    }
                }
            }
            catch { }

            // ข้อยกเว้นสำหรับ Jinnawit.Ananpatiwet@gpv-group.com ให้แสดงปุ่มได้เสมอ
            if (!string.IsNullOrEmpty(userEmail) && userEmail.IndexOf("Jinnawit.Ananpatiwet", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isIndustrial = true;
            }

            ViewData["EMPLOYEE_DEPARTMENT"] = dept;
            ViewData["IsIndustrial"] = isIndustrial;

            return View();
        }
        public ActionResult ManageMasterMAList(string id)
        {
            try
            {
                ViewData["ID"] = id;
            }
            catch
            {
                ViewData["ID"] = "";
            }
            return View();
        }
        public ActionResult PinList()
        {
            return View();
        }
        public ActionResult ManagePinList(string id)
        {
            try
            {
                ViewData["ID"] = id;
            }
            catch
            {
                ViewData["ID"] = "";
            }
            return View();
        }
        public ActionResult MachineList()
        {
            return View();
        }
        public ActionResult ManageMachineList(string id)
        {
            try
            {
                ViewData["ID"] = id;
            }
            catch
            {
                ViewData["ID"] = "";
            }
            return View();
        }
        public ActionResult ManageCustomer(string id)
        {
            try
            {
                ViewData["ID"] = id;
                ViewData["EMPLOYEE_ID"] = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
            }
            catch
            {
                ViewData["ID"] = "";
                ViewData["EMPLOYEE_ID"] = "";
            }
            return View();
        }
        public ActionResult Email()
        {
            return View();
        }
        public ActionResult ManageEmailList(string id)
        {
            try
            {
                ViewData["ID"] = id;
            }
            catch
            {
                ViewData["ID"] = "";
            }
            return View();
        }
        public ActionResult Instruction()
        {
            return View();
        }
        public ActionResult Report_Analysis()
        {
            return View();
        }
        public ActionResult Report_Overdue()
        {
            return View();
        }
        public ActionResult Report_Nearly()
        {
            return View();
        }
        public ActionResult THT_Location()
        {
            return View();
        }
        public ActionResult Summary_THT_Pallet()
        {
            return View();
        }
        public class CustomerOnly
        {
            public string Customer { get; set; }
        }
        public ActionResult Load_All_Lists()
        {
            try
            {
                using (MAFixtureEntities EntityMA = SettingAccountMAFixture())
                using (WipEntities EntityWIP = SettingAccountWIPstatus())
                {

                    EntityMA.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    EntityWIP.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    var customer = EntityWIP.Database.SqlQuery<string>(@"
                                    SELECT Customer COLLATE Latin1_General_CS_AS AS Customer
                                    FROM WIP_status
                                    GROUP BY Customer COLLATE Latin1_General_CS_AS
                                    ORDER BY Customer;")
                                    .ToList();
                    List<GroupEmail> department = EntityMA.GroupEmails.AsNoTracking().ToList();
                    List<Tbl_Pindetail> pinnumber = EntityMA.Tbl_Pindetail.AsNoTracking().ToList();
                    //List<Tbl_MasterMATester> MasterMA = EntityMA.Tbl_MasterMATester.Where(x => x.Status == "1").ToList();
                    List<Machine> Machine = EntityMA.Machines.AsNoTracking().ToList();
                    DateTime twoYearsAgo = DateTime.Now.AddYears(-2);
                    
                    var rawDrawings = EntityWIP.WIP_status.AsNoTracking()
                        .Where(w => w.StartDate > twoYearsAgo && !string.IsNullOrEmpty(w.Drawing))
                        .Select(w => w.Drawing)
                        .Distinct()
                        .ToList();

                    var Drawing = rawDrawings
                        .Select(d => new
                        {
                            Drawing = d.Contains(" ") ? d.Substring(0, d.IndexOf(" ")) : d
                        })
                        .Distinct()
                        .OrderBy(d => d.Drawing)
                        .ToList();

                    // 1. ดึงข้อมูล 2 คอลัมน์ที่ต้องการจาก Database แค่รอบเดียว (ลด Database Roundtrip)
                    var maplanData = EntityMA.MAPlans.AsNoTracking()
                        .Select(m => new { m.Serial_No, m.Location })
                        .ToList();

                    // 2. นำข้อมูลใน Memory (RAM) มากรองและจัดเรียง Serial 
                    var Serial = maplanData
                        .Where(m => !string.IsNullOrEmpty(m.Serial_No))
                        .Select(m => m.Serial_No)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    // 3. นำข้อมูลใน Memory (RAM) มากรองและจัดเรียง Location
                    var Location = maplanData
                        .Where(x => !string.IsNullOrEmpty(x.Location))
                        .Select(x => x.Location)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var summarizeTypes = EntityMA.Tbl_Summarize_Tools.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Type))
                        .Select(x => x.Type);

                    var masterTypes = EntityMA.Tbl_MasterTools.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Type))
                        .Select(x => x.Type);

                    var maplanTypes = EntityMA.MAPlans.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Type_Tools))
                        .Select(x => x.Type_Tools);

                    var toolTypes = summarizeTypes.Union(masterTypes).Union(maplanTypes)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var summarizeProductNames = EntityMA.Tbl_Summarize_Tools.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Product_name))
                        .Select(x => x.Product_name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var lists = new
                    {
                        Customer = customer,
                        Department = department,
                        Pinnumber = pinnumber,
                        //MasterMA = MasterMA,
                        Drawing = Drawing,
                        Serial = Serial,
                        Machine = Machine,
                        Location = Location,
                        ToolTypes = toolTypes,
                        SummarizeProductNames = summarizeProductNames
                    };

                    return Json(new { success = true, data = lists }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_Dateinfo()
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    var Year = Entity.Tbl_KPIoverdue.AsNoTracking()
                                .Where(x => x.Year != null)
                                .Select(x => x.Year)
                                .Distinct()
                                .OrderByDescending(y => y)
                                .ToList();
                    if (Year.Count == 0)
                    {
                        return Json(new { success = false, word = "Not Found Data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, Year = Year}, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Load_Drawing_By_Customer(string Customer)
        {
            try
            {
                using (var EntityWIP = SettingAccountWIPstatus())
                {
                    EntityWIP.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    var Drawing = EntityWIP.WIP_status.AsNoTracking()
                        .Where(w => w.Customer == Customer)  // Filter by Customer
                        .Select(w => w.Drawing)  // Select the Drawing column
                        .ToList()  // Retrieve data from the database
                        .Where(d => !string.IsNullOrEmpty(d) && d.Contains(' ')) // Ensure there is a space
                        .Select(d => d.Substring(0, d.IndexOf(' ')))  // Extract the part before the first space
                        .Distinct()  // Ensure distinct values
                        .OrderBy(d => d) 
                        .ToList(); // Final list of distinct drawing values

                    return Json(new { success = true, data = Drawing }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message });
            }
        }

        public ActionResult Load_MasterMA(string DetailEng,string DetailThai,string Category,string Section,string Machinename,string Type_MA)
        {
            try
            {
                using (var Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    var query = Entity.Tbl_MasterMATester.AsNoTracking().AsQueryable();
                    if (!string.IsNullOrEmpty(DetailEng))
                    {
                        DetailEng = DetailEng.Trim();
                        query = query.Where(x => x.DetailEng.Contains(DetailEng));
                    }
                    if (!string.IsNullOrEmpty(DetailThai))
                    {
                        DetailThai = DetailThai.Trim();
                        query = query.Where(x => x.DetailThai.Contains(DetailThai));
                    }
                    if (!string.IsNullOrEmpty(Category))
                    {
                        Category = Category.Trim();
                        query = query.Where(x => x.Category.Contains(Category));
                    }
                    if (!string.IsNullOrEmpty(Section))
                    {
                        Section = Section.Trim();
                        query = query.Where(x => x.Section.Contains(Section));
                    }
                    if (!string.IsNullOrEmpty(Machinename))
                    {
                        Machinename = Machinename.Trim();
                        query = query.Where(x => x.Machinename.Contains(Machinename));
                    }
                    if (!string.IsNullOrEmpty(Type_MA))
                    {
                        Type_MA = Type_MA.Trim();
                        query = query.Where(x => x.Type_MA.Contains(Type_MA));
                    }
                    var data = query.Where(w => w.Status == "1").ToList(); // เทียบเท่ากับ TOP 1


                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message });
            }
        }
        public ActionResult Load_Customer_By_Drawing(string Drawing)
        {
            try
            { 
                using (var EntityWIP = SettingAccountWIPstatus())
                {
                    EntityWIP.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    var Customer = EntityWIP.WIP_status.AsNoTracking()
                         .Where(w => w.Drawing.StartsWith(Drawing)) // ใช้ StartsWith() แทน LIKE 'xxx%'
                         .OrderByDescending(w => w.StartDate) // เรียงลำดับตาม Customer
                         .Select(w => w.Customer) // เลือกเฉพาะ Customer
                         .FirstOrDefault(); // เทียบเท่ากับ TOP 1


                    return Json(new { success = true, data = Customer }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message });
            }
        }
        public ActionResult Load_Machine_By_Section(string section)
        {
            try
            {
                using (var Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;

                    // แยกคำค้นหาด้วยเครื่องหมาย , หรือ / เพื่อรองรับทั้งข้อมูลใหม่และข้อมูลเดิม
                    var keywords = (section ?? "").Split(new[] { ',', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    int sectionCount = keywords.Length; // Store the length in a variable
                    List<string> data;
                    data = Entity.MAPlans.AsNoTracking()
                        .Where(m => keywords.Contains(m.Section) && m.StatusF == 1)
                        .Select(m => m.Equipment_Name)
                        .Distinct()
                        .ToList();
                       

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = "Error: " + ex.Message });
            }
        }

        public ActionResult Load_Report_Analysis(string Year)
        {
            try
            {
                using (var Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;

                    var Sumequipment = Entity.Tbl_KPIoverdue.AsNoTracking()
                        .Where(x => x.Year == Year)
                        .GroupBy(x => new { x.WorkWeek, x.Department })
                        .Select(g => new
                        {
                            WorkWeek = g.Key.WorkWeek,
                            Department = g.Key.Department,
                            SumEquipment = g.Sum(x => x.QtyEquipment),
                        })
                        .Where(result => result.SumEquipment > 0 && !result.Department.Contains("Test_Assembly"))
                        .ToList();

                    var Sumtools = Entity.Tbl_KPIoverdue.AsNoTracking()
                        .Where(x => x.Year == Year)
                        .GroupBy(x => new { x.WorkWeek, x.Department })
                        .Select(g => new
                        {
                            WorkWeek = g.Key.WorkWeek,
                            Department = g.Key.Department,
                            SumTools = g.Sum(x => x.QtyTools),
                        })
                        .Where(result => result.SumTools > 0)
                        .ToList();

                    var Sumdepartment = Entity.Tbl_KPIoverdue.AsNoTracking()
                        .Where(x => x.Year == Year)
                        .GroupBy(x => new { x.WorkWeek, x.Department })
                        .Select(g => new
                        {
                            WorkWeek = g.Key.WorkWeek,
                            Department = g.Key.Department,
                            SumEquipment = g.Sum(x => x.QtyEquipment),
                            SumTools = g.Sum(x => x.QtyTools),
                        })
                        .Where(result =>!result.Department.Contains("Test_"))
                        .ToList();

                    return Json(new { success = true, Sumequipment = Sumequipment, Sumtools = Sumtools, Sumdepartment = Sumdepartment }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = "Error: " + ex.Message });
            }
        }
        public ActionResult Load_Report_Overdue(string Serial_No, string From, string To, string Customer, string Product_name, string Section, string Type_MC, string Type_MA, string Location, string Property)
        {
            try
            {
                using (var Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;

                    DateTime today = DateTime.Now;

                    // เริ่มต้น Query
                    var query = Entity.MAPlans.AsNoTracking().AsQueryable();

                    if (!String.IsNullOrEmpty(Serial_No))
                    {
                        query = query.Where(x => x.Serial_No.Contains(Serial_No));
                    }
                    if (!string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1);
                        query = query.Where(x => x.LastMA_date >= dateFrom && x.LastMA_date < dateTo);
                    }
                    else if (!string.IsNullOrEmpty(From))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        query = query.Where(x => x.LastMA_date >= dateFrom);
                    }
                    else if (!string.IsNullOrEmpty(To))
                    {
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1);
                        query = query.Where(x => x.LastMA_date < dateTo);
                    }
                    if (!String.IsNullOrEmpty(Customer))
                    {
                        query = query.Where(x => x.Customer_Name.Contains(Customer));
                    }
                    if (!String.IsNullOrEmpty(Product_name))
                    {
                        query = query.Where(x => x.Product_name.Contains(Product_name));
                    }
                    if (!String.IsNullOrEmpty(Section))
                    {
                        query = query.Where(x => x.Section.Equals(Section));
                    }
                    if (!String.IsNullOrEmpty(Type_MC))
                    {
                        if (Type_MC == "0")
                        {
                            query = query.Where(x => x.Type_MC == "Maintenance");
                        }
                        else if (Type_MC == "1")
                        {
                            query = query.Where(x => x.Type_MC == "Calibration");
                        }
                    }
                    if (!String.IsNullOrEmpty(Type_MA))
                    {
                        if (Type_MA == "0")
                        {
                            query = query.Where(x => x.Type_MA == "Equipment/Machine");
                        }
                        else if (Type_MA == "1")
                        {
                            query = query.Where(x => x.Type_MA == "Sparepart");
                        }
                        else if (Type_MA == "2")
                        {
                            query = query.Where(x => x.Type_MA == "Tools");
                        }
                    }
                    if (!String.IsNullOrEmpty(Location))
                    {
                        query = query.Where(x => x.Location.Contains(Location));
                    }
                    if (!String.IsNullOrEmpty(Property))
                    {
                        query = query.Where(x => x.Property.Contains(Property));
                    }

                    query = query.Where(x =>
                        (x.Plan_Weekly <= today ||
                         x.Plan_Monthly <= today ||
                         x.Plan_Quarterly <= today ||
                         x.Plan_Semi_annually <= today ||
                         x.Plan_Yearly <= today)
                        && x.StatusF == 1);

                    var data = query.ToList();

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = "Error: " + ex.Message });
            }
        }
        public ActionResult Load_Report_Nearly(string Serial_No, string From, string To, string Customer, string Product_name, string Section, string Type_MC, string Type_MA, string Location, string Property)
        {
            try
            {
                using (var Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;

                    DateTime today = DateTime.Now;
                    DateTime weeklyLimit = today.AddDays(3);
                    DateTime monthlyLimit = today.AddDays(7);
                    DateTime quarterlyLimit = today.AddDays(14);
                    DateTime semiAnnuallyLimit = today.AddDays(14);
                    DateTime yearlyLimit = today.AddDays(14);
                    // เริ่มต้น Query
                    var query = Entity.MAPlans.AsNoTracking().AsQueryable();

                    if (!String.IsNullOrEmpty(Serial_No))
                    {
                        query = query.Where(x => x.Serial_No.Contains(Serial_No));
                    }
                    if (!string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1);
                        query = query.Where(x => x.LastMA_date >= dateFrom && x.LastMA_date < dateTo);
                    }
                    else if (!string.IsNullOrEmpty(From))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        query = query.Where(x => x.LastMA_date >= dateFrom);
                    }
                    else if (!string.IsNullOrEmpty(To))
                    {
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1);
                        query = query.Where(x => x.LastMA_date < dateTo);
                    }
                    if (!String.IsNullOrEmpty(Customer))
                    {
                        query = query.Where(x => x.Customer_Name.Contains(Customer));
                    }
                    if (!String.IsNullOrEmpty(Product_name))
                    {
                        query = query.Where(x => x.Product_name.Contains(Product_name));
                    }
                    if (!String.IsNullOrEmpty(Section))
                    {
                        query = query.Where(x => x.Section.Equals(Section));
                    }
                    if (!String.IsNullOrEmpty(Type_MC))
                    {
                        if (Type_MC == "0")
                        {
                            query = query.Where(x => x.Type_MC == "Maintenance");
                        }
                        else if (Type_MC == "1")
                        {
                            query = query.Where(x => x.Type_MC == "Calibration");
                        }
                    }
                    if (!String.IsNullOrEmpty(Type_MA))
                    {
                        if (Type_MA == "0")
                        {
                            query = query.Where(x => x.Type_MA == "Equipment/Machine");
                        }
                        else if (Type_MA == "1")
                        {
                            query = query.Where(x => x.Type_MA == "Sparepart");
                        }
                        else if (Type_MA == "2")
                        {
                            query = query.Where(x => x.Type_MA == "Tools");
                        }
                    }
                    if (!String.IsNullOrEmpty(Location))
                    {
                        query = query.Where(x => x.Location.Contains(Location));
                    }
                    if (!String.IsNullOrEmpty(Property))
                    {
                        query = query.Where(x => x.Property.Contains(Property));
                    }

                    query = query.Where(x =>
                        (
                            (x.Plan_Weekly >= today && x.Plan_Weekly <= weeklyLimit) ||
                            (x.Plan_Monthly >= today && x.Plan_Monthly <= monthlyLimit) ||
                            (x.Plan_Quarterly >= today && x.Plan_Quarterly <= quarterlyLimit) ||
                            (x.Plan_Semi_annually >= today && x.Plan_Semi_annually <= semiAnnuallyLimit) ||
                            (x.Plan_Yearly >= today && x.Plan_Yearly <= yearlyLimit)
                        )
                        && x.StatusF == 1
                    );

                    var data = query.ToList();

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = "Error: " + ex.Message });
            }
        }


        //public ActionResult Load_Customer_By_ID(string ID)
        //{
        //    try
        //    {
        //        // Convert the string ID to long
        //        long customerId;
        //        if (!long.TryParse(ID, out customerId))
        //        {
        //            return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
        //        }

        //        using (MAFixtureEntities Entity = SettingAccountMAFixture())
        //        {
        //            Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
        //            Customer customer = Entity.Customers.FirstOrDefault(x => x.ID == customerId);

        //            if (customer == null)
        //            {
        //                return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
        //            }

        //            return Json(new { success = true, data = customer }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        return Json(new { success = false, word = "Error, " + E.Message });
        //    }
        //}
        public ActionResult GetInformation(string ID)
        {
            try
            {
                // Convert the string ID to long
                long pinnumberId;
                if (!long.TryParse(ID, out pinnumberId))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Tbl_Pindetail pinnumber = Entity.Tbl_Pindetail.FirstOrDefault(x => x.ID == pinnumberId);

                    if (pinnumber == null)
                    {
                        return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = pinnumber }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        //public ActionResult GetInformation(string pinnumber)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(pinnumber))
        //        {
        //            return Json(new { success = false, word = "Invalid ID" }, JsonRequestBehavior.AllowGet);
        //        }

        //        using (MAFixtureEntities Entity = SettingAccountMAFixture())
        //        {
        //            Entity.Configuration.ProxyCreationEnabled = false;

        //            // Fetch data based on the ID
        //            Tbl_Pindetail data = Entity.Tbl_Pindetail.FirstOrDefault(x => x.Lifetime.ToString() == pinnumber);

        //            if (data == null)
        //            {
        //                return Json(new { success = false, word = "No data found for the selected ID" }, JsonRequestBehavior.AllowGet);
        //            }

        //            return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, word = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult Load_EmailList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long id;
                if (!long.TryParse(ID, out id))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    GroupEmail emailList = Entity.GroupEmails.FirstOrDefault(x => x.ID == id);

                    if (emailList == null)
                    {
                        return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = emailList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_MasterMAList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long id;
                if (!long.TryParse(ID, out id))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Tbl_MasterMATester MasterMAList = Entity.Tbl_MasterMATester.FirstOrDefault(x => x.ID == id);

                    if (MasterMAList == null)
                    {
                        return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = MasterMAList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_PinList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long id;
                if (!long.TryParse(ID, out id))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Tbl_Pindetail PinList = Entity.Tbl_Pindetail.FirstOrDefault(x => x.ID == id);

                    if (PinList == null)
                    {
                        return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = PinList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_MachineList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long id;
                if (!long.TryParse(ID, out id))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Machine MasterMAList = Entity.Machines.FirstOrDefault(x => x.ID == id);

                    if (MasterMAList == null)
                    {
                        return Json(new { success = false, word = "Couldn't load data" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = MasterMAList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_MAPlan(string Serial_No, string From, string To, string Customer, string Product_name, string Section, string Type_MC, string Type_MA, string Location, string Property, string Type_Tools = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    // เริ่มต้น Query
                    var query = Entity.MAPlans.AsNoTracking().AsQueryable();

                    // เพิ่มเงื่อนไขตาม type
                    if (!String.IsNullOrEmpty(Serial_No))
                    {
                        query = query.Where(x => x.Serial_No.Contains(Serial_No));
                    }
                    if (!string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1); // add 1 day

                        query = query.Where(x => x.LastMA_date >= dateFrom && x.LastMA_date < dateTo);
                    }
                    else if (!string.IsNullOrEmpty(From))
                    {
                        DateTime dateFrom = Convert.ToDateTime(From).Date;
                        query = query.Where(x => x.LastMA_date >= dateFrom);
                    }
                    else if (!string.IsNullOrEmpty(To))
                    {
                        DateTime dateTo = Convert.ToDateTime(To).Date.AddDays(1);
                        query = query.Where(x => x.LastMA_date < dateTo);
                    }
                    if (!String.IsNullOrEmpty(Customer))
                    {
                        query = query.Where(x => x.Customer_Name.Contains(Customer));
                    }
                    if (!String.IsNullOrEmpty(Product_name))
                    {
                        query = query.Where(x => x.Product_name.Contains(Product_name));
                    }
                    if (!String.IsNullOrEmpty(Section))
                    {
                        query = query.Where(x => x.Section.Equals(Section));
                    }
                    if (!String.IsNullOrEmpty(Type_MC))
                    {
                        if (Type_MC == "0")
                        {
                            query = query.Where(x => x.Type_MC == "Maintenance");
                        }
                        else if (Type_MC == "1")
                        {
                            query = query.Where(x => x.Type_MC == "Calibration");
                        }
                    }
                    if (!String.IsNullOrEmpty(Type_MA))
                    {
                        if (Type_MA == "0")
                        {
                            query = query.Where(x => x.Type_MA == "Equipment/Machine");
                        }
                        else if (Type_MA == "1")
                        {
                            query = query.Where(x => x.Type_MA == "Sparepart");
                        }
                        else if (Type_MA == "2")
                        {
                            query = query.Where(x => x.Type_MA == "Tools");
                        }
                    }
                    if (!String.IsNullOrEmpty(Type_Tools))
                    {
                        query = query.Where(x => x.Type_Tools == Type_Tools);
                    }
                    if (!String.IsNullOrEmpty(Location))
                    {
                        query = query.Where(x => x.Location.Contains(Location));
                    }
                    if (!String.IsNullOrEmpty(Property))
                    {
                        query = query.Where(x => x.Property.Contains(Property));
                    }

                    // Retrieve the maplan data based on Serial_No
                    var data = query.Where(x => x.StatusF == 1).ToList();

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }


                    // แปลง JSON ให้ไวขึ้นด้วย Newtonsoft.Json
                    string jsonStr = JsonConvert.SerializeObject(new { success = true, data = data });
                    return Content(jsonStr, "application/json");
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_THTLocation(string Product_name, string GPVProcess, string ProcessSide, string Location, string Serial_No, string WithTopCover, string WithControlTooling, string Grade_Status, string Section)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    // เริ่มต้น Query
                    var query = Entity.MAPlans.AsNoTracking().AsQueryable();

                    // เพิ่มเงื่อนไขตาม type
                    if (!String.IsNullOrEmpty(Product_name))
                    {
                        query = query.Where(x => x.Product_name.Contains(Product_name.Trim()));
                    }

                    if (!String.IsNullOrEmpty(GPVProcess))
                    {
                        query = query.Where(x => x.GPVProcess.Contains(GPVProcess.Trim()));
                    }
                    if (!String.IsNullOrEmpty(ProcessSide))
                    {
                        query = query.Where(x => x.ProcessSide.Contains(ProcessSide.Trim()));
                    }
                    if (!String.IsNullOrEmpty(Location))
                    {
                        query = query.Where(x => x.Location.Contains(Location.Trim()));
                    }
                    if (!String.IsNullOrEmpty(Serial_No))
                    {
                        query = query.Where(x => x.Serial_No.Contains(Serial_No.Trim()));
                    }
                    if (!String.IsNullOrEmpty(WithTopCover))
                    {
                        query = query.Where(x => x.WithTopCover.Contains(WithTopCover.Trim()));
                    }
                    if (!String.IsNullOrEmpty(WithControlTooling))
                    {
                        query = query.Where(x => x.WithControlTooling.Contains(WithControlTooling.Trim()));
                    }
                    if (!String.IsNullOrEmpty(Grade_Status))
                    {
                        query = query.Where(x => x.Grade_Status.Contains(Grade_Status.Trim()));
                    }
                    if (!String.IsNullOrEmpty(Section))
                    {
                        query = query.Where(x => x.Section.Contains(Section.Trim()));
                    }

                    // Retrieve the maplan data based on Serial_No
                    var data = query.Where(x => x.StatusF == 1).ToList();

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }


                    var jsonResult = Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_Obsolete()
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    // Retrieve the maplan data based on Serial_No
                    var data = Entity.MAPlans.AsNoTracking().Where(x => x.StatusF == 2).ToList();

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }


                    var jsonResult = Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_MAFixture_By_ID(string MA_ID)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    long maIdAsDouble;
                    if (!long.TryParse(MA_ID, out maIdAsDouble))
                    {
                        return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                    }

                    var data = Entity.MAPlans.AsNoTracking().FirstOrDefault(x => x.MA_ID == maIdAsDouble && x.StatusF != 0);

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    // Create a new maplan object to populate the data
                    MAPlan mappedData = new MAPlan
                    {
                        MA_ID = data.MA_ID,
                        Equipment_No = data.Equipment_No,
                        Equipment_Name = data.Equipment_Name,
                        Product_name = data.Product_name,
                        Type_MC = data.Type_MC,
                        Model = data.Model,
                        Serial_No = data.Serial_No,
                        Property = data.Property,
                        Referrence_No = data.Referrence_No,
                        Responsible_Name = data.Responsible_Name,
                        Manufacture = data.Manufacture,
                        Description = data.Description,
                        LastMA_date = data.LastMA_date,
                        GE_ID = data.GE_ID,
                        Section = data.Section,
                        Type_MA = data.Type_MA,
                        Startdate = data.Startdate,
                        CreateBy = data.CreateBy,
                        UpdateBy = data.UpdateBy,
                        Customer = data.Customer,
                        Customer_Name = data.Customer_Name,
                        Plan_Daily = data.Plan_Daily,
                        Plan_Weekly = data.Plan_Weekly,
                        Plan_Monthly = data.Plan_Monthly,
                        Plan_Semi_annually = data.Plan_Semi_annually,
                        Plan_Quarterly = data.Plan_Quarterly,
                        Plan_Yearly = data.Plan_Yearly,
                        Plan_Manually = data.Plan_Manually,
                        P_ID = data.P_ID,
                        Pinnumber = data.Pinnumber,

                        Lifetime = data.Lifetime,
                        Location = data.Location,
                        StatusF = data.StatusF,

                        GPVProcess = data.GPVProcess,
                        ProcessSide = data.ProcessSide,
                        WithTopCover = data.WithTopCover,
                        WithControlTooling = data.WithControlTooling,
                        Grade_Status = data.Grade_Status,
                        Dimension = data.Dimension,
                        Equipment_Picture = data.Equipment_Picture,
                        Type_Tools = data.Type_Tools,
                    };

                    mappedData.FilesMAs = new List<FilesMA>();

                    //var filesMA = Entity.FilesMAs.Where(x => x.MA_ID == data.MA_ID).ToList();
                    var filesMA = Entity.FilesMAs.AsNoTracking()
                        .Where(x => x.MA_ID == data.MA_ID)
                        .Select(x => new
                        {
                            x.ID,
                            x.MA_ID,
                            x.Createby,
                            x.Createdate,
                            x.FilenameAfter,
                            x.FilenameBefore,
                            x.Format,
                            //x.MAChecklistdetails,
                            x.MAPlan,
                            x.PlanName,
                            x.Name,
                            x.MA_Status,
                            x.Comment,
                            //x.ImageBefores,
                            //x.ImageAfters,
                        })
                        .ToList();
                    if (filesMA.Any())
                    {
                        foreach (var item in filesMA)
                        {
                            var MAChecklistdetails = new List<MAChecklistdetail>();
                            var ImageBefores = new List<ImageBefore>();
                            var ImageAfters = new List<ImageAfter>();
                            var maChecklistdetails = Entity.MAChecklistdetails.AsNoTracking().Where(x => x.FID == item.ID).ToList();

                            if (maChecklistdetails.Any())
                            {
                                var mappedChecklistDetails = new List<MAChecklistdetail>();

                                foreach (var checklistItem in maChecklistdetails)
                                {
                                    var mappedChecklistItem = new MAChecklistdetail
                                    {
                                        ID = checklistItem.ID,
                                        FID = checklistItem.FID,
                                        Checklistdetail = checklistItem.Checklistdetail,
                                        Status = checklistItem.Status,
                                        Problem = checklistItem.Problem,
                                        Remark = checklistItem.Remark
                                    };

                                    mappedChecklistDetails.Add(mappedChecklistItem);
                                }

                                MAChecklistdetails = mappedChecklistDetails;
                            }

                            var imageBefore = Entity.ImageBefores.AsNoTracking().Where(b => b.FilesMA_ID == item.ID).ToList();

                            if (imageBefore.Any())
                            {
                                var imageBeforeDetails = new List<ImageBefore>();

                                foreach (var imageBeforeItem in imageBefore)
                                {
                                    var mappedimageBeforeItem = new ImageBefore
                                    {
                                        ID = imageBeforeItem.ID,
                                        FilesMA_ID = imageBeforeItem.FilesMA_ID,
                                        Image_Name = imageBeforeItem.Image_Name,
                                        Date = imageBeforeItem.Date
                                    };

                                    imageBeforeDetails.Add(mappedimageBeforeItem);
                                }

                                ImageBefores = imageBeforeDetails;
                            }

                            var imageAfter = Entity.ImageAfters.AsNoTracking().Where(a => a.FilesMA_ID == item.ID).ToList();

                            if (imageAfter.Any())
                            {
                                var imageAfterDetails = new List<ImageAfter>();

                                foreach (var imageAfterItem in imageAfter)
                                {
                                    var mappedImageAfterItem = new ImageAfter
                                    {
                                        ID = imageAfterItem.ID,
                                        FilesMA_ID = imageAfterItem.FilesMA_ID,
                                        Image_Name = imageAfterItem.Image_Name,
                                        Date = imageAfterItem.Date
                                    };

                                    imageAfterDetails.Add(mappedImageAfterItem);
                                }

                                ImageAfters = imageAfterDetails;
                            }

                            var mappedFile = new FilesMA
                            {
                                ID = item.ID,
                                MA_ID = item.MA_ID,
                                Name = item.Name,
                                //File = item.File != null ? item.File.Take(100).ToArray() : null, 
                                Createdate = item.Createdate,
                                PlanName = item.PlanName,
                                Format = item.Format,
                                FilenameBefore = item.FilenameBefore,
                                FilenameAfter = item.FilenameAfter,
                                MAChecklistdetails = MAChecklistdetails,
                                ImageBefores = ImageBefores,
                                ImageAfters = ImageAfters,
                                Createby = item.Createby,
                                MA_Status = item.MA_Status,
                                Comment = item.Comment,
                            };

                            mappedData.FilesMAs.Add(mappedFile);
                        }
                    }

                    var jsonResult = Json(new { success = true, data = mappedData }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_MAFixture_By_Serial(string Serial_No)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    // Retrieve the maplan data based on Serial_No using Where (this returns a collection)
                    var data = Entity.MAPlans.AsNoTracking().Where(x => x.Serial_No.ToLower().Trim() == Serial_No.ToLower().Trim() && x.StatusF != 0).ToList();

                    if (!data.Any())
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    List<MAPlan> mappedDataList = new List<MAPlan>();

                    if (data.Count > 1)
                    {
                        foreach (var dataItem in data)
                        {
                            MAPlan mappedData = new MAPlan
                            {
                                MA_ID = dataItem.MA_ID,
                                Equipment_No = dataItem.Equipment_No,
                                Equipment_Name = dataItem.Equipment_Name,
                                Product_name = dataItem.Product_name,
                                Type_MC = dataItem.Type_MC,
                                Model = dataItem.Model,
                                Serial_No = dataItem.Serial_No,
                                Property = dataItem.Property,
                                Referrence_No = dataItem.Referrence_No,
                                Responsible_Name = dataItem.Responsible_Name,
                                Manufacture = dataItem.Manufacture,
                                Description = dataItem.Description,
                                LastMA_date = dataItem.LastMA_date,

                                GE_ID = dataItem.GE_ID,
                                Section = dataItem.Section,
                                Type_MA = dataItem.Type_MA,
                                Startdate = dataItem.Startdate,
                                CreateBy = dataItem.CreateBy,
                                UpdateBy = dataItem.UpdateBy,
                                //Customer = dataItem.Customer,
                                Customer_Name = dataItem.Customer_Name,
                                Plan_Daily = dataItem.Plan_Daily,
                                Plan_Weekly = dataItem.Plan_Weekly,
                                Plan_Monthly = dataItem.Plan_Monthly,
                                Plan_Semi_annually = dataItem.Plan_Semi_annually,
                                Plan_Quarterly = dataItem.Plan_Quarterly,
                                Plan_Yearly = dataItem.Plan_Yearly,
                                Plan_Manually = dataItem.Plan_Manually,
                                P_ID = dataItem.P_ID,
                                Pinnumber = dataItem.Pinnumber,

                                Lifetime = dataItem.Lifetime,
                                Location = dataItem.Location,
                                StatusF = dataItem.StatusF,
                                GPVProcess = dataItem.GPVProcess,
                                ProcessSide = dataItem.ProcessSide,
                                WithTopCover = dataItem.WithTopCover,
                                WithControlTooling = dataItem.WithControlTooling,
                                Grade_Status = dataItem.Grade_Status,
                                Dimension = dataItem.Dimension,
                                Equipment_Picture = dataItem.Equipment_Picture,
                            };
                            mappedDataList.Add(mappedData);
                        }
                    }
                    else
                    {
                        foreach (var dataItem in data)
                        {
                            MAPlan mappedData = new MAPlan
                            {
                                MA_ID = dataItem.MA_ID,
                                Equipment_No = dataItem.Equipment_No,
                                Equipment_Name = dataItem.Equipment_Name,
                                Product_name = dataItem.Product_name,
                                Type_MC = dataItem.Type_MC,
                                Model = dataItem.Model,
                                Serial_No = dataItem.Serial_No,
                                Property = dataItem.Property,
                                Referrence_No = dataItem.Referrence_No,
                                Responsible_Name = dataItem.Responsible_Name,
                                Manufacture = dataItem.Manufacture,
                                Description = dataItem.Description,
                                LastMA_date = dataItem.LastMA_date,

                                GE_ID = dataItem.GE_ID,
                                Section = dataItem.Section,
                                Type_MA = dataItem.Type_MA,
                                Startdate = dataItem.Startdate,
                                CreateBy = dataItem.CreateBy,
                                UpdateBy = dataItem.UpdateBy,
                                //Customer = dataItem.Customer,
                                Customer_Name = dataItem.Customer_Name,
                                Plan_Daily = dataItem.Plan_Daily,
                                Plan_Weekly = dataItem.Plan_Weekly,
                                Plan_Monthly = dataItem.Plan_Monthly,
                                Plan_Semi_annually = dataItem.Plan_Semi_annually,
                                Plan_Quarterly = dataItem.Plan_Quarterly,
                                Plan_Yearly = dataItem.Plan_Yearly,
                                Plan_Manually = dataItem.Plan_Manually,
                                P_ID = dataItem.P_ID,
                                Pinnumber = dataItem.Pinnumber,

                                Lifetime = dataItem.Lifetime,
                                Location = dataItem.Location,
                                StatusF = dataItem.StatusF,
                                GPVProcess = dataItem.GPVProcess,
                                ProcessSide = dataItem.ProcessSide,
                                WithTopCover = dataItem.WithTopCover,
                                WithControlTooling = dataItem.WithControlTooling,
                                Grade_Status = dataItem.Grade_Status,
                                Dimension = dataItem.Dimension,
                                Equipment_Picture = dataItem.Equipment_Picture,
                            };

                            mappedData.FilesMAs = new List<FilesMA>();

                            //var filesMA = Entity.FilesMAs.Where(x => x.MA_ID == dataItem.MA_ID).ToList();
                            var filesMA = Entity.FilesMAs.AsNoTracking()
                                .Where(x => x.MA_ID == dataItem.MA_ID)
                                .Select(x => new
                                {
                                    x.ID,
                                    x.MA_ID,
                                    x.Createby,
                                    x.Createdate,
                                    x.FilenameAfter,
                                    x.FilenameBefore,
                                    x.Format,
                                    //x.MAChecklistdetails,
                                    x.MAPlan,
                                    x.PlanName,
                                    x.Name,
                                    x.MA_Status,
                                    x.Comment,
                                    //x.ImageBefores,
                                    //x.ImageAfters,
                                })
                                .ToList();
                            if (filesMA.Any())
                            {
                                foreach (var item in filesMA)
                                {
                                    var MAChecklistdetails = new List<MAChecklistdetail>();
                                    var ImageBefores = new List<ImageBefore>();
                                    var ImageAfters = new List<ImageAfter>();
                                    var maChecklistdetails = Entity.MAChecklistdetails.AsNoTracking().Where(x => x.FID == item.ID).ToList();

                                    if (maChecklistdetails.Any())
                                    {
                                        var mappedChecklistDetails = new List<MAChecklistdetail>();

                                        foreach (var checklistItem in maChecklistdetails)
                                        {
                                            var mappedChecklistItem = new MAChecklistdetail
                                            {
                                                ID = checklistItem.ID,
                                                FID = checklistItem.FID,
                                                Checklistdetail = checklistItem.Checklistdetail,
                                                Status = checklistItem.Status,
                                                Problem = checklistItem.Problem,
                                                Remark = checklistItem.Remark
                                            };

                                            mappedChecklistDetails.Add(mappedChecklistItem);
                                        }

                                        MAChecklistdetails = mappedChecklistDetails;
                                    }

                                    var imageBefore = Entity.ImageBefores.Where(b => b.FilesMA_ID == item.ID).ToList();

                                    if (imageBefore.Any())
                                    {
                                        var imageBeforeDetails = new List<ImageBefore>();

                                        foreach (var imageBeforeItem in imageBefore)
                                        {
                                            var mappedimageBeforeItem = new ImageBefore
                                            {
                                                ID = imageBeforeItem.ID,
                                                FilesMA_ID = imageBeforeItem.FilesMA_ID,
                                                Image_Name = imageBeforeItem.Image_Name,
                                                Date = imageBeforeItem.Date
                                            };

                                            imageBeforeDetails.Add(mappedimageBeforeItem);
                                        }

                                        ImageBefores = imageBeforeDetails;
                                    }

                                    var imageAfter = Entity.ImageAfters.Where(a => a.FilesMA_ID == item.ID).ToList();

                                    if (imageAfter.Any())
                                    {
                                        var imageAfterDetails = new List<ImageAfter>();

                                        foreach (var imageAfterItem in imageAfter)
                                        {
                                            var mappedImageAfterItem = new ImageAfter
                                            {
                                                ID = imageAfterItem.ID,
                                                FilesMA_ID = imageAfterItem.FilesMA_ID,
                                                Image_Name = imageAfterItem.Image_Name,
                                                Date = imageAfterItem.Date
                                            };

                                            imageAfterDetails.Add(mappedImageAfterItem);
                                        }

                                        ImageAfters = imageAfterDetails;
                                    }

                                    var mappedFile = new FilesMA
                                    {
                                        ID = item.ID,
                                        MA_ID = item.MA_ID,
                                        Name = item.Name,
                                        //File = item.File,
                                        Createdate = item.Createdate,
                                        PlanName = item.PlanName,
                                        Format = item.Format,
                                        FilenameBefore = item.FilenameBefore,
                                        FilenameAfter = item.FilenameAfter,
                                        MAChecklistdetails = MAChecklistdetails,
                                        ImageBefores = ImageBefores,
                                        ImageAfters = ImageAfters,
                                        Createby = item.Createby,
                                        MA_Status = item.MA_Status,
                                        Comment = item.Comment,
                                    };

                                    mappedData.FilesMAs.Add(mappedFile);
                                }
                            }
                            // Add the mapped data to the list
                            mappedDataList.Add(mappedData);
                        }
                    }

                    var jsonResult = Json(new { success = true, data = mappedDataList }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        public ActionResult Load_Master_Checklist(Tbl_MasterMATester MasterMA)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    // นับว่ามี machinename ที่เจอตาม input ไหม
                    var value = Entity.Tbl_MasterMATester
                                      .Count(x => (x.Machinename).Contains(MasterMA.Machinename));

                    IQueryable<Tbl_MasterMATester> query = Entity.Tbl_MasterMATester
                        .Where(x =>
                            x.Category.Contains(MasterMA.Category) &&
                            x.Section.Contains(MasterMA.Section) &&
                            x.Type_MA == MasterMA.Type_MA &&
                            x.Status == "1"
                        );

                    if (value > 0)
                    {
                        query = query.Where(x => (x.Machinename ?? "").Contains(MasterMA.Machinename));
                    }
                    else
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.Machinename));
                    }

                    var data = query.ToList();

                    if (!data.Any())
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = data, value = value }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }


        public ActionResult Load_Sparepart_by_EquipmentName(string Serial_No)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    // Ensure ProxyCreationEnabled is disabled if needed
                    // Entity.Configuration.ProxyCreationEnabled = false;

                    var data = Entity.MAPlans
                        .Where(m => m.Referrence_No == Serial_No)
                        .Select(m => new
                        {
                            m.MA_ID,
                            m.Serial_No
                        }) // ดึงเฉพาะฟิลด์ที่ต้องการ
                        .ToList();
                    if (!data.Any())
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }

        public ActionResult CheckForDuplicateNames(string serialNo, string Equipment_No, string maId)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;
                    long id = 0;
                    if (!string.IsNullOrEmpty(maId))
                    {
                        // Attempt to parse maId to a long
                        if (!long.TryParse(maId, out id))
                        {
                            return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (serialNo != "" && serialNo != null)
                    {
                        bool serialNoDuplicate;
                        if (!string.IsNullOrEmpty(maId))
                        {
                            serialNoDuplicate = Entity.MAPlans
                            .Any(x => x.Serial_No == serialNo && x.StatusF != 0 && x.MA_ID != id);
                        }
                        else
                        {
                            serialNoDuplicate = Entity.MAPlans
                            .Any(x => x.Serial_No == serialNo && x.StatusF != 0);
                        }

                        if (serialNoDuplicate)
                        {
                            return Json(new { success = false, word = "Duplicate Serial No found." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Equipment_No != "" && Equipment_No != null)
                    {
                        bool Equipment_NoDuplicate;
                        if (!string.IsNullOrEmpty(maId))
                        {
                            Equipment_NoDuplicate = Entity.MAPlans
                              .Any(x => x.Equipment_No == Equipment_No && x.StatusF != 0 && x.MA_ID != id);
                        }
                        else
                        {
                            Equipment_NoDuplicate = Entity.MAPlans
                            .Any(x => x.Equipment_No == Equipment_No && x.StatusF != 0);
                        }

                        if (Equipment_NoDuplicate)
                        {
                            return Json(new { success = false, word = "Duplicate Equipment Name found." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, word = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var originalName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);

                originalName = originalName.Replace(" ", "_");

                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                var fileName = originalName + "_" + timeStamp + extension;

                var path = Path.Combine(Server.MapPath("~/Equipment_Picture"), fileName);

                file.SaveAs(path);

                return Json(new { success = true, fileName = fileName });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult Record_MAPlan(MAPlan data, string replace, string source = "")
        {
            try
            {
                MAPlan _MAPlan = null;
                List<Tbl_MasterMATester> Tbl_MasterMATesters = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;
                    if (data.MA_ID > 0)
                    {
                        _MAPlan = Entity.MAPlans.FirstOrDefault(x => x.MA_ID == data.MA_ID);
                    }

                    if (!string.IsNullOrEmpty(replace) && !string.IsNullOrEmpty(data.Equipment_Name))
                    {
                        string sql = @"
                    UPDATE Tbl_MasterMATester
                    SET Machinename = REPLACE(Machinename, @p0, @p1)
                    WHERE Machinename LIKE '%' + @p0 + '%' ";

                        Entity.Database.ExecuteSqlCommand(sql, replace, data.Equipment_Name);
                    }

                    bool Mode_Add = false;

                    if (_MAPlan == null)
                    {
                        long maxIdPlusOne = Entity.MAPlans.Any() ? Entity.MAPlans.Max(x => x.MA_ID) + 1 : 1;

                        _MAPlan = new MAPlan()
                        {
                            MA_ID = maxIdPlusOne,
                            Equipment_No = data.Equipment_No,
                            Equipment_Name = data.Equipment_Name,
                            Product_name = data.Product_name,
                            Type_MC = data.Type_MC,
                            Model = data.Model,
                            Serial_No = data.Serial_No,
                            Property = data.Property,
                            Referrence_No = data.Referrence_No,
                            Responsible_Name = data.Responsible_Name,
                            Manufacture = data.Manufacture,
                            Description = data.Description,
                            LastMA_date = data.Startdate ?? DateTime.Now,

                            GE_ID = data.GE_ID,
                            Section = data.Section,
                            Type_MA = data.Type_MA,
                            Startdate = data.Startdate,
                            CreateBy = data.CreateBy,
                            Modifyby = !string.IsNullOrEmpty(data.Modifyby) ? data.Modifyby : data.CreateBy,
                            Modifydate = DateTime.Now,
                            Customer = data.Customer,
                            Customer_Name = data.Customer_Name,
                            Plan_Daily = data.Plan_Daily,
                            Plan_Weekly = data.Plan_Weekly,
                            Plan_Monthly = data.Plan_Monthly,
                            Plan_Semi_annually = data.Plan_Semi_annually,
                            Plan_Quarterly = data.Plan_Quarterly,
                            Plan_Yearly = data.Plan_Yearly,
                            Plan_Manually = data.Plan_Manually,
                            P_ID = data.P_ID,
                            Pinnumber = data.Pinnumber,

                            Lifetime = data.Lifetime,
                            Location = data.Location,

                            GPVProcess = data.GPVProcess,
                            ProcessSide = data.ProcessSide,
                            WithTopCover = data.WithTopCover,
                            WithControlTooling = data.WithControlTooling,
                            Grade_Status = data.Grade_Status,
                            Dimension = data.Dimension,
                            Equipment_Picture = data.Equipment_Picture,
                            Type_Tools = data.Type_Tools,
                            StatusF = 1
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        if (source == "Maintenance" || (!string.IsNullOrEmpty(data.UpdateBy) && string.IsNullOrEmpty(data.Modifyby)))
                        {
                            // Action มาจาก FixtureMaintenance: บันทึกเฉพาะ LastMA_date และ UpdateBy (รวมถึงแผนงานที่คำนวณใหม่)
                            _MAPlan.LastMA_date = DateTime.Now;
                            _MAPlan.UpdateBy = data.UpdateBy;

                            _MAPlan.Plan_Daily = data.Plan_Daily;
                            _MAPlan.Plan_Weekly = data.Plan_Weekly;
                            _MAPlan.Plan_Monthly = data.Plan_Monthly;
                            _MAPlan.Plan_Semi_annually = data.Plan_Semi_annually;
                            _MAPlan.Plan_Quarterly = data.Plan_Quarterly;
                            _MAPlan.Plan_Yearly = data.Plan_Yearly;
                            _MAPlan.Plan_Manually = data.Plan_Manually;
                            _MAPlan.Grade_Status = data.Grade_Status;
                        }
                        else
                        {
                            // Action มาจาก NewEquipment (Edit): บันทึก Modifyby และ Modifydate (ไม่แตะ LastMA_date และ UpdateBy)
                            _MAPlan.Modifyby = data.Modifyby;
                            _MAPlan.Modifydate = DateTime.Now;

                            _MAPlan.Equipment_No = data.Equipment_No;
                            _MAPlan.Equipment_Name = data.Equipment_Name;
                            _MAPlan.Product_name = data.Product_name;
                            _MAPlan.Type_MC = data.Type_MC;
                            _MAPlan.Model = data.Model;
                            _MAPlan.Serial_No = data.Serial_No;
                            _MAPlan.Property = data.Property;
                            _MAPlan.Referrence_No = data.Referrence_No;
                            _MAPlan.Responsible_Name = data.Responsible_Name;
                            _MAPlan.Manufacture = data.Manufacture;
                            _MAPlan.Description = data.Description;

                            _MAPlan.GE_ID = data.GE_ID;
                            _MAPlan.Section = data.Section;
                            _MAPlan.Type_MA = data.Type_MA;
                            _MAPlan.Startdate = data.Startdate;
                            _MAPlan.Customer = data.Customer;
                            _MAPlan.Customer_Name = data.Customer_Name;
                            _MAPlan.Plan_Daily = data.Plan_Daily;
                            _MAPlan.Plan_Weekly = data.Plan_Weekly;
                            _MAPlan.Plan_Monthly = data.Plan_Monthly;
                            _MAPlan.Plan_Semi_annually = data.Plan_Semi_annually;
                            _MAPlan.Plan_Quarterly = data.Plan_Quarterly;
                            _MAPlan.Plan_Yearly = data.Plan_Yearly;
                            _MAPlan.Plan_Manually = data.Plan_Manually; 
                            _MAPlan.P_ID = data.P_ID;
                            _MAPlan.Pinnumber = data.Pinnumber;

                            _MAPlan.Lifetime = data.Lifetime;
                            _MAPlan.Location = data.Location;
                            _MAPlan.StatusF = data.StatusF;

                            _MAPlan.GPVProcess = data.GPVProcess;
                            _MAPlan.ProcessSide = data.ProcessSide;
                            _MAPlan.WithTopCover = data.WithTopCover;
                            _MAPlan.WithControlTooling = data.WithControlTooling;
                            _MAPlan.Grade_Status = data.Grade_Status;
                            _MAPlan.Dimension = data.Dimension;
                            _MAPlan.Equipment_Picture = data.Equipment_Picture;
                            _MAPlan.Type_Tools = data.Type_Tools;

                            // Edit: ลบรายการเดิมทั้งหมดของ MA_ID นี้ออก
                            var existingProducts = Entity.Tbl_ProductName.Where(x => x.MA_ID == _MAPlan.MA_ID).ToList();
                            if (existingProducts.Any())
                            {
                                Entity.Tbl_ProductName.RemoveRange(existingProducts);
                            }

                            // Edit: เพิ่มรายการใหม่ที่ split ด้วย comma
                            if (!string.IsNullOrEmpty(data.Product_name))
                            {
                                var productNames = data.Product_name
                                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(p => p.Trim())
                                    .Where(p => !string.IsNullOrEmpty(p))
                                    .Distinct();

                                foreach (var pName in productNames)
                                {
                                    Entity.Tbl_ProductName.Add(new Tbl_ProductName
                                    {
                                        MA_ID = _MAPlan.MA_ID,
                                        Product_name = pName
                                    });
                                }
                            }
                        }

                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.MAPlans.Add(_MAPlan);

                        // Add: เพิ่มรายการที่ split ด้วย comma ลงใน Tbl_ProductName
                        if (!string.IsNullOrEmpty(_MAPlan.Product_name))
                        {
                            var productNames = _MAPlan.Product_name
                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .Where(p => !string.IsNullOrEmpty(p))
                                .Distinct();

                            foreach (var pName in productNames)
                            {
                                Entity.Tbl_ProductName.Add(new Tbl_ProductName
                                {
                                    MA_ID = _MAPlan.MA_ID,
                                    Product_name = pName
                                });
                            }
                        }
                    }

                    Entity.SaveChanges();
                }
                return Json(new { 
                    success = true, 
                    word = "Saved successfully!", 
                    data = new { 
                        MA_ID = _MAPlan.MA_ID,
                        Equipment_No = _MAPlan.Equipment_No,
                        Equipment_Name = _MAPlan.Equipment_Name
                    } 
                });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_FilesMA(FilesMA data)
        {
            try
            {
                FilesMA _FilesMA = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    if (data.ID > 0)
                    {
                        _FilesMA = Entity.FilesMAs.FirstOrDefault(x => x.ID == data.ID);
                    }

                    bool Mode_Add = false;

                    if (_FilesMA == null)
                    {
                        long maxIdPlusOne = Entity.FilesMAs.Any() ? Entity.FilesMAs.Max(x => x.ID) + 1 : 1;

                        _FilesMA = new FilesMA()
                        {
                            ID = maxIdPlusOne,
                            MA_ID = data.MA_ID,
                            Name = data.Name,
                            File = data.File,
                            Createdate = data.Createdate,
                            PlanName = data.PlanName,
                            Format = data.Format,
                            FilenameBefore = data.FilenameBefore,
                            FilenameAfter = data.FilenameAfter,
                            Createby = data.Createby,
                            Insertdate = DateTime.Now,
                            Serial_No = data.Serial_No,
                            MA_Status = data.MA_Status,
                            GradeAfter = data.GradeAfter,
                            GradeBefore = data.GradeBefore,
                            Comment = data.Comment,
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        _FilesMA.Name = data.Name;
                        _FilesMA.File = data.File;
                        _FilesMA.Createdate = data.Createdate;
                        _FilesMA.PlanName = data.PlanName;
                        _FilesMA.Format = data.Format;
                        _FilesMA.FilenameBefore = data.FilenameBefore;
                        _FilesMA.FilenameAfter = data.FilenameAfter;
                        _FilesMA.Createby = data.Createby;
                        _FilesMA.Insertdate = DateTime.Now;
                        _FilesMA.Serial_No = data.Serial_No;
                        _FilesMA.Comment = data.Comment;
                        _FilesMA.MA_Status = data.MA_Status;
                        _FilesMA.GradeAfter = data.GradeAfter;
                        _FilesMA.GradeBefore = data.GradeBefore;
                        _FilesMA.Comment = data.Comment;
                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.FilesMAs.Add(_FilesMA);
                    }

                    Entity.SaveChanges();
                }
                return Json(new { success = true, word = "Saved successfully!", data = _FilesMA });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_MAChecklistdetail(List<MAChecklistdetail> data)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    foreach (var item in data)
                    {
                        MAChecklistdetail _MAChecklistdetail = null;

                        if (item.ID > 0)
                        {
                            _MAChecklistdetail = Entity.MAChecklistdetails.FirstOrDefault(x => x.ID == item.ID);
                        }

                        bool Mode_Add = false;

                        if (_MAChecklistdetail == null)
                        {
                            _MAChecklistdetail = new MAChecklistdetail()
                            {
                                FID = item.FID,
                                Checklistdetail = item.Checklistdetail,
                                Status = item.Status,
                                Problem = item.Problem,
                                Remark = item.Remark,
                            };
                            Mode_Add = true;
                        }
                        else
                        {
                            _MAChecklistdetail.FID = item.FID;
                            _MAChecklistdetail.Checklistdetail = item.Checklistdetail;
                            _MAChecklistdetail.Status = item.Status;
                            _MAChecklistdetail.Problem = item.Problem;
                            _MAChecklistdetail.Remark = item.Remark;
                            Mode_Add = false;
                        }

                        if (Mode_Add)
                        {
                            Entity.MAChecklistdetails.Add(_MAChecklistdetail);
                        }
                    }

                    Entity.SaveChanges();
                }

                return Json(new { success = true, word = "Saved successfully!" });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_ImageBefore(List<ImageBefore> data)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    foreach (var item in data)
                    {
                        ImageBefore _ImageBefore = null;

                        if (item.ID > 0)
                        {
                            _ImageBefore = Entity.ImageBefores.FirstOrDefault(x => x.ID == item.ID);
                        }

                        bool Mode_Add = false;

                        if (_ImageBefore == null)
                        {
                            _ImageBefore = new ImageBefore()
                            {
                                FilesMA_ID = item.FilesMA_ID,
                                Image_Name = item.Image_Name,
                                Date = item.Date
                            };
                            Mode_Add = true;
                        }
                        else
                        {

                            _ImageBefore.FilesMA_ID = item.FilesMA_ID;
                            _ImageBefore.Image_Name = item.Image_Name;
                            _ImageBefore.Date = item.Date;
                            Mode_Add = false;
                        }

                        if (Mode_Add)
                        {
                            Entity.ImageBefores.Add(_ImageBefore);
                        }
                    }

                    Entity.SaveChanges();
                }

                return Json(new { success = true, word = "Saved successfully!" });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_ImageAfter(List<ImageAfter> data)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    foreach (var item in data)
                    {
                        ImageAfter _ImageAfter = null;

                        if (item.ID > 0)
                        {
                            _ImageAfter = Entity.ImageAfters.FirstOrDefault(x => x.ID == item.ID);
                        }

                        bool Mode_Add = false;

                        if (_ImageAfter == null)
                        {
                            _ImageAfter = new ImageAfter()
                            {
                                FilesMA_ID = item.FilesMA_ID,
                                Image_Name = item.Image_Name,
                                Date = item.Date
                            };
                            Mode_Add = true;
                        }
                        else
                        {

                            _ImageAfter.FilesMA_ID = item.FilesMA_ID;
                            _ImageAfter.Image_Name = item.Image_Name;
                            _ImageAfter.Date = item.Date;
                            Mode_Add = false;
                        }

                        if (Mode_Add)
                        {
                            Entity.ImageAfters.Add(_ImageAfter);
                        }
                    }

                    Entity.SaveChanges();
                }

                return Json(new { success = true, word = "Saved successfully!" });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        private void CleanMachineNameFromMasterChecklist(string equipmentName, long currentMaId, MAFixtureEntities entity)
        {
            if (string.IsNullOrWhiteSpace(equipmentName))
            {
                return;
            }

            equipmentName = equipmentName.Trim();

            // ตรวจสอบว่ายังมี Fixture ตัวอื่นที่ Active (StatusF == 1) และใช้ Equipment_Name นี้อยู่อีกหรือไม่
            bool hasOtherActive = entity.MAPlans.Any(x => x.MA_ID != currentMaId && x.StatusF == 1 && x.Equipment_Name == equipmentName);
            if (hasOtherActive)
            {
                // หากยังมี Fixture อื่นที่ Active และใช้ชื่อเดียวกัน ห้ามลบชื่อเครื่องออกจาก Master Checklist
                return;
            }

            // ค้นหา Tbl_MasterMATester ทั้งหมดที่มีชื่อเครื่องนี้อยู่ใน Machinename
            var relatedMasters = entity.Tbl_MasterMATester
                                       .Where(x => x.Machinename != null && x.Machinename.Contains(equipmentName))
                                       .ToList();

            if (relatedMasters == null || !relatedMasters.Any())
            {
                return;
            }

            char[] delimiters = new char[] { ',', '/' };
            foreach (var master in relatedMasters)
            {
                if (string.IsNullOrEmpty(master.Machinename))
                {
                    continue;
                }

                // แยกรายการชื่อเครื่องด้วย comma หรือ slash
                var machineList = master.Machinename
                    .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim())
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                // กรองเอาชื่อเครื่องที่ตรงกันออก (Exact match, Case-insensitive)
                var updatedList = machineList
                    .Where(m => !m.Equals(equipmentName, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();

                // ปรับปรุงเฉพาะเมื่อมีการเปลี่ยนแปลงจริง
                if (updatedList.Count != machineList.Count)
                {
                    string newMachineName = string.Join(",", updatedList);
                    master.Machinename = string.IsNullOrEmpty(newMachineName) ? null : newMachineName;
                }
            }
        }
        [HttpPost]
        public ActionResult Obsolete_Fixture_By_MA_ID(string MA_ID)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    double maIdAsDouble;
                    if (double.TryParse(MA_ID, out maIdAsDouble))
                    {
                        var employeeId = "";
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null) employeeId = claim.Value;
                        }
                        catch { }

                        long maId = (long)maIdAsDouble;
                        var maplan = Entity.MAPlans.FirstOrDefault(x => x.MA_ID == maId);
                        if (maplan != null)
                        {
                            maplan.StatusF = 2;
                            maplan.Modifyby = employeeId;
                            maplan.Modifydate = DateTime.Now;

                            // ลบ Tbl_ProductName ของ MA_ID นี้ออก
                            var existingProducts = Entity.Tbl_ProductName.Where(x => x.MA_ID == maId).ToList();
                            if (existingProducts.Any())
                            {
                                Entity.Tbl_ProductName.RemoveRange(existingProducts);
                            }

                            // นำชื่อเครื่องออกจาก Master Checklist (Tbl_MasterMATester) หากไม่มีเครื่องอื่นที่ Active ใช้งานชื่อนี้แล้ว
                            CleanMachineNameFromMasterChecklist(maplan.Equipment_Name, maId, Entity);

                            Entity.SaveChanges();
                            return Json(new { success = true, word = "Obsoleted successfully!" });
                        }
                        else return Json(new { success = false, word = "Couldn't obsolete fixture" });
                    }
                    else
                    {
                        return Json(new { success = false, word = "Couldn't obsolete fixture" });
                    }
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Recover_Fixture_By_MA_ID(string MA_ID)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    double maIdAsDouble;
                    if (double.TryParse(MA_ID, out maIdAsDouble))
                    {
                        var employeeId = "";
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null) employeeId = claim.Value;
                        }
                        catch { }

                        long maId = (long)maIdAsDouble;
                        var maplan = Entity.MAPlans.FirstOrDefault(x => x.MA_ID == maId);
                        if (maplan != null)
                        {
                            maplan.StatusF = 1;
                            maplan.Modifyby = employeeId;
                            maplan.Modifydate = DateTime.Now;

                            // ลบของเดิมออกก่อนกันซ้ำ (ถ้ามี)
                            var existingProducts = Entity.Tbl_ProductName.Where(x => x.MA_ID == maId).ToList();
                            if (existingProducts.Any())
                            {
                                Entity.Tbl_ProductName.RemoveRange(existingProducts);
                            }

                            // เพิ่ม product_name กลับเข้าไปใหม่โดย split ด้วย comma
                            if (!string.IsNullOrEmpty(maplan.Product_name))
                            {
                                var productNames = maplan.Product_name
                                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(p => p.Trim())
                                    .Where(p => !string.IsNullOrEmpty(p))
                                    .Distinct();

                                foreach (var pName in productNames)
                                {
                                    Entity.Tbl_ProductName.Add(new Tbl_ProductName
                                    {
                                        MA_ID = maplan.MA_ID,
                                        Product_name = pName
                                    });
                                }
                            }

                            Entity.SaveChanges();
                            return Json(new { success = true, word = "Recovered successfully!" });
                        }
                        else return Json(new { success = false, word = "Couldn't recover fixture" });
                    }
                    else
                    {
                        return Json(new { success = false, word = "Couldn't recover fixture" });
                    }
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Delete_Fixture_By_MA_ID(string MA_ID)
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    double maIdAsDouble;
                    if (double.TryParse(MA_ID, out maIdAsDouble))
                    {
                        var employeeId = "";
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null) employeeId = claim.Value;
                        }
                        catch { }

                        long maId = (long)maIdAsDouble;
                        var maplan = Entity.MAPlans.FirstOrDefault(x => x.MA_ID == maId);
                        if (maplan != null)
                        {
                            maplan.StatusF = 0;
                            maplan.Modifyby = employeeId;
                            maplan.Modifydate = DateTime.Now;

                            // ลบ Tbl_ProductName ของ MA_ID นี้ออก
                            var existingProducts = Entity.Tbl_ProductName.Where(x => x.MA_ID == maId).ToList();
                            if (existingProducts.Any())
                            {
                                Entity.Tbl_ProductName.RemoveRange(existingProducts);
                            }

                            // นำชื่อเครื่องออกจาก Master Checklist (Tbl_MasterMATester) หากไม่มีเครื่องอื่นที่ Active ใช้งานชื่อนี้แล้ว
                            CleanMachineNameFromMasterChecklist(maplan.Equipment_Name, maId, Entity);

                            Entity.SaveChanges();
                            return Json(new { success = true, word = "Deleted successfully!" });
                        }
                        else return Json(new { success = false, word = "Couldn't delete fixture" });
                    }
                    else
                    {
                        return Json(new { success = false, word = "Couldn't delete fixture" });
                    }
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        //[HttpPost]
        //public ActionResult Record_Customer(Customer data)
        //{
        //    try
        //    {
        //        Customer _Customer = null;
        //        using (MAFixtureEntities Entity = SettingAccountMAFixture())
        //        {
        //            Entity.Configuration.ProxyCreationEnabled = false;
        //            if (data.ID > 0)
        //            {
        //                _Customer = Entity.Customers.FirstOrDefault(x => x.ID == data.ID);
        //            }

        //            bool Mode_Add = false;

        //            if (_Customer == null)
        //            {

        //                _Customer = new Customer()
        //                {
        //                    Cust_name = data.Cust_name,
        //                    Cust_codename = data.Cust_codename,
        //                    CreateDate = data.CreateDate,
        //                    Createby = data.Createby
        //                };

        //                Mode_Add = true;
        //            }
        //            else
        //            {
        //                _Customer.Cust_name = data.Cust_name;
        //                _Customer.Cust_codename = data.Cust_codename;
        //                Mode_Add = false;
        //            }

        //            if (Mode_Add)
        //            {
        //                Entity.Customers.Add(_Customer);
        //            }

        //            Entity.SaveChanges();
        //        }
        //        return Json(new { success = true, word = "Saved successfully!", data = _Customer });
        //    }
        //    catch (Exception E)
        //    {
        //        return Json(new { success = false, word = "Error, " + E.Message });
        //    }
        //}
        [HttpPost]
        public ActionResult Record_EmailList(GroupEmail data)
        {
            try
            {
                GroupEmail _GroupEmail = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;
                    if (data.ID > 0)
                    {
                        _GroupEmail = Entity.GroupEmails.FirstOrDefault(x => x.ID == data.ID);
                    }

                    bool Mode_Add = false;

                    if (_GroupEmail == null)
                    {
                        long maxIdPlusOne = Entity.GroupEmails.Any() ? Entity.GroupEmails.Max(x => x.ID) + 1 : 1;
                        _GroupEmail = new GroupEmail()
                        {
                            ID = maxIdPlusOne,
                            MailTo = data.MailTo,
                            MailCC = data.MailCC,
                            Section = data.Section,
                            Acknowledge_Overdue = data.Acknowledge_Overdue
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        _GroupEmail.MailTo = data.MailTo;
                        _GroupEmail.MailCC = data.MailCC;
                        _GroupEmail.Section = data.Section;
                        _GroupEmail.Acknowledge_Overdue = data.Acknowledge_Overdue;
                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.GroupEmails.Add(_GroupEmail);
                    }

                    Entity.SaveChanges();
                }
                return Json(new { success = true, word = "Saved successfully!", data = _GroupEmail });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_MasterMAList(Tbl_MasterMATester data)
        {
            try
            {
                Tbl_MasterMATester _MasterMA = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;
                    var EmpID = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID").Value;
                    if (data.ID > 0)
                    {
                        _MasterMA = Entity.Tbl_MasterMATester.FirstOrDefault(x => x.ID == data.ID);
                    }

                    bool Mode_Add = false;

                    if (_MasterMA == null)
                    {
                        _MasterMA = new Tbl_MasterMATester()
                        {
                            DetailEng = data.DetailEng,
                            DetailThai = data.DetailThai,
                            Category = data.Category,
                            Section = data.Section,
                            Machinename = data.Machinename,
                            Type_MA = data.Type_MA,
                            Createby = EmpID,
                            Createdate = DateTime.Now,
                            Status = "1"
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        _MasterMA.DetailEng = data.DetailEng;
                        _MasterMA.DetailThai = data.DetailThai;
                        _MasterMA.Category = data.Category;
                        _MasterMA.Section = data.Section;
                        _MasterMA.Machinename = data.Machinename;
                        _MasterMA.Type_MA = data.Type_MA;
                        _MasterMA.Updateby = EmpID;
                        _MasterMA.Updatedate = DateTime.Now;
                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.Tbl_MasterMATester.Add(_MasterMA);
                    }

                    Entity.SaveChanges();
                }
                return Json(new { success = true, word = "Saved successfully!", data = _MasterMA });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_PinList(Tbl_Pindetail data)
        {
            try
            {
                Tbl_Pindetail _Pin = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;
                    if (Entity.Tbl_Pindetail.Any(x => x.Pinnumber == data.Pinnumber))
                    {
                        return Json(new { success = false, word = "Duplicate Machinename" });
                    }
                    if (data.ID > 0)
                    {
                        _Pin = Entity.Tbl_Pindetail.FirstOrDefault(x => x.ID == data.ID);
                    }

                    bool Mode_Add = false;

                    if (_Pin == null)
                    {
                        _Pin = new Tbl_Pindetail()
                        {
                            Pinnumber = data.Pinnumber,
                            Product = data.Product,
                            Lifetime = data.Lifetime,
                            Pinname = data.Pinname
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        _Pin.Pinnumber = data.Pinnumber;
                        _Pin.Product = data.Product;
                        _Pin.Lifetime = data.Lifetime;
                        _Pin.Pinname = data.Pinname;
                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.Tbl_Pindetail.Add(_Pin);
                    }

                    Entity.SaveChanges();
                }
                return Json(new { success = true, word = "Saved successfully!", data = _Pin });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Record_MachineList(Machine data)
        {
            try
            {
                Machine _Machine = null;
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false;

                    if (Entity.Machines.Any(x => x.Machinename == data.Machinename && x.Section == data.Section))
                    {
                        return Json(new { success = false, word = "Duplicate Pinnumber" });
                    }
                    if (data.ID > 0)
                    {
                        _Machine = Entity.Machines.FirstOrDefault(x => x.ID == data.ID);
                    }

                    bool Mode_Add = false;

                    if (_Machine == null)
                    {
                        _Machine = new Machine()
                        {
                            Machinename = data.Machinename,
                            Section = data.Section,
                        };

                        Mode_Add = true;
                    }
                    else
                    {
                        _Machine.Machinename = data.Machinename;
                        _Machine.Section = data.Section;
                        Mode_Add = false;
                    }

                    if (Mode_Add)
                    {
                        Entity.Machines.Add(_Machine);
                    }

                    Entity.SaveChanges();
                }
                return Json(new { success = true, word = "Saved successfully!", data = _Machine });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files, string filesMA,string type)
        {
            try
            {
                if (files != null && files.Any())
                {
                    string[] filenames = new string[files.Count()];
                    int i = 0;
                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileExtension = Path.GetExtension(file.FileName);
                            string formattedDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                            string newFileName = formattedDate + "_" + filesMA + "_" + type + "_" + (i + 1).ToString() + fileExtension;
                            filenames[i] = newFileName;

                            string folderPath = Server.MapPath("~/Uploads/");
                            string FilemaPath = Server.MapPath("~/Uploads/" + filesMA);
                            string filePath = Path.Combine(FilemaPath, newFileName);

                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }
                            if (!Directory.Exists(FilemaPath))
                            {
                                Directory.CreateDirectory(FilemaPath);
                            }

                            // Process each file
                            using (var image = Image.FromStream(file.InputStream))
                            {
                                // Check EXIF data for rotation (optional)
                                if (image.PropertyIdList.Contains(0x0112)) // EXIF orientation tag
                                {
                                    int orientation = BitConverter.ToUInt16(image.GetPropertyItem(0x0112).Value, 0);
                                    RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

                                    switch (orientation)
                                    {
                                        case 1: // Normal
                                            rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                                            break;
                                        case 3: // 180 degrees
                                            rotateFlipType = RotateFlipType.Rotate180FlipNone;
                                            break;
                                        case 6: // 90 degrees clockwise
                                            rotateFlipType = RotateFlipType.Rotate90FlipNone;
                                            break;
                                        case 8: // 90 degrees counter-clockwise
                                            rotateFlipType = RotateFlipType.Rotate270FlipNone;
                                            break;
                                    }

                                    image.RotateFlip(rotateFlipType); // Apply rotation if needed
                                }

                                // Resize the image if needed
                                int maxWidth = 1200;
                                int maxHeight = 800;
                                int newWidth = image.Width;
                                int newHeight = image.Height;

                                if (image.Width > maxWidth || image.Height > maxHeight)
                                {
                                    double ratioX = (double)maxWidth / image.Width;
                                    double ratioY = (double)maxHeight / image.Height;
                                    double ratio = Math.Min(ratioX, ratioY);

                                    newWidth = (int)(image.Width * ratio);
                                    newHeight = (int)(image.Height * ratio);
                                }

                                using (var newImage = new Bitmap(image, newWidth, newHeight))
                                {
                                    newImage.Save(filePath, ImageFormat.Jpeg);
                                }
                            }
                        }
                        i++;
                    }

                    return Json(new { success = true, data = filenames, message = "Files uploaded successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "No files selected or files are empty!" });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error, " + e.Message });
            }
        }
        //[HttpPost]
        //public ActionResult Delete_Customer_By_ID(string ID)
        //{
        //    try
        //    {
        //        // Convert the string ID to long
        //        long customerId;
        //        if (!long.TryParse(ID, out customerId))
        //        {
        //            return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
        //        }

        //        using (MAFixtureEntities Entity = SettingAccountMAFixture())
        //        {
        //            Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
        //            Customer customer = Entity.Customers.FirstOrDefault(x => x.ID == customerId);

        //            if (customer == null)
        //            {
        //                return Json(new { success = false, word = "Customer not found" }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Remove the customer from the database
        //            Entity.Customers.Remove(customer);
        //            Entity.SaveChanges(); // Commit the changes to the database

        //            return Json(new { success = true, word = "Customer deleted successfully" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        return Json(new { success = false, word = "Error: " + E.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [HttpPost]
        public ActionResult Delete_EmailList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long emailListId;
                if (!long.TryParse(ID, out emailListId))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    GroupEmail emailList = Entity.GroupEmails.FirstOrDefault(x => x.ID == emailListId);

                    if (emailList == null)
                    {
                        return Json(new { success = false, word = "Email list not found" }, JsonRequestBehavior.AllowGet);
                    }

                    // Remove the email list from the database
                    Entity.GroupEmails.Remove(emailList);
                    Entity.SaveChanges(); // Commit the changes to the database

                    return Json(new { success = true, word = "Email list deleted successfully" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Delete_MasterMAList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long MasterMAListId;
                if (!long.TryParse(ID, out MasterMAListId))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Tbl_MasterMATester MasterMAList = Entity.Tbl_MasterMATester.FirstOrDefault(x => x.ID == MasterMAListId);

                    if (MasterMAList == null)
                    {
                        return Json(new { success = false, word = "Email list not found" }, JsonRequestBehavior.AllowGet);
                    }

                    //// Remove the email list from the database
                    //Entity.Tbl_MasterMATester.Remove(MasterMAList);
                    MasterMAList.Status = "0";
                    Entity.SaveChanges(); // Commit the changes to the database

                    return Json(new { success = true, word = "Email list deleted successfully" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Delete_PinList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long MasterMAListId;
                if (!long.TryParse(ID, out MasterMAListId))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Tbl_Pindetail PinList = Entity.Tbl_Pindetail.FirstOrDefault(x => x.ID == MasterMAListId);

                    if (PinList == null)
                    {
                        return Json(new { success = false, word = "Email list not found" }, JsonRequestBehavior.AllowGet);
                    }

                    // Remove the email list from the database
                    Entity.Tbl_Pindetail.Remove(PinList);
                    Entity.SaveChanges(); // Commit the changes to the database

                    return Json(new { success = true, word = "Email list deleted successfully" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Delete_MachineList_By_ID(string ID)
        {
            try
            {
                // Convert the string ID to long
                long MachineListId;
                if (!long.TryParse(ID, out MachineListId))
                {
                    return Json(new { success = false, word = "Invalid ID format" }, JsonRequestBehavior.AllowGet);
                }

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading
                    Machine MasterMAList = Entity.Machines.FirstOrDefault(x => x.ID == MachineListId);

                    if (MasterMAList == null)
                    {
                        return Json(new { success = false, word = "Email list not found" }, JsonRequestBehavior.AllowGet);
                    }

                    // Remove the email list from the database
                    Entity.Machines.Remove(MasterMAList);
                    Entity.SaveChanges(); // Commit the changes to the database

                    return Json(new { success = true, word = "Email list deleted successfully" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error: " + E.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ViewFile(int id)
        {
            using (MAFixtureEntities Entity = SettingAccountMAFixture())
            {
                var filesMA = Entity.FilesMAs.FirstOrDefault(x => x.ID == id);

                var file = filesMA.File;
                if (file == null || file.Length == 0)
                {
                    return HttpNotFound();  // If the file is not found or is empty, return 404 error
                }

                // Set the Content-Disposition header to inline to display in the browser
                Response.AppendHeader("Content-Disposition", "inline; filename=MAFile.pdf");

                // Return the file content with the appropriate MIME type
                return File(file, "application/pdf");
            }
        }
        [HttpPost]
        public ActionResult Load_SummaryTHTPallet(string Product_name, string Section, string Type = "", string Type_MA = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    var query = from s in Entity.Tbl_Summarize_Tools
                                join m in Entity.Tbl_MasterTools.Where(x => x.Status != "0" || x.Status == null)
                                    on new { s.Product_name, s.Section, s.Type } equals new { m.Product_name, m.Section, m.Type } into sm
                                from m in sm.DefaultIfEmpty()
                                select new
                                {
                                    ID = s.ID,
                                    MasterToolID = (int?)m.ID,
                                    Product_name = s.Product_name,
                                    Section = s.Section,
                                    Type = s.Type,
                                    SummarizeAmount = s.Amount ?? 0,
                                    MasterAmount = m.Amount ?? 0
                                };

                    if (!string.IsNullOrEmpty(Product_name))
                    {
                        Product_name = Product_name.Trim();
                        query = query.Where(x => x.Product_name.Contains(Product_name));
                    }

                    if (!string.IsNullOrEmpty(Section))
                    {
                        Section = Section.Trim();
                        query = query.Where(x => x.Section == Section || x.Section.Contains(Section));
                    }

                    var typeFilter = !string.IsNullOrEmpty(Type) ? Type : Type_MA;
                    if (!string.IsNullOrEmpty(typeFilter))
                    {
                        typeFilter = typeFilter.Trim();
                        query = query.Where(x => x.Type == typeFilter || x.Type.Contains(typeFilter));
                    }

                    var data = query.OrderBy(x => x.Product_name).ToList();

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    // ดึง Cycle_Time ล่าสุดของแต่ละ MT_ID จาก Tbl_LogMasterTools โดยจัดกลุ่มป้องกันแถวซ้ำ
                    var latestCycleTimes = Entity.Tbl_LogMasterTools.AsNoTracking()
                        .Where(l => l.MT_ID.HasValue && l.Cycle_Time.HasValue)
                        .GroupBy(l => l.MT_ID.Value)
                        .Select(g => new
                        {
                            MT_ID = g.Key,
                            Cycle_Time = g.OrderByDescending(x => x.ID).Select(x => x.Cycle_Time).FirstOrDefault()
                        })
                        .ToList();

                    var cycleMap = latestCycleTimes.ToDictionary(x => x.MT_ID, x => x.Cycle_Time);

                    var resultData = data.Select(x => new
                    {
                        ID = x.ID,
                        Product_name = x.Product_name,
                        Section = x.Section,
                        Type = x.Type,
                        SummarizeAmount = x.SummarizeAmount,
                        MasterAmount = x.MasterAmount,
                        Cycle_Time = (x.MasterToolID.HasValue && cycleMap.ContainsKey(x.MasterToolID.Value)) ? cycleMap[x.MasterToolID.Value] : null
                    }).ToList();

                    var jsonResult = Json(new { success = true, data = resultData }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Load_MasterToolsList(string Product_name = "", string Section = "", string Type = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    Entity.Configuration.ProxyCreationEnabled = false; // Disable proxy creation for lazy loading

                    var query = from m in Entity.Tbl_MasterTools.AsNoTracking()
                                where m.Status != "0" || m.Status == null
                                join s in Entity.Tbl_Summarize_Tools.AsNoTracking()
                                    on new { m.Product_name, m.Section, m.Type } equals new { s.Product_name, s.Section, s.Type } into ms
                                from s in ms.DefaultIfEmpty()
                                select new
                                {
                                    ID = m.ID,
                                    Product_name = m.Product_name,
                                    Section = m.Section,
                                    Type = m.Type,
                                    Amount = m.Amount ?? 0,
                                    SummarizeAmount = s.Amount ?? 0
                                };

                    if (!string.IsNullOrEmpty(Product_name))
                    {
                        Product_name = Product_name.Trim();
                        query = query.Where(x => x.Product_name.Contains(Product_name));
                    }

                    if (!string.IsNullOrEmpty(Section))
                    {
                        Section = Section.Trim();
                        query = query.Where(x => x.Section == Section || x.Section.Contains(Section));
                    }

                    if (!string.IsNullOrEmpty(Type))
                    {
                        Type = Type.Trim();
                        query = query.Where(x => x.Type == Type || x.Type.Contains(Type));
                    }

                    var data = query.OrderBy(x => x.Product_name).ToList();

                    if (data == null)
                    {
                        return Json(new { success = false, word = "Could not load data." }, JsonRequestBehavior.AllowGet);
                    }

                    // ดึง Cycle_Time ล่าสุดของแต่ละ MT_ID จาก Tbl_LogMasterTools
                    var latestCycleTimes = Entity.Tbl_LogMasterTools.AsNoTracking()
                        .Where(l => l.MT_ID.HasValue && l.Cycle_Time.HasValue)
                        .GroupBy(l => l.MT_ID.Value)
                        .Select(g => new
                        {
                            MT_ID = g.Key,
                            Cycle_Time = g.OrderByDescending(x => x.ID).Select(x => x.Cycle_Time).FirstOrDefault()
                        })
                        .ToList();

                    var cycleMap = latestCycleTimes.ToDictionary(x => x.MT_ID, x => x.Cycle_Time);

                    var resultData = data.Select(x => new
                    {
                        ID = x.ID,
                        Product_name = x.Product_name,
                        Section = x.Section,
                        Type = x.Type,
                        Amount = x.Amount,
                        SummarizeAmount = x.SummarizeAmount,
                        Cycle_Time = cycleMap.ContainsKey(x.ID) ? cycleMap[x.ID] : null
                    }).ToList();

                    var jsonResult = Json(new { success = true, data = resultData }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + E.Message });
            }
        }
        [HttpPost]
        public ActionResult Adjust_MasterTools_Amount(int ID, int Step, string Comment = "", string UpdateBy = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    var masterTool = Entity.Tbl_MasterTools.FirstOrDefault(x => x.ID == ID);
                    if (masterTool == null)
                    {
                        return Json(new { success = false, word = "Master Tool record not found." });
                    }

                    string employeeId = UpdateBy;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null)
                            {
                                employeeId = claim.Value;
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(employeeId) && employeeId.Length > 10)
                    {
                        employeeId = employeeId.Substring(0, 10);
                    }

                    // ค้นหาข้อมูล Summarize ที่ตรงกัน (Product_name, Section, Type)
                    var summarizeTool = Entity.Tbl_Summarize_Tools.FirstOrDefault(s =>
                        s.Product_name == masterTool.Product_name &&
                        s.Section == masterTool.Section &&
                        s.Type == masterTool.Type);

                    int maxLimit = summarizeTool != null && summarizeTool.Amount.HasValue ? summarizeTool.Amount.Value : 0;
                    int oldAmount = masterTool.Amount ?? 0;
                    int newAmount = oldAmount + Step;

                    if (newAmount < 0)
                    {
                        return Json(new { success = false, word = "Amount cannot be less than 0." });
                    }

                    if (newAmount > maxLimit)
                    {
                        return Json(new { success = false, word = "Amount cannot exceed Summarize Amount (" + maxLimit + ")." });
                    }

                    string diffText = (newAmount >= oldAmount ? "+" : "") + (newAmount - oldAmount);

                    // สร้างข้อความบันทึกรายละเอียดครบถ้วนลงใน Update_Log (varchar(500))
                    string userComment = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                    string detailedLog = string.Format(
                        "Amount: {0} -> {1} ({2}) | Product: {3} | Section: {4} | Type: {5} | Summarize Max: {6}{7}",
                        oldAmount,
                        newAmount,
                        diffText,
                        masterTool.Product_name,
                        masterTool.Section,
                        masterTool.Type,
                        maxLimit,
                        !string.IsNullOrEmpty(userComment) ? " | Comment: " + userComment : ""
                    );
                    if (detailedLog.Length > 500)
                    {
                        detailedLog = detailedLog.Substring(0, 500);
                    }

                    // บันทึกความเห็นหรือหมายเหตุลงใน Update_Comment (nvarchar(500) รองรับ Unicode)
                    string logComment = !string.IsNullOrEmpty(userComment)
                        ? userComment
                        : string.Format("Amount changed: {0} -> {1} ({2})", oldAmount, newAmount, diffText);
                    if (logComment.Length > 500)
                    {
                        logComment = logComment.Substring(0, 500);
                    }

                    var log = new Tbl_LogMasterTools
                    {
                        MT_ID = masterTool.ID,
                        Updateby = employeeId,
                        Updatedate = DateTime.Now,
                        Update_Comment = logComment,
                        Update_Log = detailedLog
                    };
                    Entity.Tbl_LogMasterTools.Add(log);

                    masterTool.Amount = newAmount;
                    Entity.SaveChanges();

                    // ส่งอีเมลแจ้งเตือน E - Industrial Engineering
                    try
                    {
                        string userEmail = "";
                        try
                        {
                            var emailClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_EMAIL");
                            if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                            {
                                userEmail = emailClaim.Value.Trim();
                            }
                        }
                        catch { }

                        SendEmail(log, masterTool, oldAmount, maxLimit, userEmail);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("SendEmail Trigger Error: " + ex.Message);
                    }

                    return Json(new { success = true, newAmount = newAmount, word = "Amount updated successfully." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add(validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                return Json(new { success = false, word = "Validation Error: " + string.Join("; ", errorMessages) });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + (E.InnerException != null ? E.InnerException.Message : E.Message) });
            }
        }
        [HttpPost]
        public ActionResult Update_MasterTools_Amount(int ID, int NewAmount, string Comment = "", string UpdateBy = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    var masterTool = Entity.Tbl_MasterTools.FirstOrDefault(x => x.ID == ID);
                    if (masterTool == null)
                    {
                        return Json(new { success = false, word = "Master Tool record not found." });
                    }

                    string employeeId = UpdateBy;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null)
                            {
                                employeeId = claim.Value;
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(employeeId) && employeeId.Length > 10)
                    {
                        employeeId = employeeId.Substring(0, 10);
                    }

                    // ค้นหาข้อมูล Summarize ที่ตรงกัน (Product_name, Section, Type)
                    var summarizeTool = Entity.Tbl_Summarize_Tools.FirstOrDefault(s =>
                        s.Product_name == masterTool.Product_name &&
                        s.Section == masterTool.Section &&
                        s.Type == masterTool.Type);

                    int maxLimit = summarizeTool != null && summarizeTool.Amount.HasValue ? summarizeTool.Amount.Value : 0;
                    int oldAmount = masterTool.Amount ?? 0;

                    if (NewAmount < 0)
                    {
                        return Json(new { success = false, word = "Amount cannot be less than 0." });
                    }

                    if (NewAmount > maxLimit)
                    {
                        return Json(new { success = false, word = "Cannot save: Amount cannot exceed Summarize Amount (" + maxLimit + ")." });
                    }

                    string diffText = (NewAmount >= oldAmount ? "+" : "") + (NewAmount - oldAmount);

                    // สร้างข้อความบันทึกรายละเอียดครบถ้วนลงใน Update_Log (varchar(500))
                    string userComment = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                    string detailedLog = string.Format(
                        "Amount: {0} -> {1} ({2}) | Product: {3} | Section: {4} | Type: {5} | Summarize Max: {6}{7}",
                        oldAmount,
                        NewAmount,
                        diffText,
                        masterTool.Product_name,
                        masterTool.Section,
                        masterTool.Type,
                        maxLimit,
                        !string.IsNullOrEmpty(userComment) ? " | Comment: " + userComment : ""
                    );
                    if (detailedLog.Length > 500)
                    {
                        detailedLog = detailedLog.Substring(0, 500);
                    }

                    // บันทึกความเห็นหรือหมายเหตุลงใน Update_Comment (nvarchar(500) รองรับ Unicode)
                    string logComment = !string.IsNullOrEmpty(userComment)
                        ? userComment
                        : string.Format("Amount changed: {0} -> {1} ({2})", oldAmount, NewAmount, diffText);
                    if (logComment.Length > 500)
                    {
                        logComment = logComment.Substring(0, 500);
                    }

                    var log = new Tbl_LogMasterTools
                    {
                        MT_ID = masterTool.ID,
                        Updateby = employeeId,
                        Updatedate = DateTime.Now,
                        Update_Comment = logComment,
                        Update_Log = detailedLog
                    };
                    Entity.Tbl_LogMasterTools.Add(log);

                    masterTool.Amount = NewAmount;
                    Entity.SaveChanges();

                    // ส่งอีเมลแจ้งเตือน E - Industrial Engineering
                    try
                    {
                        string userEmail = "";
                        try
                        {
                            var emailClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_EMAIL");
                            if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                            {
                                userEmail = emailClaim.Value.Trim();
                            }
                        }
                        catch { }

                        SendEmail(log, masterTool, oldAmount, maxLimit, userEmail);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("SendEmail Trigger Error: " + ex.Message);
                    }

                    return Json(new { success = true, newAmount = NewAmount, word = "Amount updated successfully." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add(validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                return Json(new { success = false, word = "Validation Error: " + string.Join("; ", errorMessages) });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + (E.InnerException != null ? E.InnerException.Message : E.Message) });
            }
        }
        [HttpPost]
        public ActionResult Update_MasterTools_CycleTime(int ID, double NewCycleTime, string Comment = "", string UpdateBy = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    var masterTool = Entity.Tbl_MasterTools.FirstOrDefault(x => x.ID == ID);
                    if (masterTool == null)
                    {
                        return Json(new { success = false, word = "Master Tool record not found." });
                    }

                    bool isIndustrial = false;
                    string userEmail = "";
                    try
                    {
                        var emailClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_EMAIL");
                        if (emailClaim != null && !string.IsNullOrEmpty(emailClaim.Value))
                        {
                            userEmail = emailClaim.Value.Trim();
                        }
                    }
                    catch { }

                    try
                    {
                        var deptClaim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_DEPARTMENT");
                        if (deptClaim != null && !string.IsNullOrEmpty(deptClaim.Value))
                        {
                            if (deptClaim.Value.IndexOf("Industrial", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isIndustrial = true;
                            }
                        }
                    }
                    catch { }

                    // ข้อยกเว้นสำหรับ Jinnawit.Ananpatiwet@gpv-group.com ให้มีสิทธิ์บันทึกได้เสมอ
                    if (!string.IsNullOrEmpty(userEmail) && userEmail.IndexOf("Jinnawit.Ananpatiwet", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isIndustrial = true;
                    }

                    if (!isIndustrial)
                    {
                        return Json(new { success = false, word = "Permission denied: Only E - Industrial can modify Cycle Time." });
                    }

                    if (NewCycleTime < 0)
                    {
                        return Json(new { success = false, word = "Cycle Time cannot be less than 0." });
                    }

                    string employeeId = UpdateBy;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null)
                            {
                                employeeId = claim.Value;
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(employeeId) && employeeId.Length > 10)
                    {
                        employeeId = employeeId.Substring(0, 10);
                    }

                    // ค้นหา Cycle_Time ล่าสุดเดิม
                    var lastLog = Entity.Tbl_LogMasterTools
                        .Where(l => l.MT_ID == masterTool.ID && l.Cycle_Time.HasValue)
                        .OrderByDescending(l => l.ID)
                        .FirstOrDefault();

                    double oldCycleTime = lastLog != null && lastLog.Cycle_Time.HasValue ? lastLog.Cycle_Time.Value : 0.0;
                    double diff = NewCycleTime - oldCycleTime;
                    string diffText = (diff >= 0 ? "+" : "") + diff.ToString("0.##");

                    string userComment = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                    string detailedLog = string.Format(
                        "Cycle Time: {0:0.##} -> {1:0.##} ({2}) | Product: {3} | Section: {4} | Type: {5} | By: {6}{7}",
                        oldCycleTime,
                        NewCycleTime,
                        diffText,
                        masterTool.Product_name,
                        masterTool.Section,
                        masterTool.Type,
                        employeeId,
                        !string.IsNullOrEmpty(userComment) ? " | Comment: " + userComment : ""
                    );
                    if (detailedLog.Length > 500)
                    {
                        detailedLog = detailedLog.Substring(0, 500);
                    }

                    string logComment = !string.IsNullOrEmpty(userComment)
                        ? userComment
                        : string.Format("Cycle Time changed: {0:0.##} -> {1:0.##} ({2})", oldCycleTime, NewCycleTime, diffText);
                    if (logComment.Length > 500)
                    {
                        logComment = logComment.Substring(0, 500);
                    }

                    var log = new Tbl_LogMasterTools
                    {
                        MT_ID = masterTool.ID,
                        Cycle_Time = NewCycleTime,
                        Updateby = employeeId,
                        Updatedate = DateTime.Now,
                        Update_Comment = logComment,
                        Update_Log = detailedLog
                    };
                    Entity.Tbl_LogMasterTools.Add(log);
                    Entity.SaveChanges();

                    return Json(new { success = true, newCycleTime = NewCycleTime, word = "Cycle Time updated successfully." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add(validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                return Json(new { success = false, word = "Validation Error: " + string.Join("; ", errorMessages) });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + (E.InnerException != null ? E.InnerException.Message : E.Message) });
            }
        }
        [HttpPost]
        public ActionResult Delete_MasterTool(int ID, string Comment = "", string UpdateBy = "")
        {
            try
            {
                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    var masterTool = Entity.Tbl_MasterTools.FirstOrDefault(x => x.ID == ID);
                    if (masterTool == null)
                    {
                        return Json(new { success = false, word = "Master Tool not found." });
                    }

                    if (masterTool.Status == "0")
                    {
                        return Json(new { success = false, word = "This Master Tool has already been deleted." });
                    }

                    string employeeId = UpdateBy;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null)
                            {
                                employeeId = claim.Value;
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(employeeId) && employeeId.Length > 10)
                    {
                        employeeId = employeeId.Substring(0, 10);
                    }

                    // ค้นหา Cycle_Time ล่าสุดเดิม เพื่อบันทึกเก็บไว้ใน Log
                    var lastLog = Entity.Tbl_LogMasterTools
                        .Where(l => l.MT_ID == masterTool.ID && l.Cycle_Time.HasValue)
                        .OrderByDescending(l => l.ID)
                        .FirstOrDefault();
                    double? lastCycleTime = lastLog != null ? lastLog.Cycle_Time : null;

                    string userComment = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                    string detailedLog = string.Format(
                        "Deleted Master Tool: Product: {0} | Section: {1} | Type: {2} | Amount: {3} | By: {4}{5}",
                        masterTool.Product_name,
                        masterTool.Section,
                        masterTool.Type,
                        masterTool.Amount,
                        employeeId,
                        !string.IsNullOrEmpty(userComment) ? " | Reason: " + userComment : ""
                    );
                    if (detailedLog.Length > 500)
                    {
                        detailedLog = detailedLog.Substring(0, 500);
                    }

                    string logComment = !string.IsNullOrEmpty(userComment)
                        ? userComment
                        : string.Format("Deleted tool (ID: {0}, Product: {1})", masterTool.ID, masterTool.Product_name);
                    if (logComment.Length > 500)
                    {
                        logComment = logComment.Substring(0, 500);
                    }

                    // ปรับ Status เป็น 0 (ลบ)
                    masterTool.Status = "0";

                    // บันทึกประวัติลง Tbl_LogMasterTools
                    var log = new Tbl_LogMasterTools
                    {
                        MT_ID = masterTool.ID,
                        Cycle_Time = lastCycleTime,
                        Updateby = employeeId,
                        Updatedate = DateTime.Now,
                        Update_Comment = logComment,
                        Update_Log = detailedLog
                    };
                    Entity.Tbl_LogMasterTools.Add(log);
                    Entity.SaveChanges();

                    return Json(new { success = true, word = "Master Tool deleted successfully." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add(validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                return Json(new { success = false, word = "Validation Error: " + string.Join("; ", errorMessages) });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + (E.InnerException != null ? E.InnerException.Message : E.Message) });
            }
        }
        [HttpPost]
        public ActionResult Add_MasterTool(string Product_name, string Section, string Type, int Amount, string Comment = "", string CreateBy = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Product_name) || string.IsNullOrWhiteSpace(Section) || string.IsNullOrWhiteSpace(Type))
                {
                    return Json(new { success = false, word = "Please fill in all required fields (Product Name, Section, Type)." });
                }

                if (Amount < 0)
                {
                    return Json(new { success = false, word = "Amount cannot be less than 0." });
                }

                Product_name = Product_name.Trim();
                Section = Section.Trim();
                Type = Type.Trim();

                using (MAFixtureEntities Entity = SettingAccountMAFixture())
                {
                    string employeeId = CreateBy;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        try
                        {
                            var claim = HttpContext.GetOwinContext().Authentication.User.FindFirst("EMPLOYEE_ID");
                            if (claim != null)
                            {
                                employeeId = claim.Value;
                            }
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(employeeId) && employeeId.Length > 6)
                    {
                        employeeId = employeeId.Substring(0, 6);
                    }

                    // ตรวจสอบข้อมูลซ้ำตาม Product_name, Section, Type
                    var existingTool = Entity.Tbl_MasterTools.FirstOrDefault(x =>
                        x.Product_name == Product_name &&
                        x.Section == Section &&
                        x.Type == Type);

                    if (existingTool != null)
                    {
                        // ถ้ามีข้อมูลอยู่และยังไม่ได้ถูกลบ (Status != "0")
                        if (existingTool.Status != "0")
                        {
                            return Json(new { success = false, word = "This Master Tool already exists for this Section and Type." });
                        }

                        // ถ้าเคยถูกลบไว้ (Status == "0") ให้ทำการ Restore กลับมาใช้งานใหม่
                        int oldAmount = existingTool.Amount ?? 0;
                        existingTool.Status = "1";
                        existingTool.Amount = Amount;
                        existingTool.Createby = employeeId;
                        existingTool.Createdate = DateTime.Now;

                        string userComment = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                        string detailedLog = string.Format(
                            "Restored Master Tool: Product: {0} | Section: {1} | Type: {2} | Amount: {3} (was {4}) | By: {5}{6}",
                            Product_name,
                            Section,
                            Type,
                            Amount,
                            oldAmount,
                            employeeId,
                            !string.IsNullOrEmpty(userComment) ? " | Comment: " + userComment : ""
                        );
                        if (detailedLog.Length > 500)
                        {
                            detailedLog = detailedLog.Substring(0, 500);
                        }

                        string logComment = !string.IsNullOrEmpty(userComment)
                            ? userComment
                            : string.Format("Restored & added Master Tool (ID: {0}, Amount: {1})", existingTool.ID, Amount);
                        if (logComment.Length > 500)
                        {
                            logComment = logComment.Substring(0, 500);
                        }

                        var restoreLog = new Tbl_LogMasterTools
                        {
                            MT_ID = existingTool.ID,
                            Updateby = employeeId,
                            Updatedate = DateTime.Now,
                            Update_Comment = logComment,
                            Update_Log = detailedLog
                        };
                        Entity.Tbl_LogMasterTools.Add(restoreLog);
                        Entity.SaveChanges();

                        return Json(new { success = true, word = "Master Tool restored and updated successfully." });
                    }

                    // กรณีเพิ่มรายการใหม่
                    var newTool = new Tbl_MasterTools
                    {
                        Product_name = Product_name,
                        Section = Section,
                        Type = Type,
                        Amount = Amount,
                        Status = "1",
                        Createby = employeeId,
                        Createdate = DateTime.Now
                    };
                    Entity.Tbl_MasterTools.Add(newTool);
                    Entity.SaveChanges();

                    string remark = !string.IsNullOrEmpty(Comment) ? Comment.Trim() : "";
                    string createDetailedLog = string.Format(
                        "Created Master Tool: Product: {0} | Section: {1} | Type: {2} | Amount: {3} | By: {4}{5}",
                        Product_name,
                        Section,
                        Type,
                        Amount,
                        employeeId,
                        !string.IsNullOrEmpty(remark) ? " | Comment: " + remark : ""
                    );
                    if (createDetailedLog.Length > 500)
                    {
                        createDetailedLog = createDetailedLog.Substring(0, 500);
                    }

                    string createLogComment = !string.IsNullOrEmpty(remark)
                        ? remark
                        : string.Format("Created Master Tool (ID: {0}, Amount: {1})", newTool.ID, Amount);
                    if (createLogComment.Length > 500)
                    {
                        createLogComment = createLogComment.Substring(0, 500);
                    }

                    var log = new Tbl_LogMasterTools
                    {
                        MT_ID = newTool.ID,
                        Updateby = employeeId,
                        Updatedate = DateTime.Now,
                        Update_Comment = createLogComment,
                        Update_Log = createDetailedLog
                    };
                    Entity.Tbl_LogMasterTools.Add(log);
                    Entity.SaveChanges();

                    return Json(new { success = true, word = "Master Tool added successfully." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add(validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                return Json(new { success = false, word = "Validation Error: " + string.Join("; ", errorMessages) });
            }
            catch (Exception E)
            {
                return Json(new { success = false, word = "Error, " + (E.InnerException != null ? E.InnerException.Message : E.Message) });
            }
        }
        private void SendEmail(Tbl_LogMasterTools data, Tbl_MasterTools masterTool, int oldAmount, int maxLimit, string userEmail)
        {
            try
            {
                using (WCF227Service.Service1Client WCF227 = new WCF227Service.Service1Client())
                using (TraningdatabaseEntities Entity = SettingAccountTraningdatabase())
                {
                    string greeting = "Dear All,";
                    string introMessage = "";

                    List<string> mailToList = new List<string>();
                    List<string> mailCcList = new List<string>();

                    string prodName = masterTool != null && masterTool.Product_name != null ? masterTool.Product_name : "-";
                    string section = masterTool != null && masterTool.Section != null ? masterTool.Section : "-";
                    string toolType = masterTool != null && masterTool.Type != null ? masterTool.Type : "-";
                    int newAmount = masterTool != null && masterTool.Amount.HasValue ? masterTool.Amount.Value : 0;
                    string diffText = (newAmount >= oldAmount ? "+" : "") + (newAmount - oldAmount);
                    string diffColor = newAmount >= oldAmount ? "#16a34a" : "#dc2626";

                    string subject = string.Format("[MA Fixture] Master Tool Amount Updated - {0} ({1} / {2})", prodName, section, toolType);
                    greeting = "Dear All,";
                    introMessage = "Master Tool amount has been updated in the MA Fixture System.<br>Please find the update details below:";

                    // ดึงรายชื่ออีเมลของแผนก E - Industrial Engineering จาก Tbl_employee
                    try
                    {
                        var employees = Entity.Tbl_employee
                            .Where(x => (x.Depart == "E - Industrial Engineering" || x.Depart.Contains("Industrial")) && x.Status == "A")
                            .ToList();

                        foreach (var emp in employees)
                        {
                            if (!string.IsNullOrEmpty(emp.Email))
                            {
                                mailToList.Add(emp.Email.Trim());
                            }
                        }
                    }
                    catch { }

                    // ดึงรายชื่ออีเมลกลุ่มจาก WCF227 (ถ้ามี)
                    try
                    {
                        string convertedMailTo = WCF227.Convertemailfromstring("E - Industrial Engineering");
                        if (!string.IsNullOrEmpty(convertedMailTo))
                        {
                            var splitEmails = convertedMailTo.Split(new[] { ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .Where(x => !string.IsNullOrEmpty(x));
                            mailToList.AddRange(splitEmails);
                        }
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        mailCcList.Add(userEmail.Trim());
                    }

                    mailToList = mailToList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    mailCcList = mailCcList.Where(c => !mailToList.Contains(c, StringComparer.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    if (!mailToList.Any() && !mailCcList.Any())
                    {
                        return; // No recipients
                    }

                    string mailBody = "";
                    mailBody += "<p style='font-size: 14px; color: #333;'>" + greeting + "</p>";
                    mailBody += "<p style='font-size: 14px; color: #333;'>" + introMessage + "</p>";

                    mailBody += "<table style='border-collapse: collapse; width: 100%; font-size: 13px; text-align: left; margin-top: 15px; font-family: Arial, sans-serif;'>";
                    mailBody += "<thead style='background-color: #1e293b; color: white;'>";
                    mailBody += "<tr>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px;'>Product Name</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px;'>Section</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px;'>Type</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>Old Amount</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>New Amount</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>Change</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>Summarize Max</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>Updated By</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>Date</th>";
                    mailBody += "<th style='border: 1px solid #cbd5e1; padding: 10px;'>Comment / Remark</th>";
                    mailBody += "</tr>";
                    mailBody += "</thead>";
                    mailBody += "<tbody>";
                    mailBody += "<tr style='background-color: #f8fafc;'>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; font-weight: bold; color: #1e40af;'>" + System.Web.HttpUtility.HtmlEncode(prodName) + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px;'>" + System.Web.HttpUtility.HtmlEncode(section) + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px;'>" + System.Web.HttpUtility.HtmlEncode(toolType) + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center; color: #64748b;'>" + oldAmount + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center; font-weight: bold; font-size: 14px;'>" + newAmount + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center; font-weight: bold; color: " + diffColor + ";'>" + diffText + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center; color: #64748b;'>" + maxLimit + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center;'>" + System.Web.HttpUtility.HtmlEncode(data != null && data.Updateby != null ? data.Updateby : "") + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; text-align: center; font-size: 12px; color: #64748b;'>" + (data != null && data.Updatedate.HasValue ? data.Updatedate.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) + "</td>";
                    mailBody += "<td style='border: 1px solid #cbd5e1; padding: 10px; color: #334155;'>" + System.Web.HttpUtility.HtmlEncode(data != null && !string.IsNullOrEmpty(data.Update_Comment) ? data.Update_Comment : "-") + "</td>";
                    mailBody += "</tr>";
                    mailBody += "</tbody>";
                    mailBody += "</table>";

                    mailBody += "<div style='margin-top: 25px; font-size: 13px; color: #333;'>";
                    mailBody += "<p>Please visit the system to check details: <a href='https://rdreporters/MATesting/Global/MasterToolsList' style='color: #2563eb; text-decoration: none; font-weight: bold;'>MA Fixture System - Master Tools List</a></p>";
                    mailBody += "<p style='color: #64748b;'>Best regards,<br>MA Fixture System</p>";
                    mailBody += "</div>";

                    string mailHost = "10.52.60.227";
                    try
                    {
                        string host = WCF227.GetHostemail();
                        if (!string.IsNullOrEmpty(host))
                        {
                            mailHost = host;
                        }
                    }
                    catch { }

                    int mailPort = 25;
                    string mailForm = "sw-rd@gpv-group.com";
                    string mailDisplay = "MA Fixture System";

                    using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(mailHost, mailPort))
                    using (System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage())
                    {
                        message.From = new System.Net.Mail.MailAddress(mailForm, mailDisplay);

                        // BCC
                        try
                        {
                            string[] emaillist = WCF227.Getemailall("IndirectSW");
                            if (emaillist != null && emaillist.Length > 0 && !string.IsNullOrEmpty(emaillist[0]))
                            {
                                message.Bcc.Add(emaillist[0]);
                            }
                        }
                        catch { }

                        foreach (var email in mailToList)
                        {
                            message.To.Add(email);
                        }

                        foreach (var email in mailCcList)
                        {
                            message.CC.Add(email);
                        }

                        message.Subject = subject;
                        message.Body = mailBody;
                        message.IsBodyHtml = true;

                        if (!string.IsNullOrEmpty(userEmail) && userEmail.Equals("Jinnawit.Ananpatiwet@gpv-group.com", StringComparison.OrdinalIgnoreCase))
                        {
                            message.To.Clear();
                            message.CC.Clear();
                            message.To.Add("Jinnawit.Ananpatiwet@gpv-group.com");
                        }

                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendEmail Error: " + ex.Message);
            }
        }
    }
}