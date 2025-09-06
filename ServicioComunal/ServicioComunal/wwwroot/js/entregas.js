// Variables globales
let esEdicion = false;

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
        console.log('Mostrando sección anexo');
    } else {
        seccionAnexo.style.display = 'none';
        // Limpiar anexo seleccionado
        document.getElementById('entregaAnexo').value = '';
        console.log('Ocultando sección anexo');
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
    
    // Restablecer tipo de recurso a anexo
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
            mostrarAlerta('Todos los campos marcados con * son obligatorios', 'warning');
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
                mostrarAlerta(result.message, 'success');
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEntrega'));
                modal.hide();
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                mostrarAlerta(result.message, 'danger');
            }
        } catch (error) {
            console.error('Error:', error);
            mostrarAlerta('Error al actualizar la entrega', 'danger');
        }
    } else {
        // Modo creación - crear entrega para todos los grupos o grupo específico
        const destinatarioSeleccionado = document.querySelector('input[name="destinatarios"]:checked');
        const enviarATodos = destinatarioSeleccionado ? destinatarioSeleccionado.value === 'todos' : true;
        const grupoEspecifico = enviarATodos ? null : parseInt(document.getElementById('entregaGrupoEspecifico').value);
        
        // Validar grupo específico si se seleccionó esa opción
        if (!enviarATodos && !grupoEspecifico) {
            mostrarAlerta('Debes seleccionar un grupo específico', 'warning');
            return;
        }
        
        const entregaDto = {
            nombre: formData.get('nombre'),
            descripcion: formData.get('descripcion'),
            fechaLimite: formData.get('fechaLimite'),
            formularioIdentificacion: null, // Ya no se usan formularios
            tipoAnexo: formData.get('tipoAnexo') ? parseInt(formData.get('tipoAnexo')) : null,
            enviarATodosLosGrupos: enviarATodos,
            grupoEspecifico: grupoEspecifico
        };

        // Validaciones
        if (!entregaDto.nombre || !entregaDto.descripcion || !entregaDto.fechaLimite) {
            mostrarAlerta('Todos los campos marcados con * son obligatorios', 'warning');
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
                mostrarAlerta(result.message, 'success');
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalEntrega'));
                modal.hide();
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                mostrarAlerta(result.message, 'danger');
            }
        } catch (error) {
            console.error('Error:', error);
            mostrarAlerta('Error al crear la entrega', 'danger');
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
                estadoHtml = '<span class="badge badge-info"><i class="fas fa-upload"></i> Entregada</span>';
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
            mostrarAlerta(result.message, 'danger');
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarAlerta('Error al cargar los detalles de la entrega', 'danger');
    }
}

// Función para editar entrega
async function editarEntrega(id) {
    try {
        const response = await fetch(`/Home/ObtenerEntrega/${id}`);
        const result = await response.json();

        if (result.success) {
            const entrega = result.entrega;
            
            document.getElementById('entregaId').value = entrega.identificacion;
            document.getElementById('entregaNombre').value = entrega.nombre;
            document.getElementById('entregaDescripcion').value = entrega.descripcion;
            
            // Convertir fecha al formato datetime-local
            const fechaLimite = new Date(entrega.fechaLimite);
            const fechaLocal = new Date(fechaLimite.getTime() - fechaLimite.getTimezoneOffset() * 60000);
            document.getElementById('entregaFechaLimite').value = fechaLocal.toISOString().slice(0, 16);
            
            document.getElementById('entregaGrupo').value = entrega.grupoNumero;
            
            // Establecer el formulario seleccionado si existe
            if (entrega.formularioIdentificacion) {
                document.getElementById('entregaFormulario').value = entrega.formularioIdentificacion;
            } else {
                document.getElementById('entregaFormulario').value = '';
            }
            
            document.getElementById('modalEntregaLabel').textContent = 'Editar Entrega';
            document.getElementById('modoEdicion').style.display = 'block';
            document.getElementById('seccionDestinatarios').style.display = 'none';
            document.getElementById('infoNuevaEntrega').style.display = 'none';
            esEdicion = true;
            
            const modal = new bootstrap.Modal(document.getElementById('modalEntrega'));
            modal.show();
        } else {
            mostrarAlerta(result.message, 'danger');
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarAlerta('Error al cargar la entrega', 'danger');
    }
}

// Función para eliminar entrega
async function eliminarEntrega(id) {
    if (!confirm('¿Está seguro de que desea eliminar esta entrega?')) {
        return;
    }

    try {
        const response = await fetch(`/Home/EliminarEntrega/${id}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (result.success) {
            mostrarAlerta(result.message, 'success');
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            mostrarAlerta(result.message, 'danger');
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarAlerta('Error al eliminar la entrega', 'danger');
    }
}

// Función para mostrar alertas
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
