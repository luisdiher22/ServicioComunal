// Variables globales
let estudianteAEliminar = null;
let modoEdicion = false;

// Inicialización
document.addEventListener('DOMContentLoaded', function() {
    inicializarEventos();
    inicializarFiltros();
});

function inicializarEventos() {
    // Modal de estudiante
    const modalEstudiante = document.getElementById('modalEstudiante');
    modalEstudiante.addEventListener('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    // Validación en tiempo real
    const form = document.getElementById('formEstudiante');
    form.addEventListener('input', function(e) {
        validarCampo(e.target);
    });
}

function inicializarFiltros() {
    // Búsqueda en tiempo real
    document.getElementById('searchInput').addEventListener('input', function() {
        filtrarTabla();
    });

    // Filtros por select
    document.getElementById('filterClase').addEventListener('change', function() {
        filtrarTabla();
    });

    document.getElementById('filterGrupo').addEventListener('change', function() {
        filtrarTabla();
    });
}

function filtrarTabla() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const claseFilter = document.getElementById('filterClase').value;
    const grupoFilter = document.getElementById('filterGrupo').value;
    
    const tbody = document.querySelector('#tablaEstudiantes tbody');
    const rows = tbody.querySelectorAll('tr');
    
    let visibleRows = 0;
    
    rows.forEach(row => {
        const cedula = row.cells[0].textContent.toLowerCase();
        const nombre = row.cells[1].textContent.toLowerCase();
        const clase = row.cells[2].textContent.trim();
        const estadoGrupo = row.cells[3].textContent.includes('Con Grupo') ? 'con-grupo' : 'sin-grupo';
        
        let mostrar = true;
        
        // Filtro de búsqueda
        if (searchTerm && !cedula.includes(searchTerm) && !nombre.includes(searchTerm)) {
            mostrar = false;
        }
        
        // Filtro de clase
        if (claseFilter && clase !== claseFilter) {
            mostrar = false;
        }
        
        // Filtro de grupo
        if (grupoFilter && estadoGrupo !== grupoFilter) {
            mostrar = false;
        }
        
        row.style.display = mostrar ? '' : 'none';
        if (mostrar) visibleRows++;
    });
    
    // Mostrar mensaje si no hay resultados
    mostrarMensajeNoResultados(visibleRows === 0);
}

function mostrarMensajeNoResultados(mostrar) {
    let mensajeRow = document.getElementById('noResultsRow');
    
    if (mostrar && !mensajeRow) {
        const tbody = document.querySelector('#tablaEstudiantes tbody');
        mensajeRow = document.createElement('tr');
        mensajeRow.id = 'noResultsRow';
        mensajeRow.innerHTML = `
            <td colspan="5" class="text-center py-4">
                <div class="no-results">
                    <i class="fas fa-search fa-2x text-muted mb-2"></i>
                    <p class="text-muted">No se encontraron estudiantes que coincidan con los filtros aplicados.</p>
                </div>
            </td>
        `;
        tbody.appendChild(mensajeRow);
    } else if (!mostrar && mensajeRow) {
        mensajeRow.remove();
    }
}

function limpiarFiltros() {
    document.getElementById('searchInput').value = '';
    document.getElementById('filterClase').value = '';
    document.getElementById('filterGrupo').value = '';
    filtrarTabla();
}

