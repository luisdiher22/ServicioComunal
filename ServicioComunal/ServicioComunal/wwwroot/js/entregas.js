// Variables globales
let esEdicion = false;

// Función para cargar formularios disponibles
async function cargarFormularios() {
    try {
        const response = await fetch('/Home/ObtenerFormulariosDisponibles');
        const data = await response.json();
        
        if (data.success) {
            const select = document.getElementById('entregaAnexo');
            // Limpiar opciones existentes excepto la primera
            while (select.children.length > 1) {
                select.removeChild(select.lastChild);
            }
            
            // Agregar formularios disponibles
            data.formularios.forEach(formulario => {
                const option = document.createElement('option');
                option.value = formulario.id;
                option.textContent = `${formulario.nombre} - ${formulario.descripcion}`;
                select.appendChild(option);
            });
        } else {
            console.error('Error al cargar formularios:', data.message);
        }
    } catch (error) {
        console.error('Error al cargar formularios:', error);
    }
}

// Función para mostrar/ocultar secciones según el tipo de recurso seleccionado
function mostrarSeccionTipoRecurso() {
    const tipoSeleccionado = document.querySelector('input[name="tipoRecurso"]:checked');
    console.log('Tipo seleccionado:', tipoSeleccionado ? tipoSeleccionado.value : 'ninguno');
    
    const seccionAnexo = document.getElementById('seccionAnexo');
    
    if (!tipoSeleccionado) {
        console.log('No hay tipo seleccionado');
        return;
    }
    
    const valor = tipoSeleccionado.value;
    
    if (valor === 'anexo') {
        seccionAnexo.style.display = 'block';
        console.log('Mostrando sección formulario');
    } else {
        seccionAnexo.style.display = 'none';
        // Limpiar formulario seleccionado
        document.getElementById('entregaAnexo').value = '';
        console.log('Ocultando sección formulario');
    }
}

// Función para mostrar/ocultar la sección de grupo específico
function mostrarSeccionGrupoEspecifico() {
    const destinatarioSeleccionado = document.querySelector('input[name="destinatarios"]:checked');
    const seccionGrupoEspecifico = document.getElementById('seccionGrupoEspecifico');
    const textoInfo = document.getElementById('textoInfoEntrega');
    
    if (!destinatarioSeleccionado) {
        return;
    }
    
    if (destinatarioSeleccionado.value === 'especifico') {
        seccionGrupoEspecifico.style.display = 'block';
        textoInfo.textContent = 'Al crear una nueva entrega, se generará una tarea únicamente para el grupo seleccionado.';
    } else {
        seccionGrupoEspecifico.style.display = 'none';
        document.getElementById('entregaGrupoEspecifico').value = '';
        textoInfo.textContent = 'Al crear una nueva entrega, se generará automáticamente una tarea para cada grupo existente.';
    }
}

// Función para limpiar el formulario
function limpiarFormulario() {
    document.getElementById('formEntrega').reset();
    document.getElementById('entregaId').value = '';
    document.getElementById('modalEntregaLabel').textContent = 'Nueva Entrega';
    document.getElementById('modoEdicion').style.display = 'none';
    document.getElementById('seccionDestinatarios').style.display = 'block';
    document.getElementById('infoNuevaEntrega').style.display = 'block';
    
    // Restablecer tipo de recurso a formulario
    document.getElementById('tipoAnexo').checked = true;
    
    // Restablecer destinatarios a todos los grupos
    document.getElementById('todosLosGrupos').checked = true;
    
    // Asegurar que las secciones se muestren correctamente
    setTimeout(() => {
        mostrarSeccionTipoRecurso();
        mostrarSeccionGrupoEspecifico();
    }, 100);
    
    esEdicion = false;
}

