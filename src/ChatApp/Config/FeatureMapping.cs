using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace ChatApp.Config
{
    public class FeatureConvention : IControllerModelConvention, IViewLocationExpander
    {
        public void Apply(ControllerModel controller)
        {
            controller.Properties.Add("feature", GetFeatureName(controller.ControllerType));
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var controllerActionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            string featureName = controllerActionDescriptor.Properties["feature"] as string;
            foreach (var location in viewLocations)
            {
                yield return location.Replace("{3}", featureName);
            }
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        private string GetFeatureName(TypeInfo controllerType)
        {
            string[] tokens = controllerType.FullName.Split('.');
            if (!tokens.Any(t => t == "Features")) return "";
            string featureName = tokens
              .SkipWhile(t => !t.Equals("features",
                StringComparison.CurrentCultureIgnoreCase))
              .Skip(1)
              .Take(1)
              .FirstOrDefault();
            return featureName;
        }
    }
}
