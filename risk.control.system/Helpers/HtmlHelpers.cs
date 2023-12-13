//using System.Reflection;

//using Microsoft.AspNetCore.Html;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc.Routing;

//using risk.control.system.Models.ViewModel;

//namespace risk.control.system.Helpers
//{
//    public static class HtmlHelpers
//    {
//        public static string IsSelected(this IHtmlHelper html, string controller = null, string action = null, string cssClass = null)
//        {
//            if (String.IsNullOrEmpty(cssClass))
//                cssClass = "active";

//            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
//            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

//            if (String.IsNullOrEmpty(controller))
//                controller = currentController;

//            if (String.IsNullOrEmpty(action))
//                action = currentAction;

//            bool result = (controller == currentController && action == currentAction);

//            return result ?
//                cssClass : String.Empty;
//        }

//        public static string IsSelected(this IHtmlHelper html, string actions = null)
//        {
//            var cssClass = "active";

//            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
//            string[] actionarr = !String.IsNullOrEmpty(actions) ? actions.Split(',') : new string[] { };

//            bool result = (actionarr.Contains<string>(currentAction));

//            return result ?
//                cssClass : String.Empty;
//        }

//        public static string PageClass(this IHtmlHelper htmlHelper)
//        {
//            string currentAction = (string)htmlHelper.ViewContext.RouteData.Values["action"];
//            return currentAction;
//        }
//        public static string AbsoluteAction(
//        this IUrlHelper url,
//        string actionName,
//        string controllerName,
//        object routeValues = null)
//        {
//            if (url != null)
//                return url.Action(actionName, controllerName, routeValues,
//                           url.ActionContext.HttpContext.Request.Scheme);
//            else
//                return string.Empty;
//        }

//        public static IHtmlContent BuildBreadcrumbNavigation(this IHtmlHelper helper,
//                                                List<BreadcrumbItem> breadcrumbExtras)
//        {
//            IHtmlContentBuilder rtn = null;

//            string controllerName = helper.ViewContext.RouteData.Values["controller"].ToString();
//            string actionName = helper.ViewContext.RouteData.Values["action"].ToString();
//            string query = helper.ViewContext.HttpContext.Request.QueryString.Value;

//            var type = typeof(Microsoft.AspNetCore.Mvc.Controller);
//            var controllersTypes = AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(s => s.GetTypes())
//                .Where(p => p.IsSubclassOf(type))
//                .ToList();

//            var urlHelperFactory = helper.ViewContext.HttpContext
//                       .RequestServices.GetRequiredService<IUrlHelperFactory>();
//            var urlHelper = urlHelperFactory.GetUrlHelper(helper.ViewContext);
//            var homeLink = urlHelper.Action("Index", "Home");
//            var breadcrumb = new HtmlContentBuilder()
//                       .AppendHtml("<ol class='breadcrumb float-sm-right'><li class='breadcrumb-item'>")
//                       .AppendHtml($"<a href='{homeLink}'><i class='fas fa-home'></i></a>")
//                       .AppendHtml("</li>");

//            var controllerType = controllersTypes
//                                     .FirstOrDefault(x => x.Name == $"{controllerName}Controller");
//            if (controllerType != null)
//            {
//                var breadcrumbControllerAttributes = controllerType
//                                    .GetCustomAttributes<BreadcrumbAttribute>();
//                if (breadcrumbControllerAttributes != null && breadcrumbControllerAttributes.Any())
//                {
//                    foreach (var bs in breadcrumbControllerAttributes)
//                    {
//                        breadcrumb.AppendHtml(BuildBreadcrumb(bs, urlHelper, query));
//                    }
//                }

//                if (breadcrumbExtras != null && breadcrumbExtras.Any())
//                {
//                    foreach (var be in breadcrumbExtras)
//                    {
//                        breadcrumb.AppendHtml(BuildBreadcrumb(be));
//                    }
//                }
//                else
//                {
//                    var actionProp = controllerType.GetTypeInfo().GetMethods()
//                        .FirstOrDefault(x => x.Name == actionName &&
//                                x.CustomAttributes.Any(y => y.AttributeType == typeof(BreadcrumbAttribute)));
//                    var breadcrumbMethodsAttributes = actionProp?.GetCustomAttributes<BreadcrumbAttribute>();

//                    if (breadcrumbMethodsAttributes != null && breadcrumbMethodsAttributes.Any())
//                    {
//                        foreach (var bs in breadcrumbMethodsAttributes)
//                        {
//                            breadcrumb.AppendHtml(BuildBreadcrumb(bs, urlHelper, query));
//                        }
//                    }
//                }
//            }
//            rtn = breadcrumb.AppendHtml("</ol>");

//            return rtn;
//        }
//        private static string BuildBreadcrumb(BreadcrumbItem bs)
//        {
//            string link = "<li class='breadcrumb-item'>";

//            if (!string.IsNullOrEmpty(bs.URL))
//                link += $"<a href='{bs.URL}'>{bs.Label}</a>";
//            else
//                link += bs.Label;

//            link += "</li>";

//            return link;
//        }
//        private static string BuildBreadcrumb(BreadcrumbAttribute bs,
//                                      IUrlHelper urlHelper,
//                                      string parameters)
//        {
//            string link = "<li class='breadcrumb-item'>";

//            if (!string.IsNullOrEmpty(bs.ActionName) && !string.IsNullOrEmpty(bs.ControllerName))
//                link += $"<a href='{urlHelper.AbsoluteAction(bs.ActionName, bs.ControllerName)}" +
//                        $"{(bs.PassArguments ? parameters : string.Empty)}'>{bs.Label}</a>";
//            else
//                link += bs.Label;

//            link += "</li>";

//            return link;
//        }
//    }
//}