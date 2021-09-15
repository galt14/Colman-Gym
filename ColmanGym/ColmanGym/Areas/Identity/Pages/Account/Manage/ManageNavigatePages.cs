using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColmanGym.Areas.Identity.Pages.Account.Manage
{
    public class ManageNavigatePages
    {
        public static string IndexNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "Index");

        public static string EmailNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "Email");

        public static string ChangePasswordNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "ChangePassword");

        public static string DownloadPersonalDataNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "DownloadPersonalData");

        public static string DeletePersonalDataNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "DeletePersonalData");

        public static string ExternalLoginsNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "ExternalLogins");

        public static string PersonalDataNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "PersonalData");

        public static string TwoFactorAuthenticationNavigateClass(ViewContext viewContext) => PageNavigateClass(viewContext, "TwoFactorAuthentication");

        private static string PageNavigateClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
