
namespace Tlabs.Proc.Common {

  ///<summary>Process automation control descriptor.</summary>
  public interface IProcesssControl : IAutomationDescriptor {
    ///<summary>Automation controlled process type.</summary>
    IAutoProcessType ProcessType { get; }
  }

  ///<summary>Control description of a sequel process auto. executed once a precursor process has been completed (sucessfully).</summary>
  public interface ISequelControl : IProcesssControl {
    ///<summary>Predecessor LOP to cause automated execution of this LOP once completed (sucessfully).</summary>
    IAutoProcessType Precursor { get; }
    ///<summary>Automation control state.</summary>
    bool IsEnabled { get; }
  }

  ///<summary>Control description of a process auto. executed based on time-schedule.</summary>
  public interface ITimeScheduleControl : IProcesssControl {
    ///<summary>Schedule id (unique).</summary>
    string ScheduleId { get; }
    ///<summary>Time pattern to define the schedule.</summary>
    string TimePattern { get; }
    ///<summary>Process message.</summary>
    object? Message { get; }
  }

}
