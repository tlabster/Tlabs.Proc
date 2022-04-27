using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Tlabs.Data.Serialize.Json;

namespace Tlabs.Proc.Service.Config {
  using Props= Dictionary<string, object?>;


  ///<summary>Abstract base of auto. LOP</summary>
  internal abstract class ProcessControl : Common.IProcesssControl {
    ///<summary>Ctor from <paramref name="pType"/>.</summary>
    public ProcessControl(IAutoProcessType pType) {
      this.ProcessType= pType;
    }
    ///<inheritdoc/>
    public IAutoProcessType ProcessType { get; }
    ///<inheritdoc/>
    public string Name => ProcessType.Name;
    ///<inheritdoc/>
    public string Description => ProcessType.Description;
  }


  internal class SequelControl : ProcessControl, Common.ISequelControl {
    public static Data.AutoProcessCfgData.SequelData ToEntity(Common.ISequelControl seq) =>
      new Data.AutoProcessCfgData.SequelData {
        Continuation= seq.ProcessType.Name,
        Process= seq.Precursor.Name
      };
    public SequelControl(IAutoProcessType precursor, IAutoProcessType pType, bool enabled) : base(pType) {
      this.Precursor= precursor;
      this.IsEnabled= enabled;
    }
    public SequelControl(IProcessAutomationConfig config, Data.AutoProcessCfgData.SequelData entity)
      : this(config.NamedPTypes[entity.Process ?? "?"],
             config.NamedPTypes[entity.Continuation ?? "?"],
             true) { }
    public IAutoProcessType Precursor { get; }
    public bool IsEnabled { get; }
  }

  internal class TimeScheduleControl : ProcessControl, Common.ITimeScheduleControl {
    // readonly object? message;

    public static Data.AutoProcessCfgData.ScheduleData ToEntity(Common.ITimeScheduleControl tsch) {
      var ent = new Data.AutoProcessCfgData.ScheduleData {
        Process= tsch.ProcessType.Name,
        ScheduleId= tsch.ScheduleId,
        Time= tsch.TimePattern,
      };
      if (null != tsch.Message) {
        /* Convert Message object into AutoProcessCfgData.MessageProp list
         */
        var strm = new MemoryStream();
        var json = JsonFormat.CreateDynSerializer();
        json.WriteObj(strm, tsch.Message);
        strm.Seek(0, SeekOrigin.Begin);
        var dict = (Props)json.LoadObj(strm, typeof(Props));
        ent.MsgProps= dict.Select(p => new Data.AutoProcessCfgData.MessageProp {
          Name= p.Key,
          Value= p.Value?.ToString() ?? "",
          Type= Data.AutoProcessCfgData.PropTypeName(p.Value?.GetType())
        }).ToList();
      }
      return ent;
    }
    public TimeScheduleControl(string Id, string? timePattern, IAutoProcessType pType, object? msgObj) : base(pType) {
      this.ScheduleId= Id;
      if (null == timePattern) throw new ArgumentNullException(nameof(timePattern));
      this.TimePattern= timePattern;
      this.Message= msgObj;
    }
    public TimeScheduleControl(IProcessAutomationConfig config, Data.AutoProcessCfgData.ScheduleData entity)
      : this(entity.ScheduleId ?? "?",
             entity.Time ?? "?",
             config.NamedPTypes[entity.Process ?? "?"],
             null) {
      var json = JsonFormat.CreateDynSerializer();
      var strm = new MemoryStream();
      json.WriteObj(strm, entity.MsgProps.ToDictionary(e => e.Name ?? "?", e => Data.AutoProcessCfgData.ConvertPropType(e.Type, e.Value)));
      strm.Seek(0, SeekOrigin.Begin);
      this.Message= json.LoadObj(strm, this.ProcessType.MsgType);
    }

    public string ScheduleId { get; }
    public string TimePattern { get; }
    public object? Message { get; }
  }
}