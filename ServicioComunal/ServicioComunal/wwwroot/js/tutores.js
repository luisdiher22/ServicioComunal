// Variables globales
let tutorAEliminar = null;
let tutorEnEdicion = null;
let modoEdicion = false;

// Inicialización
document.addEventListener('DOMContentLoaded', function() {
    inicializarEventos();
    inicializarFiltros();
    inicializarImportacion();
});

function inicializarEventos() {
    // Modal de tutor
    const modalTutor = document.getElementById('modalTutor');
    modalTutor.addEventListener('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    modalTutor.addEventListener('shown.bs.modal', function () {
        // Solo limpiar validación si no estamos en modo edición
        if (!modoEdicion) {
            limpiarValidacion();
        }
    });

    // Validación solo en blur después del primer intento
    const form = document.getElementById('formTutor');
    let validacionActivada = false;
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        validacionActivada = true;
        guardarTutor();
    });

    // Solo validar en blur después del primer intento de guardar
    form.addEventListener('blur', function(e) {
        if (validacionActivada && e.target.matches('input, select')) {
            validarCampo(e.target);
        }
    }, true);
}

function inicializarImportacion() {
    // Event listener para el input de archivo
    const inputArchivo = document.getElementById('archivoExcel');
    if (inputArchivo) {
        inputArchivo.addEventListener('change', function(e) {
            const archivo = e.target.files[0];
            if (archivo) {
                const btn = document.getElementById('btnImportar');
                btn.disabled = false;
                btn.textContent = `Importar ${archivo.name}`;
            } else {
                const btn = document.getElementById('btnImportar');
                btn.disabled = true;
                btn.textContent = 'Importar Excel';
            }
        });
    }
}

function inicializarFiltros() {
    // Búsqueda en tiempo real
    document.getElementById('searchInput').addEventListener('input', function() {
        filtrarTabla();
    });

    // Filtros por select
    document.getElementById('filterRol').addEventListener('change', function() {
        filtrarTabla();
    });

    document.getElementById('filterDisponibilidad').addEventListener('change', function() {
        filtrarTabla();
    });
}

