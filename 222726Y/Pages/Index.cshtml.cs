using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _222726Y.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpContextAccessor contxt;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            contxt = httpContextAccessor;
        }

        public void OnGet()
        {
            var id = contxt.HttpContext.Session.GetString("id");
            Console.WriteLine(id);
            Console.WriteLine("here");
        }
    }
}