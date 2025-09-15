# Ground Truth Curation Flow Chart

- Author: Laura Lund
- Last Updated: 2025-09-15
- Tool: [Mermaid Chart](https://mermaid.js.org/)
- Docs: [Mermaid Syntax](https://docs.mermaidchart.com/mermaid-oss/syntax/flowchart.html#a-node-default)

## Overview

This flow chart describes the logic flow of the ground truth application.

### Ground Truth Definition

```mermaid
flowchart TD
    A([Start])
    A-->B["Create a Ground Truth Definition
    with status New."]
    B-->C{Does it need a/another data query?}
    C-- Yes -->D[Define a data query.]
    D-->C
    C-- No -->E{Does it need a/another context?}
    E-- Yes -->F[Define a context.]
    E-- No -->J["Mark Ground Truth Definition
    as status New, Data Curated."]
    J-->H{"Does the Data Curator
    need to leave a note?"}
    H-- Yes -->I[Add a comment]
    I-->G
    H-- No -->G([End])
    F-->E
```

## Ground Truth Definition Validation Status Updates

```mermaid
flowchart LR
    A([Start])
    A-->B[Review Ground Truth Definition.]
    B-->C{"Are the data query/queries
    and response correct?"}
    C-- Yes -->D["Mark the Ground Truth Definition
    as status Validated."]
    C-- No -->E["Mark the Ground Truth Definition
    as status Revisions Requested and
    add a comment explaining
    what needs to change."]
    C-- Not Sure-->I{Is the user query out of scope?}
    E-->F["Revise the Ground Truth Definition
    based on Review Comment."]
    F-->H["Mark the Ground Truth Definition
    as Revised."]
    H-->C
    I-- Yes -->J["Mark Ground Truth Definition
    as Out of Scope."]
    J-->G
    I-- Not Sure-->K["Mark Ground Truth Definition
    as status Pending and
    leave a comment explaining
    what investigation
    will be done."]
    K-->L[Complete investigation.]
    L-->I
    I-- No -->C
    D-->G([End])
```
