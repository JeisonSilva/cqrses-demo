namespace Infrastructure.EventSourcing
{
    public class VersionedEvent<TSourceId> : IVersionedEvent<TSourceId>
    {
        public TSourceId SourceId { get; internal set; }
        public int Version { get; internal set; }
    }
}