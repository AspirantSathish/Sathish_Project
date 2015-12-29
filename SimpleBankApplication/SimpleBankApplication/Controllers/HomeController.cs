using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimpleBankApplication.Models;
using System.Data;


namespace SimpleBankApplication.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseContext db = new DatabaseContext();       
        int RecordCount = 0;
        decimal AccountHolderAmount = 0;
        Random Ran_Number = new Random();
        string Number = string.Empty, Description = string.Empty;
        public ActionResult Index()
        {
            if (Session["UserName"] != null)
            {
                Session.Clear();
                Session.Abandon();                
            }
            LoginDetails LoginDetails = new LoginDetails();
            return View(LoginDetails);
        }
        [HttpPost]
        public ActionResult Index(LoginDetails login)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (login.UserName.ToUpper() == "ADMIN" && login.Password.ToUpper() == "ADMIN")
                    {
                        Session["UserName"] = login.UserName;
                        return RedirectToAction("About", "Home");
                    }
                    else
                    {
                        Session["UserName"] = null;
                        ModelState.AddModelError("InvalidMessage", "Invalid UserName or Password");                       
                    }
                }
                return View(login);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult CreateAccount(string UserName = null)
        {
            try{
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (Session["CreateAccountSuccess"] != null)
            {
                ViewBag.Success = Session["CreateAccountSuccess"].ToString();
                ViewBag.Update = "Account Created Succcessfully...";
                if (Session["CreateAccountUpdate"] != null)
                {
                    ViewBag.Update = "Updated Succcessfully...";
                    Session["CreateAccountUpdate"] = null;
                }
                Session["CreateAccountSuccess"] = null;
            }

            BankAccount bankaccount = new BankAccount();
            if (UserName != null)
            {
                bankaccount = bankaccount = db.bank_account_list.Find(UserName);
            }
            return View(bankaccount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult BankAccountList()
        {
            try
            {
                if (Session["UserName"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(db.bank_account_list.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult CreateAccount(BankAccount bankaccount)
        {
           try{
            if (bankaccount.AcNumber==0) //Create New Account 
            {
                if (ModelState.IsValid)
                {

                    if (CheckUserNameStatus(bankaccount.UserName))
                    {
                        RecordCount = db.bank_account_list.Count();
                        Number = Convert.ToString(GetRandomNumber(RecordCount, 99999999)) + Convert.ToString(RecordCount);
                        bankaccount.AcNumber = Convert.ToInt64(Number);
                        bankaccount.CreatedDateTime = DateTime.Now;
                        db.bank_account_list.Add(bankaccount);
                        db.SaveChanges();
                        Session["CreateAccountSuccess"] = "User Name : " + bankaccount.UserName + ", A/c Number : " + bankaccount.AcNumber + "";
                        
                        ModelState.Clear();
                        bankaccount = new BankAccount();
                        return RedirectToAction("CreateAccount", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("InvalidMessage", "User Name Already Exists...");
                    }
                }
                
            }
            else  //Update Bank Account Details
            {
                bankaccount.CreatedDateTime = DateTime.Now;
                db.Entry(bankaccount).State = EntityState.Modified;
                db.SaveChanges();
                Session["CreateAccountSuccess"] = "User Name : " + bankaccount.UserName + ", A/c Number : " + bankaccount.AcNumber + "";
                Session["CreateAccountUpdate"] = "Updated";
                ModelState.Clear();
                bankaccount = new BankAccount();
                return RedirectToAction("CreateAccount", "Home");
            }
            return View(bankaccount);
           }
           catch (Exception ex)
           {
               throw ex;
           }
            
        }
        [HttpGet]
        public ActionResult BalanceEnquiry(string UserName)
        {
            try
            {
                if (Session["UserName"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (UserName != null && UserName != "")
                {
                    if (!CheckUserNameStatus(UserName))
                    {
                        var transactiondetails = from trans in db.transaction_details_list
                                                 join user in db.bank_account_list on trans.UserName equals user.UserName
                                                 where trans.UserName == UserName
                                                 orderby trans.TransactionDateTime
                                                 select new
                                                 {
                                                     trans.UserName,
                                                     user.AcNumber,
                                                     user.Name,
                                                     trans.Amount
                                                 };
                        AccountHolderAmount = transactiondetails.Sum(T => T.Amount);
                        foreach (var details in transactiondetails)
                        {
                            ViewBag.UserName = details.UserName.ToString();
                            ViewBag.AccountNumber = details.AcNumber.ToString();
                            ViewBag.CurrentAmount = AccountHolderAmount;
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("InvalidMessage", "Invalid User Name");
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Operations()
        {
            try
            {
                if (Session["UserName"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (Session["TransferSuccess"] != null)
                {
                    string[] Success = Session["TransferSuccess"].ToString().Split('@');
                    ViewBag.Success = Success[0].ToString();
                    ViewBag.Update = Success[1].ToString();
                    Session["TransferSuccess"] = null;
                }

                TransactionDetails transactiondetails = new TransactionDetails();
                transactiondetails.TransactionDateTime = DateTime.Now;
                ViewBag.TransModeList = TransactionModeList();
                return View(transactiondetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult Operations(TransactionDetails transactiondetails)
        {
            try
            {
                
                BankAccount accountdetails = new BankAccount();
                string ErrorMessage = "";
                long transactionid = 0;
                if (ModelState.IsValid)
                {
                    if (!CheckUserNameStatus(transactiondetails.UserName))
                    {
                        transactionid = db.transaction_details_list.Count();
                        Number = Convert.ToString(GetRandomNumber(Convert.ToInt32(transactionid), 999999999)) + Convert.ToString(transactionid);
                        transactiondetails.TransactionId = Convert.ToInt64(Number);
                        transactiondetails.TransactionDateTime = DateTime.Now;
                        if (transactiondetails.TransactionMode == 1) //Deposit Amount
                        {
                            db.transaction_details_list.Add(transactiondetails);
                            db.SaveChanges();
                            Session["TransferSuccess"] = "Amount Deposited Successfully...!" + "@" + "Your Current Balance is : " + UserCurrentBalance(transactiondetails.UserName).ToString("N2") + "";
                        }
                        else if (transactiondetails.TransactionMode == 2) //Withdraw Amount
                        {
                            if (transactiondetails.Amount < UserCurrentBalance(transactiondetails.UserName))
                            {
                                transactiondetails.Amount = -(transactiondetails.Amount);
                                db.transaction_details_list.Add(transactiondetails);
                                db.SaveChanges();
                                Session["TransferSuccess"] = "Amount Withdrawal Successfully...!" + "@" + "Your Current Balance is : " + UserCurrentBalance(transactiondetails.UserName).ToString("N2") + "";
                            }
                            else
                            {
                                ErrorMessage = "Insufficient Balance...";
                            }

                        }
                        else if (transactiondetails.TransactionMode == 3)  //Transfer Amount
                        {
                            if (!CheckUserNameStatus(transactiondetails.UserName))
                            {
                                if (transactiondetails.Amount < UserCurrentBalance(transactiondetails.UserName))
                                {
                                    transactiondetails.Amount = -(transactiondetails.Amount);
                                    //Debit
                                    db.transaction_details_list.Add(transactiondetails);
                                    db.SaveChanges();
                                    //Credit 
                                    TransactionDetails transferAmount = new TransactionDetails();
                                    transactionid = db.transaction_details_list.Count();
                                    Number = Convert.ToString(GetRandomNumber(Convert.ToInt32(transactionid), 9999999)) + Convert.ToString(transactionid);
                                    transferAmount.TransactionId = Convert.ToInt64(Number);
                                    transferAmount.UserName = transactiondetails.TransferToUserName;
                                    transferAmount.TransferFromUserName = transactiondetails.UserName;
                                    transferAmount.TransferToUserName = "";
                                    transferAmount.Amount = -(transactiondetails.Amount);
                                    transferAmount.TransactionDateTime = DateTime.Now;
                                    transferAmount.TransactionMode = transactiondetails.TransactionMode;
                                    db.transaction_details_list.Add(transferAmount);
                                    db.SaveChanges();
                                    Session["TransferSuccess"] = "Amount Transfered Successfully...!" + "@" + "Your Current Balance is : " + UserCurrentBalance(transactiondetails.UserName).ToString("N2") + "";

                                }
                                else
                                {
                                    ErrorMessage = "Insufficient Balance...You can't Transfer Amount";
                                }
                            }
                            else { ErrorMessage = "Invalid Transfer User Name..."; }
                        }
                    }
                    else
                    {
                        ErrorMessage = "Invalid User Name...";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid Process...";
                }
                if (ErrorMessage != "")
                {
                    ModelState.AddModelError("InvalidMessage", ErrorMessage);
                }
                else
                {
                    transactiondetails = new TransactionDetails();
                    ModelState.Clear();
                    return RedirectToAction("Operations", "Home");
                }
                ViewBag.TransModeList = TransactionModeList();
                return View(transactiondetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet]
        public ActionResult Statement(string UserName)
        {
            try
            {
                if (Session["UserName"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (UserName != null && UserName != "")
                {
                    if (!CheckUserNameStatus(UserName))
                    {
                        List<BankStatement> bankstatementlist = new List<BankStatement>();
                        var transactiondetails = from trans in db.transaction_details_list
                                                 join user in db.bank_account_list on trans.UserName equals user.UserName
                                                 where trans.UserName == UserName
                                                 orderby trans.TransactionDateTime
                                                 select new
                                                 {
                                                     trans.UserName,
                                                     user.AcNumber,
                                                     user.Name,
                                                     trans.TransactionDateTime,
                                                     trans.Amount,
                                                     trans.DepositorName,
                                                     trans.TransactionId,
                                                     trans.TransactionMode,
                                                     trans.TransferFromUserName,
                                                     trans.TransferToUserName
                                                 };
                        AccountHolderAmount = transactiondetails.Sum(T => T.Amount);
                        foreach (var details in transactiondetails)
                        {
                            if (details.TransactionMode == 1)
                            {
                                Description = details.DepositorName == null ? "Deposited" : "Amount Deposited by " + details.DepositorName + "";                                
                                bankstatementlist.Add(new BankStatement { UserName = details.Name, AcNumber = details.AcNumber, CreditAmount = details.Amount.ToString("N2"), DateTime = details.TransactionDateTime.ToString("dd-MMM-yyyy HH:mm"), DebitAmount = "", Description = Description });
                            }
                            else if (details.TransactionMode == 2)
                            {
                                Description = details.DepositorName == null ? "Withdrawal" : "Amount Withdrawal by " + details.DepositorName + "";
                                bankstatementlist.Add(new BankStatement { UserName = details.Name, AcNumber = details.AcNumber, CreditAmount = "", DateTime = details.TransactionDateTime.ToString("dd-MMM-yyyy HH:mm"), DebitAmount = (-(details.Amount)).ToString("N2"), Description = Description });
                            }
                            else if (details.TransactionMode == 3)
                            {
                                if (details.TransferToUserName != "")
                                {
                                    Description = details.TransferToUserName == null ? "Transfered" : "Amount Transfered to " + details.TransferToUserName + "";
                                    bankstatementlist.Add(new BankStatement { UserName = details.Name, AcNumber = details.AcNumber, CreditAmount = "", DateTime = details.TransactionDateTime.ToString("dd-MMM-yyyy HH:mm"), DebitAmount = (-(details.Amount)).ToString("N2"), Description = Description });

                                }
                                else
                                {
                                    Description = details.TransferFromUserName == null ? "Deposited" : "Amount Deposited from " + details.TransferFromUserName + "";
                                    bankstatementlist.Add(new BankStatement { UserName = details.Name, AcNumber = details.AcNumber, CreditAmount = details.Amount.ToString("N2"), DateTime = details.TransactionDateTime.ToString("dd-MMM-yyyy HH:mm"), DebitAmount = "", Description = Description });
                                }
                            }
                        }
                        if (bankstatementlist.Count > 0)
                        {
                            ViewBag.UserName = bankstatementlist[0].UserName.ToString();
                            ViewBag.AccountNumber = bankstatementlist[0].AcNumber.ToString();
                            ViewBag.CurrentAmount = AccountHolderAmount.ToString("N2");
                        }
                        return View(bankstatementlist);
                    }
                    else
                    {
                        ModelState.AddModelError("InvalidMessage", "Invalid User Name");
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult About()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        } 

        public List<TransactionModedetails> TransactionModeList()
        {
            List<TransactionModedetails> modeList = new List<TransactionModedetails>();

            modeList.Add(new TransactionModedetails { TransModeId = 1, TransModeName = "Deposit" });
            modeList.Add(new TransactionModedetails { TransModeId = 2, TransModeName = "WithDraw" });
            modeList.Add(new TransactionModedetails { TransModeId = 3, TransModeName = "Transfer" });
            return modeList;
        }

        public long GetRandomNumber(int min, int max)
        {
            return Ran_Number.Next(min, max);
        }

        public Boolean CheckUserNameStatus(string User)
        {
            Boolean ReturnStatus = false;
            var UserState = db.bank_account_list.Where(U => U.UserName == User).SingleOrDefault();

            if (UserState == null)
            { ReturnStatus = true; }
            return ReturnStatus;
        }

        public decimal UserCurrentBalance(string UserName)
        {
            List<TransactionDetails> TransList = db.transaction_details_list.Where(T => T.UserName == UserName).ToList();
            AccountHolderAmount = TransList.Sum(t => t.Amount);
            return AccountHolderAmount;
        }
        public JsonResult Autocomplete(string term)
        {
            var BankUserList = (from bank_ac in db.bank_account_list
                                where bank_ac.UserName.ToLower().Contains(term.ToLower())
                                select new { bank_ac.UserName, bank_ac.AcNumber }).ToList();

            return Json(BankUserList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

    }
}
