(function () {
  "use strict";

  function isCheckoutPage() {
    const params = new URLSearchParams(window.location.search);
    return params.get("page")?.toLowerCase() === "checkout" && Number(params.get("ticketId") || 0) > 0;
  }

  function getTicketId() {
    return Number(new URLSearchParams(window.location.search).get("ticketId") || 0);
  }

  function removeMount() {
    const mount = document.getElementById("eh-checkout-mount");
    if (mount) mount.remove();
  }

  function hasSpaPageContent() {
    const rootInner = document.getElementById("root")?.firstElementChild;
    if (!rootInner) return false;

    return Array.from(rootInner.children).some(function (child) {
      return child.tagName !== "HEADER" && child.id !== "eh-checkout-mount";
    });
  }

  function syncCheckoutVisibility() {
    if (!isCheckoutPage() || hasSpaPageContent()) {
      removeMount();
      return;
    }

    if (!document.getElementById("eh-checkout-mount")) {
      mountCheckout();
    }
  }

  function setupNavigationCleanup() {
    window.addEventListener("popstate", syncCheckoutVisibility);

    ["pushState", "replaceState"].forEach(function (method) {
      const original = history[method].bind(history);
      history[method] = function () {
        const result = original.apply(history, arguments);
        syncCheckoutVisibility();
        return result;
      };
    });

    document.addEventListener(
      "click",
      function (event) {
        if (!document.getElementById("eh-checkout-mount")) return;
        if (event.target.closest("#eh-checkout-mount")) return;

        const navTarget = event.target.closest("#root header a, #root header button");
        if (navTarget) {
          setTimeout(removeMount, 0);
        }
      },
      true
    );

    const root = document.getElementById("root");
    if (!root) return;

    const observer = new MutationObserver(function () {
      if (!document.getElementById("eh-checkout-mount")) return;
      if (hasSpaPageContent()) removeMount();
    });

    observer.observe(root, { childList: true, subtree: true });
  }

  setupNavigationCleanup();

  function esc(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function normalizeMethod(name) {
    return String(name ?? "")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .trim()
      .toLowerCase();
  }

  function methodKind(name) {
    const normalized = normalizeMethod(name);
    if (normalized.includes("cartao")) return "card";
    if (normalized.includes("mb way")) return "mbway";
    if (normalized.includes("paypal")) return "paypal";
    return "unknown";
  }

  function formatPrice(value) {
    return new Intl.NumberFormat("pt-PT", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(Number(value || 0));
  }

  function fieldHtml(label, id, value, options) {
    const opts = options || {};
    return (
      '<label class="eh-checkout-field' +
      (opts.full ? " eh-checkout-field--full" : "") +
      '">' +
      '<span class="eh-checkout-label">' +
      esc(label) +
      "</span>" +
      '<input id="' +
      esc(id) +
      '" type="' +
      esc(opts.type || "text") +
      '" value="' +
      esc(value) +
      '" placeholder="' +
      esc(opts.placeholder || "") +
      '" class="eh-checkout-input" />' +
      "</label>"
    );
  }

  function renderPaymentFields(kind) {
    if (kind === "card") {
      return (
        '<div class="eh-checkout-payment-grid">' +
        fieldHtml("Numero do cartao", "eh-numero-cartao", "", { placeholder: "0000 0000 0000 0000" }) +
        fieldHtml("Nome do titular", "eh-nome-titular", "", { placeholder: "Como aparece no cartao" }) +
        fieldHtml("Validade", "eh-validade-cartao", "", { placeholder: "MM/AA" }) +
        fieldHtml("CVV", "eh-cvv", "", { placeholder: "123" }) +
        fieldHtml("Telemovel de contacto", "eh-telemovel", "", { placeholder: "912345678", full: true }) +
        "</div>"
      );
    }

    if (kind === "mbway") {
      return (
        '<p style="margin:0 0 1rem;font-family:ui-monospace,monospace;font-size:0.875rem;color:rgba(255,255,255,0.55);">Vais receber um pedido de pagamento no telemovel associado a MB Way.</p>' +
        fieldHtml("Telemovel MB Way", "eh-telemovel", "", { placeholder: "912345678" })
      );
    }

    if (kind === "paypal") {
      return (
        '<p style="margin:0 0 1rem;font-family:ui-monospace,monospace;font-size:0.875rem;color:rgba(255,255,255,0.55);">Indica o email da conta PayPal que queres usar para pagar.</p>' +
        fieldHtml("Email PayPal", "eh-email-paypal", "", { type: "email", placeholder: "conta@paypal.com" })
      );
    }

    return '<p style="font-family:ui-monospace,monospace;font-size:0.875rem;color:rgba(255,255,255,0.5);">Seleciona um metodo de pagamento para ver os campos necessarios.</p>';
  }

  function summaryRow(label, value) {
    return (
      '<div style="display:grid;grid-template-columns:120px 1fr;gap:1rem;margin-bottom:0.75rem;">' +
      '<span style="color:rgba(255,255,255,0.45);">' +
      esc(label) +
      "</span>" +
      '<span style="color:rgba(255,255,255,0.85);">' +
      esc(value) +
      "</span></div>"
    );
  }

  function renderPage(data) {
    const methods = Array.isArray(data.tiposPagamento) ? data.tiposPagamento : [];
    const options = methods
      .map(function (method) {
        return (
          '<option value="' +
          esc(method.idTipoPagamento) +
          '">' +
          esc(method.nome) +
          "</option>"
        );
      })
      .join("");

    const firstMethod = methods[0] || null;
    const initialKind = firstMethod ? methodKind(firstMethod.nome) : "unknown";

    return (
      '<main class="eh-checkout-main">' +
      '<div class="eh-checkout-layout">' +
      '<section class="eh-checkout-panel">' +
      '<button type="button" id="eh-checkout-back" style="margin-bottom:2rem;display:flex;align-items:center;gap:0.5rem;font-family:ui-monospace,monospace;font-size:0.75rem;font-weight:700;letter-spacing:0.28em;text-transform:uppercase;color:rgba(255,255,255,0.8);background:none;border:none;cursor:pointer;">← Voltar</button>' +
      '<div style="margin-bottom:1rem;font-family:ui-monospace,monospace;font-size:0.75rem;font-weight:900;letter-spacing:0.38em;text-transform:uppercase;color:#facc15;">Resumo da compra</div>' +
      '<h1 style="margin:0 0 2rem;font-size:clamp(2rem,4vw,3rem);font-weight:900;text-transform:uppercase;letter-spacing:-0.02em;">' +
      esc(data.nomeBilhete) +
      "</h1>" +
      '<div style="border-top:1px solid rgba(255,255,255,0.1);padding-top:1.5rem;font-family:ui-monospace,monospace;font-size:0.875rem;">' +
      summaryRow("Evento", data.nomeEvento) +
      summaryRow("Tipo", data.tipoBilhete) +
      summaryRow("Data", data.dataEvento || "-") +
      summaryRow("Hora", data.horaEvento || "-") +
      summaryRow("Local", data.localEvento || "-") +
      summaryRow("Acesso", data.descricaoAcesso) +
      "</div>" +
      '<p style="margin:2rem 0 0;border-radius:1rem;border:1px solid rgba(255,255,255,0.1);background:rgba(255,255,255,0.04);padding:1rem 1.25rem;font-family:ui-monospace,monospace;font-size:0.875rem;color:rgba(255,255,255,0.7);">' +
      (data.quantidadeDisponivel > 0
        ? data.quantidadeDisponivel + " bilhetes disponiveis neste tipo."
        : "Este tipo de bilhete esta atualmente esgotado.") +
      "</p>" +
      '<div style="margin-top:2.5rem;display:flex;align-items:center;justify-content:space-between;border-top:1px solid rgba(255,255,255,0.1);padding-top:1.5rem;">' +
      '<span style="font-family:ui-monospace,monospace;font-size:0.75rem;letter-spacing:0.24em;text-transform:uppercase;color:rgba(255,255,255,0.5);">Total</span>' +
      '<span style="font-size:2rem;font-weight:900;color:#facc15;">' +
      formatPrice(data.preco) +
      " EUR</span></div></section>" +
      '<section class="eh-checkout-panel">' +
      '<div style="margin-bottom:1rem;font-family:ui-monospace,monospace;font-size:0.75rem;font-weight:900;letter-spacing:0.38em;text-transform:uppercase;color:#facc15;">Pagamento</div>' +
      '<h2 style="margin:0 0 0.5rem;font-size:clamp(1.75rem,3vw,2.5rem);font-weight:900;text-transform:uppercase;letter-spacing:-0.02em;">Dados do comprador</h2>' +
      '<p style="margin:0 0 2rem;font-family:ui-monospace,monospace;font-size:0.875rem;color:rgba(255,255,255,0.6);">Escolhe o metodo de pagamento e preenche apenas os campos necessarios.</p>' +
      '<div id="eh-checkout-message"></div>' +
      '<form id="eh-checkout-form">' +
      fieldHtml("Nome", "eh-nome", data.nomeComprador) +
      fieldHtml("Email de contacto", "eh-email", data.email, { type: "email" }) +
      fieldHtml("Morada", "eh-morada", data.morada) +
      '<label class="eh-checkout-field"><span class="eh-checkout-label">Metodo de pagamento</span>' +
      '<select id="eh-metodo" class="eh-checkout-select">' +
      '<option value="">Seleciona um metodo</option>' +
      options +
      "</select></label>" +
      '<div id="eh-payment-fields" style="margin-bottom:1.5rem;border-radius:24px;border:1px solid rgba(255,255,255,0.1);background:rgba(255,255,255,0.03);padding:1.5rem;">' +
      renderPaymentFields(initialKind) +
      "</div>" +
      '<div class="eh-checkout-actions">' +
      '<button type="submit" id="eh-submit" class="eh-checkout-btn-primary"' +
      (data.quantidadeDisponivel <= 0 ? " disabled" : "") +
      ">Confirmar compra</button>" +
      '<button type="button" id="eh-cancel" class="eh-checkout-btn-secondary">Cancelar</button>' +
      "</div></form></section></div></main>"
    );
  }

  function showMessage(text, isError) {
    const box = document.getElementById("eh-checkout-message");
    if (!box) return;
    box.style.marginBottom = "1.5rem";
    box.style.borderRadius = "1rem";
    box.style.padding = "1rem 1.25rem";
    box.style.fontFamily = "ui-monospace, monospace";
    if (isError) {
      box.style.border = "1px solid rgba(248, 113, 113, 0.35)";
      box.style.background = "rgba(248, 113, 113, 0.1)";
      box.style.color = "#fecaca";
    } else {
      box.style.border = "1px solid rgba(250, 204, 21, 0.35)";
      box.style.background = "rgba(250, 204, 21, 0.15)";
      box.style.color = "#fef9c3";
    }
    box.textContent = text;
  }

  function bindCheckout(data) {
    const methods = Array.isArray(data.tiposPagamento) ? data.tiposPagamento : [];
    const methodSelect = document.getElementById("eh-metodo");
    const paymentFields = document.getElementById("eh-payment-fields");
    const form = document.getElementById("eh-checkout-form");

    if (methods.length && methodSelect) {
      methodSelect.value = String(methods[0].idTipoPagamento);
    }

    function refreshPaymentFields() {
      const selected = methods.find(function (item) {
        return String(item.idTipoPagamento) === methodSelect.value;
      });
      paymentFields.innerHTML = renderPaymentFields(selected ? methodKind(selected.nome) : "unknown");

      const telemovel = document.getElementById("eh-telemovel");
      if (telemovel && data.telemovel) {
        telemovel.value = data.telemovel;
      }
    }

    methodSelect.addEventListener("change", refreshPaymentFields);
    refreshPaymentFields();

    document.getElementById("eh-checkout-back").onclick = function () {
      window.location.href = "/?page=events";
    };
    document.getElementById("eh-cancel").onclick = function () {
      window.location.href = "/?page=events";
    };

    form.addEventListener("submit", async function (event) {
      event.preventDefault();
      const selected = methods.find(function (item) {
        return String(item.idTipoPagamento) === methodSelect.value;
      });
      const kind = selected ? methodKind(selected.nome) : "unknown";
      const payload = {
        idBilheteEvento: data.idBilheteEvento,
        nomeComprador: document.getElementById("eh-nome").value.trim(),
        email: document.getElementById("eh-email").value.trim(),
        morada: document.getElementById("eh-morada").value.trim(),
        idTipoPagamento: Number(methodSelect.value),
        telemovel: (document.getElementById("eh-telemovel") || {}).value || "",
        numeroCartao: (document.getElementById("eh-numero-cartao") || {}).value || undefined,
        nomeTitular: (document.getElementById("eh-nome-titular") || {}).value || undefined,
        validadeCartao: (document.getElementById("eh-validade-cartao") || {}).value || undefined,
        cvv: (document.getElementById("eh-cvv") || {}).value || undefined,
        emailPaypal: (document.getElementById("eh-email-paypal") || {}).value || undefined
      };

      if (kind !== "card") {
        delete payload.numeroCartao;
        delete payload.nomeTitular;
        delete payload.validadeCartao;
        delete payload.cvv;
      }
      if (kind !== "paypal") delete payload.emailPaypal;
      if (kind === "paypal") payload.telemovel = data.telemovel || "";

      const submit = document.getElementById("eh-submit");
      submit.disabled = true;
      submit.textContent = "A processar...";

      try {
        const response = await fetch("/api/bilhetes/checkout", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          credentials: "include",
          body: JSON.stringify(payload)
        });

        const body = await response.json().catch(function () {
          return {};
        });

        if (!response.ok) {
          throw new Error(body.message || "Nao foi possivel concluir a compra.");
        }

        showMessage(body.message || "Compra concluida com sucesso!", false);
        setTimeout(function () {
          window.location.href = "/Home/Index?page=purchase_history";
        }, 1400);
      } catch (error) {
        showMessage(error.message || "Nao foi possivel concluir a compra.", true);
        submit.disabled = false;
        submit.textContent = "Confirmar compra";
      }
    });
  }

  function insertMountPoint() {
    const root = document.getElementById("root");
    if (!root) return null;

    const existing = document.getElementById("eh-checkout-mount");
    if (existing) return existing;

    const mount = document.createElement("div");
    mount.id = "eh-checkout-mount";
    mount.innerHTML = '<div class="eh-checkout-main" style="text-align:center;font-family:ui-monospace,monospace;color:rgba(255,255,255,0.6);">A carregar checkout...</div>';

    const header = root.querySelector("header");
    if (header) {
      header.insertAdjacentElement("afterend", mount);
    } else {
      root.insertAdjacentElement("afterbegin", mount);
    }

    return mount;
  }

  async function mountCheckout() {
    const mountPoint = insertMountPoint();
    if (!mountPoint) return;

    const currentTicketId = getTicketId();
    if (!currentTicketId) return;

    try {
      const response = await fetch("/api/bilhetes/checkout/" + currentTicketId, {
        credentials: "include"
      });
      const body = await response.json().catch(function () {
        return null;
      });

      if (!response.ok) {
        throw new Error((body && body.message) || "Nao foi possivel carregar o checkout.");
      }

      if (!isCheckoutPage() || hasSpaPageContent()) {
        removeMount();
        return;
      }

      mountPoint.innerHTML = renderPage(body);
      bindCheckout(body);
    } catch (error) {
      if (!isCheckoutPage() || hasSpaPageContent()) {
        removeMount();
        return;
      }

      mountPoint.innerHTML =
        '<div class="eh-checkout-main"><div class="eh-checkout-panel" style="max-width:720px;margin:0 auto;text-align:center;color:#fecaca;">' +
        esc(error.message || "Checkout indisponivel.") +
        "</div></div>";
    }
  }

  if (isCheckoutPage() && getTicketId()) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", mountCheckout);
    } else {
      setTimeout(mountCheckout, 50);
    }
  }
})();
