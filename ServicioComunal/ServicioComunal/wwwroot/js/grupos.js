// Variables globales
let grupoAEliminar = null;
let grupoEnEdicion = null;

// Debug function
console.log('grupos.js cargado correctamente');

// Inicialización
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM cargado, inicializando eventos');
    inicializarEventos();
    inicializarFiltros();
});

function inicializarEventos() {
    // Modal de grupo
    const modalGrupo = document.getElementById('modalGrupo');
    modalGrupo.addEventListener('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    modalGrupo.addEventListener('shown.bs.modal', function () {
        limpiarValidacion();
    });

    // Validación solo en blur después del primer intento
    const form = document.getElementById('formGrupo');
    let validacionActivada = false;
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        validacionActivada = true;
        crearGrupo();
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
    document.getElementById('filterEstado').addEventListener('change', function() {
        filtrarTabla();
    });

    document.getElementById('filterTamaño').addEventListener('change', function() {
        filtrarTabla();
    });
}

function filtrarTabla() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const estadoFilter = document.getElementById('filterEstado').value;
    const tamañoFilter = document.getElementById('filterTamaño').value;
    
    const tbody = document.querySelector('#tablaGrupos tbody');
    const rows = tbody.querySelectorAll('tr');
    
    let visibleRows = 0;
    
    rows.forEach(row => {
        const numero = row.cells[0].textContent.toLowerCase();
        const estudiantes = row.cells[1].textContent;
        const tutor = row.cells[2].textContent;
        const estado = row.cells[3].textContent;
        
        const cantidadEstudiantes = parseInt(estudiantes.match(/\d+/)[0]) || 0;
        const tieneTutor = !tutor.includes('Sin asignar');
        const tieneEstudiantes = cantidadEstudiantes > 0;
        
        let mostrar = true;
        
        // Filtro de búsqueda
        if (searchTerm && !numero.includes(searchTerm)) {
            mostrar = false;
        }
        
        // Filtro de estado
        if (estadoFilter) {
            switch (estadoFilter) {
                case 'con-estudiantes':
                    if (!tieneEstudiantes) mostrar = false;
                    break;
                case 'sin-estudiantes':
                    if (tieneEstudiantes) mostrar = false;
                    break;
                case 'con-tutor':
                    if (!tieneTutor) mostrar = false;
                    break;
                case 'sin-tutor':
                    if (tieneTutor) mostrar = false;
                    break;
            }
        }
        
        // Filtro de tamaño
        if (tamañoFilter) {
            switch (tamañoFilter) {
                case 'pequeno':
                    if (cantidadEstudiantes < 1 || cantidadEstudiantes > 2) mostrar = false;
                    break;
                case 'mediano':
                    if (cantidadEstudiantes < 3 || cantidadEstudiantes > 4) mostrar = false;
                    break;
                case 'grande':
                    if (cantidadEstudiantes < 5) mostrar = false;
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
        const tbody = document.querySelector('#tablaGrupos tbody');
        mensajeRow = document.createElement('tr');
        mensajeRow.id = 'noResultsRow';
        mensajeRow.innerHTML = `
            <td colspan="5" class="text-center py-4">
                <div class="no-results">
                    <i class="fas fa-search fa-2x text-muted mb-2"></i>
                    <p class="text-muted">No se encontraron grupos que coincidan con los filtros aplicados.</p>
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
    document.getElementById('filterEstado').value = '';
    document.getElementById('filterTamaño').value = '';
    filtrarTabla();
}

function crearGrupo() {
    const form = document.getElementById('formGrupo');
    
    if (!validarFormulario(form)) {
        return;
    }
    
    const formData = new FormData(form);
    const grupo = {
        numero: parseInt(formData.get('numero'))
    };
    
    fetch('/Home/CrearGrupo', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(grupo)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalGrupo'));
            modal.hide();
            
            mostrarExito('Grupo creado exitosamente');
            
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            mostrarError(data.message || 'Error al crear el grupo');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        mostrarError('Error al comunicarse con el servidor');
    });
}

function verGrupo(numero) {
    // Obtener datos del grupo
    fetch(`/Home/ObtenerGrupo/${numero}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                mostrarDetallesGrupo(data.grupo);
            } else {
                mostrarError('Error al cargar los datos del grupo');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor');
        });
}

function mostrarDetallesGrupo(grupo) {
    // TODO: Implementar modal de detalles o navegación a página de detalles
    let detalles = `Detalles del Grupo ${grupo.numero}:\n`;
    detalles += `Estudiantes: ${grupo.estudiantes.length}\n`;
    if (grupo.tutor) {
        detalles += `Tutor: ${grupo.tutor.nombre} ${grupo.tutor.apellidos}`;
    } else {
        detalles += 'Sin tutor asignado';
    }
    alert(detalles);
}

// Variables globales para gestión de estudiantes
let estudiantesDisponiblesData = [];
let estudiantesGrupoData = [];
let grupoActual = null;

function gestionarEstudiantes(numero) {
    grupoActual = numero;
    document.getElementById('grupoNumeroModal').textContent = numero;
    
    // Mostrar modal
    const modal = new bootstrap.Modal(document.getElementById('modalGestionarEstudiantes'));
    modal.show();
    
    // Cargar datos
    cargarEstudiantesParaGestion(numero);
    
    // Configurar event listeners
    configurarEventListenersGestion();
}

// Cargar estudiantes disponibles y del grupo
async function cargarEstudiantesParaGestion(numeroGrupo) {
    try {
        // Mostrar loading
        document.getElementById('estudiantesDisponibles').innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-spinner fa-spin fa-2x mb-2"></i>
                <div>Cargando estudiantes...</div>
            </div>
        `;
        document.getElementById('estudiantesGrupo').innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-spinner fa-spin fa-2x mb-2"></i>
                <div>Cargando grupo...</div>
            </div>
        `;

        // Cargar estudiantes disponibles (sin grupo)
        const responseDisponibles = await fetch('/Home/ObtenerEstudiantesSinGrupo');
        const dataDisponibles = await responseDisponibles.json();
        
        if (dataDisponibles.success) {
            estudiantesDisponiblesData = dataDisponibles.estudiantes;
        } else {
            throw new Error(dataDisponibles.message);
        }

        // Cargar estudiantes del grupo actual
        const responseGrupo = await fetch(`/Home/ObtenerGrupo/${numeroGrupo}`);
        const dataGrupo = await responseGrupo.json();
        
        if (dataGrupo.success) {
            estudiantesGrupoData = dataGrupo.grupo.estudiantes || [];
        } else {
            estudiantesGrupoData = [];
        }

        // Renderizar listas
        renderizarEstudiantesDisponibles();
        renderizarEstudiantesGrupo();
        actualizarContadores();
        
    } catch (error) {
        console.error('Error cargando estudiantes:', error);
        mostrarError('Error al cargar estudiantes: ' + error.message);
    }
}

// Renderizar lista de estudiantes disponibles
function renderizarEstudiantesDisponibles() {
    const container = document.getElementById('estudiantesDisponibles');
    const filtro = document.getElementById('buscarDisponibles') ? document.getElementById('buscarDisponibles').value.toLowerCase() : '';
    
    const estudiantesFiltrados = estudiantesDisponiblesData.filter(est => 
        est.nombre.toLowerCase().includes(filtro) || 
        est.apellidos.toLowerCase().includes(filtro) ||
        est.identificacion.toString().includes(filtro) ||
        est.clase.toLowerCase().includes(filtro)
    );

    if (estudiantesFiltrados.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-user-slash fa-2x mb-2"></i>
                <div>${filtro ? 'No se encontraron estudiantes' : 'No hay estudiantes disponibles'}</div>
            </div>
        `;
        return;
    }

    container.innerHTML = estudiantesFiltrados.map(estudiante => `
        <div class="estudiante-item mb-2 p-2 border rounded" 
             data-id="${estudiante.identificacion}"
             onclick="toggleSeleccion(this, 'disponibles')"
             ondblclick="moverEstudianteAGrupo(${estudiante.identificacion})"
             style="cursor: pointer;">
            <div class="d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-2" onclick="event.stopPropagation()">
                <div class="flex-grow-1">
                    <div class="fw-bold">${estudiante.nombre} ${estudiante.apellidos}</div>
                    <small class="text-muted">
                        <i class="fas fa-id-card me-1"></i>${estudiante.identificacion} - 
                        <i class="fas fa-graduation-cap me-1"></i>${estudiante.clase}
                    </small>
                </div>
            </div>
        </div>
    `).join('');
}

// Renderizar lista de estudiantes del grupo
function renderizarEstudiantesGrupo() {
    const container = document.getElementById('estudiantesGrupo');
    const filtro = document.getElementById('buscarGrupo') ? document.getElementById('buscarGrupo').value.toLowerCase() : '';
    
    const estudiantesFiltrados = estudiantesGrupoData.filter(est => 
        est.nombre.toLowerCase().includes(filtro) || 
        est.apellidos.toLowerCase().includes(filtro) ||
        est.identificacion.toString().includes(filtro) ||
        est.clase.toLowerCase().includes(filtro)
    );

    if (estudiantesFiltrados.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-users fa-2x mb-2"></i>
                <div>${filtro ? 'No se encontraron estudiantes en el grupo' : 'No hay estudiantes en este grupo'}</div>
            </div>
        `;
        return;
    }

    container.innerHTML = estudiantesFiltrados.map(estudiante => `
        <div class="estudiante-item mb-2 p-2 border rounded border-primary bg-light" 
             data-id="${estudiante.identificacion}"
             onclick="toggleSeleccion(this, 'grupo')"
             ondblclick="moverEstudianteADisponibles(${estudiante.identificacion})"
             style="cursor: pointer;">
            <div class="d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-2" onclick="event.stopPropagation()">
                <div class="flex-grow-1">
                    <div class="fw-bold">${estudiante.nombre} ${estudiante.apellidos}</div>
                    <small class="text-muted">
                        <i class="fas fa-id-card me-1"></i>${estudiante.identificacion} - 
                        <i class="fas fa-graduation-cap me-1"></i>${estudiante.clase}
                    </small>
                </div>
                <i class="fas fa-check-circle text-success"></i>
            </div>
        </div>
    `).join('');
}

// Toggle selección de estudiante
function toggleSeleccion(elemento, tipo) {
    const checkbox = elemento.querySelector('input[type="checkbox"]');
    checkbox.checked = !checkbox.checked;
    
    if (checkbox.checked) {
        elemento.classList.add('selected');
        elemento.style.backgroundColor = tipo === 'disponibles' ? '#e3f2fd' : '#f3e5f5';
    } else {
        elemento.classList.remove('selected');
        elemento.style.backgroundColor = tipo === 'disponibles' ? '' : '#f8f9fa';
    }
}

// Mover estudiante específico al grupo
function moverEstudianteAGrupo(identificacion) {
    const estudiante = estudiantesDisponiblesData.find(e => e.identificacion === identificacion);
    if (estudiante) {
        // Mover de disponibles a grupo
        estudiantesDisponiblesData = estudiantesDisponiblesData.filter(e => e.identificacion !== identificacion);
        estudiantesGrupoData.push(estudiante);
        
        // Actualizar renderizado
        renderizarEstudiantesDisponibles();
        renderizarEstudiantesGrupo();
        actualizarContadores();
    }
}

// Mover estudiante específico a disponibles
function moverEstudianteADisponibles(identificacion) {
    const estudiante = estudiantesGrupoData.find(e => e.identificacion === identificacion);
    if (estudiante) {
        // Mover de grupo a disponibles
        estudiantesGrupoData = estudiantesGrupoData.filter(e => e.identificacion !== identificacion);
        estudiantesDisponiblesData.push(estudiante);
        
        // Actualizar renderizado
        renderizarEstudiantesDisponibles();
        renderizarEstudiantesGrupo();
        actualizarContadores();
    }
}

// Mover estudiantes seleccionados al grupo
function moverSeleccionadosAGrupo() {
    const seleccionados = document.querySelectorAll('#estudiantesDisponibles .estudiante-item input:checked');
    const identificaciones = Array.from(seleccionados).map(cb => 
        parseInt(cb.closest('.estudiante-item').dataset.id)
    );
    
    identificaciones.forEach(id => moverEstudianteAGrupo(id));
}

// Mover estudiantes seleccionados a disponibles
function moverSeleccionadosADisponibles() {
    const seleccionados = document.querySelectorAll('#estudiantesGrupo .estudiante-item input:checked');
    const identificaciones = Array.from(seleccionados).map(cb => 
        parseInt(cb.closest('.estudiante-item').dataset.id)
    );
    
    identificaciones.forEach(id => moverEstudianteADisponibles(id));
}

// Mover todos los estudiantes al grupo
function moverTodosAGrupo() {
    estudiantesDisponiblesData.forEach(estudiante => {
        estudiantesGrupoData.push(estudiante);
    });
    estudiantesDisponiblesData = [];
    
    renderizarEstudiantesDisponibles();
    renderizarEstudiantesGrupo();
    actualizarContadores();
}

// Mover todos los estudiantes a disponibles
function moverTodosADisponibles() {
    estudiantesGrupoData.forEach(estudiante => {
        estudiantesDisponiblesData.push(estudiante);
    });
    estudiantesGrupoData = [];
    
    renderizarEstudiantesDisponibles();
    renderizarEstudiantesGrupo();
    actualizarContadores();
}

// Actualizar contadores
function actualizarContadores() {
    const countDisponibles = document.getElementById('countDisponibles');
    const countGrupo = document.getElementById('countGrupo');
    
    if (countDisponibles) countDisponibles.textContent = estudiantesDisponiblesData.length;
    if (countGrupo) countGrupo.textContent = estudiantesGrupoData.length;
}

// Configurar event listeners para la gestión
function configurarEventListenersGestion() {
    // Botones de movimiento
    const btnMoverSelAGrupo = document.getElementById('moverSeleccionadosAGrupo');
    const btnMoverSelADisp = document.getElementById('moverSeleccionadosADisponibles');
    const btnMoverTodosAGrupo = document.getElementById('moverTodosAGrupo');
    const btnMoverTodosADisp = document.getElementById('moverTodosADisponibles');
    
    if (btnMoverSelAGrupo) btnMoverSelAGrupo.onclick = moverSeleccionadosAGrupo;
    if (btnMoverSelADisp) btnMoverSelADisp.onclick = moverSeleccionadosADisponibles;
    if (btnMoverTodosAGrupo) btnMoverTodosAGrupo.onclick = moverTodosAGrupo;
    if (btnMoverTodosADisp) btnMoverTodosADisp.onclick = moverTodosADisponibles;
    
    // Filtros de búsqueda
    const buscarDisp = document.getElementById('buscarDisponibles');
    const buscarGrupo = document.getElementById('buscarGrupo');
    
    if (buscarDisp) buscarDisp.oninput = renderizarEstudiantesDisponibles;
    if (buscarGrupo) buscarGrupo.oninput = renderizarEstudiantesGrupo;
    
    // Botón guardar
    const btnGuardar = document.getElementById('guardarAsignaciones');
    if (btnGuardar) btnGuardar.onclick = guardarAsignaciones;
}

// Función dedicada para cerrar el modal de gestión de estudiantes
function cerrarModalGestionEstudiantes() {
    console.log('Intentando cerrar modal...');
    
    const modalId = 'modalGestionarEstudiantes';
    const modalElement = document.getElementById(modalId);
    
    if (!modalElement) {
        console.log('Modal no encontrado');
        return;
    }
    
    // Método 1: jQuery (si está disponible)
    if (typeof $ !== 'undefined') {
        console.log('Cerrando con jQuery...');
        $(`#${modalId}`).modal('hide');
    }
    
    // Método 2: Bootstrap 5 nativo
    try {
        console.log('Cerrando con Bootstrap 5...');
        const modal = bootstrap.Modal.getInstance(modalElement);
        if (modal) {
            console.log('Instancia encontrada, cerrando...');
            modal.hide();
        } else {
            console.log('Creando nueva instancia...');
            const newModal = new bootstrap.Modal(modalElement);
            newModal.hide();
        }
    } catch (e) {
        console.error('Error con Bootstrap modal:', e);
    }
    
    // Método 3: Fallback manual
    setTimeout(() => {
        console.log('Aplicando fallback manual...');
        modalElement.classList.remove('show');
        modalElement.style.display = 'none';
        modalElement.setAttribute('aria-hidden', 'true');
        modalElement.removeAttribute('aria-modal');
        
        // Remover backdrop
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(backdrop => backdrop.remove());
        
        // Remover clase del body
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
    }, 500);
}

// Guardar asignaciones en la base de datos
async function guardarAsignaciones() {
    console.log('guardarAsignaciones iniciado');
    try {
        const btnGuardar = document.getElementById('guardarAsignaciones');
        const textoOriginal = btnGuardar.innerHTML;
        
        // Mostrar loading
        btnGuardar.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Guardando...';
        btnGuardar.disabled = true;

        // Preparar datos para enviar
        const asignaciones = {
            grupoNumero: grupoActual,
            estudiantesIds: estudiantesGrupoData.map(e => e.identificacion)
        };

        console.log('Enviando asignaciones:', asignaciones);

        // Enviar al servidor
        const response = await fetch('/Home/ActualizarAsignacionesGrupo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(asignaciones)
        });

        const data = await response.json();
        console.log('Respuesta del servidor:', data);
        
        if (data.success) {
            console.log('Guardado exitoso, cerrando modal...');
            mostrarExito('Asignaciones guardadas correctamente');
            
            // Restaurar botón inmediatamente
            const btnGuardar = document.getElementById('guardarAsignaciones');
            if (btnGuardar) {
                btnGuardar.innerHTML = '<i class="fas fa-save me-2"></i>Guardar Cambios';
                btnGuardar.disabled = false;
            }
            
            // Cerrar modal con función dedicada
            cerrarModalGestionEstudiantes();
            
            // Recargar tabla después de un pequeño delay
            setTimeout(() => {
                console.log('Recargando página...');
                location.reload();
            }, 2000);
        } else {
            throw new Error(data.message);
        }
        
    } catch (error) {
        console.error('Error guardando asignaciones:', error);
        mostrarError('Error al guardar asignaciones: ' + error.message);
    } finally {
        // Restaurar botón
        const btnGuardar = document.getElementById('guardarAsignaciones');
        btnGuardar.innerHTML = '<i class="fas fa-save me-2"></i>Guardar Cambios';
        btnGuardar.disabled = false;
    }
}

