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
    scrollToTop: () => window.scrollTo({ top: 0, behavior: 'smooth' })
};
