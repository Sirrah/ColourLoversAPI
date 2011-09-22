using System;
using System.Text;
using System.IO;
using Cairo;
using Gtk;
using System.Collections.Generic;
using ColourLoversAPI;

public partial class MainWindow: Gtk.Window
{
	CairoGraphic drawing;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		drawing = new CairoGraphic ();
		Box box = new HBox (true, 0);
		box.Add (drawing);
		Add (box);
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected override bool OnKeyPressEvent (Gdk.EventKey args)
	{
		switch (args.Key)
		{
		case Gdk.Key.space:
			drawing.CreateNewDrawing ();
			break;
		case Gdk.Key.Left:
			drawing.CurrentDrawing = drawing.CurrentDrawing - 1;
			break;
		case Gdk.Key.Right:
			drawing.CurrentDrawing = drawing.CurrentDrawing + 1;
			break;
		case Gdk.Key.Down:
			drawing.CurrentSeedIndex = drawing.CurrentSeedIndex - 1;
			break;
		case Gdk.Key.Up:
			drawing.CurrentSeedIndex = drawing.CurrentSeedIndex + 1;
			break;
		case Gdk.Key.b:
		case Gdk.Key.B:
			drawing.SaveToBmp (args);
			break;
		case Gdk.Key.s:
		case Gdk.Key.S:
			drawing.SaveToSvg (args);
			break;
		case Gdk.Key.p:
		case Gdk.Key.P:
			drawing.SaveToPdf (args);
			break;
		case Gdk.Key.o:
		case Gdk.Key.O:
			drawing.ShowOSD = !drawing.ShowOSD;
			break;
		case Gdk.Key.Escape:
		case Gdk.Key.q:
		case Gdk.Key.Q:
			Application.Quit ();
			break;
		default:
			return base.OnKeyPressEvent (args);
		}	
		return true;
	}

}

public class CairoGraphic : DrawingArea
{
	private Random seed_generator = new Random ();
	private int current_state;
	private List<int> current_seed_indices;
	private List<DrawingState> state_history;
	private bool showOSD = true;

	public CairoGraphic ()
	{
		// create the first state
		DrawingState new_state;
		new_state.palette = ColourLovers.RandomPalette;
		//new_state.palette = ColourLovers.SelectedPalette;
		new_state.Seeds = new List<int> ();
		new_state.Seeds.Add (seed_generator.Next ());

		// and record it in the history lists
		state_history = new List<DrawingState> ();
		state_history.Add (new_state);
		current_seed_indices = new List<int> ();

		// set the current indices of the history lists to the beginning
		current_state = 0;
		current_seed_indices.Add (0);
		ReadTextFiles ();
	}

	// For navigating through the history
	public int CurrentDrawing {
		get {
			return current_state;
		}
		set {
			// create a new drawing and set the index
			if (value == state_history.Count)
			{
				DrawingState new_state;
				new_state.palette = ColourLovers.RandomPalette;
				//new_state.palette = ColourLovers.SelectedPalette;
				new_state.Seeds = new List<int> ();
				new_state.Seeds.Add (seed_generator.Next ());

				// add the new drawing state to the history lists
				state_history.Add (new_state);
				current_seed_indices.Add (0);
				current_state = value;
				
				QueueDraw ();
			}
			// the drawing already exists, just set the index
			else if (value < state_history.Count && value >= 0)
			{
				current_state = value;
				QueueDraw ();
			}	
		}
	}
	
	public int CurrentSeedIndex {
		get {
			return current_seed_indices [current_state];
		}
		set {
			// create a new seed and set the index
			if (value == state_history [current_state].Seeds.Count)
			{
				state_history [current_state].Seeds.Add (seed_generator.Next ());
				
				current_seed_indices [current_state] = value;
				QueueDraw ();
			}
			// the seed already exists, just set the index
			else if (value < state_history [current_state].Seeds.Count && value >= 0)
			{
				current_seed_indices [current_state] = value;
				QueueDraw ();
			}
		}
	}

	public bool ShowOSD {
		get {
			return showOSD;
		}
		set {
			if (showOSD != value)
			{
				showOSD = value;
				QueueDraw ();
			}
		}
	}

	// This forces the creation of a new drawing by raising the current_state counter
	public void CreateNewDrawing ()
	{
		CurrentDrawing = state_history.Count;
	}
	
