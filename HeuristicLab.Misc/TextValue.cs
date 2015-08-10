#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  [Item("TextValue", "Represents a text (marker class for string).")]
  [StorableClass]
  public class TextValue : StringValue {

    [StorableConstructor]
    protected TextValue(bool deserializing) : base(deserializing) { }
    protected TextValue(TextValue original, Cloner cloner)
      : base(original, cloner) {
    }
    public TextValue() : base() { }
    public TextValue(string value) : base(value) { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new TextValue(this, cloner);
    }
  }
}
