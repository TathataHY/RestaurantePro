// Gráficos para la página de Preparaciones
window.preparacionesCharts = {
    // Gráfico de distribución por estado
    initDistribucionEstado: function(data) {
        const ctx = document.getElementById('distribucionEstadoChart');
        if (!ctx) return;

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Pendientes', 'En Proceso', 'Listas', 'Entregadas'],
                datasets: [{
                    data: data || [12, 8, 15, 25],
                    backgroundColor: [
                        '#F59E0B', // Amarillo para pendientes
                        '#3B82F6', // Azul para en proceso
                        '#10B981', // Verde para listas
                        '#6B7280'  // Gris para entregadas
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
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
                            usePointStyle: true
                        }
                    }
                }
            }
        });
    },

    // Gráfico de tiempo de preparación por hora
    initTiempoPorHora: function(data) {
        const ctx = document.getElementById('tiempoPorHoraChart');
        if (!ctx) return;

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: data?.labels || ['8:00', '9:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00', '19:00', '20:00', '21:00'],
                datasets: [{
                    label: 'Tiempo Promedio (min)',
                    data: data?.values || [15, 18, 22, 25, 28, 30, 25, 20, 18, 22, 26, 28, 24, 20],
                    borderColor: '#3B82F6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
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
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    },

    // Gráfico de rendimiento por cocinero
    initRendimientoCocinero: function(data) {
        const ctx = document.getElementById('rendimientoCocineroChart');
        if (!ctx) return;

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data?.labels || ['Juan', 'María', 'Carlos', 'Ana', 'Luis'],
                datasets: [{
                    label: 'Preparaciones Completadas',
                    data: data?.values || [25, 30, 22, 28, 20],
                    backgroundColor: [
                        '#10B981',
                        '#3B82F6',
                        '#F59E0B',
                        '#EF4444',
                        '#8B5CF6'
                    ],
                    borderWidth: 0,
                    borderRadius: 6
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
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    },

    // Inicializar todos los gráficos
    initAll: function(estadisticas) {
        // Datos de ejemplo - en producción vendrían del servidor
        const distribucionData = estadisticas ? [
            estadisticas.preparacionesPendientes || 0,
            estadisticas.preparacionesEnProceso || 0,
            estadisticas.preparacionesCompletadas || 0,
            estadisticas.preparacionesEntregadas || 0
        ] : [12, 8, 15, 25];

        const tiempoPorHoraData = {
            labels: ['8:00', '9:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00', '19:00', '20:00', '21:00'],
            values: [15, 18, 22, 25, 28, 30, 25, 20, 18, 22, 26, 28, 24, 20]
        };

        const rendimientoData = {
            labels: ['Juan', 'María', 'Carlos', 'Ana', 'Luis'],
            values: [25, 30, 22, 28, 20]
        };

        this.initDistribucionEstado(distribucionData);
        this.initTiempoPorHora(tiempoPorHoraData);
        this.initRendimientoCocinero(rendimientoData);
    }
};

// Función para descargar archivos
window.downloadFile = function(filename, base64Data) {
    const link = document.createElement('a');
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64Data;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
