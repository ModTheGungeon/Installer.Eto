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

		public Task CurrentTask;

		public DynamicLayout MainLayout;

		public Log Log;

		public ExeSelector ExeSelector;
		public StackLayout CheckboxesLayout;
		public ComponentList ComponentList;
		public Options Options;
		public VersionDisplay VersionDisplay;
		public Button InstallButton;
		public Button UninstallButton;
		
		public InstallerForm() {
			KeyDown += (sender, e) => {
				Console.WriteLine($"XXX {e.Key}");
				if (e.Key.HasFlag(Keys.Enter) && e.Modifiers.HasFlag(Keys.Control)) Options.Visible = true;
			};

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) => {
				var ex = (Exception)e.ExceptionObject;
				Log.WriteLine("\nUnhandled error!");
				Log.WriteLine(ex.Message);
			});

			Downloader = new Downloader();
			Installer = new Installer(Downloader, null);

			Title = $"Mod the Gungeon Installer {Installer.Version}";
			ClientSize = new Size(640, 400);

			MainLayout = new DynamicLayout();



			MainLayout.BeginHorizontal();

			MainLayout.BeginVertical();
			MainLayout.Add(new Label { Text = "Output log" });
			MainLayout.Add(Log = new Log());
			MainLayout.Add(new ImageView { Image = Bitmap.FromResource("icon") }, false, false);
			MainLayout.EndVertical();

			MainLayout.BeginVertical();
			MainLayout.Add(new Label { Text = "Path to the Gungeon executable" });
			MainLayout.Add(ExeSelector = new ExeSelector());
			Installer.ChangeExePath(ExeSelector.Path);
			ExeSelector.PathChanged += (sender, e) => {
				Installer.ChangeExePath(ExeSelector.Path);
				UpdateInstallButton();
			};

			MainLayout.Add(CheckboxesLayout = new StackLayout {
				Orientation = Orientation.Horizontal,
			});

			ComponentList = new ComponentList(Downloader.Components.Values);
			Options = new Options { Visible = false };
			ComponentList.SelectedVersionsChanged += (sender, e) => UpdateInstallButton();

			ShowCheckboxes();

			MainLayout.Add(null, false, true);
			VersionDisplay = new VersionDisplay();
			VersionDisplay.ConnectTo(ComponentList);
			MainLayout.Add(InstallButton = new Button { Text = "Install", Enabled = false, Size = new Size(100, -1) }, true, false);
			MainLayout.Add(UninstallButton = new Button { Text = "Uninstall", Size = new Size(100, -1) }, true, false);

			InstallButton.Click += (sender, e) => Install();
			UninstallButton.Click += (sender, e) => Uninstall();

			MainLayout.EndVertical();

			Content = MainLayout;

			Log.WriteLine("Welcome!");

			var subscriber = new Logger.Subscriber((logger, loglevel, indent, str) => {
				var formatted = logger.String(loglevel, str, indent);
				Application.AsyncInvoke(() => Log.WriteLine(formatted));
			});
			Logger.Subscribe(subscriber);
		}

		public void UpdateInstallButton() {
			InstallButton.Enabled = ExeSelector.Path != null && ComponentList.SelectedVersions.Count > 0;
		}

		public void ShowCheckboxes() {
			CheckboxesLayout.Items.Clear();
			CheckboxesLayout.Items.Add(ComponentList);
			CheckboxesLayout.Items.Add(Options);
		}

		public void ShowStatus() {
			CheckboxesLayout.Items.Clear();
			CheckboxesLayout.Items.Add(VersionDisplay);
		}

		public void Uninstall() {
			Log.Clear();
			Log.WriteLine("Reverting...");
			Block();

			CurrentTask = Task.Run(() => {
				try {
					Installer.Restore(force: true);
				} catch (Exception e) {
					_PostUninstall(e);
					return;
				}

				_PostUninstall();
			});
		}

		private void _PostUninstall(Exception e = null) {
			Application.AsyncInvoke(() => {
				if (e != null) {
					Log.WriteLine($"\nError while uninstalling: {_CorrectExceptionText(e.Message)}");
					MessageBox.Show(_CorrectExceptionText(e.Message), "Error while installing", MessageBoxType.Error);
				} else {
					Log.WriteLine("\nDone.");
				}

				CurrentTask.Wait();
				CurrentTask = null;

				Unblock();
			});
		}

		public void Install() {
			Log.Clear();
			Log.WriteLine("Installing...");
			Block();

			ShowStatus();

			CurrentTask = Task.Run(() => {
				try {
					Application.AsyncInvoke(() => VersionDisplay.SetIndex(0));

					foreach (var version in ComponentList.SelectedVersions) {
						var ver = version.Version;

						using (var build = Downloader.Download(ver)) {
							var installable = new Installer.InstallableComponent(version.Component, ver, build);

							if (!Options.ForceBackupOption) Installer.Restore();
							Installer.Backup(Options.ForceBackupOption);

							if (!Options.SkipVersionChecksOption) {
								installable.ValidateGungeonVersion(Autodetector.GetVersionIn(ExeSelector.Path));
							}

							Installer.InstallComponent(installable, leave_mmdlls: Options.LeavePatchDLLsOption);
						}

						Application.AsyncInvoke(() => VersionDisplay.IncreaseIndex());
					}
				} catch (Exception e) {
					_PostInstall(e);
					return;
				}

				_PostInstall();
			});
		}

		private string _CorrectExceptionText(string msg) {
			return msg.Replace("'--force'", "the 'Skip Version Checks' advanced option");
		}

		private void _PostInstall(Exception e = null) {
			Application.AsyncInvoke(() => {
				if (e != null) {
					Log.WriteLine($"\nError while installing: {_CorrectExceptionText(e.Message)}");
					MessageBox.Show(_CorrectExceptionText(e.Message), "Error while installing", MessageBoxType.Error);
				} else {
					Log.WriteLine("\nDone.");
				}

				CurrentTask.Wait();
				CurrentTask = null;

				Unblock();
				VersionDisplay.DisableIndex();
				ShowCheckboxes();
			});
		}

		public void Block() {
			ExeSelector.Enabled = false;
			ComponentList.Enabled = false;
			VersionDisplay.Enabled = false;
			InstallButton.Enabled = false;
			UninstallButton.Enabled = false;
		}

		public void Unblock() {
			ExeSelector.Enabled = true;
			ComponentList.Enabled = true;
			VersionDisplay.Enabled = true;
			UpdateInstallButton();
			UninstallButton.Enabled = true;
		}

		[STAThread]
		public static void Main() {
			Application = new Application();
			Application.Run(new InstallerForm());
		}
	}
}
