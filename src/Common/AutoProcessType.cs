using System;

namespace Tlabs.Proc.Common {

  ///<summary>Abstract automation process type descriptor for messsage type <typeparamref name="TMsg"/> with result <typeparamref name="TRes"/>.</summary>
  ///<remarks>
  ///This abstract base class is to faciliate the defintion of a <see cref="IAutoProcessType"/> with a process message as with:
  ///<code>
  ///public class MyProcessMsg {
  ///  public class PType : AutoProcessType&lt;MyProcessMsg, VoidUnit> {
  ///    public PType() : base("My process description...") { }
  ///  }
  ///}
  ///</code>
  ///</remarks>
  public abstract class AutoProcessType<TMsg, TRes> : AutoProcessType {
    ///<summary>Message type suffix.</summary>
    public const string MSG_SUFFIX= "Msg";

    ///<summary>Ctor from <paramref name="description"/> and optional <paramref name="restriction"/> or <paramref name="name"/>.</summary>
    public AutoProcessType(string description, AutoProcessRestriction? restriction= null, string? name= null)
      : base(name ?? buildTypeName(), typeof(TMsg), typeof(TRes), description, restriction) { }

    static string buildTypeName() {
      var tname = typeof(TMsg).Name;
      if (tname.EndsWith(MSG_SUFFIX, StringComparison.Ordinal))
        tname= tname[..^MSG_SUFFIX.Length];   //strip off MSG_SUFFIX
      return tname;
    }
  }

  ///<summary>Automation process type descriptor for messsage type <typeparamref name="TMsg"/>, base <typeparamref name="TBaseMsg"/> with result <typeparamref name="TRes"/>.</summary>
  ///<remarks>
  ///This class is to faciliate the defintion of a <see cref="IAutoProcessType"/> with a messsage type <typeparamref name="TMsg"/>
  ///that is derived from <typeparamref name="TBaseMsg"/> just to build a new <see cref="IAutoProcessType"/>.
  ///</remarks>
  public class AutoProcessType<TMsg, TBaseMsg, TRes> : AutoProcessType<TMsg, TRes> where TMsg : TBaseMsg {
    ///<summary>Ctor from <paramref name="description"/> and optional <paramref name="restriction"/> or <paramref name="name"/>.</summary>
    public AutoProcessType(string description, AutoProcessRestriction? restriction= null, string? name= null) : base(description, restriction, name) { }
    ///<inheritdoc/>
    public override Type MsgType => typeof(TBaseMsg);
  }

  ///<summary>Automation process base type descriptor.</summary>
  public class AutoProcessType : IAutoProcessType {
    ///<summary>Ctor from all properties.</summary>
    protected AutoProcessType(string name, Type msgType, Type resType, string description, AutoProcessRestriction? restriction= null) {
      this.Name= name;
      this.MsgType= msgType;
      this.ResultType= resType;
      this.Description= description;
      this.ExecRestriction= restriction;
    }

    ///<summary>Copy ctor from other <paramref name="pType"/> and optional new <paramref name="restriction"/>.</summary>
    protected AutoProcessType(IAutoProcessType pType, AutoProcessRestriction? restriction= null) {
      this.Name= pType.Name;
      this.MsgType= pType.MsgType;
      this.ResultType= pType.ResultType;
      this.Description= pType.Description;
      this.ExecRestriction= restriction;
    }

    ///<inheritdoc/>
    public string Name { get; }

    ///<inheritdoc/>
    public string Description { get; }

    ///<inheritdoc/>
    public virtual Type MsgType { get; }

    ///<inheritdoc/>
    public Type ResultType { get; }

    ///<inheritdoc/>
    public AutoProcessRestriction? ExecRestriction { get; }

    ///<inheritdoc/>
    public IAutoProcessType Copy(AutoProcessRestriction? restriction= null) => new AutoProcessType(this, restriction);

    ///<inheritdoc/>
    public override bool Equals(object? obj) {
      if (obj is not IAutoProcessType other) return false;
      return MsgType == other.MsgType && ResultType == other.ResultType;
    }

    ///<inheritdoc/>
    public override int GetHashCode() => this.GetType().GetHashCode();

  }
}
