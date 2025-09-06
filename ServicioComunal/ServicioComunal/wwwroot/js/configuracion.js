// Variables globales
let isEditing = false;
let currentEditingId = 0;
let currentEditingType = '';

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
