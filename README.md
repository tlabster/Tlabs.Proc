# Tlabs.Proc

#### A *business* process automation framework

## PROCESS AUTOMATION

Business administration defines a *BUSINESS PROCESS* as a set of related, structured activities to be carried out in a coordinated sequence to jointly produce a business goal (typically a service or product).

> NOTE: A **process** is a component of actions for a defined activity whilst **operation** is a multitude of processes and controls to ensure the delivery of something.”

In this context of BUSINESS PROCESS AUTOMATION (BPA) a `auto. process` pepresents the abstract notion of a (business) process with the formal signature of an asynchronous procedure of a computer program.  
A `auto. process` is triggered or activated from an event with an optional input message. The asynchronous result of the `auto. process` could be used as an event to invoke another process. Thus `auto. processes` are typically forming a process chain by comprising a set of sequential sub-processes or tasks with alternative paths, depending on certain conditions as applicable, performed to achieve a given objective or produce given outputs. Each process has one or more needed inputs. The inputs and outputs may be received from, or sent to other `auto. processes`.  
The interface of `auto. process` is technically implemented with `auto. procedures`. Business Administration defines a [(business) procedure](https://en.wikipedia.org/wiki/Procedure_(business)) as a detailed instruction on how to execute one or more activities of a business process. Thus, translated into computing a procedure can be seen as (one of possible multiple alternative) programmatic implementation of the abstract business process.


### Configurable Automation
A typicall solution's BPA module supports a highly flexible control of
* What event triggers which `auto. process`
* Which registered `auto. procedure(s)` are actually implementing a `auto. process` (to allow fine grained customization of functionality)
* Whether and how `auto. processes` are chained together
* When time-controlled events are about to occur (and then trigger any `auto. processes`)
All these details of the actual `auto. process` execution can be controlled by means of configuration that is being applied during runtime (i.e. changes of the configuration are applied in real time).

### Automation Process Activity Monitoring
The BPA module provides an detailed internal high performance `auto. process` status tracking system that can be utilized for various monitoring (e.g. BAM) and auditing tasks.


## .NET version dependency
*	`2.1.*` .NET 6
*	`2.2.*` .NET 8
