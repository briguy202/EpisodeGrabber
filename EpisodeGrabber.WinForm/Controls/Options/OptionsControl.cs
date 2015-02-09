using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using EpisodeGrabber.Library;

namespace EpisodeGrabber.Controls.Options {
	public class OptionsControl : UserControl {
		public UserConfiguration Configuration { get; private set; }

		public void SetConfiguration(UserConfiguration configuration) {
			this.Configuration = configuration;
			this.OnSetConfiguration();
		}

		public void Save() {
			if (this.Configuration != null) {
				this.OnSave();
			}
		}

		protected virtual void OnSetConfiguration() {
			// do nothing
		}

		protected virtual void OnSave() {
			// do nothing
		}
	}
}
