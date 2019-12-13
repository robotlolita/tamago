### Layers and guarantees

  - [**Total**](./layer-total.md)
    -- Provides ways of abstracting data and transforming it, but not abstracting over transformations. Guarantees that all computations written in the layer terminate, but makes no guarantees about resource usage. Does not include any form of recursion, but co-recursive operations are supported, as long as they operate on finite data.

  - **Core**
    -- Depends on **Total**. Provides ways of abstracting over data transformations. Guarantees that all abstractions are deterministic. Does not include effects or mutable state, thus can be considered pure.

  - **Cooperative**
    -- Depends on **Core**. Provides ways of cooperatively interleaving computational processes. Guarantees that all interleavings are deterministic. Does not include effects or mutable state, thus can be considered pure (despite interleaved processes).

  - **Effect**
    -- Depends on **Cooperative**. Provides ways of interacting with the outside world through effects and co-effects. Guarantees that all effects are controllable, but makes no guarantees over external handlers. 

  - **Module**
    -- Depends on **Core**. Provides ways of describing boundaries, scopes, and trust. Guarantees that subprograms are not able to do anything that its parent program does not allow it to, but does not bound time and memory in any way.

  - **External**
    -- Depends on **Module**. Provides ways of interacting with external languages. Makes no guarantees about the semantics of this interaction for now. Programs using **External** are unpredictable.

  - **Safe-data**
    -- Depends on **Total**. Provides first-order contracts for data structures, but no abstractions over contracts. Can make no guarantees about resource usage.

  - **Safe**
    -- Depends on **Safe-data** and **Core**. Provides higher-order contracts and abstractions over contracts. Can make no guarantees about resource usage.

  - **Meta**
    -- Depends on **Total**. Provides ways of attaching meta-data to computational descriptions. Descriptions are visible to the entire application.

  - **Macro**
    -- Depends on **Meta**. Provides ways of abstracting over computational descriptions. Macro boundaries can be configured with the same security tools of **Module**s, if available.

  - **Distribute**
    -- Depends on **Module** and **Effect**. Provides ways of performing distributed computations with data mobility. Makes no guarantees about the correctness of computations on external nodes.

  - **Distribute-compute**
    -- Depends on **Distribute**. Provides ways of performing distributed computations with code mobility. Makes no guarantees about the correctness of computations when executed on external nodes.
