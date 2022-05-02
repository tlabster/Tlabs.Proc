using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.Proc.Common;
namespace Tlabs.Proc.Service {

  ///<summary>Process automation service.</summary>
  public class ProcessAutomation : IProcessAutomation {
    /***TODO: Refactor the configuration.
     *  Configuration comes from two sources:
     *  1. DI/service provider
     *     - Process types, Procedure descriptors and a default procedure configuration (enabled, hasResult)
     *     - Default process restriction (TODO: The restrictions need to be separated,
     *       because they need to be changed during runtime - process type is constant...)
     *     - TODO: Default automation control settings (schedule and sequels)
     *  2. Configuration loaded during runtime
     *     - process restrictions
     *     - automation control settings (schedule and sequels)
     *     - Runtime changes very likely require to block any process execution until fully applied.
     * 
     *  Vision:
     *  - Default configuration from DI/service provider must be exhaustive to serve as ground truth
     *    and can be optional the sole configuration source (no runtime update required).
     *  - Loaded config. used as fine-tuning customization with possibility to fall back to a factory reset of
     *    of the default configuration.
     */
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
    public IEnumerable<ISequelControl> ProcessSequelsByPrecursor(IAutoProcessType precursorType, bool enabledOnly= true)
      => config.ProcessSequelsByPrecursor(precursorType, enabledOnly);

    ///<inheritdoc/>
    public IEnumerable<ITimeScheduleControl> TimeSchedulesByType(IAutoProcessType pType) => config.TimeSchedulesByType(pType);

    ///<inheritdoc/>
    public async Task<TRes> ExecuteProcess<TMsg, TRes>(IAutoProcessType pType, TMsg msg, int timeout= 0) where TRes : class
      => await execAgent.PublishExecutionRequest<TMsg, TRes>(pType, msg, timeout);

    ///<inheritdoc/>
    public void ScheduleProcessAsap(IAutoProcessType pType, string scheduleId) => config.ScheduleProcessAsap(pType, scheduleId);
  }

}