function verEstudiante(identificacion) {
    // Obtener datos del estudiante
    fetch(`/Home/ObtenerEstudiante/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                mostrarDetallesEstudiante(data.estudiante);
            } else {
                mostrarError('Error al cargar los datos del estudiante');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
}

function mostrarDetallesEstudiante(estudiante) {
    // TODO: Implementar modal de detalles o navegación a página de detalles
    alert(`Detalles del estudiante:\nCédula: ${estudiante.identificacion}\nNombre: ${estudiante.nombre} ${estudiante.apellidos}\nClase: ${estudiante.clase}`);
}

function editarEstudiante(identificacion) {
    modoEdicion = true;
    
    // Obtener datos del estudiante
    fetch(`/Home/ObtenerEstudiante/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                cargarDatosEnFormulario(data.estudiante);
                document.getElementById('modalEstudianteLabel').textContent = 'Editar Estudiante';
                const modal = new bootstrap.Modal(document.getElementById('modalEstudiante'));
                modal.show();
            } else {
                mostrarError('Error al cargar los datos del estudiante');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
}

function cargarDatosEnFormulario(estudiante) {
    document.getElementById('estudianteId').value = estudiante.identificacion;
    document.getElementById('identificacion').value = estudiante.identificacion;
    document.getElementById('nombre').value = estudiante.nombre;
    document.getElementById('apellidos').value = estudiante.apellidos;
    document.getElementById('clase').value = estudiante.clase;
    
    // En modo edición, deshabilitar el campo de cédula
    document.getElementById('identificacion').disabled = true;
}

function eliminarEstudiante(identificacion) {
    estudianteAEliminar = identificacion;
    
    // Obtener datos del estudiante para mostrar en el modal
    fetch(`/Home/ObtenerEstudiante/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('estudianteInfo').innerHTML = `
                    <strong>Cédula:</strong> ${data.estudiante.identificacion}<br>
                    <strong>Nombre:</strong> ${data.estudiante.nombre} ${data.estudiante.apellidos}<br>
                    <strong>Clase:</strong> ${data.estudiante.clase}
                `;
                
                const modal = new bootstrap.Modal(document.getElementById('modalEliminar'));
                modal.show();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al cargar los datos del estudiante');
        });
}

// Confirmar eliminación
document.getElementById('btnConfirmarEliminar').addEventListener('click', function() {
    if (estudianteAEliminar) {
        fetch(`/Home/EliminarEstudiante/${estudianteAEliminar}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Remover la fila de la tabla
                const row = document.querySelector(`tr[data-estudiante-id="${estudianteAEliminar}"]`);
                if (row) {
                    row.remove();
                }
                
                // Actualizar estadísticas
                actualizarEstadisticas();
                
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEliminar'));
                modal.hide();
                
                mostrarExito('Estudiante eliminado exitosamente');
            } else {
                mostrarError(data.message || 'Error al eliminar el estudiante');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        })
        .finally(() => {
            estudianteAEliminar = null;
        });
    }
});

function guardarEstudiante() {
    const form = document.getElementById('formEstudiante');
    
    if (!validarFormulario(form)) {
        return;
    }
    
    const formData = new FormData(form);
    const estudiante = {
        identificacion: parseInt(formData.get('identificacion')),
        nombre: formData.get('nombre').trim(),
        apellidos: formData.get('apellidos').trim(),
        clase: formData.get('clase')
    };
    
    // Si estamos en modo edición, incluir la identificación original
    if (modoEdicion) {
        estudiante.identificacionOriginal = parseInt(formData.get('identificacionOriginal'));
    }
    
    const url = modoEdicion ? '/Home/ActualizarEstudiante' : '/Home/CrearEstudiante';
    const method = modoEdicion ? 'PUT' : 'POST';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(estudiante)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalEstudiante'));
            modal.hide();
            
            mostrarExito(modoEdicion ? 'Estudiante actualizado exitosamente' : 'Estudiante creado exitosamente');
            
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            mostrarError(data.message || 'Error al guardar el estudiante');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor');
    });
}

function validarFormulario(form) {
    let valido = true;
    const campos = form.querySelectorAll('input[required], select[required]');
    
    campos.forEach(campo => {
        if (!validarCampo(campo)) {
            valido = false;
        }
    });
    
    return valido;
}

