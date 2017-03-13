using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace Minimum.MVC
{
    public class FormIdentity : IPrincipal
    {        
        [JsonIgnore] public IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        private string _name;
        public string Name { get { return _name; } set { _name = value; Identity = new GenericIdentity(_name); } }
        public int UserID { get; set; }
        public string UserData { get; set; }
        public IList<string> Roles { get; set; }

        public FormIdentity()
        {
            Roles = new List<string>();
        }

        public bool SignIn()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName);
            if (cookie == null) { cookie = new HttpCookie(FormsAuthentication.FormsCookieName); HttpContext.Current.Response.Cookies.Add(cookie); }

            UserData = Serializer.JSON.Load(this);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(0, 
                Name,
                DateTime.Now,
                DateTime.Now.AddMonths(1),
                true,
                UserData,
                FormsAuthentication.FormsCookiePath
            );
            cookie.Value = FormsAuthentication.Encrypt(ticket);

            HttpContext.Current.Response.Cookies.Set(cookie);
            HttpContext.Current.User = this;
            
            return true;
        }

        public bool SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);

            return true;
        }
    }
    
    public class FormIdentity<T> : FormIdentity, IPrincipal
    {
        public T Data { get; set; }
    }
}