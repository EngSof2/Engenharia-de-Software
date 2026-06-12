import { useEffect, useState } from "react";
import { api, Profile } from "../api";

type Props = {
  onNavigate: (page: "events" | "edit_profile" | "purchase_history") => void;
};

function profileLabel(profile?: Profile) {
  if (!profile) return "Utilizador";
  if (profile.tipoUti === 1 || profile.perfil === "Admin") return "Administrador";
  if (profile.tipoUti === 3 || profile.perfil === "Organizador") return "Organizador";
  return "Utilizador";
}

export function ProfileView({ onNavigate }: Props) {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.getProfile().then(setProfile).catch((err) => setError(err.message));
  }, []);

  return (
    <main className="relative min-h-screen overflow-hidden px-6 pb-32 pt-[240px] text-white">
      <ProfileBackground />

      <div className="relative z-10 mx-auto grid w-full max-w-[1860px] grid-cols-1 items-center gap-20 lg:grid-cols-[1fr_0.88fr]">
        <section>
          <button
            onClick={() => onNavigate("events")}
            className="mb-8 font-mono text-xs font-bold uppercase tracking-[0.28em] text-white/80 hover:text-yellow-400"
          >
            {"<-"} Voltar para eventos
          </button>

          <div className="mb-6 font-mono text-xs font-black uppercase tracking-[0.38em] text-yellow-400">
            Area pessoal
          </div>
          <h1 className="mb-8 text-[92px] font-black uppercase leading-[0.82] tracking-tight xl:text-[132px]">
            O Meu Perfil
          </h1>
          <p className="mb-8 max-w-3xl font-mono text-lg text-white/85">
            Consulta os dados ligados a tua conta Event Horizon.
          </p>

          <div className="flex flex-wrap gap-4">
            <button
              onClick={() => onNavigate("edit_profile")}
              className="rounded-full bg-yellow-400 px-8 py-4 font-black uppercase tracking-[0.18em] text-black hover:bg-yellow-300"
            >
              Editar dados
            </button>
            <button
              onClick={() => onNavigate("purchase_history")}
              className="rounded-full border border-white/20 bg-white/5 px-8 py-4 font-black uppercase tracking-[0.18em] text-white hover:bg-white/10"
            >
              Bilhetes
            </button>
          </div>
        </section>

        <article className="rounded-[30px] border border-white/10 bg-[#0a0a0a]/72 p-12 shadow-2xl backdrop-blur-2xl">
          <h2 className="mb-12 text-[42px] font-black uppercase tracking-tight">Informacoes da conta</h2>
          {error && <p className="mb-6 rounded-xl bg-yellow-400/10 p-4 font-mono text-yellow-100">{error}</p>}
          {!profile ? (
            <p className="font-mono text-white/50">A carregar perfil...</p>
          ) : (
            <div className="divide-y divide-white/10">
              <InfoRow label="Nome" value={profile.nome} />
              <InfoRow label="Email" value={profile.email} />
              <InfoRow label="Telemovel" value={profile.telemovel || "Nao definido"} />
              <InfoRow label="Tipo" value={profileLabel(profile)} badge />
            </div>
          )}
        </article>
      </div>
    </main>
  );
}

function ProfileBackground() {
  return (
    <div className="absolute inset-0 z-0 overflow-hidden">
      <img
        src="https://images.unsplash.com/photo-1549490349-8643362247b5?q=80&w=2200&auto=format&fit=crop"
        alt=""
        className="h-full w-full scale-105 object-cover opacity-20 grayscale blur-[2px]"
      />
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_78%_22%,rgba(255,204,0,0.13),transparent_24rem),linear-gradient(120deg,rgba(51,53,51,0.97)_0%,rgba(20,21,20,0.90)_52%,rgba(51,53,51,0.98)_100%)]" />
    </div>
  );
}

function InfoRow({ label, value, badge }: { label: string; value: string; badge?: boolean }) {
  return (
    <div className="grid grid-cols-[220px_1fr] items-center gap-6 py-5">
      <span className="font-mono text-xs font-black uppercase tracking-[0.18em] text-white/60">{label}</span>
      {badge ? (
        <span className="w-fit rounded-full border border-yellow-400/45 bg-yellow-400/10 px-4 py-2 text-xs font-black uppercase tracking-[0.16em] text-yellow-400">
          {value}
        </span>
      ) : (
        <span className="font-mono text-xl">{value}</span>
      )}
    </div>
  );
}
