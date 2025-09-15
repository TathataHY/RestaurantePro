// ===== SIDEBAR TOGGLE FUNCTIONALITY =====

window.toggleSidebar = function() {
    console.log('=== TOGGLE SIDEBAR FUNCTION CALLED ===');
    
    try {
        const sidebar = document.querySelector('.sidebar');
        const page = document.querySelector('.page');
        
        console.log('Sidebar element:', sidebar);
        console.log('Page element:', page);
        
        if (!sidebar) {
            console.error('❌ Sidebar element not found!');
            return false;
        }
        
        if (!page) {
            console.error('❌ Page element not found!');
            return false;
        }
        
        // Detectar si estamos en móvil/tablet
        const isMobile = window.innerWidth < 1024;
        console.log('Is mobile/tablet:', isMobile, 'Window width:', window.innerWidth);
        
        // Log estado actual
        console.log('Sidebar classes before:', sidebar.className);
        console.log('Page classes before:', page.className);
        
        if (isMobile) {
            // En móvil, usar la clase 'open' en lugar de 'collapsed'
            sidebar.classList.toggle('open');
            console.log('Mobile: Toggling "open" class');
        } else {
            // En desktop, usar las clases normales
            sidebar.classList.toggle('collapsed');
            page.classList.toggle('sidebar-collapsed');
            console.log('Desktop: Toggling "collapsed" classes');
        }
        
        // Verificar estado después del toggle
        const isCollapsed = sidebar.classList.contains('collapsed');
        const isOpen = sidebar.classList.contains('open');
        const pageHasCollapsed = page.classList.contains('sidebar-collapsed');
        
        console.log('Sidebar classes after:', sidebar.className);
        console.log('Page classes after:', page.className);
        console.log('Is collapsed:', isCollapsed);
        console.log('Is open:', isOpen);
        console.log('Page has collapsed class:', pageHasCollapsed);
        
        // Guardar estado en localStorage (diferente para móvil y desktop)
        if (isMobile) {
            localStorage.setItem('sidebarOpen', isOpen);
            console.log('Mobile state saved to localStorage:', isOpen);
        } else {
            localStorage.setItem('sidebarCollapsed', isCollapsed);
            console.log('Desktop state saved to localStorage:', isCollapsed);
        }
        
        // Forzar re-render del layout
        page.style.display = 'none';
        page.offsetHeight; // Trigger reflow
        page.style.display = '';
        
        // Forzar recálculo del grid
        const mainContent = document.querySelector('.main-content');
        const content = document.querySelector('.content');
        
        if (mainContent) {
            mainContent.style.width = 'auto';
            mainContent.offsetHeight; // Trigger reflow
            mainContent.style.width = '';
        }
        
        if (content) {
            content.style.width = 'auto';
            content.offsetHeight; // Trigger reflow
            content.style.width = '';
            
            // Forzar reposicionamiento del contenido
            content.style.position = 'relative';
            content.style.left = '0';
            content.style.marginLeft = '0';
            
            // Solo expandir contenedores principales, no interferir con elementos internos
            const topLevelDivs = content.querySelectorAll('> div');
            topLevelDivs.forEach(element => {
                element.style.maxWidth = 'none';
                element.style.width = '100%';
            });
        }
        
        console.log('✅ Sidebar toggle completed successfully');
        return true;
        
    } catch (error) {
        console.error('❌ Error in toggleSidebar:', error);
        return false;
    }
};

// Cargar estado del sidebar al cargar la página
window.loadSidebarState = function() {
    console.log('=== LOADING SIDEBAR STATE ===');
    
    try {
        const sidebar = document.querySelector('.sidebar');
        const page = document.querySelector('.page');
        
        console.log('Sidebar element:', sidebar);
        console.log('Page element:', page);
        
        if (!sidebar || !page) {
            console.log('ℹ️ Sidebar or page elements not found');
            return;
        }
        
        // Detectar si estamos en móvil/tablet
        const isMobile = window.innerWidth < 1024;
        console.log('Loading state - Is mobile/tablet:', isMobile, 'Window width:', window.innerWidth);
        
        if (isMobile) {
            // En móvil, cargar estado de 'open'
            const isOpen = localStorage.getItem('sidebarOpen') === 'true';
            console.log('Stored mobile open state:', isOpen);
            
            if (isOpen) {
                sidebar.classList.add('open');
                console.log('✅ Mobile sidebar state loaded and applied');
            } else {
                sidebar.classList.remove('open');
                console.log('ℹ️ Mobile sidebar closed by default');
            }
        } else {
            // En desktop, cargar estado de 'collapsed'
            const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
            console.log('Stored desktop collapsed state:', isCollapsed);
            
            if (isCollapsed) {
                sidebar.classList.add('collapsed');
                page.classList.add('sidebar-collapsed');
                console.log('✅ Desktop sidebar state loaded and applied');
            } else {
                console.log('ℹ️ Desktop sidebar expanded by default');
            }
        }
        
        console.log('Final sidebar classes:', sidebar.className);
        console.log('Final page classes:', page.className);
        
    } catch (error) {
        console.error('❌ Error loading sidebar state:', error);
    }
};

// Ejecutar al cargar la página
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM Content Loaded - Initializing sidebar state');
    window.loadSidebarState();
});

// También ejecutar después de un pequeño delay para asegurar que Blazor haya renderizado
setTimeout(function() {
    console.log('Delayed sidebar state initialization');
    window.loadSidebarState();
}, 1000);

// Listener para cambios de tamaño de ventana (móvil ↔ desktop)
window.addEventListener('resize', function() {
    console.log('Window resized - reloading sidebar state');
    // Pequeño delay para asegurar que el resize se complete
    setTimeout(function() {
        window.loadSidebarState();
    }, 100);
});

// Listener para cerrar sidebar móvil al hacer clic fuera o en enlaces
document.addEventListener('click', function(event) {
    const isMobile = window.innerWidth < 1024;
    if (!isMobile) return; // Solo en móvil/tablet
    
    const sidebar = document.querySelector('.sidebar');
    const sidebarToggle = document.querySelector('.sidebar-toggle');
    
    if (!sidebar || !sidebarToggle) return;
    
    // Si el sidebar está abierto (tiene clase 'open')
    if (sidebar.classList.contains('open')) {
        // Verificar si el clic fue en un enlace de navegación
        const navLink = event.target.closest('.nav-link, a[href], button');
        const isNavLink = navLink && sidebar.contains(navLink);
        
        // Verificar si el clic fue fuera del sidebar y del botón toggle
        const isClickInsideSidebar = sidebar.contains(event.target);
        const isClickOnToggle = sidebarToggle.contains(event.target);
        
        console.log('Mobile click detected:', {
            isClickInsideSidebar,
            isClickOnToggle,
            isNavLink,
            target: event.target
        });
        
        // Cerrar sidebar si:
        // 1. Se hizo clic fuera del sidebar (y no fue en el toggle)
        // 2. Se hizo clic en un enlace de navegación dentro del sidebar
        if ((!isClickInsideSidebar && !isClickOnToggle) || isNavLink) {
            console.log('Closing mobile sidebar - click outside or nav link');
            sidebar.classList.remove('open');
            localStorage.setItem('sidebarOpen', false);
        }
    }
});