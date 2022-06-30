# Device Twin Schema
This document explains the used schema for the poolboy device twin

## Properties
### Desired properties
|Property| Description |
|---|---|---|
|patchId|property used to check if the desired properties are acknowleged| 
|poolPumpConfig| Configuration for the pool pump  | 
|chlorinePumpConfig| Configuration for the clorine pump |

### Reported properties
|Property| Description | Example value|
|---|---|---|
|lastPatchId|property used to check if the desired properties are acknowleged|  1 |
|poolPumpStatus| Status of the pool pump  | ```{ active: true}```|
|chlorinePumpStatus| Status of the clorine pump|```{ active:true, runId: 1, startedAt: 1651754852  }```|

## Json Schemas 

### patchId
number

### poolPumpConfig Schema
|Property| Type | Description | Example value|
|---|---|---|---|
|enabled| bool | pool pump enabled, used for stopping the pump at any time|  true/false |
|timeslots| array |Timeslots for enabling pool pump | |

### Timeslot Schema
|Property| Type | Description | Example value|
|---|---|---|---|
|startTime| string |Time when the pump starts (format hh:mm:ss) |10:12:59|
|stopTime|string |Time when the pump stops (format hh:mm:ss) |10:20:00|

### chlorinePumpConfig Schema
|Property| Type |Description | Example value|
|---|---|---|---|
|enabled| bool | pool pump enabled, used for stopping the pump at any time|  true/false |
|runId| number | current run id |1|
|runtime| number| Seconds how long the pump should run |3600|

### lastPatchId
number

### poolPump Schema
|Property| Type | Description | Example value|
|---|---|---|---|
|active| bool | pump currently active or not|  true/false |

### chlorinePump Schema
|Property| Type | Description | Example value|
|---|---|---|---|
|active| bool | pump currently active or not|  true/false |
|runId| number | current run id|  1 |
|startedAt| number| Unix timestamp when the run started|  1651754852 |
