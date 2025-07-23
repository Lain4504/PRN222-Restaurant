// Payment method handling
let currentPaymentMethod = 'vnpay';

function setPaymentMethod(method) {
    currentPaymentMethod = method;
    
    // Hide all payment info sections
    document.getElementById('card-payment-info').classList.add('hidden');
    document.getElementById('cash-payment-info').classList.add('hidden');
    document.getElementById('vnpay-payment-info').classList.add('hidden');
    
    // Show selected payment info section
    document.getElementById(`${method}-payment-info`).classList.remove('hidden');
    
    // Update tab states
    document.getElementById('card-tab').classList.remove('tab-active');
    document.getElementById('cash-tab').classList.remove('tab-active');
    document.getElementById('vnpay-tab').classList.remove('tab-active');
    document.getElementById(`${method}-tab`).classList.add('tab-active');
}

async function handleCheckout() {
    event.preventDefault();

    // Get cart items from localStorage
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    if (cart.length === 0) {
        showToast('error', 'Your cart is empty');
        return;
    }

    // Get form data
    const form = document.getElementById('checkout-form');
    const formData = new FormData(form);
    
    // Calculate total
    const subtotal = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const total = subtotal; // No tax or delivery fee

    // Prepare payment data
    const paymentData = {
        amount: total,
        orderType: currentPaymentMethod === 'vnpay' ? 'vnpay' : 'other',
        orderDescription: `Order for ${formData.get('firstName')} ${formData.get('lastName')}`,
        name: `${formData.get('firstName')} ${formData.get('lastName')}`,
        orderId: Date.now().toString()
    };

    try {
        if (currentPaymentMethod === 'vnpay') {
            // Submit form for VNPay payment
            const response = await fetch('/checkout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(paymentData)
            });

            if (response.ok) {
                const paymentUrl = await response.text();
                window.location.href = paymentUrl;
            } else {
                showToast('error', 'Failed to process payment');
            }
        } else {
            // Handle other payment methods
            // ... existing code for other payment methods ...
        }
    } catch (error) {
        console.error('Payment error:', error);
        showToast('error', 'An error occurred while processing your payment');
    }
}

// Toast notification helper
function showToast(type, message) {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} mb-2`;
    toast.innerHTML = `
        <div class="flex items-center gap-2">
            ${type === 'error' ? `
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
            ` : `
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
            `}
            <span>${message}</span>
        </div>
    `;
    
    const container = document.getElementById('toast-container');
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 5000);
} 