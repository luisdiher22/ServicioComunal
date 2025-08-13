// Variables globales
let isEditing = false;
let currentEditingId = 0;
let currentEditingType = '';

// Funciones para mostrar/ocultar secciones
function mostrarSeccionFormularios() {
    ocultarTodasLasSecciones();
    document.getElementById('seccionFormularios').style.display = 'block';
    // Scroll suave a la sección
    document.getElementById('seccionFormularios').scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start' 
    });
}

function mostrarSeccionEntregas() {
    ocultarTodasLasSecciones();
    document.getElementById('seccionEntregas').style.display = 'block';
    // Scroll suave a la sección
    document.getElementById('seccionEntregas').scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start' 
    });
}

function ocultarSeccion(seccionId) {
    document.getElementById(seccionId).style.display = 'none';
}

function ocultarTodasLasSecciones() {
    document.getElementById('seccionFormularios').style.display = 'none';
    document.getElementById('seccionEntregas').style.display = 'none';
}

// Función para guardar configuración general
function guardarConfiguracion() {
    const maxIntegrantes = document.getElementById('maxIntegrantes').value;
    
    if (!maxIntegrantes || maxIntegrantes < 1) {
        showNotification('error', 'El número máximo de integrantes debe ser mayor a 0');
        return;
    }
    
    // Aquí podrías hacer una llamada AJAX para guardar en la base de datos
    // Por ahora, solo mostramos una notificación
    showNotification('success', `Configuración guardada: Máximo ${maxIntegrantes} integrantes por grupo`);
}

// Funciones para gestión de formularios
function mostrarModalFormulario(editar = false, id = 0) {
    isEditing = editar;
    currentEditingId = id;
    currentEditingType = 'formulario';
    
    const modal = new bootstrap.Modal(document.getElementById('modalFormulario'));
    const titulo = document.getElementById('modalFormularioTitulo');
    
    if (editar) {
        titulo.textContent = 'Editar Formulario';
        cargarDatosFormulario(id);
    } else {
        titulo.textContent = 'Nuevo Formulario';
        limpiarFormularioForm();
    }
    
    modal.show();
}

function cargarDatosFormulario(id) {
    fetch(`/Home/ObtenerFormulario/${id}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const formulario = data.formulario;
                document.getElementById('formularioId').value = formulario.identificacion;
                document.getElementById('formularioNombre').value = formulario.nombre;
                document.getElementById('formularioDescripcion').value = formulario.descripcion;
                document.getElementById('formularioArchivo').value = formulario.archivoRuta || '';
                document.getElementById('formularioProfesor').value = formulario.profesorIdentificacion || '';
            } else {
                showNotification('error', data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('error', 'Error al cargar los datos del formulario');
        });
}

function limpiarFormularioForm() {
    document.getElementById('formularioId').value = '0';
    document.getElementById('formularioNombre').value = '';
    document.getElementById('formularioDescripcion').value = '';
    document.getElementById('formularioArchivo').value = '';
    document.getElementById('formularioProfesor').value = '';
}

function guardarFormulario() {
    const id = document.getElementById('formularioId').value;
    const nombre = document.getElementById('formularioNombre').value.trim();
    const descripcion = document.getElementById('formularioDescripcion').value.trim();
    const archivo = document.getElementById('formularioArchivo').value.trim();
    const profesorId = document.getElementById('formularioProfesor').value;
    
    if (!nombre || !descripcion) {
        showNotification('error', 'Nombre y descripción son requeridos');
        return;
    }
    
    const formulario = {
        identificacion: parseInt(id) || 0,
        nombre: nombre,
        descripcion: descripcion,
        archivoRuta: archivo,
        profesorIdentificacion: parseInt(profesorId) || 0
    };
    
    const url = isEditing ? '/Home/ActualizarFormulario' : '/Home/CrearFormulario';
    const method = isEditing ? 'PUT' : 'POST';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(formulario)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('success', data.message);
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalFormulario'));
            modal.hide();
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'Error al guardar el formulario');
    });
}

function editarFormulario(id) {
    mostrarModalFormulario(true, id);
}

function eliminarFormulario(id) {
    if (!confirm('¿Está seguro de que desea eliminar este formulario?')) {
        return;
    }
    
    fetch(`/Home/EliminarFormulario/${id}`, {
        method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('success', data.message);
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'Error al eliminar el formulario');
    });
}

// Funciones para gestión de entregas
function mostrarModalEntrega(editar = false, id = 0) {
    isEditing = editar;
    currentEditingId = id;
    currentEditingType = 'entrega';
    
    const modal = new bootstrap.Modal(document.getElementById('modalEntrega'));
    const titulo = document.getElementById('modalEntregaTitulo');
    
    if (editar) {
        titulo.textContent = 'Editar Entrega';
        cargarDatosEntrega(id);
    } else {
        titulo.textContent = 'Nueva Entrega';
        limpiarEntregaForm();
    }
    
    modal.show();
}

function cargarDatosEntrega(id) {
    fetch(`/Home/ObtenerEntrega/${id}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const entrega = data.entrega;
                document.getElementById('entregaId').value = entrega.identificacion;
                document.getElementById('entregaNombre').value = entrega.nombre;
                document.getElementById('entregaDescripcion').value = entrega.descripcion;
                document.getElementById('entregaArchivo').value = entrega.archivoRuta || '';
                document.getElementById('entregaRetroalimentacion').value = entrega.retroalimentacion || '';
                document.getElementById('entregaGrupo').value = entrega.grupoNumero || '';
                document.getElementById('entregaProfesor').value = entrega.profesorIdentificacion || '';
                
                // Formatear fecha para input date
                if (entrega.fechaLimite) {
                    const fecha = new Date(entrega.fechaLimite);
                    const fechaFormateada = fecha.toISOString().split('T')[0];
                    document.getElementById('entregaFechaLimite').value = fechaFormateada;
                }
            } else {
                showNotification('error', data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('error', 'Error al cargar los datos de la entrega');
        });
}

