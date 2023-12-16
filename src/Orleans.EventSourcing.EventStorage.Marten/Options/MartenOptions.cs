using Marten;
using Marten.Events;

// ReSharper disable once CheckNamespace
namespace Orleans.Configuration;

public class MartenOptions
{
    /// <summary>
    /// Delegate to configure Marten <see cref="StoreOptions"/>
    /// </summary>
    public Action<StoreOptions> StoreOptions { get; set; } = null!;
    
    /// <summary>
    /// Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;
    
    /// <summary>
    /// The delegate used to create a Marten <see cref="DocumentStore"/>.
    /// </summary>
    public Func<Action<StoreOptions>, Task<DocumentStore>> CreateDocumentStore { get; set; } = DefaultCreateDocumentStore;

    /// <summary>
    /// The default DocumentStore creation delegate
    /// </summary>
    /// <param name="configureOptions">DocumentStore configureOptions</param>
    /// <returns>The <see cref="DocumentStore"/></returns>
    public static Task<DocumentStore> DefaultCreateDocumentStore(Action<StoreOptions> configureOptions)
    {
        var storeOptions = new StoreOptions
        {
            Events =
            {
                StreamIdentity = StreamIdentity.AsString
            }
        };

        configureOptions(storeOptions);
        
        return Task.FromResult(new DocumentStore(storeOptions));
    }
}