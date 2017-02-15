using System.Collections.Generic;

namespace HeuristicLab.Problems.Instances.CFG {
  public enum DataType {
    Boolean, Integer, Float, String, ListBoolean, ListInteger, ListFloat, ListString
  }

  public static class DataTypeExtensions {
    public static IEnumerable<DataType> Requires(this DataType dt) {
      switch (dt) {
        case DataType.ListBoolean:
          return new List<DataType>() { DataType.Boolean };
        case DataType.ListInteger:
          return new List<DataType>() { DataType.Integer };
        case DataType.ListFloat:
          return new List<DataType>() { DataType.Float };
        case DataType.ListString:
          return new List<DataType>() { DataType.String };
        default:
          break;
      }
      return new List<DataType>();
    }
  }
}
