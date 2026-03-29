window.umblink = {
    initSortable: (el, dotnetRef) => {
        if (!el) return;
        Sortable.create(el, {
            animation: 150,
            handle: '.drag-handle',
            onEnd: async (evt) => {
                await dotnetRef.invokeMethodAsync('OnReorder', evt.oldIndex, evt.newIndex);
            }
        });
    },
    renderChart: (canvasId, type, labels, datasets) => {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        if (ctx._chart) ctx._chart.destroy();
        ctx._chart = new Chart(ctx, {
            type,
            data: { labels, datasets },
            options: {
                responsive: true,
                plugins: { legend: { display: datasets.length > 1 } }
            }
        });
    },
    copyToClipboard: async (text) => {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            return false;
        }
    },
    scrollToTop: () => window.scrollTo({ top: 0, behavior: 'smooth' }),
    getTheme: () => localStorage.getItem('umblink-theme') || 'light',
    setTheme: (theme) => {
        localStorage.setItem('umblink-theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
    },
    initTheme: () => {
        const saved = localStorage.getItem('umblink-theme') || 'light';
        document.documentElement.setAttribute('data-theme', saved);
    },
    initHeroGradient: (el) => {
        if (!el) return;
        let targetX = 50, targetY = 50, currentX = 50, currentY = 50;
        let rafId = null;

        const lerp = (a, b, t) => a + (b - a) * t;

        function animate() {
            currentX = lerp(currentX, targetX, 0.06);
            currentY = lerp(currentY, targetY, 0.06);
            el.style.setProperty('--gx', currentX + '%');
            el.style.setProperty('--gy', currentY + '%');
            rafId = requestAnimationFrame(animate);
        }

        el.addEventListener('mousemove', (e) => {
            const rect = el.getBoundingClientRect();
            targetX = ((e.clientX - rect.left) / rect.width) * 100;
            targetY = ((e.clientY - rect.top) / rect.height) * 100;
        });

        el.addEventListener('mouseleave', () => {
            targetX = 50;
            targetY = 50;
        });

        animate();

        // Fallback: auto-animation for touch devices
        if ('ontouchstart' in window) {
            cancelAnimationFrame(rafId);
            el.classList.add('hero-gradient-auto');
        } else {
            el._gradientRafId = rafId;
        }
    },
    disposeHeroGradient: (el) => {
        if (el && el._gradientRafId) {
            cancelAnimationFrame(el._gradientRafId);
            el._gradientRafId = null;
        }
    }
};
