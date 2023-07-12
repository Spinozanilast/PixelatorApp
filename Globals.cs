using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixelator
{
    partial class MainWindow: Window
    {
        //Image to pixelate info

        /// <summary>
        /// Gets the info about image was open or not.
        /// </summary>
        private bool isImageOpened = false;

        /// <summary>
        /// Last open image file path.
        /// </summary>
        private string selectedImagePath;

        /// <summary>
        /// Temporary (save currently pixelized image) bitmap image keeper for future saving on path user would.
        /// </summary>
        private Bitmap tempBitmapPixelate;

        /// <summary>
        /// Give the info about appearing Description panel.
        /// </summary>
        private bool isDescriptionFixed = false;

        /// <summary>
        /// Watch Icon, showed on tool mouse over
        /// </summary>
        private ImageSource watchIconSource = null;

        /// <summary>
        /// Settings Icon, showed on tool mouse over
        /// </summary>
        private ImageSource settingsIconSource = null;

        /// <summary>
        /// Monitor screen width.
        /// </summary>
        private int windowWidth;

        /// <summary>
        /// Show the tool setting showed or not.
        /// </summary>
        private bool isToolSettingsShow = false;


        /// <summary>
        /// Minimum block size which consists of 1 pixel.
        /// </summary>
        private const int MinBlockSize = 1;
    }
}
