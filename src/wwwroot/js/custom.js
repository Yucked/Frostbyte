document.addEventListener('DOMContentLoaded', function () {
    const menu = document.getElementById('menu');
    menu.addEventListener('click', function () {
        menu.classList.toggle('active');
    });
});