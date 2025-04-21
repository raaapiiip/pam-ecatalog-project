document.addEventListener('DOMContentLoaded', function () {
    function getNormalizedPath(fullPath) {
        if (fullPath.endsWith('/')) {
            fullPath = fullPath.slice(0, -1);
        }
        fullPath = fullPath.split('?')[0];
        return fullPath.toLowerCase();
    }

    const currentPath = getNormalizedPath(window.location.pathname);

    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    navLinks.forEach(link => {
        link.classList.remove('active');
    });

    let bestMatchLength = -1;
    let bestMatchLink = null;

    navLinks.forEach(link => {
        let href = link.getAttribute('href');

        if (href.startsWith('~/')) {
            href = href.substring(2);
        }

        if (!href.startsWith('/')) {
            href = '/' + href;
        }

        const normalizedHref = getNormalizedPath(href);

        if (currentPath.includes(normalizedHref) ||
            (normalizedHref === '/' && (currentPath === '' || currentPath === '/'))) {

            if (normalizedHref.length > bestMatchLength) {
                bestMatchLength = normalizedHref.length;
                bestMatchLink = link;
            }
        }
    });

    if (bestMatchLink) {
        bestMatchLink.classList.add('active');
    }
});