function validarCampo(campo) {
    let valido = true;
    let mensaje = '';
    
    // Validación básica de campo requerido
    if (campo.hasAttribute('required') && !campo.value.trim()) {
        valido = false;
        mensaje = 'Este campo es requerido';
    }
    
    // Validaciones específicas
    switch (campo.id) {
        case 'identificacion':
            if (campo.value && (isNaN(campo.value) || campo.value <= 0)) {
                valido = false;
                mensaje = 'Debe ser un número válido';
            }
            break;
        case 'nombre':
        case 'apellidos':
            if (campo.value && campo.value.length < 2) {
                valido = false;
                mensaje = 'Debe tener al menos 2 caracteres';
            }
            break;
    }
    
    // Aplicar estilos de validación
    if (valido) {
        campo.classList.remove('is-invalid');
        campo.classList.add('is-valid');
    } else {
        campo.classList.remove('is-valid');
        campo.classList.add('is-invalid');
        
        // Actualizar mensaje de error
        const feedback = campo.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = mensaje;
        }
    }
    
    return valido;
}

function limpiarFormulario() {
    const form = document.getElementById('formEstudiante');
    form.reset();
    
    // Limpiar clases de validación
    const campos = form.querySelectorAll('.form-control, .form-select');
    campos.forEach(campo => {
        campo.classList.remove('is-valid', 'is-invalid');
        campo.disabled = false;
    });
    
    // Restablecer título del modal
    document.getElementById('modalEstudianteLabel').textContent = 'Agregar Estudiante';
    modoEdicion = false;
}

function actualizarEstadisticas() {
    // Contar filas visibles
    const rows = document.querySelectorAll('#tablaEstudiantes tbody tr:not(#noResultsRow)');
    let total = 0;
    let conGrupo = 0;
    let sinGrupo = 0;
    
    rows.forEach(row => {
        if (row.style.display !== 'none') {
            total++;
            if (row.cells[3].textContent.includes('Con Grupo')) {
                conGrupo++;
            } else {
                sinGrupo++;
            }
        }
    });
    
    document.getElementById('totalEstudiantes').textContent = total;
    document.getElementById('conGrupo').textContent = conGrupo;
    document.getElementById('sinGrupo').textContent = sinGrupo;
}

function mostrarExito(mensaje) {
    mostrarNotificacion(mensaje, 'success');
}

function mostrarError(mensaje) {
    mostrarNotificacion(mensaje, 'error');
}

function mostrarNotificacion(mensaje, tipo) {
    // Crear elemento de notificación
    const notificacion = document.createElement('div');
    notificacion.className = `alert alert-${tipo === 'success' ? 'success' : 'danger'} alert-dismissible fade show notification-toast`;
    notificacion.innerHTML = `
        <i class="fas fa-${tipo === 'success' ? 'check-circle' : 'exclamation-triangle'}"></i>
        ${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Agregar al DOM
    document.body.appendChild(notificacion);
    
    // Auto-eliminar después de 5 segundos
    setTimeout(() => {
        if (notificacion.parentNode) {
            notificacion.remove();
        }
    }, 5000);
}

function exportarEstudiantes() {
    // Mostrar notificación de procesamiento
    const notificacionProceso = document.createElement('div');
    notificacionProceso.className = 'alert alert-info alert-dismissible fade show notification-toast';
    notificacionProceso.id = 'exportNotification';
    notificacionProceso.innerHTML = `
        <i class="fas fa-spinner fa-spin"></i>
        Generando archivo Excel... Por favor espere.
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(notificacionProceso);
    
    // Crear un enlace temporal para la descarga
    const link = document.createElement('a');
    link.href = '/Home/ExportarEstudiantesExcel';
    link.download = '';
    link.style.display = 'none';
    document.body.appendChild(link);
    
    // Simular click en el enlace
    link.click();
    
    // Limpiar el enlace temporal
    document.body.removeChild(link);
    
    // Remover notificación de procesamiento después de 3 segundos
    setTimeout(() => {
        const notification = document.getElementById('exportNotification');
        if (notification && notification.parentNode) {
            notification.remove();
            // Mostrar notificación de éxito
            mostrarNotificacion('Archivo Excel generado exitosamente. La descarga debería iniciar automáticamente.', 'success');
        }
    }, 3000);
}
