document.addEventListener("DOMContentLoaded", () => {
    const detalheEvento = document.querySelector(".evento-detalhe");
    const modalAtividade = document.querySelector("[data-atividade-modal]");

    document.querySelectorAll("[data-evento-id]").forEach((element) => {
        element.addEventListener("click", (event) => {
            if (event.target.closest("a, button, form")) {
                return;
            }

            const eventoId = element.getAttribute("data-evento-id");
            if (!eventoId) {
                return;
            }

            window.location.href = `/Evento/Detalhes/${eventoId}`;
        });
    });

    if (!detalheEvento || !modalAtividade) {
        return;
    }

    const abrirModal = () => {
        modalAtividade.classList.add("is-open");
        document.body.classList.add("evento-modal-open");
    };

    const fecharModal = () => {
        modalAtividade.classList.remove("is-open");
        document.body.classList.remove("evento-modal-open");
    };

    document.querySelectorAll("[data-open-atividade-card]").forEach((button) => {
        button.addEventListener("click", abrirModal);
    });

    document.querySelectorAll("[data-close-atividade-card]").forEach((button) => {
        button.addEventListener("click", fecharModal);
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            fecharModal();
        }
    });

    if (detalheEvento.getAttribute("data-open-atividade") === "true") {
        abrirModal();
    }
});
