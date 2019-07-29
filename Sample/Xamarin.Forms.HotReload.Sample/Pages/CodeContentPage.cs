namespace Xamarin.Forms.HotReload.Sample.Pages
{
    [HotReloader.CSharpVisual]
    public class CodeContentPage : ContentPage, ICsharpRestorable
    {
        private Color _backColor;

        public CodeContentPage(Color backColor)
        {
            BackgroundColor = _backColor = backColor;
            Content = new StackLayout
            {
                BackgroundColor = Color.Red,
                Margin = 50,
                Children =
                {
                    new Button
                    {
                        FontSize = 40,
                        Text = "Button Click"
                    }
                }
            };

            this.SetBinding(TitleProperty, "Title");
        }

        public object[] ConstructorRestoringParameters => new object[] { _backColor };
    }
}
