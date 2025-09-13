// Funciones JavaScript para optimización de imágenes
console.log('🔍 Cargando image-upload.js...');

// Función simple y robusta para crear vista previa
window.createOptimizedPreview = function (file) {
    console.log('🔍 createOptimizedPreview llamado con archivo:', file.name, file.size);
    
    return new Promise((resolve) => {
        try {
            const reader = new FileReader();
            reader.onload = function(e) {
                console.log('✅ Vista previa creada exitosamente');
                resolve(e.target.result);
            };
            reader.onerror = function() {
                console.error('❌ Error al leer archivo');
                resolve(null);
            };
            reader.readAsDataURL(file);
        } catch (error) {
            console.error('❌ Error en createOptimizedPreview:', error);
            resolve(null);
        }
    });
};

// Objeto para compatibilidad
window.imageUpload = {
    createOptimizedPreview: window.createOptimizedPreview
};

console.log('✅ image-upload.js cargado correctamente');
