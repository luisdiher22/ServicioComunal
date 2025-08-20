// Scripts específicos para el panel de tutor
document.addEventListener('DOMContentLoaded', function() {
    // Inicializar funcionalidades del dashboard de tutor
    initTutorDashboard();
});

function initTutorDashboard() {
    // Aplicar tema de tutor al body
    if (document.querySelector('.tutor-theme')) {
        document.body.classList.add('tutor-theme');
    }

    // Configurar tooltips para avatars
    initAvatarTooltips();

    // Configurar animaciones suaves para las cards
    initCardAnimations();

    // Configurar notificaciones
    initNotifications();
}

function initAvatarTooltips() {
    const avatars = document.querySelectorAll('.avatar, .avatar-mini, .avatar-large');
    avatars.forEach(avatar => {
        if (avatar.hasAttribute('title')) {
            avatar.addEventListener('mouseenter', function(e) {
                showTooltip(e.target, e.target.getAttribute('title'));
            });
            
            avatar.addEventListener('mouseleave', function() {
                hideTooltip();
            });
        }
    });
}

function initCardAnimations() {
    const cards = document.querySelectorAll('.dashboard-card, .grupo-card, .entregable-item');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animation = 'fadeInUp 0.5s ease-out';
            }
        });
    }, {
        threshold: 0.1
    });

    cards.forEach(card => {
        observer.observe(card);
    });
}

function initNotifications() {
    // Simular notificaciones en tiempo real (demo)
    setTimeout(() => {
        showNotification('Nueva entrega recibida de Grupo 2', 'info');
    }, 5000);
}

// Función para mostrar tooltips personalizados
function showTooltip(element, text) {
    const tooltip = document.createElement('div');
    tooltip.className = 'custom-tooltip';
    tooltip.textContent = text;
    
    document.body.appendChild(tooltip);
    
    const rect = element.getBoundingClientRect();
    tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
    tooltip.style.top = rect.top - tooltip.offsetHeight - 8 + 'px';
    
    setTimeout(() => {
        tooltip.classList.add('show');
    }, 10);
}

function hideTooltip() {
    const tooltip = document.querySelector('.custom-tooltip');
    if (tooltip) {
        tooltip.classList.remove('show');
        setTimeout(() => {
            tooltip.remove();
        }, 150);
    }
}

// Función para mostrar notificaciones
function showNotification(message, type = 'info', duration = 5000) {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas ${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
        <button class="notification-close" onclick="closeNotification(this)">
            <i class="fas fa-times"></i>
        </button>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.classList.add('show');
    }, 10);
    
    setTimeout(() => {
        closeNotification(notification.querySelector('.notification-close'));
    }, duration);
}

function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'fa-check-circle';
        case 'error': return 'fa-exclamation-circle';
        case 'warning': return 'fa-exclamation-triangle';
        default: return 'fa-info-circle';
    }
}

function closeNotification(closeButton) {
    const notification = closeButton.closest('.notification');
    notification.classList.remove('show');
    setTimeout(() => {
        notification.remove();
    }, 300);
}

// Funciones específicas para la gestión de grupos
function expandirGrupo(numeroGrupo) {
    const card = document.querySelector(`[data-grupo="${numeroGrupo}"]`);
    if (card) {
        card.classList.toggle('expanded');
    }
}

function filtrarEntregables(estado) {
    const entregables = document.querySelectorAll('.entregable-item');
    entregables.forEach(item => {
        if (estado === 'todos' || item.classList.contains(estado)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// Función para actualizar el progreso en tiempo real
function actualizarProgreso(grupoNumero, nuevoProgreso) {
    const progressBars = document.querySelectorAll(`[data-grupo="${grupoNumero}"] .progress-fill, [data-grupo="${grupoNumero}"] .progress-fill-table`);
    progressBars.forEach(bar => {
        bar.style.width = nuevoProgreso + '%';
    });
    
    const progressTexts = document.querySelectorAll(`[data-grupo="${grupoNumero}"] .progreso-porcentaje`);
    progressTexts.forEach(text => {
        text.textContent = nuevoProgreso + '%';
    });
}

// Función para buscar en las tablas
function buscarEnTabla(input, tabla) {
    const filtro = input.value.toLowerCase();
    const filas = tabla.querySelectorAll('tbody tr');
    
    filas.forEach(fila => {
        const texto = fila.textContent.toLowerCase();
        if (texto.includes(filtro)) {
            fila.style.display = '';
        } else {
            fila.style.display = 'none';
        }
    });
}

// Animaciones CSS adicionales
const additionalStyles = `
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.custom-tooltip {
    position: absolute;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 12px;
    z-index: 10000;
    opacity: 0;
    transform: translateY(-5px);
    transition: all 0.15s ease;
    pointer-events: none;
}

.custom-tooltip.show {
    opacity: 1;
    transform: translateY(0);
}

.custom-tooltip:before {
    content: '';
    position: absolute;
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    border: 4px solid transparent;
    border-top-color: rgba(0, 0, 0, 0.8);
}

.notification {
    position: fixed;
    top: 20px;
    right: 20px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    padding: 16px;
    max-width: 400px;
    z-index: 10000;
    opacity: 0;
    transform: translateX(100%);
    transition: all 0.3s ease;
    border-left: 4px solid #8b7765;
}

.notification.show {
    opacity: 1;
    transform: translateX(0);
}

.notification-content {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-right: 30px;
}

.notification-content i {
    color: #8b7765;
    font-size: 16px;
}

.notification-close {
    position: absolute;
    top: 8px;
    right: 8px;
    background: none;
    border: none;
    color: #6c757d;
    cursor: pointer;
    padding: 4px;
    border-radius: 4px;
    transition: background-color 0.2s ease;
}

.notification-close:hover {
    background-color: #f8f9fa;
}

.notification-success {
    border-left-color: #7a8471;
}

.notification-error {
    border-left-color: #c1666b;
}

.notification-warning {
    border-left-color: #d4a574;
}

.grupo-row.expanded {
    background-color: rgba(139, 119, 101, 0.05);
}

.loading-spinner {
    display: inline-block;
    width: 20px;
    height: 20px;
    border: 2px solid #f3f3f3;
    border-top: 2px solid #8b7765;
    border-radius: 50%;
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}
`;

// Agregar estilos al documento
const styleSheet = document.createElement('style');
styleSheet.textContent = additionalStyles;
document.head.appendChild(styleSheet);
