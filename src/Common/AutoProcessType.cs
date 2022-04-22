using System;

namespace Tlabs.Proc.Common {

  ///<summary>Abstract automation process type descriptor for messsage type <typeparamref name="TMsg"/> with result <typeparamref name="TRes"/>.</summary>
  ///<remarks>
  ///This abstract helper class is to faciliate the defintion of a <see cref="IAutoProcessType"/> with a process message lie with:
  ///<code>
  ///public class MyProcessMsg {
  ///  public class PType : AutoProcessType&lt;MyProcessMsg, VoidUnit> {
  ///    public PType() : base("My process description...") { }
  ///  }
  ///}
  ///</code>
  ///</remarks>
  public abstract class AutoProcessType<TMsg, TRes> : IAutoProcessType {
    const string MSG_SUFFIX= "Msg";

    ///<summary>Ctor from <paramref name="description"/> and optional <paramref name="name"/>.</summary>
    public AutoProcessType(string description, string? name= null) : this(null, description, name) { }

    ///<summary>Ctor from <paramref name="description"/> and optional <paramref name="name"/>.</summary>
    public AutoProcessType(AutoProcessRestriction? restriction, string description, string? name= null) {
      this.ExecRestriction= restriction;
      this.Description= description ?? "-?-";
      this.Name= name ?? buildTypeName();
    }

    ///<inheritdoc/>
    public string Name { get; }

    ///<inheritdoc/>
    public string Description { get; }

    ///<inheritdoc/>
    public virtual Type MsgType => typeof(TMsg);

    ///<inheritdoc/>
    public Type ResultType => typeof(TRes);

    ///<inheritdoc/>
    public AutoProcessRestriction? ExecRestriction { get; }

    ///<inheritdoc/>
    public override bool Equals(object? obj) => obj is AutoProcessType<TMsg, TRes>;

    ///<inheritdoc/>
    public override int GetHashCode() => this.GetType().GetHashCode();

    static string buildTypeName() {
      var tname= typeof(TMsg).Name;
      if (tname.EndsWith(MSG_SUFFIX, StringComparison.Ordinal))
        tname= tname[..^MSG_SUFFIX.Length];   //strip off MSG_SUFFIX
      return tname;
    }
  }

  ///<summary>Abstract automation process type descriptor for messsage type <typeparamref name="TMsg"/>, base <typeparamref name="TBaseMsg"/> with result <typeparamref name="TRes"/>.</summary>
  ///<remarks>
  ///This abstract helper class is to faciliate the defintion of a <see cref="IAutoProcessType"/> with a messsage type <typeparamref name="TMsg"/>
  ///that is derived from <typeparamref name="TBaseMsg"/> just to build a new <see cref="IAutoProcessType"/>.
  ///</remarks>
  public abstract class AutoProcessType<TMsg, TBaseMsg, TRes> : AutoProcessType<TMsg, TRes> where TMsg : TBaseMsg {
    ///<summary>Ctor from <paramref name="desc"/> and optional <paramref name="name"/>.</summary>
    public AutoProcessType(string desc, string? name= null) : base(desc, name) { }
    ///<inheritdoc/>
    public override Type MsgType => typeof(TBaseMsg);
  }

}
