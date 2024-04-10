// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1027 // File may only contain a single type
#pragma warning disable SA1202 // File may only contain a single type

namespace RegexMatchVisualizer;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using RegexMatchVisualizer.ObjectSource;

/// <summary>
/// Debugger visualizer provider class for <see cref="Match"/>.
/// </summary>
[VisualStudioContribution]
internal class RegexMatchDebuggerVisualizerProvider : DebuggerVisualizerProvider
{
	/// <inheritdoc/>
	public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration => new("%RegexMatchVisualizer.RegexMatchDebuggerVisualizerProvider.DisplayName%", typeof(Match))
	{
		VisualizerObjectSourceType = new("RegexMatchVisualizer.ObjectSource.RegexMatchObjectSource, RegexMatchObjectSource"),
	};

	/// <inheritdoc/>
	public override async Task<IRemoteUserControl> CreateVisualizerAsync(VisualizerTarget visualizerTarget, CancellationToken cancellationToken)
	{
		var regexMatch = await visualizerTarget.ObjectSource.RequestDataAsync<RegexMatch>(jsonSerializer: null, cancellationToken);
		var r2 = new RegexMatch2
		{
			Groups = regexMatch.Groups,
		};
		return new RegexMatchVisualizerUserControl(r2);
	}
}

[DataContract]
internal class RegexMatch2 : RegexMatch
{

	static RegexMatch2()
	{
		var filePath = System.IO.Path.GetTempFileName();
		System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));
		pathToImage = filePath;
	}

	public static string base64 { get; } = "iVBORw0KGgoAAAANSUhEUgAAAEQAAABECAYAAAA4E5OyAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC6SURBVHhe7dBBCsJAEEXBOXqO5j6HGiGgkoebIIkiVfDX3bwxb+u017Ygg80uyDKWecW2yz9KkBAkBAlBQpAQJAQJQUKQECQEie8EWcaxXUiQECQECUFCkBAkBAlBQpAQJAQJQUKQ2AU57N3zZ+xCgoQgIUgIEoKEICFICBKChCAhSHwW5A8JEoKEICFICBKChCAhSAgSgoQgIUgIEoKEICFICBKChCAhSAgSgoQgIUgIEoLEM4g9ts4709vHjnHNhYQAAAAASUVORK5CYII=";
	public static string pathToImage { get; }

	//Works.
	[DataMember]
	public string ImageUrl { get => pathToImage; }

	//Does not work.
	[DataMember]
	public BitmapSource BitmapSource { get => new BitmapImage(new Uri(pathToImage)); }

	//Does not work.
	[DataMember]
	public Uri ImageUri { get => new Uri(pathToImage); }

	//Does not work.
	[DataMember]
	public string ImageBase64 { get => base64; }

}

public class Base64ImageConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var s = value as string;

		if (s == null)
		{
			return null;
		}

		BitmapImage bi = new BitmapImage();

		bi.BeginInit();
		bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(s));
		bi.EndInit();

		return bi;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
