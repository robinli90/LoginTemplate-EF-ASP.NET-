using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp1.Models;
using WebGrease.Css.Extensions;

namespace WebApp1.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            Models.User.DisplayUsers();
            return View();
        }

        [Route("Account/Registration/{paramOne}")]
        public ActionResult Registration(string paramOne)
        {
            // if UserManagement
            if (paramOne == null || paramOne.ToLower() == "newuser")
                return View("~/Views/Account/Registration.cshtml", new User());

            // return registration with current user
            return View(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()));
        }


        [HttpPost]
        [Route("Account/Login/CheckCredentials")]
        public ActionResult CheckCredentials(User model)
        {
            
            if (Models.User.Login(model.Email, model.Password))
            {
                // Create context for email to reference for authentication in login lifetime
                HttpContext.Session["Email"] = model.Email.ToLower();

                return Redirect("/");
            }

            return Redirect("/Account/Login/welcomePage");
        }

        [Route("Account/Logout/{paramOne}")]
        public ActionResult Logout(string paramOne)
        {
            HttpContext.Session.Abandon();
            return Redirect("/Account/Login/welcomePage");
        }

        [HttpPost]
        public ActionResult UserChangeAdd(User model)
        {
            bool sessionAvailable = HttpContext.Session != null && HttpContext.Session["Email"] != null;

            // If fields not complete or email exists
            if (!sessionAvailable && (model == null || 
                model.FirstName == null || 
                model.LastName == null || 
                model.Email == null || 
                model.Password == null ||
                    Models.User.DoesEmailExist(model.Email) &&                  // Check if email exists
                    !(model.Email == HttpContext.Session["Email"].ToString()    // Check if not same email as context
                )) 
            )
                return View("~/Views/Account/Registration.cshtml", model);

            // Delete old user entry in database
            if (sessionAvailable)
                Models.User.DeleteUser(HttpContext.Session["Email"].ToString());

            if (!Models.User.DoesEmailExist(model.Email))
            {
                // Create new one
                Models.User.CreateNewUser(model);
                // Set context to new email
                HttpContext.Session["Email"] = model.Email.ToLower();
            }

            return View("~/Views/Home/Index.cshtml", model);
        }
        
        [HttpPost]
        public ActionResult UploadImage()
        {
            string[] validFileExtensions = {".png", ".jpg", ".jpeg"};
            
            foreach (string file in Request.Files)
            {
                // If not within file extensions, ignore file
                if (!validFileExtensions.Any(x => file.Contains(x))) break;

                HttpPostedFileBase fileContent = Request.Files[file];

                Stream stream = fileContent.InputStream;

                // Copy image to stream
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    Models.User.SaveImage(Models.User.GetUserByEmail(HttpContext.Session["Email"].ToString()), memoryStream);
                }
            }

            return Redirect("/");
        }
        


        [HttpPost]
        [Route("Account/SaveViewables/{paramOne}")]
        public ActionResult SaveViewables(string paramOne)
        {
            //Models.User.DisplayUsers();

            List<string> viewableParam = paramOne.Split(new[] {"_"}, StringSplitOptions.None).ToList();

            if (viewableParam.Count > 1)
            {
                List<string> viewableList = viewableParam.GetRange(1, viewableParam.Count - 1);
                Models.User.SaveViewables(Models.User.GetUserById(viewableParam[0]), String.Join("_", viewableList));

            }
            return RedirectToAction("Users", "Home");
        }
    }
}