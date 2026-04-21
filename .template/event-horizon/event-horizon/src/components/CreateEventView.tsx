import { useEffect, useState } from "react";
import { motion } from "motion/react";
import { Calendar, ChevronDown, Clock, Image as ImageIcon } from "lucide-react";

type Categoria = { id: number; nome: string };

export function CreateEventView({ onCancel, onCreated }: { onCancel?: () => void; onCreated?: (id: number) => void }) {
  const [categorias, setCategorias] = useState<Categoria[]>([]);

  const [nome, setNome] = useState("");
  const [data, setData] = useState("");
  const [horaInicio, setHoraInicio] = useState("");
  const [local, setLocal] = useState("");
  const [capacidade, setCapacidade] = useState<string>("");
  const [preco, setPreco] = useState<string>("");
  const [idCategoria, setIdCategoria] = useState<string>("");
  const [descricao, setDescricao] = useState("");
  const [imageUrl, setImageUrl] = useState("");

  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetch("/api/categorias", { credentials: "include" })
      .then((r) => (r.ok ? r.json() : []))
      .then((data) => setCategorias(Array.isArray(data) ? data : []))
      .catch(() => setCategorias([]));
  }, []);

  const handleCreate = async () => {
    if (isSubmitting) return;

    setIsSubmitting(true);
    try {
      const payload = {
        nome,
        data,
        horaInicio,
        local,
        descricao,
        capacidade: capacidade ? Number(capacidade) : null,
        preco: preco ? Number(preco) : null,
        idCategoria: idCategoria ? Number(idCategoria) : null,
        imageUrl: imageUrl || null,
      };

      const res = await fetch("/api/eventos", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (res.status === 401 || res.status === 403) {
        window.location.href = "/Login/Index";
        return;
      }

      if (!res.ok) {
        const msg = await res.text();
        window.alert(msg || "Nao foi possivel criar o evento.");
        return;
      }

      const json = await res.json();
      if (json?.id) onCreated?.(json.id);
    } catch {
      window.alert("Nao foi possivel criar o evento.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-[1600px] mx-auto px-10 pt-20 pb-32">
      {/* Header section matching Home aesthetic */}
      <div className="flex flex-col md:flex-row md:items-end justify-end gap-12 mb-20 relative border-b border-white/10 pb-8">
        <h1 className="text-7xl md:text-[110px] font-medium tracking-tighter leading-none text-right shrink-0 relative">
          Criar Evento
        </h1>
      </div>

      {/* Form Body Container */}
      <div className="w-full max-w-[1000px] mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-10">
          {/* Lado Esquerdo */}
          <div className="space-y-10">
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Nome</label>
              <input
                type="text"
                placeholder="Ex: Conferencia de Tecnologia"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div className="grid grid-cols-2 gap-6">
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Data</label>
                <div className="relative group">
                  <input
                    type="date"
                    value={data}
                    onChange={(e) => setData(e.target.value)}
                    className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white/50 focus:outline-none focus:text-white focus:border-yellow-400 focus:bg-white/[0.02] transition-colors relative z-10 [&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:inset-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer"
                    style={{ colorScheme: "dark" }}
                  />
                  <Calendar className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-0" />
                </div>
              </div>
              <div>
                <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Hora</label>
                <div className="relative group">
                  <input
                    type="time"
                    value={horaInicio}
                    onChange={(e) => setHoraInicio(e.target.value)}
                    className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white/50 focus:outline-none focus:text-white focus:border-yellow-400 focus:bg-white/[0.02] transition-colors relative z-10 [&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:inset-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer"
                    style={{ colorScheme: "dark" }}
                  />
                  <Clock className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-0" />
                </div>
              </div>
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Local</label>
              <input
                type="text"
                placeholder="Ex: Porto, Centro de Congressos"
                value={local}
                onChange={(e) => setLocal(e.target.value)}
                className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>
          </div>

          {/* Lado Direito */}
          <div className="space-y-10">
            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Capacidade MÃ¡xima</label>
              <input
                type="number"
                value={capacidade}
                onChange={(e) => setCapacidade(e.target.value)}
                className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">PreÃ§o</label>
              <input
                type="number"
                value={preco}
                onChange={(e) => setPreco(e.target.value)}
                className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
              />
            </div>

            <div>
              <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">Categoria</label>
              <div className="relative group border border-white/20 rounded-[24px] focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
                <select
                  className="w-full h-full bg-transparent rounded-[24px] pl-6 pr-12 py-4 text-sm text-white focus:outline-none appearance-none cursor-pointer relative z-10"
                  value={idCategoria}
                  onChange={(e) => setIdCategoria(e.target.value)}
                >
                  <option value="" className="bg-[#1a1a1a] text-white/50">
                    Sem categoria
                  </option>
                  {categorias.map((c) => (
                    <option key={c.id} value={String(c.id)} className="bg-[#1a1a1a]">
                      {c.nome}
                    </option>
                  ))}
                </select>
                <ChevronDown className="absolute right-6 top-1/2 -translate-y-1/2 w-4 h-4 text-white/50 pointer-events-none group-focus-within:text-yellow-400 transition-colors z-20" />
              </div>
            </div>
          </div>

          {/* SecÃ§Ãµes Inferiores */}
          <div className="col-span-1 md:col-span-2 pt-4">
            <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">URL da Imagem</label>
            <input
              type="text"
              placeholder="https://exemplo.com/imagem.jpg"
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
              className="w-full bg-transparent border border-white/20 rounded-[24px] px-6 py-4 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors mb-8"
            />

            {/* Image Preview Container */}
            <div
              className={`w-full h-[320px] shrink-0 rounded-[30px] border border-dashed flex flex-col items-center justify-center p-3 overflow-hidden relative transition-colors ${
                imageUrl ? "bg-black/40 border-white/10" : "bg-black/10 border-white/20 hover:border-white/30"
              }`}
            >
              {imageUrl ? (
                <img
                  src={imageUrl}
                  alt="Preview da Imagem do Evento"
                  className="w-full h-full object-cover rounded-[20px] shadow-2xl"
                  onError={(e) => (e.currentTarget.style.display = "none")}
                  onLoad={(e) => (e.currentTarget.style.display = "block")}
                />
              ) : (
                <div className="text-white/30 flex flex-col items-center gap-4">
                  <ImageIcon className="w-10 h-10 opacity-50" />
                  <span className="text-sm font-mono uppercase tracking-widest text-center">Preview da Imagem</span>
                </div>
              )}
            </div>
          </div>

          <div className="col-span-1 md:col-span-2 pt-4 pb-12 border-b border-white/10">
            <label className="block text-white/50 font-mono text-xs uppercase tracking-widest mb-3">DescriÃ§Ã£o</label>
            <textarea
              placeholder="Descreve o evento. Explora detalhes como linha-de-atuaÃ§Ã£o, vibe, e outras informaÃ§Ãµes cruciais..."
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              className="w-full bg-transparent border border-white/20 rounded-[30px] px-6 py-6 text-sm text-white placeholder-white/30 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors min-h-[160px] resize-y leading-relaxed"
            ></textarea>
          </div>
        </div>

        {/* Footer Actions */}
        <div className="flex justify-end gap-4 pt-8">
          <button
            type="button"
            onClick={() => onCancel?.()}
            className="px-10 py-4 bg-transparent border border-white/20 rounded-[24px] text-sm text-white font-bold tracking-widest uppercase hover:bg-white/5 hover:border-white transition-all"
          >
            Cancelar
          </button>
          <button
            type="button"
            disabled={isSubmitting}
            onClick={handleCreate}
            className="px-10 py-4 bg-yellow-400 text-black rounded-[24px] text-sm font-bold tracking-widest uppercase shadow-[0_0_15px_rgba(250,204,21,0.3)] hover:bg-white hover:shadow-[0_0_25px_rgba(255,255,255,0.5)] hover:scale-105 transition-all drop-shadow-xl disabled:opacity-60 disabled:hover:scale-100 disabled:hover:bg-yellow-400 disabled:cursor-not-allowed"
          >
            Criar Evento
          </button>
        </div>
      </div>
    </motion.div>
  );
}

