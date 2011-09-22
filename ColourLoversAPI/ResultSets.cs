using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing;

/*
These classes are used for deserialization of the XML stream recieved from
the colourlovers API.
For details on the API see: http://www.colourlovers.com/api

I make use of System.Drawing.Color here, but you could just as well change this
to something of your choice, like Cairo.Color.
*/
namespace ColourLoversAPI
{
	[XmlRootAttribute("lovers", IsNullable=false)]
	public class LoverSet
	{
		[XmlElement("lovers")]
		public List<Lover> Lovers;
	}

	[XmlRootAttribute("colors", IsNullable=false)]
	public class ColorSet
	{
		[XmlElement("color")]
		public List<BelovedColor> Colors;
	}

	[XmlRootAttribute("patterns", IsNullable=false)]
	public class PatternSet
	{
		[XmlElement("pattern")]
		public List<Pattern> Patterns;
	}

	[XmlRootAttribute("palettes", IsNullable=false)]
	public class PaletteSet
	{
		[XmlElement("palette")]
		public List<Palette> Palettes;
	}

	public class Lover
	{
		public int id;
		public string userName;
		public string dateRegistered;
		public string dateLastActive;
		public int rating;
		public string location;
		public int numColors;
		public int numPalettes;
		public int numPatterns;
		public int numCommentsMade;
		public int numLovers;
		public int numCommentsOnProfile;
		[XmlArray("comments")]
		[XmlArrayItem("comment")]
		public Comment[] comments;
		public string url;
		public string apiUrl;

		public class Comment
		{
			public string commentDate;
			public string commentUserName;
			public string commentComments;
		}
	}

	[XmlRootAttribute("stats", IsNullable=false)]
	public class Stats
	{
		public int total;
	}

	/*
	 * The naming is a bit inconsistent here, but usually my namespace already
	 * contains Cairo.Color or System.Drawing.Color, don't need to add a
	 * third Color to the mix.
	 */
	public class BelovedColor : LoversBase
	{
		public string hex;
		[XmlElement("rgb")]
		public RGB Rgb;
		[XmlElement("hsv")]
		public HSV Hsv;

		public Color Color {
			get	{ return Color.FromArgb (Rgb.R, Rgb.G, Rgb.B); }
		}

		public class RGB
		{
			[XmlElement("red")]
			public int R;
			[XmlElement("green")]
			public int G;
			[XmlElement("blue")]
			public int B;
		}

		public class HSV
		{
			[XmlElement("hue")]
			public int H;
			[XmlElement("saturation")]
			public int S;
			[XmlElement("value")]
			public int V;
		}
	}

	public class Pattern : LoversBase
	{
		[XmlArray("colors")]
		[XmlArrayItem("hex")]
		public string[] hex_colors;
		private Color[] colors = null;

		public Color[] Colors {
			get {
				// Populate the colors array by converting the hex values retrieved from
				// the XML stream.
				// If anyone knows a proper way to do this directly from the XML stream ...
				if (colors == null)
				{
					colors = new Color[hex_colors.Length];
					for (int i=0; i<hex_colors.Length; i++)
					{
						string hex = hex_colors [i];
						colors [i] = Color.FromArgb (
								Int32.Parse (hex.Substring (0, 2), System.Globalization.NumberStyles.HexNumber),
								Int32.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber),
						  		Int32.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber));
					}
				}
				return colors;
			}
		}
	}

	public class Palette : Pattern
	{
		public string colorWidths;
		private List<double> colorWidthList;

		public List<double> ColorWidths {
			get {
				if (colorWidthList == null)
				{
					colorWidthList = new List<double> ();
					double sum = 0;
					foreach (string val in colorWidths.Split(','))
					{
						double x = double.Parse (val);
						colorWidthList.Add (x);
						sum += x;
					}
					// normalize so that all widths sum to 1
					for (int i=0; i < colorWidthList.Count; i++)
					{
						colorWidthList [i] = colorWidthList [i] / sum;
					}
				}
				return colorWidthList;
			}
		}
	}

	public class LoversBase
	{
		public int id;
		public string title;
		public string userName;
		public int numViews;
		public int numVotes;
		public int numComments;
		public float numHearts;
		public int rank;
		public string dateCreated;
		public string description;
		public string url;
		public string imageUrl;
		public string badgeUrl;
		public string apiUrl;

		public DateTime DateCreated {
			get { return DateTime.Parse (dateCreated); }
		}
	}
}

