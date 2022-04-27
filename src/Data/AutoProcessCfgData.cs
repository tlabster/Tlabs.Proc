#pragma warning disable CS1591
using System;
using System.Linq;
using System.Collections.Generic;

using Tlabs.Data.Entity.Intern;

namespace Tlabs.Proc.Data {

  public class AutoProcessCfgData {
    List<ProcessTypeData>? pTypes;
    List<ProcedureData>? procedures;
    List<ScheduleData>? cntrlSchedules;
    List<SequelData>? cntrlSequels;

    public List<ProcessTypeData> PTypes { get => pTypes??= new (); set => pTypes= value; }
    public List<ProcedureData> Procedures { get => procedures??= new(); set => procedures= value; }
    public List<ScheduleData> CntrlSchedules { get => cntrlSchedules??= new(); set => cntrlSchedules= value; }
    public List<SequelData> CntrlSequels { get => cntrlSequels??= new(); set => cntrlSequels= value; }

    public class ProcessTypeData : BaseEntity {
      public string? PType { get; set; }
      public string? RestrictedStates { get; set; }
    }

    public class ProcedureData : BaseEntity {
      public ProcedureData() { Properties= new List<ProcedureProp>(); }
      public string? Name { get; set; }
      public bool? Result { get; set; }
      public List<ProcedureProp> Properties { get; set; }
    }

    public class ProcedureProp : CfgProp {
      public ProcedureData? Procedure { get; set; }
    }

    public class ScheduleData : BaseEntity {
      public ScheduleData() { MsgProps= new List<MessageProp>(); }
      public string? Process { get; set; }
      public string? ScheduleId { get; set; }
      public string? Time { get; set; }
      public List<MessageProp> MsgProps { get; set; }
    }

    public class MessageProp : CfgProp {
      public ScheduleData? TimeSchedule { get; set; }
    }

    public class SequelData : BaseEntity {
      public string? Process { get; set; }
      public string? Continuation { get; set; }
    }

    public class CfgProp : BaseEntity {
      public string? Name { get; set; }
      public string? Type { get; set; }
      public string? Value { get; set; }
    }

    public static string PropTypeName(Type? type) {
      if (typeName.TryGetValue(type ?? typeof(Nullable), out var tname)) return tname;
      return "NULL";
    }
    public static object? ConvertPropType(string? typeName, string? val) {
      if (converter.TryGetValue(typeName??"", out var cv) && cv != null) return cv(val ?? "");
      return null;
    }

    static readonly Dictionary<string, Func<string, object?>> converter= new Dictionary<string, Func<string, object?>>(StringComparer.InvariantCultureIgnoreCase) {
      ["TEXT"]= t => t,
      ["NUMBER"]= t => Convert.ToDecimal(t, App.DfltFormat),
      ["INTEGER"]= t => Convert.ToInt32(t, App.DfltFormat),
      ["BOOLEAN"]= t => Convert.ToBoolean(t, App.DfltFormat),
      ["NULL"]= t => null
    };
    static readonly Dictionary<Type, string> typeName= new Dictionary<Type, string> {
      [typeof(string)]= "TEXT",
      [typeof(decimal)]= "NUMBER",
      [typeof(double)]= "NUMBER",
      [typeof(int)]= "INTEGER",
      [typeof(Int64)]= "INTEGER",
      [typeof(bool)]= "BOOLEAN",
      [typeof(DateTime)]= "TEXT",
      [typeof(Nullable)]= "NULL"
    };
  }

}
