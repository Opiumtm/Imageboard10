using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Isam.Esent.Interop;

namespace Imageboard10.Core.ModelStorage
{
    /// <summary>
    /// Определение индекса.
    /// </summary>
    public struct IndexDefinition
    {        
        /// <summary>
        /// Поля (со знаком + или - для сортировки).
        /// </summary>
        public string[] Fields;

        /// <summary>
        /// Описание.
        /// </summary>
        public CreateIndexGrbit Grbit;

        /// <summary>
        /// Описание индекса.
        /// </summary>
        /// <returns>Строка описания индекса.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string IndexDef()
        {
            var sb = new StringBuilder();
            foreach (var f in Fields)
            {
                sb.Append(f);
                sb.Append('\0');
            }
            sb.Append('\0');
            return sb.ToString();
        }
    }
}