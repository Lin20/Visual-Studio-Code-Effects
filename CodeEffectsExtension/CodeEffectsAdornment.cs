using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;


namespace CodeEffectsExtension
{
	public class ShadowOptions : DialogPage
	{
		[Category("General")]
		[DisplayName("Quality")]
		[Description("Trades render quality at the cost of performance (default 1.0)")]
		public double Quality { get; set; } = 1.0;



		[Category("Drop Shadow")]
		[DisplayName("Enable Drop Shadow")]
		[Description("Enable drop shadow effect (default true)")]
		public bool EnableShadow { get; set; } = true;

		private double shadow_angle = 315;

		[Category("Drop Shadow")]
		[DisplayName("Angle")]
		[Description("The angle of the shadow in degrees (default 315)")]
		public double ShadowAngle
		{
			get { return shadow_angle; }
			set { shadow_angle = value; }
		}

		private double shadow_distance = 2.0;
		[Category("Drop Shadow")]
		[DisplayName("Distance")]
		[Description("The shadow distance (default 2)")]
		public double ShadowDistance
		{
			get { return shadow_distance; }
			set { shadow_distance = value; }
		}

		[Category("Drop Shadow")]
		[DisplayName("Brightness")]
		[Description("The syntax-colored multiplier of the outline (default 0.2)")]
		public double ShadowBrightness { get; set; } = 0.2;

		[Category("Drop Shadow")]
		[DisplayName("Opacity")]
		[Description("Shadow opacity (default 0.8)")]
		public double ShadowOpacity { get; set; } = 0.8;

		[Category("Drop Shadow")]
		[DisplayName("Blur Radius")]
		[Description("The radius of the drop shadow blur, creating a softer shadow effect (default 1.0)")]
		public int ShadowBlurRadius { get; set; } = 1;


		[Category("Outline")]
		[DisplayName("Enable Outline")]
		[Description("Enable outline effect (default true)")]
		public bool EnableOutline { get; set; } = true;

		[Category("Outline")]
		[DisplayName("Width")]
		[Description("The width of the outline (default 2)")]
		public double Outline1Width { get; set; } = 2.0;

		[Category("Outline")]
		[DisplayName("Brightness")]
		[Description("The syntax-colored multiplier of the outline (default 0.1)")]
		public double OutlineBrightness { get; set; } = 0.1;

		[Category("Outline")]
		[DisplayName("Brightness")]
		[Description("The opacity of the outline (default 1.0)")]
		public double OutlineOpacity { get; set; } = 1.0;

		[Category("Outline")]
		[DisplayName("Blur Radius")]
		[Description("The radius of the outline blur (default 0.0)")]
		public double OutlineBlurRadius { get; set; } = 0.0;


		[Category("Bloom")]
		[DisplayName("Enable Bloom")]
		[Description("Enable bloom effect (default false)")]
		public bool EnableBloom { get; set; } = false;

		[Category("Bloom")]
		[DisplayName("Radius")]
		[Description("The radius of the bloom (default 8)")]
		public double BloomRadius { get; set; } = 8.0;

		[Category("Bloom")]
		[DisplayName("Brightness")]
		[Description("The brightness of the bloom (default 0.5)")]
		public double BloomBrightness { get; set; } = 0.5;

		[Category("Bloom")]
		[DisplayName("Opacity")]
		[Description("The opacity of the bloom (default 0.8)")]
		public double BloomOpacity { get; set; } = 0.8;


		/*[Category("Matching")]
		[DisplayName("Enable Bloom Classifications")]
		[Description("Whether to only apply the bloom effect to the specified classification (default false)")]
		public bool BloomMatchToClassifications { get; set; } = false;
		[Category("Matching")]
		[DisplayName("Outline Outline Classifications")]
		[Description("Whether to only apply the outline effect to the specified classification (default false)")]
		public bool OutlineMatchToClassifications { get; set; } = false;

		public static string DefaultMatches = "keyword,identifier,type name,class name,static symbol,method name,local name,keyword - control";
		[Category("Matching")]
		[DisplayName("Bloom Classifications")]
		[Description("The comma-delimited classification types to apply the bloom effect to")]
		public string BloomClassifications { get; set; } = DefaultMatches;

		[Category("Matching")]
		[DisplayName("Outline Classifications")]
		[Description("The comma-delimited classification types to apply the outline effect to")]
		public string OutlineClassifications { get; set; } = DefaultMatches;*/

