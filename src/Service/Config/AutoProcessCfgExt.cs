using System;
using Microsoft.Extensions.Logging;

using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service.Config {

  internal static class AutoProcessCfgExt {
    static readonly ILogger log= App.Logger<IProcedureConfig>();
    internal static IProcedureConfig? AsProcedureConfig(this Data.AutoProcessCfgData.ProcedureData entity, IProcessAutomationConfig config) {
      IProcedureConfig? pcfg= null;
      try { pcfg= new ProcedureConfig(config, entity); }
      catch (ArgumentException e) { log.LogWarning("Ignoring invalid procedure config: {name} ({msg})", entity.Name, e.Message); }
      return pcfg;
    }

    internal static ITimeScheduleControl? AsTimeSchedule(this Data.AutoProcessCfgData.ScheduleData entity, IProcessAutomationConfig config) {
      ITimeScheduleControl? sch= null;
      try { sch= new TimeScheduleControl(config, entity); }
      catch (Exception e) { log.LogWarning("Ignoring invalid schedule config ({msg})", e.Message); }
      return sch;
    }

    internal static ISequelControl? AsSequel(this Data.AutoProcessCfgData.SequelData entity, IProcessAutomationConfig config) {
      ISequelControl? seq= null;
      try { seq= new SequelControl(config, entity); }
      catch (Exception e) { log.LogWarning("Ignoring invalid sequel config ({msg})", e.Message); }
      return seq;
    }
  }
}