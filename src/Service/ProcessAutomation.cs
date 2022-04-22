using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.Proc.Common;
using Tlabs.Msg;
using Microsoft.Extensions.Logging;

namespace Tlabs.Proc.Service {

  ///<summary>Process automation service.</summary>
  public class ProcessAutomation : IProcessAutomation {
    static readonly ILogger log= App.Logger<ProcessAutomation>();
    readonly IProcessAutomationConfig config;
    readonly IMessageBroker msgBroker;

    ///<summary>Ctor from DI services.</summary>
    public ProcessAutomation(IProcessAutomationConfig config, IMessageBroker msgBroker) {
      this.config= config;
      this.msgBroker= msgBroker;
    }
    
    ///<inheritdoc/>
    public IEnumerable<IAutoProcessType> AllProcessTypes() => config.NamedPTypes.Values;

    ///<inheritdoc/>
    public IAutoProcessType ProcessType(string name) {
      if (!config.NamedPTypes.TryGetValue(name, out var ptype)) throw new InvalidAutoProcessTypeException(name);
      return ptype;
    }

    ///<inheritdoc/>
    public IAutoProcedureDescriptor ProcedureDescriptor(string name) {
      throw new System.NotImplementedException();
    }

    ///<inheritdoc/>
    public Task<TRes> ExecuteProcess<TMsg, TRes>(IAutoProcessType processType, TMsg msg, int timeout = 0) where TRes : class {
      throw new System.NotImplementedException();
    }

  }

}