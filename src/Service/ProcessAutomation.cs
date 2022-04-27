using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Tlabs.Msg;
using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern;
using Tlabs.Proc.Common;
using Tlabs.Proc.Service.Config.Job;
using Tlabs.Misc;

namespace Tlabs.Proc.Service {

  ///<summary>Process automation service.</summary>
  public class ProcessAutomation : IProcessAutomation {
    readonly IProcessAutomationConfig config;
    readonly IAutoProcessExecAgent execAgent;

    ///<summary>Ctor from DI services.</summary>
    public ProcessAutomation(IProcessAutomationConfig config, IAutoProcessExecAgent execAgent) {
      this.config= config;
      this.execAgent= execAgent;
    }
    
    ///<inheritdoc/>
    public IEnumerable<IAutoProcessType> AllProcessTypes => config.NamedPTypes.Values;

    ///<inheritdoc/>
    public IAutoProcessType ProcessType(string name) => config.NamedPTypes[name];

    ///<inheritdoc/>
    public IAutoProcedureDescriptor ProcedureDescriptor(string name) => config.ProcedureDescriptor(name);

    ///<inheritdoc/>
    public IEnumerable<IProcedureConfig> ProcessProcedures(IAutoProcessType pType) => config.ProcessProcedures(pType);

    ///<inheritdoc/>
    public async Task<TRes> ExecuteProcess<TMsg, TRes>(IAutoProcessType pType, TMsg msg, int timeout= 0) where TRes : class
      => await execAgent.PublishExecutionRequest<TMsg, TRes>(pType, msg, timeout);
  }

}