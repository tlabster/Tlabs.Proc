#pragma warning disable CS1591
using System;
using System.Collections.Generic;

using Tlabs.Data.Entity.Intern;

namespace Tlabs.Proc.Data {

  public class AutoProcessCfgData {

    public List<ProcessTypeData> PTypes { get; set; }
    public List<ProcedureData> Procedures { get; set; }
    public List<TimeSchedule> AutoSchedules { get; set; }
    public List<Sequel> AutoSequels { get; set; }

    public class ProcessTypeData : BaseEntity {
      public string? PType { get; set; }
      public string? ExcludedStates { get; set; }
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

    public class TimeSchedule : BaseEntity {
      public TimeSchedule() { MsgProps= new List<MessageProp>(); }
      public string? PType { get; set; }
      public string? Schedule { get; set; }
      public string? Time { get; set; }
      public List<MessageProp> MsgProps { get; set; }
    }

    public class MessageProp : CfgProp {
      public TimeSchedule? AutoSchedule { get; set; }
    }

    public class Sequel : BaseEntity {
      public string? PType { get; set; }
      public string? Continuation { get; set; }
    }

    public class CfgProp : BaseEntity {
      public string? Name { get; set; }
      public string? Type { get; set; }
      public string? Value { get; set; }
    }

    public static string PropTypeName(Type type) {
      if (typeName.TryGetValue(type, out var tname)) return tname;
      return "NULL";
    }
    public static object? ConvertPropType(string typeName, string val) {
      if (converter.TryGetValue(typeName, out var cv) && cv != null) return cv(val);
      return null;
    }

    static Dictionary<string, Func<string, object?>> converter= new Dictionary<string, Func<string, object?>>(StringComparer.InvariantCultureIgnoreCase) {
      ["TEXT"]= t => t,
      ["NUMBER"]= t => Convert.ToDecimal(t),
      ["INTEGER"]= t => Convert.ToInt32(t),
      ["BOOLEAN"]= t => Convert.ToBoolean(t),
      ["NULL"]= t => null
    };
    static Dictionary<Type, string> typeName= new Dictionary<Type, string> {
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
