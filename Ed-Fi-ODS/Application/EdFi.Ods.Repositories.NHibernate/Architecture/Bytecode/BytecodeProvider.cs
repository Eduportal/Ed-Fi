using Castle.Windsor;
using NHibernate.Bytecode;
using NHibernate.Bytecode.Lightweight;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode
{
    /// <summary>
    /// The BytecodeProvider class is a replacement for the built-in NHibernate BytecodeProvider.
    /// It allows NHibernate classes to participate in the dependency injection chain.
    /// 
    /// This class is registered before "NHibernate.Cfg.Configuration().Configure()" is called.
    ///     var container = new WindsorContainer();
    ///     Environment.BytecodeProvider = new BytecodeProvider(container);
    /// 
    /// For a custom connection provider, registered it in the normal manner, but it will use the Windsor DI chain.
    /// <property name="connection.provider">NHibernateCastle.MyCustomConnectionProvider, NHibernateCastle</property>
    /// </summary>
    public class BytecodeProvider : BytecodeProviderImpl
    {
        private readonly IWindsorContainer _container;

        public BytecodeProvider(IWindsorContainer container)
        {
            _container = container;
        }

        public override IObjectsFactory ObjectsFactory
        {
            get
            {
                return new ActivatorObjectsFactory(_container);
            }
        }
    }
}
