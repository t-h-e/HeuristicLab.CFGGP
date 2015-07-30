#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2015 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.IronPython {
  [Plugin("HeuristicLab.IronPython", "2.7.5.0")]
  [PluginFile("HeuristicLab.IronPython-2.7.5.dll", PluginFileType.Assembly)]
  [PluginFile("IronPython-License.txt", PluginFileType.License)]
  [PluginFile("IronPython.dll", PluginFileType.Assembly)]
  [PluginFile("IronPython.Modules.dll", PluginFileType.Assembly)]
  [PluginFile("Microsoft.Dynamic.dll", PluginFileType.Assembly)]
  [PluginFile("Microsoft.Scripting.dll", PluginFileType.Assembly)]
  [PluginFile("Microsoft.Scripting.Metadata.dll", PluginFileType.Assembly)]
  public class HeuristicLabExtLibsIronPythonPlugin : PluginBase {
  }
}
