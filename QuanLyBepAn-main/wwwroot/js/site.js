// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
(function(){
	// Sidebar toggle for mobile
	const sidebarToggle = document.getElementById('sidebarToggle');
	const sidebar = document.querySelector('.sidebar');
	if (sidebarToggle && sidebar) {
		sidebarToggle.addEventListener('click', function(e){
			e.preventDefault();
			sidebar.classList.toggle('open');
		});
	}
// App UI interactions for the Management System
// Sidebar toggle, small UX helpers
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.querySelector('.sidebar');
    const toggle = document.getElementById('sidebarToggle');

    function setCollapsed(collapsed) {
        if (!sidebar) return;
        if (collapsed) sidebar.classList.add('collapsed');
        else sidebar.classList.remove('collapsed');
        try { localStorage.setItem('app.sidebar.collapsed', collapsed ? '1' : '0'); } catch(e){}
    }

    // initialize from storage
    try {
        const stored = localStorage.getItem('app.sidebar.collapsed');
        if (stored === '1') setCollapsed(true);
    } catch(e){}

    if (toggle) {
        toggle.addEventListener('click', function () { setCollapsed(!sidebar.classList.contains('collapsed')); });
    }

    // Close any bootstrap modals when Escape pressed (small convenience)
    document.addEventListener('keydown', function(e){
        if (e.key === 'Escape') {
            var modals = document.querySelectorAll('.modal.show');
            modals.forEach(function(m){
                var modal = bootstrap.Modal.getInstance(m);
                if (modal) modal.hide();
            });
        }
    });
});
	// Close sidebar when clicking outside on small screens
	document.addEventListener('click', function(e){
		if (!sidebar) return;
		if (window.innerWidth <= 767 && sidebar.classList.contains('open')){
			if (!sidebar.contains(e.target) && e.target.id !== 'sidebarToggle'){
				sidebar.classList.remove('open');
			}
		}
	});

	// Close button inside sidebar (if present)
	const sidebarClose = document.getElementById('sidebarClose');
	if (sidebarClose && sidebar) {
		sidebarClose.addEventListener('click', function(e){
			e.preventDefault();
			sidebar.classList.remove('open');
		});
	}

	// Enable Bootstrap tooltips if library available
	if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
		var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
		tooltipTriggerList.forEach(function (el) { new bootstrap.Tooltip(el); });
	}

	// Focus first input when a bootstrap modal opens
	document.addEventListener('shown.bs.modal', function(e){
		try {
			var modal = e.target;
			var first = modal.querySelector('input:enabled, textarea:enabled, select:enabled');
			if (first) first.focus();
		} catch(err) { /* ignore */ }
	});

})();
