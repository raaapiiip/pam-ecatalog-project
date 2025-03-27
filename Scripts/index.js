document.addEventListener('DOMContentLoaded', function () {
    const currentPath = window.location.pathname;

    let activeController = '';
    const pathParts = currentPath.split('/').filter(part => part.length > 0);
    if (pathParts.length > 0) {
        activeController = pathParts[0].toLowerCase();
    } else {
        activeController = 'home';
    }

    console.log("Active controller:", activeController);

    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    navLinks.forEach(link => {
        link.classList.remove('active');
    });

    navLinks.forEach(link => {
        let href = link.getAttribute('href');

        if (href.startsWith('~/')) {
            href = href.substring(2);
        }

        const hrefParts = href.split('/').filter(part => part.length > 0);
        let linkController = '';
        if (hrefParts.length > 0) {
            linkController = hrefParts[0].toLowerCase();
        }

        if (linkController === activeController) {
            link.classList.add('active');
            console.log("Activated link with controller:", linkController);
        }
    });
});