function eliminarGrupo(numero) {
    grupoAEliminar = numero;
    
    // Obtener datos del grupo para mostrar en el modal
    fetch(`/Home/ObtenerGrupo/${numero}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById('grupoInfo').innerHTML = `
                    <strong>Número:</strong> ${data.grupo.numero}<br>
                    <strong>Estudiantes:</strong> ${data.grupo.estudiantes.length}<br>
                    <strong>Tutor:</strong> ${data.grupo.tutor ? data.grupo.tutor.nombre + ' ' + data.grupo.tutor.apellidos : 'Sin asignar'}
                `;
                
                const modal = new bootstrap.Modal(document.getElementById('modalEliminar'));
                modal.show();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al cargar los datos del grupo');
        });
}

// Confirmar eliminación
document.getElementById('btnConfirmarEliminar').addEventListener('click', function() {
    if (grupoAEliminar) {
        fetch(`/Home/EliminarGrupo/${grupoAEliminar}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la respuesta del servidor');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Remover la fila de la tabla
                const row = document.querySelector(`tr[data-grupo-id="${grupoAEliminar}"]`);
                if (row) {
                    row.remove();
                }
                
                // Actualizar el contador de grupos decrementándolo
                const totalGruposElement = document.getElementById('totalGrupos');
                const currentTotal = parseInt(totalGruposElement.textContent) || 0;
                totalGruposElement.textContent = Math.max(0, currentTotal - 1);
                
                // Actualizar estadísticas completas
                actualizarEstadisticas();
                
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEliminar'));
                modal.hide();
                
                mostrarExito('Grupo eliminado exitosamente');
            } else {
                mostrarError(data.message || 'Error al eliminar el grupo');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al comunicarse con el servidor: ' + error.message);
        })
        .finally(() => {
            grupoAEliminar = null;
        });
    }
});

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
        case 'numeroGrupo':
            if (campo.value && (isNaN(campo.value) || campo.value <= 0)) {
                valido = false;
                mensaje = 'Debe ser un número válido mayor a 0';
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
    const form = document.getElementById('formGrupo');
    form.reset();
    
    // Limpiar clases de validación
    limpiarValidacion();
}

