
using System.Threading.Tasks;

namespace Tlabs.Proc.Service {

  ///<summary>Interface of an automation process execution broker.</summary>
  public interface IAutoProcessExecAgent {
    ///<summary>Publish a execution request of <paramref name="pType"/> with <paramref name="message"/> to asynchronously return <typeparamref name="TRes"/> with optional <paramref name="timeout"/>).</summary>
    Task<TRes> PublishExecutionRequest<TMsg, TRes>(IAutoProcessType pType,  TMsg message, int timeout= 0) where TRes : class;

  }
}
