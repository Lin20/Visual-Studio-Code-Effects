using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CodeEffectsExtension
{
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
	/// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
	/// </summary>
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("code")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal sealed class CodeEffectsAdornmentTextViewCreationListener : IWpfTextViewCreationListener
	{
		// Disable "Field is never assigned to..." and "Field is never used" compiler's warnings. Justification: the field is used by MEF.
#pragma warning disable 649, 169

		/// <summary>
		/// Defines the adornment layer for the scarlet adornment. This layer is ordered
		/// after the selection layer in the Z-order
		/// </summary>
		[Export(typeof(AdornmentLayerDefinition))]
		[Name("CodeEffectsAdornment")]
		[Order(Before = PredefinedAdornmentLayers.Text)]
		private AdornmentLayerDefinition editorAdornmentLayer;

		[Import]
		internal IClassifierAggregatorService ClassifierAggregatorService { get; set; }

		[Import]
		internal IClassificationFormatMapService FormatMapService { get; set; }

		[Import]
		internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

		[Import]
		internal IViewTagAggregatorFactoryService TagAggregatorFactory { get; set; }

#pragma warning restore 649, 169

		/// <summary>
		/// Instantiates a CodeEffectsAdornment manager when a textView is created.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
		public void TextViewCreated(IWpfTextView textView)
		{
			var tagAggregator = TagAggregatorFactory.CreateTagAggregator<IClassificationTag>(textView);
			var formatMap = FormatMapService.GetClassificationFormatMap(textView);
			var registry = ClassificationRegistry;

			new CodeEffectsAdornment(textView, tagAggregator, formatMap, registry);
		}
	}
}
