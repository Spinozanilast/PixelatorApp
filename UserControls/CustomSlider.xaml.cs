using System;
using System.Windows.Controls;

namespace Pixelator.UserControls
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class CustomSlider : UserControl
    {
        public event EventHandler ButtonClicked;
        public CustomSlider()
        {
            InitializeComponent();
        }

        public void SetToolSettings(string toolName, int sliderMinValue, int sliderMaxValue)
        {
            Slider.Minimum = sliderMinValue;
            Slider.Maximum = sliderMaxValue;
            SliderPropertyName.Text = toolName;
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ButtonClicked != null) ButtonClicked.Invoke(this, EventArgs.Empty);
        }
    }
}
