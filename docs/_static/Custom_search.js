document.addEventListener('DOMContentLoaded', function() {
    var searchInput = document.querySelector('input[name="q"]');
    if (searchInput) {
        searchInput.placeholder = 'Поиск';
    }
});