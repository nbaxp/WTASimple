using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Attributes;

namespace WTA.Shared.EventBus;

[Implement<IEventPublisher>]
public class DefaultEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultEventPublisher(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public async Task Publish<T>(T data)
    {
        var subscribers = this._serviceProvider.GetServices<IEventHander<T>>().ToList();
        foreach (var item in subscribers)
        {
            try
            {
                await item.Handle(data).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"{typeof(T).Name}", ex);
            }
        }
    }
}
