# Wildling #

Wildling is a distributed key/value store inspired by Amazon Dynamo and written in C#. Unlike most other implementations, it is intended for educational purposes and meant for learning and experimenting with distributed computing techniques.

*Disclaimer: Please note this project has not been used in, nor intended for, production purposes.* 

Distributed computing concepts and techniques are wonderfully interesting and useful. However, reading about these concepts only takes you so far -- I've found that implementing a technique in code provides much richer understanding and insight.

I am not an expert at distributed computing and many of these concepts are new to me. I am learning and this is my effort to put these concepts into practice and thought others may find value in this code as well. It is likely there are mistakes and misunderstandings exposed in the code -- if you see a bug or concept that is incorrect, please feel free to let me know.

Many of the concepts assembled in the Dynamo paper are useful on their own. For example, the causality tracking in this project may be useful in places where  you want eventual consistency and pessimistic or optimistic locking would be inconvenient (e.g., disconnected scenarios). So, the code in this project may be useful for other scenarios than just key/values stores.

## What's Implemented ##

Only a small portion of Dynamo is currently implemented, but it's demonstrates some neat concepts:

- Web API for Get/Put operations using self-hosted ASP.NET WebAPI
- Causality tracking using dotted-version vectors (DVV). See [2]
The Amazon Dynamo paper uses vector clocks (if I understand correctly, these would be more accurately referred to as version vectors in today's vocabulary). I preferred server-side identifiers over client-side identifiers, so Wildling uses DVV instead of VV.
- DVV kernel with sync, join, discard, and event operations. See [2]
- Partitioned consistent hash (ring) for partitioning ownership of key-space
- Fixed/hard-coded # of virtual nodes (3, by default)
- Very simple (happy-path) replication to N virtual nodes in preference list.

## What's Not Implemented Yet ##

Basically, everything else, including:

- Improve replication
  - Better failure handling (including, preference list should consist of "available" nodes)
  - Support W (# of writes) and R (# of reads) parameters
- Membership: gossip protocol
- Sloppy Quorum and Hinted hand-off
- Read repair
- Persisting vnode data to disk
- CRDTs?
- MapReduce?
- More...

## Attribution & References ##

This project was inspired by 

1. [Dynamo: Amazon's Highly Available Key-value Store](http://www.allthingsdistributed.com/files/amazon-dynamo-sosp2007.pdf)
2. [Scalable and Accurate Causality Tracking for Eventually Consistent Stores](http://asc.di.fct.unl.pt/~nmp/pubs/dais2014.pdf)
This paper describes dotted-version vectors (DVV) and dotted-version vector sets (DVVS) and the kernel operations for implementing Get/Put operations. This is a fantastic paper with lots of implementation details.
3. [coderoshi/dds](https://github.com/coderoshi/dds "Sample code for Distributed Datastructures talk")
Eric Redmond's presentations (search YouTube) and DDS sample code are truly invaluable for learning about these concepts. I referenced the DDS samples frequently in writing this project. Wildling uses a C# port of the PartitionedConsistentHash class (and unit tests) from Eric's DDS repository. [Any mistakes in translating his ruby class into C# are my own].