using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;
using MTGInstaller.YAML;

namespace MTGInstallerEto {
	public class Log : TextArea {
		public Log() {
			Size = new Size(320, 200);
			Text = "";
			ReadOnly = true;
		}

		public void Clear() => Text = "";

		public void Write(object obj) {
			Append(obj.ToString(), true);
		}

		public void WriteLine(object obj) {
			Append(obj.ToString() + '\n', true);
		}

		public void WriteLine() {
			Append("\n", true);
		}
	}
}