function limpiarValidacion() {
    const form = document.getElementById('formGrupo');
    const campos = form.querySelectorAll('.form-control, .form-select');
    campos.forEach(campo => {
        campo.classList.remove('is-valid', 'is-invalid');
        
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
    const rows = document.querySelectorAll('#tablaGrupos tbody tr:not(#noResultsRow)');
    let total = 0;
    let conEstudiantes = 0;
    let conTutor = 0;
    let sinTutor = 0;
    
    rows.forEach(row => {
        if (row.style.display !== 'none') {
            total++;
            const estudiantes = row.cells[1].textContent;
            const tutor = row.cells[2].textContent;
            
            const match = estudiantes.match(/\d+/);
            const cantidadEstudiantes = match ? parseInt(match[0]) : 0;
            const tieneTutor = !tutor.includes('Sin asignar');
            
            if (cantidadEstudiantes > 0) {
                conEstudiantes++;
                if (tieneTutor) {
                    conTutor++;
                } else {
                    sinTutor++;
                }
            }
        }
    });
    
    document.getElementById('totalGrupos').textContent = total;
    document.getElementById('conEstudiantes').textContent = conEstudiantes;
    document.getElementById('conTutor').textContent = conTutor;
    document.getElementById('sinTutor').textContent = sinTutor;
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

// ===== NUEVAS FUNCIONES PARA GESTIÓN AVANZADA =====

function verDetallesAvanzados(grupoNumero) {
    console.log('verDetallesAvanzados llamado para grupo:', grupoNumero);
    
    // Limpiar contenido y mostrar spinner
    const contenidoDiv = document.getElementById('contenidoGestionAvanzada');
    contenidoDiv.innerHTML = '<div class="d-flex justify-content-center"><div class="spinner-border" role="status"></div></div>';
    
    // Mostrar modal usando Bootstrap 5
    const modal = new bootstrap.Modal(document.getElementById('modalGestionAvanzada'));
    modal.show();
    
    fetch(`/Home/ObtenerDetallesGrupoCompleto?grupoNumero=${grupoNumero}`)
        .then(response => {
            console.log('Respuesta recibida:', response);
            return response.json();
        })
        .then(data => {
            console.log('Datos recibidos:', data);
            if (!data.success) {
                contenidoDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-times"></i> Error: ${data.message}
                    </div>
                `;
                return;
            }

            const grupo = data.grupo;
            let liderActual = grupo.miembros.find(m => m.esLider);
            
            let html = `
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        <h5 class="mb-0">
                            <i class="fas fa-users"></i> Grupo ${grupo.numero}
                            ${grupo.liderIdentificacion ? `<small class="float-end">👑 Líder: ${grupo.nombreLider}</small>` : '<small class="float-end text-warning">⚠️ Sin líder</small>'}
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row mb-3">
                            <div class="col-md-4">
                                <strong>Miembros:</strong> ${grupo.cantidadMiembros}/4
                            </div>
                            <div class="col-md-4">
                                <strong>Entregas:</strong> ${grupo.cantidadEntregas}
                            </div>
                            <div class="col-md-4">
                                <strong>Tutor:</strong> ${grupo.tieneTutor ? 'Sí' : 'No'}
                            </div>
                        </div>
            `;

            if (grupo.miembros && grupo.miembros.length > 0) {
                html += `
                    <h6><i class="fas fa-users"></i> Miembros del Grupo:</h6>
                    <div class="table-responsive">
                        <table class="table table-hover">
                            <thead>
                                <tr>
                                    <th>Estudiante</th>
                                    <th>Clase</th>
                                    <th>Rol</th>
                                    <th>Acciones</th>
                                </tr>
                            </thead>
                            <tbody>
                `;

                grupo.miembros.forEach(miembro => {
                    let iconoRol = miembro.esLider ? '👑' : '👤';
                    let textoRol = miembro.esLider ? 'Líder' : 'Miembro';
                    let badgeClass = miembro.esLider ? 'bg-warning' : 'bg-secondary';
                    
                    html += `
                        <tr>
                            <td>
                                ${iconoRol} <strong>${miembro.nombreCompleto}</strong>
                            </td>
                            <td>${miembro.clase}</td>
                            <td>
                                <span class="badge ${badgeClass}">${textoRol}</span>
                            </td>
                            <td>
                                <div class="btn-group btn-group-sm" role="group">
                    `;
                    
                    if (!miembro.esLider) {
                        html += `
                            <button class="btn btn-outline-warning" onclick="cambiarLiderGrupo(${grupo.numero}, ${miembro.estudianteId}, '${miembro.nombreCompleto}')" title="Hacer líder">
                                <i class="fas fa-crown"></i>
                            </button>
                        `;
                    }
                    
                    html += `
                            <button class="btn btn-outline-danger" onclick="eliminarEstudianteDeGrupo(${grupo.numero}, ${miembro.estudianteId}, '${miembro.nombreCompleto}', ${miembro.esLider})" title="Eliminar del grupo">
                                <i class="fas fa-user-times"></i>
                            </button>
                        </div>
                    </td>
                </tr>
                    `;
                });

                html += `
                            </tbody>
                        </table>
                    </div>
                `;
            } else {
                html += `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle"></i> Este grupo no tiene miembros.
                    </div>
                `;
            }

            html += `
                    </div>
                </div>
            `;

            contenidoDiv.innerHTML = html;
        })
        .catch(error => {
            console.error('Error en fetch:', error);
            contenidoDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-times"></i> Error al cargar detalles: ${error.message}
                </div>
            `;
        });
}

function eliminarEstudianteDeGrupo(grupoNumero, estudianteId, nombreEstudiante, esLider) {
    let mensaje = `¿Estás seguro de que quieres eliminar a "${nombreEstudiante}" del Grupo ${grupoNumero}?`;
    let icono = 'warning';
    
    if (esLider) {
        mensaje += '<br><br><strong>⚠️ ATENCIÓN:</strong> Este estudiante es el líder del grupo. Si lo eliminas, el liderazgo se asignará automáticamente a otro miembro (si los hay).';
        icono = 'error';
    }
    
    Swal.fire({
        title: 'Confirmar eliminación',
        html: mensaje,
        icon: icono,
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (!result.isConfirmed) {
            return;
        }

        fetch('/Home/EliminarEstudianteDeGrupo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                grupoNumero: grupoNumero,
                estudianteId: estudianteId
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                mostrarExito(data.message);
                // Recargar detalles del grupo
                verDetallesAvanzados(grupoNumero);
            } else {
                mostrarError('Error: ' + data.message);
            }
        })
        .catch(error => {
            mostrarError('Error de conexión: ' + error.message);
        });
    });
}

function cambiarLiderGrupo(grupoNumero, nuevoLiderId, nombreNuevoLider) {
    Swal.fire({
        title: '👑 Cambiar Líder del Grupo',
        html: `
            <div class="text-start">
                <p><strong>¿Estás seguro de que quieres hacer a "${nombreNuevoLider}" el nuevo líder del Grupo ${grupoNumero}?</strong></p>
                <hr>
                <div class="alert alert-info mb-0">
                    <i class="fas fa-info-circle"></i> El líder actual dejará de serlo y "${nombreNuevoLider}" asumirá el liderazgo del grupo.
                </div>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#ffc107',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="fas fa-crown"></i> Sí, cambiar líder',
        cancelButtonText: '<i class="fas fa-times"></i> Cancelar'
    }).then((result) => {
        if (!result.isConfirmed) {
            return;
        }

        // Mostrar spinner mientras se procesa
        Swal.fire({
            title: 'Cambiando líder...',
            html: 'Por favor espera',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        fetch('/Home/CambiarLiderGrupo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                grupoNumero: grupoNumero,
                nuevoLiderId: nuevoLiderId
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                Swal.fire({
                    title: '¡Éxito!',
                    text: data.message,
                    icon: 'success',
                    confirmButtonText: 'Aceptar',
                    confirmButtonColor: '#28a745'
                });
                // Recargar detalles del grupo
                verDetallesAvanzados(grupoNumero);
            } else {
                Swal.fire({
                    title: 'Error',
                    text: data.message,
                    icon: 'error',
                    confirmButtonText: 'Aceptar',
                    confirmButtonColor: '#dc3545'
                });
            }
        })
        .catch(error => {
            Swal.fire({
                title: 'Error de conexión',
                text: 'No se pudo conectar con el servidor: ' + error.message,
                icon: 'error',
                confirmButtonText: 'Aceptar',
                confirmButtonColor: '#dc3545'
            });
        });
    });
}



// Función para exportar grupos a Excel
function exportarGrupos() {
    // Mostrar notificación de procesamiento
    const notificacionProceso = document.createElement('div');
    notificacionProceso.className = 'alert alert-info alert-dismissible fade show notification-toast';
    notificacionProceso.id = 'exportNotificationGrupos';
    notificacionProceso.innerHTML = `
        <i class="fas fa-spinner fa-spin"></i>
        Generando archivo Excel... Por favor espere.
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(notificacionProceso);
    
    // Crear un enlace temporal para la descarga
    const link = document.createElement('a');
    link.href = '/Home/ExportarGruposExcel';
    link.download = '';
    link.style.display = 'none';
    document.body.appendChild(link);
    
    // Simular click en el enlace
    link.click();
    
    // Limpiar el enlace temporal
    document.body.removeChild(link);
    
    // Remover notificación de procesamiento después de 3 segundos
    setTimeout(() => {
        const notification = document.getElementById('exportNotificationGrupos');
        if (notification && notification.parentNode) {
            notification.remove();
            // Mostrar notificación de éxito
            if (typeof mostrarNotificacion === 'function') {
                mostrarNotificacion('Archivo Excel de grupos generado exitosamente. La descarga debería iniciar automáticamente.', 'success');
            } else {
                mostrarExito('Archivo Excel de grupos generado exitosamente');
            }
        }
    }, 3000);
}

// Hacer funciones disponibles globalmente
window.verDetallesAvanzados = verDetallesAvanzados;
window.eliminarEstudianteDeGrupo = eliminarEstudianteDeGrupo;
window.cambiarLiderGrupo = cambiarLiderGrupo;
window.cerrarModalGestionEstudiantes = cerrarModalGestionEstudiantes;
window.exportarGrupos = exportarGrupos;

console.log('Funciones avanzadas registradas globalmente:', {
    verDetallesAvanzados: typeof window.verDetallesAvanzados,
    eliminarEstudianteDeGrupo: typeof window.eliminarEstudianteDeGrupo,
    cambiarLiderGrupo: typeof window.cambiarLiderGrupo,
    cerrarModalGestionEstudiantes: typeof window.cerrarModalGestionEstudiantes
});
