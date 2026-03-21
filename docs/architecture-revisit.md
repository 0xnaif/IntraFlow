# Architecture Revisit Notes

## Create Request mixes responsibilities

### Issue
Create POST handles:
- creating draft
- creating + submitting

### Why revisit
Blurs intent of endpoint.

### Current decision
Keep as-is to avoid slowing progress.

### Future direction
Separate:
- CreateDraft
- Submit (existing request)

### Priority
Medium


## Add Auditing to Start Review Action

### Issue


### Why revisit


### Current decision


### Future direction

### Priority
