using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;

namespace MTGInstallerEto {
	public class ExeSelector : StackLayout {
		public TextBox PathBox;
		public Button FileDialogButton;

		public ExeSelector() {
			Orientation = Orientation.Horizontal;

			Items.Add(PathBox = new TextBox { Text = Autodetector.ExePath ?? "???", Size = new Size(310, 0), ReadOnly = true });
			PathBox.CaretIndex = PathBox.Text.Length;
			Items.Add(FileDialogButton = new Button { Image = Icon.FromResource("icon::folder") });
			FileDialogButton.Click += _OpenExe;
		}

		public void OpenExe() {
			var fd = new OpenFileDialog { Filters = {
				new FileDialogFilter("Enter the Gungeon executable", Autodetector.ExeName)
			} };
			fd.ShowDialog(this);
			if (fd.FileName == null) return;

			PathBox.Text = fd.FileName;
			PathBox.CaretIndex = PathBox.Text.Length;
			PathChanged.Invoke(this, new EventArgs());
		}
		private void _OpenExe(object sender, EventArgs e) => OpenExe();

		public string Path { get { return PathBox.Text; } }

		public event EventHandler<EventArgs> PathChanged;
	}
}
