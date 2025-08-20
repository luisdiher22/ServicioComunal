// Variables globales
let esEdicion = false;

// Función para limpiar el formulario
function limpiarFormulario() {
    document.getElementById('formFormulario').reset();
    document.getElementById('formularioId').value = '';
    document.getElementById('modalFormularioLabel').textContent = 'Nuevo Formulario';
    
    // Limpiar información del archivo actual si existe
    const infoArchivo = document.getElementById('infoArchivoActual');
    if (infoArchivo) {
        infoArchivo.remove();
    }
    
    esEdicion = false;
}

// Función para guardar formulario
async function guardarFormulario() {
    const form = document.getElementById('formFormulario');
    const formData = new FormData(form);
    
    // Validaciones
    const nombre = formData.get('nombre');
    const descripcion = formData.get('descripcion');
    
    if (!nombre || !descripcion) {
        mostrarAlerta('Todos los campos marcados con * son obligatorios', 'warning');
        return;
    }

    try {
        let url, method;
        
        if (esEdicion) {
            url = '/Home/ActualizarFormularioConArchivo';
            method = 'POST';
            
            // Agregar flag para mantener archivo existente si no se selecciona uno nuevo
            const archivoInput = document.getElementById('formularioArchivoInput');
            if (!archivoInput.files || archivoInput.files.length === 0) {
                formData.append('mantenerArchivo', 'true');
            }
        } else {
            url = '/Home/CrearFormularioConArchivo';
            method = 'POST';
        }

        const response = await fetch(url, {
            method: method,
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            mostrarAlerta(result.message, 'success');
            // Cerrar modal y recargar página
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalFormulario'));
            modal.hide();
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            mostrarAlerta(result.message, 'danger');
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarAlerta('Error al guardar el formulario', 'danger');
    }
}

// Función para editar formulario
async function editarFormulario(id) {
    try {
        const response = await fetch(`/Home/ObtenerFormulario/${id}`);
        const result = await response.json();

        if (result.success) {
            const formulario = result.formulario;
            
            document.getElementById('formularioId').value = formulario.identificacion;
            document.getElementById('formularioNombre').value = formulario.nombre;
            document.getElementById('formularioDescripcion').value = formulario.descripcion;
            document.getElementById('formularioArchivo').value = formulario.archivoRuta || '';
            
            // Mostrar información del archivo actual si existe
            const archivoInput = document.getElementById('formularioArchivoInput');
            const infoArchivo = document.getElementById('infoArchivoActual');
            
            if (infoArchivo) {
                infoArchivo.remove();
            }
            
            if (formulario.archivoRuta) {
                const infoDiv = document.createElement('div');
                infoDiv.id = 'infoArchivoActual';
                infoDiv.className = 'alert alert-info mt-2';
                infoDiv.innerHTML = `
                    <i class="fas fa-file"></i> 
                    <strong>Archivo actual:</strong> ${formulario.archivoRuta.split('/').pop()}
                    <br><small>Seleccione un nuevo archivo para reemplazarlo, o deje vacío para mantener el actual.</small>
                `;
                archivoInput.parentNode.appendChild(infoDiv);
            }
            
            // Limpiar el input de archivo
            archivoInput.value = '';
            
            document.getElementById('modalFormularioLabel').textContent = 'Editar Formulario';
            esEdicion = true;
            
            const modal = new bootstrap.Modal(document.getElementById('modalFormulario'));
            modal.show();
        } else {
            mostrarAlerta(result.message, 'danger');
        }
    } catch (error) {
        console.error('Error:', error);
        mostrarAlerta('Error al cargar el formulario', 'danger');
    }
}

// Función para eliminar formulario
async function eliminarFormulario(id) {
    if (!confirm('¿Está seguro de que desea eliminar este formulario?')) {
        return;
    }

    try {
        const response = await fetch(`/Home/EliminarFormulario/${id}`, {
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
        mostrarAlerta('Error al eliminar el formulario', 'danger');
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
    document.getElementById('modalFormulario').addEventListener('show.bs.modal', function () {
        if (!esEdicion) {
            limpiarFormulario();
        }
    });
    
    // Event listener para limpiar al cerrar modal
    document.getElementById('modalFormulario').addEventListener('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    // Event listener para validación de archivo
    document.getElementById('formularioArchivoInput').addEventListener('change', function(e) {
        const archivo = e.target.files[0];
        if (archivo) {
            // Validar extensión
            const extensionesPermitidas = ['.pdf', '.docx', '.doc'];
            const extension = '.' + archivo.name.split('.').pop().toLowerCase();
            
            if (!extensionesPermitidas.includes(extension)) {
                mostrarAlerta('Solo se permiten archivos PDF, DOC o DOCX', 'warning');
                e.target.value = '';
                return;
            }
            
            // Validar tamaño (10MB)
            if (archivo.size > 10 * 1024 * 1024) {
                mostrarAlerta('El archivo no puede ser mayor a 10MB', 'warning');
                e.target.value = '';
                return;
            }
            
            // Mostrar información del archivo seleccionado
            const infoDiv = document.createElement('div');
            infoDiv.className = 'mt-2 text-success';
            infoDiv.innerHTML = `<i class="fas fa-check-circle"></i> ${archivo.name} (${(archivo.size / 1024 / 1024).toFixed(2)} MB)`;
            
            // Remover info anterior si existe
            const infoAnterior = e.target.parentNode.querySelector('.text-success');
            if (infoAnterior) {
                infoAnterior.remove();
            }
            
            e.target.parentNode.appendChild(infoDiv);
        }
    });

    // Event listener para tecla Enter en campos del formulario
    const inputs = document.querySelectorAll('#formFormulario input:not([type="file"]), #formFormulario textarea, #formFormulario select');
    inputs.forEach(input => {
        input.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA') {
                e.preventDefault();
                guardarFormulario();
            }
        });
    });
});
