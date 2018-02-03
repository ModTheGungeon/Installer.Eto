using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;
using MTGInstaller.YAML;

namespace MTGInstallerEto {
	public class VersionDisplay : StackLayout {
		public ListBox List;

		public VersionDisplay() {
			Orientation = Orientation.Vertical;
			Items.Add(List = new ListBox { Size = new Size(340, 200), Enabled = false});
		}

		public void ConnectTo(ComponentList list) {
			list.SelectedVersionsChanged += _Update;
			Update(list);
		}

		public void Update(ComponentList component_list) {
			List.Items.Clear();
			foreach (var version in component_list.SelectedVersions) {
				List.Items.Add($"{version.Component.Name} {version.Version.DisplayName}");
			}
		}

		public void SetIndex(int idx) {
			List.SelectedIndex = idx;
		}

		public void IncreaseIndex() {
			List.SelectedIndex += 1;
		}

		public void DisableIndex() {
			List.SelectedIndex = -1;
		}

		private void _Update(object sender, EventArgs args) {
			Update((ComponentList)sender);
		}
	}
}
