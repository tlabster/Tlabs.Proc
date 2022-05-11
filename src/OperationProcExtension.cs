using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Tlabs.Proc.Common;

namespace Tlabs.Proc {

  ///<summary><see cref="IServiceCollection"/> extension class to aid in registering process types and automation procedures.</summary>
  public static class ProcessAutomationSvcExtension {

    ///<summary>Register automation procedure of <typeparamref name="TProc"/> described by <typeparamref name="TProcDesc"/>
    ///with message <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.
    ///</summary>
    ///<remarks>The automation procedure registration makes following types available with DI/ServiceProvider:
    ///<list type="table">
    ///<item>
    ///<term><see cref="IAutoProcedureDescriptor"/></term>
    ///<description>Used to obtain a list of all automation procedures per DI (with <c>IEnumerable&lt;IAutoProcedureDescriptor> procDesciptors</c>)
    ///e.g. by a <see cref="IProcessAutomation"/> implementation(s).</description>
    ///</item>
    ///<item>
    ///<term><typeparamref name="TProc"/></term>
    ///<description>Used to lookup a specific automation procedure implementation. </description>
    ///</item>
    ///</list>
    ///</remarks>
    public static IServiceCollection AddAutomationProcedure<TProc, TProcDesc, TMsg, TRes>(this IServiceCollection services) where TProc : class, IAutoProcedure<TMsg, TRes>
                                                                                                                            where TProcDesc : AutoProcedureDescriptor<TProc, TMsg, TRes>, IAutoProcedureDescriptor 
                                                                                                                            where TMsg : class
                                                                                                                            where TRes : notnull
      => AddAutomationProcedure<TProc, TProcDesc, TMsg, TRes>(services, typeof(IAutoProcedureDescriptor));

    ///<summary>Register a per default enabled and result returning automation procedure of <typeparamref name="TProc"/> described by <typeparamref name="TProcDesc"/>
    ///with message <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.
    ///</summary>
    ///<remarks>The automation procedure registration makes following types available for DI:
    ///<list type="table">
    ///<item>
    ///<term><see cref="IAutoProcedureDescriptor"/></term>
    ///<description>Used to obtain a list of all automation procedures per DI (with <c>IEnumerable&lt;IAutoProcedureDescriptor> procDesciptors</c>)
    ///e.g. by a <see cref="IProcessAutomation"/> implementation(s).</description>
    ///</item>
    ///<item>
    ///<term><typeparamref name="TProc"/></term>
    ///<description>Used to lookup a specific procedure implementation. </description>
    ///</item>
    ///</list>
    ///</remarks>
    public static IServiceCollection AddAutomationResultProcedure<TProc, TProcDesc, TMsg, TRes>(this IServiceCollection services) where TProc : class, IAutoProcedure<TMsg, TRes>
                                                                                                                                  where TProcDesc : AutoProcedureDescriptor<TProc, TMsg, TRes>, IAutoProcedureDescriptor
                                                                                                                                  where TMsg : class
                                                                                                                                  where TRes : notnull
      => AddAutomationProcedure<TProc, TProcDesc, TMsg, TRes>(services, typeof(IResultAutoProcedureType));

    ///<summary>Register a per default enabled automation procedure of <typeparamref name="TProc"/> described by <typeparamref name="TProcDesc"/> with message <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
    ///<remarks>The automation procedure registration makes following types available for DI:
    ///<list type="table">
    ///<item>
    ///<term><see cref="IAutoProcedureDescriptor"/></term>
    ///<description>Used to obtain a list of all automation procedures per DI (with <c>IEnumerable&lt;IAutoProcedureDescriptor> procDesciptors</c>)
    ///e.g. by a <see cref="IProcessAutomation"/> implementation(s).</description>
    ///</item>
    ///<item>
    ///<term><typeparamref name="TProc"/></term>
    ///<description>Used to lookup a specific procedure implementation. </description>
    ///</item>
    ///</list>
    ///</remarks>
    public static IServiceCollection AddAutomationDefaultProcedure<TProc, TProcDesc, TMsg, TRes>(this IServiceCollection services) where TProc : class, IAutoProcedure<TMsg, TRes>
                                                                                                                                   where TProcDesc : AutoProcedureDescriptor<TProc, TMsg, TRes>, IAutoProcedureDescriptor
                                                                                                                                   where TMsg : class
                                                                                                                                   where TRes : notnull 
      => AddAutomationProcedure<TProc, TProcDesc, TMsg, TRes>(services, typeof(IDefaultAutoProcedureType));

