using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.Proc.Common;

namespace Tlabs.Proc {

  ///<summary>Interface of a process automation service.</summary>
  public interface IProcessAutomation {
    ///<summary>Enumeration of all registered <see cref="IAutoProcessType"/>(s).</summary>
    IEnumerable<IAutoProcessType> AllProcessTypes();

    ///<summary><see cref="IAutoProcessType"/> with <paramref name="name"/>.</summary>
    IAutoProcessType ProcessType(string name);

    ///<summary><see cref="IAutoProcedureDescriptor"/> with procedure <paramref name="name"/>.</summary>
    IAutoProcedureDescriptor ProcedureDescriptor(string name);

    ///<summary>Execute automation process with <paramref name="processType"/> and message type <paramref name="msg"/> returning result of <typeparamref name="TRes"/>
    ///(with optional <paramref name="timeout"/>).
    ///</summary>
    Task<TRes> ExecuteProcess<TMsg, TRes>(IAutoProcessType processType, TMsg msg, int timeout= 0) where TRes : class;

  }
}