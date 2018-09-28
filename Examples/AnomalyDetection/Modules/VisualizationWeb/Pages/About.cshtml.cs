﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VisualizationWeb.Pages
{
    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "Your application description page.";
        }
    }
}