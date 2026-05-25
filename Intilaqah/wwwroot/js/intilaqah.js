document.addEventListener('DOMContentLoaded', function() {
    // 1. Sidebar toggle for mobile
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.il-sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (sidebarToggle && sidebar && overlay) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('open');
            overlay.classList.toggle('show');
        });

        overlay.addEventListener('click', function() {
            sidebar.classList.remove('open');
            overlay.classList.remove('show');
        });
    }

    // 2. Active nav link
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.il-sidebar .nav-link');
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });

    // 3. Auto-dismiss .il-toast alerts
    const toasts = document.querySelectorAll('.il-toast');
    toasts.forEach(toast => {
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => {
                toast.remove();
            }, 500); // Wait for transition
        }, 4500);
    });

    // 4. data-confirm attribute
    const confirmButtons = document.querySelectorAll('[data-confirm]');
    confirmButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            const message = this.getAttribute('data-confirm');
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });

    // 5. tableSearch input
    const searchInput = document.getElementById('tableSearch');
    if (searchInput) {
        searchInput.addEventListener('keyup', function() {
            const filter = this.value.toLowerCase();
            const rows = document.querySelectorAll('.il-table tbody tr');
            
            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                if (text.includes(filter)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }
});
