using Xunit;

using Tlabs.Core;
using Tlabs.Proc.Common;
using System.Collections.Generic;

namespace Tlabs.Proc.Service.Config.Job.Test {

  public class JobCfgTest {
    const string MSG= "Msg";
    static readonly TstProcessMsg.PType tstPType= new TstProcessMsg.PType();
    static readonly string tstName= tstPType.Name;
    static readonly TstProcedure.Descriptor tstProcDesc= new (tstPType);

    [Fact]
    public void SubjectTest() {
      Assert.Equal(nameof(TstProcessMsg)[..^TstProcessMsg.PType.MSG_SUFFIX.Length], tstName);

      var subj= tstPType.MsgSubject();
      Assert.True(subj.StartsWith(JobCfg.SUBJECT_PFX));
      Assert.Equal(tstName, JobCfg.ProcessFromMsgSubject(subj));
      Assert.Throws<AutoProcessException>(() => JobCfg.ProcessFromMsgSubject("--BAD--"));
    }

    [Fact]
    public void StarterTest() {
      var starter= tstPType.StarterName();
      Assert.True(starter.EndsWith(JobCfg.STARTER_SFX));

      var seqStarter= tstPType.SequelStarterName();
      Assert.EndsWith(JobCfg.SEQUEL_SFX, seqStarter);
      Assert.Equal(tstName, JobCfg.ProcessFromSequelStarter(seqStarter));
      Assert.Throws<AutoProcessException>(() => JobCfg.ProcessFromSequelStarter("--BAD--"));

      const string TSTSCHID= "tstschedule-ID";
      var schStarter= tstPType.ScheduledStarterName(TSTSCHID);
      Assert.Equal(tstName, JobCfg.ProcessFromScheduledStarter(schStarter));
      Assert.Throws<AutoProcessException>(() => JobCfg.ProcessFromScheduledStarter("--@BAD--"));
    }

    [Fact]
    public void JobTest() {
      var job= tstProcDesc.JobName();
      Assert.Contains(tstName, job);
      Assert.EndsWith(tstProcDesc.Name, job);

      var cntrlName= tstPType.AutoCntrlName();
      Assert.EndsWith(JobCfg.CNTRL_SFX, cntrlName);
      Assert.EndsWith(JobCfg.MASTER_AUTOJOB_SFX, JobCfg.MasterCntrlJobName(cntrlName));
      
      var starter= tstPType.StarterName();
      var cntrlJob= JobCfg.CntrlJobName(starter);
      Assert.Contains(starter, cntrlJob);
      Assert.EndsWith(JobCfg.AUTOJOB_SFX, cntrlJob);
      Assert.DoesNotContain(cntrlName, cntrlJob);

      cntrlJob= JobCfg.CntrlJobName(starter, cntrlName);
      Assert.Contains(cntrlName, cntrlJob);
      Assert.Equal(tstName, JobCfg.ProcessFromCntrlJob(cntrlJob));
    }

    public class TstProcessMsg {
      public class PType : AutoProcessType<TstProcessMsg, VoidUnit> {
        public PType() : base("Test process description...") { }
      }
    }

    public class TstProcedure : IAutoProcedure<TstProcessMsg, VoidUnit> {
      public class Descriptor : AutoProcedurDescriptor<TstProcedure, TstProcessMsg, VoidUnit> {
        public Descriptor(AutoProcessType<TstProcessMsg, VoidUnit> prcsType) : base(prcsType) {
          Name= nameof(TstProcedure);
          Description= $"{Name} description";
        }
      }
      public IAutoProcedure InitConfiguration(IReadOnlyDictionary<string, object> @params) {
        throw new System.NotImplementedException();
      }
      public VoidUnit Execute(TstProcessMsg msg) {
        throw new System.NotImplementedException();
      }
    }
  }

}