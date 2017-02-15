using System.Collections.Generic;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Options {
    public IEnumerable<DataType> Input { get; }
    public IEnumerable<DataType> Output { get; }
    public HashSet<DataType> Datatypes { get; }

    public bool Tree { get; }
    public int NumberOfInputVariables { get; }

    public Options(IEnumerable<DataType> input, IEnumerable<DataType> output, HashSet<DataType> datatypes, bool tree, int numberOfInputVariables) {
      Input = input;
      Output = output;
      datatypes.UnionWith(input);
      datatypes.UnionWith(output);
      Datatypes = datatypes;
      Tree = tree;
      NumberOfInputVariables = numberOfInputVariables;
    }
  }
}
