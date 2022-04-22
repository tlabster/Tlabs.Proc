using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern;
using Tlabs.JobCntrl.Model.Intern.Starter;
using Tlabs.Misc;
using Tlabs.Data.Serialize;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service.Config {
  using Props= Dictionary<string, object>;

  ///<summary>Process automation service configuration implemented with <see cref="IJobControl"/> configuration.</summary>
  public class ProcessAutomationJobConfig : IProcessAutomationConfig {
    internal const string MASTER_PROCESS_STARTER= "AutoProcess-MSG";
    internal const string MASTER_CHAINED_STARTER= "AutoProcess-CHAIN";
    internal const string MASTER_SCHEDULE_STARTER= "AutoProcess-SCHEDULE";
    internal IReadOnlyDictionary<string, object> PARLL_STARTER_PROPS= new Dictionary<string, object> {
      [MasterStarter.RPROP_PARALLEL_START]= true
    };

    static readonly ILogger log= App.Logger<ProcessAutomationJobConfig>();

    readonly IJobControl jobCntrlRuntime;
    readonly ISerializer<Data.AutoProcessCfgData> cfgSeri;
    readonly IJobCntrlConfigurator jobCntrlCfg;

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
      this.jobCntrlCfg= jobCntrlConfigs.First();

      setupJobCntrlMasterStarter();
      /*  Define a JobCntrl Starter and auto job for each process type:
       */
      foreach (var pType in pTypes) {
        setupProcessJobCntrlMaster(pType);
      }
    }

    ///<summary>Dictionary of <see cref="IAutoProcessType"/>(s) indexed by process name.</summary>
    public IReadOnlyDictionary<string, IAutoProcessType> NamedPTypes { get; }

    ///<summary>Dictionary list of <see cref="IAutoProcedureDescriptor"/> (s) indexed by <see cref="IAutoProcessType"/>.</summary>
    public IReadOnlyDictList<IAutoProcessType, IAutoProcedureDescriptor> ProcessProcedures => throw new System.NotImplementedException();


    IJobCntrlConfigurator setupJobCntrlMasterStarter()
      => jobCntrlCfg.DefineMasterStarter(MASTER_PROCESS_STARTER,
                                        "Message based LOP-Job starter.",
                                        typeof(MessageSubscription).AssemblyQualifiedName,
                                        PARLL_STARTER_PROPS)  //enable parallel starter activation
                    .DefineMasterStarter(MASTER_CHAINED_STARTER,
                                        "Follow-up LOP auto starter.",
                                        typeof(Chained).AssemblyQualifiedName,
                                        PARLL_STARTER_PROPS)  //enable parallel starter activation
                    .DefineMasterStarter(MASTER_SCHEDULE_STARTER,
                                        "Time scheduled LOP auto starter.",
                                        typeof(TimeSchedule).AssemblyQualifiedName);

    IJobCntrlConfigurator setupProcessJobCntrlMaster(IAutoProcessType pType) {
      var starterName= buildStarterName(pType);
      var subject= buildMsgSubject(pType);
      jobCntrlCfg.DefineStarter(buildStarterName(pType), MASTER_PROCESS_STARTER, pType.Description, new Props {
        [MessageSubscription.PROP_MSG_SUBJECT]= subject,
        [MessageSubscription.PROP_RET_RESULT]= true
      });
      log.LogDebug("Starter {name} listening on subject {subj} for process {op} defined.", starterName, subject, pType.Name);

      Type autoJobType= typeof(Job.AutoProcessJob<,>).MakeGenericType(pType.MsgType, pType.ResultType);
      var autoMaster = buildAutoMasterName(buildAutomationName(pType));
      jobCntrlCfg.DefineMasterJob(autoMaster, "LOP automation job", autoJobType.AssemblyQualifiedName, new Props {
        [BaseJob.PROP_LOGLEVEL]= "Debug",
        [Job.AutoProcessJob.PROP_PTYPE]= pType
      });
      log.LogDebug("Automation job {name} for process {prcs} defined.", autoMaster, pType.Name);
      return jobCntrlCfg;
    }

    static Dictionary<string, IAutoProcessType> setupNamedPTypes(IEnumerable<IAutoProcessType> pTypes) {
      try {
        var namedPTypes = pTypes.ToDictionary(p => p.Name);
        log.LogDebug("{cnt} process types discovered from service provider.", namedPTypes.Count);
        return namedPTypes;
      }
      catch (ArgumentException e) { throw new AutoProcessException("Duplicate process type name.", e); }
    }

    const string SUBJECT_PFX= "BPA.";
    const string STARTER_SFX= "-Starter";
    const string AUTO_SFX= "-Automation";
    const string CHAIN_SFX= "-Sequel";
    const char CHAIN_DELIM= '>';
    const string SCHEDULE_SFX= "-Schedule";
    const string MASTER_AUTOJOB_SFX= ":MasterAutoJob";
    const string AUTOJOB_SFX= ":AutoJob";
    const string AUTO_CHAINEDJOB_SFX= AUTO_SFX + AUTOJOB_SFX;
    const string PROCEDURE_DELIM= "-=>";

    static string buildMsgSubject(IAutoProcessType pt) => SUBJECT_PFX + pt.Name;
    static string? pTypeFromMsgSubject(string subject) =>   subject.StartsWith(SUBJECT_PFX, StringComparison.Ordinal)
                                                          ? subject[SUBJECT_PFX.Length..]
                                                          : null;
    static string buildStarterName(string pType) => pType + STARTER_SFX;
    static string buildStarterName(IAutoProcessType pt) => buildStarterName(pt.Name);
    static string buildProcedureJobNamePfx(IAutoProcessType pt) => pt.Name + PROCEDURE_DELIM;
    static string buildProcedureJobName(IAutoProcedureDescriptor pd) => buildProcedureJobNamePfx(pd.ProcessType) + pd.Name;
    static string buildAutomationName(IAutoProcessType pt) => pt.Name + AUTO_SFX;
    static string buildAutoMasterName(string autoName) => autoName + MASTER_AUTOJOB_SFX;
    static string buildAutoJobName(string starterName, string? autoName= null)
      => $"{starterName}{(string.IsNullOrEmpty(autoName) ? string.Empty : (CHAIN_DELIM+autoName))}{AUTOJOB_SFX}";
    static string buildFollowupStarterName(IAutoProcessType pt) => pt.Name + CHAIN_SFX;
    static string buildScheduledStarterName(IAutoProcessType pt, string scheduleId) => $"{pt.Name}@{scheduleId}{SCHEDULE_SFX}";

  }
}
