namespace Microsoft.Azure.TypeEdge.Modules.Messages
{
    public class Reference<T> : EdgeMessage
        where T : IEdgeMessage
    {
        public int ReferenceCount { get; set; }

        public string FileName { get; set; }

        public T Message { get; set; }
    }
}
