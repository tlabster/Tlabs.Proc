
using System;
using System.Threading.Tasks;

using Tlabs.Proc.Common;

namespace Tlabs.Proc {

  ///<summary>Interface of an automation process with messsage type <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
  ///<remarks>
  ///An automation process represents the abstract notion of a business process. A process is implemented with one or more procedures. The signature of
  ///a process (and its procedures) is given by its <typeparamref name="TMsg"/>.
  ///</remarks>
  public interface IAutoProcess<in TMsg, TRes> where TRes : class {
    ///<summary>Automation procedure type.</summary>
    IAutoProcessType ProcessType { get; }

    ///<summary>Return result of a process execution (with optional <paramref name="timeout"/>).</summary>
    Task<TRes> ExecResult(TMsg msg, int timeout = 0);
  }

  ///<summary>Interface of an process type descriptor.</summary>
  public interface IAutoProcessType : IAutomationDescriptor {
    ///<summary>Unique operation message type.</summary>
    Type MsgType { get; }
    ///<summary>Operation reuslt type.</summary>
    Type ResultType { get; }
    /// <summary><see cref="AutoProcessRestriction"/> to be checked with a <see cref="IStatefulMessage"/>.</summary>
    /// <remarks>A process execution with a <see cref="IStatefulMessage"/> that matches the <see cref="AutoProcessRestriction"/> is refused.</remarks>
    AutoProcessRestriction? ExecRestriction { get; set; }
  }

  ///<summary>Interface of an automation process descriptor.</summary>
  public interface IAutomationDescriptor {
    ///<summary>Process name.</summary>
    string Name { get; }
    ///<summary>Process description.</summary>
    string Description { get; }
  }

}