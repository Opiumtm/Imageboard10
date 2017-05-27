using System.Threading.Tasks;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Провайдер экземпляров ESENT.
    /// </summary>
    public interface IEsentInstanceProvider
    {
        /// <summary>
        /// Получить экземпляр.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        Task<IEsentInstance> GetInstance();
    }
}