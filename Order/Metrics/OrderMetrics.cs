using System.Diagnostics.Metrics;

namespace Order.Metrics;

public class OrderMetrics
{
    private readonly Counter<int> _productSoldCounter;

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Order.Metrics");
        _productSoldCounter = meter.CreateCounter<int>("minishop.product.sold");
    }

    public void ProductsSold((string productId, int quantity)[] products)
    {
        foreach (var product in products)
        {
            _productSoldCounter.Add(
                product.quantity,
                KeyValuePair.Create<string, object?>("minishop.product.id", product.productId));
        }
    }
}
