using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tlabs.JobCntrl;
using Tlabs.JobCntrl.Model;

namespace Tlabs.Proc.Service.Config.Job {

  ///<summary>Generic automation procedure job.</summary>
  public class ProcedureJob<TProc, TMsg, TRes> : AutoProcessJob where TProc : Common.IAutoProcedure<TMsg, TRes> where TMsg : class where TRes : notnull {
    bool noResult;
    Common.IAutoProcedure<TMsg, TRes> procedure;

    ///<summary>Ctor from <paramref name="procedure"/>.</summary>
    ///<remarks>A <paramref name="procedure"/> of <typeparamref name="TProc"/> is required to be configured with the service provider.</remarks>
    public ProcedureJob(TProc procedure) { this.procedure= procedure; }

    ///<inheritdoc/>
    protected override IJob InternalInit() {
      this.noResult= ConfigProperties.GetBool(this.Properties, PROP_NO_RESULT, false);
      procedure= (Common.IAutoProcedure<TMsg, TRes>)procedure.InitConfiguration(this.Properties);
      return this;
    }

    ///<inheritdoc/>
    protected override IJobResult InternalRun(IReadOnlyDictionary<string, object> runProperties) {
      TRes resVal= procedure.Execute((TMsg)runProperties[PROP_PROCESS_MSG]);
      if (noResult) return CreateResult(true);    //no result

      var res= new Dictionary<string, object> {
        [PROP_RESULT]= resVal
      };
      return CreateResult(res);
    }

    ///<inheritdoc/>
    protected override void DoDispose(bool disposing) {
      if (!disposing) return;
      var p= procedure as IDisposable;
      p?.Dispose();
    }

  }

}
