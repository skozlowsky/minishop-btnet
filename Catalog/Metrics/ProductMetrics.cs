using System.Diagnostics.Metrics;

namespace Catalog.Metrics;

public class ProductMetrics
{
    private readonly Counter<int> _productDisplayedCounter;

    public ProductMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Catalog.Metrics");
        _productDisplayedCounter = meter.CreateCounter<int>("minishop.product.displayed");
    }

    public void ProductDisplayed(string productId)
    {
        _productDisplayedCounter.Add(
            1,
            KeyValuePair.Create<string, object?>("minishop.product.name", productId));
    }
}
