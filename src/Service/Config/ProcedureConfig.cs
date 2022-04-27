using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Tlabs.Data.Serialize.Json;
using Tlabs.JobCntrl;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Service.Config {
  using Props= Dictionary<string, object?>;

  internal class ProcedureConfig : IProcedureConfig {
    static readonly IReadOnlyDictionary<string, object?> EMPTY_PROPS= new Dictionary<string, object?>(); 
    public static Data.AutoProcessCfgData.ProcedureData ToEntity(IProcedureConfig procCfg) {
      if (!procCfg.IsEnabled) throw new ArgumentException("Disabled procedure must not be a persisted as entity.");
      var json= JsonFormat.CreateDynSerializer();
      var strm= new MemoryStream();
      var procData= new Data.AutoProcessCfgData.ProcedureData {
        Name= procCfg.Descriptor.Name,
        Result= procCfg.HasResult
      };
      if (null != procCfg.ProcedureParams) {
        /* Convert ProcedureParams into LopConfig.ProcessProp list
         */
        json.WriteObj(strm, procCfg.ProcedureParams);
        strm.Seek(0, SeekOrigin.Begin);
        var dict= json.LoadObj(strm, typeof(Props)) as Props ?? EMPTY_PROPS;
        procData.Properties= dict.Select(p => new Data.AutoProcessCfgData.ProcedureProp {
          Name= p.Key,
          Value= p.Value?.ToString(),
          Type= Data.AutoProcessCfgData.PropTypeName(p.Value?.GetType()),
          Procedure= procData
        }).ToList();
      }
      return procData;
    }
    public ProcedureConfig(IAutoProcedureDescriptor pd, bool enabled, bool resultReturning, IReadOnlyDictionary<string, object?> @params) {
      this.Descriptor= pd;
      this.IsEnabled= enabled;
      this.HasResult= resultReturning;
      this.ProcedureParams= @params;
    }
    public ProcedureConfig(IAutoProcedureDescriptor pd, bool enabled, IReadOnlyDictionary<string, object?>? @params= null) {
      this.Descriptor= pd;
      this.IsEnabled= enabled;
      this.ProcedureParams= @params ?? EMPTY_PROPS;
      this.HasResult= !ConfigProperties.GetBool(this.ProcedureParams, Job.AutoProcessJob.PROP_NO_RESULT, false);
    }
    public ProcedureConfig(IProcessAutomationConfig config, Data.AutoProcessCfgData.ProcedureData entity) {
      var props= new List<Data.AutoProcessCfgData.ProcedureProp>(entity.Properties ?? new List<Data.AutoProcessCfgData.ProcedureProp>());
      this.Descriptor= config.ProcedureDescriptor(entity.Name ?? "?");
      this.IsEnabled= true;
      this.HasResult= entity.Result ?? false;
      this.ProcedureParams= props.ToDictionary(p => p.Name ??"",
                                               p => Data.AutoProcessCfgData.ConvertPropType(p.Type, p.Value));
    }

    public IAutoProcedureDescriptor Descriptor { get; }
    public bool IsEnabled { get; }
    public bool HasResult { get; }
    public IReadOnlyDictionary<string, object?> ProcedureParams { get; }
  }

}
