// Funciones JavaScript para optimización de imágenes
window.imageUpload = {
    // Función para crear vista previa optimizada de imagen
    createOptimizedPreview: function (file) {
        return new Promise((resolve) => {
            try {
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                const img = new Image();
                
                img.onload = function() {
                    try {
                        // Calcular nuevas dimensiones manteniendo proporción
                        let newWidth = img.width;
                        let newHeight = img.height;
                        
                        const maxWidth = 800;
                        const maxHeight = 600;
                        const quality = 0.8;
                        
                        // Redimensionar solo si es necesario
                        if (newWidth > maxWidth) {
                            newHeight = (newHeight * maxWidth) / newWidth;
                            newWidth = maxWidth;
                        }
                        
                        if (newHeight > maxHeight) {
                            newWidth = (newWidth * maxHeight) / newHeight;
                            newHeight = maxHeight;
                        }
                        
                        // Configurar canvas
                        canvas.width = newWidth;
                        canvas.height = newHeight;
                        
                        // Aplicar suavizado para mejor calidad
                        ctx.imageSmoothingEnabled = true;
                        ctx.imageSmoothingQuality = 'high';
                        
                        // Dibujar imagen redimensionada
                        ctx.drawImage(img, 0, 0, newWidth, newHeight);
                        
                        // Convertir a base64 con calidad optimizada
                        const dataUrl = canvas.toDataURL('image/jpeg', quality);
                        
                        resolve(dataUrl);
                    } catch (error) {
                        console.error('Error al procesar la imagen:', error);
                        resolve(null);
                    }
                };
                
                img.onerror = function() {
                    console.error('Error al cargar la imagen');
                    resolve(null);
                };
                
                // Crear URL del archivo
                const fileUrl = URL.createObjectURL(file);
                img.src = fileUrl;
                
                // Limpiar URL después de un tiempo
                setTimeout(() => {
                    URL.revokeObjectURL(fileUrl);
                }, 1000);
                
            } catch (error) {
                console.error('Error en createOptimizedPreview:', error);
                resolve(null);
            }
        });
    },

    // Función alternativa más simple
    createSimplePreview: function (file) {
        return new Promise((resolve) => {
            try {
                const reader = new FileReader();
                reader.onload = function(e) {
                    resolve(e.target.result);
                };
                reader.onerror = function() {
                    resolve(null);
                };
                reader.readAsDataURL(file);
            } catch (error) {
                console.error('Error en createSimplePreview:', error);
                resolve(null);
            }
        });
    }
};

// Función global para compatibilidad
window.createOptimizedPreview = function (file) {
    return window.imageUpload.createOptimizedPreview(file);
};
