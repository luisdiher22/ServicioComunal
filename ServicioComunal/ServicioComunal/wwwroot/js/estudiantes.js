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

    modalEstudiante.addEventListener('shown.bs.modal', function () {
        // Solo limpiar validación si no estamos en modo edición
        if (!modoEdicion) {
            limpiarValidacion();
        }
    });

    // Validación solo en blur después del primer intento
    const form = document.getElementById('formEstudiante');
    let validacionActivada = false;
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        validacionActivada = true;
        guardarEstudiante();
    });

    // Solo validar en blur después del primer intento de guardar
    form.addEventListener('blur', function(e) {
        if (validacionActivada && e.target.matches('input, select')) {
            validarCampo(e.target);
        }
    }, true);
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
    // Llenar los campos del modal con los datos del estudiante
    document.getElementById('detalleCedula').textContent = estudiante.identificacion;
    document.getElementById('detalleNombre').textContent = estudiante.nombre;
    document.getElementById('detalleApellidos').textContent = estudiante.apellidos;
    document.getElementById('detalleClase').textContent = estudiante.clase;
    
    // Estado del grupo
    const detalleGrupo = document.getElementById('detallesGrupo');
    const estadoGrupo = document.getElementById('detalleEstadoGrupo');
    const infoGrupo = document.getElementById('infoGrupo');
    
    if (estudiante.gruposEstudiantes && estudiante.gruposEstudiantes.length > 0) {
        estadoGrupo.innerHTML = '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Con Grupo</span>';
        
        // Mostrar información del grupo
        const grupo = estudiante.gruposEstudiantes[0].grupo;
        infoGrupo.innerHTML = `
            <div class="grupo-info-item"><strong>Nombre del Grupo:</strong> ${grupo.nombre}</div>
            <div class="grupo-info-item"><strong>Descripción:</strong> ${grupo.descripcion || 'Sin descripción'}</div>
            <div class="grupo-info-item"><strong>Miembros:</strong> ${grupo.gruposEstudiantes ? grupo.gruposEstudiantes.length : 'N/A'}</div>
            <div class="grupo-info-item"><strong>Estado:</strong> ${grupo.activo ? 'Activo' : 'Inactivo'}</div>
        `;
        detalleGrupo.style.display = 'block';
    } else {
        estadoGrupo.innerHTML = '<span class="badge bg-warning"><i class="fas fa-exclamation-triangle"></i> Sin Grupo</span>';
        detalleGrupo.style.display = 'none';
    }
    
    // Guardar ID del estudiante actual para la función de editar
    window.estudianteActual = estudiante.identificacion;
    
    // Mostrar el modal
    const modal = new bootstrap.Modal(document.getElementById('modalDetallesEstudiante'));
    modal.show();
}

function editarEstudianteDesdeDetalles() {
    // Cerrar el modal de detalles
    const modalDetalles = bootstrap.Modal.getInstance(document.getElementById('modalDetallesEstudiante'));
    if (modalDetalles) {
        modalDetalles.hide();
    }
    
    // Usar la identificación guardada globalmente
    if (window.estudianteActual) {
        editarEstudiante(window.estudianteActual);
    }
}

