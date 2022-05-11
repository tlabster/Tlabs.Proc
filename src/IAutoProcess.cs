
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
    ///<summary>Unique process message type.</summary>
    ///<remarks>NOTE:
    ///<para>Equality of a <see cref="IAutoProcessType"/> is determined by the <see cref="Type"/> returned from <see cref="MsgType"/>.#
    ///For this the <see cref="MsgType"/> must be unique among all <see cref="IAutoProcessType"/>(s).</para>
    ///</remarks>
    Type MsgType { get; }
    ///<summary>Process reuslt type.</summary>
    Type ResultType { get; }
    /// <summary><see cref="AutoProcessRestriction"/> to be checked with a <see cref="IStatefulMessage"/>.</summary>
    /// <remarks>A process execution with a <see cref="IStatefulMessage"/> that matches the <see cref="AutoProcessRestriction"/> is refused.</remarks>
    AutoProcessRestriction? ExecRestriction { get; }
    ///<summary>Copy of automation process type with optional new <paramref name="restriction"/>.</summary>
    public IAutoProcessType Copy(AutoProcessRestriction? restriction= null);
  }

  ///<summary>Interface of an automation process descriptor.</summary>
  public interface IAutomationDescriptor {
    ///<summary>Process name.</summary>
    string Name { get; }
    ///<summary>Process description.</summary>
    string Description { get; }
  }

}