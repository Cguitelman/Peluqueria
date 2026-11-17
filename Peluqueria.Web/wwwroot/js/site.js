document.addEventListener("DOMContentLoaded", () => {
    const overlay = document.getElementById("confirmOverlay");
    const message = document.getElementById("confirmMessage");
    const cancelButton = document.getElementById("confirmCancel");
    const acceptButton = document.getElementById("confirmAccept");
    let pendingForm = null;

    document.querySelectorAll("form[data-confirm]").forEach(form => {
        form.addEventListener("submit", event => {
            if (form.dataset.confirmed === "true") {
                delete form.dataset.confirmed;
                return;
            }

            event.preventDefault();
            pendingForm = form;
            if (message) message.textContent = form.dataset.confirm || "¿Querés desactivar este registro?";
            if (overlay) {
                overlay.hidden = false;
                overlay.setAttribute("aria-hidden", "false");
                document.body.classList.add("modal-open-custom");
            }
            acceptButton?.focus();
        });
    });

    const closeConfirm = () => {
        pendingForm = null;
        if (overlay) {
            overlay.hidden = true;
            overlay.setAttribute("aria-hidden", "true");
        }
        document.body.classList.remove("modal-open-custom");
    };

    cancelButton?.addEventListener("click", closeConfirm);
    overlay?.addEventListener("click", event => {
        if (event.target === overlay) closeConfirm();
    });
    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && overlay && !overlay.hidden) closeConfirm();
    });

    acceptButton?.addEventListener("click", () => {
        if (!pendingForm) return;
        const form = pendingForm;
        pendingForm = null;
        form.dataset.confirmed = "true";
        if (overlay) overlay.hidden = true;
        document.body.classList.remove("modal-open-custom");
        form.requestSubmit();
    });
});
