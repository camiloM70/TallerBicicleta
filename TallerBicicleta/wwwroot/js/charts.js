// Módulo para manejar gráficas con Chart.js
window.graficas = {
    // Almacena las instancias de gráficas
    instancias: {},

    // Destruye una gráfica si existe
    destruirGrafica: function(chartId) {
        if (this.instancias[chartId] && this.instancias[chartId] instanceof Chart) {
            this.instancias[chartId].destroy();
            this.instancias[chartId] = null;
        }
    },

    // Genera gráfica de barras para distribución de ingresos
    generarIngresos: function(canvasId, datos) {
        try {
            // Destruir gráfica anterior si existe
            this.destruirGrafica(canvasId);

            const ctx = document.getElementById(canvasId);
            if (!ctx) {
                console.error(`No se encontró el elemento canvas con id: ${canvasId}`);
                return;
            }

            this.instancias[canvasId] = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Servicios', 'Productos', 'Comisiones', 'Ganancia Neta'],
                    datasets: [{
                        label: 'Monto ($)',
                        data: [
                            datos.gananciaServicios,
                            datos.gananciaProductos,
                            datos.comisionEmpleado,
                            datos.gananciaNeta
                        ],
                        backgroundColor: [
                            'rgba(102, 126, 234, 0.8)',
                            'rgba(40, 167, 69, 0.8)',
                            'rgba(255, 193, 7, 0.8)',
                            'rgba(0, 123, 255, 0.8)'
                        ],
                        borderColor: [
                            'rgb(102, 126, 234)',
                            'rgb(40, 167, 69)',
                            'rgb(255, 193, 7)',
                            'rgb(0, 123, 255)'
                        ],
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            display: false
                        },
                        title: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function(value) {
                                    return '$' + value.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                                }
                            }
                        }
                    }
                }
            });

            console.log(`Gráfica de ingresos creada exitosamente: ${canvasId}`);
        } catch (error) {
            console.error(`Error al generar gráfica de ingresos: ${error.message}`);
        }
    },

    // Genera gráfica de donut para composición
    generarComposicion: function(canvasId, datos) {
        try {
            // Destruir gráfica anterior si existe
            this.destruirGrafica(canvasId);

            const ctx = document.getElementById(canvasId);
            if (!ctx) {
                console.error(`No se encontró el elemento canvas con id: ${canvasId}`);
                return;
            }

            this.instancias[canvasId] = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Servicios', 'Productos'],
                    datasets: [{
                        data: [
                            datos.gananciaServicios,
                            datos.gananciaProductos
                        ],
                        backgroundColor: [
                            'rgba(102, 126, 234, 0.8)',
                            'rgba(40, 167, 69, 0.8)'
                        ],
                        borderColor: [
                            'rgb(102, 126, 234)',
                            'rgb(40, 167, 69)'
                        ],
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        },
                        title: {
                            display: false
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    const label = context.label || '';
                                    const value = context.parsed || 0;
                                    const formatted = '$' + value.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                                    return label + ': ' + formatted;
                                }
                            }
                        }
                    }
                }
            });

            console.log(`Gráfica de composición creada exitosamente: ${canvasId}`);
        } catch (error) {
            console.error(`Error al generar gráfica de composición: ${error.message}`);
        }
    },

    // Destruye todas las gráficas
    destruirTodas: function() {
        for (let chartId in this.instancias) {
            this.destruirGrafica(chartId);
        }
        console.log('Todas las gráficas fueron destruidas');
    }
};
