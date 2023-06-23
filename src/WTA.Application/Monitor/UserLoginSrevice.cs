using WTA.Application.Monitor.Entities;
using WTA.Shared.Data;
using WTA.Shared.EventBus;
using WTA.Shared.Mappers;
using WTA.Shared.SignalR;

namespace WTA.Application.Monitor;

public class UserLoginSrevice : IEventHander<SignalRConnectedEvent>, IEventHander<SignalRDisconnectedEvent>, IEventHander<SignalCommandREvent>
{
    private readonly IRepository<UserLogin> repository;

    public UserLoginSrevice(IRepository<UserLogin> repository)
    {
        this.repository = repository;
    }

    public Task Handle(SignalRConnectedEvent data)
    {
        var entity = new UserLogin().FromModel(data);
        entity.IsOnline = true;
        this.repository.Insert(entity);
        this.repository.SaveChanges();
        return Task.CompletedTask;
    }

    public Task Handle(SignalRDisconnectedEvent data)
    {
        var entity = this.repository.Queryable().FirstOrDefault(o => o.ConnectionId == data.ConnectionId);
        if (entity != null)
        {
            entity.FromObject(data);
            entity.IsOnline = false;
            this.repository.SaveChanges();
        }
        return Task.CompletedTask;
    }

    public Task Handle(SignalCommandREvent data)
    {
        return Task.CompletedTask;
    }
}
