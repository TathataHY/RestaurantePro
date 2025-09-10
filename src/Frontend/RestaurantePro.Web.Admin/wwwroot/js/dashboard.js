// Funciones JavaScript para el Dashboard
window.dashboardCharts = {
    // Crear gráfico de ventas por día
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

    // Crear gráfico de estado de mesas
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

    // Crear gráfico de comandas por estado
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

    // Crear gráfico de ingresos por hora
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
        if (chart) {
            chart.destroy();
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
