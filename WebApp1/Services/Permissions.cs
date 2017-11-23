using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp1.Models;

namespace WebApp1.Services
{
	public static class Permissions
	{
	    public static bool HasPermission(List<string> validAccountAccessors)
	    {
	        string email;

            // Test if context is available
            try
	        {
                email = HttpContext.Current.Session["Email"].ToString();
	        }
            catch
            {
                return false;
            }
            
	        AccountType refAccType = User.GetAccountType(email);

            // Administrators have master access to all
	        if (refAccType == AccountType.Administrator) return true;

	        if (validAccountAccessors.Select(x => x.ToLower()).Contains(refAccType.ToString().ToLower()))
	        {
	            return true;
	        }

	        return false;
	    }
	}
}