using System;
using WpfAnimatedGif;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pixelator.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ToolDescriptionControl.xaml
    /// </summary>
    public partial class ToolDescriptionControl : UserControl
    {
        public ToolDescriptionControl()
        {
            InitializeComponent();
        }

        public void SetGifAndDescription(Uri gifUri, string description, string toolName)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = gifUri;
            image.EndInit();
            ImageBehavior.SetAnimatedSource(ToolGif, image);

            DescriptionText.Text = description;
            ToolName.Text = toolName;
        }
    }
}
