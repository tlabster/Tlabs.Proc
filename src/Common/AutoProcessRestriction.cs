using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tlabs.Proc.Common {

  ///<summary>Process message with state information to be used with a <see cref="AutoProcessRestriction"/>.</summary>
  public interface IStatefulMessage {

    ///<summary>Restriction state context information.</summary>
    string? StateCtx { get; }
  }

  ///<summary>Process execution restrictions.</summary>
  public class AutoProcessRestriction {
    //TODO: Add support for expression based restrictions.
    const string WBND= "\\b";
    static readonly Regex STATE_PAT= new Regex(@"(\|*" +WBND+ @"(\w+)"  +WBND+ ").?", RegexOptions.Singleline | RegexOptions.Compiled);
    static readonly StateTokenComparer stateTokenComparer= new();
    readonly Regex tokPattern;

    ///<summary>Explicit convertion from string into <see cref="AutoProcessRestriction"/>.</summary>
    public static explicit operator AutoProcessRestriction(string ctxState) => new AutoProcessRestriction(ctxState);

    ///<summary>Explicit convertion from string array into <see cref="AutoProcessRestriction"/>.</summary>
    public static explicit operator AutoProcessRestriction(string[] ctxStates) => new AutoProcessRestriction(ctxStates);

    ///<summary>Ctor from <paramref name="stateTokens"/>.</summary>
    public AutoProcessRestriction(params string[]? stateTokens) {
      if (0 == stateTokens?.Length) throw new ArgumentNullException(nameof(stateTokens));
      if (1 == stateTokens!.Length) stateTokens= stateTokens[0].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
      var pat= string.Join('|', stateTokens.Select(s => "\\b" + s.Trim() + "\\b"));
      this.tokPattern= new Regex(pat, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);
      if (!stateTokens.SequenceEqual(RstrictedStates, stateTokenComparer))
        throw new ArgumentException("Invalid state value", nameof(stateTokens));
    }

    ///<summary>True if <paramref name="ctxMsg"/> is restricted by this <see cref="AutoProcessRestriction"/>.</summary>
    public bool IsRestricted(IStatefulMessage? ctxMsg) => null != ctxMsg && tokPattern.IsMatch(ctxMsg.StateCtx ?? string.Empty);

    ///<summary>List of restricted state tokens.</summary>
    public List<string> RstrictedStates => STATE_PAT.Matches(tokPattern.ToString())
                                                    .Select(match => match.Groups[2].Value)
                                                    .ToList();

    ///<summary>Comma separated list of restricted state tokens.</summary>
    public override string ToString() => string.Join(", ", RstrictedStates);

    private class StateTokenComparer : IEqualityComparer<string> {
      public bool Equals(string? x, string? y) => StringComparer.Ordinal.Equals(x?.Trim(), y?.Trim());
      public int GetHashCode(string obj) => StringComparer.Ordinal.GetHashCode(obj);
    }
  }

}