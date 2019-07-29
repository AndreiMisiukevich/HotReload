namespace Xamarin.Forms.HotReload.Sample.Pages
{
    [HotReloader.CSharpVisual]
    public class CodeContentPage : ContentPage, ICsharpRestorable
    {
        private Color _backColor;

        public CodeContentPage(Color backColor)
        {
            BackgroundColor = _backColor = backColor;

            var button = new Button
            {
                FontSize = 30,
                Text = "Button Click",
                Command = new Command(OnClicked)
            };

            button.SetBinding(Button.TextProperty, "ButtonText");

            Content = new StackLayout
            {
                BackgroundColor = Color.Red,
                Margin = 50,
                Children =
                {
                    button
                }
            };

            this.SetBinding(TitleProperty, "Title");
        }

        void OnClicked()
        {
            Navigation.PushAsync(new AnotherCodeContentPage());
        }

        public object[] ConstructorRestoringParameters => new object[] { _backColor };
    }
}
