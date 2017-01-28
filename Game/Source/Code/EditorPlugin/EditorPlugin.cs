using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Duality.Editor;

namespace Game.Editor
{
	/// <summary>
	/// Defines a Duality editor plugin.
	/// </summary>
    public class GameEditorPlugin : EditorPlugin
	{
		public override string Id
		{
			get { return "GameEditorPlugin"; }
		}
	}
}
