using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Tlabs.Misc;
using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern;
using Tlabs.JobCntrl.Model.Intern.Starter;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service.Config {
  using Props= Dictionary<string, object?>;

  internal static class JobCfg {
    internal const string MASTER_PROCESS_STARTER= "AutoProcess-MSG";
    internal const string MASTER_CHAINED_STARTER= "AutoProcess-CHAIN";
    internal const string MASTER_SCHEDULE_STARTER= "AutoProcess-SCHEDULE";


    internal const string SUBJECT_PFX= "Prcs.";
    internal const string STARTER_SFX= "-Starter";
    internal const string CNTRL_SFX= "-Cntrl";
    internal const string SEQUEL_SFX= "-Sequel";
    internal const char SEQUEL_DELIM= '>';
    internal const string SCHEDULE_SFX= "-Schedule";
    internal const string MASTER_AUTOJOB_SFX= ":MasterAutoJob";
    internal const string AUTOJOB_SFX= ":AutoJob";
    internal const string AUTO_SEQUELJOB_SFX= CNTRL_SFX + AUTOJOB_SFX;
    internal const string PROCEDURE_DELIM= "-=>";


    internal static Dictionary<string, object?> PARLL_STARTER_PROPS= new() {
      [MasterStarter.RPROP_PARALLEL_START]= true
    };
    static readonly ILogger log= App.Logger<IProcessAutomationConfig>();

    internal static IJobCntrlConfigurator SetupAutoProcessMasterStarter(this IJobCntrlConfigurator jobCntrlCfg)
      => jobCntrlCfg.DefineMasterStarter(MASTER_PROCESS_STARTER,
                                        "Message based auto.process starter.",
                                        typeof(MessageSubscription).AssemblyQualifiedName ?? $"?.{nameof(MessageSubscription)}",
                                        PARLL_STARTER_PROPS)  //enable parallel starter activation
                    .DefineMasterStarter(MASTER_CHAINED_STARTER,
                                        "Follow-up auto.process starter.",
                                        typeof(Chained).AssemblyQualifiedName ?? $"?.{nameof(Chained)}",
                                        PARLL_STARTER_PROPS)  //enable parallel starter activation
                    .DefineMasterStarter(MASTER_SCHEDULE_STARTER,
                                        "Time scheduled auto.process starter.",
                                        typeof(TimeSchedule).AssemblyQualifiedName ?? $"?.{nameof(MessageSubscription)}");

    internal static IJobCntrlConfigurator SetupProcessJobCntrlMasters(this IJobCntrlConfigurator jobCntrlCfg, IEnumerable<IAutoProcessType> pTypes) {
      foreach (var pType in pTypes) {                 // Define a JobCntrl Starter and auto job for each pType:
        var starterName= StarterName(pType);
        var subject= MsgSubject(pType);
        jobCntrlCfg.DefineStarter(StarterName(pType), MASTER_PROCESS_STARTER, pType.Description, new Props {
          [MessageSubscription.PROP_MSG_SUBJECT]= subject,
          [MessageSubscription.PROP_RET_RESULT]= true
        });
        log.LogDebug("Starter {name} listening on subject {subj} for process {op} defined.", starterName, subject, pType.Name);

        Type autoJobType= typeof(Job.AutoProcessJob<,>).MakeGenericType(pType.MsgType, pType.ResultType);
        var autoMaster= MasterCntrlJobName(AutoCntrlName(pType));
        jobCntrlCfg.DefineMasterJob(autoMaster, "Process control job", autoJobType.AssemblyQualifiedName ?? $"?.{autoJobType.Name}", new Props {
          [BaseJob.PROP_LOGLEVEL]= "Debug",
          [Job.AutoProcessJob.PROP_PTYPE]= pType
        });
        log.LogDebug("Control job {name} for process {prcs} defined.", autoMaster, pType.Name);
      }
      return jobCntrlCfg;
    }

    internal static IJobCntrlConfigurator SetupProcedureJobMasters(this IJobCntrlConfigurator jobCntrlCfg,
                                                                  IEnumerable<IAutoProcedureDescriptor> allProcDescriptors,
                                                                  DictionaryList<IAutoProcessType, IAutoProcedureDescriptor> typedProcedures)
    {
      /* Check for duplicate procedure descriptor(s):
       */
      try {
        allProcDescriptors= allProcDescriptors.ToDictionary(proc => proc.Name).Values;
      }
      catch (ArgumentException e) { throw new AutoProcessException($"Invalid duplicate {nameof(IAutoProcedureDescriptor)}", e); }

      foreach (var procDesc in allProcDescriptors) {                  // Define a master job for each procedure (descriptor):
        typedProcedures.Add(procDesc.ProcessType, procDesc);
        var jobType= typeof(Job.ProcedureJob<,,>).MakeGenericType(new Type[] { procDesc.ProcedureType, procDesc.ProcessType.MsgType, procDesc.ProcessType.ResultType });
        jobCntrlCfg.DefineMasterJob(procDesc.Name, procDesc.Description, jobType.AssemblyQualifiedName ?? $"?.{jobType.Name}", new Props {
          [BaseJob.PROP_LOGLEVEL]= "Debug"
        });
        log.LogDebug("Auto. procedure master-job {name} defined.", procDesc.Name);
      }
      return jobCntrlCfg;
    }

    internal static IEnumerable<IProcedureConfig> ConvertToProcedureConfig(this IJobCntrlConfigurator jobCntrlCfg, IEnumerable<IAutoProcedureDescriptor> procDescs) {
      // left outer join procDescs with jobs
      return procDescs.GroupJoin(jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs,
                                 pd => pd.JobName(),
                                 job => job.Name,
                                 (pd, jobs) => new { ProcDesc= pd, Jobs= jobs })
                      .SelectMany(grp => grp.Jobs.DefaultIfEmpty(),
                                  (grp, job) => new ProcedureConfig(grp.ProcDesc, null != job, job?.Properties)
      );
    }

    internal static IEnumerable<ITimeScheduleControl> AllCntrlSchedules(this IJobCntrlConfigurator jobCntrlCfg, IProcessAutomationConfig config, string? processName= null)
      => jobCntrlCfg.JobCntrlCfg.ControlCfg.Starters.Where(s =>    s.Name.EndsWith(SCHEDULE_SFX, StringComparison.Ordinal)
                                                                && null == processName || processName == ProcessFromScheduledStarter(s.Name))
                                                    .Select(s => asTimeScheduleCntrl(config.NamedPTypes[ProcessFromScheduledStarter(s.Name)], s));

    internal static IEnumerable<ISequelControl> AllSequelControls(this IJobCntrlConfigurator jobCntrlCfg, IProcessAutomationConfig config)
    => jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.Where(job => job.Name.EndsWith(AUTO_SEQUELJOB_SFX, StringComparison.Ordinal))
                                              .Select(job => new SequelControl(config.NamedPTypes[ProcessFromSequelStarter(job.Starter)],
                                                                               config.NamedPTypes[ProcessFromCntrlJob(job.Name)],
                                                                               true));

    internal static string? SetControlSchedule(this IJobCntrlConfigurator jobCntrlCfg, IAutoProcessType pType, string scheduleId, string? timePattern, object? message, bool enabled) {
      var starterName= pType.ScheduledStarterName(scheduleId);
      var startDef= jobCntrlCfg.JobCntrlCfg.ControlCfg.Starters.SingleOrDefault(s => s.Name == starterName);
      enabled= null != scheduleId && enabled;
      if (enabled) {
        if (null != startDef) { //update starter and keep autoJob
          jobCntrlCfg.JobCntrlCfg.ControlCfg.Starters.Remove(startDef);   //remove old
          jobCntrlCfg.DefineStarter(startDef.Name, startDef.Master, startDef.Description, new Props {
            [TimeSchedule.PARAM_SCHEDULE_TIME]= timePattern ?? "",
            [TimeSchedule.RUN_PROPERTY_PREFIX + Job.AutoProcessJob.PROP_PROCESS_MSG]= message
          });
          return startDef.Name;
        }
        // insert new
        jobCntrlCfg.DefineStarter(starterName, MASTER_SCHEDULE_STARTER, $"Schedule {pType.Name} activation", new Props {
          [TimeSchedule.PARAM_SCHEDULE_TIME]= timePattern,
          [TimeSchedule.RUN_PROPERTY_PREFIX + Job.AutoProcessJob.PROP_PROCESS_MSG]= message
        });
        string autoJobName= CntrlJobName(starterName);
        jobCntrlCfg.DefineJob(autoJobName, MasterCntrlJobName(AutoCntrlName(pType)), starterName, $"Schedule {pType.Name} activation job");
        return starterName;
      }
      /* Remove on enabled == false
        */
      if (null != startDef) {
        jobCntrlCfg.JobCntrlCfg.ControlCfg.Starters.Remove(startDef);
        var autoJob= jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.Single(j => CntrlJobName(startDef.Name) == j.Name);
        jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.Remove(autoJob);
      }
      return startDef?.Name;
    }


    internal static void SetControlSequel(this IJobCntrlConfigurator jobCntrlCfg, ISequelControl seq, bool enabled) {
      var starterName= seq.Precursor.SequelStarterName();
      var seqName= seq.ProcessType.AutoCntrlName();
      var seqJobName= CntrlJobName(starterName, seqName);
      var autoJob= jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.FirstOrDefault(job => seqJobName == job.Name);
      if (enabled) {
        if (null != autoJob) return;  //already enabaled
        if (!jobCntrlCfg.JobCntrlCfg.ControlCfg.Starters.Any(start => starterName == start.Name)) {   //precursor has already a sequel starter?
          jobCntrlCfg.DefineStarter(starterName, MASTER_CHAINED_STARTER, seq.Description, new Props {
            [Chained.PROP_COMPLETED_STARTER]= seq.Precursor.StarterName()
          });
        }
        jobCntrlCfg.DefineJob(seqJobName, MasterCntrlJobName(seqName), starterName, seq.Description);
      }
      else {
        if (null == autoJob) return;  //already disabled
        jobCntrlCfg.JobCntrlCfg.ControlCfg.Jobs.Remove(autoJob);
      }
    }

    static TimeScheduleControl asTimeScheduleCntrl(IAutoProcessType pType, JobCntrl.Config.JobCntrlCfg.StarterCfg scheduleStarter)
      => new TimeScheduleControl(scheduleStarter.ScheduleId(),
                                 scheduleStarter?.Properties[TimeSchedule.PARAM_SCHEDULE_TIME] as string,
                                 pType,
                                 scheduleStarter?.Properties[TimeSchedule.RUN_PROPERTY_PREFIX + Job.AutoProcessJob.PROP_PROCESS_MSG]
      );

    internal static string MsgSubject(this IAutoProcessType pt) => SUBJECT_PFX + pt.Name;
    internal static string StarterName(string pType) => pType + STARTER_SFX;
    internal static string StarterName(this IAutoProcessType pt) => StarterName(pt.Name);
    internal static string ProcedureJobNamePfx(this IAutoProcessType pt) => pt.Name + PROCEDURE_DELIM;
    internal static string JobName(this IAutoProcedureDescriptor pd) => ProcedureJobNamePfx(pd.ProcessType) + pd.Name;
    internal static string AutoCntrlName(this IAutoProcessType pt) => pt.Name + CNTRL_SFX;
    internal static string MasterCntrlJobName(string cntrlName) => cntrlName + MASTER_AUTOJOB_SFX;
    internal static string CntrlJobName(string starterName, string? cntrlName= null)
      => $"{starterName}{(string.IsNullOrEmpty(cntrlName) ? string.Empty : (SEQUEL_DELIM+cntrlName))}{AUTOJOB_SFX}";
    internal static string SequelStarterName(this IAutoProcessType pt) => pt.Name + SEQUEL_SFX;
    internal static string ScheduledStarterName(this IAutoProcessType pt, string scheduleId) => $"{pt.Name}@{scheduleId}{SCHEDULE_SFX}";

    internal static string ProcessFromMsgSubject(string subject) {
      if (!subject.StartsWith(SUBJECT_PFX, StringComparison.Ordinal)) throw new AutoProcessException($"Invalid process msg. subj.: '{subject}'");
      return subject[SUBJECT_PFX.Length..];
    }
    internal static string ProcessFromScheduledStarter(string starterName) {
      var at= starterName.IndexOf('@');
      if (at < 0 || !starterName.EndsWith(SCHEDULE_SFX, StringComparison.Ordinal)) throw new AutoProcessException($"Invalid schdule starter name: '{starterName}'");
      return starterName[..at];
    }
    internal static string ScheduleId(this JobCntrl.Config.JobCntrlCfg.StarterCfg starter) {
      var at= starter.Name.IndexOf('@');
      if (at < 0 || !starter.Name.EndsWith(SCHEDULE_SFX, StringComparison.Ordinal)) throw new AutoProcessException($"Invalid schdule starter name: '{starter.Name}'");
      return starter.Name.Substring(at+1, starter.Name.Length - at - SCHEDULE_SFX.Length-1);
    }
    internal static string ProcessFromSequelStarter(string starterName) => ExtractSufixName(starterName, SEQUEL_SFX);
    internal static string ProcessFromCntrlJob(string jobName) => ExtractSufixName(jobName, AUTO_SEQUELJOB_SFX).Split(SEQUEL_DELIM, 2).Last();
    internal static string ExtractSufixName(string starterName, string sfx= STARTER_SFX) {
      if (!starterName.EndsWith(sfx, StringComparison.Ordinal)) throw new AutoProcessException($"Invalid starter name: '{starterName}'");
      return starterName[..^sfx.Length];
    }

  }

}
