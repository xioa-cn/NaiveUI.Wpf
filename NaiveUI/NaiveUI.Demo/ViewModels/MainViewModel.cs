using System.Reflection;
using System.Windows.Controls;

namespace NaiveUI.Demo.ViewModels
{
    internal class MainViewModel
    {
        public List<object?> Content { get; set; } = new();

        public MainViewModel()
        {
            var types = Assembly.Load("NaiveUI.Demo")
                .GetTypes()
                .Where(e =>
                e.BaseType == typeof(UserControl)
                && e.FullName != null
                && e.FullName.Contains("Components")
                && e.FullName.Split('.').Length == 6
                )
                .ToList();
            types.Select(e => new { e, Name = e.FullName?.Split('.')[4].Replace("_","__") }).ToList()
                .ForEach(e => this.Content.Add(e));
        }
    }
}
