using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service {
  ///<summary>Abstract base class of an automation process (proxy) service.</summary>
  public abstract class AutoProcessProxySvc {
    ///<summary><see cref="IProcessAutomation"/>.</summary>
    protected IProcessAutomation autoSvc;
    ///<summary>Ctor from <paramref name="autoSvc"/>.</summary>
    public AutoProcessProxySvc(IProcessAutomation autoSvc) {
      this.autoSvc= autoSvc;
    }
  }

  ///<summary>Automation process (proxy) service.</summary>
  ///<remarks>Instances of this class should be obtained via dependency injection. An automation process service can be used to programmatically execute a process.</remarks>
  public class AutoProcessProxySvc<TMsg, TRes> : AutoProcessProxySvc, IAutoProcess<TMsg, TRes> where TMsg : class  where TRes : class {
    ///<summary>Ctor from <paramref name="autoSvc"/> and <paramref name="procDescriptors"/>.</summary>
    public AutoProcessProxySvc(IProcessAutomation autoSvc, AutoProcessType<TMsg, TRes> processType, IEnumerable<IAutoProcedureDescriptor<TMsg, TRes>> procDescriptors) : base(autoSvc) {
      ProcessType= processType;
      if (!procDescriptors.Any()) throw new AutoProcessException($"{processType.Name} ({processType.GetType()}) must have at least ONE joint procedure.");
    }

    ///<inheritdoc/>
    public IAutoProcessType ProcessType { get; }

    ///<inheritdoc/>
    public Task<TRes> ExecResult(TMsg msg, int timeout = 0) => autoSvc.ExecuteProcess<TMsg, TRes>(ProcessType, msg, timeout);

  }
}
