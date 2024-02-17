using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Web.Security;
using MusicStoreMVC.Membership;
using MusicStoreMVC.Models;
using System.Web.UI.WebControls;
using System.Data.Entity;

namespace MusicStoreMVC.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        // GET: Registration
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        // POST: Registrtion
        [HttpPost]
        public ActionResult Registration(RegistrationView registrationView)
        {
            bool statusRegistration = false;
            string messageRegistration = string.Empty;

            if (ModelState.IsValid)
            {
                // EMAIL Verification
                string userName = System.Web.Security.Membership.GetUserNameByEmail(registrationView.Email);
                if (!string.IsNullOrEmpty(userName))
                {
                    ModelState.AddModelError("Warning Email", "Sorry: Email already Exists");
                    return View(registrationView);                      
                }

                // Save User Data
                using(MusicStoreEntities dbContext = new MusicStoreEntities())
                {
                    var user = new User 
                    {
                        Username = registrationView.Username,
                        Firstname = registrationView.FirstName,
                        Lastname = registrationView.LastName,
                        Email = registrationView.Email,
                        Password = registrationView.Password,
                        ActivationCode = Guid.NewGuid()
                    };
                    
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();

                    // Verification Email
                    VerificationEmail(registrationView.Email,user.ActivationCode.ToString());
                    messageRegistration = "Account Created SuccessFully !!!";
                    statusRegistration = true;
                }
                // for part 9
                if (statusRegistration == true)
                {
                    MigrateShoppingCart(registrationView.Username);
                    FormsAuthentication.SetAuthCookie(registrationView.Username, false);
                }
            }
            else
            {
                messageRegistration = "Something Wrong";
            }
            ViewBag.Message  = messageRegistration;
            ViewBag.Status = statusRegistration;
            return View(registrationView);
        }

        // EMAIL - Verification - Method
        [NonAction]
        public void VerificationEmail(string email, string activationCode)
        {
            var url = string.Format("/Account/ActivationAccount/{0}", activationCode);
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery,url);

            var fromEmail = new MailAddress("hereissilly@gmail.com", "Activation Account - Music Store");
            var toEmail = new MailAddress(email);

            var fromEmailPassword = "vmvc aqbr rhes wyfu";
            string subject = "Account Activation!";
            string body = "<br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'> Activation Account ! </a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address ,fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            })
            smtp.Send(message);
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login(string ReturnUrl = "")
        {
            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(MusicStoreMVC.Models.LoginView loginView, string ReturnUrl ="")
        /*public ActionResult Login(User model)*/
        
        {
            CustomMembership customMembership = new CustomMembership();
            if (ModelState.IsValid)
            {
                if (customMembership.ValidateUser(loginView.UserName, loginView.Password))
                {
                    // next line is from part 9 
                    MigrateShoppingCart(loginView.UserName); // this line
                    var user = (CustomMembershipUser)System.Web.Security.Membership.GetUser(loginView.UserName, false);
                    if (user != null)
                    {
                        CustomSerializeModel userModel = new Models.CustomSerializeModel()
                        {
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            RoleName = user.Roles.Select(r => r.RoleName).ToList()
                        };
                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                            1,loginView.UserName,DateTime.Now,DateTime.Now.AddMinutes(15),false,userData
                            );
                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                        Response.Cookies.Add(faCookie); 
                    }
                    if(Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index","Store");
                    }
                }
            }
            // Working Part ONE starts
            /*   bool isValidUser;
               if (ModelState.IsValid)
               {
                   using (MusicStoreEntities dbContext = new MusicStoreEntities())
                   {
                       var user = (from us in dbContext.Users
                                   where string.Compare(loginView.UserName, us.Username, StringComparison.OrdinalIgnoreCase) == 0
                                   && string.Compare(loginView.Password, us.Password, StringComparison.OrdinalIgnoreCase) == 0
                                   && us.IsActive == true
                                   select us).FirstOrDefault();
                       isValidUser = (user != null) ? true : false;
                   }
                   if (isValidUser)
                   {
                       // next line is from part 9 
                       MigrateShoppingCart(loginView.UserName);
                       //
                       CustomMembershipUser selectedUser;
                       using (MusicStoreEntities dbContext = new MusicStoreEntities())
                       {
                           var dbuser = (from us in dbContext.Users
                                       where string.Compare(loginView.UserName, us.Username, StringComparison.OrdinalIgnoreCase) == 0
                                       select us).FirstOrDefault();

                           if (dbuser == null)
                           {
                               return null;
                           }
                           selectedUser = new CustomMembershipUser(dbuser);
                       }
                       if (selectedUser != null)
                       {
                           CustomSerializeModel userModel = new CustomSerializeModel()
                           {
                               UserId = selectedUser.UserId,
                               FirstName = selectedUser.FirstName,
                               LastName = selectedUser.LastName,
                               RoleName = selectedUser.Roles.Select(r => r.RoleName).ToList()
                           };

                           string userData = JsonConvert.SerializeObject(userModel);
                           FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                               (
                                 1, loginView.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                               );

                           string enTicket = FormsAuthentication.Encrypt(authTicket);
                           HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                           Response.Cookies.Add(faCookie);
                       }

                       ReturnUrl = "/Store/Index/";
                       if (Url.IsLocalUrl(ReturnUrl))
                       {
                           return Redirect(ReturnUrl);
                       }

                       else
                       {
                            return RedirectToAction("Index");
                       }
                   }   
               }

               ModelState.AddModelError("", "Something Wrong : Username or Password invalid ^_^ ");
               return View(loginView);
   */
            //WORKING part one ENDS

            /*MusicStoreEntities storeDb = new MusicStoreEntities();
            bool isValid = storeDb.Users.Any(x => x.Username == model.Username && x.Password == model.Password);
            if (isValid)
            {
                FormsAuthentication.SetAuthCookie(model.Username, true,"Cookie1");
                return RedirectToAction("Index", "StoreManager");
            }
            ModelState.AddModelError("", "Invalid username and password");
            return View();*/
            
            ModelState.AddModelError("", "Something wrong: username or pwd incorrect ~_~");
            return View(loginView);
        }

        // GET: Logout
        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }

        // GET
        // Activation Account
        [HttpGet]
        public ActionResult ActivationAccount(string id)
        {
            bool statusAccount = false;
            using(MusicStoreEntities dbContext = new MusicStoreEntities()) 
            {
                var userAccount = dbContext.Users.Where(u => u.ActivationCode.ToString().Equals(id)).FirstOrDefault();

                if (userAccount != null) 
                {
                    userAccount.IsActive = true;
                    dbContext.SaveChanges();
                    statusAccount = true;
                }
                else
                {
                    ViewBag.Message = "Something Wrong";
                }
            }
            ViewBag.Status = statusAccount;
            return View();
        }

        // added from part 9 for checkout
        private void MigrateShoppingCart(string UserName)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;
        }
        // 
    }
}