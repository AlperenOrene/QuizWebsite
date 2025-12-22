using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SoruDeneme.Filters
{
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public RequireRoleAttribute(string role)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(role) || userId == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            if (role != _role)
            {
                // İstersen 403 yerine role göre anasayfaya atarız:
                if (role == "Egitmen")
                    context.Result = new RedirectToActionResult("EgitmenHome", "Home", null);
                else
                    context.Result = new RedirectToActionResult("OgrenciHome", "Home", null);

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
