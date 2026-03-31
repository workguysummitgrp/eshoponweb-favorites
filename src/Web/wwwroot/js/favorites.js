(function () {
    'use strict';

    var TOGGLE_URL = '/api/favorites/toggle';
    var LOGIN_URL = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
    var TOOLTIP_DISMISS_MS = 5000;

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function setHeartState(btn, favorited) {
        if (favorited) {
            btn.classList.add('esh-favorite-heart--favorited');
            btn.setAttribute('aria-pressed', 'true');
            btn.setAttribute('aria-label', btn.getAttribute('aria-label').replace('Add', 'Remove'));
            btn.setAttribute('title', 'Remove from favorites');
        } else {
            btn.classList.remove('esh-favorite-heart--favorited');
            btn.setAttribute('aria-pressed', 'false');
            btn.setAttribute('aria-label', btn.getAttribute('aria-label').replace('Remove', 'Add'));
            btn.setAttribute('title', 'Add to favorites');
        }
    }

    function setLoading(btn, loading) {
        if (loading) {
            btn.setAttribute('data-loading', 'true');
            btn.classList.add('esh-favorite-heart--loading');
        } else {
            btn.removeAttribute('data-loading');
            btn.classList.remove('esh-favorite-heart--loading');
        }
    }

    function shakeHeart(btn) {
        btn.classList.add('esh-favorite-heart--error');
        setTimeout(function () {
            btn.classList.remove('esh-favorite-heart--error');
        }, 400);
    }

    function bounceHeart(btn) {
        btn.classList.add('esh-favorite-heart--bounce');
        setTimeout(function () {
            btn.classList.remove('esh-favorite-heart--bounce');
        }, 300);
    }

    function shrinkHeart(btn) {
        btn.classList.add('esh-favorite-heart--shrink');
        setTimeout(function () {
            btn.classList.remove('esh-favorite-heart--shrink');
        }, 200);
    }

    var activeTooltip = null;
    var tooltipTimer = null;

    function dismissTooltip() {
        if (activeTooltip && activeTooltip.parentNode) {
            activeTooltip.parentNode.removeChild(activeTooltip);
        }
        activeTooltip = null;
        if (tooltipTimer) {
            clearTimeout(tooltipTimer);
            tooltipTimer = null;
        }
    }

    function showLoginTooltip(btn) {
        dismissTooltip();
        var tooltip = document.createElement('div');
        tooltip.className = 'esh-favorite-tooltip';
        tooltip.innerHTML = '<a href="' + LOGIN_URL + '">Log in</a> to save favorites';
        var wrapper = btn.closest('.esh-catalog-thumbnail-wrapper');
        if (wrapper) {
            wrapper.appendChild(tooltip);
        } else {
            btn.parentNode.appendChild(tooltip);
        }
        activeTooltip = tooltip;
        tooltipTimer = setTimeout(dismissTooltip, TOOLTIP_DISMISS_MS);
    }

    function onEscapeKey(e) {
        if (e.key === 'Escape' && activeTooltip) {
            dismissTooltip();
        }
    }

    document.addEventListener('keydown', onEscapeKey);

    function toggleFavorite(btn) {
        var catalogItemId = parseInt(btn.getAttribute('data-catalog-item-id'), 10);
        if (!catalogItemId || catalogItemId <= 0) return;
        if (btn.hasAttribute('data-loading')) return;

        var wasFavorited = btn.classList.contains('esh-favorite-heart--favorited');
        setHeartState(btn, !wasFavorited);
        if (!wasFavorited) { bounceHeart(btn); } else { shrinkHeart(btn); }
        setLoading(btn, true);

        var token = getAntiForgeryToken();
        var xhr = new XMLHttpRequest();
        xhr.open('POST', TOGGLE_URL, true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader('RequestVerificationToken', token);

        xhr.onload = function () {
            setLoading(btn, false);
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    setHeartState(btn, data.isFavorited);
                } catch (e) { }
            } else {
                setHeartState(btn, wasFavorited);
                shakeHeart(btn);
            }
        };

        xhr.onerror = function () {
            setLoading(btn, false);
            setHeartState(btn, wasFavorited);
            shakeHeart(btn);
        };

        xhr.send(JSON.stringify({ catalogItemId: catalogItemId }));
    }

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.esh-favorite-heart');
        if (!btn) return;
        e.preventDefault();
        e.stopPropagation();
        if (btn.classList.contains('esh-favorite-heart--disabled')) {
            showLoginTooltip(btn);
            return;
        }
        toggleFavorite(btn);
    });
})();
