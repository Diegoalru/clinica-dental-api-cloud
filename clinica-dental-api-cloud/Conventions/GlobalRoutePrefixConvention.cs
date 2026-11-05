using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace clinica_dental_api_cloud.Conventions;

/// <summary>
/// Prepends a route prefix (e.g. "api/v1") to every controller.
/// A controller with [Route("[controller]")] ends up at /api/v1/{controller}.
/// </summary>
public class GlobalRoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
        {
            selector.AttributeRouteModel = selector.AttributeRouteModel is null
                ? _prefix
                : AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
        }
    }
}