	static void DrawCurvedRectangle (Cairo.Context gr, double x, double y, double width, double height)
	{
		gr.Save ();
		gr.MoveTo (x, y + height / 2);
		gr.CurveTo (x, y, x, y, x + width / 2, y);
		gr.CurveTo (x + width, y, x + width, y, x + width, y + height / 2);
		gr.CurveTo (x + width, y + height, x + width, y + height, x + width / 2, y + height);
		gr.CurveTo (x, y + height, x, y + height, x, y + height / 2);
		gr.Restore ();
	}

	static void DrawHeart (Cairo.Context gr)
	{
		double radius = Math.Sqrt (0.5);
		
		gr.Save ();
		gr.Translate (-2, radius);
		gr.MoveTo (0, -2);
		gr.LineTo (1, -1);
		gr.Arc (0.5, -0.5, radius, -45.0 / 180.0 * Math.PI, 135.0 / 180.0 * Math.PI);
		gr.Arc (-0.5, -0.5, radius, 45.0 / 180.0 * Math.PI, 215.0 / 180.0 * Math.PI);
		gr.ClosePath ();
		gr.Restore ();
	}
	
	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		using (Context g = (Context)Gdk.CairoHelper.Create (args.Window))
		{
			int window_width, window_height;
			args.Window.GetSize (out window_width, out window_height);
			DrawCurrentState (g, window_width, window_height);
		}
		return true;
	}
	
	public void SaveToPdf (Gdk.Event args)
	{
		int width, height;
		args.Window.GetSize (out width, out height);
		
		DrawingState state = state_history [current_state];
		string filename = string.Format ("{0}/{1}_by_{2}_({3}x{4}_seed={5}).pdf",
			Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop),
		    state.palette.title, state.palette.userName, width, height,
		    state.Seeds [current_seed_indices [current_state]]);

		using (Surface surface = new PdfSurface (filename, width, height))
		{
			using (Context g = new Context(surface))
			{
				DrawCurrentState (g, width, height);
			}
		}
	}
	
	public void SaveToBmp (Gdk.Event args)
	{
		int width, height;
		args.Window.GetSize (out width, out height);
		
		DrawingState state = state_history [current_state];
		string filename = string.Format ("{0}/{1}_by_{2}_({3}x{4}_seed={5}).png",
			Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop),
		    state.palette.title, state.palette.userName, width, height,
		    state.Seeds [current_seed_indices [current_state]]);

		using (Surface surface = new ImageSurface (Format.Argb32, width, height))
		{
			using (Context g = new Context(surface))
			{
				DrawCurrentState (g, width, height);
			}
			surface.WriteToPng (filename);
		}
	}
	
	public void SaveToSvg (Gdk.Event args)
	{
		int width, height;
		args.Window.GetSize (out width, out height);
		
		DrawingState state = state_history [current_state];
		
		string filename = string.Format ("{0}/{1}_by_{2}_({3}x{4}_seed={5}).svg",
			Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop),
		    state.palette.title, state.palette.userName, width, height,
		    state.Seeds [current_seed_indices [current_state]]);

		using (Surface surface = new SvgSurface(filename, width, height))
		{
			using (Context g = new Context(surface))
			{
				DrawCurrentState (g, width, height);
			}
		}
	}

	// Retrieve a color from the given palette while respecting the color widths
	private Cairo.Color GetColorFromPalette (Palette palette, double val)
	{
		System.Drawing.Color c;
		double sum = 0.0;
		for (int i=0; i<palette.ColorWidths.Count; i++)
		{
			sum += palette.ColorWidths [i];
			if (val <= sum)
			{
				c = palette.Colors [i];
				return new Color (c.R / 255.0, c.G / 255.0, c.B / 255.0);
			}
		}

		// otherwise choose the last color in the palette
		// this can happen when the colorWidths don't sum up to 1.0
		c = palette.Colors [palette.ColorWidths.Count - 1];
		return new Color (c.R / 255.0, c.G / 255.0, c.B / 255.0);
	}
	
	public void DrawCurrentState (Context g, int window_width, int window_height)
	{
		DrawingState state = state_history [current_state];
		
		// initialize a new Random number generator with the stored seed
		Random rand = new Random (state.Seeds [current_seed_indices [current_state]]);

		double max_size = Math.Max (window_height, window_width);
		max_size = max_size * 0.40;
		double min_size = max_size * 0.20;

		// rotate around the center of the screen
		g.Save ();
		g.Translate (window_width * 0.5, window_height * 0.5);
		g.Rotate (rand.NextDouble () * Math.PI * 2);
		g.Translate (window_width * -0.5, window_height * -0.5);

		// now translate all rectangles
		g.Translate (-window_width + (rand.NextDouble () * window_width * 2),
					-window_height + (rand.NextDouble () * window_height * 2));

		// background
		g.Color = GetColorFromPalette (state.palette, rand.NextDouble ());
		g.Paint ();

		for (int i=0; i < rand.Next(50,200); i++)
		{
			// figure
			double x = rand.NextDouble () * (window_width + max_size * 2) - max_size;
			double y = rand.NextDouble () * (window_height + max_size * 2) - max_size;
			double width = rand.NextDouble () * (max_size - min_size) + min_size;
			double height = rand.NextDouble () * (max_size - min_size) + min_size;
			double border_width = Math.Min (width, height) * 0.05;
			
			DrawCurvedRectangle (g, x, y, width, height);
			
			Cairo.Color fillColor = GetColorFromPalette (state.palette, rand.NextDouble ());
			fillColor.A = rand.NextDouble ();
			g.Color = fillColor;
			g.FillPreserve ();
			
			// border
			Cairo.Color borderColor = GetColorFromPalette (state.palette, rand.NextDouble ());
			borderColor.A = rand.NextDouble ();
			g.Color = borderColor;
			g.LineWidth = border_width;
			g.Stroke ();
			
			DrawText (g, x, y, width, height, border_width, borderColor, rand);
		}
		g.Translate (window_width * 0.5, window_height * 0.5);
		g.Restore ();

		if (showOSD)
			DrawOSD (g, state, window_width, window_height);
	}
	
	Dictionary<string, List<string>> textFiles;
	static string[] filenames = { "ColourLovers.cs", "ResultSets.cs" };

	private void ReadTextFiles ()
	{
		textFiles = new Dictionary<string, List<string>> (filenames.Length);
		foreach (string filename in filenames)
		{
			StreamReader reader = new StreamReader (File.OpenRead (
					"../../../ColourLoversAPI/" + filename));
			string input = null;
			List<string > contents = new List<string> ();
			while ((input = reader.ReadLine()) != null)
			{
				int idx = input.IndexOf ('%');
				if (idx >= 0)
				{
					input = input.Remove (idx, input.Length - idx);
				}
				input = input.Trim ();
				
				if (input.Length > 1)
					contents.Add (input);
			}
			textFiles [filename] = contents;
			reader.Close ();
		}
	}
	
	private void DrawText (Cairo.Context g, double x, double y, double width, double height, double border_width, Color text_color, Random rand)
	{
		border_width *= 2;
		if (width < 100 || height < 100)
			return;
		
		g.Save ();
		
		g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Bold);
		double character_height = Math.Max (height / 15, 6);
		character_height = Math.Min (character_height, 9);
		g.SetFontSize (character_height);
		FontExtents fe = g.FontExtents;
		text_color.A *= 0.6;
		text_color.R -= 0.1;
		text_color.G -= 0.1;
		text_color.B -= 0.1;
		g.Color = text_color;
