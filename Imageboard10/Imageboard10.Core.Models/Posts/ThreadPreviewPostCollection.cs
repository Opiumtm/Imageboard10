using System;
using Imageboard10.Core.ModelInterface.Posts;

namespace Imageboard10.Core.Models.Posts
{
    /// <summary>
    /// ������ ������.
    /// </summary>
    public class ThreadPreviewPostCollection : BoardPostCollection, IThreadPreviewPostCollection
    {
        /// <summary>
        /// ���������� �����������.
        /// </summary>
        public int? ImageCount { get; set; }

        /// <summary>
        /// ��������� �����������.
        /// </summary>
        public int? OmitImages { get; set; }

        /// <summary>
        /// ��������� ������.
        /// </summary>
        public int? Omit { get; set; }

        /// <summary>
        /// ���������� �������.
        /// </summary>
        public int? ReplyCount { get; set; }

        /// <summary>
        /// ������� �� ��������.
        /// </summary>
        public int OnPageSequence { get; set; }
    }
}