using Microsoft.AspNetCore.Localization;

namespace dotnetDemo.Components.I18n;

public static class IServiceCollectionLocalizationExtensions
{
    public static IServiceCollection AddSupportedCultures(
        this IServiceCollection services,
        string defaultCulture,
        string[] supportedCultures
    )
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(culture: defaultCulture, uiCulture: defaultCulture);
            options.SetDefaultCulture(defaultCulture);
            options.AddSupportedCultures(supportedCultures);
            options.AddSupportedUICultures(supportedCultures);
        });

        return services;
    }
}
