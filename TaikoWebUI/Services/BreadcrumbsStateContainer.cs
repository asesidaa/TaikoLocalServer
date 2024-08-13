namespace TaikoWebUI.Services
{
    public class BreadcrumbsStateContainer
    {
        public List<BreadcrumbItem> breadcrumbs = new();

        public event Action? OnChange;

        public void SetBreadcrumbs(List<BreadcrumbItem> _breadcrumbs)
        {
            breadcrumbs = _breadcrumbs;
            NotifyStateChanged();
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
