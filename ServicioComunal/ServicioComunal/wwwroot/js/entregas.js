// Variables globales
let esEdicion = false;

// Función para limpiar el formulario
function limpiarFormulario() {
    document.getElementById('formEntrega').reset();
    document.getElementById('entregaId').value = '';
    document.getElementById('modalEntregaLabel').textContent = 'Nueva Entrega';
    document.getElementById('modoEdicion').style.display = 'none';
    document.getElementById('infoNuevaEntrega').style.display = 'block';
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
        // Modo creación - crear entrega para todos los grupos
        const entregaDto = {
            nombre: formData.get('nombre'),
            descripcion: formData.get('descripcion'),
            fechaLimite: formData.get('fechaLimite'),
            formularioIdentificacion: formData.get('formularioIdentificacion') ? parseInt(formData.get('formularioIdentificacion')) : null
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
});
