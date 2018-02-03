using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;
using MTGInstaller.YAML;

namespace MTGInstallerEto {
	public class Options : StackLayout {
		public CheckBox LeavePatchDLLs;
		public CheckBox SkipVersionChecks;
		public CheckBox ForceBackup;

		public bool LeavePatchDLLsOption { get { return LeavePatchDLLs.Checked.GetValueOrDefault(); } }
		public bool SkipVersionChecksOption { get { return SkipVersionChecks.Checked.GetValueOrDefault(); } }
		public bool ForceBackupOption { get { return ForceBackup.Checked.GetValueOrDefault(); } }

		public Options() {
			Orientation = Orientation.Vertical;

			Items.Add(new Label { Text = "Advanced configuration" });
			Items.Add(LeavePatchDLLs = new CheckBox { Text = "Leave patch DLLs" });
			Items.Add(SkipVersionChecks = new CheckBox { Text = "Skip version checks" });
			Items.Add(ForceBackup = new CheckBox { Text = "Force backup" });
		}
	}
}
