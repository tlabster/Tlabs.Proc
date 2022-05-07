using System;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tlabs.Config;
using Tlabs.Data.Serialize;
using Tlabs.Data.Serialize.Xml;


namespace Tlabs.Proc.Service {
  ///<summary>Operations service configurator.</summary>
  public class Configurator : IConfigurator<IServiceCollection> {
    readonly ILogger log= Tlabs.App.Logger<Configurator>();

    ///<inheritdoc/>
    public void AddTo(IServiceCollection services, IConfiguration cfg) {
      services.AddSingleton<XmlFormat<Data.AutoProcessCfgData, Data.Xml.AutoProcessCfgDataXmlSchema>>();
      services.AddSingleton<ISerializer<Data.AutoProcessCfgData>, XmlFormat<Data.AutoProcessCfgData, Data.Xml.AutoProcessCfgDataXmlSchema>.Serializer>();
      services.AddSingleton<IProcessAutomationConfig, Config.ProcessAutomationJobConfig>();
      services.AddSingleton<IAutoProcessExecAgent, Config.AutoProcessJobAgent>();
      services.AddSingleton<IProcessAutomation, ProcessAutomation>();
      log.LogInformation("Process automation service configured.");
    }
  }

#if false
  ///<summary>Startup validation configurator.</summary>
  public class StartupValidation : IConfigurator<MiddlewareContext> {
    readonly ILogger log= Tlabs.App.Logger<StartupValidation>();
    ///<inheritdoc/>
    public void AddTo(MiddlewareContext mware, IConfiguration cfg) {
      var pAuto= Tlabs.App.ServiceProv.GetRequiredService<IProcessAutomation>(); //unscoped singleton
      if (!pAuto.AllProcessTypes.Any())
        log.LogWarning("No process (types) configured!");
      foreach (var pt in pAuto.AllProcessTypes) {
        var procs= pAuto.ProcessProcedures(pt);
        var resProcCnt= procs.Count(p => p.IsEnabled && p.HasResult);
        if (0 == resProcCnt) log.LogWarning("Automation process {p} has no result processor. (Invocations will return null.)", pt.Name);
        if (resProcCnt > 1) log.LogWarning("Automation process {p} has multiple ({lst}) result processors. (Invocation result is unpredictable...)",
                                           pt.Name, String.Join(", ", procs.Select(p => p.Descriptor.Name)));
      }
    }
  }
#endif
}
