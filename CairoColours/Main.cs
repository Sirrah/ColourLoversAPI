using System;
using Gtk;
using ColourLoversAPI;

namespace CairoColours
{
	/**
	 * This is a small sample application for the ColourLoversAPI. It creates a GTK
	 * window and uses Cairo to draw random rectangles filled with text.
	 * You can use the arrow keys to regenerate the rectangles and use a
	 * different set of colours from ColourLovers.
	 */
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.ShowAll ();

			Application.Run ();
		}
	}
}