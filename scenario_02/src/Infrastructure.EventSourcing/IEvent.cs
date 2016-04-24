namespace Infrastructure.EventSourcing
{
    public interface IEvent<out TSourceId>
    {
        TSourceId SourceId { get; }
    }
}