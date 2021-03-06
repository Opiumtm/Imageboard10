using System.IO;
using Imageboard10.ModuleInterface;

namespace Imageboard10.Core.ModelStorage.Blobs
{
    /// <summary>
    /// ����� ��� ������ �� �����.
    /// </summary>
    internal sealed class InlineFileStream : InlineBlobStreamBase
    {
        /// <summary>
        /// �����������.
        /// </summary>
        /// <param name="globalErrorHandler">���������� ���������� ������.</param>
        /// <param name="inlinedStream">������.</param>
        public InlineFileStream(IGlobalErrorHandler globalErrorHandler, Stream inlinedStream) : base(globalErrorHandler, inlinedStream)
        {
        }
    }
}