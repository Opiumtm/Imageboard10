using System;
using System.Threading.Tasks;
using Imageboard10.Core.Modules;
using Imageboard10.ModuleInterface;
using Microsoft.Isam.Esent.Interop;
using IModuleProvider = Imageboard10.Core.Modules.IModuleProvider;

namespace Imageboard10.Core.Database
{
    /// <summary>
    /// Провайдер экземпляров ESENT.
    /// </summary>
    public class EsentInstanceProvider : ModuleBase<IEsentInstanceProvider>, IEsentInstanceProvider
    {

        /// <summary>
        /// Действие по инициализации.
        /// </summary>
        /// <param name="moduleProvider">Провайдер модулей.</param>
        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            return Nothing.Value;
        }

        /// <summary>
        /// Действие по завершению работы.
        /// </summary>
        protected override async ValueTask<Nothing> OnDispose()
        {
            await base.OnDispose();
            return Nothing.Value;
        }

        private int _activeConnections;

        private object _lock = new object();

        private bool _isSuspended;

        private void ReleaseConnection()
        {
        }

        private void CheckActiveInstanceState()
        {            
        }

        /// <summary>
        /// Получить экземпляр.
        /// </summary>
        /// <returns>Экземпляр.</returns>
        public Task<IEsentInstance> GetInstance()
        {
            throw new System.NotImplementedException();
        }

        private class EsentInstance : IEsentInstance
        {
            private readonly EsentInstanceProvider _parent;
            private readonly Instance _instance;

            public Instance Instance => _instance;

            public void Dispose()
            {
                _parent.ReleaseConnection();
            }
        }
    }
}