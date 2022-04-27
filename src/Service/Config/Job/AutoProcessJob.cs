using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern.Starter;

namespace Tlabs.Proc.Service.Config.Job {

  ///<summary>Automation process job .</summary>
  public abstract class AutoProcessJob : BaseJob {
    ///<summary>Message run-propery name.</summary>
    public const string PROP_PROCESS_MSG= "Process-Msg";
    ///<summary>Process result property name.</summary>
    public const string PROP_RESULT= "Process-Res";
    ///<summary>Suppress result property name.</summary>
    public const string PROP_NO_RESULT= "No-Result";
    ///<summary>Message run-propery name.</summary>
    public const string PROP_PTYPE= "ProcessType";

    ///<inheritdoc/>
    protected override IJob InternalInit() => this;
  }

  ///<summary>Generic LOP automation job .</summary>
  public class AutoProcessJob<TMsg, TRes> : AutoProcessJob where TMsg : class where TRes : class {
    static readonly ILogger logger= Tlabs.App.Logger<AutoProcessJob>();
    readonly IProcessAutomation pauto;
    ///<summary>Ctor from <paramref name="pauto"/>.</summary>
    public AutoProcessJob(IProcessAutomation pauto) { this.pauto= pauto; }

    ///<inheritdoc/>
    protected override IJobResult InternalRun(IReadOnlyDictionary<string, object> runProperties) {
      var name= this.Name;

      if (  !runProperties.TryGetValue(PROP_PTYPE, out var o)
          || o is not IAutoProcessType pType) throw new AutoProcessException($"Invalid property {nameof(AutoProcessJob)}.{PROP_PTYPE}: {o}");

      string propName;
      if (   runProperties.TryGetValue(Chained.RPROP_PREVIOUS_RESULTS, out o)
          && o is IReadOnlyDictionary<string, object> prevRunProps) {
        runProperties= prevRunProps;
        o= runProperties[propName= PROP_RESULT];
      }
      else o= runProperties[propName= PROP_PROCESS_MSG];
      if (o is not TMsg msg) throw new AutoProcessException($"Invalid property {nameof(AutoProcessJob)}.{propName}: {o}");

      return CreateAsyncResult(pauto.ExecuteProcess<TMsg, TRes>(pType, msg), processRes =>
          ConfigProperties.GetBool(this.Properties, PROP_NO_RESULT, false)
        ? CreateResult(true)   //suppress result
        : CreateResult(new Dictionary<string, object> { [PROP_RESULT]= processRes })
      );
    }
    ///<inheritdoc/>
    protected override void DoDispose(bool disposing) {
      if (!disposing) return;
    }

  }

}