function limpiarEntregaForm() {
    document.getElementById('entregaId').value = '0';
    document.getElementById('entregaNombre').value = '';
    document.getElementById('entregaDescripcion').value = '';
    document.getElementById('entregaArchivo').value = '';
    document.getElementById('entregaRetroalimentacion').value = '';
    document.getElementById('entregaGrupo').value = '';
    document.getElementById('entregaProfesor').value = '';
    document.getElementById('entregaFechaLimite').value = '';
}

function guardarEntrega() {
    const id = document.getElementById('entregaId').value;
    const nombre = document.getElementById('entregaNombre').value.trim();
    const descripcion = document.getElementById('entregaDescripcion').value.trim();
    const archivo = document.getElementById('entregaArchivo').value.trim();
    const retroalimentacion = document.getElementById('entregaRetroalimentacion').value.trim();
    const grupoNumero = document.getElementById('entregaGrupo').value;
    const profesorId = document.getElementById('entregaProfesor').value;
    const fechaLimite = document.getElementById('entregaFechaLimite').value;
    
    if (!nombre || !descripcion || !fechaLimite || !grupoNumero || !profesorId) {
        showNotification('error', 'Todos los campos marcados son requeridos');
        return;
    }
    
    const entrega = {
        identificacion: parseInt(id) || 0,
        nombre: nombre,
        descripcion: descripcion,
        archivoRuta: archivo,
        fechaLimite: fechaLimite,
        retroalimentacion: retroalimentacion,
        grupoNumero: parseInt(grupoNumero),
        profesorIdentificacion: parseInt(profesorId),
        fechaRetroalimentacion: new Date().toISOString()
    };
    
    const url = isEditing ? '/Home/ActualizarEntrega' : '/Home/CrearEntrega';
    const method = isEditing ? 'PUT' : 'POST';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(entrega)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('success', data.message);
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalEntrega'));
            modal.hide();
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'Error al guardar la entrega');
    });
}

function editarEntrega(id) {
    mostrarModalEntrega(true, id);
}

function eliminarEntrega(id) {
    if (!confirm('¿Está seguro de que desea eliminar esta entrega?')) {
        return;
    }
    
    fetch(`/Home/EliminarEntrega/${id}`, {
        method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('success', data.message);
            // Recargar la página para mostrar los cambios
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showNotification('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('error', 'Error al eliminar la entrega');
    });
}

// Función para mostrar notificaciones
function showNotification(type, message) {
    // Crear elemento de notificación
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show notification-toast`;
    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Agregar al body
    document.body.appendChild(notification);
    
    // Auto-remover después de 5 segundos
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Inicialización cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    console.log('Página de configuración cargada');
});
