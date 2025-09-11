// Gráficos para la página de reservaciones
window.inicializarGraficos = (estadisticas) => {
    // Colores del tema
    const colores = {
        primary: '#3b82f6',
        success: '#10b981',
        warning: '#f59e0b',
        danger: '#ef4444',
        info: '#06b6d4',
        secondary: '#6b7280'
    };

    // Gráfico de distribución por estado
    const estadoCtx = document.getElementById('estadoChart');
    if (estadoCtx) {
        new Chart(estadoCtx, {
            type: 'doughnut',
            data: {
                labels: ['Pendientes', 'Confirmadas', 'En Proceso', 'Completadas', 'Canceladas', 'No-Show'],
                datasets: [{
                    data: [
                        estadisticas.reservacionesPendientes || 0,
                        estadisticas.reservacionesConfirmadas || 0,
                        estadisticas.reservacionesEnProceso || 0,
                        estadisticas.reservacionesCompletadas || 0,
                        estadisticas.reservacionesCanceladas || 0,
                        estadisticas.reservacionesNoShow || 0
                    ],
                    backgroundColor: [
                        colores.warning,
                        colores.success,
                        colores.info,
                        colores.secondary,
                        colores.danger,
                        '#f97316'
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
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? ((context.parsed / total) * 100).toFixed(1) : 0;
                                return `${context.label}: ${context.parsed} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    // Gráfico de reservaciones por hora
    const horaCtx = document.getElementById('horaChart');
    if (horaCtx) {
        // Datos de ejemplo para las horas del día
        const horas = Array.from({length: 24}, (_, i) => `${i.toString().padStart(2, '0')}:00`);
        const datosHora = horas.map(hora => {
            // Simular datos basados en patrones típicos de restaurantes
            const horaNum = parseInt(hora.split(':')[0]);
            if (horaNum >= 12 && horaNum <= 14) return Math.floor(Math.random() * 20) + 10; // Almuerzo
            if (horaNum >= 19 && horaNum <= 22) return Math.floor(Math.random() * 25) + 15; // Cena
            return Math.floor(Math.random() * 5); // Otras horas
        });

        new Chart(horaCtx, {
            type: 'bar',
            data: {
                labels: horas,
                datasets: [{
                    label: 'Reservaciones',
                    data: datosHora,
                    backgroundColor: colores.primary,
                    borderColor: colores.primary,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 5
                        }
                    },
                    x: {
                        ticks: {
                            maxTicksLimit: 12
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            title: function(context) {
                                return `Hora: ${context[0].label}`;
                            },
                            label: function(context) {
                                return `Reservaciones: ${context.parsed.y}`;
                            }
                        }
                    }
                }
            }
        });
    }

    // Gráfico de tendencia semanal
    const tendenciaCtx = document.getElementById('tendenciaChart');
    if (tendenciaCtx) {
        const diasSemana = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
        const datosSemana = diasSemana.map(dia => {
            // Simular datos de la semana actual
            return Math.floor(Math.random() * 30) + 10;
        });

        new Chart(tendenciaCtx, {
            type: 'line',
            data: {
                labels: diasSemana,
                datasets: [{
                    label: 'Reservaciones',
                    data: datosSemana,
                    borderColor: colores.primary,
                    backgroundColor: colores.primary + '20',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: colores.primary,
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 5
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            title: function(context) {
                                return `Día: ${context[0].label}`;
                            },
                            label: function(context) {
                                return `Reservaciones: ${context.parsed.y}`;
                            }
                        }
                    }
                }
            }
        });
    }
};
