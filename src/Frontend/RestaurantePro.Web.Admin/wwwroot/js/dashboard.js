// Funciones JavaScript para el Dashboard Ejecutivo
window.dashboardCharts = {
    // Crear gráfico de ventas por hora
    crearGraficoVentasHora: function (canvasId, datos) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no está disponible');
            return null;
        }
        
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas no encontrado:', canvasId);
            return null;
        }
        const ctx = canvas.getContext('2d');
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: datos.map(d => d.hora + ':00'),
                datasets: [{
                    label: 'Ventas ($)',
                    data: datos.map(d => d.monto),
                    borderColor: '#1e71f6',
                    backgroundColor: 'rgba(30, 113, 246, 0.1)',
                    tension: 0.4,
                    fill: true,
                    pointBackgroundColor: '#1e71f6',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#6b7280',
                            font: {
                                size: 12
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: '#f3f4f6'
                        },
                        ticks: {
                            color: '#6b7280',
                            font: {
                                size: 12
                            },
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                elements: {
                    point: {
                        hoverBackgroundColor: '#1e71f6'
                    }
                }
            }
        });
    },

    // Crear gráfico de ingresos por categoría (gráfico de dona)
    crearGraficoIngresosCategoria: function (canvasId, datos) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no está disponible');
            return null;
        }
        
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas no encontrado:', canvasId);
            return null;
        }
        const ctx = canvas.getContext('2d');
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: datos.map(d => d.categoria),
                datasets: [{
                    data: datos.map(d => d.porcentaje),
                    backgroundColor: datos.map(d => d.color),
                    borderWidth: 0,
                    cutout: '60%'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed + '%';
                            }
                        }
                    }
                },
                elements: {
                    arc: {
                        borderWidth: 0
                    }
                }
            }
        });
    },

    // Crear gráfico de productos más vendidos (barras horizontales)
    crearGraficoProductosVendidos: function (canvasId, datos) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no está disponible');
            return null;
        }
        
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas no encontrado:', canvasId);
            return null;
        }
        const ctx = canvas.getContext('2d');
        return new Chart(ctx, {
            type: 'bar',
            data: {
                labels: datos.map(d => d.nombre),
                datasets: [{
                    label: 'Cantidad Vendida',
                    data: datos.map(d => d.cantidadVendida),
                    backgroundColor: '#10b981',
                    borderColor: '#059669',
                    borderWidth: 1,
                    borderRadius: 4,
                    borderSkipped: false
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const data = datos[context.dataIndex];
                                return `${data.nombre}: ${data.cantidadVendida} unidades - $${data.ingresos.toLocaleString()}`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: {
                            color: '#f3f4f6'
                        },
                        ticks: {
                            color: '#6b7280',
                            font: {
                                size: 12
                            },
                            stepSize: 1
                        }
                    },
                    y: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#6b7280',
                            font: {
                                size: 12
                            }
                        }
                    }
                },
                elements: {
                    bar: {
                        borderWidth: 1
                    }
                }
            }
        });
    },

    // Crear gráfico de estados de mesas (dona)
    crearGraficoEstadosMesas: function (canvasId, datos) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js no está disponible');
            return null;
        }
        
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas no encontrado:', canvasId);
            return null;
        }
        
        const ctx = canvas.getContext('2d');
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: datos.map(d => d.estado),
                datasets: [{
                    data: datos.map(d => d.cantidad),
                    backgroundColor: [
                        '#ef4444', // Rojo para Ocupadas
                        '#10b981', // Verde para Libres
                        '#f59e0b', // Amarillo para Reservadas
                        '#6b7280'  // Gris para Mantenimiento
                    ],
                    borderColor: [
                        '#dc2626',
                        '#059669',
                        '#d97706',
                        '#4b5563'
                    ],
                    borderWidth: 2,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: {
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const data = datos[context.dataIndex];
                                const total = datos.reduce((sum, d) => sum + d.cantidad, 0);
                                const porcentaje = ((data.cantidad / total) * 100).toFixed(1);
                                return `${data.estado}: ${data.cantidad} mesas (${porcentaje}%)`;
                            }
                        }
                    }
                },
                cutout: '60%',
                elements: {
                    arc: {
                        borderWidth: 2
                    }
                }
            }
        });
    },

    // Crear gráfico de ventas por día (mantener para compatibilidad)
    crearGraficoVentas: function (canvasId, datos) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        return new Chart(ctx, {
            type: 'line',
            data: {
                labels: datos.map(d => d.fecha),
                datasets: [{
                    label: 'Ventas ($)',
                    data: datos.map(d => d.monto),
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    tension: 0.1,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    },

    // Crear gráfico de estado de mesas (mantener para compatibilidad)
    crearGraficoMesas: function (canvasId, datos) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Disponibles', 'Ocupadas', 'Reservadas', 'En Limpieza'],
                datasets: [{
                    data: [datos.disponibles, datos.ocupadas, datos.reservadas, datos.enLimpieza],
                    backgroundColor: [
                        '#28a745',
                        '#dc3545',
                        '#ffc107',
                        '#6c757d'
                    ],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    },

    // Crear gráfico de comandas por estado (mantener para compatibilidad)
    crearGraficoComandas: function (canvasId, datos) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        return new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Pendientes', 'En Preparación', 'Listas', 'Completadas', 'Canceladas'],
                datasets: [{
                    label: 'Cantidad',
                    data: [datos.pendientes, datos.enPreparacion, datos.listas, datos.completadas, datos.canceladas],
                    backgroundColor: [
                        '#ffc107',
                        '#17a2b8',
                        '#28a745',
                        '#6c757d',
                        '#dc3545'
                    ],
                    borderWidth: 1,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    },

    // Crear gráfico de ingresos por hora (mantener para compatibilidad)
    crearGraficoIngresosHora: function (canvasId, datos) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        return new Chart(ctx, {
            type: 'bar',
            data: {
                labels: datos.map(d => d.hora + ':00'),
                datasets: [{
                    label: 'Ingresos ($)',
                    data: datos.map(d => d.monto),
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    },

    // Destruir gráfico existente
    destruirGrafico: function (chart) {
        if (chart && typeof chart.destroy === 'function') {
            try {
                chart.destroy();
            } catch (error) {
                console.warn('Error al destruir gráfico:', error);
            }
        }
    }
};

// Función para formatear números como moneda
window.formatearMoneda = function (valor) {
    return new Intl.NumberFormat('es-CL', {
        style: 'currency',
        currency: 'CLP'
    }).format(valor);
};

// Función para formatear porcentajes
window.formatearPorcentaje = function (valor) {
    return valor.toFixed(1) + '%';
};

// Función para crear vista previa optimizada de imagen
window.createOptimizedPreview = function (file) {
    return new Promise((resolve) => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const img = new Image();
        
        img.onload = function() {
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
        };
        
        img.onerror = function() {
            console.error('Error al cargar la imagen');
            resolve(null);
        };
        
        // Crear URL del archivo
        const fileUrl = URL.createObjectURL(file);
        img.src = fileUrl;
    });
};