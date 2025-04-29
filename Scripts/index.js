document.addEventListener('DOMContentLoaded', function () {
    let basePath = '';
    const baseTag = document.querySelector('base');
    if (baseTag && baseTag.getAttribute('href')) {
        basePath = baseTag.getAttribute('href').replace(/\/$/, '');
    }

    if (!basePath) {
        const fullPath = window.location.pathname;
        const pathSegments = fullPath.split('/').filter(part => part.length > 0);


        const commonControllers = ['home', 'products', 'users'];

        if (pathSegments.length > 0 && !commonControllers.includes(pathSegments[0].toLowerCase())) {
            basePath = '/' + pathSegments[0];
        }
    }

    const fullPath = window.location.pathname;
    const effectivePath = basePath ? fullPath.substring(basePath.length) : fullPath;

    const pathParts = effectivePath.split('/').filter(part => part.length > 0);
    let activeController = '';

    if (pathParts.length > 0) {
        activeController = pathParts[0].toLowerCase();
    } else {
        activeController = 'home';
    }

    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    navLinks.forEach(link => {
        link.classList.remove('active');
    });

    navLinks.forEach(link => {
        let href = link.getAttribute('href');

        if (href.startsWith('~/')) {
            href = href.substring(2);
        }

        if (basePath && !href.startsWith(basePath)) {
            if (href.startsWith('/')) {
                href = basePath + href;
            } else {
                href = basePath + '/' + href;
            }
        }

        const hrefWithoutBase = basePath ? href.replace(basePath, '') : href;
        const hrefParts = hrefWithoutBase.split('/').filter(part => part.length > 0);
        let linkController = '';

        if (hrefParts.length > 0) {
            linkController = hrefParts[0].toLowerCase();
        }

        if (linkController === activeController) {
            link.classList.add('active');
        }
    });
});