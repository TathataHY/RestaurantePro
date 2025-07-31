using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantePro.Mobile.Controls;

public partial class SkeletonLoader : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty SkeletonTypeProperty =
        BindableProperty.Create(nameof(SkeletonType), typeof(SkeletonType), typeof(SkeletonLoader), SkeletonType.Card, propertyChanged: OnSkeletonTypeChanged);

    public SkeletonType SkeletonType
    {
        get => (SkeletonType)GetValue(SkeletonTypeProperty);
        set => SetValue(SkeletonTypeProperty, value);
    }

    public SkeletonLoader()
    {
        InitializeComponent();
        BuildSkeleton();
    }

    private static void OnSkeletonTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SkeletonLoader loader)
        {
            loader.BuildSkeleton();
        }
    }

    private void BuildSkeleton()
    {
        SkeletonContainer.Children.Clear();

        switch (SkeletonType)
        {
            case SkeletonType.Card:
                BuildCardSkeleton();
                break;
            case SkeletonType.List:
                BuildListSkeleton();
                break;
            case SkeletonType.Table:
                BuildTableSkeleton();
                break;
        }
    }

    private void BuildCardSkeleton()
    {
        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Stroke = Color.FromArgb("#E9ECEF"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16),
            Margin = new Thickness(16, 8)
        };

        var content = new VerticalStackLayout { Spacing = 12 };

        // Título skeleton
        var titleSkeleton = CreateSkeletonBox(200, 24, 8);
        content.Children.Add(titleSkeleton);

        // Subtítulo skeleton
        var subtitleSkeleton = CreateSkeletonBox(150, 16, 8);
        content.Children.Add(subtitleSkeleton);

        // Contenido skeleton
        var contentSkeleton = CreateSkeletonBox(250, 16, 8);
        content.Children.Add(contentSkeleton);

        card.Content = content;
        SkeletonContainer.Children.Add(card);
    }

    private void BuildListSkeleton()
    {
        var listContainer = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(16) };

        for (int i = 0; i < 5; i++)
        {
            var item = new HorizontalStackLayout { Spacing = 12 };

            // Avatar skeleton
            var avatarSkeleton = CreateSkeletonBox(40, 40, 20);
            item.Children.Add(avatarSkeleton);

            // Contenido skeleton
            var contentStack = new VerticalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
            var titleSkeleton = CreateSkeletonBox(180, 16, 8);
            var subtitleSkeleton = CreateSkeletonBox(120, 12, 6);
            contentStack.Children.Add(titleSkeleton);
            contentStack.Children.Add(subtitleSkeleton);
            item.Children.Add(contentStack);

            listContainer.Children.Add(item);
        }

        SkeletonContainer.Children.Add(listContainer);
    }

    private void BuildTableSkeleton()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Star }
            },
            ColumnSpacing = 8,
            RowSpacing = 8,
            Padding = new Thickness(16)
        };

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var tableSkeleton = CreateSkeletonBox(80, 80, 8);
                grid.Children.Add(tableSkeleton);
                Grid.SetRow(tableSkeleton, row);
                Grid.SetColumn(tableSkeleton, col);
            }
        }

        SkeletonContainer.Children.Add(grid);
    }

    private View CreateSkeletonBox(double width, double height, double cornerRadius)
    {
        var skeleton = new Border
        {
            BackgroundColor = Color.FromArgb("#E9ECEF"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = (float)cornerRadius },
            WidthRequest = width,
            HeightRequest = height
        };

        return skeleton;
    }
}

public enum SkeletonType
{
    Card,
    List,
    Table
} 