//		LinearGradient p = new LinearGradient (x, y, x+width, y+height);
//		p.AddColorStop (0, new Color (1, 0, 0, 1.0));
//		p.AddColorStop (0.4, new Color (0, 1, 0, 1.0));
//		p.AddColorStop (0.6, new Color (0, 1, 0, 1.0));
//		p.AddColorStop (1.0, new Color (1, 0, 0, 1.0));
//		g.Pattern = p;
		
		string filename = filenames [rand.Next (0, filenames.Length - 1)];
		List<string > text = textFiles [filename];
		int starting_point = rand.Next (0, text.Count - 1);
		
		int counter = 0;
		while (border_width*2 + fe.Height*(counter+1) < height && counter+starting_point < text.Count)
		{
			string line = text [starting_point + counter];
			TextExtents te = g.TextExtents (line);
			while (te.XAdvance > width-border_width*2)
			{
				double to_be_cut = te.XAdvance - (width - border_width * 2);
				int characters_to_be_cut = (int)(to_be_cut / fe.MaxXAdvance) + 1;
				line = line.Remove (line.Length - characters_to_be_cut - 1, characters_to_be_cut).Trim ();
				te = g.TextExtents (line);
			}
			
			g.MoveTo (x + border_width, y + border_width + fe.Height * (counter + 1));
			g.ShowText (line);
			counter++;
		}
		
		g.Restore ();
	}
	
	private void DrawOSD (Context g, DrawingState state, int window_width, int window_height)
	{
		string[] osd_text = { 	state.palette.title,
								"by " + state.palette.userName,
		};
		g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Bold);
		g.SetFontSize (16);
		FontExtents fe_title = g.FontExtents;

		double heart_size = 6;
		double palette_height = 60;
		double palette_width = 120;
		double osd_width = palette_width;
		double osd_height = fe_title.Height * osd_text.Length + palette_height + (state.palette.numHearts > 0.0 ? heart_size * 2.3 : 0);
		double osd_border = 10;
		double numHearts = state.palette.numHearts;
		
		{
			g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Bold);
			g.SetFontSize (16);
			TextExtents te = g.TextExtents (osd_text [0]);
			
			osd_width = Math.Max (osd_width, te.Width);

			g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Bold);
			g.SetFontSize (14);
			te = g.TextExtents (osd_text [1]);
			
			osd_width = Math.Max (osd_width, te.Width);
			osd_width = Math.Ceiling (Math.Max (osd_width, Math.Ceiling (numHearts) * heart_size * 2.5));
		}

		double osd_x = window_width - osd_width - osd_border - 10;
		double osd_y = window_height - osd_height - osd_border - 10;
		g.Rectangle (osd_x - osd_border, osd_y - osd_border, osd_width + osd_border * 2, osd_height + osd_border * 2);
		//DrawCurvedRectangle (g, osd_x - osd_border, osd_y - osd_border, osd_width + osd_border*2, osd_height + osd_border*2);
		g.Color = new Cairo.Color (0, 0, 0, 0.5);
		g.Fill ();

