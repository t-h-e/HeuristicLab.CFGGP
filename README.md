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

### Publications:

_Stefan Forstenlechner, David Fagan, Miguel Nicolau and Michael O'Neill_
**Extending Program Synthesis Grammars for Grammar-Guided Genetic Programming**
PPSN 2018: 15th International Conference on Parallel Problem Solving from Nature, Springer Verlag, 8–12 Sep. 2017.

_Stefan Forstenlechner, David Fagan, Miguel Nicolau and Michael O'Neill_
**Towards Effective Semantic Operators for Program Synthesis in Genetic Programming**
GECCO ’18: Genetic and Evolutionary Computation Conference, ACM, 15–19 July 2018.

_Stefan Forstenlechner, David Fagan, Miguel Nicolau and Michael O'Neill_
**Towards Understanding and Refining the General Program Synthesis Benchmark Suite with Genetic Programming**
CEC 2018: IEEE Congress on Evolutionary Computation, IEEE, 8–13 July 2018.

_Stefan Forstenlechner, David Fagan, Miguel Nicolau and Michael O'Neill_
**Semantics-based crossover for program synthesis in genetic programming**
Artifcial Evolution, Springer Verlag, 25-27 Oct. 2017.

_Stefan Forstenlechner, David Fagan, Miguel Nicolau and Michael O'Neill_
**A Grammar Design Pattern for Arbitrary Program Synthesis Problems in Genetic Programming**
EuroGP 2017: Proceedings of the 20th European Conference on Genetic Programming, LNCS, Vol. 10196, pp. 262-277, Springer Verlag, 19-21 April 2017.

_Stefan Forstenlechner, Miguel Nicolau, David Fagan and Michael O'Neill_
**Grammar Design for Derivation Tree Based Genetic Programming Systems**
EuroGP 2016: Proceedings of the 19th European Conference on Genetic Programming, LNCS, Vol. 9594, pp. 199-214, Springer Verlag, 30 March-1 April 2016.
