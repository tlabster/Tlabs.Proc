
using System.Collections.Generic;

namespace Tlabs.Proc.Common {
  
  ///<summary>Automation procedure runtime configuration.</summary>
  public interface IProcedureConfig {
    ///<summary>Processor descriptor.</summary>
    IAutoProcedureDescriptor Descriptor { get; }
    ///<summary>Enabled status.</summary>
    bool IsEnabled { get; }
    ///<summary>Result status.</summary>
    bool HasResult { get; }
    ///<summary>Named procedure parameters.</summary>
    ///<remarks>
    ///These parameters are passed to <see cref="IAutoProcedure{TMsg, TRes}.InitConfiguration(IReadOnlyDictionary{string, object})"/>
    ///to allow for procedure configuration with properties from a persistent store.
    ///</remarks>
    IReadOnlyDictionary<string, object?> ProcedureParams { get; }
  }
}
