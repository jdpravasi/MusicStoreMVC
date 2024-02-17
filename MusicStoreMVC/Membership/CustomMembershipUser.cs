using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using MusicStoreMVC.Models;

namespace MusicStoreMVC.Membership
{
    public class CustomMembershipUser : MembershipUser
    {
        #region User ni property 
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Role> Roles { get; set; }
        #endregion
        public CustomMembershipUser(User user) : base("CustomMembership",user.Username,user.UserId,user.Email,string.Empty,string.Empty,true,false,DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now)
        {
            UserId = user.UserId;
            FirstName = user.Firstname;
            LastName = user.Lastname;
            Roles = user.Roles;
        }
    }
}