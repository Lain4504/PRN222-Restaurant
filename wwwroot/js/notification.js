// Initialize SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Start the connection
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        
        // Join appropriate groups based on user role
        const userRole = document.body.dataset.userRole;
        if (userRole === 'staff') {
            await connection.invoke("JoinGroup", "staff");
        } else if (userRole === 'kitchen') {
            await connection.invoke("JoinGroup", "kitchen");
        }
    } catch (err) {
        console.log(err);
        setTimeout(startConnection, 5000);
    }
}

connection.onclose(async () => {
    await startConnection();
});

// Handle notifications
connection.on("ReceiveNotification", (user, message) => {
    showNotification('info', message);
});

connection.on("ReceiveOrderUpdate", (orderId, status, message) => {
    showNotification('info', message);
    updateOrderStatus(orderId, status);
});

connection.on("ReceiveReservationUpdate", (reservationId, status, message) => {
    showNotification('info', message);
    updateReservationStatus(reservationId, status);
});

connection.on("ReceivePrivateNotification", (message) => {
    showNotification('info', message);
});

connection.on("ReceiveGroupNotification", (message) => {
    showNotification('info', message);
});

// Helper functions
function showNotification(type, message) {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} shadow-lg max-w-sm`;
    notification.innerHTML = `
        <div class="flex items-center">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="stroke-info shrink-0 w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <span>${message}</span>
        </div>
    `;

    // Add to notification container
    const container = document.getElementById('notification-container');
    if (!container) {
        // Create container if it doesn't exist
        const newContainer = document.createElement('div');
        newContainer.id = 'notification-container';
        newContainer.className = 'toast toast-end';
        document.body.appendChild(newContainer);
        container = newContainer;
    }

    container.appendChild(notification);

    // Remove after 5 seconds
    setTimeout(() => {
        notification.remove();
    }, 5000);
}

function updateOrderStatus(orderId, status) {
    const orderElement = document.querySelector(`[data-order-id="${orderId}"]`);
    if (orderElement) {
        const statusElement = orderElement.querySelector('.order-status');
        if (statusElement) {
            statusElement.textContent = status;
            statusElement.className = `order-status status-${status.toLowerCase()}`;
        }
    }
}

function updateReservationStatus(reservationId, status) {
    const reservationElement = document.querySelector(`[data-reservation-id="${reservationId}"]`);
    if (reservationElement) {
        const statusElement = reservationElement.querySelector('.reservation-status');
        if (statusElement) {
            statusElement.textContent = status;
            statusElement.className = `reservation-status status-${status.toLowerCase()}`;
        }
    }
}

// Start the connection when the document is ready
document.addEventListener('DOMContentLoaded', startConnection); 