using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp1.Models
{

    public class ViewableLink
	{
	    public int Id { get; set; }
	    public virtual ICollection<User> Viewable { get; set; }
    }
}