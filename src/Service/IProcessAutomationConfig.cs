
using System;
using System.Collections.Generic;
using System.IO;

using Tlabs.Misc;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service {

  ///<summary>Process automation configuration.</summary>
  public interface IProcessAutomationConfig {

    ///<summary>Named automation process types.</summary>
    public IReadOnlyDictionary<string, IAutoProcessType> NamedPTypes { get; }

    ///<summary>Procedure config(s) by process type.</summary>
    public IEnumerable<IProcedureConfig> ProcessProcedures(IAutoProcessType pType);

    ///<summary>Procedure descriptor by <paramref name="name"/>.</summary>
    public IAutoProcedureDescriptor ProcedureDescriptor(string name);

    ///<summary>Returns all <see cref="ISequelControl"/>(s) registered for <paramref name="precursorType"/> and optional <paramref name="enabledOnly"/>.</summary>
    public IEnumerable<ISequelControl> ProcessSequelsByPrecursor(IAutoProcessType precursorType, bool enabledOnly= true);

    ///<summary>Returns all <see cref="ITimeScheduleControl"/>(s) registered for <paramref name="pType"/>.</summary>
    public IEnumerable<ITimeScheduleControl> TimeSchedulesByType(IAutoProcessType pType);

    ///<summary>Set status of <paramref name="procDesc"/>to <paramref name="enabled"/> and <paramref name="resultReturning"/> with optionam <paramref name="params"/>.</summary>
    public void SetProcedureStatus(IAutoProcedureDescriptor procDesc, bool enabled, bool resultReturning, IReadOnlyDictionary<string, object?>? @params= null);

    ///<summary>Set automation control schedule for process <paramref name="pType"/> with <paramref name="scheduleId"/>, <paramref name="timePattern"/>, <paramref name="message"/> as <paramref name="enabled"/>.</summary>
    public string? SetControlSchedule(IAutoProcessType pType, string scheduleId, string timePattern, object message, bool enabled);

    ///<summary>Set automation control <paramref name="sequel"/> as <paramref name="enabled"/>.</summary>
    public void SetControlSequel(ISequelControl sequel, bool enabled);

    ///<summary>Schedule process with <paramref name="pType"/> with <paramref name="scheduleId"/> to run as soon as possible.</summary>
    void ScheduleProcessAsap(IAutoProcessType pType, string scheduleId);


    ///<summary>Load configuration from <paramref name="strm"/>.</summary>
    public void LoadConfiguration(Stream strm);

    ///<summary>With (exclusive configuration access) <paramref name="perform"/>.</summary>
    public T WithExclusiveAccess<T>(Func<T> perform);
  }
}