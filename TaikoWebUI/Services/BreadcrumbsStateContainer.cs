namespace TaikoWebUI.Services
{
    public class BreadcrumbsStateContainer
    {
        public List<BreadcrumbItem> breadcrumbs = new();

        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
