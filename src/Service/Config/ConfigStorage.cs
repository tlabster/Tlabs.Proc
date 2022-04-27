using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Tlabs.Data;

namespace Tlabs.Proc.Service.Config {

  /*  Static storage helper class
   */
  internal static class ConfigStorage {
    private static void With(Action<IDataStore> call) {
      Tlabs.App.WithServiceScope(svcProv => {
        call(svcProv.GetRequiredService<IDataStore>());
      });
    }

    public static Data.AutoProcessCfgData Load() {
      Data.AutoProcessCfgData? cfgData= null;
      With(store => {
        cfgData= new Data.AutoProcessCfgData {
          PTypes=         store.UntrackedQuery<Data.AutoProcessCfgData.ProcessTypeData>()
                               .ToList(),
          Procedures=     store.UntrackedQuery<Data.AutoProcessCfgData.ProcedureData>()
                               .LoadRelated(store, p => p.Properties)
                               .ToList(),
          CntrlSchedules= store.UntrackedQuery<Data.AutoProcessCfgData.ScheduleData>()
                               .LoadRelated(store, t => t.MsgProps)
                               .ToList(),
          CntrlSequels=   store.UntrackedQuery<Data.AutoProcessCfgData.SequelData>()
                               .ToList()
        };
      });
      if (null == cfgData) throw new AutoProcessException("Error loading configuration.");
      return cfgData;
    }

    public static void Save(Data.AutoProcessCfgData cfgData) => With(store => {
      cleanUp(store);

      //insert new config
      foreach(var ent in cfgData.PTypes) {
        store.Insert(ent);
      }
      foreach (var ent in cfgData.Procedures) {
        store.Insert(ent);
        foreach (var prop in ent.Properties) store.Insert(prop);
      }
      foreach (var ent in cfgData.CntrlSchedules) {
        store.Insert(ent);
        foreach (var prop in ent.MsgProps) store.Insert(prop);
      }
      foreach (var ent in cfgData.CntrlSequels) {
        store.Insert(ent);
      }
      store.CommitChanges();
    });

    public static void CleanUp() => With(store => {
      cleanUp(store);
      store.CommitChanges();
    });

    private static void cleanUp(IDataStore store) {
      foreach (var ent in store.UntrackedQuery<Data.AutoProcessCfgData.ProcessTypeData>())
        store.Delete(ent);
      foreach (var ent in store.UntrackedQuery<Data.AutoProcessCfgData.ProcedureData>())
        store.Delete(ent);
      foreach (var ent in store.UntrackedQuery<Data.AutoProcessCfgData.ScheduleData>())
        store.Delete(ent);
      foreach (var ent in store.UntrackedQuery<Data.AutoProcessCfgData.SequelData>())
        store.Delete(ent);
    }
  }
}