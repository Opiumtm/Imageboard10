namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// Идентификатор блоба.
    /// </summary>
    public struct BlobId
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id;

        public bool Equals(BlobId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlobId && Equals((BlobId) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}