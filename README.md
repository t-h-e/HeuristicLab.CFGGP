# HeuristicLab.CFGGP

### Description

Provides Context Free Grammar Problems for HeuristicLab

A given BNF grammar is transformed in a SymbolicExpressionGrammar to create GP trees.

Also includes all problem instances from ["General Program Synthesis Benchmark Suite"](http://dl.acm.org/citation.cfm?id=2754769) (described in more detail in the technical report: [Detailed Problem Descriptions for General Program Synthesis Benchmark Suite](https://web.cs.umass.edu/publication/docs/2015/UM-CS-2015-006.pdf))

### How to use

1. If you want to build HeursiticLab from scratch
 1. Clone [HeuristicLab](https://github.com/HeuristicLab/HeuristicLab)
 2. Build "HeuristicLab.ExtLibs.sln" and then "HeuristicLab 3.3.sln"
 3. Clone HeuristicLab.CFGGP next to HeursticLab
 4. Open and build "HeuristicLab.CFGGP.sln" solution
 5. Binaries will automatically be copied to "HeuristicLab/bin"

2. If you an existing version of HeuristicLab
 1. Clone HeuristicLab.CFGGP
 2. Open and build "HeuristicLab.CFGGP.sln" solution
 3. A folder "HeuristicLab/bin" will be created outside of the repository, which contains the binaries
 4. Copy the binaries in you HeursiticLab version 

A new Problem "Context Free Grammar Problem" will be available in the "New Item" Dialog under Problems -> Genetic Programming.