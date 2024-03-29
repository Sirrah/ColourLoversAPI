This is a Monodevelop/VisualStudio solution with two projects. I've only used the code once to generate a cover for my thesis (unrelated subject). At the very least it may serve as a nice sample application, either for RESTful interfaces with .Net or for drawing with Cairo.

# ColourLoversAPI
A simple API to the RESTful interface of [COLOURLovers](http://colourlovers.com "ColourLovers").

## Dependencies
Only a few system libraries:
System.Web, System.Xml and System.Drawing (for System.Drawing.Color, can be easily replaced)

# CairoColours
A sample application to ColourLoversAPI. It uses Cairo and Pango to draw an image and uses the ColourLoversAPI to determine the colours.

## Controls
* **Space**          new drawing
* **Left**/**Right** previous/next colour palette
* **Up**/**Down**    new seed for the random generator
* **b**              save to image on the Desktop (png)
* **s**              save to image on the Desktop (svg)
* **p**              save to image on the Desktop (pdf)
* **o**              toggle colour information
* **Escape**/**q**   quit

## Dependencies
This is a somewhat old project so any Gnome2 era (or later) release of the following libraries should work:
GTK, Cairo, Pango