function editarEstudiante(identificacion) {
    modoEdicion = true;
    
    // Obtener datos del estudiante
    fetch(`/Home/ObtenerEstudiante/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                cargarDatosEnFormulario(data.estudiante, true);
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

function abrirModalAgregar() {
    // Resetear modo edición y limpiar formulario
    modoEdicion = false;
    cargarDatosEnFormulario(null, false);
    
    // Cambiar título del modal
    document.getElementById('modalEstudianteLabel').textContent = 'Agregar Estudiante';
    
    // Abrir modal
    const modal = new bootstrap.Modal(document.getElementById('modalEstudiante'));
    modal.show();
}

function cargarDatosEnFormulario(estudiante, esEdicion = false) {
    if (estudiante) {
        document.getElementById('estudianteId').value = estudiante.identificacion || '';
        document.getElementById('identificacion').value = estudiante.identificacion || '';
        document.getElementById('nombre').value = estudiante.nombre || '';
        document.getElementById('apellidos').value = estudiante.apellidos || '';
        document.getElementById('clase').value = estudiante.clase || '';
    } else {
        // Limpiar formulario si no hay datos
        document.getElementById('estudianteId').value = '';
        document.getElementById('identificacion').value = '';
        document.getElementById('nombre').value = '';
        document.getElementById('apellidos').value = '';
        document.getElementById('clase').value = '';
    }
    
    // En modo edición, deshabilitar el campo de cédula
    setTimeout(() => {
        const campoIdentificacion = document.getElementById('identificacion');
        if (campoIdentificacion) {
            if (esEdicion && estudiante) {
                campoIdentificacion.disabled = true;
                campoIdentificacion.setAttribute('readonly', 'readonly');
            } else {
                campoIdentificacion.disabled = false;
                campoIdentificacion.removeAttribute('readonly');
            }
        }
    }, 50);
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
    
    // Habilitar temporalmente el campo identificacion si está deshabilitado
    const campoIdentificacion = document.getElementById('identificacion');
    const estabaDeshabilite = campoIdentificacion.disabled;
    if (estabaDeshabilite) {
        campoIdentificacion.disabled = false;
    }
    
    const formData = new FormData(form);
    
    // Restaurar el estado del campo
    if (estabaDeshabilite) {
        campoIdentificacion.disabled = true;
    }
    
    const estudiante = {
        identificacion: parseInt(formData.get('identificacion')),
        nombre: formData.get('nombre').trim(),
        apellidos: formData.get('apellidos').trim(),
        clase: formData.get('clase')
    };
    
    // Si estamos en modo edición, incluir la identificación original
    if (modoEdicion) {
        const estudianteId = formData.get('estudianteId');
        estudiante.identificacionOriginal = parseInt(estudianteId);
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
    
    // Activar validación visual para todos los campos
    campos.forEach(campo => {
        if (!validarCampo(campo)) {
            valido = false;
        }
    });
    
    // Si hay errores, hacer scroll al primer campo con error
    if (!valido) {
        const primerError = form.querySelector('.is-invalid');
        if (primerError) {
            primerError.focus();
            primerError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
    
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
    
    // Aplicar estilos de validación solo si hay errores
    if (!valido) {
        campo.classList.remove('is-valid');
        campo.classList.add('is-invalid');
        
        // Actualizar mensaje de error
        const feedback = campo.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = mensaje;
        }
    } else {
        campo.classList.remove('is-invalid');
        campo.classList.add('is-valid');
    }
    
    return valido;
}

function limpiarFormulario() {
    const form = document.getElementById('formEstudiante');
    form.reset();
    
    // Limpiar clases de validación
    limpiarValidacion();
    
    // Restablecer título del modal
    document.getElementById('modalEstudianteLabel').textContent = 'Agregar Estudiante';
    modoEdicion = false;
}

function limpiarValidacion() {
    const form = document.getElementById('formEstudiante');
    const campos = form.querySelectorAll('.form-control, .form-select');
    campos.forEach(campo => {
        campo.classList.remove('is-valid', 'is-invalid');
        
        // Solo habilitar campos que no sean el campo de identificación en modo edición
        if (!(modoEdicion && campo.id === 'identificacion')) {
            campo.disabled = false;
        }
        
        // Remover y agregar required para evitar validación HTML5 automática
        if (campo.hasAttribute('required')) {
            campo.removeAttribute('required');
            campo.dataset.wasRequired = 'true';
        }
    });
    
    // Restaurar required después de un momento
    setTimeout(() => {
        campos.forEach(campo => {
            if (campo.dataset.wasRequired === 'true') {
                campo.setAttribute('required', '');
                campo.removeAttribute('data-was-required');
            }
        });
    }, 100);
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

// Funciones para importar estudiantes desde Excel
function importarEstudiantes() {
    const archivo = document.getElementById('archivoExcelEstudiantes').files[0];
    
    if (!archivo) {
        mostrarError('Por favor selecciona un archivo Excel o CSV');
        return;
    }

    // Validar tipo de archivo
    const tiposPermitidos = [
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
        'application/vnd.ms-excel', // .xls
        'text/csv', // .csv
        'application/csv'
    ];

    const extension = archivo.name.toLowerCase().split('.').pop();
    const extensionesPermitidas = ['xlsx', 'xls', 'csv'];

    if (!extensionesPermitidas.includes(extension)) {
        mostrarError('El archivo debe ser un Excel (.xlsx o .xls) o CSV (.csv)');
        return;
    }

    // Mostrar progreso
    const btn = document.getElementById('btnImportarEstudiantes');
    const textoOriginal = btn.textContent;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Importando...';

    const formData = new FormData();
    formData.append('archivo', archivo);

    fetch('/Home/ImportarEstudiantes', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Error en la respuesta del servidor');
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            // Construir mensaje detallado
            let mensaje = data.message;
            
            if (data.advertencias && data.advertencias.length > 0) {
                mensaje += '\n\nAdvertencias:\n';
                data.advertencias.slice(0, 5).forEach(adv => {
                    mensaje += '• ' + adv + '\n';
                });
                if (data.advertencias.length > 5) {
                    mensaje += `• ... y ${data.advertencias.length - 5} advertencias más`;
                }
            }

            if (data.errores && data.errores.length > 0) {
                mensaje += '\n\nErrores menores:\n';
                data.errores.slice(0, 3).forEach(err => {
                    mensaje += '• ' + err + '\n';
                });
                if (data.errores.length > 3) {
                    mensaje += `• ... y ${data.errores.length - 3} errores más`;
                }
            }

            Swal.fire({
                icon: 'success',
                title: '¡Importación Exitosa!',
                text: mensaje,
                timer: 5000,
                showConfirmButton: true
            });
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalImportarEstudiantes'));
            modal.hide();
            
            // Recargar tabla de estudiantes
            setTimeout(() => {
                location.reload();
            }, 2000);
        } else {
            let errorMsg = data.message;
            
            if (data.errores && data.errores.length > 0) {
                errorMsg += '\n\nDetalles:\n';
                data.errores.slice(0, 5).forEach(err => {
                    errorMsg += '• ' + err + '\n';
                });
                if (data.errores.length > 5) {
                    errorMsg += `• ... y ${data.errores.length - 5} errores más`;
                }
            }

            Swal.fire({
                icon: 'error',
                title: 'Error en la Importación',
                text: errorMsg,
                confirmButtonText: 'Entendido'
            });
        }
    })
    .catch(error => {
        console.error('Error:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error de Conexión',
            text: 'Error al procesar la solicitud: ' + error.message,
            confirmButtonText: 'Entendido'
        });
    })
    .finally(() => {
        // Restaurar botón
        btn.disabled = false;
        btn.innerHTML = textoOriginal;
    });
}

function descargarPlantillaEstudiantes() {
    // Mostrar notificación de procesamiento
    mostrarNotificacion('Generando plantilla...', 'info');
    
    // Crear un enlace temporal para la descarga
    const link = document.createElement('a');
    link.href = '/Home/DescargarPlantillaEstudiantes';
    link.download = 'Plantilla_Estudiantes.xlsx';
    link.style.display = 'none';
    document.body.appendChild(link);
    
    // Simular click en el enlace
    link.click();
    
    // Limpiar el enlace temporal
    document.body.removeChild(link);
    
    // Mostrar notificación de éxito
    setTimeout(() => {
        mostrarExito('Plantilla descargada exitosamente');
    }, 500);
}

// Inicializar eventos para importación
document.addEventListener('DOMContentLoaded', function() {
    // Event listener para el input de archivo de estudiantes
    const inputArchivo = document.getElementById('archivoExcelEstudiantes');
    if (inputArchivo) {
        inputArchivo.addEventListener('change', function(e) {
            const archivo = e.target.files[0];
            const btn = document.getElementById('btnImportarEstudiantes');
            
            if (archivo) {
                btn.disabled = false;
                btn.innerHTML = `<i class="fas fa-upload"></i> Importar ${archivo.name}`;
            } else {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-upload"></i> Importar Excel';
            }
        });
    }
});
