using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StalkerFontRenderer.Services;
using StalkerFontRenderer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace StalkerFontRenderer.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly ISnackbarService _snackbarService;
    private FontService? _fontService;

    [ObservableProperty]
    private bool _isControlsEnabled;

    [ObservableProperty]
    private IEnumerable<string> _fontNames;

    [ObservableProperty]
    private string _selectedFontName;

    [ObservableProperty]
    private ImageSource _fontTextureSource;

    [ObservableProperty]
    private ImageSource _textImageSource;

    [ObservableProperty]
    private SolidColorBrush _imageBackgroundColor;

    [ObservableProperty]
    private SolidColorBrush _imageTextColor;

    [ObservableProperty]
    private string _imageText;

    [ObservableProperty]
    private bool _copyWithTransparentBackground;

    [ObservableProperty]
    private int _minimalImageHeight;

    [ObservableProperty]
    private int _imageHeight;

    [ObservableProperty]
    private int _minimalImageWidth;

    [ObservableProperty]
    private int _imageWidth;

    [ObservableProperty]
    private int _minimalLineHeight;

    [ObservableProperty]
    private int _lineHeight;

    [ObservableProperty]
    private int _minimalCharacterSpacing;

    [ObservableProperty]
    private int _characterSpacing;

    public MainWindowViewModel(ISnackbarService service)
    {
        _snackbarService = service;

        var imageSource = GetImageSourceFromUri("pack://application:,,,/Assets/font.png");
        _fontTextureSource = imageSource;
        _textImageSource = imageSource;

        _imageBackgroundColor = Color.FromArgb(255, 0, 0, 0).ToBrush();
        _imageTextColor = Color.FromArgb(255, 255, 255, 255).ToBrush();

        _fontNames = Enumerable.Empty<string>();

        _selectedFontName = string.Empty;
        _imageText = string.Empty;

        _copyWithTransparentBackground = true;

        _minimalImageHeight = 128;
        _imageHeight = _minimalImageHeight * 4;

        _minimalImageWidth = 128;
        _imageWidth = _minimalImageWidth * 4;

        _minimalCharacterSpacing = -1;
        _characterSpacing = 0;
    }

    [RelayCommand]
    private void OnChangeTheme() =>
        ApplicationThemeManager.Apply(ApplicationThemeManager.GetAppTheme() is ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark);

    [RelayCommand]
    private Task OnAboutAppAsync()
    {
        var aboutAppPage = new AboutAppPage();

        var messageBox = new Wpf.Ui.Controls.MessageBox()
        {
            Title = aboutAppPage.Title,
            Content = aboutAppPage,
            CloseButtonText = "Закрити"
        };

        return messageBox.ShowDialogAsync();
    }

    [RelayCommand]
    private Task OnLoadFontsAsync()
    {
        var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() is not DialogResult.Cancel)
            return InitFontServiceAsync(dialog.SelectedPath);

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnCopyToClipboardAsync()
    {
        if (_fontService is null)
            return;

        using var stream = await _fontService.RenderTextImageAsync(CopyWithTransparentBackground).ConfigureAwait(false);

        var data = new DataObject();
        data.SetData("PNG", false, stream);

        Clipboard.SetDataObject(data, true);
    }

    [RelayCommand]
    private Task OnChangeTextColorAsync()
    {
        if (_fontService is null)
            return Task.CompletedTask;

        if (GetColor() is Color color)
        {
            ImageTextColor = color.ToBrush();
            return _fontService!.SetTextColorAsync(color);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OnChangeBackgroundColorAsync()
    {
        if (_fontService is null)
            return Task.CompletedTask;

        if (GetColor() is Color color)
        {
            ImageBackgroundColor = color.ToBrush();
            return _fontService!.SetBackgroundColorAsync(color);
        }

        return Task.CompletedTask;
    }

    async partial void OnSelectedFontNameChanged(string value)
    {
        if (_fontService is null)
            return;

        IsControlsEnabled = false;

        await _fontService.SetFontAsync(value).ConfigureAwait(false);
        MinimalLineHeight = _fontService.LineHeight;

        await _fontService.SetBackgroundColorAsync(ImageBackgroundColor.Color).ConfigureAwait(false);
        await _fontService.SetTextImageHeightAsync(ImageHeight).ConfigureAwait(false);
        await _fontService.SetTextImageWidthAsync(ImageWidth).ConfigureAwait(false);
        await _fontService.SetTextImageCharacterSpacingAsync(CharacterSpacing).ConfigureAwait(false);
        await _fontService.SetTextImageLineHeightAsync(LineHeight).ConfigureAwait(false);
        await _fontService.SetImageTextAsync(ImageText).ConfigureAwait(false);

        IsControlsEnabled = true;
    }

    async partial void OnImageTextChanged(string value)
    {
        if (_fontService is not null)
            await _fontService.SetImageTextAsync(value, true).ConfigureAwait(false);
    }

    async partial void OnImageHeightChanged(int value)
    {
        if (_fontService is not null)
            await _fontService.SetTextImageHeightAsync(value).ConfigureAwait(false);
    }

    async partial void OnImageWidthChanged(int value)
    {
        if (_fontService is not null)
            await _fontService.SetTextImageWidthAsync(value).ConfigureAwait(false);
    }

    async partial void OnCharacterSpacingChanged(int value)
    {
        if (_fontService is not null)
            await _fontService.SetTextImageCharacterSpacingAsync(value).ConfigureAwait(false);
    }

    async partial void OnLineHeightChanged(int value)
    {
        if (_fontService is not null)
            await _fontService.SetTextImageLineHeightAsync(value).ConfigureAwait(false);
    }

    partial void OnMinimalLineHeightChanged(int value)
    {
        if (LineHeight < value)
        {
            LineHeight = value;
        }
    }

    public void Dispose()
    {
        if (_fontService is not null)
        {
            _fontService.TextImageUpdated -= OnTextImageUpdated;
            _fontService.FontTextureUpdated -= OnFontTextureUpdated;

            _fontService.Dispose();
            _fontService = null;
        }
    }

    private Task InitFontServiceAsync(string fontsPath)
    {
        Dispose();

        _fontService = new(fontsPath);
        _fontService.FontTextureUpdated += OnFontTextureUpdated;
        _fontService.TextImageUpdated += OnTextImageUpdated;
        _fontService.ImageNotCompleted += OnImageNotCompleted;

        FontNames = _fontService.FontNames;
        return _fontService.SetTextColorAsync(ImageTextColor.Color);
    }

    private Color? GetColor()
    {
        var dialog = new ColorDialog();
        if (dialog.ShowDialog() is DialogResult.Cancel)
            return null;

        var color = dialog.Color;
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private ImageSource GetImageSourceFromUri(string uri)
    {
        var imageSource = new BitmapImage();

        imageSource.BeginInit();
        imageSource.UriSource = new Uri(uri);
        imageSource.EndInit();

        return imageSource;
    }

    private void OnTextImageUpdated(object? sender, Events.ImageSourceEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() => TextImageSource = e.ImageSource);
    }

    private void OnFontTextureUpdated(object? sender, Events.ImageSourceEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() => FontTextureSource = e.ImageSource);
    }

    private void OnImageNotCompleted(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            _snackbarService.Show(
                "Увага",
                "Збільште роздільну здатність зображення, оскільки текст не поміщається на зображенні.",
                ControlAppearance.Danger, 
                new SymbolIcon(SymbolRegular.SlideSize24),
                TimeSpan.FromSeconds(2.5)));
    }
}
