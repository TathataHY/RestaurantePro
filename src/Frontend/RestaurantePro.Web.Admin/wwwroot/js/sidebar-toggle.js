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
        
        // Log estado actual
        console.log('Sidebar classes before:', sidebar.className);
        console.log('Page classes before:', page.className);
        
        // Toggle classes
        sidebar.classList.toggle('collapsed');
        page.classList.toggle('sidebar-collapsed');
        
        // Verificar estado después del toggle
        const isCollapsed = sidebar.classList.contains('collapsed');
        const pageHasCollapsed = page.classList.contains('sidebar-collapsed');
        
        console.log('Sidebar classes after:', sidebar.className);
        console.log('Page classes after:', page.className);
        console.log('Is collapsed:', isCollapsed);
        console.log('Page has collapsed class:', pageHasCollapsed);
        
        // Guardar estado en localStorage
        localStorage.setItem('sidebarCollapsed', isCollapsed);
        console.log('State saved to localStorage:', isCollapsed);
        
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
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        const sidebar = document.querySelector('.sidebar');
        const page = document.querySelector('.page');
        
        console.log('Stored collapsed state:', isCollapsed);
        console.log('Sidebar element:', sidebar);
        console.log('Page element:', page);
        
        if (sidebar && page && isCollapsed) {
            sidebar.classList.add('collapsed');
            page.classList.add('sidebar-collapsed');
            console.log('✅ Sidebar state loaded and applied');
            console.log('Sidebar classes:', sidebar.className);
            console.log('Page classes:', page.className);
        } else {
            console.log('ℹ️ No collapsed state to load or elements not found');
        }
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