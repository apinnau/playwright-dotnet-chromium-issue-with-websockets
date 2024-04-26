using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace dotnetDemo.Components.I18n;

public class MyStringLocalizer<TComponent>(IOptions<LocalizationOptions> localizationOptions)
    : IStringLocalizer<TComponent>
{
    private readonly IOptions<LocalizationOptions> _localizationOptions = localizationOptions;

    public LocalizedString this[string name] => FindLocalziedString(name);
    public LocalizedString this[string name, params object[] arguments] => FindLocalziedString(name, arguments);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var resourceManager = CreateResourceManager();
        var result = new List<LocalizedString>();

        try
        {
            var resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            if (resourceSet != null)
            {
                result = resourceSet
                    .Cast<DictionaryEntry>()
                    .Select(item => new LocalizedString(
                        (string)item.Key,
                        (string)item.Value,
                        false,
                        GetResourceLocation()
                    ))
                    .ToList();
            }
        }
        catch
        {
            result.Add(new("", "", true, GetResourceLocation()));
        }

        return result;
    }

    private LocalizedString FindLocalziedString(string key, object[]? arguments = default)
    {
        var resourceManager = CreateResourceManager();
        LocalizedString result;

        try
        {
            string value = resourceManager.GetString(key, CultureInfo.CurrentCulture);

            if (arguments is not null)
            {
                value = string.Format(value, arguments);
            }

            result = new(key, value, false, GetResourceLocation());
        }
        catch
        {
            result = new(key, "", true, GetResourceLocation());
        }

        return result;
    }

    private ResourceManager CreateResourceManager()
    {
        string resourceLocaltion = GetResourceLocation();
        var resourceManager = new ResourceManager(resourceLocaltion, Assembly.GetExecutingAssembly());

        return resourceManager;
    }

    private string GetResourceLocation()
    {
        var componentType = typeof(TComponent);
        var nameParts = componentType.FullName.Split('.').ToList();
        nameParts.Insert(1, _localizationOptions.Value.ResourcesPath);
        string resourceLocation = string.Join(".", nameParts);

        return resourceLocation;
    }
}
