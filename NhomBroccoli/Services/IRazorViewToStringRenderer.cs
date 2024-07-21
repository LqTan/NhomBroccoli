namespace NhomBroccoli.Services
{
    public interface IRazorViewToStringRenderer
    {
        public Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
