
using System.Collections.Generic;

using Tlabs.Misc;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service {

  ///<summary>Process automation configuration.</summary>
  public interface IProcessAutomationConfig {

    ///<summary>Named automation process types.</summary>
    public IReadOnlyDictionary<string, IAutoProcessType> NamedPTypes { get; }

    ///<summary>Procedure descriptor(s) by process type.</summary>
    public IReadOnlyDictList<IAutoProcessType, IAutoProcedureDescriptor> ProcessProcedures { get; }

    

  }
}