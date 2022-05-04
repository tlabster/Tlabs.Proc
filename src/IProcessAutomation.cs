using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.Proc.Common;

namespace Tlabs.Proc {

  ///<summary>Interface of a process automation service.</summary>
  public interface IProcessAutomation {
    ///<summary>Enumeration of all registered <see cref="IAutoProcessType"/>(s).</summary>
    IEnumerable<IAutoProcessType> AllProcessTypes { get; }

    ///<summary><see cref="IAutoProcessType"/> with <paramref name="name"/>.</summary>
    IAutoProcessType ProcessType(string name);

    ///<summary><see cref="IAutoProcedureDescriptor"/> with procedure <paramref name="name"/>.</summary>
    IAutoProcedureDescriptor ProcedureDescriptor(string name);

    ///<summary>Procedure config(s) by process type.</summary>
    public IEnumerable<IProcedureConfig> ProcessProcedures(IAutoProcessType pType);

    ///<summary>Returns all <see cref="ISequelControl"/>(s) registered for <paramref name="precursorType"/> and optional <paramref name="enabledOnly"/>.</summary>
    public IEnumerable<ISequelControl> ProcessSequelsByPrecursor(IAutoProcessType precursorType, bool enabledOnly = true);

    ///<summary>Returns all <see cref="ITimeScheduleControl"/>(s) registered for <paramref name="pType"/>.</summary>
    public IEnumerable<ITimeScheduleControl> TimeSchedulesByType(IAutoProcessType pType);

    ///<summary>Execute automation process with <paramref name="processType"/> and message type <paramref name="msg"/> returning result of <typeparamref name="TRes"/>
    ///(with optional <paramref name="timeout"/>).
    ///</summary>
    Task<TRes> ExecuteProcess<TMsg, TRes>(IAutoProcessType processType, TMsg msg, int timeout= 0) where TRes : class;

    ///<summary>Schedule process with <paramref name="pType"/> with <paramref name="scheduleId"/> to run as soon as possible.</summary>
    void ScheduleProcessAsap(IAutoProcessType pType, string scheduleId);

  }
}