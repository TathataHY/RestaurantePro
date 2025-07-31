using RestaurantePro.Mobile.Services;

namespace RestaurantePro.Mobile.Controls
{
    public partial class LocalizedLabel : ContentView
    {
        public static readonly BindableProperty TextKeyProperty =
            BindableProperty.Create(nameof(TextKey), typeof(string), typeof(LocalizedLabel), string.Empty, propertyChanged: OnTextKeyChanged);

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(LocalizedLabel), Colors.Black, propertyChanged: OnTextColorChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(LocalizedLabel), 14.0, propertyChanged: OnFontSizeChanged);

        public static readonly BindableProperty FontAttributesProperty =
            BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(LocalizedLabel), FontAttributes.None, propertyChanged: OnFontAttributesChanged);

        public static new readonly BindableProperty HorizontalOptionsProperty =
            BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(LocalizedLabel), LayoutOptions.Start, propertyChanged: OnHorizontalOptionsChanged);

        public static new readonly BindableProperty VerticalOptionsProperty =
            BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(LocalizedLabel), LayoutOptions.Start, propertyChanged: OnVerticalOptionsChanged);

        public string TextKey
        {
            get => (string)GetValue(TextKeyProperty);
            set => SetValue(TextKeyProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public FontAttributes FontAttributes
        {
            get => (FontAttributes)GetValue(FontAttributesProperty);
            set => SetValue(FontAttributesProperty, value);
        }

        public new LayoutOptions HorizontalOptions
        {
            get => (LayoutOptions)GetValue(HorizontalOptionsProperty);
            set => SetValue(HorizontalOptionsProperty, value);
        }

        public new LayoutOptions VerticalOptions
        {
            get => (LayoutOptions)GetValue(VerticalOptionsProperty);
            set => SetValue(VerticalOptionsProperty, value);
        }

        public LocalizedLabel()
        {
            InitializeComponent();
            LocalizationService.Instance.CultureChanged += OnCultureChanged;
            UpdateText();
        }

        private static void OnTextKeyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label)
            {
                label.UpdateText();
            }
        }

        private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label && newValue is Color color)
            {
                label.MainLabel.TextColor = color;
            }
        }

        private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label && newValue is double fontSize)
            {
                label.MainLabel.FontSize = fontSize;
            }
        }

        private static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label && newValue is FontAttributes fontAttributes)
            {
                label.MainLabel.FontAttributes = fontAttributes;
            }
        }

        private static void OnHorizontalOptionsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label && newValue is LayoutOptions options)
            {
                label.MainLabel.HorizontalOptions = options;
            }
        }

        private static void OnVerticalOptionsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LocalizedLabel label && newValue is LayoutOptions options)
            {
                label.MainLabel.VerticalOptions = options;
            }
        }

        private void OnCultureChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (!string.IsNullOrEmpty(TextKey))
            {
                MainLabel.Text = LocalizationService.Instance.GetString(TextKey);
            }
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            
            if (Handler == null)
            {
                // Control is being unloaded
                LocalizationService.Instance.CultureChanged -= OnCultureChanged;
            }
        }
    }
} 