using Orleans.Runtime;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class MartenOptionsValidator : IConfigurationValidator
{
    private readonly MartenOptions _options;
    private readonly string _name;

    public MartenOptionsValidator(MartenOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (_options.StoreOptions is null)
        {
            throw new OrleansConfigurationException(
                $"Configuration for Marten event storage provider {_name} is invalid. {nameof(_options.StoreOptions)} must be configured."
            );
        }
    }
}