		public static event EventHandler OptionsChanged;

		protected override void OnApply(PageApplyEventArgs e)
		{
			base.OnApply(e);
			OptionsChanged?.Invoke(this, e);
		}
	}

	internal sealed class CodeEffectsAdornment
	{
		private readonly IWpfTextView view;
		private readonly IAdornmentLayer adornmentLayer;
		private readonly ITagAggregator<IClassificationTag> tagAggregator;
		private readonly IClassificationFormatMap formatMap;

		private double ShadowDistance => CodeEffectsExtensionPackage.Options.ShadowDistance;
		private bool EnableDropShadows => CodeEffectsExtensionPackage.Options.EnableShadow;
		private bool EnableBloom => CodeEffectsExtensionPackage.Options.EnableBloom;
		private bool EnableOutline => CodeEffectsExtensionPackage.Options.EnableOutline;

		private double ShadowBlurRadius => CodeEffectsExtensionPackage.Options.ShadowBlurRadius;
		private double ShadowOpacity => CodeEffectsExtensionPackage.Options.ShadowOpacity;
		private double ShadowAngle => CodeEffectsExtensionPackage.Options.ShadowAngle;

		private double OutlineWidth => CodeEffectsExtensionPackage.Options.Outline1Width;
		private double OutlineBrightness => CodeEffectsExtensionPackage.Options.OutlineBrightness;
		private double BloomRadius => CodeEffectsExtensionPackage.Options.BloomRadius;

		/*private string[] outline_classifications => CodeEffectsExtensionPackage.Options.OutlineMatchToClassifications ? CodeEffectsExtensionPackage.Options.OutlineClassifications.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
		private string[] bloom_classifications => CodeEffectsExtensionPackage.Options.BloomMatchToClassifications ? CodeEffectsExtensionPackage.Options.BloomClassifications.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;*/


		public CodeEffectsAdornment(IWpfTextView view, ITagAggregator<IClassificationTag> tagAggregator, IClassificationFormatMap formatMap, IClassificationTypeRegistryService registry)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.adornmentLayer = view.GetAdornmentLayer("CodeEffectsAdornment");
			this.tagAggregator = tagAggregator;
			this.formatMap = formatMap;

			this.view.LayoutChanged += OnLayoutChanged;
			ShadowOptions.OptionsChanged += ShadowOptions_OptionsChanged;
		}

