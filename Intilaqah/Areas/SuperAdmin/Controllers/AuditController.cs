using Intilaqah.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? entity = null,
            string? logAction = null,
            int     page   = 1)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entity))
                query = query.Where(a => a.EntityName == entity);
            if (!string.IsNullOrEmpty(logAction))
                query = query.Where(a => a.Action == logAction);

            var total = await query.CountAsync();
            var logs  = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * 50)
                .Take(50)
                .ToListAsync();

            ViewBag.TotalCount   = total;
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / 50.0);
            ViewBag.EntityFilter = entity;
            ViewBag.ActionFilter = logAction;

            return View(logs);
        }
    }
}
