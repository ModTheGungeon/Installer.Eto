using System;
using Eto.Forms;
using Eto.Drawing;
using MTGInstaller;
using MTGInstaller.YAML;
using System.Collections.Generic;

namespace MTGInstallerEto {
	public class ComponentList : StackLayout {
		public IEnumerable<ETGModComponent> Components;
		public Dictionary<ETGModComponent, List<RadioButton>> RadioButtonMap = new Dictionary<ETGModComponent, List<RadioButton>>();
		public List<ComponentVersion> SelectedVersions = new List<ComponentVersion>();

		public event EventHandler<EventArgs> SelectedVersionsChanged;

		private void _SetRadioButtonSetState(ETGModComponent key, bool enabled) {
			List<RadioButton> controls;
			if (RadioButtonMap.TryGetValue(key, out controls)) {
				foreach (var control in controls) {
					control.Enabled = enabled;
					control.Parent.Visible = enabled;
					if (control.Checked) {
						if (enabled) SelectedVersions.Add((ComponentVersion)control.Tag);
						else SelectedVersions.Remove((ComponentVersion)control.Tag);
					}
					SelectedVersionsChanged.Invoke(this, new EventArgs());
				}
			}
		}

		public void CheckEvent(object sender, EventArgs args) {
			var checkbox = (CheckBox)sender;
			var component = (ETGModComponent)checkbox.Tag;
			_SetRadioButtonSetState(component, checkbox.Checked.GetValueOrDefault());
		}

		public void RadioCheckEvent(object sender, EventArgs args) {
			var radiobutton = (RadioButton)sender;
			var version = (ComponentVersion)radiobutton.Tag;
			if (radiobutton.Checked) SelectedVersions.Add(version);
			else SelectedVersions.Remove(version);

			SelectedVersionsChanged.Invoke(this, new EventArgs());
		}

		public ComponentList(IEnumerable<ETGModComponent> components) {
			Orientation = Orientation.Vertical;
			Components = components;
			Size = new Size(160, -1);

			Items.Add(new Label { Text = "Components" });

			foreach (var component in components) {
				var checkbox = new CheckBox { Text = component.Name, Tag = component, Size = new Size(160, -1) };
				checkbox.CheckedChanged += CheckEvent;
				Items.Add(checkbox);

				bool first = true;
				RadioButton control = null;

				foreach (var version in component.Versions) {
					RadioButton button;

					if (first) {
						first = false;
						button = control = new RadioButton() { Checked = true };
					} else {
						button = new RadioButton(control);
					}

					button.Size = new Size(160, -1);
					button.Tag = new ComponentVersion(component, version);
					button.Text = version.DisplayName;
					button.Enabled = false;
					button.CheckedChanged += RadioCheckEvent;
					Items.Add(new StackLayout {
						Orientation = Orientation.Horizontal,
						Items = {
							new Label { Text = "    " },
							button
						},
						Visible = false
					});

					List<RadioButton> buttons;
					if (!RadioButtonMap.TryGetValue(component, out buttons)) buttons = RadioButtonMap[component] = new List<RadioButton>();
					buttons.Add(button);
				}
			}
		}
	}
}
