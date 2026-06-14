import { FormEvent, useEffect, useMemo, useState } from "react";
import { ArrowLeft, CreditCard, Smartphone, Wallet } from "lucide-react";
import { api, CheckoutData, PaymentMethod } from "../api";

type Props = {
  ticketId: number;
  onNavigate: (page: "purchase_history" | "events" | "event_details") => void;
  onBack: () => void;
};

type FormState = {
  nomeComprador: string;
  email: string;
  telemovel: string;
  morada: string;
  idTipoPagamento: string;
  numeroCartao: string;
  nomeTitular: string;
  validadeCartao: string;
  cvv: string;
  emailPaypal: string;
};

const emptyForm: FormState = {
  nomeComprador: "",
  email: "",
  telemovel: "",
  morada: "",
  idTipoPagamento: "",
  numeroCartao: "",
  nomeTitular: "",
  validadeCartao: "",
  cvv: "",
  emailPaypal: ""
};

function normalizeMethod(name: string) {
  return name
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .trim()
    .toLowerCase();
}

function methodKind(name: string): "card" | "mbway" | "paypal" | "unknown" {
  const normalized = normalizeMethod(name);
  if (normalized.includes("cartao") || normalized.includes("cartao bancario")) return "card";
  if (normalized.includes("mb way")) return "mbway";
  if (normalized.includes("paypal")) return "paypal";
  return "unknown";
}

function formatPrice(value: number) {
  return new Intl.NumberFormat("pt-PT", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value);
}

