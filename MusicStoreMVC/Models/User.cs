using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicStoreMVC.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive{ get; set; }
        public Guid ActivationCode{ get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}