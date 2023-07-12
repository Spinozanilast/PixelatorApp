using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pixelator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.windowWidth = Screen.PrimaryScreen.Bounds.Width;

            watchIconSource = new BitmapImage(new Uri("/Assets/Pictures/WatchIcon.png", UriKind.Relative));
            settingsIconSource = new BitmapImage(new Uri("/Assets/Pictures/SettingsIcon.png", UriKind.Relative));
        }

        /// <summary>
        /// Get the Filter string for all supported image types.
        /// To be used in the FileDialog class Filter Property.
        /// </summary>
        /// <returns></returns>
        public static string GetImageFilter()
        {
            string imageExtensions = string.Empty;
            string separator = "";
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            Dictionary<string, string> imageFilters = new Dictionary<string, string>();
            foreach (ImageCodecInfo codec in codecs)
            {
                imageExtensions = $"{imageExtensions}{separator}{codec.FilenameExtension.ToLower()}";
                separator = ";";
                imageFilters.Add($"{codec.FormatDescription} files ({codec.FilenameExtension.ToLower()})", codec.FilenameExtension.ToLower());
            }
            string result = string.Empty;
            separator = "";
            foreach (KeyValuePair<string, string> filter in imageFilters)
            {
                result += $"{separator}{filter.Key}|{filter.Value}";
                separator = "|";
            }
            if (!string.IsNullOrEmpty(imageExtensions))
            {
                result += $"{separator}Image files|{imageExtensions}";
            }
            return result;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException) 
            {
                
            }
        }
        
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonMinMax_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState) 
            {
                case WindowState.Normal:
                    MaximizeBorder.BorderThickness = new Thickness(5);
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    MaximizeBorder.BorderThickness = new Thickness(0);
                    this.WindowState = WindowState.Normal;
                    break;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WorkspaceBackground_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];
                    this.selectedImagePath = imagePath;
                    SetTextBoxState(true);
                    BitmapImage image = new BitmapImage(new Uri(imagePath));
                    ImageToPixelate.ImageSource = image;
                    imageRectangle.IsHitTestVisible = true;
                }
            }
        }

        private void SetTextBoxState(bool isEnabled)
        {
            if (isEnabled) FilenameTextBox.Text = this.selectedImagePath;
            FilenameTextBox.IsEnabled = isEnabled ? true : false;
        }

        private void WorkspaceBackground_FolderDialogBrowser(object sender, MouseButtonEventArgs e)
        {
            if (this.isImageOpened) return;

            using(var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select an image to pixelate";
                dialog.Multiselect = false;
                dialog.Filter = GetImageFilter();
                DialogResult dialogInstance = dialog.ShowDialog();

                if (dialogInstance == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dialog.FileName;
                    this.selectedImagePath = selectedPath;
                    SetTextBoxState(true);
                    BitmapImage image = new BitmapImage(new Uri(selectedPath));
                    ImageToPixelate.ImageSource  = image;
                    isImageOpened = true;
                    imageRectangle.IsHitTestVisible = true;
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var newSize = e.NewSize;
            if (newSize.Height == MaxHeight &&  newSize.Width > this.windowWidth) 
            {
                MaximizeBorder.BorderThickness = new Thickness(5);
            }
        }

        private void PixelateTool_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var gifUri = new Uri("/Assets/Gifs/PixelateToolGif.gif", UriKind.Relative);
            const string description =
                "The pixelate tool is a feature of the Pixelator app that allows you to apply a pixelation effect to an image.\n" +
                "You can use the slider to adjust the level of pixelation, from low to high.\n" +
                "The pixelate tool is useful for creating artistic effects or hiding sensitive information in an image.\r\n";

            DescriptionControl.SetGifAndDescription(gifUri, description, "Pixelate Tool");
            DescriptionPanel.Visibility = Visibility.Visible;

            if(isToolSettingsShow) return;

            ProcessIcon.Source = watchIconSource;
            ProcessIcon.Visibility = Visibility.Visible;
        }

        private void PixelateTool_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDescriptionFixed = !isDescriptionFixed;

            if (selectedImagePath is null) return;

            CustomSlider.SetToolSettings("Block Size", MinBlockSize, (int)ImageToPixelate.ImageSource.Width);
            CustomSlider.Visibility = CustomSlider.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

            ProcessIcon.Source = settingsIconSource;
            ProcessIcon.Visibility = Visibility.Visible;
        }
        private void PixelateTool_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDescriptionFixed) return;                                                                      
            DescriptionPanel.Visibility = Visibility.Collapsed;
            ProcessIcon.Visibility = Visibility.Collapsed;
                                                                                                   
            if(!(selectedImagePath is null)) isToolSettingsShow = true;
        }

        private void CustomSlider_OnButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath)) return;

            PixelatorTool pixelateTool = new PixelatorTool((int)CustomSlider.Slider.Value);
            ImageToPixelate.ImageSource = pixelateTool.DrawPixelateImage(ImageToPixelate.ImageSource, out tempBitmapPixelate);
        }

        private void ProcessIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isToolSettingsShow) return;

            PixelateTool_MouseLeftButtonDown(sender, e);
            ProcessIcon.Visibility = Visibility.Collapsed;

        }
    }                                                      
}
