
using System;

namespace CairoColours
{
	
	
	public partial class ControlWindow : Gtk.Window
	{
		
		public ControlWindow() : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}
	}
}