// Función para guardar entrega
async function guardarEntrega() {
    const form = document.getElementById('formEntrega');
    const formData = new FormData(form);
    
    if (esEdicion) {
        // Modo edición - actualizar entrega específica
        const entrega = {
            identificacion: parseInt(formData.get('identificacion')) || 0,
            nombre: formData.get('nombre'),
            descripcion: formData.get('descripcion'),
            fechaLimite: formData.get('fechaLimite'),
            grupoNumero: parseInt(formData.get('grupoNumero')) || 0,
            formularioIdentificacion: formData.get('formularioIdentificacion') ? parseInt(formData.get('formularioIdentificacion')) : null,
            archivoRuta: "", // El administrador no maneja archivos
            retroalimentacion: "" // El administrador no da retroalimentación inicial
        };

        // Validaciones
        if (!entrega.nombre || !entrega.descripcion || !entrega.fechaLimite) {
            mostrarAdvertencia('Todos los campos marcados con * son obligatorios');
            return;
        }

        try {
            const response = await fetch('/Home/ActualizarEntrega', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(entrega)
            });

            const result = await response.json();

            if (result.success) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEntrega'));
                modal.hide();
                
                Swal.fire({
                    icon: 'success',
                    title: '¡Éxito!',
                    text: result.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(() => {
                    window.location.reload();
                });
            } else {
                mostrarError(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            mostrarError('Error al actualizar la entrega');
        }
    } else {
        // Modo creación - crear entrega para todos los grupos o grupo específico
        const destinatarioSeleccionado = document.querySelector('input[name="destinatarios"]:checked');
        const enviarATodos = destinatarioSeleccionado ? destinatarioSeleccionado.value === 'todos' : true;
        const grupoEspecifico = enviarATodos ? null : parseInt(document.getElementById('entregaGrupoEspecifico').value);
        
        // Validar grupo específico si se seleccionó esa opción
        if (!enviarATodos && !grupoEspecifico) {
            mostrarAdvertencia('Debes seleccionar un grupo específico');
            return;
        }
        
        const entregaDto = {
            nombre: formData.get('nombre'),
            descripcion: formData.get('descripcion'),
            fechaLimite: formData.get('fechaLimite'),
            formularioIdentificacion: formData.get('tipoAnexo') ? parseInt(formData.get('tipoAnexo')) : null,
            tipoAnexo: null, // No usar TipoAnexo para formularios
            enviarATodosLosGrupos: enviarATodos,
            grupoEspecifico: grupoEspecifico
        };

        // Validaciones
        if (!entregaDto.nombre || !entregaDto.descripcion || !entregaDto.fechaLimite) {
            mostrarAdvertencia('Todos los campos marcados con * son obligatorios');
            return;
        }

        try {
            const response = await fetch('/Home/CrearEntrega', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(entregaDto)
            });

            const result = await response.json();

            if (result.success) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEntrega'));
                modal.hide();
                
                Swal.fire({
                    icon: 'success',
                    title: '¡Éxito!',
                    text: result.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(() => {
                    window.location.reload();
                });
            } else {
                mostrarError(result.message);
            }
        } catch (error) {
            console.error('Error:', error);
            mostrarError('Error al crear la entrega');
        }
    }
}

// Función para ver detalles de entrega
async function verEntrega(id) {
    try {
        const response = await fetch(`/Home/ObtenerEntrega/${id}`);
        const result = await response.json();

        if (result.success) {
            const entrega = result.entrega;
            
            const fechaVencida = new Date(entrega.fechaLimite) < new Date();
            const tieneArchivo = entrega.archivoRuta && entrega.archivoRuta.trim() !== '';
            const tieneRetroalimentacion = entrega.retroalimentacion && entrega.retroalimentacion.trim() !== '';
            
            let estadoHtml = '';
            if (tieneArchivo && tieneRetroalimentacion) {
                estadoHtml = '<span class="badge badge-success"><i class="fas fa-check"></i> Completada</span>';
            } else if (tieneArchivo) {
                estadoHtml = '<span class="badge badge-info"><i class="fas fa-upload"></i> Pendiente revisión tutor</span>';
            } else {
                estadoHtml = '<span class="badge badge-secondary"><i class="fas fa-clock"></i> Pendiente</span>';
            }

            document.getElementById('detallesEntrega').innerHTML = `
                <div class="row">
                    <div class="col-md-6">
                        <h6><strong>Información General</strong></h6>
                        <p><strong>ID:</strong> ${entrega.identificacion}</p>
                        <p><strong>Nombre:</strong> ${entrega.nombre}</p>
                        <p><strong>Grupo:</strong> Grupo ${entrega.grupoNumero}</p>
                        <p><strong>Estado:</strong> ${estadoHtml}</p>
                        ${entrega.formularioNombre ? `<p><strong>Formulario Asociado:</strong> ${entrega.formularioNombre}</p>` : ''}
                    </div>
                    <div class="col-md-6">
                        <h6><strong>Fechas</strong></h6>
                        <p><strong>Fecha Límite:</strong> ${new Date(entrega.fechaLimite).toLocaleString()}</p>
                        ${fechaVencida ? '<p class="text-danger"><i class="fas fa-exclamation-triangle"></i> <strong>VENCIDA</strong></p>' : ''}
                        ${entrega.fechaRetroalimentacion && entrega.fechaRetroalimentacion !== '0001-01-01T00:00:00' ? 
                            `<p><strong>Fecha Retroalimentación:</strong> ${new Date(entrega.fechaRetroalimentacion).toLocaleString()}</p>` : ''}
                    </div>
                </div>
                <hr>
                <div class="row">
                    <div class="col-md-12">
                        <h6><strong>Descripción</strong></h6>
                        <p>${entrega.descripcion}</p>
                    </div>
                </div>
                ${entrega.archivoRuta ? `
                <div class="row">
                    <div class="col-md-12">
                        <h6><strong>Archivo Entregado</strong></h6>
                        <p><i class="fas fa-file"></i> ${entrega.archivoRuta}</p>
                    </div>
                </div>
                ` : ''}
                ${entrega.retroalimentacion ? `
                <div class="row">
                    <div class="col-md-12">
                        <h6><strong>Retroalimentación</strong></h6>
                        <div class="alert alert-info">
                            ${entrega.retroalimentacion}
                        </div>
                    </div>
                </div>
                ` : ''}
            `;
            
            const modal = new bootstrap.Modal(document.getElementById('modalVerEntrega'));
            modal.show();
        } else {
            mostrarError(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarError('Error al cargar los detalles de la entrega');
    }
}

// Función para editar entrega
async function editarEntrega(id) {
    console.log('Iniciando edición para entrega ID:', id);
    
    try {
        console.log('Haciendo petición a:', `/Home/ObtenerEntrega/${id}`);
        const response = await fetch(`/Home/ObtenerEntrega/${id}`);
        const result = await response.json();

        console.log('Respuesta del servidor:', result);

        if (result.success) {
            const entrega = result.entrega;
            console.log('Datos de la entrega:', entrega);
            
            // Abrir el modal primero
            console.log('Abriendo modal primero...');
            const modalElement = document.getElementById('modalEntrega');
            if (!modalElement) {
                throw new Error('Elemento modal no encontrado');
            }

            // Configurar el modal
            if (typeof $ !== 'undefined' && $.fn.modal) {
                console.log('Usando jQuery para abrir modal');
                $('#modalEntrega').modal('show');
                
                // Esperar a que el modal se abra completamente
                $('#modalEntrega').on('shown.bs.modal', function () {
                    llenarFormularioEdicion(entrega);
                });
            } else {
                console.log('Usando Bootstrap nativo para abrir modal');
                const modal = new bootstrap.Modal(modalElement);
                modal.show();
                
                // Esperar a que el modal se abra completamente
                modalElement.addEventListener('shown.bs.modal', function () {
                    llenarFormularioEdicion(entrega);
                });
            }
            
        } else {
            console.error('Error del servidor:', result.message);
            mostrarError(result.message);
        }
    } catch (error) {
        console.error('Error completo:', error);
        mostrarError('Error al cargar la entrega: ' + error.message);
    }
}

// Función para llenar el formulario de edición una vez que el modal está completamente abierto
function llenarFormularioEdicion(entrega) {
    console.log('Llenando formulario de edición con:', entrega);
    
    try {
        // Verificar que los elementos existan antes de usarlos
        const elementoId = document.getElementById('entregaId');
        const elementoNombre = document.getElementById('entregaNombre');
        const elementoDescripcion = document.getElementById('entregaDescripcion');
        const elementoFechaLimite = document.getElementById('entregaFechaLimite');
        const elementoGrupo = document.getElementById('entregaGrupo');
        
        console.log('Elementos DOM encontrados en modal abierto:', {
            id: !!elementoId,
            nombre: !!elementoNombre,
            descripcion: !!elementoDescripcion,
            fechaLimite: !!elementoFechaLimite,
            grupo: !!elementoGrupo
        });
        
        if (!elementoId || !elementoNombre || !elementoDescripcion || !elementoFechaLimite || !elementoGrupo) {
            throw new Error('Uno o más elementos del formulario no fueron encontrados después de abrir el modal');
        }
        
        elementoId.value = entrega.identificacion;
        elementoNombre.value = entrega.nombre;
        elementoDescripcion.value = entrega.descripcion;
        
        // Convertir fecha al formato datetime-local
        const fechaLimite = new Date(entrega.fechaLimite);
        const fechaLocal = new Date(fechaLimite.getTime() - fechaLimite.getTimezoneOffset() * 60000);
        elementoFechaLimite.value = fechaLocal.toISOString().slice(0, 16);
        
        elementoGrupo.value = entrega.grupoNumero;
        
        // Configurar elementos del modal
        const modalLabel = document.getElementById('modalEntregaLabel');
        const modoEdicion = document.getElementById('modoEdicion');
        const seccionDestinatarios = document.getElementById('seccionDestinatarios');
        const infoNuevaEntrega = document.getElementById('infoNuevaEntrega');
        
        console.log('Elementos del modal encontrados:', {
            modalLabel: !!modalLabel,
            modoEdicion: !!modoEdicion,
            seccionDestinatarios: !!seccionDestinatarios,
            infoNuevaEntrega: !!infoNuevaEntrega
        });
        
        if (modalLabel) modalLabel.textContent = 'Editar Entrega';
        if (modoEdicion) modoEdicion.style.display = 'block';
        if (seccionDestinatarios) seccionDestinatarios.style.display = 'none';
        if (infoNuevaEntrega) infoNuevaEntrega.style.display = 'none';
        
        esEdicion = true;
        
        console.log('Formulario de edición llenado exitosamente');
        
    } catch (error) {
        console.error('Error al llenar formulario:', error);
        mostrarError('Error al cargar los datos en el formulario: ' + error.message);
    }
}

// Función para eliminar entrega
async function eliminarEntrega(id) {
    const result = await Swal.fire({
        title: '¿Eliminar entrega?',
        text: "Esta acción no se puede deshacer",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    });

    if (!result.isConfirmed) {
        return;
    }

    try {
        const response = await fetch(`/Home/EliminarEntrega/${id}`, {
            method: 'DELETE'
        });

        const responseResult = await response.json();

        if (responseResult.success) {
            Swal.fire({
                icon: 'success',
                title: '¡Eliminado!',
                text: responseResult.message,
                showConfirmButton: false,
                timer: 1500
            }).then(() => {
                window.location.reload();
            });
        } else {
            mostrarError(responseResult.message);
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarError('Error al eliminar la entrega');
    }
}

// Función para filtrar entregas
function filtrarEntregas() {
    const busqueda = document.getElementById('buscarEntrega').value.toLowerCase();
    const filtroEstado = document.getElementById('filtroEstado').value;
    const filtroGrupo = document.getElementById('filtroGrupo').value;
    
    const filas = document.querySelectorAll('.entrega-row');
    
    filas.forEach(fila => {
        const id = fila.getAttribute('data-id');
        const nombre = fila.getAttribute('data-nombre');
        const descripcion = fila.getAttribute('data-descripcion');
        const grupo = fila.getAttribute('data-grupo');
        const estado = fila.getAttribute('data-estado');
        
        let mostrar = true;
        
        // Filtro por búsqueda (ID, nombre o descripción)
        if (busqueda && !id.includes(busqueda) && !nombre.includes(busqueda) && !descripcion.includes(busqueda)) {
            mostrar = false;
        }
        
        // Filtro por estado
        if (filtroEstado && estado !== filtroEstado) {
            mostrar = false;
        }
        
        // Filtro por grupo
        if (filtroGrupo && grupo !== filtroGrupo) {
            mostrar = false;
        }
        
        fila.style.display = mostrar ? '' : 'none';
    });
}

// Función para limpiar filtros
function limpiarFiltros() {
    document.getElementById('buscarEntrega').value = '';
    document.getElementById('filtroEstado').value = '';
    document.getElementById('filtroGrupo').value = '';
    filtrarEntregas();
}

// Funciones de helper para SweetAlert
function mostrarExito(mensaje) {
    Swal.fire({
        icon: 'success',
        title: '¡Éxito!',
        text: mensaje,
        showConfirmButton: false,
        timer: 1500
    });
}

function mostrarError(mensaje) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: mensaje,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#d33'
    });
}

function mostrarAdvertencia(mensaje) {
    Swal.fire({
        icon: 'warning',
        title: 'Atención',
        text: mensaje,
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#f39c12'
    });
}

// Función para mostrar alertas (mantenida para compatibilidad)
function mostrarAlerta(mensaje, tipo) {
    // Crear el elemento de alerta
    const alerta = document.createElement('div');
    alerta.className = `alert alert-${tipo} alert-dismissible fade show`;
    alerta.style.position = 'fixed';
    alerta.style.top = '20px';
    alerta.style.right = '20px';
    alerta.style.zIndex = '9999';
    alerta.style.minWidth = '300px';
    
    alerta.innerHTML = `
        ${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    // Agregar al body
    document.body.appendChild(alerta);
    
    // Auto-remover después de 5 segundos
    setTimeout(() => {
        if (alerta.parentNode) {
            alerta.remove();
        }
    }, 5000);
}

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM cargado, configurando event listeners');
    
    // Cargar formularios disponibles
    cargarFormularios();
    
    // Event listener para limpiar formulario al abrir modal
    document.getElementById('modalEntrega').addEventListener('show.bs.modal', function () {
        if (!esEdicion) {
            limpiarFormulario();
        }
    });
    
    // Event listener para limpiar al cerrar modal
    document.getElementById('modalEntrega').addEventListener('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    // Event listeners para los radio buttons de destinatarios
    const radioButtonsDestinatarios = document.querySelectorAll('input[name="destinatarios"]');
    radioButtonsDestinatarios.forEach(radio => {
        radio.addEventListener('change', function() {
            console.log('Destinatario cambiado a:', this.value);
            mostrarSeccionGrupoEspecifico();
        });
    });

    // Event listeners para los radio buttons de tipo de recurso
    const radioButtons = document.querySelectorAll('input[name="tipoRecurso"]');
    console.log('Radio buttons encontrados:', radioButtons.length);
    
    radioButtons.forEach((radio, index) => {
        console.log(`Configurando radio ${index}: ${radio.value}`);
        radio.addEventListener('change', function() {
            console.log('Radio cambiado a:', this.value);
            mostrarSeccionTipoRecurso();
        });
        
        // También agregar evento click como respaldo
        radio.addEventListener('click', function() {
            console.log('Radio clickeado:', this.value);
            mostrarSeccionTipoRecurso();
        });
    });

    // Event listener para tecla Enter en campos del formulario
    const inputs = document.querySelectorAll('#formEntrega input, #formEntrega textarea, #formEntrega select');
    inputs.forEach(input => {
        input.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA') {
                e.preventDefault();
                guardarEntrega();
            }
        });
    });
    
    // Inicializar la visualización de secciones
    console.log('Inicializando visualización de secciones');
    mostrarSeccionTipoRecurso();
    mostrarSeccionGrupoEspecifico();
});

// Función de prueba para verificar elementos (puedes llamarla desde la consola)
function probarElementosModal() {
    const elementos = {
        modalEntrega: document.getElementById('modalEntrega'),
        entregaId: document.getElementById('entregaId'),
        entregaNombre: document.getElementById('entregaNombre'),
        entregaDescripcion: document.getElementById('entregaDescripcion'),
        entregaFechaLimite: document.getElementById('entregaFechaLimite'),
        entregaGrupo: document.getElementById('entregaGrupo'),
        modalEntregaLabel: document.getElementById('modalEntregaLabel'),
        modoEdicion: document.getElementById('modoEdicion'),
        seccionDestinatarios: document.getElementById('seccionDestinatarios'),
        infoNuevaEntrega: document.getElementById('infoNuevaEntrega')
    };
    
    console.log('Elementos del modal:', elementos);
    
    Object.keys(elementos).forEach(key => {
        if (!elementos[key]) {
            console.error(`Elemento faltante: ${key}`);
        }
    });
    
    return elementos;
}
