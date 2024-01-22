#pragma warning disable CS1591
using System.Xml.Serialization;

using Tlabs.Data.Serialize.Xml;
using Tlabs.Proc.Common;

namespace Tlabs.Proc.Data.Xml {

  public class AutoProcessCfgDataXmlSchema : XmlFormat<AutoProcessCfgData, AutoProcessCfgDataXmlSchema>.Schema {
    ///<summary>Default ctor</summary>
    public AutoProcessCfgDataXmlSchema() : base() {
      //XML root element: <process-config>
      var xmlAttr= new XmlAttributes();
      xmlAttr.XmlRoot= new XmlRootAttribute("process-config");
      this.Add(typeof(AutoProcessCfgData), xmlAttr);

      //<process-config><procedure>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("procedure"));
      this.Add(typeof(AutoProcessCfgData), nameof(AutoProcessCfgData.Procedures), xmlAttr);

      //<processor name="">...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("name");
      this.Add(typeof(AutoProcessCfgData.ProcedureData), nameof(AutoProcessCfgData.ProcedureData.Name), xmlAttr);

      //<processor result="">...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("result");
      this.Add(typeof(AutoProcessCfgData.ProcedureData), nameof(AutoProcessCfgData.ProcedureData.Result), xmlAttr);

      //<processor><prop>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("prop"));
      this.Add(typeof(AutoProcessCfgData.ProcedureData), nameof(AutoProcessCfgData.ProcedureData.Properties), xmlAttr);

      //<CfgProp name="" >
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("name");
      this.Add(typeof(AutoProcessCfgData.CfgProp), nameof(AutoProcessCfgData.CfgProp.Name), xmlAttr);

      //<CfgProp type="" >
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("type");
      this.Add(typeof(AutoProcessCfgData.CfgProp), nameof(AutoProcessCfgData.CfgProp.Type), xmlAttr);

      //<CfgProp>value...</tier>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlText= new XmlTextAttribute();
      this.Add(typeof(AutoProcessCfgData.CfgProp), nameof(AutoProcessCfgData.CfgProp.Value), xmlAttr);

      //<process-config><auto-schedule>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("auto-schedule"));
      this.Add(typeof(AutoProcessCfgData), nameof(AutoProcessCfgData.CntrlSchedules), xmlAttr);

      //<auto-schedule process="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("process");
      this.Add(typeof(AutoProcessCfgData.ScheduleData), nameof(AutoProcessCfgData.ScheduleData.Process), xmlAttr);

      //<auto-schedule schedule="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("schedule");
      this.Add(typeof(AutoProcessCfgData.ScheduleData), nameof(AutoProcessCfgData.ScheduleData.ScheduleId), xmlAttr);

      //<auto-schedule time="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("time");
      this.Add(typeof(AutoProcessCfgData.ScheduleData), nameof(AutoProcessCfgData.ScheduleData.Time), xmlAttr);

      //<auto-schedule><msg-CfgProp>
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("msg-CfgProp"));
      this.Add(typeof(AutoProcessCfgData.ScheduleData), nameof(AutoProcessCfgData.ScheduleData.MsgProps), xmlAttr);

      //<process-config><auto-SequelData>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("auto-SequelData"));
      this.Add(typeof(AutoProcessCfgData), nameof(AutoProcessCfgData.CntrlSequels), xmlAttr);

      //<auto-SequelData process="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("process");
      this.Add(typeof(AutoProcessCfgData.SequelData), nameof(AutoProcessCfgData.SequelData.Process), xmlAttr);

      //<auto-SequelData follower="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("follower");
      this.Add(typeof(AutoProcessCfgData.SequelData), nameof(AutoProcessCfgData.SequelData.Continuation), xmlAttr);

      //<process-config><process-restrictions>...
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlElements.Add(new XmlElementAttribute("process-restrictions"));
      this.Add(typeof(AutoProcessCfgData), nameof(AutoProcessCfgData.PTypes), xmlAttr);

      //<process-restrictions process="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("process");
      this.Add(typeof(AutoProcessCfgData.ProcessTypeData), nameof(AutoProcessCfgData.ProcessTypeData.PType), xmlAttr);

      //<process-restrictions excludes="">
      xmlAttr= new XmlAttributes();
      xmlAttr.XmlAttribute= new XmlAttributeAttribute("excludes");
      this.Add(typeof(AutoProcessCfgData.ProcessTypeData), nameof(AutoProcessCfgData.ProcessTypeData.RestrictedStates), xmlAttr);
    }
    ///<inheritdoc/>
    public override AutoProcessCfgData? Finished(AutoProcessCfgData? cfg) => Finishing(cfg);

    ///<summary>finishing by adding references from children to parents.</summary>
    public static AutoProcessCfgData? Finishing(AutoProcessCfgData? cfg) {
      if (null == cfg) return cfg;
      foreach (var proc in cfg.Procedures) {
        foreach (var CfgProp in proc.Properties)
          CfgProp.Procedure= proc;
      }
      foreach(var sch in cfg.CntrlSchedules) {
        foreach (var CfgProp in sch.MsgProps)
          CfgProp.TimeSchedule= sch;
      }
      foreach (var pt in cfg.PTypes)
        pt.RestrictedStates= new AutoProcessRestriction(pt.RestrictedStates ?? "").ToString(); //validated process restrictions
      return cfg;
    }
  }
}



