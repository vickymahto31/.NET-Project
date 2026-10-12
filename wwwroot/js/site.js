// WhisperSpace Client Script
document.addEventListener('DOMContentLoaded', function () {
    // 1. Character counters for textareas
    const contentTextarea = document.getElementById('postContent');
    const charCounter = document.getElementById('charCount');
    if (contentTextarea && charCounter) {
        contentTextarea.addEventListener('input', function () {
            charCounter.textContent = this.value.length;
        });
    }

    // 2. Random Pseudonym Generator
    const aliasInput = document.getElementById('authorAlias');
    const randomizeBtn = document.getElementById('btnRandomAlias');
    if (aliasInput && randomizeBtn) {
        const adjectives = ['Quiet', 'Curious', 'Silent', 'Hopeful', 'Tired', 'Gentle', 'Brave', 'Thoughtful', 'Wandering', 'Calm'];
        const nouns = ['Phoenix', 'Owl', 'Penguin', 'Sparrow', 'Nomad', 'Seeker', 'Anchor', 'Breeze', 'Voyager', 'Dreamer'];

        randomizeBtn.addEventListener('click', function () {
            const adj = adjectives[Math.floor(Math.random() * adjectives.length)];
            const noun = nouns[Math.floor(Math.random() * nouns.length)];
            aliasInput.value = `${adj} ${noun}`;
        });
    }

    // 3. Simple Vanilla Modal triggers for Report Dialog
    const openReportBtns = document.querySelectorAll('[data-report-trigger]');
    const reportModal = document.getElementById('reportModal');
    const closeReportBtns = document.querySelectorAll('[data-close-modal]');

    if (reportModal) {
        openReportBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                const postId = this.getAttribute('data-post-id');
                const postTitle = this.getAttribute('data-post-title');
                
                const hiddenInput = document.getElementById('modalReportPostId');
                const titleSpan = document.getElementById('modalReportPostTitle');
                
                if (hiddenInput) hiddenInput.value = postId;
                if (titleSpan) titleSpan.textContent = postTitle || 'this post';

                reportModal.style.display = 'flex';
            });
        });

        closeReportBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                reportModal.style.display = 'none';
            });
        });

        // Close on backdrop click
        window.addEventListener('click', function (e) {
            if (e.target === reportModal) {
                reportModal.style.display = 'none';
            }
        });
    }
});
