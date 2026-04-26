import { useEffect, useMemo, useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import { Search, ChevronDown, Clock, Users, MapPin, ArrowLeft, X } from "lucide-react";

type UiEventDetails = {
  id: number;
  title: string;
  date: string;
  location: string;
  image?: string | null;
};

type ApiActivity = {
  id: number;
  nome: string;
  local: string;
  capacidade: number;
  categoria?: { id: number; nome: string };
};

type UiActivity = {
  id: number;
  name: string;
  location: string;
  capacity: number;
  time: string;
};

function pad2(n: number) {
  return String(n).padStart(2, "0");
}

function timeByIndex(i: number) {
  const base = 18 * 60;
  const t = base + i * 60;
  const hh = Math.floor(t / 60) % 24;
  const mm = t % 60;
  return `${pad2(hh)}:${pad2(mm)}`;
}

export function EventDetailsView({ event, onBack }: { event: UiEventDetails; onBack: () => void }) {
  const [showFilters, setShowFilters] = useState(false);
  const [searchName, setSearchName] = useState("");
  const [searchLocation, setSearchLocation] = useState("");
  const [sortOrder, setSortOrder] = useState<"time_asc" | "time_desc" | "alpha">("time_asc");

  const [activities, setActivities] = useState<UiActivity[]>([]);

  useEffect(() => {
    let cancelled = false;

    fetch(`/api/eventos/${event.id}/atividades`, { credentials: "include" })
      .then(async (r) => {
        if (!r.ok) return [];
        const json = await r.json();
        return Array.isArray(json) ? (json as ApiActivity[]) : [];
      })
      .then((list) => {
        if (cancelled) return;
        const ui = list.map((a, i) => ({
          id: a.id,
          name: a.nome,
          location: a.local,
          capacity: a.capacidade,
          time: timeByIndex(i),
        }));
        setActivities(ui);
      })
      .catch(() => {
        if (!cancelled) setActivities([]);
      });

    return () => {
      cancelled = true;
    };
  }, [event.id]);

  const filteredActivities = useMemo(() => {
    let result = [...activities];

    if (searchName) {
      result = result.filter((act) => act.name.toLowerCase().includes(searchName.toLowerCase()));
    }
    if (searchLocation) {
      result = result.filter((act) => act.location.toLowerCase().includes(searchLocation.toLowerCase()));
    }

    result.sort((a, b) => {
      if (sortOrder === "alpha") return a.name.localeCompare(b.name);

      const timeA = parseInt(a.time.replace(":", ""));
      const timeB = parseInt(b.time.replace(":", ""));
      if (sortOrder === "time_asc") return timeA - timeB;
      if (sortOrder === "time_desc") return timeB - timeA;
      return 0;
    });

    return result;
  }, [activities, searchName, searchLocation, sortOrder]);

  return (
    <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-[1600px] mx-auto px-10 pt-20 pb-32">
      <div className="flex items-center justify-between mb-10 border-b border-white/10 pb-8">
  <button
    onClick={onBack}
    className="flex items-center gap-2 text-xs font-bold font-mono tracking-widest uppercase text-white/60 hover:text-white transition-colors"
  >
    <ArrowLeft className="w-4 h-4" /> Voltar
  </button>
  
    href={`/Atividade/Criar?idEvento=${event.id}`}
    className="px-6 py-2.5 bg-yellow-400 text-black text-xs font-bold font-mono uppercase tracking-widest rounded-full hover:bg-yellow-300 transition-colors"
  >
    + Adicionar Atividade
  </a>
</div>

      <div className="grid grid-cols-1 lg:grid-cols-[1fr_380px] gap-16">
        <div>
          {/* Hero */}
          <div className="relative aspect-[16/9] rounded-[30px] overflow-hidden border border-white/10 mb-10">
            <div className="absolute inset-0 bg-gradient-to-t from-[#0a0a0a] via-black/30 to-transparent z-10" />
            <img src={event.image || ""} alt={event.title} className="w-full h-full object-cover opacity-90" />
            <div className="absolute bottom-10 left-10 right-10 z-20">
              <div className="flex flex-wrap gap-4 items-center text-white/60 font-mono text-xs uppercase tracking-widest mb-4">
                <span>{event.date}</span>
                <span>&bull;</span>
                <span>{event.location}</span>
              </div>
              <h1 className="text-5xl md:text-7xl font-black tracking-tighter uppercase leading-none">{event.title}</h1>
            </div>
          </div>

          {/* Toolbar */}
          <div className="flex items-center justify-between mb-8 border-b border-white/10 pb-6">
            <div className="flex items-center gap-4">
              <div className="text-white/60 font-mono text-sm uppercase tracking-widest hidden sm:block">
                {filteredActivities.length} Atividades
              </div>
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center gap-2 text-xs font-bold font-mono tracking-widest uppercase transition-colors px-4 py-2 rounded-full border ${
                  showFilters
                    ? "bg-yellow-400 text-black border-yellow-400 shadow-[0_0_15px_rgba(250,204,21,0.3)]"
                    : "bg-transparent text-white border-white/20 hover:border-white/60"
                }`}
              >
                <Search className="w-4 h-4" />
                {showFilters ? "Esconder" : "Pesquisar"}
              </button>
            </div>

            <div className="relative group border border-white/20 rounded-full focus-within:border-yellow-400 focus-within:bg-white/[0.02] transition-colors">
              <select
                className="bg-transparent rounded-full pl-5 pr-12 py-2.5 text-xs text-white focus:outline-none appearance-none cursor-pointer relative z-10"
                value={sortOrder}
                onChange={(e) => setSortOrder(e.target.value as any)}
              >
                <option value="time_asc" className="bg-[#1a1a1a]">
                  Hora (Asc)
                </option>
                <option value="time_desc" className="bg-[#1a1a1a]">
                  Hora (Desc)
                </option>
                <option value="alpha" className="bg-[#1a1a1a]">
                  A-Z
                </option>
              </select>
              <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white/50 pointer-events-none z-0 group-focus-within:text-yellow-400 transition-colors" />
            </div>
          </div>

          {/* Expandable Search & Filters Row */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                initial={{ height: 0, opacity: 0, y: -10 }}
                animate={{ height: "auto", opacity: 1, y: 0 }}
                exit={{ height: 0, opacity: 0, y: -10 }}
                className="overflow-hidden"
              >
                <div className="flex flex-row items-center gap-3 mb-10 overflow-x-auto w-full pb-3 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden shrink-0">
                  <input
                    type="text"
                    placeholder="Pesquisar por nome da atividade..."
                    value={searchName}
                    onChange={(e) => setSearchName(e.target.value)}
                    className="min-w-[250px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
                    autoFocus
                  />

                  <input
                    type="text"
                    placeholder="Pesquisar por local/palco..."
                    value={searchLocation}
                    onChange={(e) => setSearchLocation(e.target.value)}
                    className="min-w-[200px] flex-1 bg-transparent border border-white/20 rounded-full px-5 py-2.5 text-xs text-white placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/[0.02] transition-colors"
                  />

                  {/* Clear button */}
                  <button
                    onClick={() => {
                      setSearchName("");
                      setSearchLocation("");
                    }}
                    className="shrink-0 flex items-center justify-center w-10 h-10 bg-transparent border border-white/20 rounded-full text-white/60 hover:bg-white/10 hover:text-white hover:border-white transition-all group"
                    aria-label="Limpar Filtros"
                    title="Limpar Filtros"
                  >
                    <X className="w-5 h-5 group-hover:scale-110 transition-transform duration-300" />
                  </button>
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Activities List */}
          <div className="flex flex-col border-t border-white/5">
            {filteredActivities.length === 0 ? (
              <div className="py-20 text-center text-white/40 font-mono tracking-widest text-sm uppercase">Nenhuma atividade encontrada</div>
            ) : (
              filteredActivities.map((activity, i) => (
                <motion.div
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: i * 0.05 }}
                  key={activity.id}
                  className="group border-b border-white/5 py-8 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:bg-white/[0.02] transition-colors md:px-6 md:-mx-6 rounded-xl"
                >
                  <div className="flex items-center gap-6 md:gap-10 w-full md:flex-1">
                    <span className="text-yellow-400 font-mono text-xl font-bold flex items-center gap-2">
                      <Clock className="w-5 h-5 opacity-50" /> {activity.time}
                    </span>
                    <div>
                      <h3 className="text-2xl md:text-3xl font-bold tracking-tighter uppercase group-hover:text-yellow-400 transition-colors mb-2">
                        {activity.name}
                      </h3>
                      <div className="flex flex-wrap items-center gap-4 text-white/40 font-mono text-xs uppercase tracking-widest">
                        <span className="flex items-center gap-1">
                          <MapPin className="w-3.5 h-3.5" /> {activity.location}
                        </span>
                        <span>&bull;</span>
                        <span className="flex items-center gap-1">
                          <Users className="w-3.5 h-3.5" /> Cap: {activity.capacity}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="w-full md:w-auto mt-2 md:mt-0">
                    <button className="w-full md:w-auto px-6 py-3 border border-white/10 rounded-full text-xs font-bold font-mono uppercase tracking-widest text-white hover:bg-white hover:text-black transition-colors focus:outline-none">
                      Inscrever
                    </button>
                  </div>
                </motion.div>
              ))
            )}
          </div>
        </div>
      </div>
    </motion.div>
  );
}