    ///<summary>Register automation process procedure of <typeparamref name="TProc"/> described by <typeparamref name="TProcDesc"/> with message <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
    public static IServiceCollection AddAutomationProcedure<TProc, TProcDesc, TMsg, TRes>(this IServiceCollection services,
                                                                                         Type procType) where TProc : class, IAutoProcedure<TMsg, TRes>
                                                                                                        where TProcDesc : AutoProcedureDescriptor<TProc, TMsg, TRes>, IAutoProcedureDescriptor
                                                                                                        where TMsg : class
                                                                                                        where TRes : notnull
    {
      if (!typeof(IAutoProcedure).IsAssignableFrom(procType)) throw new ArgumentException($"Invalid {nameof(procType)}: {procType?.Name}");
      var procDesc= typeof(TProcDesc);
      services.TryAddEnumerable(ServiceDescriptor.Singleton(procType, procDesc));
      services.TryAddEnumerable(ServiceDescriptor.Singleton<IAutoProcedureDescriptor<TMsg, TRes>, TProcDesc>());
      services.TryAddScoped<TProc>(); //could be added multiple times!
      return services;
    }

    ///<summary>Register auto. process type.</summary>
    ///<remarks>The auto. process type registration makes following types available for DI:
    ///<list type="table">
    ///<item>
    ///<term><see cref="IAutoProcessType"/></term>
    ///<description>Used to obtain a list of all automation process types per DI (with <c>IEnumerable&lt;ILoyaltyOpType> opTypes</c>)
    ///e.g. by a <see cref="IProcessAutomation"/> implementation(s).</description>
    ///</item>
    ///<item>
    ///<term><see cref="AutoProcessType{TMsg, TRes}"/></term>
    ///<description>Used to lookup a specific process type per DI with <see cref="AutoProcedureDescriptor{TProc, TMsg, TRes}"/> implementations. </description>
    ///</item>
    ///<item>
    ///<term><see cref="IAutoProcess{TMsg, TRes}"/></term>
    ///<description>Used by process automation consumers to invoke the auto. process functionality. </description>
    ///</item>
    ///</list>
    ///</remarks>
    public static IServiceCollection AddProcessType<TProcess, TMsg, TRes>(this IServiceCollection services) where TProcess : class, IAutoProcessType
                                                                                                            where TMsg : class
                                                                                                            where TRes : class
    {
      services.TryAddEnumerable(ServiceDescriptor.Singleton<IAutoProcessType, TProcess>());
      services.AddSingleton(typeof(AutoProcessType<TMsg, TRes>), typeof(TProcess));
      services.AddSingleton(typeof(IAutoProcess<TMsg, TRes>), typeof(Service.AutoProcessProxySvc<TMsg, TRes>));
      return services;
    }

    ///<summary>Register auto. process type for special sequle process.</summary>
    ///<remarks>This allows to define a auto. process suitable as an additional sequel of a anteceding <see cref="IAutoProcess{TMsg0, TBaseMsg}"/> where <typeparamref name="TMsg"/> must
    /// inherit from anteceding <typeparamref name="TBaseMsg"/> (<typeparamref name="TMsg"/> just serves to make the auto. process unique...).
    ///</remarks>
    public static IServiceCollection AddProcessType<TProcess, TMsg, TBaseMsg, TRes>(this IServiceCollection services) where TProcess : class, IAutoProcessType
                                                                                                                      where TMsg : class, TBaseMsg
                                                                                                                      where TBaseMsg : class
                                                                                                                      where TRes : class {
      services.TryAddEnumerable(ServiceDescriptor.Singleton<IAutoProcessType, TProcess>());
      services.AddSingleton(typeof(AutoProcessType<TMsg, TBaseMsg, TRes>), typeof(TProcess));
      services.AddSingleton(typeof(IAutoProcess<TMsg, TRes>), typeof(Service.AutoProcessProxySvc<TMsg, TRes>));
      return services;
    }
  }
}
