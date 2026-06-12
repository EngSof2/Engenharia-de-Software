import { useEffect, useMemo, useState } from "react";
import { api, Purchase } from "../api";

type Props = {
  onNavigate: (page: "events") => void;
};

export function PurchaseHistoryView({ onNavigate }: Props) {
  const [purchases, setPurchases] = useState<Purchase[]>([]);
  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<Purchase | null>(null);

  useEffect(() => {
    api.getPurchaseHistory().then(setPurchases);
  }, []);

  const visible = useMemo(() => {
    const query = search.toLowerCase();
    if (!query) return purchases;
    return purchases.filter((purchase) =>
      [
        purchase.recibo,
        purchase.evento,
        purchase.nomeBilhete,
        purchase.acesso,
        purchase.metodoPagamento,
        purchase.data
      ].some((value) => value.toLowerCase().includes(query))
    );
  }, [purchases, search]);

  return (
    <main className="relative min-h-screen px-4 pb-32 pt-32 text-white">
      <div className="absolute inset-0 overflow-hidden" aria-hidden="true">
        <img
          src="/images/purchase-history-bg.jpg"
          alt=""
          className="h-full w-full scale-110 object-cover opacity-20 grayscale blur-[5px]"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-[#333533] via-[#333533]/85 to-[#333533]/95" />
      </div>
      <div className="relative z-10 mx-auto max-w-[1400px]">
        <div className="mb-8 flex flex-col justify-between gap-6 md:flex-row md:items-end">
          <div>
            <h1 className="mb-2 text-5xl font-bold tracking-tighter">
              Historico de <span className="text-yellow-400">compras</span>
            </h1>
            <p className="font-mono text-sm text-white/50">
              Consulta todos os bilhetes que ja compraste e respetivos metodos de pagamento.
            </p>
          </div>
          <button
            onClick={() => onNavigate("events")}
            className="rounded-xl border border-white/20 px-6 py-3 text-sm font-bold uppercase tracking-widest hover:bg-white/5"
          >
            Ver eventos
          </button>
        </div>

        <div className="relative mb-8 w-full max-w-md">
          <span className="pointer-events-none absolute left-[18px] top-1/2 z-10 -translate-y-1/2 text-white/45">
            <i className="bi bi-search" />
          </span>
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Pesquisar por recibo, evento, bilhete..."
            className="w-full rounded-full border border-white/10 bg-black/35 py-3.5 pr-6 pl-12 font-mono text-sm text-white outline-none backdrop-blur-md focus:border-yellow-400"
          />
        </div>

        <div className="overflow-hidden rounded-[30px] border border-white/10 bg-[#0a0a0a]/60 backdrop-blur-2xl">
          <table className="w-full text-left">
            <thead className="bg-white/[0.02] text-xs uppercase tracking-widest text-white/50">
              <tr>
                <th className="p-6">Recibo</th>
                <th className="p-6">Evento</th>
                <th className="p-6">Bilhete</th>
                <th className="p-6">Acesso</th>
                <th className="p-6">Pagamento</th>
                <th className="p-6">Data</th>
                <th className="p-6 text-right">Valor</th>
              </tr>
            </thead>
            <tbody>
              {visible.map((purchase) => (
                <tr
                  key={purchase.idRecibo}
                  onClick={() => setSelected(purchase)}
                  className="cursor-pointer border-t border-white/5 hover:bg-white/[0.02]"
                >
                  <td className="p-6 font-mono text-white/70">{purchase.recibo}</td>
                  <td className="p-6 font-medium">{purchase.evento}</td>
                  <td className="p-6">
                    <div className="font-bold">{purchase.nomeBilhete}</div>
                    <div className="text-xs text-white/50">{purchase.tipoBilhete}</div>
                  </td>
                  <td className="max-w-xs p-6 text-sm text-white/70">{purchase.acesso}</td>
                  <td className="p-6 text-sm text-white/70">{purchase.metodoPagamento}</td>
                  <td className="p-6 font-mono text-sm text-white/70">{purchase.data}</td>
                  <td className="p-6 text-right font-bold">{purchase.valor.toFixed(2)} EUR</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {selected && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4" onClick={() => setSelected(null)}>
          <div
            className="rounded-[30px] border border-white/10 bg-[#0a0a0a] text-center"
            style={{ width: "min(92vw, 480px)", padding: "56px 48px 48px" }}
            onClick={(event) => event.stopPropagation()}
          >
            <h2 className="mb-2 text-2xl font-bold uppercase">Bilhete {selected.recibo}</h2>
            <p className="mb-8 font-mono text-sm text-white/50">{selected.evento}</p>
            <div className="mx-auto flex h-[250px] w-[250px] items-center justify-center rounded-[18px] bg-white p-6">
              <img
                src={`https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=${encodeURIComponent(selected.recibo + selected.evento)}`}
                alt="QR Code"
                className="h-full w-full object-contain"
              />
            </div>
          </div>
        </div>
      )}
    </main>
  );
}
