using WTA.Application.Monitor.Entities;
using WTA.Shared.Data;
using WTA.Shared.EventBus;
using WTA.Shared.Job;
using WTA.Shared.Mappers;
using WTA.Shared.SignalR;

namespace WTA.Application.Monitor;

public class UserLoginSrevice : IEventHander<SignalRConnectedEvent>,
    IEventHander<SignalRDisconnectedEvent>,
    IEventHander<SignalRHeartbeatEvent>,
    IEventHander<SignalCommandREvent>,
    IJobService
{
    private readonly IRepository<UserLogin> _repository;

    public UserLoginSrevice(IRepository<UserLogin> repository)
    {
        this._repository = repository;
    }

    public Task Handle(SignalRConnectedEvent data)
    {
        var entity = new UserLogin().FromModel(data);
        entity.IsOnline = true;
        this._repository.Insert(entity);
        this._repository.SaveChanges();
        return Task.CompletedTask;
    }

    public Task Handle(SignalRDisconnectedEvent data)
    {
        var entity = this._repository.Queryable().FirstOrDefault(o => o.ConnectionId == data.ConnectionId);
        if (entity != null)
        {
            entity.FromObject(data);
            entity.IsOnline = false;
            this._repository.SaveChanges();
        }
        return Task.CompletedTask;
    }

    public Task Handle(SignalRHeartbeatEvent data)
    {
        this._repository.Update(o => o.SetProperty(c => c.Heartbeat, DateTime.UtcNow), o => o.ConnectionId == data.ConnectionId);
        return Task.CompletedTask;
    }

    public Task Handle(SignalCommandREvent data)
    {
        return Task.CompletedTask;
    }

    public void Invoke()
    {
        var time = DateTime.UtcNow.AddMinutes(-1);
        this._repository.Update(o => o.SetProperty(c => c.IsOnline, false).SetProperty(c => c.Logout, DateTime.UtcNow), o => o.Heartbeat == null || o.Heartbeat < time);
    }
}
