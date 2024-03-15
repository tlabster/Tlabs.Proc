using System.Collections.Generic;

namespace Tlabs.Proc.Common {

  ///<summary>Interface of an automation procedure with messsage of type <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
  public interface IAutoProcedure<in TMsg, TRes> : IAutoProcedure where TMsg : class where TRes : notnull {

    ///<summary>Initialize procedure with configuration <paramref name="params"/>.</summary>
    ///<returns><para>
    ///A Procedure instance for invocating <see cref="IAutoProcedure{TMsg, TRes}.Execute(TMsg)"/>
    /// (typically just <c>this</c>/>.</para>
    ///NOTE: Could be different (e.g. shared instance).
    ///</returns>
    IAutoProcedure InitConfiguration(IReadOnlyDictionary<string, object?> @params);

    ///<summary>Execute procedure (synchronously) with messsage type <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
    TRes Execute(TMsg msg);
  }

  ///<summary>Base interface of an automation procedure.</summary>
  public interface IAutoProcedure { }

  ///<summary>Interface of an automation procedure with messsage type <typeparamref name="TMsg"/> derived from <typeparamref name="TBaseMsg"/> and result <typeparamref name="TRes"/>.</summary>
  public interface IAutoProcedure<TMsg, TBaseMsg, TRes> : IAutoProcedure<TBaseMsg, TRes> where TMsg : class, TBaseMsg where TBaseMsg : class where TRes : notnull {
    ///<summary>Execute procedure (synchronously) with messsage of type <typeparamref name="TMsg"/> and result <typeparamref name="TRes"/>.</summary>
    TRes Execute(TMsg msg);
  }

}