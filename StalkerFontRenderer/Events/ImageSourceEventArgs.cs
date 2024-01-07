using System;
using System.Windows.Media;

namespace StalkerFontRenderer.Events;

public class ImageSourceEventArgs(ImageSource imageSource) : EventArgs()
{
    public ImageSource ImageSource { get; } = imageSource;
}