export function CheckoutView({ ticketId, onNavigate, onBack }: Props) {
  const [checkout, setCheckout] = useState<CheckoutData | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    api
      .getCheckout(ticketId)
      .then((data) => {
        setCheckout(data);
        setForm({
          ...emptyForm,
          nomeComprador: data.nomeComprador,
          email: data.email,
          telemovel: data.telemovel,
          morada: data.morada,
          idTipoPagamento: data.tiposPagamento[0]?.idTipoPagamento?.toString() ?? ""
        });
      })
      .catch((err) => setError(err instanceof Error ? err.message : "Nao foi possivel carregar o checkout."))
      .finally(() => setLoading(false));
  }, [ticketId]);

  const selectedMethod = useMemo(() => {
    if (!checkout || !form.idTipoPagamento) return null;
    return checkout.tiposPagamento.find((item) => String(item.idTipoPagamento) === form.idTipoPagamento) ?? null;
  }, [checkout, form.idTipoPagamento]);

  const paymentKind = selectedMethod ? methodKind(selectedMethod.nome) : "unknown";

  function updateField<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!checkout || submitting || checkout.quantidadeDisponivel <= 0) return;

    setSubmitting(true);
    setError(null);

    try {
      const response = await api.purchaseTicket({
        idBilheteEvento: checkout.idBilheteEvento,
        nomeComprador: form.nomeComprador,
        email: form.email,
        telemovel: form.telemovel,
        morada: form.morada,
        idTipoPagamento: Number(form.idTipoPagamento),
        numeroCartao: paymentKind === "card" ? form.numeroCartao : undefined,
        nomeTitular: paymentKind === "card" ? form.nomeTitular : undefined,
        validadeCartao: paymentKind === "card" ? form.validadeCartao : undefined,
        cvv: paymentKind === "card" ? form.cvv : undefined,
        emailPaypal: paymentKind === "paypal" ? form.emailPaypal : undefined
      });

      setSuccess(response.message);
      setTimeout(() => onNavigate("purchase_history"), 1400);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Nao foi possivel concluir a compra.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return (
      <main className="relative min-h-screen bg-[#333533] px-6 py-32 text-white">
        <div className="mx-auto max-w-[1200px] rounded-[30px] border border-white/10 bg-[#0a0a0a]/72 p-12 text-center font-mono text-white/60">
          A carregar checkout...
        </div>
      </main>
    );
  }

  if (!checkout) {
    return (
      <main className="relative min-h-screen bg-[#333533] px-6 py-32 text-white">
        <div className="mx-auto max-w-[1200px] rounded-[30px] border border-red-400/30 bg-red-400/10 p-12 text-center font-mono">
          {error || "Checkout indisponivel."}
        </div>
      </main>
    );
  }

  return (
    <main className="relative overflow-hidden bg-[#333533] px-6 pt-24 pb-10 text-white">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,rgba(250,204,21,0.08),transparent_35%)]" />

      <div className="relative z-10 mx-auto grid max-w-[1400px] grid-cols-1 items-start gap-8 md:grid-cols-2">
        <section className="rounded-[30px] border border-white/10 bg-[#0a0a0a]/72 p-10 shadow-2xl backdrop-blur-2xl">
          <button
            type="button"
            onClick={onBack}
            className="mb-8 flex items-center gap-2 font-mono text-xs font-bold uppercase tracking-[0.28em] text-white/80 hover:text-yellow-400"
          >
            <ArrowLeft className="h-4 w-4" /> Voltar
          </button>

          <div className="mb-4 font-mono text-xs font-black uppercase tracking-[0.38em] text-yellow-400">
            Resumo da compra
          </div>
          <h1 className="mb-8 text-5xl font-black uppercase tracking-tight">{checkout.nomeBilhete}</h1>

          <div className="space-y-4 border-t border-white/10 pt-6 font-mono text-sm">
            <SummaryRow label="Evento" value={checkout.nomeEvento} />
            <SummaryRow label="Tipo" value={checkout.tipoBilhete} />
            <SummaryRow label="Data" value={checkout.dataEvento || "-"} />
            <SummaryRow label="Hora" value={checkout.horaEvento || "-"} />
            <SummaryRow label="Local" value={checkout.localEvento || "-"} />
            <SummaryRow label="Acesso" value={checkout.descricaoAcesso} />
          </div>

          <p className="mt-8 rounded-2xl border border-white/10 bg-white/[0.04] px-5 py-4 font-mono text-sm text-white/70">
            {checkout.quantidadeDisponivel > 0
              ? `${checkout.quantidadeDisponivel} bilhetes disponiveis neste tipo.`
              : "Este tipo de bilhete esta atualmente esgotado."}
          </p>

          <div className="mt-10 flex items-center justify-between border-t border-white/10 pt-6">
            <span className="font-mono text-xs uppercase tracking-[0.24em] text-white/50">Total</span>
            <span className="text-3xl font-black text-yellow-400">{formatPrice(checkout.preco)} EUR</span>
          </div>
        </section>

        <section className="rounded-[30px] border border-white/10 bg-[#0a0a0a]/72 p-10 shadow-2xl backdrop-blur-2xl">
          <div className="mb-4 font-mono text-xs font-black uppercase tracking-[0.38em] text-yellow-400">
            Pagamento
          </div>
          <h2 className="mb-2 text-4xl font-black uppercase tracking-tight">Dados do comprador</h2>
          <p className="mb-8 font-mono text-sm text-white/60">
            Escolhe o metodo de pagamento e preenche apenas os campos necessarios.
          </p>

          {success && (
            <div className="mb-6 rounded-2xl border border-yellow-400/35 bg-yellow-400/15 px-5 py-4 font-mono text-yellow-100">
              {success}
            </div>
          )}
          {error && (
            <div className="mb-6 rounded-2xl border border-red-400/35 bg-red-400/10 px-5 py-4 font-mono text-red-200">
              {error}
            </div>
          )}

          <form onSubmit={submit} className="space-y-6">
            <Field label="Nome" value={form.nomeComprador} onChange={(value) => updateField("nomeComprador", value)} />
            <Field label="Email de contacto" type="email" value={form.email} onChange={(value) => updateField("email", value)} />
            <Field label="Morada" value={form.morada} onChange={(value) => updateField("morada", value)} />

            <label className="block space-y-3">
              <span className="block font-mono text-xs font-black uppercase tracking-[0.18em] text-white/80">
                Metodo de pagamento
              </span>
              <div className="relative rounded-2xl border border-white/10 bg-white/[0.07] focus-within:border-yellow-400">
                <select
                  value={form.idTipoPagamento}
                  onChange={(event) => updateField("idTipoPagamento", event.target.value)}
                  className="w-full appearance-none bg-transparent px-5 py-4 font-mono text-lg text-white outline-none"
                >
                  <option value="" className="bg-[#1a1a1a]">
                    Seleciona um metodo
                  </option>
                  {checkout.tiposPagamento.map((method: PaymentMethod) => (
                    <option key={method.idTipoPagamento} value={method.idTipoPagamento} className="bg-[#1a1a1a]">
                      {method.nome}
                    </option>
                  ))}
                </select>
              </div>
            </label>

            {selectedMethod && (
              <div className="rounded-[24px] border border-white/10 bg-white/[0.03] p-6">
                <div className="mb-5 flex items-center gap-3">
                  {paymentKind === "card" && <CreditCard className="h-5 w-5 text-yellow-400" />}
                  {paymentKind === "mbway" && <Smartphone className="h-5 w-5 text-yellow-400" />}
                  {paymentKind === "paypal" && <Wallet className="h-5 w-5 text-yellow-400" />}
                  <div>
                    <p className="font-mono text-xs uppercase tracking-[0.24em] text-white/45">Metodo selecionado</p>
                    <p className="text-lg font-bold uppercase tracking-tight">{selectedMethod.nome}</p>
                  </div>
                </div>

                {paymentKind === "card" && (
                  <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <Field
                      label="Numero do cartao"
                      value={form.numeroCartao}
                      onChange={(value) => updateField("numeroCartao", value)}
                      placeholder="0000 0000 0000 0000"
                    />
                    <Field
                      label="Nome do titular"
                      value={form.nomeTitular}
                      onChange={(value) => updateField("nomeTitular", value)}
                      placeholder="Como aparece no cartao"
                    />
                    <Field
                      label="Validade"
                      value={form.validadeCartao}
                      onChange={(value) => updateField("validadeCartao", value)}
                      placeholder="MM/AA"
                    />
                    <Field
                      label="CVV"
                      value={form.cvv}
                      onChange={(value) => updateField("cvv", value)}
                      placeholder="123"
                    />
                    <div className="md:col-span-2">
                      <Field
                        label="Telemovel de contacto"
                        value={form.telemovel}
                        onChange={(value) => updateField("telemovel", value)}
                        placeholder="912345678"
                      />
                    </div>
                  </div>
                )}

                {paymentKind === "mbway" && (
                  <div className="space-y-4">
                    <p className="font-mono text-sm text-white/55">
                      Vais receber um pedido de pagamento no telemovel associado a MB Way.
                    </p>
                    <Field
                      label="Telemovel MB Way"
                      value={form.telemovel}
                      onChange={(value) => updateField("telemovel", value)}
                      placeholder="912345678"
                    />
                  </div>
                )}

                {paymentKind === "paypal" && (
                  <div className="space-y-4">
                    <p className="font-mono text-sm text-white/55">
                      Indica o email da conta PayPal que queres usar para pagar.
                    </p>
                    <Field
                      label="Email PayPal"
                      type="email"
                      value={form.emailPaypal}
                      onChange={(value) => updateField("emailPaypal", value)}
                      placeholder="conta@paypal.com"
                    />
                  </div>
                )}
              </div>
            )}

            <div className="flex flex-wrap gap-4 pt-4">
              <button
                type="submit"
                disabled={submitting || checkout.quantidadeDisponivel <= 0 || !form.idTipoPagamento}
                className="rounded-full bg-yellow-400 px-9 py-4 font-black uppercase tracking-[0.18em] text-black hover:bg-yellow-300 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {submitting ? "A processar..." : "Confirmar compra"}
              </button>
              <button
                type="button"
                onClick={onBack}
                className="rounded-full border border-white/20 bg-white/5 px-9 py-4 font-black uppercase tracking-[0.18em] text-white hover:bg-white/10"
              >
                Cancelar
              </button>
            </div>
          </form>
        </section>
      </div>
    </main>
  );
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="grid grid-cols-[120px_1fr] gap-4">
      <span className="text-white/45">{label}</span>
      <span className="text-white/85">{value}</span>
    </div>
  );
}

function Field(props: {
  label: string;
  value: string;
  type?: string;
  placeholder?: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-3">
      <span className="block font-mono text-xs font-black uppercase tracking-[0.18em] text-white/80">{props.label}</span>
      <input
        type={props.type || "text"}
        value={props.value}
        placeholder={props.placeholder}
        onChange={(event) => props.onChange(event.target.value)}
        className="w-full rounded-2xl border border-white/10 bg-white/[0.07] px-5 py-4 font-mono text-lg text-white outline-none placeholder:text-white/25 focus:border-yellow-400"
      />
    </label>
  );
}
