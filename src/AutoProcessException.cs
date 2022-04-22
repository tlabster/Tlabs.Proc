using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Tlabs.Proc.Common;

namespace Tlabs.Proc {

  /// <summary>General automation process exception.</summary>
  public class AutoProcessException : Tlabs.GeneralException {
    /// <summary>Default ctor</summary>
    public AutoProcessException() : this(null) { }

    /// <summary>Ctor from message</summary>
    public AutoProcessException(string? message) : this(message, null) { }

    /// <summary>Ctor from message and inner exception.</summary>
    public AutoProcessException(string? message, Exception? e) : base(message, e) {
    }
  }
  
  /// <summary>Invalid automation process type exception.</summary>
  public class InvalidAutoProcessTypeException : AutoProcessException {
    /// <summary>Ctor from message</summary>
    public InvalidAutoProcessTypeException(string? message) : this(message, null) { }
    /// <summary>Ctor from message and inner exception.</summary>
    public InvalidAutoProcessTypeException(string? msg, Exception? e) : base(msg, e) { }
  }

  /// <summary>Automation process restriction violation exception.</summary>
  public class AutoPrcsRestrictionViolationException : AutoProcessException {
    /// <summary>Ctor from message and inner exception.</summary>
    public AutoPrcsRestrictionViolationException(IAutoProcessType PType, IStatefulMessage? stfMsg)
    : base($"Violation of process {PType.Name} restriction with state '{stfMsg?.StateCtx}'") {
      this.SetMissingTemplateData(this.Message, PType.Name, stfMsg?.StateCtx);
    }
  }

  /// <summary>Automation process execution exception.</summary>
  public class AutoProcessExecutionException : AutoProcessException {
    readonly List<Error> errors= new();
    static readonly Regex r= new Regex("'[^ ]*Exception[^ ]* ", RegexOptions.Compiled);
    readonly string processName;
    /// <summary>Ctor from process <paramref name="name"/>.</summary>
    public AutoProcessExecutionException(string name) { this.processName= name; }
    static readonly char[] crlf= new char[] {'\r', '\n'};
    
    /// <summary>Ctor from process <paramref name="name"/> and <paramref name="e"/>.</summary>
    public AutoProcessExecutionException(string name, Exception e): base(name, e) { this.processName= name; }
    /// <summary>Add procedure error.</summary>
    public string AddProcedureError(string procedureName, string? message, Exception? ex= null) {
      var msg= ex?.ResolvedMsgTemplate();
      if (null == msg) {
        msg= message ?? string.Empty;
        var idx= msg.IndexOfAny(crlf);
        if (idx > 0) msg= msg.Substring(0, idx);    //strip off stacktrace
        msg= r.Replace(msg, ""); //Try to remove exception name
      }

      var err= new Error(Msg: $"Procedure {procedureName} has failed: {msg}", Ex: ex ?? this.InnerException);
      errors.Add(err);
      return err.Msg;
    }
    
    ///<inheritdoc/>
    public override string Message { 
      get {
        var err= errors.FirstOrDefault(e => e.Ex != null)?.Ex;
        var msgTemplate= err?.MsgTemplate();
        if (string.IsNullOrEmpty(msgTemplate))
          return err?.Message ?? string.Empty;
        return this.SetTemplateData(msgTemplate, err.TemplateData()?.Values.ToArray()).ResolvedMsgTemplate();
      }
    }

    record Error(string Msg, Exception? Ex);
  }
}
