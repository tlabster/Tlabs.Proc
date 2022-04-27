using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern;
using Tlabs.JobCntrl.Model.Intern.Starter;
using Tlabs.Misc;
using Tlabs.Data.Serialize;
using Tlabs.Proc.Common;
using Tlabs.Proc.Service.Config.Job;

namespace Tlabs.Proc.Service.Config {
  using Props= Dictionary<string, object>;

  ///<summary>Process automation service configuration implemented with <see cref="IJobControl"/> configuration.</summary>
  public class ProcessAutomationJobConfig : IProcessAutomationConfig {
    static readonly ILogger log= App.Logger<IProcessAutomationConfig>();

    readonly IJobControl jobCntrlRuntime;
    readonly ISerializer<Data.AutoProcessCfgData> cfgSeri;
    readonly IJobCntrlConfigurator jobCntrlCfg;
    readonly Data.AutoProcessCfgData defaultConfig;

    /* Procedure (descriptor) by IAutoProcessType:
     */
    readonly DictionaryList<IAutoProcessType, IAutoProcedureDescriptor> typedProcedures= new ();


    ///<summary>Ctor from DI services.</summary>
    public ProcessAutomationJobConfig(IJobControl jobCntrlRuntime,
                                      ISerializer<Data.AutoProcessCfgData> cfgSeri,
                                      IEnumerable<IJobCntrlConfigurator> jobCntrlConfigs,
                                      IEnumerable<IAutoProcessType> pTypes,
                                      IEnumerable<IAutoProcedureDescriptor> procedureDesciptors,
                                      IEnumerable<IResultAutoProcedureType> rsltProcedureDesc,
                                      IEnumerable<IDefaultAutoProcedureType> dfltProcedureDesc) {
      this.jobCntrlRuntime= jobCntrlRuntime;
      this.cfgSeri= cfgSeri;
      this.NamedPTypes= setupNamedPTypes(pTypes);
      this.jobCntrlCfg= jobCntrlConfigs.First()
                                       .SetupAutoProcessMasterStarter()
                                       .SetupProcessJobCntrlMasters(pTypes)
                                       .SetupProcedureJobMasters(procedureDesciptors.Concat(rsltProcedureDesc.Cast<IAutoProcedureDescriptor>())
                                                                                    .Concat(dfltProcedureDesc.Cast<IAutoProcedureDescriptor>()),
                                                                 typedProcedures);

      /* Configure per default result returning processors:
       */
      foreach (var procDesc in rsltProcedureDesc.Cast<IAutoProcedureDescriptor>())
        SetProcedureStatus(procDesc, enabled: true, resultReturning: true);

      /* Configure per default connected processors:
       */
      foreach (var procDesc in dfltProcedureDesc.Cast<IAutoProcedureDescriptor>())
        SetProcedureStatus(procDesc, enabled: true, resultReturning: false);

      defaultConfig= currentConfig();

    }

    ///<inheritdoc/>
    public IReadOnlyDictionary<string, IAutoProcessType> NamedPTypes { get; }

    ///<inheritdoc/>
    public IEnumerable<IProcedureConfig> ProcessProcedures(IAutoProcessType pType) {
      lock(syncLock) {
        if (typedProcedures.TryGetValue(pType, out var configs))
          return jobCntrlCfg.ConvertToProcedureConfig(configs);
        return Enumerable.Empty<IProcedureConfig>();
      }
    }

    ///<inheritdoc/>
    public IAutoProcedureDescriptor ProcedureDescriptor(string name) => typedProcedures.Values.Single(pd => name == pd.Name);

    ///<inheritdoc/>
    public IEnumerable<ITimeScheduleControl> TimeSchedulesByType(IAutoProcessType pType) {
      lock (syncLock) {
        return jobCntrlCfg.AllCntrlSchedules(this, pType.Name);
      }
    }

    ///<inheritdoc/>
    public IEnumerable<ISequelControl> ProcessSequelsByTypeType(IAutoProcessType precursorType, bool enabledOnly = true) {
      lock (syncLock) {
        var starterName= JobCfg.SequelStarterName(precursorType);
        var followupTypes= NamedPTypes.Values
                                      .Where(t => precursorType.ResultType.IsAssignableFrom(t.MsgType));
        // left outer join of sequel types with autoJobs
        var jobsByPTypes= followupTypes.GroupJoin(jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs,
                                                   ptype => JobCfg.CntrlJobName(starterName, JobCfg.AutoCntrlName(ptype)),
                                                   job => job.Name,
                                                   (ptype, job) => new { OpType= ptype, AutoJob= job })
                                       .ToList();
        return jobsByPTypes.SelectMany( grp        => grp.AutoJob.DefaultIfEmpty(),
                                       (grp, job)  => new SequelControl(precursorType, grp.OpType, null != job))
                           .Where(sq => !enabledOnly || sq.IsEnabled);
      }
    }

    ///<inheritdoc/>
    public void SetProcedureStatus(IAutoProcedureDescriptor procDesc, bool enabled, bool resultReturning, IReadOnlyDictionary<string, object?>? @params= null) {
      lock (syncLock) {
        var jobName= procDesc.JobName();
        var procJob= jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.FirstOrDefault(job => jobName == job.Name);
        if (enabled) {
          if (null != procJob) return;   //already enabled
          if (!resultReturning) @params= new ConfigProperties(@params, new Props { [Job.AutoProcessJob.PROP_NO_RESULT]= true });
          jobCntrlCfg.DefineJob(jobName, procDesc.Name, procDesc.ProcessType.StarterName(), procDesc.Description, @params);
        }
        else {
          if (null == procJob) return;   //already disabled
          jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.Remove(procJob);
        }
      }
    }

