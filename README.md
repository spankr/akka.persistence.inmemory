# Akka.Persistence.InMemory

This library contains in-memory persistence actors that have their underlying collections exposed for unit testing purposes.
It is based off (heavily, I might add) of the [MongoDB persistence library](https://github.com/akkadotnet/Akka.Persistence.MongoDB).

**DISCLAIMER:** This project is by no means sanctioned by the Akka.NET community. I am
using the akka.* namespace strictly in the hopes that it will be easier to merge into a
proper akka.net solution.


**DISCLAIMER:** My exposure to Akka persistence is about a week at the time of writing this so, this likely isn't the ideal solution.

### Setup

To activate the journal plugin, add the following lines to actor system configuration file:

```
akka.persistence.journal.plugin = "akka.persistence.journal.inmemory"
akka.persistence.journal.inmemory.collection = "<journal collection>"
akka.persistence.journal.inmemory.metadata-collection = "<metadata collection>"
```

Similar configuration may be used to setup a snapshot store:

```
akka.persistence.snapshot-store.plugin = "akka.persistence.snapshot-store.inmemory"
akka.persistence.snapshot-store.inmemory.collection = "<snapshot-store collection>"
```

Remember that connection string must be provided separately to Journal and Snapshot Store.

### Usage

Take a look at the Akka.Persistence.InMemory.Tests project to see how these can be used with the [Akka TestKit](https://petabridge.com/blog/how-to-unit-test-akkadotnet-actors-akka-testkit/).
