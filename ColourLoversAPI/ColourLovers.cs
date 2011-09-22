using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace ColourLoversAPI
{
	public enum Sort
	{
		Ascending,
		Descending
	}

	public enum OrderBy
	{
		DateCreated,
		Score,
		Name,
		NumberOfVotes,
		NumberOfViews
	}

	public class ColourLovers
	{
		#region Properties
		private const string baseUri = "http://www.colourlovers.com/api";

		public static int NumberOfColors {
			get {
				string requestUri = string.Format ("{0}/stats/colors", baseUri);
				return Request<Stats> (requestUri).total;
			}
		}

		public static int NumberOfPalettes {
			get {
				string requestUri = string.Format ("{0}/stats/palettes", baseUri);
				return Request<Stats> (requestUri).total;
			}
		}

		public static int NumberOfPatterns {
			get {
				string requestUri = string.Format ("{0}/stats/patterns", baseUri);
				return Request<Stats> (requestUri).total;
			}
		}

		public static int NumberOfLovers {
			get {
				string requestUri = string.Format ("{0}/stats/lovers", baseUri);
				return Request<Stats> (requestUri).total;
			}
		}

		public static Pattern RandomPattern {
			get {
				string requestUri = string.Format ("{0}/patterns/random", baseUri);
				List<Pattern > list = Request<PatternSet> (requestUri).Patterns;
				if (list.Count >= 1)
					return list [0];
				else
					return null;
			}
		}

		private static Palette selectedPalette = null;

		public static Palette SelectedPalette {
			get {
				if (selectedPalette == null)
				{
					string requestUri = @"http://www.colourlovers.com/api/palette/711652?showPaletteWidths=1";
					List<Palette > list = Request<PaletteSet> (requestUri).Palettes;
					if (list.Count >= 1)
						selectedPalette = list [0];
				}
				return selectedPalette;
			}
		}

		public static Palette RandomPalette {
			get {
				string requestUri = string.Format ("{0}/palettes/random?showPaletteWidths=1", baseUri);
				List<Palette > list = Request<PaletteSet> (requestUri).Palettes;
				if (list.Count >= 1)
					return list [0];
				else
					return null;
			}
		}

		public static BelovedColor RandomColor {
			get {
				string requestUri = string.Format ("{0}/colors/random", baseUri);
				List<BelovedColor > list = Request<ColorSet> (requestUri).Colors;
				if (list.Count >= 1)
					return list [0];
				else
					return null;
			}
		}

		#endregion

		#region Get*ColourLover(s)
		public static Lover GetColourLover (string username)
		{
			string requestUri = string.Format ("{0}/lover/{1}", baseUri, username);
			List<Lover > list = Request<LoverSet> (requestUri).Lovers;
			if (list.Count >= 1)
				return list [0];
			else
				return null;
		}

		public static List<Lover> GetColourLovers (OrderBy orderBy, Sort sort, int numResults, int resultOffSet)
		{
			return GetColourLoversHelper (baseUri + "/lovers", orderBy, sort, numResults, resultOffSet);
		}

		public static List<Lover> GetNewColourLovers (OrderBy orderBy, Sort sort, int numResults, int resultOffSet)
		{
			return GetColourLoversHelper (baseUri + "/lovers/new", orderBy, sort, numResults, resultOffSet);
		}

		public static List<Lover> GetTopColourLovers (OrderBy orderBy, Sort sort, int numResults, int resultOffSet)
		{
			return GetColourLoversHelper (baseUri + "/lovers/top", orderBy, sort, numResults, resultOffSet);
		}

		private static List<Lover> GetColourLoversHelper (string baseUri, OrderBy orderBy, Sort sort, int numResults, int resultOffSet)
		{
			StringBuilder requestUri = new StringBuilder (baseUri);
			requestUri.Append ("?orderCol=");
			switch (orderBy)
			{
			case OrderBy.DateCreated:
				requestUri.Append ("dateCreated");
				break;
			case OrderBy.Score:
				requestUri.Append ("score");
				break;
			case OrderBy.Name:
				requestUri.Append ("name");
				break;
			case OrderBy.NumberOfVotes:
				requestUri.Append ("numVotes");
				break;
			case OrderBy.NumberOfViews:
				requestUri.Append ("numViews");
				break;
			}

			if (Sort.Ascending.Equals (sort))
				requestUri.Append ("&sortBy=ASC");
			else
				requestUri.Append ("&sortBy=DESC");
			requestUri.Append ("&numResults=").Append (numResults);
			requestUri.Append ("&resultOffset=").Append (resultOffSet);
			return Request<LoverSet> (requestUri.ToString ()).Lovers;
		}

		#endregion

		#region Get*Color(s)
		public static BelovedColor GetColor (string id)
		{
			string requestUri = string.Format ("{0}/color/{1}", baseUri, id);
			List<BelovedColor > list = Request<ColorSet> (requestUri).Colors;
			if (list.Count >= 1)
				return list [0];
			else
				return null;
		}

		public static List<BelovedColor> GetColors (string lover,
				int min_hue, int max_hue, int min_bri, int max_bri,
		        string[] keywords, bool keywordExact,
				OrderBy orderBy, Sort sort, int numResults)
		{
			return GetColorsHelper (baseUri + "/colors", lover, min_hue, max_hue,
			                       min_bri, max_bri, keywords, keywordExact,
			                       orderBy, sort, numResults);
		}

		public static List<BelovedColor> GetNewColors (string lover,
				int min_hue, int max_hue, int min_bri, int max_bri,
		        string[] keywords, bool keywordExact,
				OrderBy orderBy, Sort sort, int numResults)
		{
			return GetColorsHelper (baseUri + "/colors/new", lover, min_hue, max_hue,
			                       min_bri, max_bri, keywords, keywordExact,
			                       orderBy, sort, numResults);
		}

		public static List<BelovedColor> GetTopColors (string lover,
				int min_hue, int max_hue, int min_bri, int max_bri,
		        string[] keywords, bool keywordExact,  
				OrderBy orderBy, Sort sort, int numResults)
		{
			return GetColorsHelper (baseUri + "/colors/top", lover, min_hue, max_hue,
			                       min_bri, max_bri, keywords, keywordExact,
			                       orderBy, sort, numResults);
		}
			
		private static List<BelovedColor> GetColorsHelper (string baseUri, string lover,
				int min_hue, int max_hue, int min_bri, int max_bri,
		        string[] keywords, bool keywordExact,  
				OrderBy orderBy, Sort sort, int numResults)
		{
			StringBuilder requestUri = new StringBuilder (baseUri + "?");
			if (!string.IsNullOrEmpty (lover))
				requestUri.Append ("lover=" + lover);

			requestUri.AppendFormat ("&hueRange={0},{1}", min_hue, max_hue);
			requestUri.AppendFormat ("&briRange={0},{1}", min_bri, max_bri);

			requestUri.Append ("&keywords=");
			foreach (string keyword in keywords)
				requestUri.Append (keyword + "+");
			requestUri.Remove (requestUri.Length - 1, 1);
			requestUri.Append ("&keywordExact=" + (keywordExact ? "1" : "0"));
			
			requestUri.Append ("&orderCol=");
			if (orderBy == OrderBy.DateCreated)
				requestUri.Append ("dateCreated");
			else if (orderBy == OrderBy.Score)
				requestUri.Append ("score");
			else if (orderBy == OrderBy.Name)
				requestUri.Append ("name");
			else if (orderBy == OrderBy.NumberOfVotes)
				requestUri.Append ("numVotes");
			else if (orderBy == OrderBy.NumberOfViews)
				requestUri.Append ("numViews");
			if (sort == Sort.Ascending)
				requestUri.Append ("&sortBy=ASC");
			else
				requestUri.Append ("&sortBy=DESC");
			requestUri.Append ("&numResults=" + numResults);

			return Request<ColorSet> (requestUri.ToString ()).Colors;
		}
		#endregion

		/*
		 * This method handles the actual communication with the ColourLovers
		 * website. This method is public in case I forgot something in the API
		 * but it shouldn't have to be used directly.
		 */
		static int retries = 0;

		public static T Request<T> (string requestUri)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create (requestUri);
				T parsedSet;
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					using (Stream responseStream = response.GetResponseStream())
					{
						XmlSerializer serializer = new XmlSerializer (typeof(T));
						parsedSet = (T)serializer.Deserialize (responseStream);
					}
				}
				retries = 0;
				return parsedSet;
			}
			catch (WebException e)
			{
				retries++;
				if (retries <= 3)
				{
					System.Threading.Thread.Sleep (1000);
					return Request<T> (requestUri);
				}
				else
					throw e;
			}
		}
	}
}