    ///<inheritdoc/>
    public string? SetControlSchedule(IAutoProcessType pType, string scheduleId, string? timePattern, object? message, bool enabled) {
      lock (syncLock) {
        return jobCntrlCfg.SetControlSchedule(pType, scheduleId, timePattern, message, enabled);
      }
    }

    ///<inheritdoc/>
    public void SetControlSequel(ISequelControl sequel, bool enabled) {
      lock(syncLock) {
        jobCntrlCfg.SetControlSequel(sequel, enabled);
      }
    }
    ///<inheritdoc/>
    public void LoadConfiguration(Stream strm) {
      lock (syncLock) configure(cfgSeri.LoadObj(strm));
    }

    ///<inheritdoc/>
    public void ResetConfiguration() {
      lock (syncLock) {
        configure(defaultConfig);
        applyJobCntrlCfg();
        ConfigStorage.CleanUp();
      }
    }

    ///<inheritdoc/>
    public T WithExclusiveAccess<T>(Func<T> perform) {
      lock(syncLock) return perform();
    }

    object syncLock => this.jobCntrlCfg;

    private void applyJobCntrlCfg() {
      jobCntrlRuntime.Stop();
      jobCntrlRuntime.Init();
      jobCntrlRuntime.Start();
    }
    private Data.AutoProcessCfgData currentConfig() {
      return new Data.AutoProcessCfgData {
        PTypes=         NamedPTypes.Values
                                   .Where(tp => null != tp.ExecRestriction)
                                   .Select(tp => new Data.AutoProcessCfgData.ProcessTypeData { PType= tp.Name, RestrictedStates= tp.ExecRestriction?.ToString() })
                                   .ToList(),
        Procedures=     jobCntrlCfg.ConvertToProcedureConfig(typedProcedures.Values)
                                   .Where(pd => pd.IsEnabled)
                                   .Select(pd => ProcedureConfig.ToEntity(pd)).ToList(),
        CntrlSchedules= NamedPTypes.Values
                                   .SelectMany(ptype => TimeSchedulesByType(ptype))
                                   .Select(s => TimeScheduleControl.ToEntity(s)).ToList(),
        CntrlSequels=   NamedPTypes.Values
                                   .SelectMany(ptype => ProcessSequelsByTypeType(ptype))
                                   .Select(sq => SequelControl.ToEntity(sq)).ToList()
      };
    }

    private void configure(Data.AutoProcessCfgData autoCfg) {
      try {
        var typedProcConfigs= new DictionaryList<IAutoProcessType, IProcedureConfig>();
        foreach (var procCfg in autoCfg.Procedures.Select(p => p.AsProcedureConfig(this)).Where(p => null != p))
          typedProcConfigs.Add(procCfg!.Descriptor.ProcessType, procCfg);

        foreach (var pair in typedProcConfigs) {
          foreach (var procCfg in ProcessProcedures(pair.Key).Where(p => p.IsEnabled))          //disable current/default settings
            SetProcedureStatus(procCfg.Descriptor, false, false, null);
          foreach (var procCfg in pair.Value)                                                   //connect new config
            SetProcedureStatus(procCfg.Descriptor, true, procCfg.HasResult, procCfg.ProcedureParams);
        }

        foreach (var sch in jobCntrlCfg.AllCntrlSchedules(this) .ToList())
          SetControlSchedule(sch.ProcessType, sch.ScheduleId, null, null, false);               //disbale all current
        foreach (var sch in autoCfg.CntrlSchedules.Select(s => s.AsTimeSchedule(this)))
          if (null != sch) SetControlSchedule(sch.ProcessType, sch.ScheduleId, sch.TimePattern, sch.Message, true);

        foreach (var seq in jobCntrlCfg.AllSequelControls(this).ToList())                       //disable all current sequel(s)
          SetControlSequel(seq, false);
        foreach (var seq in autoCfg.CntrlSequels.Select(s => s.AsSequel(this)))
          if (null != seq) SetControlSequel(seq, true);

        applyProcessRestrictions(autoCfg.PTypes);

      }
      catch (Exception e) { throw new AutoProcessException($"Invalid {nameof(Data.AutoProcessCfgData)}", e); }
    }

    void applyProcessRestrictions(List<Data.AutoProcessCfgData.ProcessTypeData> pTypeData) {
      foreach (var pdat in pTypeData) {
        var pType= NamedPTypes[pdat.PType ?? "?"];

        if (!string.IsNullOrEmpty(pdat.RestrictedStates))
          pType.ExecRestriction= (AutoProcessRestriction)pdat.RestrictedStates;
        else {
          var dflt= defaultConfig.PTypes.SingleOrDefault(d => d.PType == pdat.PType);
          if (!string.IsNullOrEmpty(dflt?.RestrictedStates))
            pType.ExecRestriction= (AutoProcessRestriction)dflt.RestrictedStates;
        }        
      }
    }

    static IReadOnlyDictionary<string, IAutoProcessType> setupNamedPTypes(IEnumerable<IAutoProcessType> pTypes) {
      try {
        var namedPTypes= new LookupTable<string, IAutoProcessType>(pTypes.ToDictionary(p => p.Name), name => throw new InvalidAutoProcessTypeException(name));
        log.LogDebug("{cnt} process types discovered from service provider.", namedPTypes.Count);
        return namedPTypes;
      }
      catch (ArgumentException e) { throw new AutoProcessException("Duplicate process type name.", e); }
    }

  }
}
