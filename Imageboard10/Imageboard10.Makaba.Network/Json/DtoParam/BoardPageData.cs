using System;
using Imageboard10.Core.Models.Links.LinkTypes;

namespace Imageboard10.Makaba.Network.Json
{
    /// <summary>
    /// ������ �� �������� �����.
    /// </summary>
    public struct BoardPageData
    {
        /// <summary>
        /// �������� Makaba.
        /// </summary>
        public BoardEntity2 Entity;

        /// <summary>
        /// ������.
        /// </summary>
        public BoardLink Link;

        /// <summary>
        /// ����� ��������.
        /// </summary>
        public DateTimeOffset LoadedTime;

        /// <summary>
        /// Etag.
        /// </summary>
        public string Etag;
    }
}