function filtrarTabla() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const rolFilter = document.getElementById('filterRol').value;
    const disponibilidadFilter = document.getElementById('filterDisponibilidad').value;
    
    const tbody = document.querySelector('#tablaTutores tbody');
    const rows = tbody.querySelectorAll('tr');
    
    let visibleRows = 0;
    
    rows.forEach(row => {
        const tutor = row.cells[0].textContent.toLowerCase();
        const rol = row.cells[1].textContent.trim();
        const grupos = row.cells[2].textContent;
        const disponibilidad = row.cells[3].textContent;
        
        const cantidadGrupos = parseInt(grupos.match(/\d+/)[0]) || 0;
        
        let mostrar = true;
        
        // Filtro de búsqueda
        if (searchTerm && !tutor.includes(searchTerm)) {
            mostrar = false;
        }
        
        // Filtro de rol
        if (rolFilter && rol !== rolFilter) {
            mostrar = false;
        }
        
        // Filtro de disponibilidad
        if (disponibilidadFilter) {
            switch (disponibilidadFilter) {
                case 'disponible':
                    if (cantidadGrupos > 0) mostrar = false;
                    break;
                case 'asignado':
                    if (cantidadGrupos === 0 || disponibilidad.includes('Capacidad completa')) mostrar = false;
                    break;
                case 'completo':
                    if (!disponibilidad.includes('Capacidad completa')) mostrar = false;
                    break;
            }
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
        const tbody = document.querySelector('#tablaTutores tbody');
        mensajeRow = document.createElement('tr');
        mensajeRow.id = 'noResultsRow';
        mensajeRow.innerHTML = `
            <td colspan="5" class="text-center py-4">
                <div class="no-results">
                    <i class="fas fa-search fa-2x text-muted mb-2"></i>
                    <p class="text-muted">No se encontraron tutores que coincidan con los filtros aplicados.</p>
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
    document.getElementById('filterRol').value = '';
    document.getElementById('filterDisponibilidad').value = '';
    filtrarTabla();
}

function verTutor(identificacion) {
    // Obtener datos del tutor
    fetch(`/Home/ObtenerTutor/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                mostrarDetallesTutor(data.tutor);
            } else {
                mostrarError('Error al cargar los datos del tutor');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
}

function mostrarDetallesTutor(tutor) {
    // TODO: Implementar modal de detalles o navegación a página de detalles
    let detalles = `Detalles del tutor:\n`;
    detalles += `Cédula: ${tutor.identificacion}\n`;
    detalles += `Nombre: ${tutor.nombre} ${tutor.apellidos}\n`;
    detalles += `Rol: ${tutor.rol}\n`;
    detalles += `Grupos asignados: ${tutor.gruposAsignados || 0}`;
    alert(detalles);
}

function editarTutor(identificacion) {
    modoEdicion = true;
    
    // Obtener datos del tutor
    fetch(`/Home/ObtenerTutor/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                cargarDatosEnFormulario(data.tutor, true);
                document.getElementById('modalTutorLabel').textContent = 'Editar Tutor';
                const modal = new bootstrap.Modal(document.getElementById('modalTutor'));
                modal.show();
            } else {
                mostrarError('Error al cargar los datos del tutor');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
}

function cargarDatosEnFormulario(tutor, esEdicion = false) {
    if (tutor) {
        document.getElementById('tutorId').value = tutor.identificacion || '';
        document.getElementById('identificacion').value = tutor.identificacion || '';
        document.getElementById('nombre').value = tutor.nombre || '';
        document.getElementById('apellidos').value = tutor.apellidos || '';
        document.getElementById('rol').value = tutor.rol || '';
    } else {
        // Limpiar formulario si no hay datos
        document.getElementById('tutorId').value = '';
        document.getElementById('identificacion').value = '';
        document.getElementById('nombre').value = '';
        document.getElementById('apellidos').value = '';
        document.getElementById('rol').value = '';
    }
    
    // En modo edición, deshabilitar el campo de cédula
    setTimeout(() => {
        const campoIdentificacion = document.getElementById('identificacion');
        if (campoIdentificacion) {
            if (esEdicion && tutor) {
                campoIdentificacion.disabled = true;
                campoIdentificacion.setAttribute('readonly', 'readonly');
            } else {
                campoIdentificacion.disabled = false;
                campoIdentificacion.removeAttribute('readonly');
            }
        }
    }, 50);
}

// Función para abrir el modal para agregar (sin datos precargados)
function abrirModalAgregar() {
    // Resetear modo edición y limpiar formulario
    modoEdicion = false;
    cargarDatosEnFormulario(null, false);
    
    // Cambiar título del modal
    document.getElementById('modalTutorLabel').textContent = 'Agregar Docente';
    
    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('modalTutor'));
    modal.show();
}

function asignarGrupos(identificacion) {
    tutorEnEdicion = parseInt(identificacion);
    
    console.log('Asignando grupos para tutor:', tutorEnEdicion);
    
    // Obtener información del tutor y cargar grupos
    fetch(`/Home/ObtenerTutor/${tutorEnEdicion}`)
        .then(response => {
            console.log('Respuesta ObtenerTutor:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Datos del tutor:', data);
            if (data.success) {
                document.getElementById('tutorNombreModal').textContent = `${data.tutor.nombre} ${data.tutor.apellidos}`;
                
                // Cargar grupos disponibles y asignados
                console.log('Cargando grupos disponibles y asignados...');
                return Promise.all([
                    fetch('/Home/ObtenerGruposDisponibles'),
                    fetch(`/Home/ObtenerGruposAsignadosATutor?tutorId=${tutorEnEdicion}`)
                ]);
            } else {
                throw new Error(data.message || 'Error al obtener datos del tutor');
            }
        })
        .then(responses => {
            console.log('Respuestas grupos:', responses.map(r => r.status));
            // Verificar que ambas respuestas sean exitosas
            responses.forEach((response, index) => {
                if (!response.ok) {
                    throw new Error(`Error en consulta ${index}: ${response.status}`);
                }
            });
            return Promise.all(responses.map(r => r.json()));
        })
        .then(([disponibles, asignados]) => {
            console.log('Datos grupos disponibles:', disponibles);
            console.log('Datos grupos asignados:', asignados);
            if (disponibles.success && asignados.success) {
                cargarGruposModal(disponibles.grupos, asignados.grupos);
                const modal = new bootstrap.Modal(document.getElementById('modalAsignarGrupos'));
                modal.show();
            } else {
                throw new Error('Error al cargar los grupos: ' + 
                    (!disponibles.success ? disponibles.message : '') + 
                    (!asignados.success ? asignados.message : ''));
            }
        })
        .catch(error => {
            console.error('Error completo:', error);
            mostrarError('Error al comunicarse con el servidor: ' + error.message);
        });
}

function cargarGruposModal(disponibles, asignados) {
    const contenedorDisponibles = document.getElementById('gruposDisponibles');
    const contenedorAsignados = document.getElementById('gruposAsignados');
    
    // Cargar grupos disponibles
    contenedorDisponibles.innerHTML = '';
    disponibles.forEach(grupo => {
        const div = document.createElement('div');
        div.className = 'grupo-item disponible';
        div.innerHTML = `
            <div class="grupo-info">
                <strong>Grupo ${grupo.numero}</strong>
                <small class="text-muted">${grupo.estudiantes} estudiantes</small>
            </div>
            <button class="btn btn-sm btn-success" onclick="asignarGrupoATutor(${grupo.numero})">
                <i class="fas fa-plus"></i>
            </button>
        `;
        contenedorDisponibles.appendChild(div);
    });
    
    // Cargar grupos asignados
    contenedorAsignados.innerHTML = '';
    asignados.forEach(grupo => {
        const div = document.createElement('div');
        div.className = 'grupo-item asignado';
        div.innerHTML = `
            <div class="grupo-info">
                <strong>Grupo ${grupo.numero}</strong>
                <small class="text-muted">${grupo.estudiantes} estudiantes</small>
            </div>
            <button class="btn btn-sm btn-danger" onclick="quitarGrupoDeTutor(${grupo.numero})">
                <i class="fas fa-times"></i>
            </button>
        `;
        contenedorAsignados.appendChild(div);
    });
}

function asignarGrupoATutor(grupoNumero) {
    console.log('Asignando grupo', grupoNumero, 'al tutor', tutorEnEdicion);
    
    fetch('/Home/AsignarGrupoATutor', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            tutorId: parseInt(tutorEnEdicion),
            grupoNumero: parseInt(grupoNumero)
        })
    })
    .then(response => {
        console.log('Respuesta AsignarGrupoATutor:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Resultado asignación:', data);
        if (data.success) {
            // Recargar los grupos en el modal
            asignarGrupos(tutorEnEdicion);
            mostrarExito('Grupo asignado al tutor');
        } else {
            mostrarError(data.message || 'Error al asignar el grupo al tutor');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor: ' + error.message);
    });
}

function quitarGrupoDeTutor(grupoNumero) {
    console.log('Quitando grupo', grupoNumero, 'del tutor', tutorEnEdicion);
    
    fetch('/Home/QuitarGrupoDeTutor', {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            tutorId: parseInt(tutorEnEdicion),
            grupoNumero: parseInt(grupoNumero)
        })
    })
    .then(response => {
        console.log('Respuesta QuitarGrupoDeTutor:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Resultado remoción:', data);
        if (data.success) {
            // Recargar los grupos en el modal
            asignarGrupos(tutorEnEdicion);
            mostrarExito('Grupo quitado del tutor');
        } else {
            mostrarError(data.message || 'Error al quitar el grupo del tutor');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor: ' + error.message);
    });
}

function asignacionAutomatica() {
    if (confirm('¿Deseas realizar una asignación automática de tutores a grupos? Esta acción asignará tutores disponibles a grupos sin tutor.')) {
        fetch('/Home/AsignacionAutomaticaTutores', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                mostrarExito(`Asignación automática completada. ${data.asignaciones} grupos asignados.`);
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                mostrarError(data.message || 'Error en la asignación automática');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
    }
}

function eliminarTutor(identificacion) {
    tutorAEliminar = identificacion;
    
    // Obtener datos del tutor para mostrar en el modal
    fetch(`/Home/ObtenerTutor/${identificacion}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('tutorInfo').innerHTML = `
                    <strong>Cédula:</strong> ${data.tutor.identificacion}<br>
                    <strong>Nombre:</strong> ${data.tutor.nombre} ${data.tutor.apellidos}<br>
                    <strong>Rol:</strong> ${data.tutor.rol}<br>
                    <strong>Grupos asignados:</strong> ${data.tutor.gruposAsignados || 0}
                `;
                
                const modal = new bootstrap.Modal(document.getElementById('modalEliminar'));
                modal.show();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al cargar los datos del tutor');
        });
}

// Confirmar eliminación
document.getElementById('btnConfirmarEliminar').addEventListener('click', function() {
    if (tutorAEliminar) {
        fetch(`/Home/EliminarTutor/${tutorAEliminar}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Remover la fila de la tabla
                const row = document.querySelector(`tr[data-tutor-id="${tutorAEliminar}"]`);
                if (row) {
                    row.remove();
                }
                
                // Actualizar estadísticas
                actualizarEstadisticas();
                
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEliminar'));
                modal.hide();
                
                mostrarExito('Tutor eliminado exitosamente');
            } else {
                mostrarError(data.message || 'Error al eliminar el tutor');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        })
        .finally(() => {
            tutorAEliminar = null;
        });
    }
});

function guardarTutor() {
    const form = document.getElementById('formTutor');
    
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
    
    const tutor = {
        identificacion: parseInt(formData.get('identificacion')),
        nombre: formData.get('nombre').trim(),
        apellidos: formData.get('apellidos').trim(),
        rol: formData.get('rol')
    };
    
    // Si estamos en modo edición, incluir la identificación original
    if (modoEdicion) {
        const tutorId = formData.get('tutorId');
        tutor.identificacionOriginal = parseInt(tutorId);
    }
    
    const url = modoEdicion ? '/Home/ActualizarTutor' : '/Home/CrearTutor';
    const method = modoEdicion ? 'PUT' : 'POST';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(tutor)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalTutor'));
            modal.hide();
            
            mostrarExito(modoEdicion ? 'Tutor actualizado exitosamente' : 'Tutor creado exitosamente');
            
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            mostrarError(data.message || 'Error al guardar el tutor');
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
    const form = document.getElementById('formTutor');
    form.reset();
    
    // Limpiar clases de validación
    limpiarValidacion();
    
    // Restablecer título del modal
    document.getElementById('modalTutorLabel').textContent = 'Agregar Tutor';
    modoEdicion = false;
}

function limpiarValidacion() {
    const form = document.getElementById('formTutor');
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
    const rows = document.querySelectorAll('#tablaTutores tbody tr:not(#noResultsRow)');
    let total = 0;
    let conGrupos = 0;
    let disponibles = 0;
    let totalGruposAsignados = 0;
    
    rows.forEach(row => {
        if (row.style.display !== 'none') {
            total++;
            const grupos = row.cells[2].textContent;
            const cantidadGrupos = parseInt(grupos.match(/\d+/)[0]) || 0;
            
            totalGruposAsignados += cantidadGrupos;
            
            if (cantidadGrupos > 0) {
                conGrupos++;
            } else {
                disponibles++;
            }
        }
    });
    
    document.getElementById('totalTutores').textContent = total;
    document.getElementById('conGrupos').textContent = conGrupos;
    document.getElementById('disponibles').textContent = disponibles;
    document.getElementById('promedio').textContent = total > 0 ? (totalGruposAsignados / total).toFixed(1) : 0;
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

// Función para descargar la plantilla de Excel
function descargarPlantilla() {
    // Crear datos de ejemplo
    const datosEjemplo = [
        ['Apellido', 'Nombre', 'Cédula'],
        ['García', 'María José', '123456789'],
        ['Rodríguez', 'Juan Carlos', '987654321'],
        ['López', 'Ana Sofía', '456789123']
    ];

    // Crear workbook y worksheet
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(datosEjemplo);

    // Agregar worksheet al workbook
    XLSX.utils.book_append_sheet(wb, ws, 'Docentes');

    // Descargar archivo
    XLSX.writeFile(wb, 'Plantilla_Docentes.xlsx');
}

// Función para importar profesores desde Excel
function importarProfesores() {
    const archivo = document.getElementById('archivoExcel').files[0];
    
    if (!archivo) {
        mostrarError('Por favor selecciona un archivo Excel');
        return;
    }

    // Validar tipo de archivo
    const tiposPermitidos = [
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
        'application/vnd.ms-excel' // .xls
    ];

    if (!tiposPermitidos.includes(archivo.type)) {
        mostrarError('El archivo debe ser un Excel (.xlsx o .xls)');
        return;
    }

    // Mostrar progreso
    const btn = document.getElementById('btnImportar');
    const textoOriginal = btn.textContent;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Importando...';

    const formData = new FormData();
    formData.append('archivo', archivo);

    fetch('/Home/ImportarProfesores', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            mostrarExito(`Importación exitosa: ${data.procesados} docentes procesados, ${data.nuevos} nuevos, ${data.errores} errores`);
            
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalImportar'));
            modal.hide();
            
            // Recargar página después de 2 segundos
            setTimeout(() => {
                location.reload();
            }, 2000);
        } else {
            mostrarError(data.message || 'Error durante la importación');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor');
    })
    .finally(() => {
        // Restaurar botón
        btn.disabled = false;
        btn.textContent = textoOriginal;
        
        // Limpiar input
        document.getElementById('archivoExcel').value = '';
    });
}

// Función para cambiar el rol de un profesor
function cambiarRol(identificacion, nuevoRol) {
    console.log(`Cambiando rol para identificacion: ${identificacion} a: ${nuevoRol}`);
    
    // Confirmar cambio
    let mensaje = `¿Estás seguro de cambiar el rol del docente a ${nuevoRol}?`;
    if (nuevoRol === 'Administrador') {
        mensaje += '\n\nEl docente tendrá acceso completo a las funciones administrativas.';
    }

    if (!confirm(mensaje)) {
        console.log('Cambio de rol cancelado por el usuario');
        // Restaurar valor anterior del select
        const select = document.querySelector(`select[data-profesor-id="${identificacion}"]`);
        if (select) {
            select.value = select.dataset.rolOriginal || 'Profesor';
        }
        return;
    }

    console.log('Enviando solicitud de cambio de rol...');

    fetch('/Home/CambiarRolProfesor', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            profesorId: parseInt(identificacion),
            nuevoRol: nuevoRol
        })
    })
    .then(response => {
        console.log('Respuesta recibida:', response.status);
        return response.json();
    })
    .then(data => {
        console.log('Datos de respuesta:', data);
        if (data.success) {
            mostrarExito('Rol actualizado exitosamente');
            
            // Actualizar la interfaz
            const badge = document.querySelector(`tr[data-tutor-id="${identificacion}"] .badge-rol`);
            console.log('Badge encontrado:', badge);
            if (badge) {
                // Remover clases anteriores
                badge.classList.remove('administrador', 'profesor', 'tutor', 'coordinador', 'supervisor');
                badge.className = 'badge badge-rol';
                
                if (nuevoRol === 'Administrador') {
                    badge.classList.add('administrador');
                    badge.innerHTML = '<i class="fas fa-crown"></i> Administrador';
                    console.log('Badge actualizado a Administrador');
                } else {
                    badge.classList.add(nuevoRol.toLowerCase());
                    badge.textContent = nuevoRol;
                    console.log(`Badge actualizado a ${nuevoRol}`);
                }
            } else {
                console.log('No se encontró el badge para actualizar');
            }
            
            // Actualizar el valor del select
            const select = document.querySelector(`select[data-profesor-id="${identificacion}"]`);
            if (select) {
                select.dataset.rolOriginal = nuevoRol;
                console.log(`Select actualizado, rol original ahora es: ${nuevoRol}`);
            }
        } else {
            console.error('Error del servidor:', data.message);
            mostrarError(data.message || 'Error al cambiar el rol');
            
            // Restaurar valor anterior del select
            const select = document.querySelector(`select[data-profesor-id="${identificacion}"]`);
            if (select) {
                select.value = select.dataset.rolOriginal || 'Profesor';
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor');
        
        // Restaurar valor anterior del select
        const select = document.querySelector(`select[data-profesor-id="${identificacion}"]`);
        if (select) {
            select.value = select.dataset.rolOriginal || 'Profesor';
        }
    });
}
