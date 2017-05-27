using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imageboard10.Core;
using Imageboard10.Core.Modules;
using Imageboard10.ModuleInterface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IModule = Imageboard10.Core.Modules.IModule;
using IModuleLifetime = Imageboard10.Core.Modules.IModuleLifetime;
using IModuleProvider = Imageboard10.Core.Modules.IModuleProvider;

namespace Imageboard10UnitTests
{
    [TestClass]
    public class ModuleLifetimeTests
    {
        [TestMethod]
        [TestCategory("ModuleLifetime")]
        public async Task TestDispose()
        {
            var collection = new ModuleCollection();
            var modules = new SuspensionAwareModule[]
            {
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
            };
            foreach (var m in modules)
            {
                var lt = m.QueryView<IModuleLifetime>();
                Assert.IsNotNull(lt, "Модуль должен поддерживать управление временем жизни");
                Assert.IsFalse(lt.IsSuspendAware, "Модуль не должен поддерживать приостановку работы");
            }
            foreach (var m in modules)
            {
                collection.RegisterModule<SuspensionAwareModule, ISuspensionAwareModule>(m);
            }
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsInitialized, "Модуль не должен быть инициализирован до Seal()");
                Assert.IsFalse(m.IsAllInitialized, "Не должен быть вызван метод AllInitialized() до Seal()");
            }
            await collection.Seal();
            foreach (var m in modules)
            {
                Assert.IsTrue(m.IsInitialized, "Модуль должен быть инициализирован после Seal()");
                Assert.IsTrue(m.IsAllInitialized, "Должен быть вызван метод AllInitialized() после Seal()");
                Assert.AreEqual(1, m.AllInitializedCount, "Метод AllInitialized() должен быть вызван один раз");
            }
            await collection.Dispose();
            foreach (var m in modules)
            {
                Assert.IsTrue(m.IsDisposed, "Модуль должен быть завершён");
            }
        }

        [TestMethod]
        [TestCategory("ModuleLifetime")]
        public async Task TestSuspend()
        {
            var collection = new ModuleCollection();
            var modules = new SuspensionAwareModule[]
            {
                new SuspensionAwareModule(true),
                new SuspensionAwareModule(true),
                new SuspensionAwareModule(true),
                new SuspensionAwareModule(true),
            };
            foreach (var m in modules)
            {
                var lt = m.QueryView<IModuleLifetime>();
                Assert.IsNotNull(lt, "Модуль должен поддерживать управление временем жизни");
                Assert.IsTrue(lt.IsSuspendAware, "Модуль должен поддерживать приостановку работы");
            }
            foreach (var m in modules)
            {
                collection.RegisterModule<SuspensionAwareModule, ISuspensionAwareModule>(m);
            }
            await collection.Seal();
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен");
            }
            await collection.Suspend();
            foreach (var m in modules)
            {
                Assert.IsTrue(m.IsSuspended, "Модуль должен быть приостановлен");
            }
            await collection.Resume();
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен после Resume");
                Assert.IsTrue(m.IsAllResumed, "Должен быть вызван метод AllResumed()");
                Assert.AreEqual(1, m.AllResumedCount, "Метод AllResumed() должен быть вызван один раз");
            }
            await collection.Dispose();
        }

        [TestMethod]
        [TestCategory("ModuleLifetime")]
        public async Task TestSuspendUnaware()
        {
            var collection = new ModuleCollection();
            var modules = new SuspensionAwareModule[]
            {
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
                new SuspensionAwareModule(false),
            };
            foreach (var m in modules)
            {
                var lt = m.QueryView<IModuleLifetime>();
                Assert.IsNotNull(lt, "Модуль должен поддерживать управление временем жизни");
                Assert.IsFalse(lt.IsSuspendAware, "Модуль не должен поддерживать приостановку работы");
            }
            foreach (var m in modules)
            {
                collection.RegisterModule<SuspensionAwareModule, ISuspensionAwareModule>(m);
            }
            await collection.Seal();
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен");
            }
            await collection.Suspend();
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен после Suspend");
            }
            await collection.Resume();
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен после Resume");
                Assert.IsFalse(m.IsAllResumed, "Не должен быть вызван метод AllResumed()");
            }
            await collection.Dispose();
        }

        [TestMethod]
        [TestCategory("ModuleLifetime")]
        public async Task TestAttachedLifetime()
        {
            var collection = new ModuleCollection();
            await collection.Seal();
            var provider = collection.GetModuleProvider();

            var modules = new SuspensionAwareModule[]
            {
                new SuspensionAwareModule(true, true),
                new SuspensionAwareModule(true, true),
                new SuspensionAwareModule(true, true),
                new SuspensionAwareModule(true, true),
            };

            var suspendTasks = new List<Task>();
            var resumeTasks = new List<Task>();
            var disposeTasks = new List<Task>();

            foreach (var m in modules)
            {
                var lt = m.QueryView<IModuleLifetime>();
                var lte = m.QueryView<IModuleLifetimeEvents>();
                Assert.IsNotNull(lt, "Модуль должен поддерживать управление временем жизни");
                Assert.IsNotNull(lte, "Модуль должен поддерживать события времени жизни");
                await lt.InitializeModule(provider);
                var tcsd = new TaskCompletionSource<bool>();
                var tcss = new TaskCompletionSource<bool>();
                var tcsr = new TaskCompletionSource<bool>();
                var tcsar = new TaskCompletionSource<bool>();
                lte.Disposed += _ =>
                {
                    tcsd.SetResult(true);
                };
                lte.Suspended += _ =>
                {
                    tcss.SetResult(true);
                };
                lte.Resumed += _ =>
                {
                    tcsr.SetResult(true);
                };
                lte.AllResumed += _ =>
                {
                    tcsar.SetResult(true);
                };
                disposeTasks.Add(tcsd.Task);
                suspendTasks.Add(tcss.Task);
                resumeTasks.Add(tcsr.Task);
                resumeTasks.Add(tcsar.Task);
            }

            foreach (var m in modules)
            {
                var lt = m.QueryView<IModuleLifetime>();
                await lt.AllModulesInitialized();
            }

            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен");
            }

            await collection.Suspend();
            var suspendWaiter = new Task[]
            {
                Task.WhenAll(suspendTasks.ToArray()),
                Task.Delay(TimeSpan.FromSeconds(10))
            };
            var suspendResult = await Task.WhenAny(suspendWaiter);
            Assert.AreSame(suspendWaiter[0], suspendResult, "Сработал таймаут по событию Suspended");
            foreach (var m in modules)
            {
                Assert.IsTrue(m.IsSuspended, "Модуль должен быть приостановлен");
            }
            await collection.Resume();
            var resumeWaiter = new Task[]
            {
                Task.WhenAll(resumeTasks.ToArray()),
                Task.Delay(TimeSpan.FromSeconds(10))
            };
            var resumeResult = await Task.WhenAny(resumeWaiter);
            Assert.AreSame(resumeWaiter[0], resumeResult, "Сработал таймаут по событию Resumed");
            foreach (var m in modules)
            {
                Assert.IsFalse(m.IsSuspended, "Модуль не должен быть приостановлен после Resume");
                Assert.IsTrue(m.IsAllResumed, "Должен быть вызван метод AllResumed()");
                Assert.AreEqual(1, m.AllResumedCount, "Метод AllResumed() должен быть вызван один раз");
            }
            await collection.Dispose();
            var disposeWaiter = new Task[]
            {
                Task.WhenAll(disposeTasks.ToArray()),
                Task.Delay(TimeSpan.FromSeconds(10))
            };
            var disposeResult = await Task.WhenAny(disposeWaiter);
            Assert.AreSame(disposeWaiter[0], disposeResult, "Сработал таймаут по событию Disposed");
            foreach (var m in modules)
            {
                Assert.IsTrue(m.IsDisposed, "Модуль должен быть завершён");
            }
        }
    }


    public interface ISuspensionAwareModule : IModule
    {
        bool IsSuspended { get; }

        bool IsAllResumed { get; }

        bool IsAllInitialized { get; }

        bool IsInitialized { get; }

        bool IsDisposed { get; }

        int AllResumedCount { get; }

        int AllInitializedCount { get; }
    }

    public class SuspensionAwareModule : ModuleBase<ISuspensionAwareModule>, ISuspensionAwareModule
    {
        public SuspensionAwareModule(bool suspensionAware)
            :base(suspensionAware, false)
        {
            IsAllResumed = suspensionAware;
        }

        public SuspensionAwareModule(bool suspensionAware, bool attachEvents)
            : base(suspensionAware, attachEvents)
        {
            IsAllResumed = suspensionAware;
        }

        protected override async ValueTask<Nothing> OnSuspended()
        {
            await base.OnSuspended();
            IsSuspended = true;
            IsAllResumed = false;
            return Nothing.Value;
        }

        protected override async ValueTask<Nothing> OnResumed()
        {
            await base.OnResumed();
            IsSuspended = false;
            return Nothing.Value;
        }

        protected override async ValueTask<Nothing> OnAllResumed()
        {
            await base.OnAllResumed();
            IsAllResumed = true;
            AllResumedCount = AllResumedCount + 1;
            return Nothing.Value;
        }

        protected override async ValueTask<Nothing> OnInitialize(IModuleProvider moduleProvider)
        {
            await base.OnInitialize(moduleProvider);
            IsInitialized = true;
            return Nothing.Value;
        }

        protected override async ValueTask<Nothing> OnDispose()
        {
            await base.OnDispose();
            IsDisposed = true;
            return Nothing.Value;
        }

        protected override async ValueTask<Nothing> OnAllInitialized()
        {
            await base.OnAllInitialized();
            IsAllInitialized = true;
            AllInitializedCount = AllInitializedCount + 1;
            return Nothing.Value;
        }

        public bool IsSuspended { get; private set; }
        public bool IsAllResumed { get; private set; }
        public bool IsAllInitialized { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }
        public int AllResumedCount { get; private set; }
        public int AllInitializedCount { get; private set; }
    }
}