//		g.Save();
//		// draw a continuous palette
//		DrawCurvedRectangle(g, osd_x, osd_y, palette_width, palette_height);
//		LinearGradient palette_gradient = new LinearGradient(osd_x, 0, osd_x+palette_width, 0);
//		double width = 0;
//		for (int i=0; i < state.palette.ColorWidths.Count; i++)
//		{
//			System.Drawing.Color color = state.palette.Colors[i];
//			width += state.palette.ColorWidths[i];
//			palette_gradient.AddColorStop(width,
//					new Cairo.Color(color.R/255.0, color.G/255.0, color.B/255.0));
//		}
//		g.Pattern = palette_gradient;
//		g.Fill();
		
		// draw discrete palette
		{
			double current_x = Math.Floor (osd_x);
			for (int i=0; i < state.palette.ColorWidths.Count; i++)
			{
				System.Drawing.Color color = state.palette.Colors [i];
				g.Color = new Cairo.Color (color.R / 255.0, color.G / 255.0, color.B / 255.0);
				
				double width = state.palette.ColorWidths [i];
				g.Rectangle (current_x, osd_y, Math.Ceiling (width * palette_width), palette_height);
				g.Fill ();
				current_x += Math.Ceiling (width * palette_width);
			}
		}
		
		g.Color = new Cairo.Color (.9, .9, .9, 1);
		g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Bold);
		g.SetFontSize (16);
		g.MoveTo (osd_x + 1, fe_title.Height + osd_y + palette_height);
		g.ShowText (osd_text [0]);

		g.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
		g.SetFontSize (14);
		FontExtents fe_normal = g.FontExtents;
		g.MoveTo (osd_x + 1, fe_title.Height + fe_normal.Height + osd_y + palette_height);
		g.ShowText (osd_text [1]);
				
		int counter = 0;
		while (numHearts > 0.0)
		{
			g.Save ();
			g.Translate (osd_x - 1 + 2.5 * heart_size * counter, fe_title.Height + fe_normal.Height + osd_y + palette_height + heart_size * 2.3);
			
			double scale = Math.Min (numHearts, 1.0);
			g.Scale (Math.Sqrt (scale) * heart_size, Math.Sqrt (scale) * heart_size);
			
			g.Rotate (Math.PI);
			DrawHeart (g);
			g.Restore ();
						
//			if (numHearts < 1.0)
//			{
//				LinearGradient gradient = new Cairo.LinearGradient (0, 0, 800, 600);
//				gradient.AddColorStop (0.0, new Cairo.Color(0.8, 0, 0, 1.0));
//				gradient.AddColorStop (1.0, new Color(0, 1.0, 0, 0.0));
//				g.Pattern = gradient;
//			}
			
			g.Color = new Cairo.Color (0.8, 0, 0, Math.Sqrt (scale));
			g.Fill ();
			
			numHearts -= 1.0f;
			counter++;
		}
	}

	public struct DrawingState
	{
		public Palette palette;

		// store the seed to recreate the exact drawing, allow to store multiples
		// so we can create multiple drawings with the same pattern
		public List<int> Seeds;
	}
}