using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;
using MTGInstaller.YAML;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGInstallerEto {
	public class InstallerForm : Form {
		public static Application Application;

		public Downloader Downloader;
		public Installer Installer;

		public Task InstallTask;

		public DynamicLayout MainLayout;

		public Log Log;

		public ExeSelector ExeSelector;
		public ComponentList ComponentList;
		public VersionDisplay VersionDisplay;
		public Button InstallButton;

		public Queue<string> LogMessages = new Queue<string>();

		public InstallerForm() {
			Downloader = new Downloader();
			Installer = new Installer(Downloader, null);

			Title = $"Mod the Gungeon Installer {Installer.Version}";
			ClientSize = new Size(640, 480);

			MainLayout = new DynamicLayout();

			MainLayout.BeginHorizontal();

			MainLayout.BeginVertical();
			MainLayout.Add(new Label { Text = "Fun log" });
			MainLayout.Add(Log = new Log());
			MainLayout.EndVertical();

			MainLayout.BeginVertical();
			MainLayout.Add(new Label { Text = "Extremely Rudimentary GUI Installer" });
			MainLayout.Add(ExeSelector = new ExeSelector());
			Installer.ChangeExePath(ExeSelector.Path);
			ExeSelector.PathChanged += (sender, e) => Installer.ChangeExePath(ExeSelector.Path);
			MainLayout.Add(ComponentList = new ComponentList(Downloader.Components.Values));
			ComponentList.SelectedVersionsChanged += (sender, e) => {
				InstallButton.Enabled = ComponentList.SelectedVersions.Count > 0;
			};
			MainLayout.Add(null, false, true);
			MainLayout.Add(VersionDisplay = new VersionDisplay());
			VersionDisplay.ConnectTo(ComponentList);
			MainLayout.Add(InstallButton = new Button { Text = "Install", Size = new Size(100, -1), Enabled = false }, false, false);
			InstallButton.Click += (sender, e) => Install();

			MainLayout.EndVertical();

			Content = MainLayout;

			Log.WriteLine("Installer initialized");

			var subscriber = new Logger.Subscriber((logger, loglevel, indent, str) => {
				var formatted = logger.String(loglevel, str, indent);
				Application.AsyncInvoke(() => Log.WriteLine(formatted));
			});
			Logger.Subscribe(subscriber);
		}

		public void Install() {
			Log.WriteLine("Installing");
			Log.Clear();
			Block();

			InstallTask = Task.Run(() => {
				Application.AsyncInvoke(() => VersionDisplay.SetIndex(0));

				foreach (var version in ComponentList.SelectedVersions) {
					var ver = version.Version;

					using (var build = Downloader.Download(ver)) {
						var installable = new Installer.InstallableComponent(version.Component, ver, build);

						Installer.Restore();
						Installer.Backup();
						Installer.InstallComponent(installable, leave_mmdlls: true);
					}

					Application.AsyncInvoke(() => VersionDisplay.IncreaseIndex());
				}

				_PostInstall();
			});
		}

		private void _PostInstall() {
			Application.AsyncInvoke(() => {
				Log.WriteLine("\nDone.");

				InstallTask.Wait();
				InstallTask = null;

				Unblock();
				VersionDisplay.DisableIndex();
			});
		}

		public void Block() {
			ExeSelector.Enabled = false;
			ComponentList.Enabled = false;
			VersionDisplay.Enabled = false;
			InstallButton.Enabled = false;
		}

		public void Unblock() {
			ExeSelector.Enabled = true;
			ComponentList.Enabled = true;
			VersionDisplay.Enabled = true;
			InstallButton.Enabled = true;
		}

		[STAThread]
		public static void Main() {
			Application = new Application();
			Application.Run(new InstallerForm());
		}
	}
}