		private void ShadowOptions_OptionsChanged(object sender, EventArgs e)
		{
			textBitmapCache.Clear();
			textGeometryCache.Clear();
			adornmentLayer.RemoveAllAdornments();

			if (!EnableOutline && !EnableBloom && !EnableDropShadows)
				return;

			List<(string text, double left, double top, Color color)> text_blocks = new List<(string text, double left, double top, Color color)>();
			foreach (var line in view.TextViewLines)
			{
				ProcessLine(line, text_blocks);

				if (text_blocks.Count > 0)
				{
					SnapshotPoint start = line.Start;
					SnapshotPoint end = line.End;

					var visibleSpan = new SnapshotSpan(start, end);
					adornmentLayer.RemoveAdornmentsByVisualSpan(visibleSpan);

					var outline_text = CreateOutlinedTextVisual(text_blocks.ToArray(), view.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize,
						view.FormattedLineSource.DefaultTextProperties.Typeface, 1.0, OutlineWidth, start.Position <= view.Caret.Position.BufferPosition.Position && end.Position >= view.Caret.Position.BufferPosition);
					adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, visibleSpan, null, new VisualHost(outline_text), null);
				}

				text_blocks.Clear();
			}
		}

		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			if (!EnableOutline && !EnableBloom && !EnableDropShadows)
				return;
			// Only update lines that are newly formatted or resized
			if (e.NewOrReformattedLines.Count == 0)
				return;

			List<(string text, double left, double top, Color color)> text_blocks = new List<(string text, double left, double top, Color color)>();
			foreach (var line in e.NewOrReformattedLines)
			{
				ProcessLine(line, text_blocks);

				if (text_blocks.Count > 0)
				{
					SnapshotPoint start = line.Start;
					SnapshotPoint end = line.End;

					var visibleSpan = new SnapshotSpan(start, end);
					adornmentLayer.RemoveAdornmentsByVisualSpan(visibleSpan);

					var outline_color = Colors.Red;// Brighten(foregroundBrush.Color, Outline1Brightness);
					var outline_text = CreateOutlinedTextVisual(text_blocks.ToArray(), view.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize,
						view.FormattedLineSource.DefaultTextProperties.Typeface, 1.0, OutlineWidth, start.Position <= view.Caret.Position.BufferPosition.Position && end.Position >= view.Caret.Position.BufferPosition);
					adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, visibleSpan, null, new VisualHost(outline_text), null);
				}

				text_blocks.Clear();
			}
		}

		private static Color Brighten(Color src, double factor)
		{
			byte r = (byte)Math.Min(255, (double)src.R * factor);
			byte g = (byte)Math.Min(255, (double)src.G * factor);
			byte b = (byte)Math.Min(255, (double)src.B * factor);
			return Color.FromArgb(src.A, r, g, b);
		}

		private void ProcessLine(ITextViewLine line, List<(string text, double left, double top, Color color)> text_blocks)
		{
			var lineSpan = line.Extent;

			// Get all tags intersecting this line
			var tags = tagAggregator.GetTags(lineSpan);

			string line_text = lineSpan.GetText();
			//if (line_text.Contains("\r\n") || string.IsNullOrEmpty(line_text))
			//	return;

			int tag_count = 0;
			bool break_at_end = false;
			int last_start = -1;
			foreach (var tag in tags)
			{
				var span = tag.Span.GetSpans(view.TextSnapshot)[0];
				if (span.Start.Position < lineSpan.Start.Position || span.End.Position > lineSpan.End.Position)
				{
					span = lineSpan;
					break_at_end = true;
				}

				var classificationType = tag.Tag.ClassificationType;
				var props = formatMap.GetTextProperties(classificationType);
				var foregroundBrush = props.ForegroundBrush as SolidColorBrush;
				string name = classificationType.Classification.ToLowerInvariant();


				if ((span.Start.Position < last_start || span.Start.Position >= lineSpan.End.Position) && !name.Contains("string"))// && name != "string" && name != "string - escape character")
				{
					break;
				}

				List<SnapshotSpan> spans_to_use = new List<SnapshotSpan>();
				if (span.GetText().Contains("\t"))
				{
					// Break the text up into spans according to the tabs.
					int offset = 0;
					while (span.GetText().Contains("\t"))
					{
						int tab_index = span.GetText().IndexOf("\t");
						if (tab_index == -1)
							break;
						var new_span = new SnapshotSpan(span.Start + offset, span.Start + offset + tab_index);
						if (!string.IsNullOrEmpty(new_span.GetText()))
						{
							spans_to_use.Add(new_span);
						}
						offset = 0;// tab_index + 1;
						span = new SnapshotSpan(span.Start + tab_index + 1, span.End);
					}

					if (span.GetText().Length > 0)
					{
						spans_to_use.Add(span);
					}
				}
				else
				{
					spans_to_use.Add(span);
				}

				for (int i = 0; i < spans_to_use.Count; i++)
				{
					span = spans_to_use[i];
					string text = span.GetText();
					if (text.Contains("\r\n"))
					{
						text = text.Substring(0, text.IndexOf("\r\n"));
					}
					if (span.Start.Position == last_start)
						continue;
					last_start = span.Start.Position;



					if (string.IsNullOrWhiteSpace(text))
						continue;
					if (text.EndsWith("\r\n"))
					{
						break_at_end = true;
						text = text.Substring(0, text.Length - 2);
					}

					tag_count++;

					var bounds = view.TextViewLines.GetMarkerGeometry(span);

					if (foregroundBrush == null)
						continue;

					text_blocks.Add((text, bounds.Bounds.Left, bounds.Bounds.Top, foregroundBrush.Color));
				}

				if (break_at_end)
					break;
			}



			if (tag_count == 0)
			{
				adornmentLayer.RemoveAdornmentsByVisualSpan(lineSpan);
			}
		}

		private Dictionary<string, Geometry> textGeometryCache = new Dictionary<string, Geometry>();
		private Dictionary<string, ImageSource> textBitmapCache = new Dictionary<string, ImageSource>();

		private DrawingVisual CreateOutlinedTextVisual((string text, double left, double top, Color color)[] text_blocks, double fontSize, Typeface typeface, double opacity, double thickness, bool by_character)
		{
			var options = CodeEffectsExtensionPackage.Options;
			double quality = CodeEffectsExtensionPackage.Options.Quality;
			double zoom = view.ZoomLevel / 100.0 * quality;
			var visual = new DrawingVisual();
			//textBitmapCache.Clear();


			using (var dc = visual.RenderOpen())
			{
				double padding = Math.Max(BloomRadius * 2, thickness * 2.0);

				foreach (var block in text_blocks)
				{
					ImageSource add_or_get_string(string text)
					{
						string charCacheKey = $"{text}_{fontSize}_{typeface}_{block.color}_{opacity}_{thickness}";
						if (!textBitmapCache.TryGetValue(charCacheKey, out var charBitmap))
						{
							Color bloomColor = Brighten(block.color, options.BloomBrightness);
							Color outlineColor = Brighten(block.color, options.OutlineBrightness);
							Color shadowColor = Brighten(block.color, options.ShadowBrightness);

							RenderTargetBitmap rtb = null;

							// Render the character to a DrawingVisual
							var tempVisual = new DrawingVisual();
							if (EnableBloom)
							{
								using (var tempDC = tempVisual.RenderOpen())
								{
									var formattedText = new FormattedText(
										text,
										System.Globalization.CultureInfo.CurrentUICulture,
										FlowDirection.LeftToRight,
										typeface,
										fontSize,
										Brushes.Transparent,
										1.0); // pixelsPerDip

									var geometry = formattedText.BuildGeometry(new Point(0, 0));
									geometry.Freeze();

									var brush = new SolidColorBrush(bloomColor) { Opacity = opacity };
									var pen = new Pen(brush, thickness);

									tempDC.PushTransform(new TranslateTransform(padding, padding));
									tempDC.PushOpacity(options.BloomOpacity);

									tempDC.DrawGeometry(null, pen, geometry);

									tempDC.Pop();
									tempDC.Pop();
								}

								BlurEffect blur = new BlurEffect();
								blur.KernelType = KernelType.Gaussian;
								blur.Radius = BloomRadius;
								blur.RenderingBias = RenderingBias.Performance;
								tempVisual.Effect = blur;

								var bounds = tempVisual.ContentBounds;
								int width = (int)Math.Ceiling(bounds.Width * zoom + padding * quality * 2.5);
								int height = (int)Math.Ceiling(bounds.Height * zoom + padding * quality * 2.5);

								if (width == 0 || height == 0)
									return null; // Skip empty render

								rtb = new RenderTargetBitmap(
									width, height,
									96 * zoom, 96 * zoom,
									PixelFormats.Pbgra32);
								rtb.Render(tempVisual);
							}



							if (EnableDropShadows)
							{
								using (var tempDC = tempVisual.RenderOpen())
								{
									var formattedText = new FormattedText(
										text,
										System.Globalization.CultureInfo.CurrentUICulture,
										FlowDirection.LeftToRight,
										typeface,
										fontSize,
										new SolidColorBrush(shadowColor),
										1.0); // pixelsPerDip

									var geometry = formattedText.BuildGeometry(new Point(0, 0));
									geometry.Freeze();

									tempDC.PushTransform(new TranslateTransform(padding, padding));
									double sx = Math.Cos(ShadowAngle * Math.PI / 180.0) * ShadowDistance;
									double sy = -Math.Sin(ShadowAngle * Math.PI / 180.0) * ShadowDistance;

									tempDC.PushOpacity(ShadowOpacity);
									tempDC.DrawText(formattedText, new Point(sx, sy));

									tempDC.Pop();
									tempDC.Pop();
								}

								if (CodeEffectsExtensionPackage.Options.ShadowBlurRadius >= 1.0)
								{
									BlurEffect blur = new BlurEffect();
									blur.KernelType = KernelType.Gaussian;
									blur.Radius = ShadowBlurRadius;
									blur.RenderingBias = RenderingBias.Performance;
									tempVisual.Effect = blur;
								}
								else
								{
									tempVisual.Effect = null;
								}

								if (!EnableBloom)
								{
									var bounds = tempVisual.ContentBounds;
									int width = (int)Math.Ceiling(bounds.Width * zoom + padding * quality * 2.5);
									int height = (int)Math.Ceiling(bounds.Height * zoom + padding * quality * 2.5);

									if (width == 0 || height == 0)
										return null; // Skip empty render

									rtb = new RenderTargetBitmap(
										width, height,
										96 * zoom, 96 * zoom,
										PixelFormats.Pbgra32);
								}
								rtb.Render(tempVisual);
							}



							if (EnableOutline)
							{
								using (var tempDC = tempVisual.RenderOpen())
								{
									var formattedText = new FormattedText(
										text,
										System.Globalization.CultureInfo.CurrentUICulture,
										FlowDirection.LeftToRight,
										typeface,
										fontSize,
										Brushes.Transparent,
										1.0); // pixelsPerDip

									var geometry = formattedText.BuildGeometry(new Point(0, 0));
									geometry.Freeze();

									var brush = new SolidColorBrush(outlineColor) { Opacity = opacity };
									var pen = new Pen(brush, thickness);

									tempDC.PushTransform(new TranslateTransform(padding, padding));
									tempDC.PushOpacity(options.OutlineOpacity);

									tempDC.DrawGeometry(null, pen, geometry);

									tempDC.Pop();
									tempDC.Pop();
								}

								if (CodeEffectsExtensionPackage.Options.OutlineBlurRadius >= 1.0)
								{
									BlurEffect blur = new BlurEffect();
									blur.KernelType = KernelType.Gaussian;
									blur.Radius = CodeEffectsExtensionPackage.Options.OutlineBlurRadius;
									blur.RenderingBias = RenderingBias.Performance;
									tempVisual.Effect = blur;
								}
								else
								{
									tempVisual.Effect = null;
								}
								if (!EnableBloom && !EnableDropShadows)
								{
									var bounds = tempVisual.ContentBounds;
									int width = (int)Math.Ceiling(bounds.Width * zoom + padding * quality * 2.5);
									int height = (int)Math.Ceiling(bounds.Height * zoom + padding * quality * 2.5);

									if (width == 0 || height == 0)
										return null; // Skip empty render

									rtb = new RenderTargetBitmap(
										width, height,
										96 * zoom, 96 * zoom,
										PixelFormats.Pbgra32);
								}
								rtb.Render(tempVisual);

							}

							charBitmap = rtb;
							textBitmapCache[charCacheKey] = charBitmap;
						}

						return charBitmap;
					}


					double xOffset = block.left;

					if (by_character)
					{
						foreach (char ch in block.text)
						{
							if (char.IsWhiteSpace(ch) || (int)ch < 0x20)
							{
								// Update xOffset for next character
								var formatted_space = new FormattedText(
									ch.ToString(),
									System.Globalization.CultureInfo.CurrentUICulture,
									FlowDirection.LeftToRight,
									typeface,
									fontSize,
									Brushes.Black,
									1.0);

								xOffset += formatted_space.WidthIncludingTrailingWhitespace;
								continue;
							}

							var charBitmap = add_or_get_string(ch.ToString());
							if (charBitmap == null)
								continue;

							// Draw the cached character bitmap
							double drawLeft = xOffset - padding;
							double drawTop = block.top - padding;

							dc.DrawImage(charBitmap, new Rect(drawLeft, drawTop, charBitmap.Width, charBitmap.Height));

							// Update xOffset for next character
							var formattedChar = new FormattedText(
								ch.ToString(),
								System.Globalization.CultureInfo.CurrentUICulture,
								FlowDirection.LeftToRight,
								typeface,
								fontSize,
								Brushes.Black,
								1.0);

							xOffset += formattedChar.WidthIncludingTrailingWhitespace;
						}
					}
					else
					{
						var charBitmap = add_or_get_string(block.text);
						if (charBitmap != null)
						{
							// Draw the cached character bitmap
							double drawLeft = block.left - padding;
							double drawTop = block.top - padding;
							dc.DrawImage(charBitmap, new Rect(drawLeft, drawTop, charBitmap.Width, charBitmap.Height));
						}
					}
				}
			}

			return visual;
		}



	}

	public class VisualHost : FrameworkElement
	{
		private readonly Visual _visual;

		public VisualHost(Visual visual)
		{
			this._visual = visual;
			this.IsHitTestVisible = false; // prevent blocking mouse/cursor
		}

		protected override int VisualChildrenCount => 1;

		protected override Visual GetVisualChild(int index)
		{
			if (index != 0)
				throw new ArgumentOutOfRangeException();
			return _visual;
		}
	}
}
