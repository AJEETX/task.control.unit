//namespace risk.control.system.Helpers
//{
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method |
//         AttributeTargets.Assembly, AllowMultiple = true)]
//    public class BreadcrumbAttribute : Attribute
//    {
//        public BreadcrumbAttribute(string label,
//                                   string controller = "",
//                                   string action = "",
//                                   bool passArgs = false)
//        {
//            Label = label;
//            ControllerName = controller;
//            ActionName = action;
//            PassArguments = passArgs;
//        }

//        public BreadcrumbAttribute(Type resourceType,
//                                   string resourceName,
//                                   string controller = "",
//                                   string action = "",
//                                   bool passArgs = false)
//        {
//            Label = ResourceHelper.GetResourceLookup(resourceType, resourceName);
//            ControllerName = controller;
//            ActionName = action;
//            PassArguments = passArgs;
//        }

//        public string Label { get; }
//        public string ControllerName { get; }
//        public string ActionName { get; }
//        public bool PassArguments { get; }
//    }
//}