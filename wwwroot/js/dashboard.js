// Dashboard JavaScript functions

// Show toast notification
function showToast(message, type = 'info', duration = 3000) {
    const toastContainer = document.getElementById('toast-container');
    const toast = document.createElement('div');
    
    // Set alert class based on type
    toast.className = `alert ${type === 'error' ? 'alert-error' : type === 'success' ? 'alert-success' : 'alert-info'} mb-2`;
    
    // Create toast content
    toast.innerHTML = `
        <div>
            <span>${message}</span>
        </div>
    `;
    
    // Add to container
    toastContainer.appendChild(toast);
    
    // Remove after duration
    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => {
            toastContainer.removeChild(toast);
        }, 300);
    }, duration);
}

// Loading overlay for fetch operations
function showLoading() {
    const loadingOverlay = document.createElement('div');
    loadingOverlay.id = 'loading-overlay';
    loadingOverlay.className = 'fixed inset-0 flex items-center justify-center bg-base-300 bg-opacity-50 z-50';
    loadingOverlay.innerHTML = `
        <div class="loading loading-spinner loading-lg text-primary"></div>
    `;
    document.body.appendChild(loadingOverlay);
}

function hideLoading() {
    const loadingOverlay = document.getElementById('loading-overlay');
    if (loadingOverlay) {
        document.body.removeChild(loadingOverlay);
    }
}

// Form validation helper
function validateForm(formId) {
    const form = document.getElementById(formId);
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('input-error');
            const formControl = input.closest('.form-control');
            
            // Add error message if not exist
            if (!formControl.querySelector('.error-message')) {
                const errorMsg = document.createElement('div');
                errorMsg.className = 'text-error text-sm mt-1 error-message';
                errorMsg.innerText = 'This field is required';
                formControl.appendChild(errorMsg);
            }
            
            isValid = false;
        } else {
            input.classList.remove('input-error');
            const formControl = input.closest('.form-control');
            const errorMsg = formControl.querySelector('.error-message');
            if (errorMsg) {
                formControl.removeChild(errorMsg);
            }
        }
    });
    
    return isValid;
}

// Simple search function for tables
function setupTableSearch(tableId, inputId) {
    const searchInput = document.getElementById(inputId);
    const table = document.getElementById(tableId);
    
    if (!searchInput || !table) return;
    
    searchInput.addEventListener('input', function() {
        const searchTerm = this.value.toLowerCase();
        const rows = table.querySelectorAll('tbody tr');
        
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            if (text.includes(searchTerm)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    });
}

// Theme toggle function
function initThemeToggle() {
    const themeToggleBtn = document.getElementById('theme-toggle');
    const htmlElement = document.documentElement;
    const lightIcon = document.querySelector('.theme-light-icon');
    const darkIcon = document.querySelector('.theme-dark-icon');
    
    // Check for saved theme preference or use system preference
    const savedTheme = localStorage.getItem('theme');
    
    // Set initial theme
    if (savedTheme) {
        htmlElement.setAttribute('data-theme', savedTheme);
        updateThemeIcons(savedTheme);
    } else {
        // Default to light theme
        htmlElement.setAttribute('data-theme', 'light');
        updateThemeIcons('light');
    }
    
    // Theme toggle click handler
    themeToggleBtn.addEventListener('click', () => {
        const currentTheme = htmlElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        htmlElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        
        updateThemeIcons(newTheme);
        showToast(`${newTheme.charAt(0).toUpperCase() + newTheme.slice(1)} mode activated`, 'info', 1500);
    });
    
    // Helper function to update icons
    function updateThemeIcons(theme) {
        if (theme === 'dark') {
            lightIcon.classList.add('hidden');
            darkIcon.classList.remove('hidden');
        } else {
            lightIcon.classList.remove('hidden');
            darkIcon.classList.add('hidden');
        }
    }
}

// Render ApexCharts for Blazor components
window.renderApexChart = function(elementId, options) {
    try {
        // Rendering chart for element

        // Check if ApexCharts is loaded
        if (typeof ApexCharts === 'undefined') {
            console.error('ApexCharts is not loaded yet');
            // Retry after a delay
            setTimeout(() => window.renderApexChart(elementId, options), 500);
            return;
        }

        // Check if element exists
        const element = document.querySelector("#" + elementId);
        if (!element) {
            console.error('Chart element not found:', elementId);
            return;
        }

        // Destroy existing chart if it exists
        if (window.charts && window.charts[elementId]) {
            // Destroying existing chart
            try {
                window.charts[elementId].destroy();
            } catch (destroyError) {
                console.warn('Error destroying existing chart:', destroyError);
            }
        }

        // Initialize charts object if it doesn't exist
        if (!window.charts) {
            window.charts = {};
        }

        // Create new chart
        // Creating new chart
        const chart = new ApexCharts(element, options);

        chart.render().then(() => {
            // Chart rendered successfully
        }).catch((error) => {
            console.error('Error during chart render:', error);
        });

        // Store chart reference for later cleanup
        window.charts[elementId] = chart;
    } catch (error) {
        console.error('Error rendering chart:', error);
        console.error('Element ID:', elementId);
        console.error('Options:', options);

        // Retry once after a delay
        setTimeout(() => {
            try {
                window.renderApexChart(elementId, options);
            } catch (retryError) {
                console.error('Retry failed:', retryError);
            }
        }, 1000);
    }
};

// Document ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize any search tables
    const searchInputs = document.querySelectorAll('[data-search-table]');
    searchInputs.forEach(input => {
        const tableId = input.getAttribute('data-search-table');
        setupTableSearch(tableId, input.id);
    });

    // Initialize form validation
    const forms = document.querySelectorAll('form[data-validate]');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!validateForm(this.id)) {
                e.preventDefault();
                showToast('Please fill in all required fields', 'error');
            }
        });
    });

    // Theme toggle initialization
    initThemeToggle();
});
