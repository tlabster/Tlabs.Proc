using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Tlabs.JobCntrl.Model;
using Tlabs.JobCntrl.Model.Intern;
using Tlabs.Msg;

namespace Tlabs.Proc.Service.Config {

  ///<summary>Process execution agent.</summary>
  ///<remarks><para>This implementation is publishing the execution request with a <see cref="IMessageBroker"/> where 
  ///a <see cref="Tlabs.JobCntrl.Model.Intern.Starter.MessageSubscription"/> starter is configured to subscribe on the
  ///process specific message subject....</para>
  ///The <see cref="IStarterCompletion"/> returned is then being unwrapped into the TRes...
  ///</remarks>
  public class AutoProcessJobAgent : IAutoProcessExecAgent {
    static readonly ILogger log = App.Logger<AutoProcessJobAgent>();
    readonly IProcessAutomationConfig config;
    readonly IMessageBroker msgBroker;

    ///<summary>Ctor from DI services.</summary>
    public AutoProcessJobAgent(IProcessAutomationConfig config, IMessageBroker msgBroker) {
      this.config= config;
      this.msgBroker= msgBroker;
    }

    ///<inheritdoc/>
    public async Task<TRes> PublishExecutionRequest<TMsg, TRes>(IAutoProcessType pType, TMsg message, int timeout= 0) where TRes : class {
      AutoProcessExecutionException? errors= null;
      var tsk= config.WithExclusiveAccess(() => {
        try {
          var subject= pType.MsgSubject();
          var jobMsg= new AutomationJobMessage(source: GetType().Name, messageObj: message);
          return msgBroker.PublishRequest<IStarterCompletion>(subject, jobMsg, timeout);
        }
        catch (Exception e) {
          log.LogError(e, "Failed to publish execution request for process '{proc}' ({msg}).", pType.Name, e.Message);
          throw;
        }
      });

      var results= new List<TRes>();
      /* Await & extract completion result:
       */
      var cmplRes= await tsk;
      if (null != cmplRes.JobResults) foreach (var jobRes in cmplRes.JobResults) if (null != jobRes.ResultObjects) {
        
        if (jobRes.IsSuccessful) {  //Aggregate successful procedure results
          if (jobRes.ResultObjects.TryGetValue(Job.AutoProcessJob.PROP_RESULT, out var obj) && obj is TRes rslt)
            results.Add(rslt);
          continue;
        }

        // Aggregate errors:
        if (jobRes.ResultObjects.TryGetValue(JobCntrl.JobCntrlException.JOB_RESULT_KEY, out var o) && o is Exception resX) {
          /* The first procedure exception will be set as inner exception
            * (the rest will be summarized in the exception message)
            */
          errors??= new AutoProcessExecutionException(pType.Name, resX);
          log.LogError("Procedure error: {err}", errors.AddProcedureError(jobRes.JobName, jobRes.Message, resX));
        }
      }
      if (null != errors) throw errors;

      var processRes= results.FirstOrDefault(); // Only return a single result (from the IResultAutoProcedureType)
      if (null == processRes) throw new AutoProcessExecutionException(pType.Name, "No result found");
      return processRes;
    }
  }

}