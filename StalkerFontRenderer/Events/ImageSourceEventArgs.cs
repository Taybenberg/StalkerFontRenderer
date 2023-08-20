using System;
using System.Windows.Media;

namespace StalkerFontRenderer.Events;

public class ImageSourceEventArgs : EventArgs
{
    public ImageSource ImageSource { get; }

    public ImageSourceEventArgs(ImageSource imageSource) : base() 
    {
        ImageSource = imageSource;
    }
}
